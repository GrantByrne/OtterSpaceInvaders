using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Otter.UI {
    /// <summary>
    /// OTTER UI IS NOT SUPPORTED YET.  ACTIVELY IN DEVELOPMENT, USE AT YOUR OWN RISK!
    /// Manager of UI elements and UI menus.
    /// </summary>
    public class UIManager : Component {

        List<UIElement> elementStack = new List<UIElement>();
        List<UIElement> elements = new List<UIElement>();

        /// <summary>
        /// How long the current ActiveElement has been active.
        /// </summary>
        public float ActiveTimer = 0;

        public Button
            Up,
            Down,
            Left,
            Right,
            A,
            B,
            MouseLeft,
            MouseRight;

        public int
            MouseX,
            MouseY,
            MouseWheel;

        public UIManager() {
            MouseLeft = new Button().AddMouseButton(MouseButton.Left);
            MouseRight = new Button().AddMouseButton(MouseButton.Right);
        }

        /// <summary>
        /// Assign buttons to the menu using a controller that contains buttons
        /// named Up Down Left Right A B.
        /// </summary>
        /// <param name="Controller"></param>
        public void AssignButtons(object controller) {
            foreach (var s in new string[] { "Up", "Down", "Left", "Right", "A", "B" }) {
                if (Util.HasField(controller, s)) {
                    Util.SetFieldValue(this, s, Util.GetFieldValue(controller, s) as Button);
                }
            }
        }

        public UIElement AddElement(UIElement element, bool children = false) {
            if (element.Manager != null) return element;

            if (children) {
                foreach (var e in element.Elements) {
                    AddElement(e, children);
                }
            }

            elements.Add(element);
            element.Manager = this;
            return element;
        }

        public List<UIElement> AddElements(params UIElement[] elements) {
            var r = new List<UIElement>();
            foreach (var e in elements) {
                r.Add(AddElement(e));
            }
            return r;
        }

        public UIElement RemoveElement(UIElement element, bool children = false) {
            if (element.Manager == null) return element;

            if (children) {
                foreach (var e in element.Elements) {
                    RemoveElement(e, children);
                }
            }

            elements.Remove(element);
            element.Manager = null;
            return element;
        }

        public UIElement ActiveElement {
            get {
                if (elementStack.Count > 0) {
                    return elementStack[elementStack.Count - 1];
                }
                return null;
            }
        }

        public UIElement Activate(UIElement element) {
            elementStack.Add(element);
            element.Activated();
            element.OnActivate();
            ActiveTimer = 0;
            return element;
        }

        public UIElement Deactivate(UIElement element) {
            elementStack.Remove(element);
            return element;
        }

        public UIElement Deactivate() {
            var element = ActiveElement;
            element.Deactivated();
            element.OnDeactivate();
            elementStack.RemoveAt(elementStack.Count - 1);
            ActiveTimer = 0;
            return element;
        }

        public override void Update() {
            base.Update();

            MouseX = Input.Instance.MouseX;
            MouseY = Input.Instance.MouseY;
            MouseWheel = Input.Instance.MouseWheelDelta;

            if (ActiveElement != null) {
                if (ActiveTimer >= ActiveElement.InputStartUp) {
                    ActiveElement.HandleInput(this);
                }
                ActiveTimer += Game.Instance.DeltaTime;
            }

            foreach (var e in elements) {
                e.Update();
            }
        }

        public override void Render() {
            base.Render();

            foreach (var e in elements) {
                e.Render();
            }
        }
    }
}
