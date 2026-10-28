#nullable disable
namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Css.Values;
    using AngleSharp.Text;
    using System;
    using static ValueConverters;

    static class PlaceContentDeclaration
    {
        public static String Name = PropertyNames.PlaceContent;

        public static String[] Longhands = new[]
        {
            PropertyNames.AlignContent,
            PropertyNames.JustifyContent,
        };

        public static IValueConverter Converter = new PlaceContentAggregator();

        public static ICssValue InitialValue = null;

        public static PropertyFlags Flags = PropertyFlags.Shorthand;

        sealed class PlaceContentAggregator : IValueAggregator, IValueConverter
        {
            private static readonly IValueConverter converter = WithAny(
                AlignContentConverter,
                JustifyContentConverter);

            public ICssValue Convert(StringSource source)
            {
                return converter.Convert(source);
            }

            public ICssValue Merge(ICssValue[] values)
            {
                var alignContent = values[0];
                var justifyContent = values[1];

                if (alignContent != null || justifyContent != null)
                {
                    return new CssTupleValue(new[] { alignContent, justifyContent });
                }

                return null;
            }

            public ICssValue[] Split(ICssValue value)
            {
                // One value of the shorthand stands for both axes (CSS Box Alignment 3 §5.1), so a
                // split that has only the align side has to hand the same value to justify as well:
                // reading the second item unconditionally dropped `place-content: start` altogether.
                if (value is CssTupleValue options)
                {
                    var items = new System.Collections.Generic.List<ICssValue>(options.Items);

                    if (items.Count > 0)
                    {
                        var align = items[0];
                        var justify = items.Count > 1 ? items[1] : items[0];

                        return new[]
                        {
                            align ?? justify,
                            justify ?? align,
                        };
                    }
                }

                return null;
            }
        }
    }
}
