using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Otter.UI {
    /// <summary>
    /// OTTER UI IS NOT SUPPORTED YET.  ACTIVELY IN DEVELOPMENT, USE AT YOUR OWN RISK!
    /// UIMenu that controls a value that can be increased or decreased.
    /// </summary>
    public class UIValueMenu : UIMenu {

        public Counter Counter;

        int defaultValue;

        public UIValueMenu(int value, int min, int max) : base() {
            Counter = new Counter(value, min, max);
        }

        public override void HandleInput(UIManager manager) {
            base.HandleInput(manager);

            if (manager.Left.Pressed) {
                Counter.Decrement();
            }
            if (manager.Right.Pressed) {
                Counter.Increment();
            }
            if (manager.A.Pressed) {
                TriggerAndDeactivate(manager);
            }
            if (manager.B.Pressed) {
                CancelAndDeactivate(manager);
            }
        }

        public override void Activated() {
            base.Activated();

            defaultValue = Counter;
        }

        public override void Canceled() {
            base.Canceled();

            Counter.Value = defaultValue;
        }

        public override void Update() {
            base.Update();
        }
    }
}
