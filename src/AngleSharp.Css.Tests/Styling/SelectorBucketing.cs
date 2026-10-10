#nullable disable
namespace AngleSharp.Css.Tests.Styling
{
    using AngleSharp.Css;
    using AngleSharp.Css.Dom;
    using AngleSharp.Css.RenderTree;
    using AngleSharp.Dom;
    using AngleSharp.Html.Dom;
    using NUnit.Framework;
    using System;
    using System.Linq;
    using System.Text;
    using static CssConstructionFunctions;

    /// <summary>
    /// Covers the rule bucket index: the pruning it performs must never change whether - or in which
    /// order - a rule applies, and the document level cache it lives in must notice every change of
    /// the CSS object model or of the sheets a document exposes.
    /// </summary>
    [TestFixture]
    public class SelectorBucketingTests
    {
        private const string Red = "rgba(255, 0, 0, 1)";
        private const string Green = "rgba(0, 128, 0, 1)";

        #region Bucket keys

        [Test]
        public void EveryMemberOfASelectorListStaysReachable()
        {
            var document = ParseDocument("<!doctype html><head><style>#a, .b, h3 { color: red }</style></head><body><div id=a></div><p class=b></p><h3></h3>");
            var window = document.DefaultView;

            Assert.AreEqual(Red, window.GetComputedStyle(document.QuerySelector("#a")).GetColor());
            Assert.AreEqual(Red, window.GetComputedStyle(document.QuerySelector(".b")).GetColor());
            Assert.AreEqual(Red, window.GetComputedStyle(document.QuerySelector("h3")).GetColor());
        }

        [Test]
        public void ASelectorListWithAnUnkeyedMemberStaysReachable()
        {
            // [data-x] has no essential key, so pruning on the #never member would lose the rule for
            // every element that only satisfies the attribute selector.
            var document = ParseDocument("<!doctype html><head><style>[data-x], #never { color: red }</style></head><body><div data-x></div>");
            var window = document.DefaultView;

            Assert.AreEqual(Red, window.GetComputedStyle(document.QuerySelector("[data-x]")).GetColor());
        }

        [Test]
        public void AttributeOnlySelectorRemainsReachable()
        {
            var document = ParseDocument("<!doctype html><head><style>[data-role=button] { color: red }</style></head><body><div data-role=button></div>");
            var window = document.DefaultView;

            Assert.AreEqual(Red, window.GetComputedStyle(document.QuerySelector("[data-role=button]")).GetColor());
        }

        [Test]
        public void RootPseudoClassSelectorRemainsReachable()
        {
            var document = ParseDocument("<!doctype html><head><style>:root { color: red }</style></head><body></body>");
            var window = document.DefaultView;

            Assert.AreEqual(Red, window.GetComputedStyle(document.DocumentElement).GetColor());
        }

        [Test]
        public void NotPseudoClassDoesNotContributeItsInnerKey()
        {
            // The rule has to be keyed by "div" - the key of the rightmost compound - and must not be
            // keyed by the class inside the :not(), which is required to be *absent*.
            var document = ParseDocument("<!doctype html><head><style>div:not(.skip) { color: red }</style></head><body><div id=one></div><div id=two class=skip></div>");
            var window = document.DefaultView;

            Assert.AreEqual(Red, window.GetComputedStyle(document.GetElementById("one")).GetColor());
            Assert.AreNotEqual(Red, window.GetComputedStyle(document.GetElementById("two")).GetColor());
        }

        [Test]
        public void IsPseudoClassDoesNotContributeItsInnerKey()
        {
            var document = ParseDocument("<!doctype html><head><style>p:is(.x, .y) { color: red }</style></head><body><p class=x></p><p class=y></p><div class=x></div>");
            var window = document.DefaultView;

            Assert.AreEqual(Red, window.GetComputedStyle(document.QuerySelector("p.x")).GetColor());
            Assert.AreEqual(Red, window.GetComputedStyle(document.QuerySelector("p.y")).GetColor());
            Assert.AreNotEqual(Red, window.GetComputedStyle(document.QuerySelector("div.x")).GetColor());
        }

        [Test]
        public void CombinatorsUseTheKeyOfTheRightmostCompound()
        {
            var document = ParseDocument("<!doctype html><head><style>.outer .inner > span { color: red }</style></head><body><div class=outer><div class=inner><span id=inside></span></div></div><span id=outside></span>");
            var window = document.DefaultView;

            Assert.AreEqual(Red, window.GetComputedStyle(document.GetElementById("inside")).GetColor());
            Assert.AreNotEqual(Red, window.GetComputedStyle(document.GetElementById("outside")).GetColor());
        }

