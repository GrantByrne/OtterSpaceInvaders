using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Window;
using System.Globalization;

namespace Otter {
    /// <summary>
    /// The debug console.  Only exists when the game is built in Debug Mode.  The game will handle
    /// using this class.  Can be summoned by default with the ~ key.
    /// </summary>
    public class Debugger {

        #region Private Fields

        Text textInput = new Text(24);
        Text textCamera = new Text("Move camera with arrow keys, F2 to exit.", 24);
        Text textPastCommands = new Text(16);
        Text textCommandsBuffered = new Text(12);
        Text textCountdown = new Text(50);
        Text textFramesLeft = new Text(24);
        Text textPerformance = new Text(24);

        Image imgScrollBarBg;
        Image imgScrollBar;

        Image imgOtter;

        Image imgOverlay;
        Image imgError;

        int mouseScrollSpeed = 1;

        int textSizeSmall = 12,
            textSizeMedium = 16,
            textSizeLarge = 24,
            textSizeHuge = 50;

        string keyString = "";

        bool enterPressed = false;
        bool dismissPressed = false;
        internal bool Dismissed = false;
        bool executionError = false;
        bool locked = false;

        int paddingMax = 30;
        int padding = 30;
        int maxLines;
        int scrollBarWidth = 10;
        int textAreaHeight;
        int maxChars = 15;

        float time;

        float x, y;

        Dictionary<string, DebugCommand> commands = new Dictionary<string, DebugCommand>();
        Dictionary<string, DebugCommand> instantCommands = new Dictionary<string, DebugCommand>();

        List<string> commandBuffer = new List<string>();
        List<string> debugLog = new List<string>();

        Dictionary<string, object> watching = new Dictionary<string, object>();

        int debugLogBufferSize = 1000;

        int logIndex;

        float countDownTimer;
        int advanceFrames;

        List<string> inputHistory = new List<string>();
        int inputHistoryIndex;

        bool toggleKeyPressed;

        Surface renderSurface;

        float backgroundAlpha = 0.6f;

        int dismissFor;

        int showPerformance;

        int currentState;

        bool cameraTogglePressed = false;

        int stateNormal;
        int stateCamera = 1;

        int cameraMoveRate = 1;

        bool commandInputEnabled = true;

        int debugCamX;
        int debugCamY;

        #endregion

        #region Public Fields

        /// <summary>
        /// Reference to the active instance of the debugger.
        /// </summary>
        public static Debugger Instance;

        /// <summary>
        /// The delegate for registered commands.
        /// </summary>
        /// <param name="arguments">An array of strings passed in from the console.</param>
        public delegate void CommandFunction(params string[] arguments);

        /// <summary>
        /// The key used to summon and dismiss the debug console.
        /// </summary>
        public Key ToggleKey = Key.Tilde;

        #endregion

        #region Public Properties

        /// <summary>
        /// If the debug console is currently open.
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// If the debug console is currently visible.
        /// </summary>
        public bool Visible { get; private set; }

        /// <summary>
        /// The offset of the camera X set by debug camera mode.
        /// </summary>
        public float DebugCameraX { get; private set; }

        /// <summary>
        /// The offset of the camera Y set by debug camera mode.
        /// </summary>
        public float DebugCameraY { get; private set; }

        #endregion

        #region Private Methods

        #region Default Commands

        void CmdHelp(string[] a) {
            Log("", false);

            Log("== Available Commands:", false);
            foreach (var cmd in commands) {
                string s = cmd.Key + " " + cmd.Value.HelpArguments;
                s = s.PadRight(25, ' ');
                s += cmd.Value.HelpDescription;
                Log(s, false);
            }

            Log("", false);

            Log("== Instant Commands:", false);
            foreach (var cmd in instantCommands) {
                string s = cmd.Key + " " + cmd.Value.HelpArguments;
                s = s.PadRight(25, ' ');
                s += cmd.Value.HelpDescription;
                Log(s, false);
            }

            Log("", false);

            Log("== Other:", false);

            Log("Press F2 to move the camera.", false);

            Log("", false);

            Log("== End of Help.", false);

            Log("", false);
        }

