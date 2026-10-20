#nullable disable
namespace AngleSharp.Css.Converters
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Css.Parser;
    using AngleSharp.Css.Values;
    using AngleSharp.Text;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Parses the <c>repeat()</c> notation a list-valued property takes: <c>repeat(2, red, blue)</c> or
    /// <c>repeat(auto, red)</c>.
    /// </summary>
    /// <remarks>
    /// The value inside the notation is a comma-separated list of the property's own values. It is read
    /// here rather than by the list converter of the whole property, because that one reads to the end of
    /// the declaration and this list ends at the closing bracket. The count is left as the page wrote it -
    /// an integer or <c>auto</c> - for the reader to give a meaning to: how many gaps there are is only
    /// known once the container holding them is laid out.
    /// </remarks>
    sealed class RepeatValueConverter : IValueConverter
    {
        private readonly IValueConverter _count;
        private readonly IValueConverter _value;

        public RepeatValueConverter(IValueConverter count, IValueConverter value)
        {
            _count = count;
            _value = value;
        }

        public ICssValue Convert(StringSource source)
        {
            var start = source.Index;
            source.SkipSpacesAndComments();

            var ident = source.ParseIdent();

            if (ident != null && ident.Isi(FunctionNames.Repeat) && source.Current == Symbols.RoundBracketOpen)
            {
                source.SkipCurrentAndSpaces();
                var count = _count.Convert(source);
                var comma = source.SkipSpaces();

                if (count != null && comma == Symbols.Comma)
                {
                    source.SkipCurrentAndSpaces();

                    if (Values(source) is { Count: > 0 } values)
                    {
                        var close = source.SkipSpacesAndComments();

                        if (close == Symbols.RoundBracketClose)
                        {
                            source.Next();
                            return new CssRepeatValue(count, new CssListValue(values.ToArray()));
                        }
                    }
                }
            }

            source.BackTo(start);
            return null;
        }

        private List<ICssValue> Values(StringSource source)
        {
            var values = new List<ICssValue>();

            while (true)
            {
                var value = _value.Convert(source);

                if (value == null)
                {
                    return null;
                }

                values.Add(value);

                if (source.SkipSpacesAndComments() != Symbols.Comma)
                {
                    return values;
                }

                source.SkipCurrentAndSpaces();
            }
        }
    }
}
