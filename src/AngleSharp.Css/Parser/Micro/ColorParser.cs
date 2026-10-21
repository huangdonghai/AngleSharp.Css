#nullable disable
namespace AngleSharp.Css.Parser
{
    using AngleSharp.Css.Values;
    using AngleSharp.Text;
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// Represents extensions to for color values.
    /// </summary>
    static class ColorParser
    {
        private static readonly Dictionary<String, Func<StringSource, CssColorValue?>> ColorFunctions = new(StringComparer.OrdinalIgnoreCase)
        {
            { FunctionNames.Rgb, ParseRgba },
            { FunctionNames.Rgba, ParseRgba },
            { FunctionNames.Hsl, ParseHsla },
            { FunctionNames.Hsla, ParseHsla },
            { FunctionNames.Gray, ParseGray },
            { FunctionNames.Color, ParseColorSpace },
            { FunctionNames.Hwb, ParseHwba },
            { FunctionNames.Hwba, ParseHwba },
            { FunctionNames.Lab, ParseLab },
            { FunctionNames.Lch, ParseLch },
            { FunctionNames.Oklab, ParseOklab},
            { FunctionNames.Oklch, ParseOklch },
        };

        /// <summary>
        /// Parses a color value, if any.
        /// </summary>
        public static CssColorValue? ParseColor(this StringSource source)
        {
            var pos = source.Index;
            var result = Start(source);

            if (result == null)
            {
                source.BackTo(pos);
            }

            return result;
        }

        /// <summary>
        /// Parses a the current color value, if any.
        /// </summary>
        public static CssColorValue? ParseCurrentColor(this StringSource source) =>
            source.IsIdentifier(CssKeywords.CurrentColor) ? CssColorValue.CurrentColor : ColorParser.ParseColor(source);

        private static CssColorValue? Start(StringSource source)
        {
            if (source.Current != Symbols.Num)
            {
                var ident = source.ParseIdent();

                if (ident != null)
                {
                    if (source.Current == Symbols.RoundBracketOpen)
                    {
                        if (ColorFunctions.TryGetValue(ident, out var function))
                        {
                            source.SkipCurrentAndSpaces();
                            return function.Invoke(source);
                        }

                        return null;
                    }

                    return CssColorValue.FromName(ident);
                }

                return null;
            }

            return Literal(source);
        }

        private static CssColorValue? Literal(StringSource source)
        {
            var current = source.Next();
            var buffer = StringBuilderPool.Obtain();

            while (current.IsHex())
            {
                buffer.Append(current);
                current = source.Next();
            }

            if (CssColorValue.TryFromHex(buffer.ToPool(), out var result))
            {
                return result;
            }

            return null;
        }

        private static CssColorValue? ParseRgba(StringSource source)
        {
            var pos = source.Index;
            var color = ParseRgbaLegacy(source);

            if (!color.HasValue)
            {
                source.BackTo(pos);
                return ParseRgbaModern(source);
            }

            return color.Value;
        }

        private static CssColorValue? ParseRgbaModern(StringSource source)
        {
            var r = ParseRgbOrNoneComponent(source);
            source.SkipSpacesAndComments();
            var g = ParseRgbOrNoneComponent(source);
            source.SkipSpacesAndComments();
            var b = ParseRgbOrNoneComponent(source);
            source.SkipSpacesAndComments();
            var c = source.Current;
            var a = new Nullable<Double>(1.0);

            if (r != null && g != null && b != null)
            {
                source.SkipCurrentAndSpaces();

                if (c == Symbols.Solidus)
                {
                    a = ParseAlpha(source);
                    source.SkipSpacesAndComments();
                    c = source.Current;
                    source.SkipCurrentAndSpaces();
                }

                if (c == Symbols.RoundBracketClose && a.HasValue)
                {
                    return CssColorValue.FromRgba(r.Value, g.Value, b.Value, a.Value);
                }
            }

            return null;
        }

