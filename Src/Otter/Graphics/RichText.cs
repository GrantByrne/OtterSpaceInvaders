using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Graphics;
using SFML.Window;
using System.Diagnostics;
using System.IO;

namespace Otter {
    /// <summary>
    /// Graphic that renders text with some more options than normal Text.
    /// RichText can be very slow to render with large strings of text so be careful!
    /// For large blocks of text use the normal Text graphic.
    /// </summary>
    /// <example>
    /// richText.String = "Hello, {color:f00}this text is red!{clear} {shake:4}Shaking text!";
    /// <code>
    /// Commands:
    ///     {clear} - Clear all styles and reset back to normal, white text.
    ///     {style:name} - Apply the style 'name' to text.  Create styles with AddStyle().
    ///     {color:fff} - Colors text. Strings of 3, 4, 6, or 8 hex digits allowed.
    ///     {color0:fff} - Colors the top left corner of characters.  Strings of 3, 4, 6, or 8 hex digits allowed.
    ///     {color1:fff} - Colors the top right corner of characters.  Strings of 3, 4, 6, or 8 hex digits allowed.
    ///     {color2:fff} - Colors the bottom right corner of characters.  Strings of 3, 4, 6, or 8 hex digits allowed.
    ///     {color3:fff} - Colors the bottom left corner of characters.  Strings of 3, 4, 6, or 8 hex digits allowed.
    ///     {colorShadow:fff} - Colors text shadow. Strings of 3, 4, 6, or 8 hex digits allowed.
    ///     {colorOutline:fff} - Colors text outline. Strings of 3, 4, 6, or 8 hex digits allowed.
    ///     {shadowX:0} - Set the drop shadow of the text on the X axis.
    ///     {shadowY:0} - Set the drop shadow of the text on the Y axis.
    ///     {shadow:0} - Set the drop shadow of the text on the X and Y axes.
    ///     {outline:0} - Set the outline thickness on text.
    ///     {shakeX:0} - Shake the text on the X axis with a float range.
    ///     {shakeY:0} - Shake the text on the Y axis with a float range.
    ///     {shake:0} - Shake the text on the X and Y axes with a float range.
    ///     {waveAmpX:0} - Wave the text on the X axis with a float range.
    ///     {waveAmpY:0} - Wave the text on the Y axis with a float range.
    ///     {waveAmp:0} - Wave the text on the X and Y axes with a float range.
    ///     {waveRateX:0} - Set the wave speed for the X axis.
    ///     {waveRateY:0} - Set the wave speed for the Y axis.
    ///     {waveRate:0} - Set the wave speed for the X and Y axes.
    ///     {waveOffsetX:0} - Set the wave offset for the X axis.
    ///     {waveOffsetY:0} - Set the wave offset for the Y axis.
    ///     {waveOffset:0} - Set the wave offset for the X and Y axes.
    ///     {offset:0} - Set the offset rate for characters.
    /// </code>
    /// </example>
    public class RichText : Graphic {

        #region Static Fields

        static Dictionary<string, string> styles = new Dictionary<string, string>();

        #endregion

        #region Static Methods

        /// <summary>
        /// Add a global style to RichText objects.  The style will not be updated unless Refresh() is
        /// called on the objects.
        /// </summary>
        /// <example>
        /// RichText.AddStyle("important","color:f00,waveAmpY:2,waveRate:2");
        /// </example>
        /// <param name="name">The name of the style.</param>
        /// <param name="content">The properties to set using commas as a delim character.</param>
        static public void AddStyle(string name, string content) {
            if (styles.ContainsKey(name)) {
                styles[name] = content;
                return;
            }
            styles.Add(name, content);
        }

        /// <summary>
        /// Removes a style from all RichText objects.
        /// </summary>
        /// <param name="name">The name of the style to remove.</param>
        static public void RemoveStyle(string name) {
            styles.Remove(name);
        }

        /// <summary>
        /// Remove all styles from RichText objects.
        /// </summary>
        static public void ClearStyles() {
            styles.Clear();
        }

        #endregion

        #region Private Fields

        List<RichTextCharacter> chars = new List<RichTextCharacter>();
        List<uint> glyphs = new List<uint>();

        Color currentCharColor = Color.White;
        Color currentCharColor0 = Color.White;
        Color currentCharColor1 = Color.White;
        Color currentCharColor2 = Color.White;
        Color currentCharColor3 = Color.White;

        Color currentShadowColor = Color.Black;
        Color currentOutlineColor = Color.White;

        float currentSineAmpX = 0;
        float currentSineAmpY = 0;
        float currentSineRateX = 1;
        float currentSineRateY = 1;
        float currentSineOffsetX = 0;
        float currentSineOffsetY = 0;
        float currentOffsetAmount = 10;
        float currentShadowX = 0;
        float currentShadowY = 0;
        float currentOutlineThickness = 0;
        bool currentBold = false;

        float currentShakeX = 0;
        float currentShakeY = 0;

        int totalHeight = 0;

        float timer = 0;

        int textWidth = -1;
        int textHeight = -1;

