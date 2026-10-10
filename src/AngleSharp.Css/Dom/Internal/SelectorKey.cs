#nullable enable
namespace AngleSharp.Css.Dom
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The kind of an essential selector key.
    /// </summary>
    internal enum SelectorKeyKind : Byte
    {
        /// <summary>
        /// The key is an id, compared case-sensitively (see IdSelector).
        /// </summary>
        Id = 0,

        /// <summary>
        /// The key is a class name, compared case-sensitively (see ClassSelector).
        /// </summary>
        Class = 1,

        /// <summary>
        /// The key is a tag name, compared case-insensitively (see TypeSelector).
        /// </summary>
        Tag = 2,
    }

    /// <summary>
    /// An essential property that any element matched by a selector is required to have.
    /// </summary>
    internal readonly struct SelectorKey
    {
        /// <summary>
        /// Creates a new essential selector key.
        /// </summary>
        /// <param name="kind">The kind of the key.</param>
        /// <param name="value">The required value.</param>
        public SelectorKey(SelectorKeyKind kind, String value)
        {
            Kind = kind;
            Value = value;
        }

        /// <summary>
        /// Gets the kind of the key.
        /// </summary>
        public SelectorKeyKind Kind { get; }

        /// <summary>
        /// Gets the value the element has to carry.
        /// </summary>
        public String Value { get; }
    }

    /// <summary>
    /// Derives the set of essential selector keys of a selector, i.e. the id, class or tag names
    /// that the rightmost compound selector requires of every element it matches.
    /// </summary>
    /// <remarks>
    /// A key is only reported when it is a necessary condition of a match, so an element that does
    /// not carry it can never be matched by the selector. Callers use that to skip the rule without
    /// ever testing it. Whenever a selector cannot be reduced to such a condition - pseudo-classes
    /// and pseudo-elements, attribute selectors, namespace prefixes, nesting references, or a
    /// selector list with a member that has no key of its own - the whole selector yields no key
    /// set at all, and the associated rule has to be consulted for every element.
    /// </remarks>
    internal sealed class SelectorKeyExtractor : ISelectorVisitor
    {
        // Both of these reach the visitor through Type(), yet neither constrains the tag name of
        // the matched element: AllSelector matches everything and NestedSelector only asks for the
        // root element.
        private const String AnyName = "*";
        private const String NestingName = "&";

        private readonly List<SelectorKey> _keys = new();
        private readonly List<String> _classes = new();

        private String? _id;
        private String? _tag;
        private Boolean _hasKeylessPosition;
        private Boolean _collecting;

        private SelectorKeyExtractor()
        {
        }

        /// <summary>
        /// Extracts the essential keys of the given selector, or null if the selector cannot be
        /// reduced to a set of necessary conditions.
        /// </summary>
        /// <param name="selector">The selector to analyse, if any.</param>
        /// <returns>The keys, or null if every element has to be considered.</returns>
        public static SelectorKey[]? Extract(ISelector? selector)
        {
            if (selector is null)
            {
                return null;
            }

            var extractor = new SelectorKeyExtractor();
            extractor.VisitPosition(selector);
            return extractor._hasKeylessPosition || extractor._keys.Count == 0
                ? null
                : extractor._keys.ToArray();
        }

        #region Position handling

        /// <summary>
        /// Visits a selector that occupies a complete "rightmost compound" position: the whole
        /// selector, one member of a comma-separated list, or the tail of a combinator chain.
        /// Exactly one key is recorded for it, unless it has none.
        /// </summary>
        private void VisitPosition(ISelector selector)
        {
            ResetKey();

            var collecting = _collecting;
            var recorded = _keys.Count;
            _collecting = true;
            selector.Accept(this);
            _collecting = collecting;

            // A nested position - a member of a selector list, or the tail of a combinator chain -
            // records its own key and reports itself as keyless when it has none. Re-flushing here
            // would read that already consumed state as "this position contributes nothing".
            if (_keys.Count == recorded && !FlushKey())
            {
                // A member without a key makes the whole selector unusable as a filter: the very
                // element the caller would skip is the one this member was going to match.
                _hasKeylessPosition = true;
            }
        }

        /// <summary>
        /// Records the key collected so far, if any, and resets the collector.
        /// </summary>
        private Boolean FlushKey()
        {
            var found = false;

            if (_id is not null)
            {
                Append(SelectorKeyKind.Id, _id);
                found = true;
            }
            else if (_classes.Count > 0)
            {
                // Any single class is enough: the element has to carry it either way, so the rule
                // can live in exactly one class bucket instead of all of them.
                Append(SelectorKeyKind.Class, _classes[0]);
                found = true;
            }
            else if (_tag is not null)
            {
                Append(SelectorKeyKind.Tag, _tag);
                found = true;
            }

            ResetKey();
            return found;
        }

        private void ResetKey()
        {
            _id = null;
            _tag = null;
            _classes.Clear();
        }

        private void Append(SelectorKeyKind kind, String value)
        {
            var comparison = kind == SelectorKeyKind.Tag
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            for (var i = 0; i < _keys.Count; i++)
            {
                if (_keys[i].Kind == kind && String.Equals(_keys[i].Value, value, comparison))
                {
                    return;
                }
            }

            _keys.Add(new SelectorKey(kind, value));
        }

        #endregion

        #region ISelectorVisitor

        void ISelectorVisitor.List(IEnumerable<ISelector> selectors)
        {
            // Each member of the list is a complete alternative, so each gets its own key.
            foreach (var selector in selectors)
            {
                VisitPosition(selector);
            }
        }

        void ISelectorVisitor.Combinator(IEnumerable<ISelector> selectors, IEnumerable<String> symbols)
        {
            var last = default(ISelector);

            foreach (var selector in selectors)
            {
                last = selector;
            }

            if (last is not null)
            {
                // Only the rightmost compound has to match the element itself - everything to its
                // left may sit anywhere in the document relative to it.
                //
                // A namespace prefix never ends up here: it is applied by NamespaceCombinator to
                // the compound on its left, while the tail is concluded unchanged.
                VisitPosition(last);
            }
        }

        void ISelectorVisitor.Many(IEnumerable<ISelector> selectors)
        {
            // The simple selectors of a compound are all required, so every one of them is a
            // candidate key. Only their own callbacks are collected - never those of anything they
            // wrap, so `:not(.x)`, `:is(div)` or a nested `&` cannot contribute a key.
            foreach (var selector in selectors)
            {
                selector.Accept(this);
            }
        }

        void ISelectorVisitor.Id(String value)
        {
            if (_collecting && _id is null)
            {
                _id = value;
            }
        }

        void ISelectorVisitor.Class(String name)
        {
            if (_collecting && !_classes.Contains(name))
            {
                _classes.Add(name);
            }
        }

        void ISelectorVisitor.Type(String name)
        {
            if (_collecting && _tag is null &&
                !String.Equals(name, AnyName, StringComparison.Ordinal) &&
                !String.Equals(name, NestingName, StringComparison.Ordinal))
            {
                _tag = name;
            }
        }

        void ISelectorVisitor.Attribute(String name, String op, String? value)
        {
        }

        void ISelectorVisitor.Child(String name, Int32 step, Int32 offset, ISelector selector)
        {
        }

        void ISelectorVisitor.PseudoClass(String name)
        {
        }

        void ISelectorVisitor.PseudoElement(String name)
        {
        }

        #endregion
    }
}
