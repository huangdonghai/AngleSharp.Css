#nullable disable
namespace AngleSharp.Css.Parser
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Css.Values;
    using AngleSharp.Text;
    using System;

    /// <summary>
    /// Represents extensions to for shadow values.
    /// </summary>
    public static class ShadowParser
    {
        /// <summary>
        /// Parses the CSS shadow value.
        /// </summary>
        public static CssShadowValue ParseShadow(this StringSource source) =>
            source.ParseShadow(allowInset: true, allowSpread: true);

        /// <summary>
        /// Parses the value of a text-shadow, which is a box shadow with less in it: the grammar of
        /// CSS Text Decoration 4 &amp;sect;4.1 gives it an offset, an optional blur and a colour, and
        /// neither the spread distance nor the <c>inset</c> keyword that <c>box-shadow</c> takes.
        /// </summary>
        public static CssShadowValue ParseTextShadow(this StringSource source) =>
            source.ParseShadow(allowInset: false, allowSpread: false);

        private static CssShadowValue ParseShadow(this StringSource source, Boolean allowInset, Boolean allowSpread)
        {
            var start = source.Index;
            var inset = false;
            var offsetX = default(ICssValue);
            var offsetY = default(ICssValue);
            var blurRadius = default(ICssValue);
            var spreadRadius = default(ICssValue);
            var color = default(CssColorValue?);
            var pos = start;

            do
            {
                pos = source.Index;

                if (allowInset && !inset)
                {
                    inset = source.IsIdentifier(CssKeywords.Inset);
                    source.SkipSpacesAndComments();
                }

                if (offsetX == null)
                {
                    offsetX = source.ParseLengthOrCalc();
                    source.SkipSpacesAndComments();
                }

                if (offsetY == null)
                {
                    offsetY = source.ParseLengthOrCalc();
                    source.SkipSpacesAndComments();
                }

                if (blurRadius == null)
                {
                    blurRadius = source.ParseLengthOrCalc();
                    source.SkipSpacesAndComments();
                }

                if (allowSpread && spreadRadius == null)
                {
                    spreadRadius = source.ParseLengthOrCalc();
                    source.SkipSpacesAndComments();
                }

                if (!color.HasValue)
                {
                    // A shadow's colour is any colour, and `currentcolor` is one of them: it is the ink of
                    // the element the shadow is drawn with, which is what the declaration of a shadow that
                    // names no colour asks for (CSS Backgrounds 3 §6.1).
                    color = source.ParseCurrentColor();
                    source.SkipSpacesAndComments();
                }
            }
            while (pos != source.Index);

            if (offsetX != null && offsetY != null)
            {
                return new CssShadowValue(
                    inset,
                    offsetX,
                    offsetY,
                    blurRadius,
                    spreadRadius,
                    color);
            }

            source.BackTo(start);
            return null;
        }
    }
}
