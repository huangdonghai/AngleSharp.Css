#nullable disable
namespace AngleSharp.Css.Parser
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Css.Values;
    using AngleSharp.Text;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents extensions to for general CSS compount values.
    /// </summary>
    public static class CompoundParser
    {
        /// <summary>
        /// Parse for a CSS quote value.
        /// </summary>
        public static CssTupleValue ParseQuotes(this StringSource source)
        {
            var quotes = new List<ICssValue>();

            while (!source.IsDone)
            {
                var open = source.ParseString();
                source.SkipSpacesAndComments();
                var close = source.ParseString();
                source.SkipSpacesAndComments();

                if (open == null || close == null)
                {
                    return null;
                }

                quotes.Add(new CssQuoteValue(open, close));
            }

            return new CssTupleValue(quotes.ToArray());
        }

        /// <summary>
        /// Parse for a CSS border image slice.
        /// </summary>
        public static CssBorderImageSliceValue ParseBorderImageSlice(this StringSource source)
        {
            var lengths = new ICssValue[4];
            var filled = false;
            var completed = 0;
            var pos = 0;

            do
            {
                pos = source.Index;

                if (completed < 4)
                {
                    var length = source.ParsePercentOrNumber();

                    if (length.HasValue)
                    {
                        lengths[completed++] = length.Value;
                        source.SkipCurrentAndSpaces();
                    }
                }

                if (!filled)
                {
                    filled = source.IsIdentifier(CssKeywords.Fill);
                    source.SkipSpacesAndComments();
                }
            }
            while (source.Index != pos);

            if (completed > 0 || filled)
            {
                while (completed < 4)
                {
                    lengths[completed++] = CssLengthValue.Auto;
                }

                return new CssBorderImageSliceValue(lengths[0], lengths[1], lengths[2], lengths[3], filled);
            }

            return null;
        }

        /// <summary>
        /// Parse for a CSS background repeat.
        /// </summary>
        public static CssImageRepeatsValue ParseBackgroundRepeat(this StringSource source)
        {
            if (source.IsIdentifier(CssKeywords.RepeatX))
            {
                var h = new CssConstantValue<BackgroundRepeat>(CssKeywords.Repeat, BackgroundRepeat.Repeat);
                var v = new CssConstantValue<BackgroundRepeat>(CssKeywords.NoRepeat, BackgroundRepeat.NoRepeat);
                return new CssImageRepeatsValue(h, v);
            }
            else if (source.IsIdentifier(CssKeywords.RepeatY))
            {
                var h = new CssConstantValue<BackgroundRepeat>(CssKeywords.NoRepeat, BackgroundRepeat.NoRepeat);
                var v = new CssConstantValue<BackgroundRepeat>(CssKeywords.Repeat, BackgroundRepeat.Repeat);
                return new CssImageRepeatsValue(h, v);
            }
            else
            {
                var repeatX = source.ParseConstant(Map.BackgroundRepeats);
                source.SkipSpacesAndComments();
                var repeatY = source.ParseConstant(Map.BackgroundRepeats);

                if (repeatY != null)
                {
                    return new CssImageRepeatsValue(repeatX, repeatY);
                }
                else if (repeatX != null)
                {
                    return new CssImageRepeatsValue(repeatX, repeatX);
                }
            }

            return null;
        }

        /// <summary>
        /// Parse for a CSS image source.
        /// </summary>
        public static ICssImageValue ParseImageSource(this StringSource source)
        {
            var set = source.ParseImageSet();

            if (set != null)
            {
                return set;
            }

            var url = source.ParseUri();

            if (url == null)
            {
                return source.ParseGradient();
            }

            return url;
        }

        /// <summary>
        /// Parse for an image-set() value: one picture named at more than one resolution
        /// (CSS Images 4 §4).
        /// </summary>
        public static CssImageSetValue ParseImageSet(this StringSource source)
        {
            var pos = source.Index;
            var ident = source.ParseIdent();

            if (ident == null || !ident.Isi(FunctionNames.ImageSet) || source.Current != Symbols.RoundBracketOpen)
            {
                source.BackTo(pos);
                return null;
            }

            source.SkipCurrentAndSpaces();
            var options = new List<CssImageSetOption>();

            while (!source.IsDone && source.Current != Symbols.RoundBracketClose)
            {
                var image = source.ParseImageSource() ?? ParseImageSetString(source);

                if (image == null)
                {
                    source.BackTo(pos);
                    return null;
                }

                source.SkipSpacesAndComments();
                var resolution = source.ParseResolution();
                source.SkipSpacesAndComments();
                var type = source.ParseImageSetType();

                options.Add(new CssImageSetOption(image, resolution, type));
                source.SkipSpacesAndComments();

                if (source.Current != Symbols.Comma)
                {
                    break;
                }

                source.SkipCurrentAndSpaces();
            }

            if (options.Count == 0 || source.Current != Symbols.RoundBracketClose)
            {
                source.BackTo(pos);
                return null;
            }

            source.SkipCurrentAndSpaces();
            return new CssImageSetValue(options.ToArray());
        }

        /// <summary>
        /// Reads the option of an image-set that is written as a bare string rather than a url: the grammar
        /// takes a string there, and the string names a picture the same way (CSS Images 4 §4).
        /// </summary>
        private static ICssValue ParseImageSetString(this StringSource source)
        {
            var text = source.ParseString();

            return text is null ? null : new CssStringValue(text);
        }

        /// <summary>
        /// Reads the type() an image-set option may state, as the text it was written with - it tells a
        /// reader what kind of picture the resolution belongs to, and no reader of the value needs more.
        /// </summary>
        private static String ParseImageSetType(this StringSource source)
        {
            var pos = source.Index;
            var ident = source.ParseIdent();

            if (ident == null || !ident.Isi("type") || source.Current != Symbols.RoundBracketOpen)
            {
                source.BackTo(pos);
                return null;
            }

            source.SkipCurrentAndSpaces();
            var mime = source.ParseString();
            source.SkipSpacesAndComments();

            if (mime == null || source.Current != Symbols.RoundBracketClose)
            {
                source.BackTo(pos);
                return null;
            }

            source.SkipCurrentAndSpaces();
            return "type(" + new CssStringValue(mime).CssText + ")";
        }

        /// <summary>
        /// Parse for a generic periodic [u, y, d, x] value.
        /// </summary>
        public static CssPeriodicValue<T> ParsePeriodic<T>(this StringSource source, Func<StringSource, T> converter)
            where T : ICssValue
        {
            var values = new List<T>(4);

            while (values.Count < 4)
            {
                var result = converter.Invoke(source);

                if (result == null)
                    break;

                values.Add(result);
                source.SkipSpacesAndComments();
            }

            return values.Count > 0 ? new CssPeriodicValue<T>(values.ToArray()) : null;
        }
    }
}
