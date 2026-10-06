namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using System;
    using static ValueConverters;

    static class TextCombineUprightDeclaration
    {
        public static String Name = PropertyNames.TextCombineUpright;

        public static IValueConverter Converter = Or(
            Assign("none", "none"),
            Assign("all", "all"));

        public static ICssValue InitialValue = InitialValues.TextCombineUprightDecl;

        public static PropertyFlags Flags = PropertyFlags.Inherited;
    }
}
