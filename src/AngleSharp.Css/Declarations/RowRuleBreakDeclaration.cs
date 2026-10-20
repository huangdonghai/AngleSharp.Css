namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using System;
    using static ValueConverters;

    static class RowRuleBreakDeclaration
    {
        public static String Name = PropertyNames.RowRuleBreak;

        public static String[] Shorthands = new[]
        {
            PropertyNames.RuleBreak,
        };

        // css-gaps-1 3.2: where a decoration starts and ends at the places two gaps cross.
        public static IValueConverter Converter = Or(
            Assign(CssKeywords.None, CssKeywords.None),
            Assign(CssKeywords.Normal, CssKeywords.Normal),
            Assign(CssKeywords.Intersection, CssKeywords.Intersection));

        public static ICssValue InitialValue = InitialValues.RowRuleBreakDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }
}
