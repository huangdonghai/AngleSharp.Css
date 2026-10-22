#nullable disable
namespace AngleSharp.Css.Tests.Values
{
    using AngleSharp.Css.Converters;
    using AngleSharp.Css.Values;
    using NUnit.Framework;
    using System.Linq;
    using static CssConstructionFunctions;
    using static ValueConverters;

    [TestFixture]
    public class GradientTests
    {
        [Test]
        public void InLinearGradient()
        {
            var source = "linear-gradient(135deg, red, blue)";
            var value = GradientConverter.Convert(source);
            Assert.IsNotNull(value);
        }

        [Test]
        public void InRadialGradient()
        {
            var source = "radial-gradient(ellipse farthest-corner at 45px 45px , #00FFFF, rgba(0, 0, 255, 0) 50%, #0000FF 95%)";
            var value = GradientConverter.Convert(source);
            Assert.IsNotNull(value);
        }

        [Test]
        public void BackgroundImageLinearGradientWithAngle()
        {
            var red = CssColorValue.Red;
            var blue = CssColorValue.Blue;
            var source = $"background-image: linear-gradient(135deg, {red.CssText}, {blue.CssText})";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssLinearGradientValue;
            Assert.IsNotNull(gradient);
            Assert.IsFalse(gradient.IsRepeating);
            Assert.AreEqual(CssAngleValue.TripleHalfQuarter, gradient.Angle);
            Assert.AreEqual(2, gradient.Stops.Length);
            Assert.AreEqual(red, gradient.Stops.OfType<CssGradientStopValue>().First().Color);
            Assert.AreEqual(blue, gradient.Stops.OfType<CssGradientStopValue>().Last().Color);

            Assert.AreEqual(source, property.CssText);
        }

        [Test]
        public void BackgroundImageLinearGradientWithSide()
        {
            var source = "background-image: linear-gradient(to right, red, orange, yellow, green, blue, indigo, violet)";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssLinearGradientValue;
            Assert.IsFalse(gradient.IsRepeating);
            Assert.AreEqual(CssAngleValue.Quarter, gradient.Angle);
            var stops = gradient.Stops.ToArray();
            Assert.AreEqual(7, stops.Length);
            Assert.AreEqual(CssColors.GetColor("red").Value, ((CssGradientStopValue)stops[0]).Color);
            Assert.AreEqual(CssColors.GetColor("orange").Value, ((CssGradientStopValue)stops[1]).Color);
            Assert.AreEqual(CssColors.GetColor("yellow").Value, ((CssGradientStopValue)stops[2]).Color);
            Assert.AreEqual(CssColors.GetColor("green").Value, ((CssGradientStopValue)stops[3]).Color);
            Assert.AreEqual(CssColors.GetColor("blue").Value, ((CssGradientStopValue)stops[4]).Color);
            Assert.AreEqual(CssColors.GetColor("indigo").Value, ((CssGradientStopValue)stops[5]).Color);
            Assert.AreEqual(CssColors.GetColor("violet").Value, ((CssGradientStopValue)stops[6]).Color);
        }

        [Test]
        public void BackgroundImageLinearGradientWithCornerAndRgba()
        {
            var source = "background-image: linear-gradient(to bottom right, red, rgba(255,0,0,0))";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssLinearGradientValue;
            Assert.IsFalse(gradient.IsRepeating);
            Assert.AreEqual(CssAngleValue.TripleHalfQuarter, gradient.Angle);
            Assert.AreEqual(2, gradient.Stops.Count());
            Assert.AreEqual(CssColorValue.Red, gradient.Stops.OfType<CssGradientStopValue>().First().Color);
            Assert.AreEqual(CssColorValue.FromRgba(255, 0, 0, 0), gradient.Stops.OfType<CssGradientStopValue>().Last().Color);
        }

        [Test]
        public void BackgroundImageLinearGradientWithSideAndHsl()
        {
            var source = "background-image: linear-gradient(to bottom, hsl(0, 80%, 70%), #bada55)";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssLinearGradientValue;
            Assert.IsFalse(gradient.IsRepeating);
            Assert.AreEqual(CssAngleValue.Half, gradient.Angle);
            Assert.AreEqual(2, gradient.Stops.Count());
            Assert.AreEqual(CssColorValue.FromHsl(0f, 0.8f, 0.7f), gradient.Stops.OfType<CssGradientStopValue>().First().Color);
            Assert.AreEqual(CssColorValue.FromHex("bada55"), gradient.Stops.OfType<CssGradientStopValue>().Last().Color);
        }

