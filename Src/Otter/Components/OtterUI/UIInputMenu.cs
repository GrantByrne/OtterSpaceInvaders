using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Otter.UI {
    /// <summary>
    /// OTTER UI IS NOT SUPPORTED YET.  ACTIVELY IN DEVELOPMENT, USE AT YOUR OWN RISK!
    /// Input UIMenu designed to rebind controls.
    /// </summary>
    public class UIInputMenu : UIMenu{

        public enum State {
            Normal,
            WaitingForInput
        }

        public State CurrentState = State.Normal;

        public Key EscapeKey = Key.Escape;

        public Key ValueKey = Key.Unknown;

        public UIInputMenu() {
            
        }

        public override void Activated() {
            base.Activated();
            CurrentState = State.WaitingForInput;
        }

        public override void HandleInput(UIManager manager) {
            base.HandleInput(manager);

            if (CurrentState == State.Normal) {
                if (manager.A.Pressed) {
                    CurrentState = State.WaitingForInput;
                }
                if (manager.B.Pressed) {
                    TriggerAndDeactivate(manager);
                }
            }
            else {
                if (Input.Instance.KeyPressed(EscapeKey)) {
                    CurrentState = State.Normal;
                }
                else if (Input.Instance.KeyPressed(Key.Any)) {
                    ValueKey = Input.Instance.LastKey;
                    CurrentState = State.Normal;
                }
                else if (Input.Instance.MouseButtonPressed(MouseButton.Any)) {

                }
            }
        }
    }
}
