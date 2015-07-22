using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Otter.UI {
    /// <summary>
    /// OTTER UI IS NOT SUPPORTED YET.  ACTIVELY IN DEVELOPMENT, USE AT YOUR OWN RISK!
    /// Grid-based UIMenu designed to be navigated by a controller with button input.
    /// </summary>
    public class UIGridMenu : UIMenu {

        public GridCounter MenuGrid;

        public int MenuWidth = 0;

        public int
            XSpacing = 0,
            YSpacing = 0;

        public UIGridMenu(int menuWidth = 1) : base() {
            MenuWidth = menuWidth;
            MenuGrid = new GridCounter(0, menuWidth);
        }

        public override void HandleInput(UIManager manager) {
            if (manager.Right.Pressed) {
                MenuGrid.X += 1;
            }
            if (manager.Left.Pressed) {
                MenuGrid.X -= 1;
            }
            if (manager.Down.Pressed) {
                MenuGrid.Y += 1;
            }
            if (manager.Up.Pressed) {
                MenuGrid.Y -= 1;
            }
            for (var i = 0; i < MenuItems.Count; i++) {
                if (MenuGrid == i) {
                    MenuItems[i].Highlight();
                }
                else {
                    MenuItems[i].Dim();
                }
            }
            if (manager.A.Pressed) {
                if (CurrentMenuItem is UIMenu) {
                    manager.Activate(CurrentMenuItem);
                }
                else {
                    Trigger();
                }
            }
            else if (manager.B.Pressed) {
                CancelAndDeactivate(manager);
            }
        }

        public override void Triggered() {
            base.Triggered();

            CurrentMenuItem.Trigger();
        }

        public override UIElement CurrentMenuItem {
            get { return MenuItems[MenuGrid]; }
        }

        public override UIElement LastMenuItem {
            get { return MenuItems[MenuItems.Count - 1]; }
        }


        public override void Canceled() {
            base.Canceled();

            MenuGrid.Index = 0;
        }

        public override void UpdateMenuPositions() {
            if (MenuItems.Count > MenuGrid.Count) {
                MenuGrid.Height++;
            }

            foreach (var m in MenuItems) {
                if (m is UIMenu) {
                    (m as UIMenu).UpdateMenuPositions();
                }
            }

            float xx = 0, yy = 0;
            float maxheight = 0;
            for (var i = 0; i < MenuItems.Count; i++) {
                MenuItems[i].X = xx + X;
                MenuItems[i].Y = yy + Y;

                xx += XSpacing;

                var heightcheck = MenuItems[i].Height + YSpacing;
                maxheight = Util.Max(heightcheck, maxheight);

                xx += MenuItems[i].Width;

                if (MenuWidth > 0) {
                    if (i % MenuWidth == MenuWidth - 1) {
                        Width = (int)Util.Max(xx, Width);
                        xx = 0;
                        yy += maxheight;
                        maxheight = 0;
                        Height = (int)yy;
                    }
                }
            }
        }



        public override void Update() {
            base.Update();

        }
    }
}