        private static CssColorValue? ParseRgbaLegacy(StringSource source)
        {
            var r = ParseRgbComponent(source);
            var c1 = source.SkipGetSkip();
            var g = ParseRgbComponent(source);
            var c2 = source.SkipGetSkip();
            var b = ParseRgbComponent(source);
            var c3 = source.SkipGetSkip();

            if (r != null && g != null && b != null)
            {
                if (Check(c3, c1, c2))
                {
                    return CssColorValue.FromRgb(r.Value, g.Value, b.Value);
                }
                else
                {
                    var a = ParseAlpha(source);
                    var f = source.SkipSpacesAndComments();
                    source.SkipCurrentAndSpaces();

                    if (a != null && Check(f, c1, c2, c3))
                    {
                        return CssColorValue.FromRgba(r.Value, g.Value, b.Value, a.Value);
                    }
                }
            }

            return null;
        }

        private static CssColorValue? ParseHsla(StringSource source)
        {
            var pos = source.Index;
            var color = ParseHslaLegacy(source);

            if (!color.HasValue)
            {
                source.BackTo(pos);
                return ParseHslaModern(source);
            }

            return color.Value;
        }

        private static CssColorValue? ParseHslaModern(StringSource source)
        {
            var h = ParseHueOrNone(source);
            source.SkipSpacesAndComments();
            var s = Within(ParsePercentOrNone(source), 0.0, 1.0);
            source.SkipSpacesAndComments();
            var l = Within(ParsePercentOrNone(source), 0.0, 1.0);
            source.SkipSpacesAndComments();
            var c = source.Current;
            var a = new Nullable<Double>(1.0);

            if (h != null && s != null && l != null)
            {
                source.SkipCurrentAndSpaces();

                if (c == Symbols.Solidus)
                {
                    a = ParseAlpha(source);
                    source.SkipSpacesAndComments();
                    c = source.Current;
                    source.SkipCurrentAndSpaces();
                }

                if (c == Symbols.RoundBracketClose && a.HasValue)
                {
                    return CssColorValue.FromHsla(h.Value, s.Value, l.Value, a.Value);
                }
            }

            return null;
        }

        private static CssColorValue? ParseHslaLegacy(StringSource source)
        {
            var h = ParseAngle(source);
            var c1 = source.SkipGetSkip();
            var s = Within(source.ParsePercent(), 0.0, 1.0);
            var c2 = source.SkipGetSkip();
            var l = Within(source.ParsePercent(), 0.0, 1.0);
            var c3 = source.SkipGetSkip();

            if (h != null && s != null && l != null)
            {
                if (Check(c3, c1, c2))
                {
                    return CssColorValue.FromHsl(h.Value, s.Value, l.Value);
                }
                else
                {
                    var a = ParseAlpha(source);
                    var f = source.SkipGetSkip();

                    if (a != null && Check(f, c1, c2, c3))
                    {
                        return CssColorValue.FromHsla(h.Value, s.Value, l.Value, a.Value);
                    }
                }
            }

            return null;
        }

        private static CssColorValue? ParseGray(StringSource source)
        {
            var n = ParseRgbComponent(source);
            var c = source.SkipGetSkip();
            var a = ParseAlpha(source);
            var f = source.SkipGetSkip();

            if (n != null)
            {
                if (c == Symbols.RoundBracketClose)
                {
                    return CssColorValue.FromGray(n.Value);
                }
                else if (a != null && Check(f, c))
                {
                    return CssColorValue.FromGray(n.Value, a.Value);
                }
            }

            return null;
        }