        [Test]
        public void BackgroundImageLinearGradientNoAngle()
        {
            var source = "background-image: linear-gradient(yellow, blue 20%, #0f0)";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssLinearGradientValue;
            Assert.IsFalse(gradient.IsRepeating);
            Assert.AreEqual(CssAngleValue.Half, gradient.Angle);
            Assert.AreEqual(3, gradient.Stops.Count());
            Assert.AreEqual(CssColors.GetColor("yellow").Value, gradient.Stops.OfType<CssGradientStopValue>().First().Color);
            Assert.AreEqual(CssColors.GetColor("blue").Value, gradient.Stops.OfType<CssGradientStopValue>().Skip(1).First().Color);
            Assert.AreEqual(CssColorValue.FromRgb(0, 255, 0), gradient.Stops.OfType<CssGradientStopValue>().Skip(2).First().Color);
        }

        [Test]
        public void BackgroundImageRadialGradientCircleFarthestCorner()
        {
            var source = "background-image: radial-gradient(circle farthest-corner at 45px 45px , #00FFFF 0%, rgba(0, 0, 255, 0) 50%, #0000FF 95%)";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssRadialGradientValue;
            Assert.IsFalse(gradient.IsRepeating);
            Assert.AreEqual(new CssLengthValue(45, CssLengthValue.Unit.Px), gradient.Position.X);
            Assert.AreEqual(new CssLengthValue(45, CssLengthValue.Unit.Px), gradient.Position.Y);
            Assert.AreEqual(true, gradient.IsCircle);
            Assert.AreEqual(CssRadialGradientValue.SizeMode.FarthestCorner, gradient.Mode);
            var stops = gradient.Stops.ToArray();
            Assert.AreEqual(3, stops.Length);
            Assert.AreEqual(CssColorValue.FromRgb(0, 255, 255), ((CssGradientStopValue)stops[0]).Color);
            Assert.AreEqual(CssColorValue.FromRgba(0, 0, 255, 0), ((CssGradientStopValue)stops[1]).Color);
            Assert.AreEqual(CssColorValue.FromRgb(0, 0, 255), ((CssGradientStopValue)stops[2]).Color);
        }

        [Test]
        public void BackgroundImageRadialGradientEllipseFarthestCorner()
        {
            var source = "background-image: radial-gradient(ellipse farthest-corner at 470px 47px , #FFFF80 20%, rgba(204, 153, 153, 0.4) 30%, #E6E6FF 60%)";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssRadialGradientValue;
            Assert.IsFalse(gradient.IsRepeating);
            Assert.AreEqual(new CssLengthValue(470, CssLengthValue.Unit.Px), gradient.Position.X);
            Assert.AreEqual(new CssLengthValue(47, CssLengthValue.Unit.Px), gradient.Position.Y);
            Assert.AreEqual(false, gradient.IsCircle);
            Assert.AreEqual(CssRadialGradientValue.SizeMode.FarthestCorner, gradient.Mode);
            var stops = gradient.Stops.ToArray();
            Assert.AreEqual(3, stops.Length);
            Assert.AreEqual(CssColorValue.FromRgb(0xFF, 0xFF, 0x80), ((CssGradientStopValue)stops[0]).Color);
            Assert.AreEqual(CssColorValue.FromRgba(204, 153, 153, 0.4f), ((CssGradientStopValue)stops[1]).Color);
            Assert.AreEqual(CssColorValue.FromRgb(0xE6, 0xE6, 0xFF), ((CssGradientStopValue)stops[2]).Color);
        }

        [Test]
        public void BackgroundImageRadialGradientFarthestCornerWithPoint()
        {
            var source = "background-image: radial-gradient(farthest-corner at 45px 45px , #FF0000 0%, #0000FF 100%)";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssRadialGradientValue;
            Assert.IsFalse(gradient.IsRepeating);
            Assert.AreEqual(new CssLengthValue(45, CssLengthValue.Unit.Px), gradient.Position.X);
            Assert.AreEqual(new CssLengthValue(45, CssLengthValue.Unit.Px), gradient.Position.Y);
            Assert.AreEqual(false, gradient.IsCircle);
            Assert.AreEqual(CssRadialGradientValue.SizeMode.FarthestCorner, gradient.Mode);
            var stops = gradient.Stops.ToArray();
            Assert.AreEqual(2, stops.Length);
            Assert.AreEqual(CssColorValue.FromRgb(255, 0, 0), ((CssGradientStopValue)stops[0]).Color);
            Assert.AreEqual(CssColorValue.FromRgb(0, 0, 255), ((CssGradientStopValue)stops[1]).Color);
        }

