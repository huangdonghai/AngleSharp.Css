namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Css.Converters;
    using System;
    using static ValueConverters;

    static class ColumnRuleWidthDeclaration
    {
        public static String Name = PropertyNames.ColumnRuleWidth;

        public static String[] Shorthands = new[]
        {
            PropertyNames.ColumnRule,
        };

        // CSS Gap Decorations takes a comma-separated list, one decoration per gap (css-gaps-1 §4.3,
        // which supersedes the single value CSS Multicol 1 gave this property).
        public static IValueConverter Converter = GapRuleList(LineWidthConverter);

        public static ICssValue InitialValue = InitialValues.ColumnRuleWidthDecl;

        public static PropertyFlags Flags = PropertyFlags.Animatable;
    }
}
