using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Otter {
    /// <summary>
    /// Class used to represent a range using a min and max.
    /// </summary>
    public class Range {

        #region Public Fields

        /// <summary>
        /// The minimum of the range.
        /// </summary>
        public float Min;

        /// <summary>
        /// The maximum of the range.
        /// </summary>
        public float Max;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new Range.
        /// </summary>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        public Range(float min, float max) {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Create a new Range.
        /// </summary>
        /// <param name="max">Maximum value.  Minimum is -Maximum.</param>
        public Range(float max) : this(-max, max) { }

        #endregion
        
    }
}
