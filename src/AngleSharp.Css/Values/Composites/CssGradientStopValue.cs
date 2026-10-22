#nullable disable
namespace AngleSharp.Css.Values
{
    using AngleSharp.Css.Dom;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// More information can be found at the W3C:
    /// http://dev.w3.org/csswg/css-images-3/#color-stop-syntax
    /// </summary>
    public sealed class CssGradientStopValue : ICssCompositeValue, IEquatable<CssGradientStopValue>
    {
        #region Fields

        private readonly CssColorValue _color;
        private readonly ICssValue _location;
        private readonly Boolean _isHint;

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new gradient stop.
        /// </summary>
        /// <param name="color">The color of the stop.</param>
        /// <param name="location">The location of the stop, if any.</param>
        public CssGradientStopValue(CssColorValue color, ICssValue location = null)
        {
            _color = color;
            _location = location;
        }

        /// <summary>
        /// Creates an interpolation hint: the place where the ramp between the two colours on either side
        /// of it reaches their midpoint, which states no colour of its own (CSS Images 3 §3.4).
        /// </summary>
        /// <param name="location">Where the hint stands along the line.</param>
        public CssGradientStopValue(ICssValue location)
        {
            _location = location;
            _isHint = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the color of the gradient stop.
        /// </summary>
        public CssColorValue Color => _color;

        /// <summary>
        /// Gets if this is an interpolation hint rather than a colour stop, which leaves the colour to
        /// the two stops it stands between.
        /// </summary>
        public Boolean IsHint => _isHint;

        /// <summary>
        /// Gets if the position is determined.
        /// </summary>
        public Boolean IsDetermined => _location != null;

        /// <summary>
        /// Gets if the position is undetermined.
        /// </summary>
        public Boolean IsUndetermined => _location == null;

        /// <summary>
        /// Gets the location of the gradient stop.
        /// </summary>
        public ICssValue Location => _location;

        /// <summary>
        /// Gets the CSS text representation.
        /// </summary>
        public String CssText => IsHint
            ? _location.CssText
            : IsDetermined ? String.Concat(_color.CssText, " ", _location.CssText) : _color.CssText;

        #endregion

        #region Methods

        /// <summary>
        /// Checks if the current value is equal to the provided one.
        /// </summary>
        /// <param name="other">The value to check against.</param>
        /// <returns>True if both are equal, otherwise false.</returns>
        public Boolean Equals(CssGradientStopValue other)
        {
            if (other is not null)
            {
                var comparer = EqualityComparer<ICssValue>.Default;
                return _isHint == other._isHint && comparer.Equals(_color, other._color) && comparer.Equals(_location, other._location);
            }

            return false;
        }

        ICssValue ICssValue.Compute(ICssComputeContext context)
        {
            var location = _location?.Compute(context);

            if (_isHint)
            {
                return new CssGradientStopValue(location);
            }

            var color = ((ICssValue)_color).Compute(context);
            return new CssGradientStopValue((CssColorValue)color, location);
        }

        Boolean IEquatable<ICssValue>.Equals(ICssValue other) => other is CssGradientStopValue value && Equals(value);

        #endregion
    }
}
