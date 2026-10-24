#nullable disable
namespace AngleSharp.Css.Parser
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Css.Values;
    using AngleSharp.Text;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents extensions to for general CSS functions.
    /// </summary>
    public static class FunctionParser
    {
        /// <summary>
        /// Parses a CSS attr value.
        /// http://dev.w3.org/csswg/css-values/#funcdef-attr
        /// </summary>
        public static CssAttrValue ParseAttr(this StringSource source)
        {
            if (source.IsFunction(FunctionNames.Attr))
            {
                var content = source.ParseString() ?? source.ParseIdent();
                var f = source.SkipGetSkip();

                if (content != null && f == Symbols.RoundBracketClose)
                {
                    return new CssAttrValue(content);
                }

                // Content 3 §10.5 lets the page state the text to stand where an attribute it has not got
                // would have been. A typed reading of the attribute is not taken here, which is what it was
                // before as well: the argument after the comma has to be a string.
                if (content != null && f == Symbols.Comma)
                {
                    var fallback = source.ParseString();
                    var closing = source.SkipGetSkip();

                    if (fallback != null && closing == Symbols.RoundBracketClose)
                    {
                        return new CssAttrValue(content, fallback);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Parses a CSS variable reference value.
        /// </summary>
        public static CssReferenceValue ParseVars(this StringSource source)
        {
            var index = source.Index;
            var start = index;
            var length = FunctionNames.Var.Length;
            var refs = default(List<Tuple<TextRange, CssVarValue>>);

            while (!source.IsDone)
            {
                index = source.Content.IndexOf(FunctionNames.Var, index, StringComparison.OrdinalIgnoreCase) + length;

                if (index >= length)
                {
                    source.NextTo(index);
                    var c = source.SkipSpacesAndComments();

                    if (c == Symbols.RoundBracketOpen)
                    {
                        source.SkipCurrentAndSpaces();
                        var s = new TextPosition(0, 0, source.Index);
                        var reference = ParseVar(source);

                        if (reference == null)
                        {
                            refs = null;
                            break;
                        }

                        if (refs == null)
                        {
                            refs = new List<Tuple<TextRange, CssVarValue>>();
                        }

                        var e = new TextPosition(0, 0, source.Index);
                        refs.Add(Tuple.Create(new TextRange(s, e), reference));
                        continue;
                    }
                }

                break;
            }

            source.BackTo(start);

            if (refs != null)
            {
                return new CssReferenceValue(new CssVariableValue(source.Content), refs);
            }

            return null;
        }

        /// <summary>
        /// Parses a CSS var (variable) value.
        /// </summary>
        public static CssVarValue ParseVar(this StringSource source)
        {
            var name = source.ParseCustomIdent();
            var f = source.SkipGetSkip();

            if (name is not null)
            {
                switch (f)
                {
                    case Symbols.RoundBracketClose:
                    // A reference the sheet never closed is closed where the input ends, and the
                    // declaration it was written in stands as one that was closed (CSS Syntax 3 §5.4.3).
                    case Symbols.EndOfFile:
                        return new CssVarValue(name);
                    case Symbols.Comma:
                        source.SkipSpacesAndComments();
                        var defaultValue = ParseVarFallback(source);
                        return new CssVarValue(name, defaultValue);
                }
            }

            return null;
        }

        /// <summary>
        /// Parses a CSS var (variable) fallback value.
        /// </summary>
        public static ICssValue ParseVarFallback(this StringSource source)
        {
            var names = new Stack<String>();
            ICssValue fallback = null;
            var readFallback = true;

            while (source.IsFunction(FunctionNames.Var))
            {
                var name = source.ParseCustomIdent();
                var separator = source.SkipGetSkip();

                // The end of the input closes a reference the sheet never closed, at every depth of
                // the nesting, exactly as the paren that was left out would have.
                if (name is null ||
                    (separator != Symbols.Comma &&
                     separator != Symbols.RoundBracketClose &&
                     separator != Symbols.EndOfFile))
                {
                    readFallback = false;
                    break;
                }

                names.Push(name);

                if (separator != Symbols.Comma)
                {
                    readFallback = false;
                    break;
                }

                source.SkipSpacesAndComments();
            }

            if (readFallback)
            {
                var content = source.TakeUntilClosed();
                source.SkipCurrentAndSpaces();
                fallback = new CssAnyValue(content);
            }

            while (names.Count > 0)
            {
                fallback = new CssVarValue(names.Pop(), fallback);
            }

            return fallback;
        }

        /// <summary>
        /// Parses a CSS content value.
        /// </summary>
        public static CssContentValue ParseContent(this StringSource source)
        {
            if (source.IsFunction(FunctionNames.Content))
            {
                var name = source.ParseCustomIdent();
                var f = source.SkipGetSkip();

                if (name != null && f == Symbols.RoundBracketClose)
                {
                    return new CssContentValue(name);
                }
            }

            return null;
        }

        /// <summary>
        /// Parses a CSS running value.
        /// </summary>
        public static CssRunningValue ParseRunning(this StringSource source)
        {
            if (source.IsFunction(FunctionNames.Running))
            {
                var name = source.ParseCustomIdent();
                var f = source.SkipGetSkip();

                if (name != null && f == Symbols.RoundBracketClose)
                {
                    return new CssRunningValue(name);
                }
            }

            return null;
        }

        /// <summary>
        /// Parses a counter object.
        /// http://www.w3.org/TR/CSS2/syndata.html#value-def-counter
        /// </summary>
        public static CssCounterDefinitionValue? ParseCounter(this StringSource source)
        {
            if (source.IsFunction(FunctionNames.Counter))
            {
                var ident = source.ParseIdent();
                var f = source.SkipGetSkip();

                if (ident != null)
                {
                    return ParseCounterStyle(source, ident, null, f);
                }
            }
            else if (source.IsFunction(FunctionNames.Counters))
            {
                var ident = source.ParseIdent();
                var c = source.SkipGetSkip();
                var separator = source.ParseString();
                var f = source.SkipGetSkip();

                if (ident != null && separator != null && c == Symbols.Comma)
                {
                    return ParseCounterStyle(source, ident, separator, f);
                }
            }

            return null;
        }

        private static CssCounterDefinitionValue? ParseCounterStyle(StringSource source, String ident, String separator, Char f)
        {
            var style = CssKeywords.Decimal;

            if (f == Symbols.Comma)
            {
                style = source.ParseIdent();
                f = source.SkipGetSkip();
            }

            if (f == Symbols.RoundBracketClose)
            {
                return new CssCounterDefinitionValue(ident, style, separator);
            }

            return null;
        }
    }
}
