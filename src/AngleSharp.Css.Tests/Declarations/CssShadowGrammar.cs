namespace AngleSharp.Css.Tests.Declarations
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Css.Parser;
    using AngleSharp.Css.Values;
    using AngleSharp.Html.Parser;
    using NUnit.Framework;
    using System;

    /// <summary>
    /// The two shadow grammars are not the same one: a text shadow is an offset, an optional blur and a
    /// colour, while a box shadow adds a spread distance and the inset keyword. A declaration that writes
    /// more than its own grammar allows is dropped as a whole - the list does not keep the items that were
    /// well formed - which is what these cover.
    /// </summary>
    [TestFixture]
    public class CssShadowGrammar
    {
        private static String Declared(String property)
        {
            var html = $"<div style=\"{property}\">test</div>";
            var parser = new HtmlParser(new HtmlParserOptions(), BrowsingContext.New(Configuration.Default.WithCss(new CssParserOptions())));
            var div = parser.ParseDocument(html).QuerySelector("div");

            return div.GetStyle().GetPropertyValue(Name(property));
        }

        private static String Name(String property) => property[..property.IndexOf(':')];

        [Test]
        public void TextShadow_TakesThreeLengthsAndAColor() =>
            Assert.AreEqual("2px 2px 2px rgba(0, 0, 0, 1)", Declared("text-shadow: 2px 2px 2px #000"));

        [Test]
        public void TextShadow_LeavesTheColorToTheElement() =>
            Assert.AreEqual("2px 3px", Declared("text-shadow: 2px 3px"));

        [Test]
        public void TextShadow_KeepsEveryItemOfAList() =>
            Assert.AreEqual("1px 1px, 2px 2px rgba(0, 0, 255, 1)", Declared("text-shadow: 1px 1px, 2px 2px blue"));

        [Test]
        public void TextShadow_RejectsASpreadDistance() =>
            Assert.AreEqual(String.Empty, Declared("text-shadow: 1px 2px 3px 4px"));

        [Test]
        public void TextShadow_RejectsTheInsetKeyword() =>
            Assert.AreEqual(String.Empty, Declared("text-shadow: inset 1px 2px 3px"));

        [Test]
        public void TextShadow_RejectsTheInsetKeywordAtTheEnd() =>
            Assert.AreEqual(String.Empty, Declared("text-shadow: 0 0 2px black inset"));

        [Test]
        public void TextShadow_RejectsAShortList() =>
            Assert.AreEqual(String.Empty, Declared("text-shadow: 1px"));

        [Test]
        public void BoxShadow_TakesFourLengths() =>
            Assert.AreEqual("1px 2px 3px 4px", Declared("box-shadow: 1px 2px 3px 4px"));

        [Test]
        public void BoxShadow_TakesTheInsetKeywordAnywhere() =>
            Assert.AreEqual("inset 0 0 4px rgba(0, 0, 255, 1)", Declared("box-shadow: blue inset 0 0 4px"));

        [Test]
        public void BoxShadow_TakesTheCurrentColorKeywordAsItsColor()
        {
            // The keyword is a colour as any other here, and a shadow that names none is drawn in the same
            // ink - which is why both of them have to survive the parse rather than become black.
            var html = "<div style=\"box-shadow: currentcolor 3px 3px\">test</div>";
            var parser = new HtmlParser(new HtmlParserOptions(), BrowsingContext.New(Configuration.Default.WithCss(new CssParserOptions())));
            var property = parser.ParseDocument(html).QuerySelector("div").GetStyle().GetProperty("box-shadow");
            var shadow = (CssShadowValue)((ICssMultipleValue)property!.RawValue!)[0];

            Assert.AreEqual(CssColorValue.CurrentColor, shadow.DeclaredColor);
            Assert.AreEqual("3px 3px currentColor", Declared("box-shadow: currentcolor 3px 3px"));
        }

        [Test]
        public void BoxShadow_KeepsTheAbsentColorApartFromBlack()
        {
            var html = "<div style=\"box-shadow: 3px 3px\">test</div>";
            var parser = new HtmlParser(new HtmlParserOptions(), BrowsingContext.New(Configuration.Default.WithCss(new CssParserOptions())));
            var property = parser.ParseDocument(html).QuerySelector("div").GetStyle().GetProperty("box-shadow");
            var shadow = (CssShadowValue)((ICssMultipleValue)property!.RawValue!)[0];

            Assert.IsNull(shadow.DeclaredColor, "the colour a shadow left out is the element's own ink, not black");
        }

        [Test]
        public void BoxShadow_RejectsAFifthLength() =>
            Assert.AreEqual(String.Empty, Declared("box-shadow: 1px 2px 3px 4px 5px"));

        [Test]
        public void BoxShadow_RejectsTwoInsetKeywords() =>
            Assert.AreEqual(String.Empty, Declared("box-shadow: inset 1px 2px inset"));

        [Test]
        public void BoxShadow_LeavesOutAKeywordThatStandsInTheWayOfTheList() =>
            Assert.AreEqual(String.Empty, Declared("box-shadow: none, 4px 4px"));
    }
}
