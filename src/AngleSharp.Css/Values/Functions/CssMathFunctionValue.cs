#nullable disable
namespace AngleSharp.Css.Values
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Text;
    using System;
    using System.Linq;

    /// <summary>
    /// Represents a CSS math function: min(), max() or clamp().
    /// </summary>
    /// <remarks>
    /// The arguments are kept as written, since the value of a math function is only known where its
    /// percentages are: <c>max(10%, 100px)</c> stands for one or the other depending on the box it is
    /// resolved against. Computing therefore folds only what needs no basis - the arguments all being
    /// the same unit - and hands the function itself back otherwise, which is also the computed value
    /// CSS Values 4 §10.11 asks for.
    /// </remarks>
    public sealed class CssMathFunctionValue : ICssFunctionValue, IEquatable<CssMathFunctionValue>
    {
        #region Fields

        private readonly String _name;
        private readonly ICssValue[] _arguments;

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new math function call.
        /// </summary>
        /// <param name="name">The name of the function, min, max or clamp.</param>
        /// <param name="arguments">The arguments to take the greatest or smallest of.</param>
        public CssMathFunctionValue(String name, ICssValue[] arguments)
        {
            _name = name;
            _arguments = arguments;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public String Name => _name;

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        public ICssValue[] Arguments => _arguments;

        /// <summary>
        /// Gets the CSS text representation, which is the function written as the page wrote it.
        /// </summary>
        public String CssText => _name.CssFunction(String.Join(", ", _arguments.Select(argument => argument.CssText)));

        #endregion

        #region Methods

        ICssValue ICssValue.Compute(ICssComputeContext context)
        {
            if (_arguments.Length == 0)
            {
                return this;
            }

            var computed = new CssLengthValue[_arguments.Length];

            for (var index = 0; index < _arguments.Length; index++)
            {
                if (_arguments[index].Compute(context) is not CssLengthValue current ||
                    (index > 0 && current.Type != computed[0].Type))
                {
                    return this;
                }

                computed[index] = current;
            }

            if (_name.Equals("clamp", StringComparison.OrdinalIgnoreCase))
            {
                return computed.Length != 3
                    ? this
                    : new CssLengthValue(
                        Math.Clamp(computed[1].Value, computed[0].Value, computed[2].Value),
                        computed[0].Type);
            }

            var smallest = _name.Equals("min", StringComparison.OrdinalIgnoreCase);
            var value = computed[0].Value;

            for (var index = 1; index < computed.Length; index++)
            {
                value = smallest
                    ? Math.Min(value, computed[index].Value)
                    : Math.Max(value, computed[index].Value);
            }

            return new CssLengthValue(value, computed[0].Type);
        }

        Boolean IEquatable<ICssValue>.Equals(ICssValue other) => Object.ReferenceEquals(this, other);

        /// <summary>
        /// Checks if the current value is equal to the provided one.
        /// </summary>
        /// <param name="other">The value to check against.</param>
        /// <returns>True if both are equal, otherwise false.</returns>
        public Boolean Equals(CssMathFunctionValue other) => Object.ReferenceEquals(this, other);

        #endregion
    }
}
