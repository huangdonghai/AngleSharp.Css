namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using System;
    using static ValueConverters;

    static class RuleOverlapDeclaration
    {
        public static String Name = PropertyNames.RuleOverlap;

        // css-gaps-1 §3.5: which of the two perpendicular decorations stands above the other where
        // their gaps cross.
        public static IValueConverter Converter = Or(
            Assign(CssKeywords.RowOverColumn, CssKeywords.RowOverColumn),
            Assign(CssKeywords.ColumnOverRow, CssKeywords.ColumnOverRow));

        public static ICssValue InitialValue = InitialValues.RuleOverlapDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }
}
