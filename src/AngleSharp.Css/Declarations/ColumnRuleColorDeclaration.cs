namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Css.Converters;
    using System;
    using static ValueConverters;

    static class ColumnRuleColorDeclaration
    {
        public static String Name = PropertyNames.ColumnRuleColor;

        public static String[] Shorthands = new[]
        {
            PropertyNames.ColumnRule,
            PropertyNames.Rule,
        };

        public static IValueConverter Converter = GapRuleList(ColorConverter);

        public static ICssValue InitialValue = InitialValues.ColumnRuleColorDecl;

        public static PropertyFlags Flags = PropertyFlags.Animatable;
    }
}
