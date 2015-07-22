using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Otter {
    /// <summary>
    /// Base Component class.  Components can be added to Entities.
    /// </summary>
    public abstract class Component {

        #region Public Fields

        /// <summary>
        /// The parent Entity of the Component.
        /// </summary>
        public Entity Entity;

        /// <summary>
        /// Determines if the Component should render after the Entity has rendered.
        /// </summary>
        public bool RenderAfterEntity = true;

        /// <summary>
        /// Determines if the Component will render.
        /// </summary>
        public bool Visible = true;

        /// <summary>
        /// How long the Component has been alive (added to an Entity and updated.)
        /// </summary>
        public float Timer = 0;

        #endregion

        #region Public Methods

        /// <summary>
        /// Called when the Component is added to the Entity.
        /// </summary>
        public virtual void Added() {

        }

        /// <summary>
        /// Called when the Component is removed from the Entity.
        /// </summary>
        public virtual void Removed() {

        }

        /// <summary>
        /// Removes the Component from its parent Entity.
        /// </summary>
        public void RemoveSelf() {
            if (Entity != null) {
                Entity.RemoveComponent(this);
            }
        }

        /// <summary>
        /// Called during the UpdateFirst on the parent Entity.
        /// </summary>
        public virtual void UpdateFirst() {

        }

        /// <summary>
        /// Called during the Update on the parent Entity.
        /// </summary>
        public virtual void Update() {

        }

        /// <summary>
        /// Called during the Render on the parent Entity.
        /// </summary>
        public virtual void Render() {

        }

        /// <summary>
        /// Called during the UpdateLast on the parent Entity.
        /// </summary>
        public virtual void UpdateLast() {

        }

        #endregion

    }
}
