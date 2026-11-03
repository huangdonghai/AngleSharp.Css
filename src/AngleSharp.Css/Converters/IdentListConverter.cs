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
    /// Converts a whitespace-separated list of keywords, each one from the set the caller names: the
    /// keyword itself where there is only one, and a tuple of them where there are more. The order and
    /// the combination rules of the value are not settled here - the engine that reads the declaration
    /// answers those - only whether every word written is one the property takes.
    /// </summary>
    sealed class IdentListConverter : IValueConverter
    {
        private readonly String[] _allowed;

        public IdentListConverter(String[] allowed)
        {
            _allowed = allowed;
        }

        public ICssValue Convert(StringSource source)
        {
            var start = source.Index;
            var items = new List<ICssValue>();

            while (true)
            {
                source.SkipSpacesAndComments();

                if (source.IsDone)
                {
                    break;
                }

                var identifier = source.ParseIdent();

                if (identifier == null || !Allows(identifier))
                {
                    source.BackTo(start);
                    return null;
                }

                items.Add(new CssIdentifierValue(identifier));
            }

            if (items.Count == 0)
            {
                source.BackTo(start);
                return null;
            }

            return items.Count == 1 ? items[0] : new CssTupleValue(items.ToArray());
        }

        private Boolean Allows(String identifier)
        {
            foreach (var allowed in _allowed)
            {
                if (String.Equals(allowed, identifier, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
