using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Window;

namespace Otter {
    /// <summary>
    /// Component used for interpreting input as a button. It can recieve input from multiple sources
    /// including keyboard, mouse buttons, or joystick buttons and axes. The button input can also be
    /// controlled in code.
    /// </summary>
    public class Button : Component {

        #region Private Fields

        bool buttonsDown = false,
            currentButtonsDown = false,
            prevButtonsDown = false;

        #endregion

        #region Public Fields

        /// <summary>
        /// The keys registered to the Button.
        /// </summary>
        public List<Key> Keys = new List<Key>();

        /// <summary>
        /// The joystick Buttons registered to the Button.
        /// </summary>
        public List<List<int>> Buttons = new List<List<int>>();

        /// <summary>
        /// The mouse buttons registered to the Button.
        /// </summary>
        public List<MouseButton> MouseButtons = new List<MouseButton>();

        /// <summary>
        /// The mouse wheel registered to the Button.
        /// </summary>
        public List<MouseWheelDirection> MouseWheel = new List<MouseWheelDirection>();

        /// <summary>
        /// Determines if the Button is enabled.  If not enabled all tests return false.
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// The name of the Button.  Mostly used by the Controller class.
        /// </summary>
        public string Name = "";

        #endregion

        #region Public Properties

        /// <summary>
        /// If the button is currently controlled 
        /// </summary>
        public bool ForcedInput { get; private set; }

        /// <summary>
        /// Check if the button has been pressed.
        /// </summary>
        public bool Pressed {
            get {
                if (!Enabled) return false;

                return currentButtonsDown && !prevButtonsDown;
            }
        }

        /// <summary>
        /// Check if the button has been released.
        /// </summary>
        public bool Released {
            get {
                if (!Enabled) return false;

                return !currentButtonsDown && prevButtonsDown;
            }
        }

        /// <summary>
        /// Check if the button is down.
        /// </summary>
        public bool Down {
            get {
                if (!Enabled) return false;

                return currentButtonsDown;
            }
        }

        /// <summary>
        /// Check if the button is up.
        /// </summary>
        public bool Up {
            get {
                if (!Enabled) return false;

                return !currentButtonsDown;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a Button.
        /// </summary>
        /// <param name="name">Optional string name of the button.</param>
        public Button(string name = "") {
            for (var i = 0; i < Joystick.Count; i++) {
                Buttons.Add(new List<int>());
            }
            Name = name;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add a keyboard key to the Button.
        /// </summary>
        /// <param name="keys">The key to add.</param>
        /// <returns>The Button.</returns>
        public Button AddKey(params Key[] keys) {
            foreach (var k in keys) {
                Keys.Add(k);
            }
            return this;
        }

        /// <summary>
        /// Add a mouse button to the Button.
        /// </summary>
        /// <param name="mouseButtons">The mouse button to add.</param>
        /// <returns>The Button.</returns>
        public Button AddMouseButton(params MouseButton[] mouseButtons) {
            foreach (var mb in mouseButtons) {
                MouseButtons.Add(mb);
            }
            return this;
        }

        /// <summary>
        /// Add the mouse wheel to the Button.
        /// </summary>
        /// <param name="direction">The mouse wheel direction to add.</param>
        /// <returns>The Button.</returns>
        public Button AddMouseWheel(MouseWheelDirection direction) {
            MouseWheel.Add(direction);
            return this;
        }

        /// <summary>
        /// Add a joystick button to the Button.
        /// </summary>
        /// <param name="button">The joystick button to add.</param>
        /// <param name="joystick">The joystick id of the button to add.</param>
        /// <returns></returns>
        public Button AddButton(int button, int joystick = 0) {
            Buttons[joystick].Add(button);
            return this;
        }

        /// <summary>
        /// Add a joystick AxisButton to the Button.
        /// </summary>
        /// <param name="button">The AxisButton to add.</param>
        /// <param name="joystick">The joystick id of the button to add.</param>
        /// <returns></returns>
        public Button AddAxisButton(AxisButton button, int joystick = 0) {
            AddButton((int)button, joystick);
            return this;
        }

        /// <summary>
        /// Force the state of the button.  This will override player input.
        /// </summary>
        /// <param name="state">The state of the button, true for down, false for up.</param>
        public void ForceState(bool state) {
            forceDown = state;
            ForcedInput = true;
        }

        /// <summary>
        /// Release the button's state from forced control.  Restores player input.
        /// </summary>
        public void ReleaseState() {
            ForcedInput = false;
        }

        /// <summary>
        /// Update the button status.
        /// </summary>
        public override void UpdateFirst() {
            base.UpdateFirst();

            buttonsDown = false;

            foreach (var k in Keys) {
                if (Input.Instance.KeyDown(k)) {
                    buttonsDown = true;
                }
            }

            for (int i = 0; i < Buttons.Count; i++) {
                foreach (var button in Buttons[i]) {
                    if (Input.Instance.ButtonDown(button, i)) {
                        buttonsDown = true;
                    }
                }
            }

            foreach (var mb in MouseButtons) {
                if (Input.Instance.MouseButtonDown(mb)) {
                    buttonsDown = true;
                }
            }

            foreach (var w in MouseWheel) {
                if (w == MouseWheelDirection.Down) {
                    if (Input.Instance.MouseWheelDelta > 0) {
                        buttonsDown = true;
                    }
                }
                if (w == MouseWheelDirection.Up) {
                    if (Input.Instance.MouseWheelDelta < 0) {
                        buttonsDown = true;
                    }
                }
            }

            if (ForcedInput) {
                buttonsDown = false;
                if (forceDown) buttonsDown = true;
            }

            prevButtonsDown = currentButtonsDown;
            currentButtonsDown = buttonsDown;

        }

        #endregion

        #region Internal

        internal bool forceDown = false;

        #endregion

    }
}
