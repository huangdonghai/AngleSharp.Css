#nullable disable
namespace AngleSharp.Css.Parser
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Css.Values;
    using AngleSharp.Text;
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// Represents extensions to for calc values.
    /// </summary>
    public static class CalcParser
    {
        /// <summary>
        /// The functions that take a list of sums and answer the least or the greatest of them.
        /// </summary>
        private static readonly String[] MathFunctions = new[] { "min", "max", "clamp" };
        /// <summary>
        /// Parses a calc value, if any.
        /// </summary>
        public static CssCalcValue ParseCalc(this StringSource source)
        {
            var pos = source.Index;

            if (source.IsFunction(FunctionNames.Calc))
            {
                var expression = source.ParseExpression();

                if (expression != null)
                {
                    return new CssCalcValue(expression);
                }
            }
            else if (source.ParseMathFunction() is { } math)
            {
                // The math functions are arithmetic of the same kind, so they ride the calc converter:
                // every property that takes a calc takes min(), max() and clamp() with it. The value
                // keeps its own name when it is written back, so `height: max(100px, 50px)` reads as
                // that rather than as a calc of it.
                return new CssCalcValue(math);
            }

            source.BackTo(pos);
            return null;
        }

        /// <summary>
        /// Parses a min(), max() or clamp() call, if any. Each argument is a sum of the shape a calc
        /// expression takes, since the parentheses around a calc are left out inside a math function.
        /// </summary>
        public static ICssValue ParseMathFunction(this StringSource source)
        {
            var pos = source.Index;

            foreach (var name in MathFunctions)
            {
                if (!source.IsFunction(name))
                {
                    continue;
                }

                var arguments = new List<ICssValue>();

                while (true)
                {
                    var argument = source.ParseAddExpression();

                    if (argument == null)
                    {
                        break;
                    }

                    arguments.Add(argument);

                    if (source.Current == Symbols.Comma)
                    {
                        source.SkipCurrentAndSpaces();
                        continue;
                    }

                    if (source.Current == Symbols.RoundBracketClose)
                    {
                        source.SkipCurrentAndSpaces();

                        if (arguments.Count > 0)
                        {
                            return new CssMathFunctionValue(name, arguments.ToArray());
                        }

                        break;
                    }

                    break;
                }
            }

            source.BackTo(pos);
            return null;
        }

        private static ICssValue ParseExpression(this StringSource source)
        {
            var expression = source.ParseAddExpression();

            if (expression != null && source.Current == Symbols.RoundBracketClose)
            {
                source.SkipCurrentAndSpaces();
                return expression;
            }

            return null;
        }

        private static ICssValue ParseAddExpression(this StringSource source)
        {
            var left = ParseMulExpression(source);

            while (left != null && (source.Current == Symbols.Plus || source.Current == Symbols.Minus))
            {
                var add = source.Current == Symbols.Plus;
                source.SkipCurrentAndSpaces();
                var right = ParseMulExpression(source);

                if (right == null)
                {
                    return null;
                }

                left = add ?
                    new CssCalcAddExpression(left, right) :
                    (ICssValue)new CssCalcSubExpression(left, right);
            }

            return left;
        }

        private static ICssValue ParseMulExpression(this StringSource source)
        {
            var left = ParseBracketExpression(source);

            while (left != null && (source.Current == Symbols.Asterisk || source.Current == Symbols.Solidus))
            {
                var mul = source.Current == Symbols.Asterisk;
                source.SkipCurrentAndSpaces();
                var right = ParseBracketExpression(source);

                if (right == null)
                {
                    return null;
                }

                left = mul ?
                    new CssCalcMulExpression(left, right) :
                    (ICssValue)new CssCalcDivExpression(left, right);
            }

            return left;
        }

        private static ICssValue ParseBracketExpression(this StringSource source)
        {
            if (source.Current == Symbols.RoundBracketOpen)
            {
                source.SkipCurrentAndSpaces();
                var content = source.ParseExpression();
                return new CssCalcBracketExpression(content);
            }
            else if (source.IsFunction(FunctionNames.Calc))
            {
                var content = source.ParseExpression();
                return new CssCalcBracketExpression(content);
            }

            var math = source.ParseMathFunction();

            if (math != null)
            {
                return math;
            }

            return source.ParseAtomicExpression();
        }
        
        /// <summary>
        /// Parses a unit value into a ICssValue.
        /// </summary>
        public static ICssValue ParseAtomicExpression(this StringSource source)
        {
            var unit = source.ParseUnit();

            if (unit != null && Double.TryParse(unit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                if (String.IsNullOrEmpty(unit.Dimension))
                {
                    source.SkipSpacesAndComments();
                    return new CssLengthValue(result, CssLengthValue.Unit.None);
                }
                else if (CssLengthValue.GetUnit(unit.Dimension) != CssLengthValue.Unit.None)
                {
                    source.SkipSpacesAndComments();
                    return new CssLengthValue(result, CssLengthValue.GetUnit(unit.Dimension));
                }
                else if (CssTimeValue.GetUnit(unit.Dimension) != CssTimeValue.Unit.None)
                {
                    source.SkipSpacesAndComments();
                    return new CssTimeValue(result, CssTimeValue.GetUnit(unit.Dimension));
                }
                else if (CssAngleValue.GetUnit(unit.Dimension) != CssAngleValue.Unit.None)
                {
                    source.SkipSpacesAndComments();
                    return new CssAngleValue(result, CssAngleValue.GetUnit(unit.Dimension));
                }
                else if (CssFrequencyValue.GetUnit(unit.Dimension) != CssFrequencyValue.Unit.None)
                {
                    source.SkipSpacesAndComments();
                    return new CssFrequencyValue(result, CssFrequencyValue.GetUnit(unit.Dimension));
                }
                else if (CssResolutionValue.GetUnit(unit.Dimension) != CssResolutionValue.Unit.None)
                {
                    source.SkipSpacesAndComments();
                    return new CssResolutionValue(result, CssResolutionValue.GetUnit(unit.Dimension));
                }
            }

            return null;
        }
    }
}
