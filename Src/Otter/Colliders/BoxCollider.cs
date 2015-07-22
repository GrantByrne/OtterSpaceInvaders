using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Otter {
    /// <summary>
    /// Rectangle Collider.
    /// </summary>
    public class BoxCollider : Collider {

        #region Constructors

        /// <summary>
        /// Creates a new box collider.
        /// </summary>
        /// <param name="width">The width of the collider.</param>
        /// <param name="height">The height of the collider.</param>
        /// <param name="tags">Any tags the collider should have.</param>
        public BoxCollider(int width, int height, params int[] tags) {
            Width = width;
            Height = height;
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

            Draw.Rectangle(Left + 1, Top + 1, Width - 2, Height - 2, Color.None, Color.Red, 1f);
        }

        #endregion

    }
}
