#nullable disable
namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Css.Values;
    using AngleSharp.Text;
    using System;
    using static ValueConverters;

    static class GapDeclaration
    {
        public static readonly String Name = PropertyNames.Gap;

        // Order must match GapAggregagtor.Split()'s own [row, col] convention (values[0]/Items[0]
        // is always treated as the row value by both Merge() and Split() below) - this used to list
        // ColumnGap first, pairing it with the row value and vice versa, so `gap: 10px 20px`
        // (row-gap 10px, column-gap 20px per spec) computed row-gap as 20px and column-gap as 10px.
        public static readonly String[] Longhands = new[]
        {
            PropertyNames.RowGap,
            PropertyNames.ColumnGap,
        };

        public static readonly IValueConverter Converter = new GapAggregagtor();

        public static readonly ICssValue InitialValue = null;

        public static readonly PropertyFlags Flags = PropertyFlags.Animatable | PropertyFlags.Shorthand;

        sealed class GapAggregagtor : IValueAggregator, IValueConverter
        {
            private static readonly IValueConverter converter = WithOrder(Or(GapConverter, VarConverter), Or(GapConverter, VarConverter));

            public ICssValue Convert(StringSource source) => converter.Convert(source);

            public ICssValue Merge(ICssValue[] values)
            {
                var row = values[0];
                var col = values[1];

                if (row != null || col != null)
                {
                    return new CssTupleValue(new[] { row, col });
                }

                return null;
            }

            public ICssValue[] Split(ICssValue value)
            {
                if (value is CssTupleValue list)
                {
                    // `gap: <row>` sets the column gap to the row's own value - css-gaps-1 §2.2: "If
                    // <'column-gap'> is omitted, it's set to the same value as <'row-gap'>." Splitting
                    // the tuple as written left the column longhand holding nothing at all, and a
                    // longhand with no value is also what a lone `row-gap` leaves behind - so a reader
                    // could not tell a declaration that meant both axes from one that meant the row
                    // alone, and had to answer both axes with the row's value to stay usable.
                    return new[]
                    {
                        list.Items[0],
                        list.Items[1] ?? list.Items[0],
                    };
                }

                return null;
            }
        }
    }
}
