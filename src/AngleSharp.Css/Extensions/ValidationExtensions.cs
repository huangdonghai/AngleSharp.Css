namespace AngleSharp.Css.Dom
{
    using AngleSharp.Dom;
    using System;
    using System.Linq;

    /// <summary>
    /// Extensions for validating CSS rules.
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Determines if the rule is valid for a given device.
        /// </summary>
        /// <param name="rule">The rule to extend.</param>
        /// <param name="device">The device to check for support.</param>
        /// <returns>True if support is given, otherwise false.</returns>
        public static Boolean IsValid(this ICssSupportsRule rule, IRenderDevice device)
        {
            // A condition whose tree the parser left a hole in - an invalid selector inside a logical
            // combination is the case that reaches this - cannot be asked, and a group that gates its
            // rules on a question no engine can answer supplies none of them (CSS Conditional 4 §3).
            if (rule.Condition is not { } condition)
            {
                return false;
            }

            try
            {
                return condition.Check(device);
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if the rule is valid for a given device.
        /// </summary>
        /// <param name="rule">The rule to extend.</param>
        /// <param name="device">The device to check for conformance.</param>
        /// <returns>True if support is given, otherwise false.</returns>
        public static Boolean IsValid(this ICssMediaRule rule, IRenderDevice device) => rule.Media.Validate(device);

        /// <summary>
        /// Determines if the rule is valid for a given URL.
        /// </summary>
        /// <param name="rule">The rule to extend.</param>
        /// <param name="url">The URL to check for conformance.</param>
        /// <returns>True if the URL is matching, otherwise false.</returns>
        public static Boolean IsValid(this ICssDocumentRule rule, Url url) => rule.Conditions.Any(m => m.Matches(url));
    }
}
