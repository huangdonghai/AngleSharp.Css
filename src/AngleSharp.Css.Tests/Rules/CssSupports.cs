#nullable disable
namespace AngleSharp.Css.Tests.Rules
{
    using AngleSharp.Css.Dom;
    using System;
    using NUnit.Framework;
    using static CssConstructionFunctions;

    [TestFixture]
    public class CssSupportsTests
    {
        [Test]
        public void SupportsEmptyRule()
        {
            var source = @"@supports () { }";
            var sheet = ParseStyleSheet(source);
            var device = new DefaultRenderDevice();
            Assert.AreEqual(1, sheet.Rules.Length);
            Assert.IsInstanceOf<CssSupportsRule>(sheet.Rules[0]);
            var supports = sheet.Rules[0] as CssSupportsRule;
            Assert.AreEqual("()", supports.ConditionText);
            Assert.IsTrue(supports.Condition.Check(device));
        }

        [Test]
        public void SupportsBackgroundColorRedRule()
        {
            var source = @"@supports (background-color: red) { }";
            var sheet = ParseStyleSheet(source);
            var device = new DefaultRenderDevice();
            Assert.AreEqual(1, sheet.Rules.Length);
            Assert.IsInstanceOf<CssSupportsRule>(sheet.Rules[0]);
            var supports = sheet.Rules[0] as CssSupportsRule;
            Assert.AreEqual("(background-color: red)", supports.ConditionText);
            Assert.IsTrue(supports.Condition.Check(device));
        }

        [Test]
        public void SupportsBackgroundColorRedAndColorBlueRule()
        {
            var source = @"@supports ((background-color: red) and (color: blue)) { }";
            var sheet = ParseStyleSheet(source);
            var device = new DefaultRenderDevice();
            Assert.AreEqual(1, sheet.Rules.Length);
            Assert.IsInstanceOf<CssSupportsRule>(sheet.Rules[0]);
            var supports = sheet.Rules[0] as CssSupportsRule;
            Assert.AreEqual("((background-color: red) and (color: blue))", supports.ConditionText);
            Assert.IsTrue(supports.Condition.Check(device));
        }

        [Test]
        public void SupportsNotUnsupportedDeclarationRule()
        {
            var source = @"@supports (not (background-transparency: half)) { }";
            var sheet = ParseStyleSheet(source);
            var device = new DefaultRenderDevice();
            Assert.AreEqual(1, sheet.Rules.Length);
            Assert.IsInstanceOf<CssSupportsRule>(sheet.Rules[0]);
            var supports = sheet.Rules[0] as CssSupportsRule;
            Assert.AreEqual("(not (background-transparency: half))", supports.ConditionText);
            Assert.IsTrue(supports.Condition.Check(device));
        }

        [Test]
        public void SupportsUnsupportedDeclarationRule()
        {
            var source = @"@supports ((background-transparency: zero)) { }";
            var sheet = ParseStyleSheet(source);
            var device = new DefaultRenderDevice();
            Assert.AreEqual(1, sheet.Rules.Length);
            Assert.IsInstanceOf<CssSupportsRule>(sheet.Rules[0]);
            var supports = sheet.Rules[0] as CssSupportsRule;
            Assert.AreEqual("((background-transparency: zero))", supports.ConditionText);
            Assert.IsFalse(supports.Condition.Check(device));
        }

        [Test]
        public void SupportsBackgroundRedWithImportantRule()
        {
            var source = @"@supports (background: red !important) { }";
            var sheet = ParseStyleSheet(source);
            var device = new DefaultRenderDevice();
            Assert.AreEqual(1, sheet.Rules.Length);
            Assert.IsInstanceOf<CssSupportsRule>(sheet.Rules[0]);
            var supports = sheet.Rules[0] as CssSupportsRule;
            Assert.AreEqual("(background: red !important)", supports.ConditionText);
            Assert.IsTrue(supports.Condition.Check(device));
        }

