#nullable enable
namespace AngleSharp.Css
{
    using System;
    using System.Threading;

    /// <summary>
    /// A process wide counter that is bumped whenever the CSS object model changes in a way that can
    /// affect which rules apply to an element, or in which order.
    /// </summary>
    /// <remarks>
    /// It exists so that the document level style cache can tell whether the rules it collected are
    /// still the ones the documents describe, without having to walk - and compare - the whole rule
    /// tree on every lookup.
    ///
    /// Only mutations that can change the set of collected rules, their order, or the selector of a
    /// style rule have to report here. Changing a declaration deliberately does not: declarations
    /// are never cached, they are always read from the rule itself when the cascade runs. Reporting
    /// such a change would invalidate the cache for nothing - and worse, the cascade writes into
    /// freshly built declarations constantly, so a hook there would keep the cache from ever being
    /// used.
    /// </remarks>
    internal static class CssMutationRevision
    {
        private static Int64 _value;

        /// <summary>
        /// Gets the current revision of the CSS object model.
        /// </summary>
        public static Int64 Current => Volatile.Read(ref _value);

        /// <summary>
        /// Reports a change of the CSS object model.
        /// </summary>
        public static void Next() => Interlocked.Increment(ref _value);
    }
}
