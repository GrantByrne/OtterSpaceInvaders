using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Otter {
    /// <summary>
    /// Component representing a group of Button and Axis classes. The controller updates all buttons
    /// and axes manually. This is used by the Session class to manage player input.
    /// 
    /// Input recording should only be used in fixed framerate games!  If used with variable framerate
    /// the playback is not reliable.
    /// </summary>
    public class Controller : Component {

        #region Static Methods

        /// <summary>
        /// Creates a controller assuming the Windows interpretation of an Xbox360 Controller.
        /// </summary>
        /// <param name="joystickId">The id of the joystick.</param>
        /// <returns>A configured controller.</returns>
        public static Controller Get360Controller(params int[] joystickIds) {

            var Controller = new Controller();
            foreach (var joystick in joystickIds) {
                Controller.JoystickIds.Add(joystick);

                Controller.Up.AddAxisButton(AxisButton.PovYMinus, joystick);
                Controller.Down.AddAxisButton(AxisButton.PovYPlus, joystick);
                Controller.Left.AddAxisButton(AxisButton.PovXMinus, joystick);
                Controller.Right.AddAxisButton(AxisButton.PovXPlus, joystick);

                Controller.Up.AddAxisButton(AxisButton.YMinus, joystick);
                Controller.Down.AddAxisButton(AxisButton.YPlus, joystick);
                Controller.Left.AddAxisButton(AxisButton.XMinus, joystick);
                Controller.Right.AddAxisButton(AxisButton.XPlus, joystick);

                Controller.A.AddButton(0, joystick);
                Controller.B.AddButton(1, joystick);
                Controller.X.AddButton(2, joystick);
                Controller.Y.AddButton(3, joystick);
                Controller.L1.AddButton(4, joystick);
                Controller.R1.AddButton(5, joystick);
                Controller.Select.AddButton(6, joystick);
                Controller.Start.AddButton(7, joystick);
                Controller.AxisLeftClick.AddButton(8, joystick);
                Controller.AxisRightClick.AddButton(9, joystick);

                Controller.R2.AddAxisButton(AxisButton.ZMinus, joystick);
                Controller.L2.AddAxisButton(AxisButton.ZPlus, joystick);

                Controller.AxisLeft.AddAxis(JoyAxis.X, JoyAxis.Y, joystick);
                Controller.AxisRight.AddAxis(JoyAxis.U, JoyAxis.R, joystick);
                Controller.AxisDPad.AddAxis(JoyAxis.PovX, JoyAxis.PovY, joystick);
                Controller.AxisTriggers.AddAxis(JoyAxis.Z, JoyAxis.Z, joystick);
            }

            return Controller;
        }

        #endregion

        #region Private Fields

        int recordingTimer = 0;
        int playingTimer = 0;
        int playbackMax = 0;

        Dictionary<string, Button> buttonNames = new Dictionary<string, Button>();
        Dictionary<int, Dictionary<string, int>> recordedButtonData = new Dictionary<int, Dictionary<string, int>>();
        Dictionary<int, Dictionary<string, int>> playbackButtonData = new Dictionary<int, Dictionary<string, int>>();

        List<Axis> axes = new List<Axis>();
        List<Dictionary<int, AxisData>> recordedAxisData = new List<Dictionary<int, AxisData>>();
        List<Dictionary<int, AxisData>> playbackAxisData = new List<Dictionary<int, AxisData>>();

        struct AxisData {
            public float X;
            public float Y;
        }

        #endregion

        #region Public Fields

        /// <summary>
        /// The joystick id associated with this controller.
        /// </summary>
        public List<int> JoystickIds = new List<int>();

        /// <summary>
        /// Determines if the controller is enabled. If not, all buttons return false, and all axes return 0, 0.
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// If the controller should record axes data.
        /// </summary>
        public bool RecordAxes = true;

        public Button
            Up = new Button("Up"),
            Right = new Button("Right"),
            Down = new Button("Down"),
            Left = new Button("Left"),
            A = new Button("A"),
            B = new Button("B"),
            X = new Button("X"),
            Y = new Button("Y"),
            R1 = new Button("R1"),
            R2 = new Button("R2"),
            L1 = new Button("L1"),
            L2 = new Button("L2"),
            Start = new Button("Start"),
            Select = new Button("Select"),
            Home = new Button("Home"),
            AxisLeftClick = new Button("L3"),
            AxisRightClick = new Button("R3");

        public Axis
            AxisLeft = new Axis(),
            AxisRight = new Axis(),
            AxisTriggers = new Axis(),
            AxisDPad = new Axis();

        #endregion

        #region Public Properties

        /// <summary>
        /// If the controller is currently recording input.
        /// </summary>
        public bool Recording { get; private set; }

        /// <summary>
        /// If the controller is currently playing input data.
        /// </summary>
        public bool Playing { get; private set; }

        /// <summary>
        /// Alias for the PlayStation Square button.
        /// </summary>
        public Button Square {
            get { return X; }
            set { X = value; }
        }

        /// <summary>
        /// Alias for the PlayStation X button.
        /// </summary>
        public Button Cross {
            get { return A; }
            set { A = value; }
        }

        /// <summary>
        /// Alias for the PlayStation Circle button.
        /// </summary>
        public Button Circle {
            get { return B; }
            set { B = value; }
        }

        /// <summary>
        /// Alias for the PlayStation Triangle button.
        /// </summary>
        public Button Triangle {
            get { return Y; }
            set { Y = value; }
        }

        /// <summary>
        /// The last recorded data as a compressed string.
        /// </summary>
        public string LastRecordedString {
            get {
                string s = "";
                //save button data
                foreach (var rec in recordedButtonData) {
                    s += rec.Key.ToString() + ":";
                    foreach (var e in rec.Value) {
                        s += e.Key + ">" + e.Value + "|";
                    }
                    s = s.TrimEnd('|');
                    s += ".";
                }

                s = s.TrimEnd('.');

                //save axis data
                s += "%";
                foreach (var axisdata in recordedAxisData) {
                    foreach (var axis in axisdata) {
                        s += axis.Key + ">";
                        s += axis.Value.X.ToString() + "," + axis.Value.Y.ToString() + ";";
                    }
                    s.TrimEnd(';');
                    s += "^";
                }
                s.TrimEnd('^');
                return Util.CompressString(s);
            }
        }

        #endregion

        #region Constructors

        public Controller(params int[] joystickId) {
            RegisterButton(Up, Right, Down, Left, A, B, X, Y, R1, R2, L1, L2, Start, Select, Home, AxisLeftClick, AxisRightClick);
            axes.Add(AxisLeft);
            axes.Add(AxisRight);
            axes.Add(AxisDPad);
            axes.Add(AxisTriggers);

            foreach (var i in joystickId) {
                JoystickIds.Add(i);
            }
        }

        #endregion

        #region Public Methods

        void RegisterButton(params Button[] buttons) {
            foreach (var b in buttons) {
                buttonNames.Add(b.Name, b);
            }
        }

        public override void UpdateFirst() {
            base.UpdateFirst();

            AxisLeft.Enabled = Enabled;
            AxisRight.Enabled = Enabled;
            AxisDPad.Enabled = Enabled;
            Up.Enabled = Enabled;
            Down.Enabled = Enabled;
            Left.Enabled = Enabled;
            Right.Enabled = Enabled;
            A.Enabled = Enabled;
            B.Enabled = Enabled;
            X.Enabled = Enabled;
            Y.Enabled = Enabled;
            R1.Enabled = Enabled;
            R2.Enabled = Enabled;
            L1.Enabled = Enabled;
            L2.Enabled = Enabled;
            Start.Enabled = Enabled;
            Select.Enabled = Enabled;
            Home.Enabled = Enabled;
            AxisLeftClick.Enabled = Enabled;
            AxisRightClick.Enabled = Enabled;

            Up.UpdateFirst();
            Down.UpdateFirst();
            Left.UpdateFirst();
            Right.UpdateFirst();
            A.UpdateFirst();
            B.UpdateFirst();
            X.UpdateFirst();
            Y.UpdateFirst();
            R1.UpdateFirst();
            R2.UpdateFirst();
            L1.UpdateFirst();
            L2.UpdateFirst();
            Start.UpdateFirst();
            Select.UpdateFirst();
            Home.UpdateFirst();
            AxisLeftClick.UpdateFirst();
            AxisRightClick.UpdateFirst();
            AxisLeft.UpdateFirst();
            AxisRight.UpdateFirst();
            AxisDPad.UpdateFirst();

            // The recording and playback code is pretty ugly, sorry :I
            if (Recording) {
                foreach (var b in buttonNames) {
                    if (b.Value.Pressed || b.Value.Released) {
                        if (!recordedButtonData.ContainsKey(recordingTimer)) {
                            recordedButtonData.Add(recordingTimer, new Dictionary<string, int>());
                        }
                    }
                    if (b.Value.Pressed) {
                        recordedButtonData[recordingTimer].Add(b.Key, 1);
                    }
                    if (b.Value.Released) {
                        recordedButtonData[recordingTimer].Add(b.Key, 0);
                    }
                }

                if (RecordAxes) {
                    if (AxisLeft.HasInput && (AxisLeft.X != AxisLeft.LastX || AxisLeft.Y != AxisLeft.LastY)) {
                        recordedAxisData[0].Add(recordingTimer, new AxisData { X = AxisLeft.X, Y = AxisLeft.Y });
                        //Console.WriteLine("Time: " + recordingTimer + " X: " + AxisLeft.X + " Y: " + AxisLeft.Y);
                    }

                    if (AxisRight.HasInput && (AxisRight.X != AxisRight.LastX || AxisRight.Y != AxisRight.LastY))
                        recordedAxisData[1].Add(recordingTimer, new AxisData { X = AxisRight.X, Y = AxisRight.Y });

                    if (AxisDPad.HasInput && (AxisDPad.X != AxisDPad.LastX || AxisDPad.Y != AxisDPad.LastY))
                        recordedAxisData[2].Add(recordingTimer, new AxisData { X = AxisDPad.X, Y = AxisDPad.Y });

                    if (AxisTriggers.HasInput && (AxisTriggers.X != AxisTriggers.LastX || AxisTriggers.LastY != 0))
                        recordedAxisData[3].Add(recordingTimer, new AxisData { X = AxisTriggers.X, Y = AxisTriggers.Y });
                }

                recordingTimer++;
            }
            if (Playing) {

                if (playingTimer > playbackMax) {
                    Stop();
                }

                if (playbackButtonData.ContainsKey(playingTimer)) {
                    foreach (var act in playbackButtonData[playingTimer]) {
                        if (act.Value == 0) {
                            buttonNames[act.Key].ForceState(false);
                            //Util.Log("Time: " + playingTimer + " " + act.Key + " Released");
                        }
                        else {
                            buttonNames[act.Key].ForceState(true);
                            //Util.Log("Time: " + playingTimer + " " + act.Key + " Pressed");
                        }
                    }
                }

                var i = 0;
                foreach (var a in axes) {
                    if (playbackAxisData[i].ContainsKey(playingTimer)) {
                        a.ForceState(playbackAxisData[i][playingTimer].X, playbackAxisData[i][playingTimer].Y);
                        //Util.Log("Time: " + playingTimer + " X: " + playbackAxisData[i][playingTimer].X + " Y: " + playbackAxisData[i][playingTimer].Y);
                    }
                    i++;
                }

                playingTimer++;
            }
        }

        /// <summary>
        /// Record the input to a string.  Optionally save it out to a file when finished.
        /// </summary>
        public void Record() {
            Playing = false;
            Recording = true;
            recordingTimer = 0;

            recordedButtonData.Clear();
            recordedAxisData.Clear();
            recordedAxisData.Add(new Dictionary<int, AxisData>());
            recordedAxisData.Add(new Dictionary<int, AxisData>());
            recordedAxisData.Add(new Dictionary<int, AxisData>());
            recordedAxisData.Add(new Dictionary<int, AxisData>());

        }

        /// <summary>
        /// Play back recorded input data.
        /// </summary>
        /// <param name="source">The recorded data.</param>
        public void Playback(string source) {
            PlaybackInternal(source);
        }

        public void PlaybackFile(string path) {
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/" + path;
            PlaybackInternal(File.ReadAllText(path));
        }

        /// <summary>
        /// Save the last recorded input data to a file.
        /// </summary>
        /// <param name="path">The path to save the data to.</param>
        public void SaveRecording(string path = "") {
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/" + path;
            File.WriteAllText(path, LastRecordedString);
        }

        /// <summary>
        /// Stop the recording or playback of the controller.  This will also release input states.
        /// </summary>
        public void Stop() {
            if (Recording || Playing) {
                foreach (var b in buttonNames) {
                    b.Value.ReleaseState();
                }
                foreach (var a in axes) {
                    a.ReleaseState();
                }
            }

            Recording = false;
            Playing = false;
        }

        public void Disable() {
            Enabled = false;
        }

        public void Enable() {
            Enabled = true;
        }

        #endregion

        #region Private Methods

        void PlaybackInternal(string source) {
            Recording = false;
            Playing = true;
            playingTimer = 0;
            playbackMax = 0;

            playbackButtonData.Clear();
            playbackAxisData.Clear();
            playbackAxisData.Add(new Dictionary<int, AxisData>());
            playbackAxisData.Add(new Dictionary<int, AxisData>());
            playbackAxisData.Add(new Dictionary<int, AxisData>());
            playbackAxisData.Add(new Dictionary<int, AxisData>());

            string s = Util.DecompressString(source);

            var sb = s.Split('%');

            if (sb[0] != "") {
                var split = sb[0].Split('.');
                foreach (var rec in split) {
                    var timedata = rec.Split(':');
                    var time = int.Parse(timedata[0]);
                    playbackMax = (int)Util.Max(time, playbackMax);
                    playbackButtonData.Add(time, new Dictionary<string, int>());
                    var entries = timedata[1].Split('|');
                    foreach (var e in entries) {
                        var data = e.Split('>');
                        playbackButtonData[time].Add(data[0], int.Parse(data[1]));
                    }
                }
            }

            var i = 0;
            foreach (var axesdata in sb[1].Split('^')) {
                foreach (var axis in axesdata.Split(';')) {
                    if (axis == "") continue;
                    var axisdata = axis.Split('>');
                    var time = int.Parse(axisdata[0]);
                    playbackMax = (int)Util.Max(time, playbackMax);
                    axisdata = axisdata[1].Split(',');
                    var x = axisdata[0];
                    var y = axisdata[1];
                    playbackAxisData[i].Add(time, new AxisData { X = float.Parse(x), Y = float.Parse(y) });
                }
                i++;
            }

            foreach (var b in buttonNames) {
                b.Value.ForceState(false);
            }
            foreach (var a in axes) {
                a.ForceState(0, 0);
            }
        }

        #endregion

    }
}
