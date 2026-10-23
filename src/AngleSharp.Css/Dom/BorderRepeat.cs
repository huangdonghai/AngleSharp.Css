namespace AngleSharp.Css.Dom
{
    /// <summary>
    /// The enumeration over all possible border repeat values.
    /// </summary>
    public enum BorderRepeat : byte
    {
        /// <summary>
        /// Keyword indicating that the image must be stretched to fill
        /// the gap between the two borders.
        /// </summary>
        Stretch,
        /// <summary>
        /// Keyword indicating that the image must be repeated until it
        /// fills the gap between the two borders.
        /// </summary>
        Repeat,
        /// <summary>
        /// Keyword indicating that the image must be repeated until it
        /// fills the gap between the two borders. If the image doesn't fit
        /// after being repeated an integral number of times, the image is
        /// rescaled to fit.
        /// </summary>
        Round,
        /// <summary>
        /// Keyword indicating that the image must be repeated at its own size, with the space left over
        /// shared equally between the copies (CSS Backgrounds 3 §6.4).
        /// </summary>
        Space
    }
}
