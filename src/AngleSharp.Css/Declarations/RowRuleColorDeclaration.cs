namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Css.Converters;
    using System;
    using static ValueConverters;

    static class RowRuleColorDeclaration
    {
        public static String Name = PropertyNames.RowRuleColor;

        public static String[] Shorthands = new[]
        {
            PropertyNames.RowRule,
        };

        // css-gaps-1 §4.1: one decoration per row gap, so the value is a list.
        public static IValueConverter Converter = GapRuleList(ColorConverter);

        public static ICssValue InitialValue = InitialValues.RowRuleColorDecl;

        public static PropertyFlags Flags = PropertyFlags.Animatable;
    }
}