        void CmdMusic(params string[] a) {
            var volume = float.Parse(a[0]);
            Music.GlobalVolume = volume;
        }

        void CmdSound(params string[] a) {
            var volume = float.Parse(a[0]);
            Sound.GlobalVolume = volume;
        }

        void CmdOverlay(string[] a) {
            backgroundAlpha = Util.Clamp(float.Parse(a[0]), 0, 1);
        }

        void CmdExit(string[] a) {
            game.Close();
        }

        void CmdClear(string[] a) {
            inputHistory.Clear();
            inputHistoryIndex = 0;
            debugLog.Clear();
            logIndex = 0;
            Log("Log cleared.");
        }

        void CmdFps(string[] a) {
            showPerformance = int.Parse(a[0]);
        }

        void CmdNext(string[] a) {
            if (game.MeasureTimeInFrames && game.FixedFramerate) {
                countDownTimer = 30;
            }
            else {
                countDownTimer = 0.5f;
            }
            advanceFrames = int.Parse(a[0]);
            advanceFrames = (int)Util.Max(advanceFrames, 1);
            locked = true;
        }

        void CmdSpawn(string[] a) {
            string objectName = a[0];
            int x = int.Parse(a[1]);
            int y = int.Parse(a[2]);

            Type entityType = Util.GetTypeFromAllAssemblies(objectName);

            if (entityType != null) {
                object entity = Activator.CreateInstance(entityType, x, y);
                game.Scene.Add((Entity)entity);
            }
            else {
                throw new ArgumentException("Entity type not found.");
            }
        }

        void CmdWatch(string[] a) {
            Log("", false);
            Log("== Watching Vars", false);
            foreach (var w in watching) {
                Log(w.Key.PadRight(20) + w.Value.ToString(), false);
            }
            Log("", false);
        }

        #endregion

        #region EventHandlers

        void OnTextEntered(object sender, TextEventArgs e) {
            if (locked) return;

            if (!commandInputEnabled) return;

            string hexValue = (Encoding.ASCII.GetBytes(e.Unicode)[0].ToString("X"));
            int ascii = (int.Parse(hexValue, NumberStyles.HexNumber));

            if (e.Unicode == "\b") {
                if (keyString.Length > 0) {
                    keyString = keyString.Remove(keyString.Length - 1, 1);
                }
            }
            else if (ascii >= 32 && ascii < 128) {
                keyString += e.Unicode;
            }
        }

        void OnMouseWheel(object sender, MouseWheelEventArgs e) {
            logIndex -= e.Delta * mouseScrollSpeed;
            UpdateConsoleText();
        }

        void OnKeyPressed(object sender, KeyEventArgs e) {
            if (locked) return;

            //why did I make this without the input manager ugh

            game.Window.SetKeyRepeatEnabled(true);

            if (currentState == stateNormal) {
                switch ((Key)e.Code) {
                    case Key.Return:
                        enterPressed = true;
                        break;

                    case Key.PageUp:
                        logIndex -= 1;
                        if (e.Shift) logIndex -= maxLines;
                        UpdateConsoleText();
                        break;

                    case Key.PageDown:
                        logIndex += 1;
                        if (e.Shift) logIndex += maxLines;
                        UpdateConsoleText();
                        break;

                    case Key.Up:
                        LoadPreviousInput();
                        break;

                    case Key.Down:
                        LoadNextInput();
                        break;

                    case Key.LShift:
                        mouseScrollSpeed = 5;
                        break;

                    case Key.RShift:
                        mouseScrollSpeed = 20;
                        break;

                    case Key.LAlt:
                        Visible = false;
                        break;
                }

                if ((Key)e.Code == ToggleKey) {
                    dismissPressed = true;
                }
            }
            else if (currentState == stateCamera) {
                switch ((Key)e.Code) {

                    case Key.Up:
                        debugCamY -= cameraMoveRate;
                        break;
                    case Key.Down:
                        debugCamY += cameraMoveRate;
                        break;
                    case Key.Left:
                        debugCamX -= cameraMoveRate;
                        break;
                    case Key.Right:
                        debugCamX += cameraMoveRate;
                        break;
                }
            }

            switch ((Key)e.Code) {
                case Key.F2:
                    cameraTogglePressed = true;
                    break;
            }
        }