        [Test]
        public void SupportsPaddingTopOrPaddingLeftRule()
        {
            var source = @"@supports ((padding-TOP :  0) or (padding-left : 0)) { }";
            var sheet = ParseStyleSheet(source);
            var device = new DefaultRenderDevice();
            Assert.AreEqual(1, sheet.Rules.Length);
            Assert.IsInstanceOf<CssSupportsRule>(sheet.Rules[0]);
            var supports = sheet.Rules[0] as CssSupportsRule;
            Assert.AreEqual("((padding-TOP: 0) or (padding-left: 0))", supports.ConditionText);
            Assert.IsTrue(supports.Condition.Check(device));
        }

        [Test]
        public void SupportsPaddingTopOrPaddingLeftAndPaddingBottomOrPaddingRightRule()
        {
            var source = @"@supports (((padding-top: 0)  or  (padding-left: 0))  and  ((padding-bottom:  0)  or  (padding-right: 0))) { }";
            var sheet = ParseStyleSheet(source);
            var device = new DefaultRenderDevice();
            Assert.AreEqual(1, sheet.Rules.Length);
            Assert.IsInstanceOf<CssSupportsRule>(sheet.Rules[0]);
            var supports = sheet.Rules[0] as CssSupportsRule;
            Assert.AreEqual("(((padding-top: 0) or (padding-left: 0)) and ((padding-bottom: 0) or (padding-right: 0)))", supports.ConditionText);
            Assert.IsTrue(supports.Condition.Check(device));
        }

        [Test]
        public void SupportsDisplayFlexWithImportantRule()
        {
            var source = @"@supports (display: flex !important) { }";
            var sheet = ParseStyleSheet(source);
            var device = new DefaultRenderDevice();
            Assert.AreEqual(1, sheet.Rules.Length);
            Assert.IsInstanceOf<CssSupportsRule>(sheet.Rules[0]);
            var supports = sheet.Rules[0] as CssSupportsRule;
            Assert.AreEqual("(display: flex !important)", supports.ConditionText);
            Assert.IsTrue(supports.Condition.Check(device));
        }

        [Test]
        public void SupportsBareDisplayFlexRule()
        {
            var source = @"@supports display: flex { }";
            var sheet = ParseStyleSheet(source);
            Assert.AreEqual(0, sheet.Rules.Length);
        }

        [Test]
        public void SupportsDisplayFlexMultipleBracketsRule()
        {
            var source = @"@supports ((display: flex)) { }";
            var sheet = ParseStyleSheet(source);
            var device = new DefaultRenderDevice();
            Assert.AreEqual(1, sheet.Rules.Length);
            Assert.IsInstanceOf<CssSupportsRule>(sheet.Rules[0]);
            var supports = sheet.Rules[0] as CssSupportsRule;
            Assert.AreEqual("((display: flex))", supports.ConditionText);
            Assert.IsTrue(supports.Condition.Check(device));
        }

        [Test]
        public void SupportsTransitionOrAnimationNameAndTransformFrontBracketRule()
        {
            var source = @"@supports ((transition-property: color) or
           (animation-name: foo)) and
          (transform: rotate(10deg)) { }";
            var sheet = ParseStyleSheet(source);
            var device = new DefaultRenderDevice();
            Assert.AreEqual(1, sheet.Rules.Length);
            Assert.IsInstanceOf<CssSupportsRule>(sheet.Rules[0]);
            var supports = sheet.Rules[0] as CssSupportsRule;
            Assert.AreEqual("((transition-property: color) or (animation-name: foo)) and (transform: rotate(10deg))", supports.ConditionText);
            Assert.IsTrue(supports.Condition.Check(device));
        }

        [Test]
        public void SupportsTransitionOrAnimationNameAndTransformBackBracketRule()
        {
            var source = @"@supports (transition-property: color) or
           ((animation-name: foo) and
          (transform: rotate(10deg))) { }";
            var sheet = ParseStyleSheet(source);
            var device = new DefaultRenderDevice();
            Assert.AreEqual(1, sheet.Rules.Length);
            Assert.IsInstanceOf<CssSupportsRule>(sheet.Rules[0]);
            var supports = sheet.Rules[0] as CssSupportsRule;
            Assert.AreEqual("(transition-property: color) or ((animation-name: foo) and (transform: rotate(10deg)))", supports.ConditionText);
            Assert.IsTrue(supports.Condition.Check(device));
        }

