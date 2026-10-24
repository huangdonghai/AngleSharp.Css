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
            // else (CSS Conditional 4 §3.1).
            !HasTopLevelComma(_selector) &&
            _context?.GetService<ICssSelectorParser>()?.ParseSelector(_selector) is not null;

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