        bool wordWrap = false;

        int advanceSpace;

        string cachedCleanString = "";

        string textString;

        List<float> cachedLineWidths = new List<float>();

        SFML.Graphics.Font font;

        int charSize = 16;

        float boundsLeft;
        float boundsTop;

        #endregion

        #region Public Fields

        /// <summary>
        /// The alignment of the text.  Left, Right, or Center.
        /// </summary>
        public TextAlign TextAlign = TextAlign.Left;

        /// <summary>
        /// The character used to mark an opening of a command.
        /// </summary>
        public char CommandOpen = '{';

        /// <summary>
        /// The character used to mark the closing of a command.
        /// </summary>
        public char CommandClose = '}';

        /// <summary>
        /// The character used to separate the command with the command value.
        /// </summary>
        public char CommandDelim = ':';

        /// <summary>
        /// Controls the spacing between each character. If set above 0 the text will use a monospacing.
        /// </summary>
        public int MonospaceWidth = -1;

        /// <summary>
        /// The default horizontal amplitude of the sine wave.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public float DefaultSineAmpX;

        /// <summary>
        /// The default vertical amplitude of the sine wave.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public float DefaultSineAmpY;

        /// <summary>
        /// The default horizontal rate of the sine wave.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public float DefaultSineRateX = 1;

        /// <summary>
        /// The default vertical rate of the sine wave.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public float DefaultSineRateY = 1;

        /// <summary>
        /// The default horizontal offset of the sine wave.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public float DefaultSineOffsetX;

        /// <summary>
        /// The default vertical offset of the sine wave.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public float DefaultSineOffsetY;

        /// <summary>
        /// The default amount to offset each character for sine wave related transformations.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public float DefaultOffsetAmount = 10;

        /// <summary>
        /// The default X position of the text shadow.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public float DefaultShadowX;

        /// <summary>
        /// The default Y position of the text shadow.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public float DefaultShadowY;

        /// <summary>
        /// The default outline thickness.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public float DefaultOutlineThickness;

        /// <summary>
        /// The default horizontal shaking effect.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public float DefaultShakeX;

        /// <summary>
        /// The default vertical shaking effect.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public float DefaultShakeY;

        /// <summary>
        /// The default character color.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public Color DefaultCharColor = Color.White;

        /// <summary>
        /// The default color of the top left corner of each character.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public Color DefaultCharColor0 = Color.White;

        /// <summary>
        /// The default color of the top right corner of each character.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public Color DefaultCharColor1 = Color.White;

        /// <summary>
        /// The default color of the bottom right corner of each character.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public Color DefaultCharColor2 = Color.White;

        /// <summary>
        /// The default color of the bottom left corner of each character.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public Color DefaultCharColor3 = Color.White;

        /// <summary>
        /// The default shadow color.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public Color DefaultShadowColor = Color.Black;

        /// <summary>
        /// The default outline color.
        /// Will not take effect until the string changes, or Refresh() is called.
        /// </summary>
        public Color DefaultOutlineColor = Color.White;

        /// <summary>
        /// The line height. 1 is 100% of the normal line height for the font.
        /// </summary>
        public float LineHeight = 1;

        /// <summary>
        /// The letter spacing. 1 is 100% of the normal letter spacing.
        /// </summary>
        public float LetterSpacing = 1;

        /// <summary>
        /// The default config.
        /// </summary>
        public static RichTextConfig Default = new RichTextConfig();

        #endregion

        #region Public Properties

        /// <summary>
        /// True if the text is using MonospaceWidth.
        /// </summary>
        public bool Monospaced {
            get { return MonospaceWidth > 0; }
        }

        /// <summary>
        /// The width of the text box.  If not set it will be automatically set.
        /// </summary>
        public int TextWidth {
            get { return textWidth; }
            set {
                textWidth = value;
                UpdateCharacterData();
            }
        }

        /// <summary>
        /// The height of the text box.  If not set it will be automatically set.
        /// </summary>
        public int TextHeight {
            get { return textHeight; }
            set {
                textHeight = value;
                UpdateCharacterData();
            }
        }

        /// <summary>
        /// The line spacing between each vertical line.
        /// </summary>
        public int LineSpacing {
            get { return font.GetLineSpacing((uint)charSize); }
        }

        /// <summary>
        /// Determines if the text will automatically wrap.  This will not work unless TextWidth is set.
        /// </summary>
        public bool WordWrap {
            get { return wordWrap; }
            set {
                wordWrap = value;
                UpdateCharacterData();
            }
        }

        /// <summary>
        /// The font size of the text.
        /// </summary>
        public int FontSize {
            get {
                return charSize;
            }
            set {
                charSize = value;

                // Force update here
                UpdateDrawable();
            }
        }

        /// <summary>
        /// True of the width was not manually set.
        /// </summary>
        public bool AutoWidth {
            get { return TextWidth < 0; }
        }

        /// <summary>
        /// True if the height was not manually set.
        /// </summary>
        public bool AutoHeight {
            get { return TextHeight < 0; }
        }

