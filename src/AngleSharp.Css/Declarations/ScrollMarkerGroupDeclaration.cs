namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using System;
    using static ValueConverters;

    static class ScrollMarkerGroupDeclaration
    {
        public static String Name = PropertyNames.ScrollMarkerGroup;

        public static IValueConverter Converter = Any;

        public static ICssValue InitialValue = InitialValues.ScrollMarkerGroupDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }
}
