namespace AngleSharp.Css.Dom
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// The <c>font-format()</c> and <c>font-tech()</c> conditions of CSS Conditional 4 §3.1: they ask
    /// whether the engine can use a font format or a font technology. The argument is one identifier -
    /// a quoted string, a list or a space-separated pair is not a question this answers - and the
    /// answer is the set of formats and technologies the engine reads.
    /// </summary>
    sealed class FontCondition : IConditionFunction
    {
        private static readonly HashSet<String> Formats = new(StringComparer.OrdinalIgnoreCase)
        {
            "opentype",
            "truetype",
            "woff",
            "woff2",
            "collection",
        };

        private static readonly HashSet<String> Technologies = new(StringComparer.OrdinalIgnoreCase)
        {
            "features-opentype",
            "features-truetype",
            "features-graphite",
            "color-COLRv0",
            "color-COLRv1",
            "color-SVG",
            "color-sbix",
            "color-CBDT",
            "variations",
            "palettes",
        };

        private readonly String _function;
        private readonly String _argument;
        private readonly Boolean _supported;

        public FontCondition(String function, String argument)
        {
            _function = function;
            _argument = argument;
            _supported = IsIdentifier(argument) &&
                (function.Equals("font-format", StringComparison.OrdinalIgnoreCase)
                    ? Formats.Contains(argument)
                    : Technologies.Contains(argument));
        }

        public Boolean Check(IRenderDevice device) => _supported;

        public void ToCss(TextWriter writer, IStyleFormatter formatter) =>
            writer.Write(_function + "(" + _argument + ")");

        /// <summary>Gets whether the argument is one name and nothing else.</summary>
        private static Boolean IsIdentifier(String text)
        {
            if (text.Length == 0)
            {
                return false;
            }

            foreach (var c in text)
            {
                if (!(Char.IsLetterOrDigit(c) || c is '-' or '_'))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