        /// <summary>
        /// The string to display stripped of all commands.
        /// </summary>
        public string CleanString {
            get {
                if (cachedCleanString != "") return cachedCleanString;
                var str = "";
                foreach (var c in chars) {
                    str += c.Character.ToString();
                }
                cachedCleanString = str;
                return str;
            }
        }

        /// <summary>
        /// The pixel width of the longest line in the displayed string.
        /// </summary>
        public int LongestLine {
            get {
                var lines = CleanString.Split('\n');
                int longest = 0;
                for (int i = 0; i < NumLines; i++) {
                    int pixels = GetLineWidth(i);
                    longest = Math.Max(longest, pixels);
                }

                return longest;
            }
        }

        /// <summary>
        /// The displayed string broken up into an array by lines.
        /// </summary>
        public string[] Lines { get; private set; }

        /// <summary>
        /// The total number of lines in the displayed string.
        /// </summary>
        public int NumLines {
            get { return Lines.Length; }
        }

        /// <summary>
        /// The string to display.  This string can contain commands to alter the text dynamically.
        /// </summary>
        public string String {
            get {
                return textString;
            }
            set {
                textString = value;
                UpdateCharacterData();

                // Force update here to set Width and Height and other stuff
                UpdateDrawable();
            }
        }

        /// <summary>
        /// The top bounds of the RichText.
        /// </summary>
        public float BoundsTop {
            get {
                return boundsTop;
            }
        }

