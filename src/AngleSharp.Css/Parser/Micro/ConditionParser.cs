#nullable disable
namespace AngleSharp.Css.Parser
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Text;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents extensions to for CSS condition values.
    /// </summary>
    public static class ConditionParser
    {
        internal static IConditionFunction Parse(String str, IBrowsingContext context)
        {
            var source = new StringSource(str);
            source.SkipSpacesAndComments();
            var result = source.ParseConditionFunction(context);
            return source.IsDone ? result : null;
        }

        /// <summary>
        /// Parse a CSS condition function using the current browsing context.
        /// </summary>
        public static IConditionFunction ParseConditionFunction(this StringSource source, IBrowsingContext context) =>
            source.Condition(context);

        private static IConditionFunction Condition(this StringSource source, IBrowsingContext context) =>
            source.Negation(context) ?? source.ConjunctionOrDisjunction(context);

        private static IConditionFunction Negation(this StringSource source, IBrowsingContext context)
        {
            var pos = source.Index;
            var ident = source.ParseIdent();

            // The grammar asks for `not` and then whitespace before the condition it turns around. Where the
            // parenthesis follows the keyword at once, the tokenizer has made a function token of it, and no
            // negation was written at all.
            if (ident != null && ident.Isi(CssKeywords.Not) && source.Current != Symbols.RoundBracketOpen)
            {
                source.SkipSpacesAndComments();
                var condition = source.Group(context);

                if (condition != null)
                {
                    return new NotCondition(condition);
                }
            }

            source.BackTo(pos);
            return null;
        }

        private static IConditionFunction ConjunctionOrDisjunction(this StringSource source, IBrowsingContext context)
        {
            var condition = source.Group(context);
            source.SkipSpacesAndComments();
            var pos = source.Index;
            var ident = source.ParseIdent();

            // `and(` and `or(` are function tokens, not the keywords: the bracket follows the name at
            // once, and a condition that writes one is a `<general-enclosed>` form (CSS Syntax 3 §4.3.9).
            if (ident != null && source.Current != Symbols.RoundBracketOpen)
            {
                var isAnd = ident.Isi(CssKeywords.And);
                var isOr = ident.Isi(CssKeywords.Or);

                if (isAnd || isOr)
                {
                    var conditions = source.Scan(ident, condition, context);

                    if (isAnd)
                    {
                        return new AndCondition(conditions);
                    }
                    else if (isOr)
                    {
                        return new OrCondition(conditions);
                    }
                }
            }

            source.BackTo(pos);
            return condition;
        }

        /// <summary>
        /// Reads a condition written as a function, of which CSS Conditional 4 knows <c>selector()</c>,
        /// <c>at-rule()</c>, <c>font-format()</c> and <c>font-tech()</c>. The tokenizer makes one token of
        /// the name and its opening bracket, so the argument is read to the bracket that closes it.
        /// </summary>
        private static IConditionFunction Function(this StringSource source, IBrowsingContext context)
        {
            if (source.IsFunction("at-rule"))
            {
                var name = source.FunctionArgument();

                return name.Length > 0 ? new AtRuleCondition(name) : null;
            }

            if (source.IsFunction("selector"))
            {
                var selector = source.FunctionArgument();

                return selector.Length > 0 ? new SelectorCondition(context, selector) : null;
            }

            if (source.IsFunction("font-format"))
            {
                return new FontCondition("font-format", source.FunctionArgument());
            }

            if (source.IsFunction("font-tech"))
            {
                return new FontCondition("font-tech", source.FunctionArgument());
            }

            // Any other function is a <general-enclosed> form: it is false, not unreadable, which is
            // what makes `@supports not unknown()` a condition the page's rules may stand on
            // (CSS Conditional 4 §3.1).
            var functionName = source.ParseFunctionName();

            if (functionName is null)
            {
                return null;
            }

            var argument = source.FunctionArgument();
            return new FalseCondition(functionName + "(" + argument + ")");
        }

        /// <summary>Reads the name of a function token, leaving the bracket to the caller.</summary>
        private static String ParseFunctionName(this StringSource source)
        {
            var pos = source.Index;
            var name = source.ParseIdent();

            if (name is null || source.Current != Symbols.RoundBracketOpen)
            {
                source.BackTo(pos);
                return null;
            }

            return name;
        }

        /// <summary>Reads the text of a function's argument, up to the bracket that closes it.</summary>
        private static String FunctionArgument(this StringSource source)
        {
            if (source.Current == Symbols.RoundBracketOpen)
            {
                source.Next();
            }

            var argument = source.TakeUntilClosed().Trim();
            source.SkipCurrentAndSpaces();

            return argument;
        }

        private static IConditionFunction Group(this StringSource source, IBrowsingContext context)
        {
            if (source.Current == Symbols.RoundBracketOpen)
            {
                var current = source.SkipCurrentAndSpaces();
                var condition = default(IConditionFunction);

                if (current != Symbols.RoundBracketClose)
                {
                    condition = source.Condition(context) ?? source.Declaration(context);
                    current = source.SkipSpacesAndComments();

                    if (condition == null)
                    {
                        return null;
                    }
                }

                if (current == Symbols.RoundBracketClose)
                {
                    source.SkipCurrentAndSpaces();
                    return new GroupCondition(condition);
                }
            }

            return source.Function(context);
        }

        private static IConditionFunction Declaration(this StringSource source, IBrowsingContext context)
        {
            // A custom property name is not an identifier the ordinary reading accepts - two dashes stand
            // where the first character would have to be - and it is a declaration the condition may still
            // ask about (CSS Variables 1 §3.1).
            var name = source.ParseCustomName() ?? source.ParseIdent();
            var colon = source.SkipSpacesAndComments();
            source.SkipCurrentAndSpaces();
            var value = source.TakeUntilClosed();
            source.SkipSpacesAndComments();

            if (name != null && value != null && colon == Symbols.Colon)
            {
                return new DeclarationCondition(context, name, value);
            }

            return null;
        }

        private static IEnumerable<IConditionFunction> Scan(this StringSource source, String keyword, IConditionFunction condition, IBrowsingContext context)
        {
            var conditions = new List<IConditionFunction>();
            var ident = String.Empty;
            conditions.Add(condition);

            do
            {
                source.SkipSpacesAndComments();
                condition = source.Group(context);

                if (condition == null)
                    break;

                conditions.Add(condition);
                source.SkipSpacesAndComments();
                ident = source.ParseIdent();

                if (ident != null && source.Current == Symbols.RoundBracketOpen)
                {
                    ident = null;
                }
            }
            while (ident != null && ident.Isi(keyword));

            return conditions;
        }
    }
}
