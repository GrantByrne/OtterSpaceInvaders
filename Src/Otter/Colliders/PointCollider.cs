using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Otter {
    /// <summary>
    /// Point Collider.
    /// </summary>
    public class PointCollider : Collider {

        #region Constructors

        public PointCollider(int x, int y, params int[] tags) {
            Width = 1;
            Height = 1;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Draw the collider for debug purposes.
        /// </summary>
        public override void Render() {
            base.Render();

            if (Entity == null) return;

            Draw.Circle(X, Y, 0.5f, Color.Red);
        }

        #endregion

    }
}