        private static CssColorValue? ParseLab(StringSource source)
        {
            var l = Within(ParseLabComponent(source), 0.0, 100.0);
            source.SkipSpacesAndComments();
            var a = ParseLabComponent(source, percentScale: 125.0 / 100.0);
            source.SkipSpacesAndComments();
            var b = ParseLabComponent(source, percentScale: 125.0 / 100.0);
            source.SkipSpacesAndComments();
            var c = source.Current;
            var alpha = new Nullable<Double>(1.0);

            if (l != null && a != null && b != null)
            {
                source.SkipCurrentAndSpaces();

                if (c == Symbols.Solidus)
                {
                    alpha = ParseAlpha(source);
                    source.SkipSpacesAndComments();
                    c = source.Current;
                    source.SkipCurrentAndSpaces();
                }

                if (c == Symbols.RoundBracketClose && alpha.HasValue)
                {
                    return CssColorValue.FromLab(l.Value, a.Value, b.Value, alpha.Value);
                }
            }

            return null;
        }

        private static CssColorValue? ParseLch(StringSource source)
        {
            var l = Within(ParseLabComponent(source), 0.0, 100.0);
            source.SkipSpacesAndComments();
            var c = AtLeast(ParseLabComponent(source, percentScale: 1.5), 0.0);
            source.SkipSpacesAndComments();
            var h = ParseAngle(source);
            source.SkipSpacesAndComments();
            var chr = source.Current;
            var a = new Nullable<Double>(1.0);

            if (l != null && c != null && h != null)
            {
                source.SkipCurrentAndSpaces();

                if (chr == Symbols.Solidus)
                {
                    a = ParseAlpha(source);
                    source.SkipSpacesAndComments();
                    chr = source.Current;
                    source.SkipCurrentAndSpaces();
                }

                if (chr == Symbols.RoundBracketClose && a.HasValue)
                {
                    return CssColorValue.FromLch(l.Value, c.Value, h.Value, a.Value);
                }
            }

            return null;
        }


        private static CssColorValue? ParseOklab(StringSource source)
        {
            var l = Within(ParseLabComponent(source, numberScale: 100.0), 0.0, 100.0);
            source.SkipSpacesAndComments();
            var a = ParseLabComponent(source, percentScale: 0.004);
            source.SkipSpacesAndComments();
            var b = ParseLabComponent(source, percentScale: 0.004);
            source.SkipSpacesAndComments();
            var c = source.Current;
            var alpha = new Nullable<Double>(1.0);

            if (l != null && a != null && b != null)
            {
                source.SkipCurrentAndSpaces();

                if (c == Symbols.Solidus)
                {
                    alpha = ParseAlpha(source);
                    source.SkipSpacesAndComments();
                    c = source.Current;
                    source.SkipCurrentAndSpaces();
                }

                if (c == Symbols.RoundBracketClose && alpha.HasValue)
                {
                    return CssColorValue.FromOklab(l.Value, a.Value, b.Value, alpha.Value);
                }
            }

            return null;
        }

        private static CssColorValue? ParseOklch(StringSource source)
        {
            var l = Within(ParseLabComponent(source, numberScale: 100.0), 0.0, 100.0);
            source.SkipSpacesAndComments();
            var c = AtLeast(ParseLabComponent(source, percentScale: 0.004), 0.0);
            source.SkipSpacesAndComments();
            var h = ParseAngle(source);
            source.SkipSpacesAndComments();
            var chr = source.Current;
            var a = new Nullable<Double>(1.0);

            if (l != null && c != null && h != null)
            {
                source.SkipCurrentAndSpaces();

                if (chr == Symbols.Solidus)
                {
                    a = ParseAlpha(source);
                    source.SkipSpacesAndComments();
                    chr = source.Current;
                    source.SkipCurrentAndSpaces();
                }

                if (chr == Symbols.RoundBracketClose && a.HasValue)
                {
                    return CssColorValue.FromOklch(l.Value, c.Value, h.Value, a.Value);
                }
            }

            return null;
        }

        private static CssColorValue? ParseHwba(StringSource source)
        {
            var pos = source.Index;
            var color = ParseHwbaLegacy(source);

            if (!color.HasValue)
            {
                source.BackTo(pos);
                return ParseHwbaModern(source);
            }

            return color.Value;
        }

