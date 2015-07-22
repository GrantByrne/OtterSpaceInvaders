using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Otter.UI {
    /// <summary>
    /// OTTER UI IS NOT SUPPORTED YET.  ACTIVELY IN DEVELOPMENT, USE AT YOUR OWN RISK!
    /// List UIMenu designed to be navigated by a controller.  One option on a list of options can
    /// be chosen.
    /// </summary>
    public class UIListMenu : UIMenu {

        public SortedDictionary<int, string> ListItems = new SortedDictionary<int, string>();

        protected Counter Counter = new Counter(0, 0, 0);

        public UIListMenu() : base() {
            Counter.Wrap = true;
        }

        public void AddListItem(int value, string name) {
            ListItems.Add(value, name);
            Counter.Max = ListItems.Count - 1;
        }

        public override void HandleInput(UIManager manager) {
            base.HandleInput(manager);

            if (manager.Left.Pressed) {
                Counter.Decrement();
                ItemSwitched();
            }
            if (manager.Right.Pressed) {
                Counter.Increment();
                ItemSwitched();
            }
            if (manager.A.Pressed) {
                Trigger();
                manager.Deactivate();
            }
            if (manager.B.Pressed) {
                CancelAndDeactivate(manager);
            }
        }

        public int CurrentKey {
            get { return ListItems.Keys.ElementAt(Counter); }
        }

        public string CurrentValue {
            get { return ListItems.Values.ElementAt(Counter); }
        }



    }
}