        [Test]
        public void BackgroundImageRadialGradientSingleSize()
        {
            var source = "background-image: radial-gradient(16px at 60px 50% , #000000 0%, #000000 14px, rgba(0, 0, 0, 0.3) 18px, rgba(0, 0, 0, 0) 19px)";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssRadialGradientValue;
            Assert.IsFalse(gradient.IsRepeating);
            Assert.AreEqual(new CssLengthValue(60f, CssLengthValue.Unit.Px), gradient.Position.X);
            Assert.AreEqual(CssLengthValue.Half, gradient.Position.Y);
            Assert.AreEqual(true, gradient.IsCircle);
            Assert.AreEqual(CssRadialGradientValue.SizeMode.None, gradient.Mode);
            Assert.AreEqual(new CssLengthValue(16f, CssLengthValue.Unit.Px), gradient.MajorRadius);
            Assert.AreEqual(CssLengthValue.Full, gradient.MinorRadius);
            var stops = gradient.Stops.ToArray();
            Assert.AreEqual(4, stops.Length);
            Assert.AreEqual(CssColorValue.FromRgb(0, 0, 0), ((CssGradientStopValue)stops[0]).Color);
            Assert.AreEqual(CssColorValue.FromRgb(0, 0, 0), ((CssGradientStopValue)stops[1]).Color);
            Assert.AreEqual(CssColorValue.FromRgba(0, 0, 0, 0.3), ((CssGradientStopValue)stops[2]).Color);
            Assert.AreEqual(CssColorValue.Transparent, ((CssGradientStopValue)stops[3]).Color);
        }

        [Test]
        public void BackgroundImageRadialGradientCircle()
        {
            var source = "background-image: radial-gradient(circle, yellow, green)";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssRadialGradientValue;
            Assert.IsFalse(gradient.IsRepeating);
            Assert.AreEqual(CssLengthValue.Half, gradient.Position.X);
            Assert.AreEqual(CssLengthValue.Half, gradient.Position.Y);
            Assert.AreEqual(true, gradient.IsCircle);
            Assert.AreEqual(CssRadialGradientValue.SizeMode.None, gradient.Mode);
            var stops = gradient.Stops.ToArray();
            Assert.AreEqual(2, stops.Length);
            Assert.AreEqual(CssColorValue.FromName("yellow").Value, ((CssGradientStopValue)stops[0]).Color);
            Assert.AreEqual(CssColorValue.FromName("green").Value, ((CssGradientStopValue)stops[1]).Color);
        }

        [Test]
        public void BackgroundImageRadialGradientOnlyGradientStops()
        {
            var source = "background-image: radial-gradient(yellow, green)";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssRadialGradientValue;
            Assert.IsFalse(gradient.IsRepeating);
            Assert.AreEqual(CssLengthValue.Half, gradient.Position.X);
            Assert.AreEqual(CssLengthValue.Half, gradient.Position.Y);
            Assert.AreEqual(false, gradient.IsCircle);
            Assert.AreEqual(CssRadialGradientValue.SizeMode.None, gradient.Mode);
            var stops = gradient.Stops.ToArray();
            Assert.AreEqual(2, stops.Length);
            Assert.AreEqual(CssColorValue.FromName("yellow").Value, ((CssGradientStopValue)stops[0]).Color);
            Assert.AreEqual(CssColorValue.FromName("green").Value, ((CssGradientStopValue)stops[1]).Color);
        }

