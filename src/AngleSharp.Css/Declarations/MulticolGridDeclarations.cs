#nullable disable
namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using System;
    using static ValueConverters;

    static class ColumnHeightDeclaration
    {
        public static String Name = PropertyNames.ColumnHeight;

        // CSS Multicol 2: the block size of a column box when the columns wrap into rows.
        public static IValueConverter Converter = Or(LengthConverter, Assign(CssKeywords.Auto, CssKeywords.Auto));

        public static ICssValue InitialValue = InitialValues.ColumnHeightDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }

    static class ColumnWrapDeclaration
    {
        public static String Name = PropertyNames.ColumnWrap;

        // CSS Multicol 2: whether column boxes that run out of room start another row of them.
        public static IValueConverter Converter = Or(
            Assign(CssKeywords.Auto, CssKeywords.Auto),
            Assign(CssKeywords.Wrap, CssKeywords.Wrap));

        public static ICssValue InitialValue = InitialValues.ColumnWrapDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }
}
