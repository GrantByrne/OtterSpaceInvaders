﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Otter {
    /// <summary>
    /// Graphic used to display simple text.  Much faster than RichText, but more limited options.
    /// </summary>
    public class Text : Graphic {

        #region Private Fields

        TextStyle textStyle;

        #endregion

        #region Private Properties

        bool isUsingShadow {
            get {
                return ShadowX != 0 && ShadowY != 0;
            }
        }

        bool isUsingOutline {
            get {
                return OutlineThickness > 0;
            }
        }

        #endregion

        #region Public Fields

        /// <summary>
        /// The color of the text shadow.
        /// </summary>
        public Color ShadowColor = Color.Black;

        /// <summary>
        /// The X position of the shadow.
        /// </summary>
        public int ShadowX;

        /// <summary>
        /// The Y position of the shadow.
        /// </summary>
        public int ShadowY;

        /// <summary>
        /// The Color of the outline.
        /// </summary>
        public Color OutlineColor = Color.Black;

        /// <summary>
        /// The thickness of the outline.
        /// </summary>
        public int OutlineThickness;

        /// <summary>
        /// The quality of the outline.  Higher quality is more rendering passes!
        /// </summary>
        public TextOutlineQuality OutlineQuality = TextOutlineQuality.Good;

        #endregion

        #region Public Properties

        /// <summary>
        /// The displayed string.
        /// </summary>
        public string String {
            get {
                return text.DisplayedString;
            }
            set {
                text.DisplayedString = value;
                NeedsUpdate = true;
                Width = (int)text.GetLocalBounds().Width;
                Height = (int)text.GetLocalBounds().Height;
                Lines = text.DisplayedString.Split('\n').Length;
            }
        }

        /// <summary>
        /// The number of lines in the text.
        /// </summary>
        public int Lines { get; private set; }

        /// <summary>
        /// The amount of space between each line of text.
        /// </summary>
        public int LineSpacing {
            get { return text.Font.GetLineSpacing(text.CharacterSize); }
        }

        /// <summary>
        /// The font size.
        /// </summary>
        public int FontSize {
            get { return (int)text.CharacterSize; }
            set { text.CharacterSize = (uint)value; }
        }

        /// <summary>
        /// Set the TextStyle (bold, italic, underline.)
        /// </summary>
        public TextStyle TextStyle {
            get { return textStyle; }
            set {
                textStyle = value;
                text.Style = (SFML.Graphics.Text.Styles)TextStyle;
                NeedsUpdate = true;
            }
        }

        /// <summary>
        /// Set both ShadowX and ShadowY.
        /// </summary>
        public int Shadow {
            set {
                ShadowX = value; ShadowY = value;
            }
        }

        /// <summary>
        /// Get the actual center Y of the Text.
        /// </summary>
        public float CenterY {
            get {
                return HalfHeight + BoundsTop;
            }
        }

        /// <summary>
        /// The top bounds of the Text.
        /// </summary>
        public float BoundsTop {
            get {
                return text.GetLocalBounds().Top;
            }
        }

        /// <summary>
        /// The left bounds of the Text.
        /// </summary>
        public float BoundsLeft {
            get {
                return text.GetLocalBounds().Left;
            }
        }


        #endregion

        #region Constructors

        /// <summary>
        /// Create a new Text object.
        /// </summary>
        /// <param name="str">The string to display.</param>
        /// <param name="font">The file path to the font to use.</param>
        /// <param name="size">The size of the font.</param>
        public Text(string str, string font = "", int size = 16)
            : base() {
            Initialize(str, font, size);
        }

        /// <summary>
        /// Create a new Text object.
        /// </summary>
        /// <param name="str">The string to display.</param>
        /// <param name="font">The stream to load the font to use.</param>
        /// <param name="size">The size of the font.</param>
        public Text(string str, Stream font, int size = 16)
            : base() {
            Initialize(str, font, size);
        }

        /// <summary>
        /// Create a new Text object.
        /// </summary>
        /// <param name="str">The string to display.</param>
        /// <param name="size">The size of the font.</param>
        public Text(string str, int size = 16) : this(str, "", size) { }

        /// <summary>
        /// Create a new Text object.
        /// </summary>
        /// <param name="size">The size of the font.</param>
        public Text(int size = 16) : this("", "", size) { }

        #endregion

        #region Private Methods

        void Initialize(string str, object font, int size) {
            if (size < 0) throw new ArgumentException("Font size must be greater than 0.");

            if (font is string) {
                if ((string)font == "") {
                    this.font = Fonts.DefaultFont;
                }
                else {
                    this.font = Fonts.Load((string)font);
                }
            }
            else {
                Fonts.Load((Stream)font);
            }

            text = new SFML.Graphics.Text(str, this.font, (uint)size);
            String = str;
            Name = "Text";

            SFMLDrawable = text;
        }

        protected override void UpdateDrawable() {
            base.UpdateDrawable();

            Width = (int)text.GetLocalBounds().Width;
            Height = (int)text.GetLocalBounds().Height;
        }

        #endregion

        #region Public Methods
        
        /// <summary>
        /// Center the Text's origin. This factors in the Text's local bounds.
        /// </summary>
        public void CenterTextOrigin() {
            CenterTextOriginX();
            CenterTextOriginY();
        }

        /// <summary>
        /// Center the Text's X origin.  This factors in the Text's left bounds.
        /// </summary>
        public void CenterTextOriginX() {
            OriginX = HalfWidth + BoundsLeft;
        }

        /// <summary>
        /// Center the Text's Y origin.  This factors in the Text's top bounds.
        /// </summary>
        public void CenterTextOriginY() {
            OriginY = HalfHeight + BoundsTop;
        }
        
        /// <summary>
        /// Draw the Text.
        /// </summary>
        /// <param name="x">The X position offset.</param>
        /// <param name="y">The Y position offset.</param>
        public override void Render(float x = 0, float y = 0) {
            if (isUsingOutline) {
                text.Color = OutlineColor.SFMLColor;
                var angleIncrement = (int)OutlineQuality;
                for (float o = OutlineThickness * 0.5f; o < OutlineThickness; o += 0.5f) {
                    for (int a = 0; a < 360; a += angleIncrement) {
                        var rx = x + Util.PolarX(a, o);
                        var ry = y + Util.PolarY(a, o);

                        base.Render(rx, ry);
                    }
                }
            }

            if (isUsingShadow) {
                text.Color = ShadowColor.SFMLColor;
                base.Render(x + ShadowY, y + ShadowX);
            }

            text.Color = Color.SFMLColor;
            base.Render(x, y);
        }

        #endregion

        #region Internal

        internal SFML.Graphics.Text text;
        internal SFML.Graphics.Font font;

        #endregion
        
    }

    #region Enum

    [Flags]
    public enum TextStyle {
        Regular = 0,
        Bold = 1,
        Italic = 2,
        Underlined = 4,
    }

    public enum TextOutlineQuality {
        Good = 45,
        Better = 30,
        Best = 15,
        Absurd = 10
    }

    #endregion
}
