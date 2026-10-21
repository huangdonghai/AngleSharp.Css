#nullable disable
namespace AngleSharp.Css.Parser
{
    using AngleSharp.Css.Values;
    using AngleSharp.Text;
    using System;

    /// <summary>
    /// Represents extensions to for shape values.
    /// </summary>
    public static class ShapeParser
    {
        /// <summary>
        /// Parses the shape value.
        /// </summary>
        public static CssShapeValue ParseShape(this StringSource source)
        {
            var pos = source.Index;

            do
            {
                if (source.IsFunction(FunctionNames.Rect))
                {
                    var top = source.ParseLengthOrAuto();
                    var isc = SkipIfComma(source);
                    var right = source.ParseLengthOrAuto();

                    if (SkipIfComma(source) != isc)
                        break;

                    var bottom = source.ParseLengthOrAuto();

                    if (SkipIfComma(source) != isc)
                        break;

                    var left = source.ParseLengthOrAuto();
                    var f = source.SkipSpacesAndComments();

                    if (top.HasValue && right.HasValue && bottom.HasValue && left.HasValue && f == Symbols.RoundBracketClose)
                    {
                        source.SkipCurrentAndSpaces();
                        return new CssShapeValue(top.Value, right.Value, bottom.Value, left.Value);
                    }
                }
            }
            while (false);

            source.BackTo(pos);
            return null;
        }

        /// <summary>
        /// Parses one side of a rectangle, which is a length or the <c>auto</c> keyword. The keyword
        /// stands for the corresponding edge of the box - the top or left edge for <c>top</c> and
        /// <c>left</c>, the far edge for the other two - so it has to survive the parse: a shape that
        /// says <c>auto</c> is a clip, and no shape at all is not.
        /// </summary>
        private static CssLengthValue? ParseLengthOrAuto(this StringSource source)
        {
            if (source.IsIdentifier(CssKeywords.Auto))
            {
                source.SkipSpacesAndComments();
                return CssLengthValue.Auto;
            }

            return source.ParseLength();
        }

        private static Boolean SkipIfComma(StringSource source)
        {
            if (source.SkipSpacesAndComments() == Symbols.Comma)
            {
                source.SkipCurrentAndSpaces();
                return true;
            }

            return false;
        }
    }
}