        void OnKeyReleased(object sender, KeyEventArgs e) {
            if (locked) return;

            switch ((Key)e.Code) {
                case Key.LShift:
                    mouseScrollSpeed = 1;
                    break;

                case Key.RShift:
                    mouseScrollSpeed = 1;
                    break;

                case Key.LAlt:
                    Visible = true;
                    break;
            }
        }

        void OnKeyPressedToggle(object sender, KeyEventArgs e) {
            if ((Key)e.Code == ToggleKey) {
                toggleKeyPressed = true;
            }
        }

        #endregion

        void RegisterInstantCommand(string name, string help, CommandFunction function, params CommandType[] types) {
            instantCommands.Add(name, new DebugCommand(function, types) { HelpDescription = help, Name = name });
        }

        void SendCommand(string str) {
            enterPressed = false;

            if (str == "" || str == null) return;

            str = str.Trim();

            Log(str);

            UpdateInputHistory(str);

            if (commands.ContainsKey(ParseCommandName(str))) {
                commandBuffer.Add(str);
            }
            else if (instantCommands.ContainsKey(ParseCommandName(str))) {
                commandBuffer.Add(str);
                ExecuteCommand();
            }
            else {
                Log("[ERROR] Command Not Found.", false);
                ErrorFlash();
            }

            UpdateConsoleText();

            ClearKeystring();
        }

        void UpdateInputHistory(string str) {
            if (inputHistory.Count > 0) {
                if (inputHistory[inputHistory.Count - 1] != str) {
                    inputHistory.Add(str);
                }
            }
            else {
                inputHistory.Add(str);
            }
            inputHistoryIndex = inputHistory.Count;
        }

        void LoadPreviousInput() {
            if (inputHistory.Count == 0) return;

            inputHistoryIndex -= 1;
            inputHistoryIndex = (int)Util.Clamp(inputHistoryIndex, 0, inputHistory.Count - 1);
            keyString = inputHistory[inputHistoryIndex];
        }

        void LoadNextInput() {
            if (inputHistory.Count == 0) return;

            inputHistoryIndex += 1;
            inputHistoryIndex = (int)Util.Clamp(inputHistoryIndex, 0, inputHistory.Count - 1);
            keyString = inputHistory[inputHistoryIndex];
        }

        string ParseCommandName(string str) {
            return str.Split(' ')[0].ToLower();
        }

        void ExecuteCommands() {
            while (commandBuffer.Count > 0) {
                ExecuteCommand(0);
            }
        }

        void ExecuteCommand(int index = -1) {
            if (index == -1) index = commandBuffer.Count - 1;

            string cmd = commandBuffer[index];
            //parse the string, when inside a quote replace space with something else
            bool inQuote = false;
            string parsedCmd = "";
            for (int i = 0; i < cmd.Length; i++) {
                char nextChar = cmd[i];
                if (cmd[i] == '"') {
                    inQuote = !inQuote;
                }

                if (inQuote) {
                    if (cmd[i] == ' ') {
                        nextChar = (char)16;
                    }
                }
                parsedCmd += nextChar;
            }

            string[] split = parsedCmd.Split(' ');

            string function = split[0].ToLower();
            string[] arguments = new string[split.Length - 1];

            //restore spaces
            for (int i = 1; i < split.Length; i++) {
                split[i] = split[i].Replace((char)16, ' ');
                if (split[i][0] == '"') {
                    //get rid of quotes in string arguments
                    split[i] = split[i].Replace("\"", "");
                }
                arguments[i - 1] = split[i];
            }

            if (commands.ContainsKey(function)) {
                try {
                    commands[function].Execute(arguments);
                }
                catch (Exception ex) {
                    Log("[ERROR] " + ex.Message, false);
                    executionError = true;
                }
            }
            else {
                try {
                    instantCommands[function].Execute(arguments);
                }
                catch (Exception ex) {
                    Log("[ERROR] " + ex.Message, false);
                    ErrorFlash();
                }
            }

            commandBuffer.RemoveAt(index);
        }

