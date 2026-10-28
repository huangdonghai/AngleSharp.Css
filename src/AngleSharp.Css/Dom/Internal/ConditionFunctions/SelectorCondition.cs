namespace AngleSharp.Css.Dom
{
    using AngleSharp.Css.Parser;
    using System;
    using System.IO;

    /// <summary>
    /// The <c>selector()</c> condition of CSS Conditional 4: it asks whether the user agent can read a
    /// selector written as this one is, which is the same question as whether the selector parses - a
    /// selector the parser answers with nothing for is one this engine has no way to match.
    /// </summary>
    sealed class SelectorCondition : IConditionFunction
    {
        private readonly IBrowsingContext _context;
        private readonly String _selector;

        public SelectorCondition(IBrowsingContext context, String selector)
        {
            _context = context;
            _selector = selector;
        }

        public Boolean Check(IRenderDevice device) =>
            // The grammar of the condition takes a selector, not a list of them: `selector(div, div)` is
            // not a question a page may ask, and a parser that answers with a list answered something
            // else (CSS Conditional 4 §3.1). What the parser accepts is then the answer: a selector it
            // reads is one this engine has a way to match, and the pseudo-elements it names are the
            // parser's to know - the same set every rule of a page is read against.
            !HasTopLevelComma(_selector) &&
            _context?.GetService<ICssSelectorParser>()?.ParseSelector(_selector) is not null;

        /// <summary>
        /// Gets whether every pseudo-class and pseudo-element the selector names is one the engine
        /// knows. The parser drops what it cannot read from a forgiving list - `:is(:foo, .a)` matches
        /// `.a` - but the condition asks about the whole selector, and a name the engine does not know
        /// is not one it can read (CSS Conditional 4 §3.1, measured against Chrome).
        /// </summary>
        private Boolean NamesOnlyKnownPseudo()
        {
            for (var i = 0; i < _selector.Length; i++)
            {
                var c = _selector[i];

                if (c is '"' or '\'')
                {
                    i = SkipString(i);
                    continue;
                }

                if (c == '[')
                {
                    i = SkipAttribute(i);
                    continue;
                }

                if (c != ':')
                {
                    continue;
                }

                var at = i + 1;
                var element = at < _selector.Length && _selector[at] == ':';

                if (element)
                {
                    at++;
                }

                var start = at;

                while (at < _selector.Length &&
                    (Char.IsLetterOrDigit(_selector[at]) || _selector[at] is '-' or '_'))
                {
                    at++;
                }

                if (at == start)
                {
                    continue;
                }

                var name = _selector.Substring(start, at - start);
                var classes = _context?.GetService<IPseudoClassSelectorFactory>();
                var elements = _context?.GetService<IPseudoElementSelectorFactory>();
                var known = !element && classes?.Create(name) is { } pseudoClass
                    ? pseudoClass
                    : elements?.Create(name);

                // A vendor pseudo-element may be written with one colon, so a name that starts with a
                // dash is asked of the pseudo-element factory as well.
                if (known is null && !element && name.StartsWith("-", StringComparison.Ordinal))
                {
                    known = elements?.Create(name);
                }

                if (known is null)
                {
                    return false;
                }

                i = at - 1;
            }

            return true;
        }

        /// <summary>Steps past a quoted run, returning the index of its closing quote.</summary>
        private Int32 SkipString(Int32 at)
        {
            var quote = _selector[at];

            for (at++; at < _selector.Length; at++)
            {
                if (_selector[at] == '\\')
                {
                    at++;
                    continue;
                }

                if (_selector[at] == quote)
                {
                    return at;
                }
            }

            return _selector.Length;
        }

        /// <summary>Steps past an attribute selector, quotes included.</summary>
        private Int32 SkipAttribute(Int32 at)
        {
            for (at++; at < _selector.Length; at++)
            {
                if (_selector[at] is '"' or '\'')
                {
                    at = SkipString(at);
                }
                else if (_selector[at] == ']')
                {
                    return at;
                }
            }

            return _selector.Length;
        }

        /// <summary>Gets whether a comma stands outside every bracket, which makes the argument a list.</summary>
        private static Boolean HasTopLevelComma(String selector)
        {
            var depth = 0;

            foreach (var c in selector)
            {
                if (c is '(' or '[')
                {
                    depth++;
                }
                else if (c is ')' or ']')
                {
                    depth--;
                }
                else if (c == ',' && depth == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public void ToCss(TextWriter writer, IStyleFormatter formatter) => writer.Write("selector(" + _selector + ")");
    }
}
