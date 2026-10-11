#nullable disable
namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Css.Values;
    using AngleSharp.Text;
    using System;
    using static ValueConverters;

    static class FlexDeclaration
    {
        public static String Name = PropertyNames.Flex;

        public static String[] Longhands = new[]
        {
            PropertyNames.FlexGrow,
            PropertyNames.FlexShrink,
            PropertyNames.FlexBasis,
        };

        public static IValueConverter Converter = new FlexAggregator();

        public static ICssValue InitialValue = null;

        public static PropertyFlags Flags = PropertyFlags.Shorthand;

        sealed class FlexAggregator : IValueAggregator, IValueConverter
        {
            private static readonly IValueConverter converter = Or(None, WithAny(
                Or(FlexGrowDeclaration.Converter, VarConverter),
                Or(FlexShrinkDeclaration.Converter, VarConverter),
                Or(FlexBasisDeclaration.Converter, VarConverter)));

            public ICssValue Convert(StringSource source)
            {
                var start = source.Index;
                var converted = converter.Convert(source);

                // `none` is one of the three forms the shorthand takes, and the only one that is not
                // a tuple of the longhands - so it is the only one Split cannot answer, and a
                // shorthand that cannot be split is stored as nothing at all: `flex: none` would
                // vanish, and the item would read at its initials, a shrink of one where none meant
                // zero. The keyword is handed on as the longhands it stands for.
                if (converted is not CssTupleValue &&
                    source.Content.Substring(start, source.Index - start).Trim().ToLowerInvariant() is "none")
                {
                    return new CssTupleValue(new[]
                    {
                        new CssNumberValue(0),
                        new CssNumberValue(0),
                        FlexBasisDeclaration.InitialValue,
                    });
                }

                return converted;
            }

            public ICssValue Merge(ICssValue[] values)
            {
                var grow = values[0];
                var shrink = values[1];
                var basis = values[2];

                if (grow != null || shrink != null || basis != null)
                {
                    return new CssTupleValue(new[] { grow, shrink, basis });
                }

                return null;
            }

            public ICssValue[] Split(ICssValue value)
            {
                var options = value as CssTupleValue;

                if (options != null)
                {
                    return new[]
                    {
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
