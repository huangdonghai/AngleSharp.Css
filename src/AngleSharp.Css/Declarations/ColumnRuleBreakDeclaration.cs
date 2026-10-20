namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using System;
    using static ValueConverters;

    static class ColumnRuleBreakDeclaration
    {
        public static String Name = PropertyNames.ColumnRuleBreak;

        public static String[] Shorthands = new[]
        {
            PropertyNames.RuleBreak,
        };

        // css-gaps-1 3.2: where a decoration starts and ends at the places two gaps cross.
        public static IValueConverter Converter = Or(
            Assign(CssKeywords.None, CssKeywords.None),
            Assign(CssKeywords.Normal, CssKeywords.Normal),
            Assign(CssKeywords.Intersection, CssKeywords.Intersection));

        public static ICssValue InitialValue = InitialValues.ColumnRuleBreakDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }
}