        [Test]
        public void BackgroundImageRadialGradientEllipseAtCenter()
        {
            var source = "background-image: radial-gradient(ellipse at center, yellow 0%, green 100%)";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssRadialGradientValue;
            Assert.IsFalse(gradient.IsRepeating);
            Assert.AreEqual(CssLengthValue.Half, gradient.Position.X);
            Assert.AreEqual(CssLengthValue.Half, gradient.Position.Y);
            Assert.AreEqual(false, gradient.IsCircle);
            Assert.AreEqual(CssRadialGradientValue.SizeMode.None, gradient.Mode);
            var stops = gradient.Stops.ToArray();
            Assert.AreEqual(2, stops.Length);
            Assert.AreEqual(CssColorValue.FromName("yellow").Value, ((CssGradientStopValue)stops[0]).Color);
            Assert.AreEqual(CssColorValue.FromName("green").Value, ((CssGradientStopValue)stops[1]).Color);
        }

        [Test]
        public void BackgroundImageRadialGradientFarthestCornerWithoutPoint()
        {
            var source = "background-image: radial-gradient(farthest-corner, yellow, green)";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssRadialGradientValue;
            Assert.IsFalse(gradient.IsRepeating);
            Assert.AreEqual(CssLengthValue.Half, gradient.Position.X);
            Assert.AreEqual(CssLengthValue.Half, gradient.Position.Y);
            Assert.AreEqual(false, gradient.IsCircle);
            Assert.AreEqual(CssRadialGradientValue.SizeMode.FarthestCorner, gradient.Mode);
            var stops = gradient.Stops.ToArray();
            Assert.AreEqual(2, stops.Length);
            Assert.AreEqual(CssColorValue.FromName("yellow").Value, ((CssGradientStopValue)stops[0]).Color);
            Assert.AreEqual(CssColorValue.FromName("green").Value, ((CssGradientStopValue)stops[1]).Color);
        }

        [Test]
        public void BackgroundImageRadialGradientClosestSideWithPoint()
        {
            var source = "background-image: radial-gradient(closest-side at 20px 30px, red, yellow, green)";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssRadialGradientValue;
            Assert.IsFalse(gradient.IsRepeating);
            Assert.AreEqual(new CssLengthValue(20f, CssLengthValue.Unit.Px), gradient.Position.X);
            Assert.AreEqual(new CssLengthValue(30f, CssLengthValue.Unit.Px), gradient.Position.Y);
            Assert.AreEqual(false, gradient.IsCircle);
            Assert.AreEqual(CssRadialGradientValue.SizeMode.ClosestSide, gradient.Mode);
            var stops = gradient.Stops.ToArray();
            Assert.AreEqual(3, stops.Length);
            Assert.AreEqual(CssColorValue.FromName("red").Value, ((CssGradientStopValue)stops[0]).Color);
            Assert.AreEqual(CssColorValue.FromName("yellow").Value, ((CssGradientStopValue)stops[1]).Color);
            Assert.AreEqual(CssColorValue.FromName("green").Value, ((CssGradientStopValue)stops[2]).Color);
        }

        [Test]
        public void BackgroundImageRadialGradientSizeAndPoint()
        {
            var source = "background-image: radial-gradient(20px 30px at 20px 30px, red, yellow, green)";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssRadialGradientValue;
            Assert.IsFalse(gradient.IsRepeating);
            Assert.AreEqual(new CssLengthValue(20f, CssLengthValue.Unit.Px), gradient.Position.X);
            Assert.AreEqual(new CssLengthValue(30f, CssLengthValue.Unit.Px), gradient.Position.Y);
            Assert.AreEqual(false, gradient.IsCircle);
            Assert.AreEqual(CssRadialGradientValue.SizeMode.None, gradient.Mode);
            Assert.AreEqual(new CssLengthValue(20f, CssLengthValue.Unit.Px), gradient.MajorRadius);
            Assert.AreEqual(new CssLengthValue(30f, CssLengthValue.Unit.Px), gradient.MinorRadius);
            var stops = gradient.Stops.ToArray();
            Assert.AreEqual(3, stops.Length);
            Assert.AreEqual(CssColorValue.FromName("red").Value, ((CssGradientStopValue)stops[0]).Color);
            Assert.AreEqual(CssColorValue.FromName("yellow").Value, ((CssGradientStopValue)stops[1]).Color);
            Assert.AreEqual(CssColorValue.FromName("green").Value, ((CssGradientStopValue)stops[2]).Color);
        }

