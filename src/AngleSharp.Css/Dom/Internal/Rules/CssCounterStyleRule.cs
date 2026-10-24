#nullable disable
namespace AngleSharp.Css.Dom
{
    using AngleSharp.Css;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Represents the @counter-style rule.
    /// </summary>
    [DebuggerDisplay(null, Name = "CssCounterStyleRule ({StyleName})")]
    sealed class CssCounterStyleRule : CssDeclarationRule, ICssCounterStyleRule, ICssRule
    {
        #region ctor

        internal CssCounterStyleRule(ICssStyleSheet owner)
            : base(owner, CssRuleType.CounterStyle, RuleNames.CounterStyle, new HashSet<String>(StringComparer.Ordinal)
            {
                // The ten descriptors of the chapter are the rule's own content, and a declaration
                // factory that has no grammar for them - which is all of them, since none of these is
                // a property of an element - would drop every one of them unless they are named here.
                "system",
                "negative",
                "prefix",
                "suffix",
                "symbols",
                "additive-symbols",
                "pad",
                "range",
                "fallback",
                "speak-as",
            })
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the counter style name argument.
        /// </summary>
        public String StyleName { get; set; }

        public String Name
        {
            get => StyleName;
            set => StyleName = value;
        }

        public String System
        {
            get => GetDescriptor("system");
            set => SetValue("system", value);
        }

        public String Symbols
        {
            get => GetDescriptor("symbols");
            set => SetValue("symbols", value);
        }

        public String AdditiveSymbols
        {
            get => GetDescriptor("additive-symbols");
            set => SetValue("additive-symbols", value);
        }

        public String Negative
        {
            get => GetDescriptor("negative");
            set => SetValue("negative", value);
        }

        public String Prefix
        {
            get => GetDescriptor("prefix");
            set => SetValue("prefix", value);
        }

        public String Suffix
        {
            get => GetDescriptor("suffix");
            set => SetValue("suffix", value);
        }

        public String Range
        {
            get => GetDescriptor("range");
            set => SetValue("range", value);
        }

        public String Pad
        {
            get => GetDescriptor("pad");
            set => SetValue("pad", value);
        }

        public String SpeakAs
        {
            get => GetDescriptor("speak-as");
            set => SetValue("speak-as", value);
        }

        public String Fallback
        {
            get => GetDescriptor("fallback");
            set => SetValue("fallback", value);
        }

        #endregion

        #region Methods

        protected override void ReplaceWith(ICssRule rule)
        {
        }

        private String GetDescriptor(String name) => GetPropertyValue(name);

        #endregion
    }
}
