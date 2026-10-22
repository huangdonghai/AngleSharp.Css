#nullable disable
namespace AngleSharp.Css.Values
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Text;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents an image-set() value: one picture named at more than one resolution, from which a
    /// reader picks the candidate that suits what it draws with (CSS Images 4 §4).
    /// </summary>
    public sealed class CssImageSetValue : ICssImageValue, IEquatable<CssImageSetValue>
    {
        #region Fields

        private readonly CssImageSetOption[] _options;

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new image-set value.
        /// </summary>
        /// <param name="options">The candidates the set names.</param>
        public CssImageSetValue(CssImageSetOption[] options)
        {
            _options = options;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public String Name => FunctionNames.ImageSet;

        /// <summary>
        /// Gets the arguments used for calling the function.
        /// </summary>
        public ICssValue[] Arguments => _options.Cast<ICssValue>().ToArray();

        /// <summary>
        /// Gets the CSS text representation.
        /// </summary>
        public String CssText => Name.CssFunction(String.Join(", ", _options.Select(m => m.CssText)));

        /// <summary>
        /// Gets the candidates the set names, in the order the page wrote them.
        /// </summary>
        public CssImageSetOption[] Options => _options;

        #endregion

        #region Methods

        /// <summary>
        /// Checks if the current value is equal to the provided one.
        /// </summary>
        /// <param name="other">The value to check against.</param>
        /// <returns>True if both are equal, otherwise false.</returns>
        public Boolean Equals(CssImageSetValue other)
        {
            if (other is not null && _options.Length == other._options.Length)
            {
                for (var i = 0; i < _options.Length; i++)
                {
                    if (!_options[i].Equals(other._options[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        Boolean IEquatable<ICssValue>.Equals(ICssValue other) => other is CssImageSetValue value && Equals(value);

        ICssValue ICssValue.Compute(ICssComputeContext context) =>
            new CssImageSetValue(_options.Select(m => (CssImageSetOption)((ICssValue)m).Compute(context)).ToArray());

        #endregion
    }

    /// <summary>
    /// One candidate of an image-set: the picture, the resolution it suits and the type it is written as.
    /// </summary>
    public sealed class CssImageSetOption : ICssValue, IEquatable<CssImageSetOption>
    {
        #region Fields

        private readonly ICssValue _image;
        private readonly ICssValue _resolution;
        private readonly String _type;

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new candidate.
        /// </summary>
        /// <param name="image">The picture, or the string that names one.</param>
        /// <param name="resolution">The resolution it suits, or null where the page stated none.</param>
        /// <param name="type">The type() it states as written, or null where it states none.</param>
        public CssImageSetOption(ICssValue image, ICssValue resolution = null, String type = null)
        {
            _image = image;
            _resolution = resolution;
            _type = type;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the picture this candidate names.
        /// </summary>
        public ICssValue Image => _image;

        /// <summary>
        /// Gets the resolution the picture suits, or null where the page stated none.
        /// </summary>
        public ICssValue Resolution => _resolution;

        /// <summary>
        /// Gets the type the candidate states, as it was written, or null where it states none.
        /// </summary>
        public String Type => _type;

        /// <summary>
        /// Gets the CSS text representation.
        /// </summary>
        public String CssText
        {
            get
            {
                var text = _image.CssText;

                if (_resolution != null)
                {
                    text += " " + _resolution.CssText;
                }

                if (_type != null)
                {
                    text += " " + _type;
                }

                return text;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks if the current value is equal to the provided one.
        /// </summary>
        /// <param name="other">The value to check against.</param>
        /// <returns>True if both are equal, otherwise false.</returns>
        public Boolean Equals(CssImageSetOption other)
        {
            if (other is null)
            {
                return false;
            }

            var comparer = EqualityComparer<ICssValue>.Default;

            return comparer.Equals(_image, other._image) &&
                comparer.Equals(_resolution, other._resolution) &&
                _type == other._type;
        }

        Boolean IEquatable<ICssValue>.Equals(ICssValue other) => other is CssImageSetOption value && Equals(value);

        ICssValue ICssValue.Compute(ICssComputeContext context) =>
            new CssImageSetOption(_image.Compute(context), _resolution?.Compute(context), _type);

        #endregion
    }
}