        [Test]
        public void NamespacePrefixedSelectorStillMatches()
        {
            var document = ParseDocument("<!doctype html><head><style>*|div { color: red }</style></head><body><div id=target></div>");
            var window = document.DefaultView;

            Assert.AreEqual(Red, window.GetComputedStyle(document.GetElementById("target")).GetColor());
        }

        [Test]
        public void ImplicitlyNestedRuleUsesItsOwnRightmostKey()
        {
            var document = ParseDocument("<!doctype html><head><style>.card { color: red; .title { color: green } }</style></head><body><div class=card><span class=title id=inside></span></div><span class=title id=outside></span>");
            var window = document.DefaultView;

            Assert.AreEqual(Green, window.GetComputedStyle(document.GetElementById("inside")).GetColor());
            Assert.AreNotEqual(Green, window.GetComputedStyle(document.GetElementById("outside")).GetColor());
        }

        [Test]
        public void ExplicitNestingReferenceContributesNoKey()
        {
            var document = ParseDocument("<!doctype html><head><style>.card { color: red; &:hover { color: green } }</style></head><body><div class=card id=target></div>");
            var window = document.DefaultView;
            var target = document.GetElementById("target");

            Assert.AreEqual(Red, window.GetComputedStyle(target).GetColor());

            target.SetPseudoClass("hover");

            Assert.AreEqual(Green, window.GetComputedStyle(target).GetColor());
        }

        [Test]
        public void ForcedPseudoClassMatchesThroughItsClassKey()
        {
            var document = ParseDocument("<!doctype html><head><style>.target:hover { color: red }</style></head><body><div class=target id=target></div>");
            var window = document.DefaultView;
            var target = document.GetElementById("target");

            Assert.AreNotEqual(Red, window.GetComputedStyle(target).GetColor());

            target.SetPseudoClass("hover");

            Assert.AreEqual(Red, window.GetComputedStyle(target).GetColor());

            target.SetPseudoClass("hover", false);

            Assert.AreNotEqual(Red, window.GetComputedStyle(target).GetColor());
        }

        [Test]
        public void EqualSpecificityIsDecidedByDocumentOrderAcrossBuckets()
        {
            var first = ParseDocument("<!doctype html><head><style>.one { color: red } .two { color: green }</style></head><body><div class='one two' id=target></div>");
            var second = ParseDocument("<!doctype html><head><style>.two { color: green } .one { color: red }</style></head><body><div class='one two' id=target></div>");

            Assert.AreEqual(Green, first.DefaultView.GetComputedStyle(first.GetElementById("target")).GetColor());
            Assert.AreEqual(Red, second.DefaultView.GetComputedStyle(second.GetElementById("target")).GetColor());
        }

        [Test]
        public void EqualSpecificityIsDecidedByDocumentOrderBetweenKeyedAndUnkeyed()
        {
            var first = ParseDocument("<!doctype html><head><style>.x { color: red } [class~=x] { color: green }</style></head><body><div class=x id=target></div>");
            var second = ParseDocument("<!doctype html><head><style>[class~=x] { color: green } .x { color: red }</style></head><body><div class=x id=target></div>");

            Assert.AreEqual(Green, first.DefaultView.GetComputedStyle(first.GetElementById("target")).GetColor());
            Assert.AreEqual(Red, second.DefaultView.GetComputedStyle(second.GetElementById("target")).GetColor());
        }

        [Test]
        public void SpecificityBeatsDocumentOrderAcrossBuckets()
        {
            var document = ParseDocument("<!doctype html><head><style>#target { color: red } div { color: green }</style></head><body><div id=target></div>");
            var window = document.DefaultView;

            Assert.AreEqual(Red, window.GetComputedStyle(document.GetElementById("target")).GetColor());
        }

        [Test]
        public void TheBucketIndexPrunesRulesThatCannotMatch()
        {
            var source = new StringBuilder();

            for (var i = 0; i < 200; i++)
            {
                // The key has to be taken from the rightmost compound of the chain, not from the
                // left one (which is the descendant prefix and matches nothing here).
                source.Append(".c-").Append(i).Append(" .child-").Append(i).Append(" { color: red }");
            }

            source.Append("h1, h2, h3, .other { color: blue }");
            source.Append(".target { color: green }");

            var document = ParseDocument("<!doctype html><head><style>" + source + "</style></head><body><div class=target id=target></div>");
            var collection = new StyleCollection(document.StyleSheets.OfType<ICssStyleSheet>(), new DefaultRenderDevice());
            var index = collection.GetBucketIndex();
            var candidates = index.CollectCandidates(document.GetElementById("target"));

            Assert.Less(candidates.Count, index.Rules.Count);
            Assert.AreEqual(1, candidates.Count, "only .target may remain - everything else is keyed to a compound the element cannot satisfy");
            Assert.AreEqual(".target", index.Rules[candidates[0]].SelectorText);
        }