        [Test]
        public void BackgroundImageRadialGradientClosestSideCircleShuffledWithPoint()
        {
            var source = "background-image: radial-gradient(closest-side circle at 20px 30px, red, yellow, green)";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssRadialGradientValue;
            Assert.IsFalse(gradient.IsRepeating);
            Assert.AreEqual(new CssLengthValue(20f, CssLengthValue.Unit.Px), gradient.Position.X);
            Assert.AreEqual(new CssLengthValue(30f, CssLengthValue.Unit.Px), gradient.Position.Y);
            Assert.AreEqual(true, gradient.IsCircle);
            Assert.AreEqual(CssRadialGradientValue.SizeMode.ClosestSide, gradient.Mode);
            var stops = gradient.Stops.ToArray();
            Assert.AreEqual(3, stops.Length);
            Assert.AreEqual(CssColorValue.FromName("red").Value, ((CssGradientStopValue)stops[0]).Color);
            Assert.AreEqual(CssColorValue.FromName("yellow").Value, ((CssGradientStopValue)stops[1]).Color);
            Assert.AreEqual(CssColorValue.FromName("green").Value, ((CssGradientStopValue)stops[2]).Color);
        }

        [Test]
        public void BackgroundImageRadialGradientFarthestSideLeftBottom()
        {
            var source = "background-image: radial-gradient(farthest-side at left bottom, red, yellow 50px, green);";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssRadialGradientValue;
            Assert.IsFalse(gradient.IsRepeating);
            Assert.AreEqual(CssLengthValue.Zero, gradient.Position.X);
            Assert.AreEqual(CssLengthValue.Full, gradient.Position.Y);
            Assert.AreEqual(false, gradient.IsCircle);
            Assert.AreEqual(CssRadialGradientValue.SizeMode.FarthestSide, gradient.Mode);
            var stops = gradient.Stops.ToArray();
            Assert.AreEqual(3, stops.Length);
            Assert.AreEqual(CssColorValue.FromName("red").Value, ((CssGradientStopValue)stops[0]).Color);
            Assert.AreEqual(CssColorValue.FromName("yellow").Value, ((CssGradientStopValue)stops[1]).Color);
            Assert.AreEqual(CssColorValue.FromName("green").Value, ((CssGradientStopValue)stops[2]).Color);
        }

        [Test]
        public void BackgroundImageRepeatingLinearGradientRedBlue()
        {
            var source = "background-image: repeating-linear-gradient(red, blue 20px, red 40px)";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssLinearGradientValue;
            Assert.IsTrue(gradient.IsRepeating);
            var stops = gradient.Stops.ToArray();
            Assert.AreEqual(3, stops.Length);
            Assert.AreEqual(CssColorValue.FromName("red").Value, ((CssGradientStopValue)stops[0]).Color);
            Assert.AreEqual(CssColorValue.FromName("blue").Value, ((CssGradientStopValue)stops[1]).Color);
            Assert.AreEqual(CssColorValue.FromName("red").Value, ((CssGradientStopValue)stops[2]).Color);
        }

        [Test]
        public void BackgroundImageRepeatingRadialGradientRedBlue()
        {
            var source = "background-image: repeating-radial-gradient(red, blue 20px, red 40px)";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssRadialGradientValue;
            Assert.IsTrue(gradient.IsRepeating);
            Assert.AreEqual(CssLengthValue.Half, gradient.Position.X);
            Assert.AreEqual(CssLengthValue.Half, gradient.Position.Y);
            Assert.AreEqual(false, gradient.IsCircle);
            Assert.AreEqual(CssRadialGradientValue.SizeMode.None, gradient.Mode);
            var stops = gradient.Stops.ToArray();
            Assert.AreEqual(3, stops.Length);
            Assert.AreEqual(CssColorValue.FromName("red").Value, ((CssGradientStopValue)stops[0]).Color);
            Assert.AreEqual(CssColorValue.FromName("blue").Value, ((CssGradientStopValue)stops[1]).Color);
            Assert.AreEqual(CssColorValue.FromName("red").Value, ((CssGradientStopValue)stops[2]).Color);
        }