        private static CssColorValue? ParseHwbaModern(StringSource source)
        {
            var h = ParseHueOrNone(source);
            source.SkipSpacesAndComments();
            var w = Within(ParsePercentOrNone(source), 0.0, 1.0);
            source.SkipSpacesAndComments();
            var b = Within(ParsePercentOrNone(source), 0.0, 1.0);
            source.SkipSpacesAndComments();
            var c = source.Current;
            var a = new Nullable<Double>(1.0);

            if (h != null && w != null && b != null)
            {
                source.SkipCurrentAndSpaces();

                if (c == Symbols.Solidus)
                {
                    a = ParseAlpha(source);
                    source.SkipSpacesAndComments();
                    c = source.Current;
                    source.SkipCurrentAndSpaces();
                }

                if (c == Symbols.RoundBracketClose && a.HasValue)
                {
                    return CssColorValue.FromHwba(h.Value, w.Value, b.Value, a.Value);
                }
            }

            return null;
        }

        private static CssColorValue? ParseHwbaLegacy(StringSource source)
        {
            var h = ParseAngle(source);
            var c1 = source.SkipGetSkip();
            var s = Within(source.ParsePercent(), 0.0, 1.0);
            var c2 = source.SkipGetSkip();
            var l = Within(source.ParsePercent(), 0.0, 1.0);
            var c3 = source.SkipGetSkip();

            if (h != null && s != null && l != null)
            {
                if (Check(c3, c1, c2))
                {
                    return CssColorValue.FromHwb(h.Value, s.Value, l.Value);
                }
                else
                {
                    var a = ParseAlpha(source);
                    var f = source.SkipSpacesAndComments();
                    source.SkipCurrentAndSpaces();

                    if (a != null && Check(f, c1, c2, c3))
                    {
                        return CssColorValue.FromHwba(h.Value, s.Value, l.Value, a.Value);
                    }
                }
            }

            return null;
        }

        // color() names its space first and then the three components of that space, each a number or a
        // percentage of it - where 100% is the full one - or none, with an alpha behind a solidus.
        private static CssColorValue? ParseColorSpace(StringSource source)
        {
            var space = source.ParseIdent();

            if (space == null)
            {
                return null;
            }

            source.SkipSpacesAndComments();

            var x = ParseColorComponent(source);
            source.SkipSpacesAndComments();
            var y = ParseColorComponent(source);
            source.SkipSpacesAndComments();
            var z = ParseColorComponent(source);
            source.SkipSpacesAndComments();

            var c = source.Current;
            var alpha = new Nullable<Double>(1.0);

            if (x != null && y != null && z != null)
            {
                source.SkipCurrentAndSpaces();

                if (c == Symbols.Solidus)
                {
                    alpha = ParseAlpha(source);
                    source.SkipSpacesAndComments();
                    c = source.Current;
                    source.SkipCurrentAndSpaces();
                }

                if (c == Symbols.RoundBracketClose && alpha.HasValue)
                {
                    return CssColorValue.FromColor(space, x.Value, y.Value, z.Value, alpha.Value);
                }
            }

            return null;
        }

