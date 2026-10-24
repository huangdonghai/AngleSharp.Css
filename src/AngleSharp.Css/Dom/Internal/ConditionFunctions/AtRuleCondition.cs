namespace AngleSharp.Css.Dom
{
    using AngleSharp.Css.Parser;
    using AngleSharp.Text;
    using System;
    using System.IO;

    /// <summary>
    /// The <c>at-rule()</c> condition of CSS Conditional 4: it asks whether the user agent can read an
    /// at-rule of that name, which is the same question as whether the parser has a rule for it.
    /// </summary>
    sealed class AtRuleCondition : IConditionFunction
    {
        private readonly String _name;
        private readonly Boolean _known;

        public AtRuleCondition(String name)
        {
            _name = name;
            _known = CssBuilder.KnowsAtRule(Unescape(name));
        }

        public Boolean Check(IRenderDevice device) => _known;

        public void ToCss(TextWriter writer, IStyleFormatter formatter) => writer.Write("@supports at-rule(" + _name + ")");

        /// <summary>
        /// Gets the name the argument spells: the grammar has it written as an at-keyword, and a name may
        /// be written with a CSS escape - <c>@support;</c> is <c>@supports</c> - so the tokenizer is asked
        /// for the identifier rather than the text read as it stands.
        /// </summary>
        private static String Unescape(String argument)
        {
            var text = argument.Trim();
            var sigil = text.StartsWith("@", StringComparison.Ordinal);
            var ident = sigil ? text.Substring(1) : text;
            var name = new StringSource(ident).ParseIdent() ?? ident;

            // The rule map is keyed by the at-keyword the tokenizer hands over, which keeps its `@`.
            return sigil ? "@" + name : name;
        }
    }
}
