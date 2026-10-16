#nullable disable
namespace AngleSharp.Css.Tests.Styling
{
    using AngleSharp.Dom;
    using NUnit.Framework;
    using static CssConstructionFunctions;

    /// <summary>
    /// `gap`/`grid-gap`'s two-value form is `&lt;row-gap&gt; &lt;column-gap&gt;` per
    /// https://drafts.csswg.org/css-align-3/#gap-shorthand - `GapDeclaration`/`GridGapDeclaration`'s
    /// own `Longhands` arrays listed `column-gap` before `row-gap`, the reverse of what their own
    /// `Merge()`/`Split()` methods already assumed (`values[0]`/`Items[0]` is always the row value),
    /// so the two longhands' computed values came out swapped. Found while adding CSS Grid track
    /// sizing support to a downstream renderer.
    /// </summary>
    [TestFixture]
    public class GapShorthandComputedStyleTests
    {
        [Test]
        public void GapShorthandDecomposesRowFirstColumnSecond()
        {
            var document = ParseDocument("<div id=target style=\"gap: 10px 20px;\"></div>");
            var target = document.GetElementById("target");
            var style = target.ComputeCurrentStyle();

            Assert.AreEqual("10px", style.GetPropertyValue("row-gap"));
            Assert.AreEqual("20px", style.GetPropertyValue("column-gap"));
        }

        [Test]
        public void GridGapShorthandDecomposesRowFirstColumnSecond()
        {
            var document = ParseDocument("<div id=target style=\"grid-gap: 10px 20px;\"></div>");
            var target = document.GetElementById("target");
            var style = target.ComputeCurrentStyle();

            Assert.AreEqual("10px", style.GetPropertyValue("grid-row-gap"));
            Assert.AreEqual("20px", style.GetPropertyValue("grid-column-gap"));
        }

        /// <summary>
        /// `gap: 10px` is `row-gap: 10px; column-gap: 10px` - css-gaps-1 §2.2: "If &lt;'column-gap'&gt;
        /// is omitted, it's set to the same value as &lt;'row-gap'&gt;." The column longhand used to come
        /// out of the split holding no value at all, which is exactly what a lone `row-gap` leaves
        /// behind as well: a renderer reading the longhands could not tell the two apart, and had to
        /// answer the column axis with the row's value - which then gave a page that set only
        /// `column-gap` a row gap nobody had asked for.
        /// </summary>
        [Test]
        public void GapShorthandWithOneValueSetsBothAxes()
        {
            var document = ParseDocument("<div id=target style=\"gap: 10px;\"></div>");
            var target = document.GetElementById("target");
            var style = target.ComputeCurrentStyle();

            Assert.AreEqual("10px", style.GetPropertyValue("row-gap"));
            Assert.AreEqual("10px", style.GetPropertyValue("column-gap"));

            // The same read the renderer's fused path makes: the longhand out of the shorthand
            // declaration the cascade holds, before anything is computed.
            Assert.AreEqual("10px", target.Style.TryGetProperty("row-gap").Value);
            Assert.AreEqual("10px", target.Style.TryGetProperty("column-gap").Value);
        }

        [Test]
        public void GridGapShorthandWithOneValueSetsBothAxes()
        {
            var document = ParseDocument("<div id=target style=\"grid-gap: 10px;\"></div>");
            var target = document.GetElementById("target");
            var style = target.ComputeCurrentStyle();

            Assert.AreEqual("10px", style.GetPropertyValue("grid-row-gap"));
            Assert.AreEqual("10px", style.GetPropertyValue("grid-column-gap"));
        }

        /// <summary>
        /// A lone longhand leaves the other axis out rather than carrying it along: only the
        /// <c>gap</c> shorthand sets both, and its omission rule is the one above.
        /// </summary>
        [Test]
        public void ALoneLonghandLeavesTheOtherAxisUnset()
        {
            var document = ParseDocument("<div id=target style=\"column-gap: 24px;\"></div>");
            var target = document.GetElementById("target");

            Assert.IsNull(target.Style.TryGetProperty("row-gap"));
            Assert.AreEqual("24px", target.Style.TryGetProperty("column-gap").Value);
        }
    }
}
