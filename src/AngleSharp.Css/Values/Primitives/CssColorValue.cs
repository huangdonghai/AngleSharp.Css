#nullable disable
namespace AngleSharp.Css.Values
{
    using AngleSharp.Css;
    using AngleSharp.Css.Dom;
    using AngleSharp.Text;
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents a color value.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1, CharSet = CharSet.Unicode)]
    public struct CssColorValue : IEquatable<CssColorValue>, IComparable<CssColorValue>, ICssPrimitiveValue
    {
        #region Basic colors

        /// <summary>
        /// The color #000000.
        /// </summary>
        public static readonly CssColorValue Black = new(0, 0, 0);

        /// <summary>
        /// The color #FFFFFF.
        /// </summary>
        public static readonly CssColorValue White = new(255, 255, 255);

        /// <summary>
        /// The color #FF0000.
        /// </summary>
        public static readonly CssColorValue Red = new(255, 0, 0);

        /// <summary>
        /// The color #FF00FF.
        /// </summary>
        public static readonly CssColorValue Magenta = new(255, 0, 255);

        /// <summary>
        /// The color #008000.
        /// </summary>
        public static readonly CssColorValue Green = new(0, 128, 0);

        /// <summary>
        /// The color #00FF00.
        /// </summary>
        public static readonly CssColorValue PureGreen = new(0, 255, 0);

        /// <summary>
        /// The color #0000FF.
        /// </summary>
        public static readonly CssColorValue Blue = new(0, 0, 255);

        /// <summary>
        /// The color #00000000.
        /// </summary>
        public static readonly CssColorValue Transparent = new(0, 0, 0, 0);

        /// <summary>
        /// The color #00010203.
        /// </summary>
        public static readonly CssColorValue CurrentColor = new(0, 1, 2, 3);

        /// <summary>
        /// The color #30201000.
        /// </summary>
        public static readonly CssColorValue InvertedColor = new(48, 32, 16, 0);

        #endregion

        #region Fields

        [FieldOffset(0)]
        private readonly Byte _alpha;
        [FieldOffset(1)]
        private readonly Byte _red;
        [FieldOffset(2)]
        private readonly Byte _green;
        [FieldOffset(3)]
        private readonly Byte _blue;
        [FieldOffset(0)]
        private readonly Int32 _hashcode;

        #endregion

        #region ctor

        /// <summary>
        /// Creates a CSS color type without any transparency (alpha = 100%).
        /// </summary>
        /// <param name="r">The red value.</param>
        /// <param name="g">The green value.</param>
        /// <param name="b">The blue value.</param>
        public CssColorValue(Byte r, Byte g, Byte b)
        {
            _hashcode = 0;
            _alpha = 255;
            _red = r;
            _blue = b;
            _green = g;
        }

        /// <summary>
        /// Creates a CSS color type.
        /// </summary>
        /// <param name="r">The red value.</param>
        /// <param name="g">The green value.</param>
        /// <param name="b">The blue value.</param>
        /// <param name="a">The alpha value.</param>
        public CssColorValue(Byte r, Byte g, Byte b, Byte a)
        {
            _hashcode = 0;
            _alpha = a;
            _red = r;
            _blue = b;
            _green = g;
        }

        #endregion

        #region Static constructors

        /// <summary>
        /// Returns the color from the given primitives.
        /// </summary>
        /// <param name="r">The value for red [0,255].</param>
        /// <param name="g">The value for green [0,255].</param>
        /// <param name="b">The value for blue [0,255].</param>
        /// <param name="a">The value for alpha [0,1].</param>
        /// <returns>The CSS color value.</returns>
        public static CssColorValue FromRgba(Byte r, Byte g, Byte b, Double a) =>
            new(r, g, b, Normalize(a));

        /// <summary>
        /// Returns the color from the given primitives.
        /// </summary>
        /// <param name="r">The value for red [0,1].</param>
        /// <param name="g">The value for green [0,1].</param>
        /// <param name="b">The value for blue [0,1].</param>
        /// <param name="a">The value for alpha [0,1].</param>
        /// <returns>The CSS color value.</returns>
        public static CssColorValue FromRgba(Double r, Double g, Double b, Double a) =>
            new(Normalize(r), Normalize(g), Normalize(b), Normalize(a));

        /// <summary>
        /// Returns the gray color from the given value.
        /// </summary>
        /// <param name="number">The value for each component [0,255].</param>
        /// <param name="alpha">The value for alpha [0,1].</param>
        /// <returns>The CSS color value.</returns>
        public static CssColorValue FromGray(Byte number, Double alpha = 1.0) =>
            new(number, number, number, Normalize(alpha));

        /// <summary>
        /// Returns the gray color from the given value.
        /// </summary>
        /// <param name="value">The value for each component [0,1].</param>
        /// <param name="alpha">The value for alpha [0,1].</param>
        /// <returns>The CSS color value.</returns>
        public static CssColorValue FromGray(Double value, Double alpha = 1.0) =>
            FromGray(Normalize(value), alpha);

        /// <summary>
        /// Returns the color with the given name.
        /// </summary>
        /// <param name="name">The name of the color.</param>
        /// <returns>The CSS color value.</returns>
        public static CssColorValue? FromName(String name) => CssColors.GetColor(name);

        /// <summary>
        /// Returns the color from the given primitives without any alpha.
        /// </summary>
        /// <param name="r">The value for red [0,255].</param>
        /// <param name="g">The value for green [0,255].</param>
        /// <param name="b">The value for blue [0,255].</param>
        /// <returns>The CSS color value.</returns>
        public static CssColorValue FromRgb(Byte r, Byte g, Byte b) => new(r, g, b);

        /// <summary>
        /// Returns the color that represents the given Lab values.
        /// </summary>
        /// <param name="l">The value for perceptual lightness [0,100].</param>
        /// <param name="a">The value for the first axis [-100,100].</param>
        /// <param name="b">The value for the second axis [-100,100].</param>
        /// <param name="alpha">The alpha value [0,1].</param>
        /// <returns>The CSS color value.</returns>
        public static CssColorValue? FromLab(Double l, Double a, Double b, Double alpha)
        {
            var fy = (l + 16.0) / 116.0;
            var fx = a / 500.0 + fy;
            var fz = fy - b / 200.0;
            var y = Grow(fy);
            var x = Grow(fx) * (0.3457 / 0.3585);
            var z = Grow(fz) * ((1.0 - 0.3457 - 0.3585) / 0.3585);
            return FromXyz50(x, y, z, alpha);
        }

        private static CssColorValue? FromXyz50(Double x, Double y, Double z, Double alpha)
        {
            var r = x * 3.1341359569958707 - y * 1.6173863321612538 - z * 0.4906619460083532;
            var g = x * -0.978795502912089 + y * 1.916254567259524 + z * 0.03344273116131949;
            var b = x * 0.07195537988411677 - y * 0.2289768264158322 + z * 1.405386058324125;
            return FromLrgb(r, g, b, alpha);
        }

        private static CssColorValue? FromLrgb(Double red, Double green, Double blue, Double alpha)
        {
            var r = Normalize(Project(red));
            var g = Normalize(Project(green));
            var b = Normalize(Project(blue));
            var a = Normalize(alpha);
            return new CssColorValue(r, g, b, a);
        }

        /// <summary>
        /// Returns the color that represents the given Lch values.
        /// </summary>
        /// <param name="l">The value for perceptual lightness [0,100].</param>
        /// <param name="c">The value for the axis [0,150].</param>
        /// <param name="h">The value for the angle to the axis [0,1].</param>
        /// <param name="alpha">The alpha value [0,1].</param>
        /// <returns>The CSS color value.</returns>
        public static CssColorValue? FromLch(Double l, Double c, Double h, Double alpha)
        {
            var angle = h * 2.0 * Math.PI;
            var a = c != 0 ? c * Math.Cos(angle) : 0.0;
            var b = c != 0 ? c * Math.Sin(angle) : 0.0;
            return FromLab(l, a, b, alpha);
        }

        /// <summary>
        /// Returns the color that represents the given Oklab values.
        /// </summary>
        /// <param name="l">The value for perceptual lightness [0,100].</param>
        /// <param name="a">The value for the first axis [-0.4,0.4].</param>
        /// <param name="b">The value for the second axis [-0.4,0.4].</param>
        /// <param name="alpha">The alpha value [0,1].</param>
        /// <returns>The CSS color value.</returns>
        public static CssColorValue? FromOklab(Double l, Double a, Double b, Double alpha)
        {
            // normalize to 0..1
            l *= 0.01;
            var L = Math.Pow(l * 0.99999999845051981432 + 0.39633779217376785678 * a + 0.21580375806075880339 * b, 3);
            var M = Math.Pow(l * 1.0000000088817607767 - 0.1055613423236563494 * a - 0.063854174771705903402 * b, 3);
            var S = Math.Pow(l * 1.0000000546724109177 - 0.089484182094965759684 * a - 1.2914855378640917399 * b, 3);
            var red = +4.076741661347994 * L - 3.307711590408193 * M + 0.230969928729428 * S;
            var green = -1.2684380040921763 * L + 2.6097574006633715 * M - 0.3413193963102197 * S;
            var blue = -0.004196086541837188 * L - 0.7034186144594493 * M + 1.7076147009309444 * S;
            return FromLrgb(red, green, blue, alpha);
        }

        /// <summary>
        /// Returns the color that represents the given Oklch values.
        /// </summary>
        /// <param name="l">The value for perceptual lightness [0,100].</param>
        /// <param name="c">The value for the axis [0,0.4].</param>
        /// <param name="h">The value for the angle to the axis [0,1].</param>
        /// <param name="alpha">The alpha value [0,1].</param>
        /// <returns>The CSS color value.</returns>
        public static CssColorValue? FromOklch(Double l, Double c, Double h, Double alpha)
        {
            var angle = h * 2.0 * Math.PI;
            var a = c != 0 ? c * Math.Cos(angle) : 0.0;
            var b = c != 0 ? c * Math.Sin(angle) : 0.0;
            return FromOklab(l, a, b, alpha);
        }

        /// <summary>
        /// Returns the color from the given hex string.
        /// </summary>
        /// <param name="color">The hex string like fff or abc123 or AA126B etc.</param>
        /// <returns>The CSS color value.</returns>
        public static CssColorValue FromHex(String color)
        {
            int r = 0, g = 0, b = 0, a = 255;

            switch (color.Length)
            {
                case 4:
                    a = 17 * color[3].FromHex();
                    goto case 3;
                case 3:
                    r = 17 * color[0].FromHex();
                    g = 17 * color[1].FromHex();
                    b = 17 * color[2].FromHex();
                    break;
                case 8:
                    a = 16 * color[6].FromHex() + color[7].FromHex();
                    goto case 6;
                case 6:
                    r = 16 * color[0].FromHex() + color[1].FromHex();
                    g = 16 * color[2].FromHex() + color[3].FromHex();
                    b = 16 * color[4].FromHex() + color[5].FromHex();
                    break;
            }

            return new CssColorValue((Byte)r, (Byte)g, (Byte)b, (Byte)a);
        }

        /// <summary>
        /// Returns the color from the given hex string if it can be converted, otherwise
        /// the color is not set.
        /// </summary>
        /// <param name="color">The hexadecimal representation of the color.</param>
        /// <param name="value">The color value to be created.</param>
        /// <returns>The status if the string can be converted.</returns>
        public static Boolean TryFromHex(String color, out CssColorValue value)
        {
            if (color.Length == 6 || color.Length == 3 || color.Length == 8 || color.Length == 4)
            {
                for (var i = 0; i < color.Length; i++)
                {
                    if (!color[i].IsHex())
                        goto fail;
                }

                value = FromHex(color);
                return true;
            }

            fail:
            value = new CssColorValue();
            return false;
        }

        /// <summary>
        /// Returns the color of non-CSS colors in a special IE notation known
        /// as "flex hex". Computes the part without the hash and possible color
        /// names. More information can be found at:
        /// http://scrappy-do.blogspot.de/2004/08/little-rant-about-microsoft-internet.html
        /// </summary>
        /// <param name="color">The color string to evaluate.</param>
        /// <returns>The color for the color string.</returns>
        public static CssColorValue FromFlexHex(String color)
        {
            var length = Math.Max(color.Length, 3);
            var remaining = length % 3;

            if (remaining != 0)
            {
                length += 3 - remaining;
            }

            var n = length / 3;
            var d = Math.Min(2, n);
            var s = Math.Max(n - 8, 0);
            var chars = new Char[length];

            for (var i = 0; i < color.Length; i++)
            {
                chars[i] = color[i].IsHex() ? color[i] : '0';
            }

            for (var i = color.Length; i < length; i++)
            {
                chars[i] = '0';
            }

            if (d == 1)
            {
                var r = chars[0 * n + s].FromHex();
                var g = chars[1 * n + s].FromHex();
                var b = chars[2 * n + s].FromHex();
                return new CssColorValue((Byte)r, (Byte)g, (Byte)b);
            }
            else
            {
                var r = 16 * chars[0 * n + s].FromHex() + chars[0 * n + s + 1].FromHex();
                var g = 16 * chars[1 * n + s].FromHex() + chars[1 * n + s + 1].FromHex();
                var b = 16 * chars[2 * n + s].FromHex() + chars[2 * n + s + 1].FromHex();
                return new CssColorValue((Byte)r, (Byte)g, (Byte)b);
            }
        }

        /// <summary>
        /// Returns the color that represents the given HSL values.
        /// </summary>
        /// <param name="h">The color angle [0,1].</param>
        /// <param name="s">The saturation [0,1].</param>
        /// <param name="l">The light value [0,1].</param>
        /// <returns>The CSS color.</returns>
        public static CssColorValue FromHsl(Double h, Double s, Double l) => FromHsla(h, s, l, 1.0);

        /// <summary>
        /// Returns the color that represents the given HSL values.
        /// </summary>
        /// <param name="h">The color angle [0,1].</param>
        /// <param name="s">The saturation [0,1].</param>
        /// <param name="l">The light value [0,1].</param>
        /// <param name="alpha">The alpha value [0,1].</param>
        /// <returns>The CSS color.</returns>
        public static CssColorValue FromHsla(Double h, Double s, Double l, Double alpha)
        {
            const Double third = 1.0 / 3.0;

            var m2 = l < 0.5 ? (l * (s + 1.0)) : (l + s - l * s);
            var m1 = 2.0 * l - m2;
            var r = Normalize(HueToRgb(m1, m2, h + third));
            var g = Normalize(HueToRgb(m1, m2, h));
            var b = Normalize(HueToRgb(m1, m2, h - third));
            return new CssColorValue(r, g, b, Normalize(alpha));
        }

        /// <summary>
        /// Returns the color that represents Hue-Whiteness-Blackness.
        /// </summary>
        /// <param name="h">The color angle [0,1].</param>
        /// <param name="w">The whiteness [0,1].</param>
        /// <param name="b">The blackness [0,1].</param>
        /// <returns>The CSS color.</returns>
        public static CssColorValue FromHwb(Double h, Double w, Double b)
        {
            return FromHwba(h, w, b, 1f);
        }

        /// <summary>
        /// Returns the color that represents Hue-Whiteness-Blackness.
        /// </summary>
        /// <param name="h">The color angle [0,1].</param>
        /// <param name="w">The whiteness [0,1].</param>
        /// <param name="b">The blackness [0,1].</param>
        /// <param name="alpha">The alpha value [0,1].</param>
        /// <returns>The CSS color.</returns>
        public static CssColorValue FromHwba(Double h, Double w, Double b, Double alpha)
        {
            var ratio = 1.0 / (w + b);
            var red = 0.0;
            var green = 0.0;
            var blue = 0.0;

            if (ratio < 1.0)
            {
                w *= ratio;
                b *= ratio;
            }

            var p = (Int32)(6.0 * h);
            var f = 6.0 * h - p;

            if ((p & 0x01) != 0)
            {
                f = 1.0 - f;
            }

            var v = 1.0 - b;
            var n = w + f * (v - w);

            switch (p)
            {
                default:
                case 6:
                case 0: red = v; green = n; blue = w; break;
                case 1: red = n; green = v; blue = w; break;
                case 2: red = w; green = v; blue = n; break;
                case 3: red = w; green = n; blue = v; break;
                case 4: red = n; green = w; blue = v; break;
                case 5: red = v; green = w; blue = n; break;
            }

            return FromRgba(red, green, blue, alpha);
        }

        /// <summary>
        /// Returns the color a color() function spells in one of the predefined spaces of
        /// CSS Color 4 12.3: the components leave the space's own transfer function, are carried to
        /// CIE XYZ by the matrix the specification derives from that space's primaries and white point,
        /// adapted to the D50 that the Lab path already speaks, and encoded to sRGB at the end of it.
        /// </summary>
        /// <param name="space">The space name, as the page wrote it.</param>
        /// <param name="r">The first component, in the space's own encoding [0,1].</param>
        /// <param name="g">The second component [0,1].</param>
        /// <param name="b">The third component [0,1].</param>
        /// <param name="alpha">The alpha value [0,1].</param>
        /// <returns>The color, or null where the name is no space the table below carries.</returns>
        public static CssColorValue? FromColor(String space, Double r, Double g, Double b, Double alpha)
        {
            switch (space.ToLowerInvariant())
            {
                case "srgb":
                    // The components are sRGB already: encoded, and in the order they are drawn in.
                    return FromRgba(r, g, b, alpha);
                case "srgb-linear":
                    return FromRgba(Project(r), Project(g), Project(b), alpha);
                case "display-p3":
                    return ToSrgb(new[] { Unproject(r), Unproject(g), Unproject(b) }, DisplayP3ToXyz, XyzWhite.D65, alpha);
                case "display-p3-linear":
                    return ToSrgb(new[] { r, g, b }, DisplayP3ToXyz, XyzWhite.D65, alpha);
                case "a98-rgb":
                    return ToSrgb(new[] { Raised(r, 563.0 / 256.0), Raised(g, 563.0 / 256.0), Raised(b, 563.0 / 256.0) }, A98ToXyz, XyzWhite.D65, alpha);
                case "a98-rgb-linear":
                    return ToSrgb(new[] { r, g, b }, A98ToXyz, XyzWhite.D65, alpha);
                case "rec2020":
                    return ToSrgb(new[] { Raised(r, 2.4), Raised(g, 2.4), Raised(b, 2.4) }, Rec2020ToXyz, XyzWhite.D65, alpha);
                case "rec2020-linear":
                    return ToSrgb(new[] { r, g, b }, Rec2020ToXyz, XyzWhite.D65, alpha);
                // ProPhoto is written on the D50 that Lab uses, so its matrix needs no adapting.
                case "prophoto-rgb":
                    return ToSrgb(new[] { Unprophoto(r), Unprophoto(g), Unprophoto(b) }, ProPhotoToXyz, XyzWhite.D50, alpha);
                case "prophoto-rgb-linear":
                    return ToSrgb(new[] { r, g, b }, ProPhotoToXyz, XyzWhite.D50, alpha);
                case "xyz":
                case "xyz-d65":
                    return ToSrgb(new[] { r, g, b }, Identity, XyzWhite.D65, alpha);
                case "xyz-d50":
                    return ToSrgb(new[] { r, g, b }, Identity, XyzWhite.D50, alpha);
            }

            return null;
        }

        private enum XyzWhite
        {
            D50,
            D65,
        }

        // The matrices are CSS Color 4 12.3 and 19's own, evaluated from the rational form it prints
        // them in; the Bradford adaptation is the one the same sample code carries. ProPhoto is the space
        // whose matrix is already expressed on D50, which is why it needs no adapting.
        private static readonly Double[,] Identity = new[,]
        {
            { 1.0, 0.0, 0.0 },
            { 0.0, 1.0, 0.0 },
            { 0.0, 0.0, 1.0 },
        };

        private static readonly Double[,] DisplayP3ToXyz = new[,]
        {
            { 0.48657094864821626, 0.26566769316909294, 0.1982172852343625 },
            { 0.22897456406974884, 0.6917385218365062, 0.079286914093745 },
            { 0.0, 0.045113381858902575, 1.0439443689009757 },
        };

        private static readonly Double[,] A98ToXyz = new[,]
        {
            { 0.5766690429101308, 0.18555823790654627, 0.18822864623499472 },
            { 0.29734497525053616, 0.627363566255466, 0.07529145849399789 },
            { 0.027031361386412378, 0.07068885253582714, 0.9913375368376389 },
        };

        private static readonly Double[,] Rec2020ToXyz = new[,]
        {
            { 0.6369580483012913, 0.14461690358620838, 0.16888097516417205 },
            { 0.26270021201126703, 0.677998071518871, 0.059301716469861945 },
            { 0.0, 0.028072693049087508, 1.0609850577107909 },
        };

        private static readonly Double[,] ProPhotoToXyz = new[,]
        {
            { 0.79776664490064230, 0.13518129740053308, 0.03134773412839220 },
            { 0.28807482881940130, 0.71183523424187300, 0.00008993693872564 },
            { 0.00000000000000000, 0.00000000000000000, 0.82510460251046020 },
        };

        private static readonly Double[,] D65ToD50 = new[,]
        {
            { 1.0479297925449969, 0.022946870601609652, -0.05019226628920524 },
            { 0.02962780877005599, 0.9904344267538799, -0.017073799063418826 },
            { -0.009243040646204504, 0.015055191490298152, 0.7518742814281371 },
        };

        private static CssColorValue? ToSrgb(Double[] components, Double[,] toXyz, XyzWhite white, Double alpha)
        {
            var xyz = Transform(toXyz, components);

            if (white == XyzWhite.D65)
            {
                xyz = Transform(D65ToD50, xyz);
            }

            return FromXyz50(xyz[0], xyz[1], xyz[2], alpha);
        }

        private static Double[] Transform(Double[,] matrix, Double[] vector) => new[]
        {
            matrix[0, 0] * vector[0] + matrix[0, 1] * vector[1] + matrix[0, 2] * vector[2],
            matrix[1, 0] * vector[0] + matrix[1, 1] * vector[1] + matrix[1, 2] * vector[2],
            matrix[2, 0] * vector[0] + matrix[2, 1] * vector[1] + matrix[2, 2] * vector[2],
        };

        // The sRGB curve run backwards, for the spaces that share it: display-p3 is that transfer
        // function on different primaries, and nothing else.
        private static Double Unproject(Double c)
        {
            var abs = Math.Abs(c);

            if (abs <= 0.04045)
            {
                return c / 12.92;
            }

            var sgn = c < 0.0 ? -1.0 : 1.0;
            return sgn * Math.Pow((abs + 0.055) / 1.055, 2.4);
        }

        private static Double Raised(Double c, Double exponent)
        {
            var sgn = c < 0.0 ? -1.0 : 1.0;
            return sgn * Math.Pow(Math.Abs(c), exponent);
        }

        // ProPhoto's curve is a gamma of 1.8 with a straight piece below 16/512, and CSS Color 4's own
        // reading of it raises the written value - the curve is applied on the way out, not on the way in.
        private static Double Unprophoto(Double c)
        {
            var abs = Math.Abs(c);

            if (abs <= 16.0 / 512.0)
            {
                return c / 16.0;
            }

            return Raised(c, 1.8);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets if hex codes should be used for serialization.
        /// </summary>
        public static Boolean UseHex { get; set; }

        /// <summary>
        /// Gets or sets if the CSSOM serialization rules should be used, i.e.,
        /// if the alpha channel of an opaque color should be omitted.
        /// </summary>
        public static Boolean UseSpecSerialization { get; set; }

        /// <summary>
        /// Gets the CSS text representation.
        /// </summary>
        public String CssText
        {
            get
            {
                if (Equals(CssColorValue.CurrentColor))
                {
                    return CssKeywords.CurrentColor;
                }
                else if (Equals(CssColorValue.InvertedColor))
                {
                    return CssKeywords.Invert;
                }
                else if (UseHex)
                {
                    var color = $"#{_red.ToString("X2", CultureInfo.InvariantCulture)}{_green.ToString("X2", CultureInfo.InvariantCulture)}{_blue.ToString("X2", CultureInfo.InvariantCulture)}";

                    if (_alpha != 255)
                    {
                        return $"{color}{_alpha.ToString("X2", CultureInfo.InvariantCulture)}";
                    }

                    return color;
                }
                else if (UseSpecSerialization && _alpha == 255)
                {
                    var fn = FunctionNames.Rgb;
                    var args = String.Join(", ", new[]
                    {
                        R.ToString(CultureInfo.InvariantCulture),
                        G.ToString(CultureInfo.InvariantCulture),
                        B.ToString(CultureInfo.InvariantCulture),
                    });
                    return fn.CssFunction(args);
                }
                else
                {
                    var fn = FunctionNames.Rgba;
                    var args = String.Join(", ", new[]
                    {
                        R.ToString(CultureInfo.InvariantCulture),
                        G.ToString(CultureInfo.InvariantCulture),
                        B.ToString(CultureInfo.InvariantCulture),
                        Alpha.ToString(CultureInfo.InvariantCulture),
                    });
                    return fn.CssFunction(args);
                }
            }
        }

        /// <summary>
        /// Gets the Int32 value of the color.
        /// </summary>
        public Int32 Value => _hashcode;

        /// <summary>
        /// Gets the alpha part of the color.
        /// </summary>
        public Byte A => _alpha;

        /// <summary>
        /// Gets the alpha part of the color in percent (0..1).
        /// </summary>
        public Double Alpha => Math.Round(_alpha / 255.0, 2);

        /// <summary>
        /// Gets the red part of the color.
        /// </summary>
        public Byte R => _red;

        /// <summary>
        /// Gets the green part of the color.
        /// </summary>
        public Byte G => _green;

        /// <summary>
        /// Gets the blue part of the color.
        /// </summary>
        public Byte B => _blue;

        #endregion

        #region Equality

        /// <summary>
        /// Compares two colors and returns a boolean indicating if the two do match.
        /// </summary>
        /// <param name="a">The first color to use.</param>
        /// <param name="b">The second color to use.</param>
        /// <returns>True if both colors are equal, otherwise false.</returns>
        public static Boolean operator ==(CssColorValue a, CssColorValue b) => a._hashcode == b._hashcode;

        /// <summary>
        /// Compares two colors and returns a boolean indicating if the two do not match.
        /// </summary>
        /// <param name="a">The first color to use.</param>
        /// <param name="b">The second color to use.</param>
        /// <returns>True if both colors are not equal, otherwise false.</returns>
        public static Boolean operator !=(CssColorValue a, CssColorValue b) => a._hashcode != b._hashcode;

        /// <summary>
        /// Checks two colors for equality.
        /// </summary>
        /// <param name="other">The other color.</param>
        /// <returns>True if both colors or equal, otherwise false.</returns>
        public Boolean Equals(CssColorValue other) => _hashcode == other._hashcode;

        /// <summary>
        /// Tests if another object is equal to this object.
        /// </summary>
        /// <param name="obj">The object to test with.</param>
        /// <returns>True if the two objects are equal, otherwise false.</returns>
        public override Boolean Equals(Object obj)
        {
            var other = obj as CssColorValue?;

            if (other != null)
            {
                return Equals(other.Value);
            }

            return false;
        }

        Boolean IEquatable<ICssValue>.Equals(ICssValue other) => other is CssColorValue value && Equals(value);

        Int32 IComparable<CssColorValue>.CompareTo(CssColorValue other) => _hashcode - other._hashcode;

        /// <summary>
        /// Returns a hash code that defines the current color.
        /// </summary>
        /// <returns>The integer value of the hashcode.</returns>
        public override Int32 GetHashCode() => _hashcode;

        #endregion

        #region Methods

        ICssValue ICssValue.Compute(ICssComputeContext context)
        {
            return this;
        }

        /// <summary>
        /// Mixes two colors using alpha compositing as described here:
        /// http://en.wikipedia.org/wiki/Alpha_compositing
        /// </summary>
        /// <param name="above">The first color (above) with transparency.</param>
        /// <param name="below">The second color (below the first one) without transparency.</param>
        /// <returns>The outcome in the crossing section.</returns>
        public static CssColorValue Mix(CssColorValue above, CssColorValue below) => Mix(above.Alpha, above, below);

        /// <summary>
        /// Mixes two colors using alpha compositing as described here:
        /// http://en.wikipedia.org/wiki/Alpha_compositing
        /// </summary>
        /// <param name="alpha">The mixing parameter.</param>
        /// <param name="above">The first color (above) (no transparency).</param>
        /// <param name="below">The second color (below the first one) (no transparency).</param>
        /// <returns>The outcome in the crossing section.</returns>
        public static CssColorValue Mix(Double alpha, CssColorValue above, CssColorValue below)
        {
            var gamma = 1.0 - alpha;
            var r = gamma * below.R + alpha * above.R;
            var g = gamma * below.G + alpha * above.G;
            var b = gamma * below.B + alpha * above.B;
            return new CssColorValue((Byte)r, (Byte)g, (Byte)b);
        }

        #endregion

        #region Helpers

        private static Double Project(Double c)
        {
            var abs = Math.Abs(c);

            if (abs > 0.0031308)
            {
                var sgn = Math.Sign(c);

                if (sgn == 0)
                {
                    sgn = 1;
                }

                return sgn * (1.055 * Math.Pow(abs, 1.0 / 2.4) - 0.055);
            }

            return c * 12.92;
        }

        private static Double Grow(Double v)
        {
            var k = Math.Pow(29.0, 3.0) / Math.Pow(3.0, 3.0);
            var e = Math.Pow(6.0, 3.0) / Math.Pow(29.0, 3.0);
            return (Math.Pow(v, 3.0) > e ? Math.Pow(v, 3.0) : (116.0 * v - 16.0) / k);
        }

        // A channel of a converted colour is a fraction of the full one, and CSS Values 3 4.3 says which
        // whole number it becomes: 255 of them, and a value standing halfway between two goes up. Scaling
        // by 256 and cutting off the remainder is a different number - lab(50% 50 0) is 78 of green, which
        // that way of doing it answers as 77.
        private static Byte Normalize(Double value) =>
            (Byte)Math.Round(Math.Max(Math.Min(value, 1.0), 0.0) * 255.0, MidpointRounding.AwayFromZero);

        private static Double HueToRgb(Double m1, Double m2, Double h)
        {
            if (h < 0.0)
            {
                h += 1.0;
            }
            else if (h > 1.0)
            {
                h -= 1.0;
            }

            if (6.0 * h < 1.0)
            {
                return m1 + (m2 - m1) * h * 6.0;
            }
            else if (2.0 * h < 1.0)
            {
                return m2;
            }
            else if (3.0 * h < 2.0)
            {
                return m1 + (m2 - m1) * (4.0 - 6.0 * h);
            }

            return m1;
        }

        #endregion
    }
}