        [Test]
        public void TheStyleCollectionIsReusedAcrossCalls()
        {
            var document = ParseDocument("<!doctype html><head><style>.a { color: red }</style></head><body><div class=a></div>");
            var window = document.DefaultView;
            var device = new DefaultRenderDevice();

            Assert.AreSame(window.GetStyleCollection(device), window.GetStyleCollection(device));
        }

        #endregion

        #region Cache invalidation

        [Test]
        public void DisablingAndEnablingAStyleSheetTakesEffect()
        {
            var document = ParseDocument("<!doctype html><head><style>.a { color: red }</style></head><body><div class=a id=target></div>");
            var window = document.DefaultView;
            var target = document.GetElementById("target");
            var sheet = GetSheet(document);

            Assert.AreEqual(Red, window.GetComputedStyle(target).GetColor());

            sheet.IsDisabled = true;
            Assert.AreNotEqual(Red, window.GetComputedStyle(target).GetColor());

            sheet.IsDisabled = false;
            Assert.AreEqual(Red, window.GetComputedStyle(target).GetColor());
        }

        [Test]
        public void InsertingARuleTakesEffect()
        {
            var document = ParseDocument("<!doctype html><head><style></style></head><body><div class=a id=target></div>");
            var window = document.DefaultView;
            var target = document.GetElementById("target");

            Assert.AreNotEqual(Red, window.GetComputedStyle(target).GetColor());

            GetSheet(document).Insert(".a { color: red }", 0);

            Assert.AreEqual(Red, window.GetComputedStyle(target).GetColor());
        }

        [Test]
        public void RemovingARuleTakesEffect()
        {
            var document = ParseDocument("<!doctype html><head><style>.a { color: red }</style></head><body><div class=a id=target></div>");
            var window = document.DefaultView;
            var target = document.GetElementById("target");

            Assert.AreEqual(Red, window.GetComputedStyle(target).GetColor());

            GetSheet(document).RemoveAt(0);

            Assert.AreNotEqual(Red, window.GetComputedStyle(target).GetColor());
        }

        [Test]
        public void ChangingARuleSelectorTakesEffect()
        {
            var document = ParseDocument("<!doctype html><head><style>.a { color: red }</style></head><body><div class=a id=target></div>");
            var window = document.DefaultView;
            var target = document.GetElementById("target");

            Assert.AreEqual(Red, window.GetComputedStyle(target).GetColor());

            ((CssStyleRule)GetSheet(document).Rules[0]).SelectorText = ".b";

            Assert.AreNotEqual(Red, window.GetComputedStyle(target).GetColor());
        }

        [Test]
        public void ChangingADeclarationTakesEffect()
        {
            var document = ParseDocument("<!doctype html><head><style>.a { color: red }</style></head><body><div class=a id=target></div>");
            var window = document.DefaultView;
            var target = document.GetElementById("target");

            Assert.AreEqual(Red, window.GetComputedStyle(target).GetColor());

            ((ICssStyleRule)GetSheet(document).Rules[0]).Style.SetProperty("color", "green");

            Assert.AreEqual(Green, window.GetComputedStyle(target).GetColor());
        }

        [Test]
        public void ReplacingARuleThroughItsCssTextTakesEffect()
        {
            var document = ParseDocument("<!doctype html><head><style>.a { color: red }</style></head><body><div class='a b' id=target></div>");
            var window = document.DefaultView;
            var target = document.GetElementById("target");

            Assert.AreEqual(Red, window.GetComputedStyle(target).GetColor());

            GetSheet(document).Rules[0].CssText = ".b { color: green }";

            Assert.AreEqual(Green, window.GetComputedStyle(target).GetColor());
        }

        [Test]
        public void AppendingAStyleElementTakesEffect()
        {
            var document = ParseDocument("<!doctype html><head></head><body><div class=a id=target></div>");
            var window = document.DefaultView;
            var target = document.GetElementById("target");

            Assert.AreNotEqual(Red, window.GetComputedStyle(target).GetColor());

            var style = document.CreateElement("style");
            style.TextContent = ".a { color: red }";
            document.Head.AppendChild(style);

            Assert.AreEqual(Red, window.GetComputedStyle(target).GetColor());
        }

