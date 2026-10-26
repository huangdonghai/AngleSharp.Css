namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using System;
    using static ValueConverters;

    static class CornerShapeDeclaration
    {
        public static String Name = PropertyNames.CornerShape;

        public static IValueConverter Converter = Any;

        public static ICssValue InitialValue = InitialValues.CornerShapeDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }
}