        /// <summary>
        /// The top bounds of the RichText.
        /// </summary>
        public float BoundsLeft {
            get {
                return boundsLeft;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new RichText object.
        /// </summary>
        /// <param name="str">The string to display. This can include commands to alter text.</param>
        /// <param name="font">The file path to the font to use.</param>
        /// <param name="size">The font size to use.</param>
        /// <param name="textWidth">The width of the text box.</param>
        /// <param name="textHeight">The height of the text box.</param>
        public RichText(string str, string font = "", int size = 16, int textWidth = -1, int textHeight = -1)
            : base() {
            Initialize(str, font, size, textWidth, textHeight);
        }

        /// <summary>
        /// Create a new RichText object.
        /// </summary>
        /// <param name="str">The string to display. This can include commands to alter text.</param>
        /// <param name="font">The stream of the font to use.</param>
        /// <param name="size">The font size to use.</param>
        /// <param name="textWidth">The width of the text box.</param>
        /// <param name="textHeight">The height of the text box.</param>
        public RichText(string str, Stream font, int size = 16, int textWidth = -1, int textHeight = -1)
            : base() {
            Initialize(str, font, size, textWidth, textHeight);

        }

        /// <summary>
        /// Create a new RichText object using a RichTextConfig.
        /// </summary>
        /// <param name="str">The starting default text.</param>
        /// <param name="config">The config to set all the default style values.</param>
        /// <param name="textWidth">The width of the text box.</param>
        /// <param name="textHeight">The height of the text box.</param>
        public RichText(string str, RichTextConfig config, int textWidth = -1, int textHeight = -1) {
            // I probably should've used a dictionary of values or something.
            if (config == null) config = Default;

            DefaultSineAmpX = config.SineAmpX;
            DefaultSineAmpY = config.SineAmpY;
            DefaultSineRateX = config.SineRateX;
            DefaultSineRateY = config.SineRateY;
            DefaultSineOffsetX = config.SineOffsetX;
            DefaultSineOffsetY = config.SineOffsetY;
            DefaultOffsetAmount = config.OffsetAmount;
            DefaultShadowX = config.ShadowX;
            DefaultShadowY = config.ShadowY;
            DefaultOutlineThickness = config.OutlineThickness;
            DefaultShakeX = config.ShakeX;
            DefaultShakeY = config.ShakeY;
            DefaultCharColor = config.CharColor;
            DefaultCharColor0 = config.CharColor0;
            DefaultCharColor1 = config.CharColor1;
            DefaultCharColor2 = config.CharColor2;
            DefaultCharColor3 = config.CharColor3;
            DefaultShadowColor = config.ShadowColor;
            DefaultOutlineColor = config.OutlineColor;

            LetterSpacing = config.LetterSpacing;
            MonospaceWidth = config.MonospaceWidth;
            TextAlign = config.TextAlign;

            if (config.String != "") {
                str = config.String;
            }
            if (config.TextWidth != -1) {
                textWidth = config.TextWidth;
            }
            if (config.TextHeight != -1) {
                textHeight = config.TextHeight;
            }

            Initialize(str, config.Font, config.FontSize, textWidth, textHeight);
        }

        /// <summary>
        /// Create a new RichText object.
        /// </summary>
        /// <param name="str">The string to display.</param>
        /// <param name="size">The size of the font.</param>
        public RichText(string str, int size) : this(str, "", size) { }

        /// <summary>
        /// Create a new RichText object.
        /// </summary>
        /// <param name="size">The size of the font.</param>
        public RichText(int size = 16) : this("", "", size) { }

        /// <summary>
        /// Create a new RichText object
        /// </summary>
        /// <param name="config">The RichTextConfig to use.</param>
        public RichText(RichTextConfig config) : this("", config) { } 

        #endregion

        #region Private Methods

        void Initialize(string str, object font, int size, int textWidth, int textHeight) {
            Dynamic = true;

            if (font is string) {
                if ((string)font == "") {
                    this.font = Fonts.DefaultFont;
                }
                else {
                    this.font = Fonts.Load((string)font);
                }
            }
            else {
                this.font = Fonts.Load((Stream)font);
            }

            charSize = size;
            TextWidth = textWidth;
            TextHeight = textHeight;
            String = str;
            roundRendering = false;
        }

        int Advance(Glyph glyph) {
            if (Monospaced) return MonospaceWidth;
            return glyph.Advance;
        }

        Glyph Glyph(uint charCode) {
            var g = font.GetGlyph(charCode, (uint)charSize, currentBold);

            //update otter texture because SFML font texture updates
            if (!glyphs.Contains(charCode)) {
                SetTexture(new Texture(font.GetTexture((uint)charSize)));
                glyphs.Add(charCode);
            }

            return g;
        }

        void PrecalculateLineWidths() {
            if (cachedLineWidths.Count > 0) return; // Already calculated, will reset when string changes.
            int lineNumber = 0;
            foreach (var line in Lines) {
                cachedLineWidths.Add(GetLineWidth(lineNumber));
                lineNumber++;
            }
        }

        void ApplyCommand(string command, string args) {
            switch (command) {
                case "color":
                    currentCharColor = new Color(args);
                    break;
                case "color0":
                    currentCharColor0 = new Color(args);
                    break;
                case "color1":
                    currentCharColor1 = new Color(args);
                    break;
                case "color2":
                    currentCharColor2 = new Color(args);
                    break;
                case "color3":
                    currentCharColor3 = new Color(args);
                    break;
                case "colorShadow":
                    currentShadowColor = new Color(args);
                    break;
                case "colorOutline":
                    currentOutlineColor = new Color(args);
                    break;
                case "outline":
                    currentOutlineThickness = float.Parse(args);
                    break;
                case "shakeX":
                    currentShakeX = float.Parse(args);
                    break;
                case "shakeY":
                    currentShakeY = float.Parse(args);
                    break;
                case "shake":
                    currentShakeX = float.Parse(args);
                    currentShakeY = float.Parse(args);
                    break;
                case "waveAmpX":
                    currentSineAmpX = float.Parse(args);
                    break;
                case "waveAmpY":
                    currentSineAmpY = float.Parse(args);
                    break;
                case "waveAmp":
                    currentSineAmpX = float.Parse(args);
                    currentSineAmpY = float.Parse(args);
                    break;
                case "waveRateX":
                    currentSineRateX = float.Parse(args);
                    break;
                case "waveRateY":
                    currentSineRateY = float.Parse(args);
                    break;
                case "waveRate":
                    currentSineRateX = float.Parse(args);
                    currentSineRateY = float.Parse(args);
                    break;
                case "waveOffsetX":
                    currentSineOffsetX = float.Parse(args);
                    break;
                case "waveOffsetY":
                    currentSineOffsetY = float.Parse(args);
                    break;
                case "waveOffset":
                    currentSineOffsetX = float.Parse(args);
                    currentSineOffsetY = float.Parse(args);
                    break;
                case "shadowX":
                    currentShadowX = float.Parse(args);
                    break;
                case "shadowY":
                    currentShadowY = float.Parse(args);
                    break;
                case "shadow":
                    currentShadowX = float.Parse(args);
                    currentShadowY = float.Parse(args);
                    break;
                case "offset":
                    currentOffsetAmount = float.Parse(args);
                    break;
                case "bold":
                    currentBold = int.Parse(args) > 0;
                    break;
            }
        }

        void UpdateCharacterData() {
            chars.Clear();
            cachedCleanString = ""; // Clear clean string cache
            cachedLineWidths.Clear();
            Clear();

            if (string.IsNullOrEmpty(textString)) textString = "";

            var writingText = true;

            //auto word wrap string on input, before parsing?
            if (!AutoWidth && WordWrap) {
                textString = PreWrap(textString);
            }

            writingText = true;

            //create the set of chars with properties and parse commands
            for (var i = 0; i < textString.Length; i++) {
                var c = textString[i];
                if (c == CommandOpen) {
                    //scan for commandclose
                    var cmdEnd = textString.IndexOf(CommandClose, i + 1);
                    if (cmdEnd >= 0) {
                        //only continue of command close character is found
                        writingText = false;
                        var cmd = textString.Substring(i + 1, cmdEnd - i - 1);
                        var cmdSplit = cmd.Split(CommandDelim);
                        var command = cmdSplit[0];
                        if (command == "clear") {
                            Clear();
                        }
                        else if (command == "style") {
                            var args = cmdSplit[1];
                            if (styles.ContainsKey(args)) {
                                var stylestring = styles[args];

                                var styleSplit = stylestring.Split(',');
                                foreach (var str in styleSplit) {
                                    var styleStrSplit = str.Split(CommandDelim);

                                    ApplyCommand(styleStrSplit[0], styleStrSplit[1]);
                                }
                            }
                        }
                        else {
                            ApplyCommand(command, cmdSplit[1]);
                        }

                        continue;
                    }
                }
                if (c == CommandClose) {
                    writingText = true;
                    continue;
                }
                if (writingText) {
                    var rtchar = new RichTextCharacter(c, i) {
                        SineAmpX = currentSineAmpX,
                        SineAmpY = currentSineAmpY,
                        SineRateX = currentSineRateX,
                        SineRateY = currentSineRateY,
                        SineOffsetX = currentSineOffsetX,
                        SineOffsetY = currentSineOffsetY,
                        OffsetAmount = currentOffsetAmount,
                        ShadowX = currentShadowX,
                        ShadowY = currentShadowY,
                        ShadowColor = currentShadowColor,
                        OutlineThickness = currentOutlineThickness,
                        OutlineColor = currentOutlineColor,
                        Color = currentCharColor,
                        Color0 = currentCharColor0,
                        Color1 = currentCharColor1,
                        Color2 = currentCharColor2,
                        Color3 = currentCharColor3,
                        ShakeX = currentShakeX,
                        ShakeY = currentShakeY,
                        Timer = timer
                    };

                    chars.Add(rtchar);
                }
            }

            Lines = CleanString.Split('\n');

            totalHeight = (int)Math.Ceiling(NumLines * font.GetLineSpacing((uint)charSize) * LineHeight);
        }

        void Clear() {
            currentCharColor = DefaultCharColor;
            currentCharColor0 = DefaultCharColor0;
            currentCharColor1 = DefaultCharColor1;
            currentCharColor2 = DefaultCharColor2;
            currentCharColor3 = DefaultCharColor3;
            currentShadowColor = DefaultShadowColor;
            currentOutlineColor = DefaultOutlineColor;
            currentOutlineThickness = DefaultOutlineThickness;
            currentShadowX = DefaultShadowX;
            currentShadowY = DefaultShadowY;
            currentShakeX = DefaultShakeX;
            currentShakeY = DefaultShakeY;
            currentSineAmpX = DefaultSineAmpX;
            currentSineAmpY = DefaultSineAmpY;
            currentSineOffsetX = DefaultSineOffsetX;
            currentSineOffsetY = DefaultSineOffsetY;
            currentSineRateX = DefaultSineRateX;
            currentSineRateY = DefaultSineRateY;
            currentOffsetAmount = DefaultOffsetAmount;
        }

        protected override void UpdateDrawable() {
            base.UpdateDrawable();

            SFMLVertices.Clear();

            PrecalculateLineWidths();

            advanceSpace = Advance(Glyph(' ')); //Figure out space ahead of time.

            float x = 0;
            float y = 0;

            int currentLine = 0;

            x = LineStartPosition(currentLine);
            y = charSize;

            float lineLength = 0;

            float maxX = 0;
            float maxY = 0;
            float minY = charSize;
            float minX = charSize;

            var quadCount = 0;

            for (var i = 0; i < chars.Count; i++) {

                var c = chars[i].Character;

                if (c == ' ' || c == '\t' || c == '\n') {

                    minX = Util.Min(minX, x - LineStartPosition(currentLine));
                    minY = Util.Min(minY, y);

                    switch (c) {
                        case ' ':
                            x += advanceSpace * LetterSpacing;
                            lineLength += advanceSpace * LetterSpacing;
                            break;
                        case '\t':
                            x += advanceSpace * 4 * LetterSpacing;
                            lineLength += advanceSpace * 4 * LetterSpacing;
                            break;
                        case '\n':
                            cachedLineWidths.Add(lineLength);
                            lineLength = 0;

                            y += LineSpacing * LineHeight;

                            currentLine++;
                            x = LineStartPosition(currentLine);
                            break;
                    }

                    maxX = Util.Max(maxX, x - LineStartPosition(currentLine));
                    maxY = Util.Max(maxY, y);

                }
                else {
                    var glyph = Glyph(c);
                    var rect = glyph.TextureRect;
                    var bounds = glyph.Bounds;

                    var cx = chars[i].OffsetX;
                    var cy = chars[i].OffsetY;

                    var left = bounds.Left;
                    var right = bounds.Left + bounds.Width;
                    var top = bounds.Top;
                    var bottom = bounds.Top + bounds.Height;

                    var u1 = rect.Left;
                    var v1 = rect.Top;
                    var u2 = rect.Left + rect.Width;
                    var v2 = rect.Top + rect.Height;

                    // Draw shadow

                    Color nextColor;

                    if (chars[i].ShadowX != 0 || chars[i].ShadowY != 0) {
                        var shadowx = cx + chars[i].ShadowX;
                        var shadowy = cy + chars[i].ShadowY;

                        nextColor = chars[i].ShadowColor * Color;

                        SFMLVertices.Append(shadowx + x + bounds.Left, shadowy + y + bounds.Top, nextColor, rect.Left, rect.Top);
                        SFMLVertices.Append(shadowx + x + bounds.Left + bounds.Width, shadowy + y + bounds.Top, nextColor, rect.Left + rect.Width, rect.Top);
                        SFMLVertices.Append(shadowx + x + bounds.Left + bounds.Width, shadowy + y + bounds.Top + bounds.Height, nextColor, rect.Left + rect.Width, rect.Top + rect.Height);
                        SFMLVertices.Append(shadowx + x + bounds.Left, shadowy + y + bounds.Top + bounds.Height, nextColor, rect.Left, rect.Top + rect.Height);

                        quadCount++;
                    }

                    // Draw outline

                    if (chars[i].OutlineThickness > 0) {
                        var outline = chars[i].OutlineThickness;
                        nextColor = chars[i].OutlineColor * Color;

                        for (float o = outline * 0.5f; o < outline; o += outline * 0.5f) {
                            for (float r = 0; r < 360; r += 45) {
                                var outlinex = Util.PolarX(r, o) + cx;
                                var outliney = Util.PolarY(r, o) + cy;

                                SFMLVertices.Append(outlinex + x + bounds.Left, outliney + y + bounds.Top, nextColor, rect.Left, rect.Top);
                                SFMLVertices.Append(outlinex + x + bounds.Left + bounds.Width, outliney + y + bounds.Top, nextColor, rect.Left + rect.Width, rect.Top);
                                SFMLVertices.Append(outlinex + x + bounds.Left + bounds.Width, outliney + y + bounds.Top + bounds.Height, nextColor, rect.Left + rect.Width, rect.Top + rect.Height);
                                SFMLVertices.Append(outlinex + x + bounds.Left, outliney + y + bounds.Top + bounds.Height, nextColor, rect.Left, rect.Top + rect.Height);

                                quadCount++;
                            }
                        }
                    }

                    // Draw character

                    nextColor = chars[i].Color.Copy() * Color;
                    nextColor *= chars[i].Color0;

                    Append(cx + x + left, cy + y + top, nextColor, u1, v1);

                    nextColor = chars[i].Color.Copy() * Color;
                    nextColor *= chars[i].Color1;

                    Append(cx + x + right, cy + y + top, nextColor, u2, v1);

                    nextColor = chars[i].Color.Copy() * Color;
                    nextColor *= chars[i].Color2;

                    Append(cx + x + right, cy + y + bottom, nextColor, u2, v2);

                    nextColor = chars[i].Color.Copy() * Color;
                    nextColor *= chars[i].Color3;

                    Append(cx + x + left, cy + y + bottom, nextColor, u1, v2);

                    // Keep track of how many quads for debugging purposes
                    quadCount++;

                    // Update bounds.
                    minX = Util.Min(minX, x + left - LineStartPosition(currentLine));
                    minY = Util.Min(minY, y + top);

                    maxX = Util.Max(maxX, x + right - LineStartPosition(currentLine));
                    maxY = Util.Max(maxY, y + bottom);

                    // Advance position
                    x += Advance(glyph) * LetterSpacing;

                    // Keep track of line length separately
                    lineLength += Advance(glyph) * LetterSpacing;

                }
            }

            // Handle Length of final line
            cachedLineWidths.Add(lineLength);

            // Figure out dimensions
            if (AutoWidth) {
                Width = (int)(maxX - minX);
            }
            else {
                Width = TextWidth;
            }

            if (AutoHeight) {
                Height = (int)(maxY - minY);
                Height = (int)Util.Max(Height, charSize); // Temp fix for negative height?
            }
            else {
                Height = TextHeight;
            }

            boundsLeft = minX;
            boundsTop = minY;
        }

        int LineStartPosition(int lineNumber) {
            if (TextAlign == TextAlign.Left) return 0;

            int lineStart = 0;
            int lineLength = GetLineWidth(lineNumber);
            switch (TextAlign) {
                case TextAlign.Center:
                    lineStart = (Width - lineLength) / 2;
                    break;
                case TextAlign.Right:
                    lineStart = Width - lineLength;
                    break;
            }
            return lineStart;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Center the RichText's origin. This factors in the RichText's local bounds.
        /// </summary>
        public void CenterTextOrigin() {
            CenterTextOriginX();
            CenterTextOriginY();
        }

        /// <summary>
        /// Center the RichText's Y origin.  This factors in the RichText's top bounds.
        /// </summary>
        public void CenterTextOriginY() {
            OriginY = HalfHeight + BoundsTop;
        }

        /// <summary>
        /// Center the RichText's X origin.  This factors in the RichText's left bounds.
        /// </summary>
        public void CenterTextOriginX() {
            OriginX = HalfWidth + BoundsLeft;
        }

        /// <summary>
        /// Insert new lines into a string to prepare it for word wrapping with this object's width.
        /// This function will not wrap text if AutoWidth is true!
        /// </summary>
        /// <param name="str">The string to wrap.</param>
        /// <returns>The wrapped string.</returns>
        public string PreWrap(string str) {
            if (AutoWidth) return str; //Auto width cannot auto word wrap.

            var finalStr = str;

            var writingText = true;

            int pixels = 0;
            int lastSpaceIndex = 0;

            for (var i = 0; i < str.Length; i++) {
                var c = str[i];
                var glyph = Glyph(c);

                if (c == CommandOpen) {
                    var cmdEnd = str.IndexOf(CommandClose, i + 1);
                    if (cmdEnd >= 0) {
                        writingText = false;
                    }
                }
                if (!writingText) {
                    if (c == CommandClose) {
                        writingText = true;
                    }
                }
                else if (writingText) {
                    if (c == '\t') {
                        pixels += Advance(glyph) * 4;
                    }
                    else if (c == '\n') {
                        pixels = 0;
                    }
                    else {
                        pixels += Advance(glyph);

                        if (c == ' ') {
                            // Keep track of the last space.
                            lastSpaceIndex = i;
                        }
                        if (pixels > TextWidth) {
                            StringBuilder sb = new StringBuilder(finalStr);

                            // Turn last space into new line if pixels exceeds allowed width
                            if (lastSpaceIndex < sb.Length) {
                                sb[lastSpaceIndex] = '\n';
                            }

                            finalStr = sb.ToString();

                            // Return the loop to the new line.
                            i = lastSpaceIndex;

                            // Reset the current pixel width.
                            pixels = 0;
                        }
                    }
                }
            }

            return finalStr;
        }

        /// <summary>
        /// The line width in pixels of a specific line.
        /// </summary>
        /// <param name="lineNumber">The line number to check.</param>
        /// <returns>The length of the line in pixels.</returns>
        public int GetLineWidth(int lineNumber) {
            if (lineNumber < 0 || lineNumber >= NumLines) throw new ArgumentOutOfRangeException("Line doesn't exist in string!");

            //This is very slow on large text objects, but I'm not sure how to get around that!

            var line = Lines[lineNumber];
            int pixels = 0;
            foreach (var c in line) {
                var glyph = Glyph(c);
                if (c == '\t') {
                    pixels += (int)Math.Ceiling(Advance(glyph) * 3 * LetterSpacing);
                }
                pixels += (int)Math.Ceiling(Advance(glyph) * LetterSpacing);
            }
            return pixels;
        }

        /// <summary>
        /// Refresh the text.  This will reapply all commands and update the text image.
        /// </summary>
        public void Refresh() {
            UpdateCharacterData();
        }

        /// <summary>
        /// Update the RichText.
        /// </summary>
        public override void Update() {
            timer += Game.Instance.DeltaTime;

            foreach (var c in chars) {
                c.Update();
            }

            base.Update();
        }

        #endregion

    }

    #region Enum

    public enum TextAlign {
        Left,
        Right,
        Center
    }

    #endregion

    /// <summary>
    /// Internal class for managing characters in RichText.
    /// </summary>
    class RichTextCharacter {

        #region Private Fields

        float finalShakeX;
        float finalShakeY;
        float finalSinX;
        float finalSinY;

        #endregion

        #region Public Fields

        /// <summary>
        /// The Color of the character.
        /// </summary>
        public Color Color = Color.White;

        /// <summary>
        /// The Color of the top left corner.
        /// </summary>
        public Color Color0 = Color.White;

        /// <summary>
        /// The Color of the top left corner.
        /// </summary>
        public Color Color1 = Color.White;

        /// <summary>
        /// The Color of the top left corner.
        /// </summary>
        public Color Color2 = Color.White;

        /// <summary>
        /// The Color of the top left corner.
        /// </summary>
        public Color Color3 = Color.White;

        /// <summary>
        /// The Color of the shadow.
        /// </summary>
        public Color ShadowColor = Color.Black;

        /// <summary>
        /// The Color of the outline.
        /// </summary>
        public Color OutlineColor = Color.White;

        /// <summary>
        /// The character.
        /// </summary>
        public char Character;

        /// <summary>
        /// Timer used for animation.
        /// </summary>
        public float Timer;

        /// <summary>
        /// The horizontal sine wave amplitude.
        /// </summary>
        public float SineAmpX;

        /// <summary>
        /// The vertical sine wave amplitude.
        /// </summary>
        public float SineAmpY;

        /// <summary>
        /// The horizontal sine wave rate.
        /// </summary>
        public float SineRateX = 1;

        /// <summary>
        /// The vertical sine wave rate.
        /// </summary>
        public float SineRateY = 1;

        /// <summary>
        /// The horizontal sine wave offset.
        /// </summary>
        public float SineOffsetX;

        /// <summary>
        /// The vertical sine wave offset.
        /// </summary>
        public float SineOffsetY;

        /// <summary>
        /// The sine wave offset for this specific character.
        /// </summary>
        public float CharOffset;

        /// <summary>
        /// The offset amount for each character.
        /// </summary>
        public float OffsetAmount = 10;

        /// <summary>
        /// The X position of the shadow.
        /// </summary>
        public float ShadowX;

        /// <summary>
        /// The Y position of the shadow.
        /// </summary>
        public float ShadowY;

        /// <summary>
        /// The outline thickness.
        /// </summary>
        public float OutlineThickness;

        /// <summary>
        /// The amount of horizontal shake.
        /// </summary>
        public float ShakeX;

        /// <summary>
        /// The amount of vertical shake.
        /// </summary>
        public float ShakeY;

        /// <summary>
        /// Determines if the character is bold.  Not supported yet.
        /// </summary>
        public bool Bold = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// The final horizontal offset position of the character when rendered.
        /// </summary>
        public float OffsetX {
            get {
                return finalShakeX + finalSinX;
            }
        }

        /// <summary>
        /// The final vertical offset position of the character when rendered.
        /// </summary>
        public float OffsetY {
            get {
                return finalShakeY + finalSinY;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new RichTextCharacter.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="charOffset">The character offset for animation.</param>
        public RichTextCharacter(char character, int charOffset = 0) {
            Character = character;
            CharOffset = charOffset;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Update the character.
        /// </summary>
        public void Update() {
            Timer += Game.Instance.DeltaTime;

            finalShakeX = Rand.Float(-ShakeX, ShakeX);
            finalShakeY = Rand.Float(-ShakeY, ShakeY);
            finalSinX = Util.SinScale((Timer + SineOffsetX - CharOffset * OffsetAmount) * SineRateX, -SineAmpX, SineAmpX);
            finalSinY = Util.SinScale((Timer + SineOffsetY - CharOffset * OffsetAmount) * SineRateY, -SineAmpY, SineAmpY);
        }

        #endregion

        #region Internal

        internal void Append(VertexArray vertices, float x, float y) {
            var col = new Color(Color0);
            col.R *= Color.R;
            col.G *= Color.G;
            col.B *= Color.B;
            col.A *= Color.A;
        }

        #endregion
        
    }

    /// <summary>
    /// A utility class used for storing default values for a RichText object.
    /// Set the values by using "var config = new RichTextConfig() { Font = "MyFont.ttf", FontSize = 16, ... };"
    /// </summary>
    public class RichTextConfig {

        #region Public Fields

        /// <summary>
        /// The horizontal sine wave amplitude.
        /// </summary>
        public float SineAmpX = 0;

        /// <summary>
        /// The vertical sine wave amplitude.
        /// </summary>
        public float SineAmpY = 0;

        /// <summary>
        /// The horizontal sine wave rate.
        /// </summary>
        public float SineRateX = 1;

        /// <summary>
        /// The vertical sine wave rate.
        /// </summary>
        public float SineRateY = 1;

        /// <summary>
        /// The horizontal sine wave offset.
        /// </summary>
        public float SineOffsetX = 0;

        /// <summary>
        /// The vertical sine wave offset.
        /// </summary>
        public float SineOffsetY = 0;

        /// <summary>
        /// The offset amount for each character.
        /// </summary>
        public float OffsetAmount = 10;

        /// <summary>
        /// The X position of the shadow.
        /// </summary>
        public float ShadowX = 0;

        /// <summary>
        /// The Y position of the shadow.
        /// </summary>
        public float ShadowY = 0;

        /// <summary>
        /// The outline thickness.
        /// </summary>
        public float OutlineThickness = 0;

        /// <summary>
        /// The amount of horizontal shake.
        /// </summary>
        public float ShakeX = 0;

        /// <summary>
        /// The amount of vertical shake.
        /// </summary>
        public float ShakeY = 0;

        /// <summary>
        /// The Color of the character.
        /// </summary>
        public Color CharColor = Color.White;

        /// <summary>
        /// The Color of the top left corner.
        /// </summary>
        public Color CharColor0 = Color.White;

        /// <summary>
        /// The Color of the top left corner.
        /// </summary>
        public Color CharColor1 = Color.White;

        /// <summary>
        /// The Color of the top left corner.
        /// </summary>
        public Color CharColor2 = Color.White;

        /// <summary>
        /// The Color of the top left corner.
        /// </summary>
        public Color CharColor3 = Color.White;

        /// <summary>
        /// The Color of the shadow.
        /// </summary>
        public Color ShadowColor = Color.Black;

        /// <summary>
        /// The Color of the outline.
        /// </summary>
        public Color OutlineColor = Color.White;

        public float LetterSpacing = 1.0f;

        /// <summary>
        /// Controls the spacing between each character. If set above 0 the text will use a monospacing.
        /// </summary>
        public int MonospaceWidth = -1;

        /// <summary>
        /// The alignment of the text.  Left, Right, or Center.
        /// </summary>
        public TextAlign TextAlign = TextAlign.Left;

        /// <summary>
        /// The filepath to the font to use.
        /// </summary>
        public string Font = "";

        /// <summary>
        /// The font size.
        /// </summary>
        public int FontSize = 16;

        /// <summary>
        /// The string to display.
        /// </summary>
        public string String = "";

        /// <summary>
        /// The width of the text block.
        /// </summary>
        public int TextWidth = -1;

        /// <summary>
        /// The height of the text block.
        /// </summary>
        public int TextHeight = -1;

        #endregion
        
    }
}