        [Test]
        public void SupportsShadowVendorPrefixesRule()
        {
            var source = @"@supports ( box-shadow: 0 0 2px black ) or
          ( -moz-box-shadow: 0 0 2px black ) or
          ( -webkit-box-shadow: 0 0 2px black ) or
          ( -o-box-shadow: 0 0 2px black ) { }";
            var sheet = ParseStyleSheet(source);
            var device = new DefaultRenderDevice();
            Assert.AreEqual(1, sheet.Rules.Length);
            Assert.IsInstanceOf<CssSupportsRule>(sheet.Rules[0]);
            var supports = sheet.Rules[0] as CssSupportsRule;
            Assert.AreEqual("(box-shadow: 0 0 2px black) or (-moz-box-shadow: 0 0 2px black) or (-webkit-box-shadow: 0 0 2px black) or (-o-box-shadow: 0 0 2px black)", supports.ConditionText);
            Assert.IsTrue(supports.Condition.Check(device));
        }

        [Test]
        public void SupportsNegatedDisplayFlexRuleWithDeclarations()
        {
            var source = @"@supports not ( display: flex ) {
  body { width: 100%; height: 100%; background: white; color: black; }
  #navigation { width: 25%; }
  #article { width: 75%; }
}";
            var sheet = ParseStyleSheet(source);
            var device = new DefaultRenderDevice();
            Assert.AreEqual(1, sheet.Rules.Length);
            Assert.IsInstanceOf<CssSupportsRule>(sheet.Rules[0]);
            var supports = sheet.Rules[0] as CssSupportsRule;
            Assert.AreEqual(3, supports.Rules.Length);
            Assert.AreEqual("not (display: flex)", supports.ConditionText);
            Assert.IsFalse(supports.Condition.Check(device));
        }

        [Test]
        public void SupportsUppercaseAndKeywordRule()
        {
            var source = @"@supports ((background-color: red) AND (color: blue)) { }";
            var sheet = ParseStyleSheet(source);
            var device = new DefaultRenderDevice();
            Assert.AreEqual(1, sheet.Rules.Length);
            Assert.IsInstanceOf<CssSupportsRule>(sheet.Rules[0]);
            var supports = sheet.Rules[0] as CssSupportsRule;
            Assert.AreEqual("((background-color: red) and (color: blue))", supports.ConditionText);
            Assert.IsTrue(supports.Condition.Check(device));
        }

        [Test]
        public void SupportsUppercaseOrKeywordRule()
        {
            var source = @"@supports ((background-transparency: half) OR (color: blue)) { }";
            var sheet = ParseStyleSheet(source);
            var device = new DefaultRenderDevice();
            Assert.AreEqual(1, sheet.Rules.Length);
            Assert.IsInstanceOf<CssSupportsRule>(sheet.Rules[0]);
            var supports = sheet.Rules[0] as CssSupportsRule;
            Assert.AreEqual("((background-transparency: half) or (color: blue))", supports.ConditionText);
            Assert.IsTrue(supports.Condition.Check(device));
        }

        [Test]
        public void SupportsMixedCaseAndKeywordChainRule()
        {
            var source = @"@supports ((background-color: red) And (color: blue) aND (width: 10px)) { }";
            var sheet = ParseStyleSheet(source);
            var device = new DefaultRenderDevice();
            Assert.AreEqual(1, sheet.Rules.Length);
            Assert.IsInstanceOf<CssSupportsRule>(sheet.Rules[0]);
            var supports = sheet.Rules[0] as CssSupportsRule;
            Assert.AreEqual("((background-color: red) and (color: blue) and (width: 10px))", supports.ConditionText);
            Assert.IsTrue(supports.Condition.Check(device));
        }

        [Test]
        public void SupportsUppercaseAndKeywordKeepsInnerRules()
        {
            var source = @"@supports (color: red) AND (display: flex) {
  body { width: 100%; }
}";
            var sheet = ParseStyleSheet(source);
            Assert.AreEqual(1, sheet.Rules.Length);
            Assert.IsInstanceOf<CssSupportsRule>(sheet.Rules[0]);
            var supports = sheet.Rules[0] as CssSupportsRule;
            Assert.AreEqual(1, supports.Rules.Length);
        }

        [Test]
        public void SupportsCustomPropertyNameIsANameTheConditionKnows()
        {
            Assert.IsTrue(Check(@"@supports (--a: green) { }"));
        }

        [Test]
        public void SupportsCustomPropertyNameNegatedIsFalse()
        {
            Assert.IsFalse(Check(@"@supports (not (--a: green)) { }"));
        }

        [Test]
        public void SupportsTwoDashesAloneAreNotAName()
        {
            Assert.IsFalse(Check(@"@supports (--: a) { }"));
        }

        [Test]
        public void SupportsCustomPropertyWithNothingAfterTheColonStillHolds()
        {
            Assert.IsTrue(Check(@"@supports (--a: ) { }"));
        }

        [Test]
        public void SupportsCustomPropertyHoldingAVariableReferenceHolds()
        {
            Assert.IsTrue(Check(@"@supports (--a: var(--b)) { }"));
        }

        [Test]
        public void SupportsVariableReferenceIsNotCheckedAgainstThePropertyGrammar()
        {
            // A length where a colour was asked is no reason for the condition to fail: what the reference
            // will come to is only known once the cascade has been read.
            Assert.IsTrue(Check(@"@supports (color: 1px var(--a)) { }"));
        }

        [Test]
        public void SupportsSpoiledVariableReferenceIsFalseAndItsNegationTrue()
        {
            Assert.IsFalse(Check(@"@supports (color: var(--a,!)) { }"));
            Assert.IsTrue(Check(@"@supports (not (color: var(--a,!))) { }"));
        }

        [Test]
        public void SupportsSpoiledFallbacksAreFalseHoweverTheyAreSpoiled()
        {
            Assert.IsFalse(Check(@"@supports (color: var(--a,;)) { }"));
            Assert.IsFalse(Check(@"@supports (color: var(--a,!important)) { }"));
            Assert.IsFalse(Check(@"@supports (color: var(--a);) { }"));
            Assert.IsFalse(Check(@"@supports (color: var(--a) !important !important) { }"));
            Assert.IsFalse(Check(@"@supports (color: var(--a,])) { }"));
        }

        [Test]
        public void SupportsFallbackMayHoldWhatItsTopLevelMayNot()
        {
            Assert.IsTrue(Check(@"@supports (color: var(--a,/**/)) { }"));
            Assert.IsTrue(Check(@"@supports (color: var(--a, green)) { }"));
            // A group that holds a semicolon is the one form still lost: the at-rule's prelude ends at the
            // semicolon wherever it stands rather than at one outside every group, so neither
            // `(color: var(--a,(;)))` nor `(color: [;] var(--a))` reaches a check at all.
        }

        [Test]
        public void SupportsCommentTokensAreNotDelimiters()
        {
            Assert.IsTrue(Check(@"@supports (color: var(--a) <!--) { }"));
            Assert.IsTrue(Check(@"@supports (color: --> var(--a)) { }"));
        }

        [Test]
        public void SupportsUnknownPropertyIsFalseWhateverItsValueNames()
        {
            Assert.IsFalse(Check(@"@supports (xprop: red) { }"));
            Assert.IsFalse(Check(@"@supports (xprop: var(--a)) { }"));
            Assert.IsFalse(Check(@"@supports (yadayada: x, calc( 2/3 )) { }"));
        }

        [Test]
        public void SupportsNegationNeedsWhitespaceBeforeItsCondition()
        {
            // Conditional 3 3 asks for `not` and then whitespace; `not(` is a function token, and the
            // condition as a whole is then neither a negation nor a declaration, so the rule is dropped.
            Assert.IsNull(ParseRule(@"@supports (not(foo: baz)) { p { color: green; } }"));
        }

        // A condition that cannot be read at all leaves no rule behind, which is the same answer to the
        // page as a condition that reads false: its block never applies.
        static Boolean Check(String source)
        {
            var sheet = ParseStyleSheet(source);

            if (sheet.Rules.Length == 0)
            {
                return false;
            }

            var supports = sheet.Rules[0] as CssSupportsRule;
            return supports.Condition.Check(new DefaultRenderDevice());
        }
    }
}
