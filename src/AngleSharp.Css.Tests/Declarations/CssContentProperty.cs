namespace AngleSharp.Css.Tests.Declarations
{
    using AngleSharp.Css.Dom;
    using NUnit.Framework;
    using static CssConstructionFunctions;

    [TestFixture]
    public class CssContentPropertyTests
    {
        [Test]
        public void CssContentParseStringWithDoubleQuoteEscape()
        {
            var source = "a{content:\"\\\"\"}";
            var parsed = ParseStyle(source);
            Assert.AreEqual("\"\\\"\"", parsed.Style.GetContent());
        }

        [Test]
        public void CssContentParseStringWithSingleQuoteEscape()
        {
            var source = "a{content:'\\''}";
            var parsed = ParseStyle(source);
            Assert.AreEqual("\"'\"", parsed.Style.GetContent());
        }

        [Test]
        public void CssContentParseStringWithDoubleQuoteMultipleEscapes()
        {
            var source = "a{content:\"abc\\\"\\\"d\\\"ef\"}";
            var parsed = ParseStyle(source);
            Assert.AreEqual("\"abc\\\"\\\"d\\\"ef\"", parsed.Style.GetContent());
        }

        [Test]
        public void CssContentParseStringWithSingleQuoteMultipleEscapes()
        {
            var source = "a{content:'abc\\'\\'d\\'ef'}";
            var parsed = ParseStyle(source);
            Assert.AreEqual("\"abc''d'ef\"", parsed.Style.GetContent());
        }

        [Test]
        public void CssContentWithCounter_Issue58()
        {
            var source = "a{content: counter(h1) \".\\00A0\"}";
            var parsed = ParseStyle(source);
            Assert.AreEqual("counter(h1) \". \"", parsed.Style.GetContent());
        }

        [Test]
        public void CssContentWithAttributeAndFallbackLegal()
        {
            // Content 3 §10.5 lets the page state the text standing for an attribute it has not got.
            var source = "a{content: attr(icon, 'none')}";
            var parsed = ParseStyle(source);
            Assert.AreEqual("attr(icon, \"none\")", parsed.Style.GetContent());
        }

        [Test]
        public void CssContentWithAlternativeTextLegal()
        {
            // Content 3 §1.2: what follows the slash is spoken rather than drawn, and is part of the value.
            var source = "a{content: \"A\" / \"spoken instead\"}";
            var parsed = ParseStyle(source);
            Assert.AreEqual("\"A\" / \"spoken instead\"", parsed.Style.GetContent());
        }

        [Test]
        public void CssContentWithAlternativeTextAfterCounterLegal()
        {
            var source = "a{content: counter(h1) \".\" / \"number\"}";
            var parsed = ParseStyle(source);
            Assert.AreEqual("counter(h1) \".\" / \"number\"", parsed.Style.GetContent());
        }

        [Test]
        public void CssContentWithAlternativeThatIsNotAStringInvalid()
        {
            var source = "a{content: \"A\" / identifier}";
            var parsed = ParseStyle(source);
            Assert.IsEmpty(parsed.Style.GetContent());
        }

        [Test]
        public void CssCountersSerializeTheirSeparatorAsAString()
        {
            // The separator is a string, and a serialization that drops the quotes and the comma between
            // the arguments cannot be read back as the value it came from.
            var source = "a{content: counters(li, '-')}";
            var parsed = ParseStyle(source);
            Assert.AreEqual("counters(li, \"-\")", parsed.Style.GetContent());
        }

        [Test]
        public void CssCountersWithAStyleSerializeEveryArgument()
        {
            var source = "a{content: counters(li, '.', lower-roman)}";
            var parsed = ParseStyle(source);
            Assert.AreEqual("counters(li, \".\", lower-roman)", parsed.Style.GetContent());
        }

        [Test]
        public void CssCounterWithANonDecimalStyleStillUsesTheComma()
        {
            var source = "a{content: counter(li, upper-alpha)}";
            var parsed = ParseStyle(source);
            Assert.AreEqual("counter(li, upper-alpha)", parsed.Style.GetContent());
        }

    }
}