        private static Double? ParseColorComponent(StringSource source)
        {
            var pos = source.Index;
            var unit = source.ParseUnit();

            if (unit != null && Double.TryParse(unit.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (unit.Dimension == String.Empty)
                {
                    return value;
                }
                else if (unit.Dimension == "%")
                {
                    return value * 0.01;
                }
            }

            source.BackTo(pos);

            return source.IsIdentifier(CssKeywords.None) ? 0 : default(Double?);
        }

        // A percentage of one of these components is a share of that component's own reference range,
        // which CSS Color 4 10's tables give per function and per argument, and the lightness of the two
        // families is written on two different scales: 100 of Lab, 1.0 of Oklab.
        private static Double? ParseLabComponent(StringSource source, Double numberScale = 1.0, Double percentScale = 1.0)
        {
            var pos = source.Index;
            var unit = source.ParseUnit();

            if (unit == null)
            {
                source.BackTo(pos);

                if (source.IsIdentifier(CssKeywords.None))
                {
                    return 0;
                }

                return null;
            }

            if ((unit.Dimension == String.Empty || unit.Dimension == "%") && Double.TryParse(unit.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                return unit.Dimension == String.Empty ? value * numberScale : value * percentScale;
            }

            return null;
        }

        private static Byte? ParseRgbOrNoneComponent(StringSource source)
        {
            var pos = source.Index;
            var value = ParseRgbComponent(source);

            if (value.HasValue)
            {
                return value;
            }

            source.BackTo(pos);

            if (source.IsIdentifier(CssKeywords.None))
            {
                return 0;
            }

            return null;
        }

        private static Byte? ParseRgbComponent(StringSource source)
        {
            var unit = source.ParseUnit();

            if (unit == null)
            {
                return null;
            }

            if (unit.Dimension == String.Empty && Double.TryParse(unit.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var num))
            {
                return ToChannel(num);
            }

            if (unit.Dimension == "%" && Double.TryParse(unit.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
            {
                return ToChannel((255.0 * val) / 100.0);
            }

            return null;
        }

        // A channel is a whole number, and CSS Values 3 4.3 says which whole: the nearest one, a value
        // standing exactly between two going up. Both the number and the percentage reach this, so
        // rgb(0 128.5 0) and rgb(0 70% 0) answer 129 and 179 as Chromium does.
        private static Byte ToChannel(Double value) =>
            (Byte)Math.Round(Math.Max(Math.Min(value, 255.0), 0.0), MidpointRounding.AwayFromZero);

        private static Double? ParseHueOrNone(StringSource source)
        {
            var pos = source.Index;
            var value = ParseAngle(source);

            if (value.HasValue)
            {
                return value;
            }

            source.BackTo(pos);

            return source.IsIdentifier(CssKeywords.None) ? 0 : default(Double?);
        }

        private static Double? ParsePercentOrNone(StringSource source)
        {
            var pos = source.Index;
            var value = source.ParsePercent();

            if (value.HasValue)
            {
                return value;
            }

            source.BackTo(pos);

            return source.IsIdentifier(CssKeywords.None) ? 0 : default(Double?);
        }

        // The chapter takes these back into their range where the value is read rather than after the
        // conversion, which is what keeps a saturation of nothing a grey rather than a colour upside down.
        private static Double? Within(Double? value, Double lowest, Double highest) =>
            value is { } found ? Math.Max(Math.Min(found, highest), lowest) : value;

        private static Double? AtLeast(Double? value, Double lowest) =>
            value is { } found ? Math.Max(found, lowest) : value;

        private static Double? ParseAlpha(StringSource source)
        {
            var pos = source.Index;
            var unit = source.ParseUnit();

            if (unit != null &&
                Double.TryParse(unit.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                if (unit.Dimension == "%")
                {
                    return value * 0.01;
                }
                else if (unit.Dimension == String.Empty)
                {
                    return Math.Max(Math.Min(value, 1.0), 0.0);
                }
            }

            source.BackTo(pos);

            // `none` is an alpha of nothing at all, which is the one reading the modern syntax gives it.
            return source.IsIdentifier(CssKeywords.None) ? 0 : default(Double?);
        }

        private static Double? ParseAngle(StringSource source)
        {
            var unit = source.ParseUnit();

            if (unit != null &&
                Double.TryParse(unit.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                var dim = CssAngleValue.Unit.Deg;

                if (unit.Dimension == String.Empty || (dim = CssAngleValue.GetUnit(unit.Dimension)) != CssAngleValue.Unit.None)
                {
                    var angle = new CssAngleValue(value, dim);
                    return angle.ToTurns();
                }
            }

            return null;
        }

        private static Boolean Check(Char closingBracket, Char firstComma = Symbols.Comma, Char secondComma = Symbols.Comma, Char thirdComma = Symbols.Comma) =>
            closingBracket == Symbols.RoundBracketClose &&
            firstComma == Symbols.Comma &&
            secondComma == Symbols.Comma &&
            thirdComma == Symbols.Comma;
    }
}
