#nullable disable
namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Text;
    using System;
    using static ValueConverters;

    static class RuleBreakDeclaration
    {
        public static String Name = PropertyNames.RuleBreak;

        // The shorthand has the grammar of one axis and gives the same value to both, which is what
        // css-gaps-1 3.2 says it is for: where a decoration starts and ends at a crossing.
        public static String[] Longhands = new[]
        {
            PropertyNames.ColumnRuleBreak,
            PropertyNames.RowRuleBreak,
        };

        public static IValueConverter Converter = new RuleBreakAggregator();

        public static ICssValue InitialValue = null;

        public static PropertyFlags Flags = PropertyFlags.Shorthand;

        sealed class RuleBreakAggregator : IValueAggregator, IValueConverter
        {
            private static readonly IValueConverter converter = Or(
                Assign(CssKeywords.None, CssKeywords.None),
                Assign(CssKeywords.Normal, CssKeywords.Normal),
                Assign(CssKeywords.Intersection, CssKeywords.Intersection));

            public ICssValue Convert(StringSource source) => converter.Convert(source);

            public ICssValue Merge(ICssValue[] values) => values[0] is { } column ? column : values[1];

            public ICssValue[] Split(ICssValue value) => value is null ? null : new[] { value, value };
        }
    }
}
