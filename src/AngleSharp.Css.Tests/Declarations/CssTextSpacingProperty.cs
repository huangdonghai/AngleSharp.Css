namespace AngleSharp.Css.Tests.Declarations
{
    using NUnit.Framework;
    using static CssConstructionFunctions;

    /// <summary>
    /// Covers the <c>text-spacing</c> shorthand, which writes a value of both <c>text-spacing-trim</c>
    /// and <c>text-autospace</c>: the words <c>none</c> and <c>auto</c> stand for a pair of them, and
    /// any other value names the longhand each of its keywords belongs to.
    /// </summary>
    [TestFixture]
    public class CssTextSpacingPropertyTests
    {
        [Test]
        public void CssTextSpacingNoneSetsBothLonghands()
        {
            var style = ParseDeclarations("text-spacing: none");
            Assert.AreEqual("space-all", style.GetPropertyValue("text-spacing-trim"));
            Assert.AreEqual("no-autospace", style.GetPropertyValue("text-autospace"));
        }

        [Test]
        public void CssTextSpacingAutoSetsBothLonghands()
        {
            var style = ParseDeclarations("text-spacing: auto");
            Assert.AreEqual("auto", style.GetPropertyValue("text-spacing-trim"));
            Assert.AreEqual("auto", style.GetPropertyValue("text-autospace"));
        }

        [Test]
        public void CssTextSpacingOneKeywordSetsTheLonghandItBelongsTo()
        {
            var trim = ParseDeclarations("text-spacing: trim-all");
            Assert.AreEqual("trim-all", trim.GetPropertyValue("text-spacing-trim"));
            Assert.AreEqual("normal", trim.GetPropertyValue("text-autospace"));

            var autospace = ParseDeclarations("text-spacing: no-autospace");
            Assert.AreEqual("normal", autospace.GetPropertyValue("text-spacing-trim"));
            Assert.AreEqual("no-autospace", autospace.GetPropertyValue("text-autospace"));
        }

        [Test]
        public void CssTextSpacingNamesALonghandInEitherOrder()
        {
            var first = ParseDeclarations("text-spacing: trim-start ideograph-alpha");
            Assert.AreEqual("trim-start", first.GetPropertyValue("text-spacing-trim"));
            Assert.AreEqual("ideograph-alpha", first.GetPropertyValue("text-autospace"));

            var second = ParseDeclarations("text-spacing: ideograph-numeric trim-both");
            Assert.AreEqual("trim-both", second.GetPropertyValue("text-spacing-trim"));
            Assert.AreEqual("ideograph-numeric", second.GetPropertyValue("text-autospace"));
        }

        [Test]
        public void CssTextSpacingAWordNoLonghandTakesIsInvalid()
        {
            var style = ParseDeclarations("text-spacing: trim-all sideways");
            Assert.AreEqual("", style.GetPropertyValue("text-spacing-trim"));
            Assert.AreEqual("", style.GetPropertyValue("text-autospace"));
        }

        [Test]
        public void CssTextSpacingALonghandWrittenAfterItWins()
        {
            var style = ParseDeclarations("text-spacing: space-all; text-spacing-trim: trim-all");
            Assert.AreEqual("trim-all", style.GetPropertyValue("text-spacing-trim"));
            Assert.AreEqual("normal", style.GetPropertyValue("text-autospace"));
        }
    }
}