        void UpdateConsoleText() {
            textPastCommands.String = "";

            int logMax = (int)Util.Max(debugLog.Count - maxLines, 0);
            logIndex = (int)Util.Clamp(logIndex, 0, logMax);

            int logStart = (int)Util.Clamp(logIndex, 0, logMax);

            for (var i = 0; i < maxLines; i++) {
                if (i < debugLog.Count) {
                    textPastCommands.String += debugLog[i + logStart] + "\n";
                }
            }

            if (commandBuffer.Count > 0) {
                textCommandsBuffered.String = "[" + commandBuffer.Count + "] Commands to be executed.  Press [" + ToggleKey + "] to execute.";
            }
            else {
                textCommandsBuffered.String = "";
            }
        }

        void ClearKeystring() {
            keyString = "";
        }

        void ErrorFlash() {
            imgError.Alpha = 0.5f;
        }

        void UpdatePerformance() {
            textPerformance.String = "";

            if (showPerformance == 1) {
                textPerformance.String = game.Framerate.ToString("00.0") + " FPS";
            }
            else if (showPerformance == 2) {
                textPerformance.String = game.Framerate.ToString("00.0") + " FPS " + game.AverageFramerate.ToString("00.0") + " AVG";
            }
            else if (showPerformance == 3) {
                textPerformance.String = game.Framerate.ToString("00.0") + " FPS " + game.AverageFramerate.ToString("00.0") + " AVG";
                textPerformance.String += "\nUpdate " + game.UpdateCount.ToString("0000") + " Entities";
                textPerformance.String += "\nRender " + game.RenderCount.ToString("0000") + " Renders";
            }
            else if (showPerformance == 4) {
                textPerformance.String = game.Framerate.ToString("00.0") + " FPS " + game.AverageFramerate.ToString("00.0") + " AVG " + game.RealDeltaTime.ToString("0") + "ms";
                textPerformance.String += "\nUpdate " + game.UpdateTime.ToString("00") + "ms (" + game.UpdateCount.ToString("0000") + " Entities)";
                textPerformance.String += "\nRender " + game.RenderTime.ToString("00") + "ms (" + game.RenderCount.ToString("0000") + " Renders)";
            }
            else if (showPerformance >= 5) {
                textPerformance.String = game.Framerate.ToString("00.0") + " FPS " + game.AverageFramerate.ToString("00.0") + " AVG " + game.RealDeltaTime.ToString("0") + "ms " + (GC.GetTotalMemory(false) / 1024 / 1024).ToString("00") + "MB";
                textPerformance.String += "\nUpdate " + game.UpdateTime.ToString("00") + "ms (" + game.UpdateCount.ToString("0000") + " Entities)";
                textPerformance.String += "\nRender " + game.RenderTime.ToString("00") + "ms (" + game.RenderCount.ToString("0000") + " Renders)";
            }

        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a command to the debugger.  Automatically derive the console name by the name of the method.
        /// </summary>
        /// <param name="function">The method to register.</param>
        /// <param name="types">The types for the arguments.</param>
        public void RegisterCommand(CommandFunction function, params CommandType[] types) {
            RegisterCommand(function.Method.Name.ToLower(), function, types);
        }

        /// <summary>
        /// Add a command to the debugger with help text.
        /// </summary>
        /// <param name="function">The method to register.</param>
        /// <param name="help">The help text to display in the console for the function.</param>
        /// <param name="types">The types for the arguments.</param>
        public void RegisterCommand(CommandFunction function, string help, params CommandType[] types) {
            RegisterCommand(function.Method.Name.ToLower(), help, function, types);
        }

        /// <summary>
        /// Add a command to the debugger with a specific name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="function"></param>
        /// <param name="types">The types for the arguments.</param>
        public void RegisterCommand(string name, CommandFunction function, params CommandType[] types) {
            if (commands.ContainsKey(name)) return;
            if (instantCommands.ContainsKey(name)) return;

            commands.Add(name, new DebugCommand(function, types) { Name = name });
        }

        /// <summary>
        /// Add a command to the debugger with a specific name and help text.
        /// </summary>
        /// <param name="name">The name of the method in the debugger.</param>
        /// <param name="help">The help text to display in the console for the function.</param>
        /// <param name="function">The method to register.</param>
        /// <param name="types">The types for the arguments.</param>
        public void RegisterCommand(string name, string help, CommandFunction function, params CommandType[] types) {
            if (commands.ContainsKey(name)) return;
            if (instantCommands.ContainsKey(name)) return;

            commands.Add(name, new DebugCommand(function, types) { HelpDescription = help, Name = name });
        }

        /// <summary>
        /// Remove a command from the debugger.
        /// </summary>
        /// <param name="name">The name of the command to remove.</param>
        public void RemoveCommand(string name) {
            if (commands.ContainsKey(name)) {
                commands.Remove(name);
            }
        }

        /// <summary>
        /// Writes log data to the console.
        /// </summary>
        /// <param name="str">The string to add to the console.</param>
        /// <param name="timestamp">Include a timestamp with the item.</param>
        public void Log(object str, bool timestamp = true) {
            if (str.ToString().Contains('\n')) {
                var split = str.ToString().Split('\n');
                foreach (var s in split) {
                    Log(s);
                }
                return;
            }
            if (logIndex == debugLog.Count - maxLines) {
                logIndex++;
            }
            if (debugLog.Count == debugLogBufferSize) {
                debugLog.RemoveAt(0);
            }
            if (timestamp) {
                string format = game.MeasureTimeInFrames && game.FixedFramerate ? "000000" : "00000.000";
                str = game.Timer.ToString(format) + ": " + str;
            }
            debugLog.Add(str.ToString());

        }

        /// <summary>
        /// Add a variable to the watch list of the debug console.  This must be called on every update
        /// to see the latest value!
        /// </summary>
        /// <param name="str">The label for the value.</param>
        /// <param name="obj">The value.</param>
        public void Watch(string str, object obj) {
            if (watching.ContainsKey(str)) {
                watching.Remove(str);
            }
            watching.Add(str, obj);
        }

        #endregion

        #region Internal

        internal Debugger(Game game) {
            Instance = this;
            this.game = game;

            imgOtter = new Image(new Texture(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Otter.otterlogo.png")));

            imgOtter.Batchable = false;

            imgOtter.CenterOrigin();
            imgOtter.Scroll = 0;

            UpdateSurface();

            textInput.Scroll = 0;
            textInput.OutlineThickness = 2;
            textInput.OutlineColor = Color.Black;

            textPastCommands.Scroll = 0;
            textPastCommands.OutlineColor = Color.Black;
            textPastCommands.OutlineThickness = 1;

            textCommandsBuffered.Scroll = 0;
            textCommandsBuffered.OutlineThickness = 2;
            textCommandsBuffered.OutlineColor = Color.Black;
            textCommandsBuffered.Color = Color.Gold;

            textCountdown.Scroll = 0;

            textCountdown.OutlineThickness = 3;
            textCountdown.OutlineColor = Color.Black;

            textFramesLeft.Scroll = 0;
            textFramesLeft.OutlineThickness = 2;
            textFramesLeft.OutlineColor = Color.Black;

            textPerformance.Scroll = 0;
            textPerformance.OutlineColor = Color.Black;
            textPerformance.OutlineThickness = 2;

            textCamera.OutlineThickness = 3;
            textCamera.OutlineColor = Color.Black;

            RegisterInstantCommand("help", "Shows help.", CmdHelp);
            RegisterInstantCommand("overlay", "Set the opacity of the console background to X.", CmdOverlay, CommandType.Float);
            RegisterInstantCommand("music", "Change the music volume. 0 - 1.", CmdMusic, CommandType.Float);
            RegisterInstantCommand("sound", "Change the sound volume. 0 - 1.", CmdSound, CommandType.Float);
            RegisterInstantCommand("clear", "Clears the console.", CmdClear);
            RegisterInstantCommand("showfps", "Shows performance information. Use 0 - 5.", CmdFps, CommandType.Int);
            RegisterInstantCommand("next", "Advances the game by X updates.", CmdNext, CommandType.Int);
            RegisterInstantCommand("watch", "Display watched values.", CmdWatch);
            RegisterInstantCommand("quit", "Exits the game.", CmdExit);

            RegisterCommand("spawn", "Add a new entity at X, Y.", CmdSpawn, CommandType.String, CommandType.Int, CommandType.Int);

            Log("== Otter Console Initialized!");
            Log("Use 'help' to see available commands.");
            Log("", false);

            IsOpen = false;
            dismissFor = 0;
        }

        internal void UpdateSurface() {
            renderSurface = new Surface((int)game.WindowWidth, (int)game.WindowHeight);
            renderSurface.CenterOrigin();
            renderSurface.X = game.Surface.X;
            renderSurface.Y = game.Surface.Y;
            renderSurface.Smooth = false;

            cameraMoveRate = (int)((renderSurface.Width + renderSurface.Height) * 0.5f * 0.1f);

            imgOverlay = Image.CreateRectangle(renderSurface.Width, renderSurface.Height, Color.Black);
            imgOverlay.Scroll = 0;

            imgError = Image.CreateRectangle(renderSurface.Width, renderSurface.Height, Color.Red);
            imgError.Scroll = 0;
            imgError.Alpha = 0;

            float fontScale = Util.ScaleClamp(renderSurface.Height, 400, 800, 0.67f, 1);
            padding = (int)Util.Clamp(paddingMax * fontScale, paddingMax * 0.25f, paddingMax);

            textInput.FontSize = (int)(textSizeLarge * fontScale);
            textPastCommands.FontSize = (int)(textSizeMedium * fontScale);
            textCommandsBuffered.FontSize = (int)(textSizeSmall * fontScale);
            textCountdown.FontSize = (int)(textSizeHuge * fontScale);
            textFramesLeft.FontSize = (int)(textSizeMedium * fontScale);
            textPerformance.FontSize = (int)(textSizeMedium * fontScale);
            textCamera.FontSize = (int)(textSizeLarge * fontScale);

            imgOtter.Scale = fontScale;

            textFramesLeft.Y = renderSurface.Height - textFramesLeft.LineSpacing;
            textPerformance.Y = 0;

            textInput.Y = renderSurface.Height - textInput.LineSpacing - padding;
            textInput.X = padding;

            textCommandsBuffered.X = textInput.X;
            textCommandsBuffered.Y = textInput.Y + textInput.LineSpacing + 3;

            textAreaHeight = renderSurface.Height - padding * 3 - textInput.LineSpacing;

            maxLines = textAreaHeight / textPastCommands.LineSpacing;
            maxChars = (int)((renderSurface.Width - padding * 2) / (textInput.FontSize * 0.6));

            textPastCommands.Y = padding;
            textPastCommands.X = padding;

            textCountdown.X = renderSurface.HalfWidth;
            textCountdown.Y = renderSurface.HalfHeight;

            imgScrollBarBg = Image.CreateRectangle(scrollBarWidth, textAreaHeight, Color.Black);
            imgScrollBar = Image.CreateRectangle(scrollBarWidth, textAreaHeight, Color.White);

            imgScrollBarBg.X = renderSurface.Width - padding - imgScrollBarBg.Width;
            imgScrollBarBg.Y = padding;

            imgScrollBar.X = imgScrollBarBg.X;
            imgScrollBar.Y = imgScrollBarBg.Y;

            imgOtter.X = renderSurface.HalfWidth;
            imgOtter.Y = renderSurface.HalfHeight;

            imgScrollBar.Scroll = 0;
            imgScrollBarBg.Scroll = 0;

            textCamera.CenterOrigin();
            textCamera.X = renderSurface.HalfWidth;
            textCamera.Y = renderSurface.Height - padding - textCamera.LineSpacing;
        }

        internal void WindowInit() {
            game.Window.KeyPressed += OnKeyPressedToggle;
        }

        internal void AddInput() {
            game.Window.TextEntered += OnTextEntered;
            game.Window.KeyPressed += OnKeyPressed;
            game.Window.MouseWheelMoved += OnMouseWheel;
            game.Window.KeyReleased += OnKeyReleased;
        }

        internal void RemoveInput() {
            game.Window.TextEntered -= OnTextEntered;
            game.Window.KeyPressed -= OnKeyPressed;
            game.Window.MouseWheelMoved -= OnMouseWheel;
            game.Window.KeyReleased -= OnKeyReleased;
        }

        internal Game game;

        internal void Update() {
            Instance = this;

            UpdatePerformance();

            if (currentState == stateNormal) {
                if (cameraTogglePressed) {
                    cameraTogglePressed = false;
                    currentState = stateCamera;
                    commandInputEnabled = false;
                    Visible = false;
                }

                if (toggleKeyPressed) {
                    toggleKeyPressed = false;
                    if (!IsOpen) {
                        Summon();
                    }
                }

                if (dismissFor > 0) {
                    int framesLeft = advanceFrames - dismissFor;
                    textFramesLeft.String = "Update " + framesLeft.ToString("000") + "/" + advanceFrames.ToString("000");
                    dismissFor--;
                    if (dismissFor == 0) {
                        Summon(true);
                    }
                }

                if (dismissPressed) {
                    Dismiss();
                }

                if (!IsOpen) {
                    return;
                }

                textCountdown.String = "";
                if (countDownTimer > 0) {
                    countDownTimer -= game.DeltaTime;
                    if (countDownTimer <= 0) {
                        dismissFor = advanceFrames;
                        locked = false;
                        Dismiss(false);
                        countDownTimer = 0;
                    }
                    if (game.MeasureTimeInFrames && game.FixedFramerate) {
                        textCountdown.String = countDownTimer.ToString("STARTING IN 00");
                    }
                    else {
                        textCountdown.String = countDownTimer.ToString("STARTING IN 00.00");
                    }
                    textCountdown.CenterOrigin();
                    return;
                }
            }
            else if (currentState == stateCamera) {
                if (cameraTogglePressed) {
                    cameraTogglePressed = false;
                    commandInputEnabled = true;
                    currentState = stateNormal;
                    Visible = true;
                }

                DebugCameraX += (debugCamX - DebugCameraX) * 0.25f;
                DebugCameraY += (debugCamY - DebugCameraY) * 0.25f;

                Scene.Instance.UpdateCamera();
            }

            imgOverlay.Alpha = Util.Approach(imgOverlay.Alpha, backgroundAlpha, 0.05f);
            imgScrollBar.Alpha = imgScrollBarBg.Alpha = imgOverlay.Alpha;
            imgOtter.Alpha = imgOverlay.Alpha * 0.25f;

            imgError.Alpha = Util.Approach(imgError.Alpha, 0, 0.02f);

            string displayString = keyString;
            if (keyString.Length > maxChars) displayString = keyString.Substring(keyString.Length - maxChars);
            textInput.String = "> " + displayString + "|";

            imgScrollBar.ScaledHeight = maxLines / Util.Max(debugLog.Count, maxLines) * textAreaHeight;

            int logMax = (int)Util.Max(debugLog.Count - maxLines, 0);
            int scrollpos = (int)Util.Floor(Util.ScaleClamp(logIndex, 0, logMax, 0, textAreaHeight - imgScrollBar.ScaledHeight));

            imgScrollBar.Y = padding + scrollpos;

            if (enterPressed) {
                SendCommand(keyString);
            }

            time += game.DeltaTime;
        }

        internal void Summon(bool autoSummon = false) {
            if (IsOpen) return;
            if (dismissFor > 0) return;

            game.ShowDebugger = true;

            game.Input.bufferReleases = false;

            game.Window.SetKeyRepeatEnabled(false);

            game.debuggerAdvance = 0;
            imgOverlay.Alpha = 0;

            AddInput();

            IsOpen = true;

            if (autoSummon) {
                Log("Next " + advanceFrames + " updates completed.");
            }
            else {
                Log("Debugger opened.");
            }
            UpdateConsoleText();

            Visible = true;
        }

        internal void Dismiss(bool execute = true) {
            if (!IsOpen) return;

            if (execute) ExecuteCommands();

            ClearKeystring();

            if (!executionError) {
                RemoveInput();
                IsOpen = false;
                game.Input.bufferReleases = true;
                Visible = false;
                game.ShowDebugger = false;
                DebugCameraX = 0;
                DebugCameraY = 0;
            }
            else {
                ErrorFlash();
                UpdateConsoleText();
                executionError = false;
            }

            dismissPressed = false;

        }

        internal void Render() {
            game.countRendering = false;

            var tempTarget = Draw.Target;
            Draw.SetTarget(renderSurface);

            Draw.Graphic(textPerformance, x, y);

            if (dismissFor > 0) {
                Draw.Graphic(textFramesLeft, x, y);
            }

            if (Visible) {
                Draw.Graphic(imgOverlay, x, y);
                Draw.Graphic(imgOtter, x, y);
                Draw.Graphic(imgError, x, y);

                if (countDownTimer > 0) {
                    Draw.Graphic(textCountdown, x, y);
                }
                else {
                    Draw.Graphic(imgScrollBarBg, x, y);
                    Draw.Graphic(imgScrollBar, x, y);
                    Draw.Graphic(textInput, x, y);
                    Draw.Graphic(textPastCommands, x, y);
                    Draw.Graphic(textCommandsBuffered, x, y);
                }
            }

            if (currentState == stateCamera) {
                Draw.Graphic(textCamera, x, y);
            }

            Draw.SetTarget(tempTarget);

            renderSurface.DrawToWindow(game);

            game.countRendering = true;
        }

        #endregion    
        
    }

    /// <summary>
    /// Class used for registering and executing commands in the console.
    /// </summary>
    public class DebugCommand {

        #region Private Fields

        protected List<Type> argumentTypes = new List<Type>();

        Debugger.CommandFunction func;

        #endregion

        #region Private Methods

        protected void RegisterTypes(CommandType[] types) {
            foreach (var t in types) {
                switch (t) {
                    case CommandType.Int:
                        argumentTypes.Add(typeof(int));
                        break;
                    case CommandType.Float:
                        argumentTypes.Add(typeof(float));
                        break;
                    case CommandType.Bool:
                        argumentTypes.Add(typeof(bool));
                        break;
                    case CommandType.String:
                        argumentTypes.Add(typeof(string));
                        break;
                }
            }
        }

        #endregion

        #region Internal

        internal string HelpArguments = "";
        internal string HelpDescription = "";

        internal string Name = "";

        internal int ArgumentCount { get; private set; }

        internal DebugCommand(Debugger.CommandFunction func, params CommandType[] types) {
            RegisterTypes(types);
            this.func = func;
            foreach (var t in types) {
                HelpArguments += t.ToString().ToLower() + " ";
            }
            HelpArguments = HelpArguments.TrimEnd(' ');
            ArgumentCount = types.Length;
        }

        internal void Execute(params string[] arguments) {
            if (ValidateTypes(arguments)) {
                func(arguments);
            }
            else {
                throw new ArgumentException(Name + ": Invalid arguments.");
            }
        }

        internal bool ValidateTypes(string[] arguments) {
            if (arguments.Length != ArgumentCount) return false;

            for (int i = 0; i < argumentTypes.Count; i++) {
                if (i >= arguments.Length) return false;

                //float
                if (argumentTypes[i] == typeof(float)) {
                    float value;
                    if (!float.TryParse(arguments[i], out value)) {
                        return false;
                    }
                }
                //int
                if (argumentTypes[i] == typeof(int)) {
                    int value;
                    if (!int.TryParse(arguments[i], out value)) {
                        return false;
                    }
                }
                //bool
                if (argumentTypes[i] == typeof(bool)) {
                    bool value;
                    if (!bool.TryParse(arguments[i], out value)) {
                        return false;
                    }
                }
                //string
                //it should be a string already lol
            }
            return true;
        }

        #endregion

    }

    #region Enum

    /// <summary>
    /// Used for registering types with a command.
    /// </summary>
    public enum CommandType {
        Int,
        Float,
        String,
        Bool
    }

    #endregion

}
