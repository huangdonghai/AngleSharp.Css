#nullable disable
namespace AngleSharp.Css.Values
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Text;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a subgrid track axis: the borrowed tracks of the grid area the box spans, named by
    /// the groups written after the keyword. Each group names one borrowed line, in order, and a group
    /// written empty stands for a line that gets no name of its own (CSS Grid 2 §8).
    /// </summary>
    public readonly struct CssSubgridValue : ICssPrimitiveValue, IEquatable<CssSubgridValue>
    {
        #region Fields

        private readonly CssLineNamesValue[] _lines;

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new subgrid axis with the line names written for it.
        /// </summary>
        /// <param name="lines">The groups of names, one per borrowed line.</param>
        public CssSubgridValue(IEnumerable<CssLineNamesValue> lines)
        {
            _lines = lines.ToArray();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the contained line name groups.
        /// </summary>
        public CssLineNamesValue[] Lines => _lines;

        /// <summary>
        /// Gets the CSS text representation.
        /// </summary>
        public String CssText => "subgrid" + String.Join(String.Empty, _lines.Select(line => " " + line.CssText));

        #endregion

        #region Methods

        /// <summary>
        /// Checks the two subgrid axes for equality.
        /// </summary>
        /// <param name="other">The other axis to check against.</param>
        /// <returns>True if both name the same lines, otherwise false.</returns>
        public Boolean Equals(CssSubgridValue other) => CssText == other.CssText;

        /// <summary>
        /// Checks for equality against the given object, which is only equal when it is a subgrid axis
        /// naming the same lines.
        /// </summary>
        /// <param name="obj">The object to check against.</param>
        /// <returns>True if both are equal, otherwise false.</returns>
        public override Boolean Equals(Object obj) => obj is CssSubgridValue other && Equals(other);

        /// <summary>
        /// Gets the hash code of the object.
        /// </summary>
        /// <returns>The computed hash code.</returns>
        public override Int32 GetHashCode() => CssText.GetHashCode();

        Boolean IEquatable<ICssValue>.Equals(ICssValue other) => other is CssSubgridValue value && Equals(value);

        ICssValue ICssValue.Compute(ICssComputeContext context) => this;

        #endregion
    }
}
