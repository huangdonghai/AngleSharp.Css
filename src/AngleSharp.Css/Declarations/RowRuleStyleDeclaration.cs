namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Css.Converters;
    using System;
    using static ValueConverters;

    static class RowRuleStyleDeclaration
    {
        public static String Name = PropertyNames.RowRuleStyle;

        public static String[] Shorthands = new[]
        {
            PropertyNames.RowRule,
        };

        // css-gaps-1 §4.2: one decoration per row gap, so the value is a list.
        public static IValueConverter Converter = GapRuleList(LineStyleConverter);

        public static ICssValue InitialValue = InitialValues.RowRuleStyleDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }
}