        [Test]
        public void BackgroundImageRepeatingRadialGradientFunky()
        {
            var source = "background-image: repeating-radial-gradient(circle closest-side at 20px 30px, red, yellow, green 100%, yellow 150%, red 200%)";
            var property = ParseDeclaration(source);
            Assert.IsTrue(property.HasValue);
            Assert.IsFalse(property.IsInitial);
            var value = property.RawValue as CssListValue;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Items.Length);
            var gradient = value.Items[0] as CssRadialGradientValue;
            Assert.IsTrue(gradient.IsRepeating);
            Assert.AreEqual(new CssLengthValue(20f, CssLengthValue.Unit.Px), gradient.Position.X);
            Assert.AreEqual(new CssLengthValue(30f, CssLengthValue.Unit.Px), gradient.Position.Y);
            Assert.AreEqual(true, gradient.IsCircle);
            Assert.AreEqual(CssRadialGradientValue.SizeMode.ClosestSide, gradient.Mode);
            var stops = gradient.Stops.ToArray();
            Assert.AreEqual(5, stops.Length);
            Assert.AreEqual(CssColorValue.FromName("red").Value, ((CssGradientStopValue)stops[0]).Color);
            Assert.AreEqual(CssColorValue.FromName("yellow").Value, ((CssGradientStopValue)stops[1]).Color);
            Assert.AreEqual(CssColorValue.FromName("green").Value, ((CssGradientStopValue)stops[2]).Color);
            Assert.AreEqual(CssColorValue.FromName("yellow").Value, ((CssGradientStopValue)stops[3]).Color);
            Assert.AreEqual(CssColorValue.FromName("red").Value, ((CssGradientStopValue)stops[4]).Color);
        }

        [Test]
        public void ConicGradientDefaultAngleIsZeroNotHalfCircle()
        {
            // https://drafts.csswg.org/css-images-4/#conic-gradients: "from <angle>" defaults to
            // 0deg when omitted. CssConicGradientValue.Angle instead falls back to
            // CssAngleValue.Half (180deg) - correct for CssLinearGradientValue.Angle's own default
            // ("to bottom" is 180deg), but wrong for conic-gradient, which does not share linear
            // gradient's default direction. Confirmed via Arguments.Length == Stops.Length (proving
            // no angle/center was actually parsed - the leading-argument insertion in
            // CssConicGradientValue.Arguments only happens for a value that was really authored),
            // so this is the property's own fallback being wrong, not a parsing gap.
            var source = "conic-gradient(red, blue)";
            var gradient = GradientConverter.Convert(source) as CssConicGradientValue;

            Assert.IsNotNull(gradient);
            Assert.AreEqual(gradient.Stops.Length, gradient.Arguments.Length, "no angle/center was actually authored");
            Assert.AreEqual(CssAngleValue.Zero, gradient.Angle, "an omitted `from` should default to 0deg per spec, not 180deg");
        }

        [Test]
        public void ConicGradientAcceptsAngleBasedStopPositions()
        {
            // A conic-gradient stop is naturally positioned with an angle ("red 0deg"), not just a
            // <length-percentage> - GradientParser.ParseGradientStop only ever tried
            // ParseDistanceOrCalc for a stop's position (shared, unparameterized, across linear/
            // radial/conic), which does not accept "0deg" at all. That left the source mid-token
            // instead of at the following comma/close-paren, which made the *entire* gradient fail
            // to parse (GradientConverter.Convert returning null) rather than just that one stop's
            // position - confirmed by first reproducing with GradientParser.ParseGradient directly
            // before tracing it to this one call site.
            var source = "conic-gradient(red 0deg, blue 90deg, green 1turn)";
            var gradient = GradientConverter.Convert(source) as CssConicGradientValue;

            Assert.IsNotNull(gradient);
            var stops = gradient.Stops.OfType<CssGradientStopValue>().ToArray();
            Assert.AreEqual(3, stops.Length);
            Assert.AreEqual(CssAngleValue.Zero, stops[0].Location);
            Assert.AreEqual(CssAngleValue.Quarter, stops[1].Location);
            Assert.AreEqual(new CssAngleValue(360.0, CssAngleValue.Unit.Deg), stops[2].Location);
        }
        [Test]
        public void ConicGradientStandsADoublePositionedColourAtBothEnds()
        {
            // CSS Images 3 §3.4.2 makes "red 0deg 90deg" the same colour at both ends of that span,
            // which is how a slice of a pie is written. ParseGradientStop read one position only, so the
            // second was left mid-token and the whole gradient failed to parse.
            var source = "conic-gradient(red 0deg 90deg, blue 90deg 1turn)";
            var gradient = GradientConverter.Convert(source) as CssConicGradientValue;

            Assert.IsNotNull(gradient);
            var stops = gradient.Stops.OfType<CssGradientStopValue>().ToArray();
            Assert.AreEqual(4, stops.Length);
            Assert.AreEqual(CssAngleValue.Zero, stops[0].Location);
            Assert.AreEqual(CssAngleValue.Quarter, stops[1].Location);
            Assert.AreEqual(stops[0].Color, stops[1].Color, "the span's far end carries the same colour");
            Assert.AreEqual(stops[2].Color, stops[3].Color, "and so does the next one's");
        }

