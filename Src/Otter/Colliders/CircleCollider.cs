using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Otter {
    /// <summary>
    /// Circle Collider.
    /// </summary>
    public class CircleCollider : Collider {

        #region Public Fields

        /// <summary>
        /// The radius of the circle.
        /// </summary>
        public int Radius;

        #endregion

        #region Public Properties

        public override float Width {
            get { return Radius * 2; }
        }

        public override float Height {
            get { return Radius * 2; }
        }

        #endregion

        #region Constructors

        public CircleCollider(int radius, params int[] tags) {
            Radius = radius;
            AddTag(tags);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Draw the collider for debug purposes.
        /// </summary>
        public override void Render() {
            base.Render();

            if (Entity == null) return;

            Draw.Circle(Left + 1, Top + 1, Radius - 1, Color.None, Color.Red, 1f);
        }

        #endregion

    }
}
