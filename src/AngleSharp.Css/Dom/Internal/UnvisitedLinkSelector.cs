#nullable enable
namespace AngleSharp.Css.Dom
{
    using AngleSharp.Dom;
    using System;

    /// <summary>
    /// Decorates the <c>:link</c> selector so that a link the caller has marked as visited stops
    /// matching it.
    /// </summary>
    /// <remarks>
    /// The two states are mutually exclusive (Selectors 4, §8.2), and AngleSharp's own <c>:link</c>
    /// is written that way: it is a link that has not been visited. What it cannot know is whether a
    /// link has been visited, because a document with no browsing session has no history and its
    /// answer is hard-coded to "never". A caller that forces the visited mark through
    /// <see cref="AngleSharp.Dom.ElementExtensions.SetPseudoClass"/> therefore left its links matching
    /// both <c>:link</c> and <c>:visited</c> at once. This is where the mark it set is read back.
    /// </remarks>
    sealed class UnvisitedLinkSelector : ISelector
    {
        private readonly ISelector _inner;

        public UnvisitedLinkSelector(ISelector inner)
        {
            _inner = inner;
        }

        public Priority Specificity => _inner.Specificity;

        public String Text => _inner.Text;

        public void Accept(ISelectorVisitor visitor) => _inner.Accept(visitor);

        public Boolean Match(IElement element, IElement? scope) =>
            _inner.Match(element, scope) && !HasBeenRead(element);

        private static Boolean HasBeenRead(IElement element) =>
            PseudoClassStateStore.TryGet(element, PseudoClassNames.Visited, out var visited) && visited;
    }
}
