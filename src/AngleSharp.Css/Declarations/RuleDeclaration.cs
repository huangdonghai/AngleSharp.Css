#nullable disable
namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Css.Parser;
    using AngleSharp.Css.Values;
    using AngleSharp.Text;
    using System;

    static class RuleDeclaration
    {
        public static String Name = PropertyNames.Rule;

        public static IValueConverter Converter = new RuleAggregator();

        public static ICssValue InitialValue = null;

        public static PropertyFlags Flags = PropertyFlags.Animatable | PropertyFlags.Shorthand;

        // css-gaps-1 4.7: `rule` stands for both axes' rule color, width and style. The two axes are
        // listed in the order the longhand declarations read them, and the triple the page wrote is
        // handed to each axis alike.
        public static String[] Longhands = new[]
        {
            PropertyNames.ColumnRuleColor,
            PropertyNames.ColumnRuleWidth,
            PropertyNames.ColumnRuleStyle,
            PropertyNames.RowRuleColor,
            PropertyNames.RowRuleWidth,
            PropertyNames.RowRuleStyle,
        };

        sealed class RuleValueConverter : IValueConverter
        {
            public ICssValue Convert(StringSource source)
            {
                var color = default(ICssValue);
                var width = default(ICssValue);
                var style = default(ICssValue);
                var pos = 0;

                do
                {
                    pos = source.Index;

                    if (color == null)
                    {
                        color = ColorParser.ParseColor(source);
                        source.SkipSpacesAndComments();
                    }

                    if (width == null)
                    {
                        width = source.ParseLineWidth();
                        source.SkipSpacesAndComments();
                    }

                    if (style == null)
                    {
                        style = source.ParseConstant(Map.LineStyles);
                        source.SkipSpacesAndComments();
                    }
                }
                while (pos != source.Index);

                return new CssTupleValue(new[] { color, width, style });
            }
        }

        sealed class RuleAggregator : IValueAggregator, IValueConverter
        {
            private static readonly IValueConverter converter = new RuleValueConverter();

            public ICssValue Convert(StringSource source)
            {
                return converter.Convert(source);
            }

            public ICssValue Merge(ICssValue[] values)
            {
                var color = values[0];
                var width = values[1];
                var style = values[2];

                if (color != null || width != null || style != null)
                {
                    return new CssTupleValue(new[] { color, width, style });
                }

                return null;
            }

            public ICssValue[] Split(ICssValue value)
            {
                if (value is CssTupleValue options && options.Items.Length == 3)
                {
                    // Both axes take the same triple, which is what the shorthand says.
                    return new[]
                    {
                        options.Items[0],
                        options.Items[1],
                        options.Items[2],
                        options.Items[0],
                        options.Items[1],
                        options.Items[2],
                    };
                }

                return null;
            }
        }
    }
}
