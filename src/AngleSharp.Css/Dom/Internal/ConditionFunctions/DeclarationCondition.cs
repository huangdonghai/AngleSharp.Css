namespace AngleSharp.Css.Dom
{
    using AngleSharp.Css.Converters;
    using AngleSharp.Css.Parser;
    using System;
    using System.IO;

    sealed class DeclarationCondition : IConditionFunction
    {
        private readonly String _name;
        private readonly String _value;
        private readonly IBrowsingContext _context;

        public DeclarationCondition(IBrowsingContext context, String name, String value)
        {
            _context = context;
            _name = name;
            _value = value;
        }

        public Boolean Check(IRenderDevice device)
        {
            var factory = _context?.GetService<IDeclarationFactory>() ?? Factory.Declaration;
            var info = factory?.Create(_name);

            if (info == null || Object.Equals(info.Converter, ValueConverters.Any) && !IsCustom())
            {
                // A name the engine has no grammar for is not a feature it can be asked about, and the
                // anything converter is how that is known. A custom property carries that same converter and
                // is a name the cascade does recognize - its value has one grammar, the declaration's own.
                return false;
            }

            var normedValue = Normalize(_value);

            if (IsCustom() || normedValue.IndexOf("var(", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // What a variable will come to is not known until the cascade has been read, so a condition
                // asking about such a value is answered by the value's grammar alone: the sequence of tokens
                // a declaration may carry, with nothing but a group holding what its top level may not. That
                // is the whole of a custom property's grammar too, and it is the one name that may be given
                // nothing at all.
                return normedValue.IsDeclarationValue(allowEmpty: IsCustom());
            }

            return info.Converter.Convert(normedValue) != null;
        }

        Boolean IsCustom() => _name is { Length: > 2 } && _name.StartsWith("--", StringComparison.Ordinal);

        public void ToCss(TextWriter writer, IStyleFormatter formatter) =>
            writer.Write(formatter.Declaration(_name, _value, false));

        private static String Normalize(String value)
        {
            var keyword = CssKeywords.BangImportant;

            if (value.EndsWith(keyword))
            {
                return value.Remove(value.Length - keyword.Length).Trim();
            }

            return value;
        }
    }
}
