#nullable disable
namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using System;
    using static ValueConverters;

    static class TextSpacingDeclaration
    {
        public static String Name = PropertyNames.TextSpacing;

        public static IValueConverter Converter = TextSpacingConverter;

        public static ICssValue InitialValue = null;

        public static PropertyFlags Flags = PropertyFlags.Shorthand;

        public static String[] Longhands = new[]
        {
            PropertyNames.TextSpacingTrim,
            PropertyNames.TextAutospace,
        };
    }
}
