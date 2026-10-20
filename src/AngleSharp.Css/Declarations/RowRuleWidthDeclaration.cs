namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Css.Converters;
    using System;
    using static ValueConverters;

    static class RowRuleWidthDeclaration
    {
        public static String Name = PropertyNames.RowRuleWidth;

        public static String[] Shorthands = new[]
        {
            PropertyNames.RowRule,
        };

        // css-gaps-1 §4.3: one decoration per row gap, so the value is a list.
        public static IValueConverter Converter = LineWidthConverter.FromList();

        public static ICssValue InitialValue = InitialValues.RowRuleWidthDecl;

        public static PropertyFlags Flags = PropertyFlags.Animatable;
    }
}
