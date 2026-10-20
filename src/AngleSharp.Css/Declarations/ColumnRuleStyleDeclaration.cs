namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Css.Converters;
    using System;
    using static ValueConverters;

    static class ColumnRuleStyleDeclaration
    {
        public static String Name = PropertyNames.ColumnRuleStyle;

        public static String[] Shorthands = new[]
        {
            PropertyNames.ColumnRule,
        };

        public static IValueConverter Converter = LineStyleConverter.FromList();

        public static ICssValue InitialValue = InitialValues.ColumnRuleStyleDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }
}
