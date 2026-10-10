#nullable enable
namespace AngleSharp.Css.Dom
{
    using AngleSharp.Dom;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Groups the rules of a style collection by the essential key of their rightmost compound
    /// selector, so that an element only has to be tested against the rules that could match it.
    /// </summary>
    /// <remarks>
    /// The index never decides a match by itself - it only prunes the rules that provably cannot
    /// match the element in question. Every rule whose selector cannot be reduced to a necessary
    /// condition, and every rule that is not a <see cref="CssStyleRule"/>, lands in the unkeyed
    /// bucket that each element consults.
    /// </remarks>
    internal sealed class StyleRuleBucketIndex
    {
        private readonly List<ICssStyleRule> _rules;
        private readonly Dictionary<String, List<Int32>> _idBuckets;
        private readonly Dictionary<String, List<Int32>> _classBuckets;
        private readonly Dictionary<String, List<Int32>> _tagBuckets;
        private readonly List<Int32> _unkeyed;

        private StyleRuleBucketIndex(
            List<ICssStyleRule> rules,
            Dictionary<String, List<Int32>> idBuckets,
            Dictionary<String, List<Int32>> classBuckets,
            Dictionary<String, List<Int32>> tagBuckets,
            List<Int32> unkeyed)
        {
            _rules = rules;
            _idBuckets = idBuckets;
            _classBuckets = classBuckets;
            _tagBuckets = tagBuckets;
            _unkeyed = unkeyed;
        }

        /// <summary>
        /// Gets the flattened rules in the order they have to be matched in. Nested rules follow
        /// the rule they are nested in.
        /// </summary>
        public List<ICssStyleRule> Rules => _rules;

        /// <summary>
        /// Builds the index for the given rules, which have to already be in the order they are to
        /// be matched in.
        /// </summary>
        /// <param name="rules">The rules to index.</param>
        /// <returns>The index over the flattened rules.</returns>
        public static StyleRuleBucketIndex Build(List<ICssStyleRule> rules)
        {
            var flattened = new List<ICssStyleRule>(rules.Count);

            for (var i = 0; i < rules.Count; i++)
            {
                Flatten(rules[i], flattened);
            }

            // The comparers have to mirror the selector matching exactly: ids and class names are
            // compared ordinally, tag names ignore case.
            var idBuckets = new Dictionary<String, List<Int32>>(StringComparer.Ordinal);
            var classBuckets = new Dictionary<String, List<Int32>>(StringComparer.Ordinal);
            var tagBuckets = new Dictionary<String, List<Int32>>(StringComparer.OrdinalIgnoreCase);
            var unkeyed = new List<Int32>();

            for (var i = 0; i < flattened.Count; i++)
            {
                var keys = flattened[i] is CssStyleRule rule ? rule.GetSelectorKeys() : null;

                if (keys is null || keys.Length == 0)
                {
                    unkeyed.Add(i);
                    continue;
                }

                for (var j = 0; j < keys.Length; j++)
                {
                    var key = keys[j];
                    var buckets = key.Kind switch
                    {
                        SelectorKeyKind.Id => idBuckets,
                        SelectorKeyKind.Class => classBuckets,
                        _ => tagBuckets,
                    };

                    if (!buckets.TryGetValue(key.Value, out var indices))
                    {
                        indices = new List<Int32>();
                        buckets.Add(key.Value, indices);
                    }

                    indices.Add(i);
                }
            }

            return new StyleRuleBucketIndex(flattened, idBuckets, classBuckets, tagBuckets, unkeyed);
        }

        /// <summary>
        /// Collects the indices of all rules that could match the given element, in ascending order
        /// and without duplicates.
        /// </summary>
        /// <param name="element">The element to query for.</param>
        /// <returns>The candidate rule indices.</returns>
        public List<Int32> CollectCandidates(IElement element)
        {
            var candidates = new List<Int32>();
            var id = element.Id;

            if (id is { Length: > 0 })
            {
                Append(_idBuckets, id, candidates);
            }

            var classes = element.ClassList;

            if (classes is not null)
            {
                for (var i = 0; i < classes.Length; i++)
                {
                    var name = classes[i];

                    if (name is not null)
                    {
                        Append(_classBuckets, name, candidates);
                    }
                }
            }

            var tag = element.LocalName;

            if (tag is not null)
            {
                Append(_tagBuckets, tag, candidates);
            }

            // Rules without a usable key have to be considered for every element.
            for (var i = 0; i < _unkeyed.Count; i++)
            {
                candidates.Add(_unkeyed[i]);
            }

            if (candidates.Count > 1)
            {
                // Ascending order is what keeps the tie-break between rules of equal specificity
                // identical to the full walk it replaces.
                candidates.Sort();
                Deduplicate(candidates);
            }

            return candidates;
        }

        #region Helpers

        private static void Flatten(ICssStyleRule rule, List<ICssStyleRule> flattened)
        {
            // Mirrors the traversal the cascade used to perform: a rule is considered before the
            // rules nested in it, so the relative order - and with it the tie-break between rules
            // of equal specificity - stays exactly the same.
            flattened.Add(rule);

            var subRules = rule.Rules;

            for (var i = 0; i < subRules.Length; i++)
            {
                if (subRules[i] is ICssStyleRule subRule)
                {
                    Flatten(subRule, flattened);
                }
            }
        }

        private static void Append(Dictionary<String, List<Int32>> buckets, String key, List<Int32> candidates)
        {
            if (buckets.TryGetValue(key, out var indices))
            {
                candidates.AddRange(indices);
            }
        }

        private static void Deduplicate(List<Int32> indices)
        {
            var write = 1;

            for (var read = 1; read < indices.Count; read++)
            {
                if (indices[read] != indices[write - 1])
                {
                    indices[write++] = indices[read];
                }
            }

            if (write < indices.Count)
            {
                indices.RemoveRange(write, indices.Count - write);
            }
        }

        #endregion
    }
}
