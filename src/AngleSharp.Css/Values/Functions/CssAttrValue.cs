#nullable disable
namespace AngleSharp.Css.Values
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Text;
    using System;

    /// <summary>
    /// Represents a CSS attr function call.
    /// </summary>
    public sealed class CssAttrValue : ICssFunctionValue, IEquatable<CssAttrValue>
    {
        #region Fields

        private readonly String _attribute;
        private readonly String _fallback;

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new attr function call.
        /// </summary>
        /// <param name="attribute">The referenced attribute name.</param>
        public CssAttrValue(String attribute)
            : this(attribute, null)
        {
        }

        /// <summary>
        /// Creates a new attr function call that states what to read where the element has no such
        /// attribute.
        /// </summary>
        /// <param name="attribute">The referenced attribute name.</param>
        /// <param name="fallback">The text standing for an attribute that is not there.</param>
        public CssAttrValue(String attribute, String fallback)
        {
            _attribute = attribute;
            _fallback = fallback;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the used attribute.
        /// </summary>
        public String Attribute => _attribute;

        /// <summary>
        /// Gets the text asked for an element that carries no such attribute, or null where the page stated
        /// none.
        /// </summary>
        public String Fallback => _fallback;

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public String Name => FunctionNames.Attr;

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        public ICssValue[] Arguments => _fallback is null ?
            new ICssValue[] { new CssIdentifierValue(_attribute) } :
            new ICssValue[] { new CssIdentifierValue(_attribute), new CssStringValue(_fallback) };

        /// <summary>
        /// Gets the CSS text representation.
        /// </summary>
        public String CssText => Name.CssFunction(_fallback is null ? _attribute : String.Concat(_attribute, ", ", _fallback.CssString()));

        #endregion

        #region Methods

        /// <summary>
        /// Checks if the current value is equal to the provided one.
        /// </summary>
        /// <param name="other">The value to check against.</param>
        /// <returns>True if both are equal, otherwise false.</returns>
        public Boolean Equals(CssAttrValue other) => other is not null &&
            _attribute == other._attribute &&
            _fallback == other._fallback;

        Boolean IEquatable<ICssValue>.Equals(ICssValue other) => other is CssAttrValue value && Equals(value);

        ICssValue ICssValue.Compute(ICssComputeContext context)
        {
            return this;
        }

        #endregion
    }
}
