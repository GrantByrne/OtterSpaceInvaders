using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Otter.UI {
    /// <summary>
    /// OTTER UI IS NOT SUPPORTED YET.  ACTIVELY IN DEVELOPMENT, USE AT YOUR OWN RISK!
    /// Core UI element.  All other UI elements extend this.  Currently very early work in progress.
    /// </summary>
    public class UIElement {

        /// <summary>
        /// How many frames to wait before accepting input.  Use this to prevent accidental double inputs.
        /// </summary>
        public int InputStartUp = 0;

        public List<UIElement> Elements = new List<UIElement>();

        public bool Visible = true;

        public UIManager Manager { get; internal set; }

        public UIElement Parent { get; protected set; }

        List<Graphic> graphics = new List<Graphic>();

        float
            localX = 0,
            localY = 0;

        public bool UseMouse = false;

        public Action
            OnTrigger       = () => { },
            OnHighlight     = () => { },
            OnDim           = () => { },
            OnUpdate        = () => { },
            OnEnable        = () => { },
            OnDisable       = () => { },
            OnDismiss       = () => { },
            OnSummon        = () => { },
            OnCancel        = () => { },
            OnActivate      = () => { },
            OnDeactivate    = () => { };

        public int
            Width = 0,
            Height = 0;

        public bool InheritHighlight = false;

        public virtual float X {
            get {
                if (Parent == null) {
                    return localX;
                }
                else {
                    return Parent.X + localX;
                }
            }
            set {
                localX = value;
            }
        }

        public virtual float Y {
            get {
                if (Parent == null) {
                    return localY;
                }
                else {
                    return Parent.Y + localY;
                }
            }
            set {
                localY = value;
            }
        }

        bool summoned = false;
        bool dismissed = false;
        bool enabled = true;
        bool highlighted = false;

        public int UIDepth { get; private set; }

        public bool IsEnabled {
            get { return enabled; }
            set {
                if (value) {
                    Enable();
                }
                else {
                    Disable();
                }
            }
        }

        public UIElement() {
            UIDepth = 0;
        }

        public void Highlight() {
            OnHighlight();

            if (!highlighted) {
                Highlighted();
                highlighted = true;
            }
        }

        public void Dim() {
            OnDim();

            if (highlighted) {
                Dimmed();
                highlighted = false;
            }
        }

        public void Summon() {
            OnSummon();

            if (!summoned) {
                summoned = true;
                Summoned();
            }
        }

        public void Dismiss() {
            OnDismiss();

            if (!dismissed) {
                dismissed = true;
                Dismissed();
            }
        }

        public void Trigger() {
            OnTrigger();

            Triggered();
        }

        public void Cancel() {
            OnCancel();

            Canceled();
        }

        public Graphic AddGraphic(Graphic g) {
            graphics.Add(g);
            return g;
        }

        public List<Graphic> AddGraphics(params Graphic[] graphics) {
            var r = new List<Graphic>();
            foreach (var g in graphics) {
                AddGraphic(g);
                r.Add(g);
            }
            return r;
        }

        public T AddElement<T>(T uielement) where T : UIElement {
            if (uielement.Parent != null) return uielement;

            Elements.Add(uielement);
            uielement.Parent = this;
            uielement.UIDepth = UIDepth + 1;
            return uielement;
        }

        public List<UIElement> AddElements(params UIElement[] uielements) {
            var r = new List<UIElement>();
            foreach (var e in uielements) {
                r.Add(AddElement(e));
            }
            return r;
        }

        public T RemoveElement<T>(T uielement) where T : UIElement {
            if (uielement.Parent == null) return uielement;

            Elements.Remove(uielement);
            uielement.UIDepth = 0;
            uielement.Parent = null;
            return uielement;
        }

        public virtual void Triggered() {

        }

        public virtual void Canceled() {

        }

        public virtual void Highlighted() {

        }

        public virtual void Dimmed() {

        }

        public virtual void Enabled() {
            
        }

        public virtual void Disabled() {

        }

        public virtual void Dismissed() {

        }

        public virtual void Summoned() {

        }

        public virtual void Activated() {

        }

        public virtual void Deactivated() {

        }

        public virtual void Enable() {
            OnEnable();

            if (!enabled) {
                Enabled();
            }
            enabled = true;
            foreach (var e in Elements) {
                e.Enable();
            }
        }

        public virtual void Disable() {
            OnDisable();

            if (enabled) {
                Disabled();
            }
            enabled = false;
            foreach (var e in Elements) {
                e.Disable();
            }
        }

        public virtual void Update() {
            OnUpdate();

            foreach (var e in Elements) {
                e.Update();
            }
        }

        /// <summary>
        /// Handle input from a manager if this element is active.
        /// </summary>
        /// <param name="manager"></param>
        public virtual void HandleInput(UIManager manager) {

        }

        public virtual void Render() {

            if (Visible) {
                foreach (var g in graphics) {
                    Draw.Graphic(g, X, Y);
                }
                foreach (var e in Elements) {
                    e.Render();
                }
            }
        }

    }
}
