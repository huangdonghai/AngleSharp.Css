#nullable disable
namespace AngleSharp.Css.Values
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Text;
    using System;

    /// <summary>
    /// Represents a CSS calculated value.
    /// </summary>
    public sealed class CssCalcValue : ICssRawValue, ICssFunctionValue
    {
        #region Fields

        private readonly ICssValue _expression;

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new calc function.
        /// </summary>
        /// <param name="expression">The argument to use.</param>
        public CssCalcValue(ICssValue expression)
        {
            _expression = expression;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public String Name => FunctionNames.Calc;

        /// <summary>
        /// Gets the raw value.
        /// </summary>
        public String Value => _expression.CssText;

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        public ICssValue[] Arguments => new[] { _expression };

        /// <summary>
        /// Gets the argument of the calc function.
        /// </summary>
        public ICssValue Expression => _expression;

        /// <summary>
        /// Gets the CSS text representation. A math function stands for itself: the page wrote
        /// max(100px, 50px), not a calc of it, and a reader of the value must see what was written.
        /// </summary>
        public String CssText => _expression is CssMathFunctionValue
            ? _expression.CssText
            : Name.CssFunction(_expression.CssText);

        #endregion

        #region Methods

        ICssValue ICssValue.Compute(ICssComputeContext context) => _expression.Compute(context);

        Boolean IEquatable<ICssValue>.Equals(ICssValue other) => Object.ReferenceEquals(this, other);

        #endregion
    }
}
