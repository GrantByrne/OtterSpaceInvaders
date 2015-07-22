using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Otter.UI {
    /// <summary>
    /// OTTER UI IS NOT SUPPORTED YET.  ACTIVELY IN DEVELOPMENT, USE AT YOUR OWN RISK!
    /// Base class that all UI menus extend.
    /// </summary>
    public class UIMenu : UIElement {

        /// <summary>
        /// If the UIMenu can be canceled by the UIManager or not.  Default to false for the base menu.
        /// </summary>
        public bool Cancelable = false;

        public List<UIElement> MenuItems { get; protected set; }

        public UIMenu() {
            MenuItems = new List<UIElement>();
        }

        /// <summary>
        /// Get a menu item from the menu.
        /// </summary>
        /// <param name="id">The index of the item.</param>
        /// <returns>The menu item at that index.</returns>
        public UIElement this[int index] {
            get { return MenuItems[index]; }
        }

        /// <summary>
        /// Add a menu item to the menu.
        /// </summary>
        /// <typeparam name="T">Inferred by the menu item parameter.</typeparam>
        /// <param name="menuItem">The menu item to add.</param>
        /// <returns>The menu item.</returns>
        public T AddMenuItem<T>(T menuItem) where T : UIElement {
            MenuItems.Add(menuItem);
            if (menuItem is UIMenu) {
                (menuItem as UIMenu).Cancelable = true;
            }
            
            UpdateMenuPositions();
            return menuItem;
        }

        /// <summary>
        /// Add multiple menu items to the menu.
        /// </summary>
        /// <param name="menuItems">The menu items to add.</param>
        /// <returns>The list of added menu items.</returns>
        public List<UIElement> AddMenuItems(params UIElement[] menuItems) {
            var r = new List<UIElement>();
            foreach (var m in menuItems) {
                r.Add(AddMenuItem(m));
            }
            return r;
        }

        /// <summary>
        /// Update the positions of the menu items.
        /// </summary>
        public virtual void UpdateMenuPositions() {

        }

        /// <summary>
        /// Called when the user has switched the currently selected item.
        /// </summary>
        public virtual void ItemSwitched() {

        }

        /// <summary>
        /// The currently selected menu item.
        /// </summary>
        public virtual UIElement CurrentMenuItem {
            get { return null; }
        }

        /// <summary>
        /// The last menu item in the list.
        /// </summary>
        public virtual UIElement LastMenuItem {
            get { return null; }
        }

        public override void Canceled() {
            base.Canceled();

            foreach (var m in MenuItems) {
                m.Dim();
            }
        }

        public override void Update() {
            base.Update();

            UpdateMenuPositions();

            

            foreach (var m in MenuItems) {
                m.Update();
            }
        }

        /// <summary>
        /// A shortcut to cancel and deactivate the menu.
        /// </summary>
        /// <param name="manager"></param>
        public virtual void CancelAndDeactivate(UIManager manager) {
            if (Cancelable) {
                manager.Deactivate();
                Cancel();
            }
        }

        /// <summary>
        /// A shortcut to trigger and deactivate the menu.
        /// </summary>
        /// <param name="manager"></param>
        public virtual void TriggerAndDeactivate(UIManager manager) {
            Trigger();
            manager.Deactivate();
        }

        public override void Render() {
            base.Render();

            foreach (var m in MenuItems) {
                m.Render();
            }
        }
    }
}
