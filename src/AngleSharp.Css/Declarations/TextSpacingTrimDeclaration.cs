namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using System;
    using static ValueConverters;

    static class TextSpacingTrimDeclaration
    {
        public static String Name = PropertyNames.TextSpacingTrim;

        public static String[] Shorthands = new[]
        {
            PropertyNames.TextSpacing,
        };

        public static IValueConverter Converter = TextSpacingTrimConverter;

        public static ICssValue InitialValue = InitialValues.TextSpacingTrimDecl;

        public static PropertyFlags Flags = PropertyFlags.Inherited;
    }
}