        [Test]
        public void ConicGradientWritesItsHeadAsOneArgument()
        {
            // `from <angle> at <position>` is one argument of the function and the grammar puts no comma
            // between its halves. The serializer wrote them as two, so a reader that takes the arguments
            // apart by commas found a position where the first colour stop should stand.
            var source = "conic-gradient(from 45deg at 30% 70%, red, blue)";
            var gradient = GradientConverter.Convert(source) as CssConicGradientValue;

            Assert.IsNotNull(gradient);
            Assert.AreEqual(
                "conic-gradient(from 45deg at 30% 70%, rgba(255, 0, 0, 1), rgba(0, 0, 255, 1))",
                gradient.CssText);
        }
        [Test]
        public void LinearGradientCarriesTheSpaceItIsMixedIn()
        {
            // CSS Color 4 13.2 lets a gradient head open with "in <color-space>", before the direction.
            // The parser took the head from the first token, so the whole function failed on a page that
            // named the space and the declaration was dropped.
            var source = "linear-gradient(in oklab to right, red, blue)";
            var gradient = GradientConverter.Convert(source) as CssLinearGradientValue;

            Assert.IsNotNull(gradient);
            Assert.AreEqual(
                "linear-gradient(in oklab 90deg, rgba(255, 0, 0, 1), rgba(0, 0, 255, 1))",
                gradient.CssText);
            Assert.IsNotNull(GradientConverter.Convert(gradient.CssText), "and the text it writes parses back");
        }

        [Test]
        public void LinearGradientWithNoDirectionWritesItsSpaceAsOneArgument()
        {
            // The space is part of the head, so where there is no direction to stand beside it, it is the
            // head and the comma that ends it comes after it. Joined to the first colour instead, a reader
            // taking the arguments apart by commas finds neither.
            var source = "linear-gradient(in hsl longer hue, red, blue)";
            var gradient = GradientConverter.Convert(source) as CssLinearGradientValue;

            Assert.IsNotNull(gradient);
            Assert.AreEqual(
                "linear-gradient(in hsl longer hue, rgba(255, 0, 0, 1), rgba(0, 0, 255, 1))",
                gradient.CssText);
            Assert.IsNotNull(GradientConverter.Convert(gradient.CssText));
        }

        [Test]
        public void RadialGradientCarriesItsSpaceBesideTheShape()
        {
            var source = "radial-gradient(in oklab circle, red, blue)";
            var gradient = GradientConverter.Convert(source) as CssRadialGradientValue;

            Assert.IsNotNull(gradient);
            StringAssert.StartsWith("radial-gradient(in oklab circle", gradient.CssText);
            Assert.IsNotNull(GradientConverter.Convert(gradient.CssText));
        }

        [Test]
        public void ConicGradientCarriesItsSpaceAndItsHueMethod()
        {
            var source = "conic-gradient(in oklch longer hue, red, blue)";
            var gradient = GradientConverter.Convert(source) as CssConicGradientValue;

            Assert.IsNotNull(gradient);
            Assert.AreEqual(
                "conic-gradient(in oklch longer hue, rgba(255, 0, 0, 1), rgba(0, 0, 255, 1))",
                gradient.CssText);
            Assert.IsNotNull(GradientConverter.Convert(gradient.CssText));
        }

        [Test]
        public void GradientKeepsItsSpaceWhenTheValueIsComputed()
        {
            // Computing resolves the stops and rebuilds the value; a space left out of that rebuild is
            // lost from the computed style, which is what a reader of the page actually sees.
            var declarations = ParseDeclarations("background-image: linear-gradient(in oklab to right, red, blue)");

            Assert.AreEqual(
                "linear-gradient(in oklab 90deg, rgba(255, 0, 0, 1), rgba(0, 0, 255, 1))",
                declarations.GetPropertyValue("background-image"));
        }

    }
}
