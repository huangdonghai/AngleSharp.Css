namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using System;
    using static ValueConverters;

    static class TextAutospaceDeclaration
    {
        public static String Name = PropertyNames.TextAutospace;

        public static String[] Shorthands = new[]
        {
            PropertyNames.TextSpacing,
        };

        public static IValueConverter Converter = TextAutospaceConverter;

        public static ICssValue InitialValue = InitialValues.TextAutospaceDecl;

        public static PropertyFlags Flags = PropertyFlags.Inherited;
    }
}
