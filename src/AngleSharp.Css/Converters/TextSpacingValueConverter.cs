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
    /// Converts the <c>text-spacing</c> shorthand, which writes a value of both its longhands: <c>none</c>
    /// and <c>auto</c> stand for a pair outright, and any other value is a set of keywords, each of which
    /// belongs to one longhand or the other, in either order. A longhand no keyword speaks for is set to
    /// its initial value, which is what the shorthand asks of it.
    /// </summary>
    sealed class TextSpacingValueConverter : IValueConverter
    {
        private static readonly String[] Trim =
        {
            "normal", "auto", "space-all", "trim-all", "trim-both", "trim-start", "space-start",
            "space-first", "trim-adjacent", "space-adjacent", "trim-end", "space-end", "allow-end",
        };

        private static readonly String[] Autospace =
        {
            "normal", "auto", "no-autospace", "ideograph-alpha", "ideograph-numeric", "punctuation",
            "insert", "replace",
        };

        public ICssValue Convert(StringSource source)
        {
            var start = source.Index;
            var words = new List<String>();

            while (!source.IsDone)
            {
                source.SkipSpacesAndComments();

                if (source.IsDone)
                {
                    break;
                }

                var identifier = source.ParseIdent();

                if (identifier == null)
                {
                    source.BackTo(start);
                    return null;
                }

                words.Add(identifier.ToLowerInvariant());
            }

            if (words.Count == 0)
            {
                source.BackTo(start);
                return null;
            }

            if (words.Count == 1 && words[0] == "none")
            {
                return Pair("space-all", "no-autospace");
            }

            if (words.Count == 1 && words[0] == "auto")
            {
                return Pair("auto", "auto");
            }

            var trim = new List<ICssValue>();
            var autospace = new List<ICssValue>();

            foreach (var word in words)
            {
                var isTrim = Contains(Trim, word);
                var isAutospace = Contains(Autospace, word);

                // A word neither longhand takes, or one only the whole value can stand for, makes the
                // declaration one the property does not accept.
                if (isTrim == isAutospace || word == "none")
                {
                    source.BackTo(start);
                    return null;
                }

                (isTrim ? trim : autospace).Add(new CssIdentifierValue(word));
            }

            return new CssTupleValue(new ICssValue[]
            {
                Collapse(trim),
                Collapse(autospace),
            });
        }

        private static ICssValue Pair(String trim, String autospace) =>
            new CssTupleValue(new ICssValue[]
            {
                new CssIdentifierValue(trim),
                new CssIdentifierValue(autospace),
            });

        private static ICssValue Collapse(List<ICssValue> values)
        {
            if (values.Count == 0)
            {
                return new CssIdentifierValue("normal");
            }

            return values.Count == 1 ? values[0] : new CssTupleValue(values.ToArray());
        }

        private static Boolean Contains(String[] allowed, String word)
        {
            foreach (var item in allowed)
            {
                if (String.Equals(item, word, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