        [Test]
        public void ChangingTheSheetMediaTakesEffect()
        {
            var document = ParseDocument("<!doctype html><head><style>@media screen { .a { color: red } }</style></head><body><div class=a id=target></div>");
            var window = document.DefaultView;
            var target = document.GetElementById("target");

            Assert.AreEqual(Red, window.GetComputedStyle(target).GetColor());

            GetSheet(document).Media.MediaText = "print";

            Assert.AreNotEqual(Red, window.GetComputedStyle(target).GetColor());
        }

        [Test]
        public void ChangingAMediaRuleConditionTakesEffect()
        {
            var document = ParseDocument("<!doctype html><head><style>@media screen { .a { color: red } }</style></head><body><div class=a id=target></div>");
            var window = document.DefaultView;
            var target = document.GetElementById("target");

            Assert.AreEqual(Red, window.GetComputedStyle(target).GetColor());

            var media = (CssMediaRule)GetSheet(document).Rules[0];
            media.ConditionText = "print";

            Assert.AreNotEqual(Red, window.GetComputedStyle(target).GetColor());
        }

        [Test]
        public void ChangingASupportsConditionTakesEffect()
        {
            var document = ParseDocument("<!doctype html><head><style>@supports (display: block) { .a { color: red } }</style></head><body><div class=a id=target></div>");
            var window = document.DefaultView;
            var target = document.GetElementById("target");

            Assert.AreEqual(Red, window.GetComputedStyle(target).GetColor());

            var supports = (CssSupportsRule)GetSheet(document).Rules[0];
            supports.ConditionText = "(display: block) and (display: not-a-display)";

            Assert.AreNotEqual(Red, window.GetComputedStyle(target).GetColor());
        }

        [Test]
        public void DifferentRenderDevicesDoNotShareTheCache()
        {
            var document = ParseDocument("<!doctype html><head><style>@media (min-width: 800px) { .a { color: red } }</style></head><body><div class=a id=target></div>");
            var window = document.DefaultView;
            var target = document.GetElementById("target");
            var wide = new DefaultRenderDevice { ViewPortWidth = 1000 };
            var narrow = new DefaultRenderDevice { ViewPortWidth = 400 };

            Assert.AreEqual(Red, window.GetStyleCollection(wide).GetDeclarations(target).GetColor());
            Assert.AreNotEqual(Red, window.GetStyleCollection(narrow).GetDeclarations(target).GetColor());
            Assert.AreEqual(Red, window.GetStyleCollection(wide).GetDeclarations(target).GetColor());
        }

        [Test]
        public void ChangingTheDeviceStateInvalidatesTheCachedCollection()
        {
            var document = ParseDocument("<!doctype html><head><style>@media (min-width: 800px) { .a { color: red } }</style></head><body><div class=a id=target></div>");
            var window = document.DefaultView;
            var target = document.GetElementById("target");
            var device = new DefaultRenderDevice { ViewPortWidth = 400 };

            Assert.AreNotEqual(Red, window.GetStyleCollection(device).GetDeclarations(target).GetColor());

            device.ViewPortWidth = 1000;

            Assert.AreEqual(Red, window.GetStyleCollection(device).GetDeclarations(target).GetColor());
        }

        #endregion

        #region Other consumers

        [Test]
        public void InnerTextStillAppliesBucketedRules()
        {
            var document = ParseDocument("<!doctype html><head><style>.hidden { display: none } .card .title { display: block }</style></head><body><div class=card id=root><span class=title>kept</span><span class=hidden>dropped</span></div>");
            var text = document.GetElementById("root").GetInnerText();

            StringAssert.Contains("kept", text);
            StringAssert.DoesNotContain("dropped", text);
        }

        [Test]
        public void RenderTreeStillAppliesBucketedRules()
        {
            var document = ParseDocument("<!doctype html><head><style>.card .title { color: red }</style></head><body><div class=card><span class=title id=target></span></div>");
            var window = document.DefaultView;
            var node = FindNode(window.Render(), document.GetElementById("target"));

            Assert.IsNotNull(node);
            Assert.AreEqual(Red, node.ComputedStyle.GetColor());
        }

        private static ElementRenderNode FindNode(IRenderNode node, IElement element)
        {
            if (node is ElementRenderNode renderNode)
            {
                if (ReferenceEquals(renderNode.Ref, element))
                {
                    return renderNode;
                }

                foreach (var child in renderNode.Children)
                {
                    var found = FindNode(child, element);

                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        #endregion

        #region Helpers

        private static ICssStyleSheet GetSheet(IHtmlDocument document) =>
            document.StyleSheets.OfType<ICssStyleSheet>().First();

        #endregion
    }
}
