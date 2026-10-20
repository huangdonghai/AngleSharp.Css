#nullable disable
namespace AngleSharp.Css.Declarations
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Text;
    using System;
    using static ValueConverters;

    /// <summary>
    /// The value an inset takes: a length or a percentage of the crossing gap, or the
    /// <c>overlap-join</c> keyword that makes decorations meet at a junction (css-gaps-1 3.3).
    /// </summary>
    static class InsetConverters
    {
        public static readonly IValueConverter Value = Or(
            LengthOrPercentConverter,
            Assign(CssKeywords.OverlapJoin, CssKeywords.OverlapJoin));
    }

    /// <summary>
    /// Gives one value to every longhand of a shorthand: every inset shorthand states a single value for
    /// all the ends it covers.
    /// </summary>
    sealed class SameValueAggregator : IValueAggregator, IValueConverter
    {
        private readonly Int32 _count;
        private readonly IValueConverter _converter;

        public SameValueAggregator(Int32 count, IValueConverter converter)
        {
            _count = count;
            _converter = converter;
        }

        public ICssValue Convert(StringSource source) => _converter.Convert(source);

        public ICssValue Merge(ICssValue[] values)
        {
            foreach (var value in values)
            {
                if (value != null)
                {
                    return value;
                }
            }

            return null;
        }

        public ICssValue[] Split(ICssValue value)
        {
            if (value is null)
            {
                return null;
            }

            var items = new ICssValue[_count];

            for (var index = 0; index < items.Length; index++)
            {
                items[index] = value;
            }

            return items;
        }
    }

    static class ColumnRuleInsetStartDeclaration
    {
        public static String Name = PropertyNames.ColumnRuleInsetStart;

        public static String[] Shorthands = new[] { PropertyNames.ColumnRuleInset, PropertyNames.RuleInset };

        public static IValueConverter Converter = InsetConverters.Value;

        public static ICssValue InitialValue = InitialValues.GapRuleInsetDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }

    static class ColumnRuleInsetEndDeclaration
    {
        public static String Name = PropertyNames.ColumnRuleInsetEnd;

        public static String[] Shorthands = new[] { PropertyNames.ColumnRuleInset, PropertyNames.RuleInset };

        public static IValueConverter Converter = InsetConverters.Value;

        public static ICssValue InitialValue = InitialValues.GapRuleInsetDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }

    static class RowRuleInsetStartDeclaration
    {
        public static String Name = PropertyNames.RowRuleInsetStart;

        public static String[] Shorthands = new[] { PropertyNames.RowRuleInset, PropertyNames.RuleInset };

        public static IValueConverter Converter = InsetConverters.Value;

        public static ICssValue InitialValue = InitialValues.GapRuleInsetDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }

    static class RowRuleInsetEndDeclaration
    {
        public static String Name = PropertyNames.RowRuleInsetEnd;

        public static String[] Shorthands = new[] { PropertyNames.RowRuleInset, PropertyNames.RuleInset };

        public static IValueConverter Converter = InsetConverters.Value;

        public static ICssValue InitialValue = InitialValues.GapRuleInsetDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }

    static class ColumnRuleInsetCapStartDeclaration
    {
        public static String Name = PropertyNames.ColumnRuleInsetCapStart;

        public static String[] Shorthands = new[]
        {
            PropertyNames.ColumnRuleInset, PropertyNames.RuleInset, PropertyNames.RuleInsetCap,
        };

        public static IValueConverter Converter = InsetConverters.Value;

        public static ICssValue InitialValue = InitialValues.GapRuleInsetDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }

    static class ColumnRuleInsetCapEndDeclaration
    {
        public static String Name = PropertyNames.ColumnRuleInsetCapEnd;

        public static String[] Shorthands = new[]
        {
            PropertyNames.ColumnRuleInset, PropertyNames.RuleInset, PropertyNames.RuleInsetCap,
        };

        public static IValueConverter Converter = InsetConverters.Value;

        public static ICssValue InitialValue = InitialValues.GapRuleInsetDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }

    static class ColumnRuleInsetJunctionStartDeclaration
    {
        public static String Name = PropertyNames.ColumnRuleInsetJunctionStart;

        public static String[] Shorthands = new[]
        {
            PropertyNames.ColumnRuleInset, PropertyNames.RuleInset, PropertyNames.RuleInsetJunction,
        };

        public static IValueConverter Converter = InsetConverters.Value;

        public static ICssValue InitialValue = InitialValues.GapRuleInsetDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }

    static class ColumnRuleInsetJunctionEndDeclaration
    {
        public static String Name = PropertyNames.ColumnRuleInsetJunctionEnd;

        public static String[] Shorthands = new[]
        {
            PropertyNames.ColumnRuleInset, PropertyNames.RuleInset, PropertyNames.RuleInsetJunction,
        };

        public static IValueConverter Converter = InsetConverters.Value;

        public static ICssValue InitialValue = InitialValues.GapRuleInsetDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }

    static class RowRuleInsetCapStartDeclaration
    {
        public static String Name = PropertyNames.RowRuleInsetCapStart;

        public static String[] Shorthands = new[]
        {
            PropertyNames.RowRuleInset, PropertyNames.RuleInset, PropertyNames.RuleInsetCap,
        };

        public static IValueConverter Converter = InsetConverters.Value;

        public static ICssValue InitialValue = InitialValues.GapRuleInsetDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }

    static class RowRuleInsetCapEndDeclaration
    {
        public static String Name = PropertyNames.RowRuleInsetCapEnd;

        public static String[] Shorthands = new[]
        {
            PropertyNames.RowRuleInset, PropertyNames.RuleInset, PropertyNames.RuleInsetCap,
        };

        public static IValueConverter Converter = InsetConverters.Value;

        public static ICssValue InitialValue = InitialValues.GapRuleInsetDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }

    static class RowRuleInsetJunctionStartDeclaration
    {
        public static String Name = PropertyNames.RowRuleInsetJunctionStart;

        public static String[] Shorthands = new[]
        {
            PropertyNames.RowRuleInset, PropertyNames.RuleInset, PropertyNames.RuleInsetJunction,
        };

        public static IValueConverter Converter = InsetConverters.Value;

        public static ICssValue InitialValue = InitialValues.GapRuleInsetDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }

    static class RowRuleInsetJunctionEndDeclaration
    {
        public static String Name = PropertyNames.RowRuleInsetJunctionEnd;

        public static String[] Shorthands = new[]
        {
            PropertyNames.RowRuleInset, PropertyNames.RuleInset, PropertyNames.RuleInsetJunction,
        };

        public static IValueConverter Converter = InsetConverters.Value;

        public static ICssValue InitialValue = InitialValues.GapRuleInsetDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }

    static class ColumnRuleInsetDeclaration
    {
        public static String Name = PropertyNames.ColumnRuleInset;

        public static String[] Longhands = new[]
        {
            PropertyNames.ColumnRuleInsetCapStart,
            PropertyNames.ColumnRuleInsetJunctionStart,
            PropertyNames.ColumnRuleInsetCapEnd,
            PropertyNames.ColumnRuleInsetJunctionEnd,
        };

        public static IValueConverter Converter = new SameValueAggregator(4, InsetConverters.Value);

        public static ICssValue InitialValue = null;

        public static PropertyFlags Flags = PropertyFlags.Shorthand;
    }

    static class RowRuleInsetDeclaration
    {
        public static String Name = PropertyNames.RowRuleInset;

        public static String[] Longhands = new[]
        {
            PropertyNames.RowRuleInsetCapStart,
            PropertyNames.RowRuleInsetJunctionStart,
            PropertyNames.RowRuleInsetCapEnd,
            PropertyNames.RowRuleInsetJunctionEnd,
        };

        public static IValueConverter Converter = new SameValueAggregator(4, InsetConverters.Value);

        public static ICssValue InitialValue = null;

        public static PropertyFlags Flags = PropertyFlags.Shorthand;
    }

    static class RuleInsetCapDeclaration
    {
        public static String Name = PropertyNames.RuleInsetCap;

        public static String[] Longhands = new[]
        {
            PropertyNames.ColumnRuleInsetCapStart,
            PropertyNames.ColumnRuleInsetCapEnd,
            PropertyNames.RowRuleInsetCapStart,
            PropertyNames.RowRuleInsetCapEnd,
        };

        public static IValueConverter Converter = new SameValueAggregator(4, InsetConverters.Value);

        public static ICssValue InitialValue = null;

        public static PropertyFlags Flags = PropertyFlags.Shorthand;
    }

    static class RuleInsetJunctionDeclaration
    {
        public static String Name = PropertyNames.RuleInsetJunction;

        public static String[] Longhands = new[]
        {
            PropertyNames.ColumnRuleInsetJunctionStart,
            PropertyNames.ColumnRuleInsetJunctionEnd,
            PropertyNames.RowRuleInsetJunctionStart,
            PropertyNames.RowRuleInsetJunctionEnd,
        };

        public static IValueConverter Converter = new SameValueAggregator(4, InsetConverters.Value);

        public static ICssValue InitialValue = null;

        public static PropertyFlags Flags = PropertyFlags.Shorthand;
    }

    static class RuleInsetDeclaration
    {
        public static String Name = PropertyNames.RuleInset;

        public static String[] Longhands = new[]
        {
            PropertyNames.ColumnRuleInsetCapStart,
            PropertyNames.ColumnRuleInsetJunctionStart,
            PropertyNames.ColumnRuleInsetCapEnd,
            PropertyNames.ColumnRuleInsetJunctionEnd,
            PropertyNames.RowRuleInsetCapStart,
            PropertyNames.RowRuleInsetJunctionStart,
            PropertyNames.RowRuleInsetCapEnd,
            PropertyNames.RowRuleInsetJunctionEnd,
        };

        public static IValueConverter Converter = new SameValueAggregator(8, InsetConverters.Value);

        public static ICssValue InitialValue = null;

        public static PropertyFlags Flags = PropertyFlags.Shorthand;
    }

    /// <summary>
    /// The value a visibility-items property takes: which gaps get a decoration, by whether the areas
    /// beside them hold anything (css-gaps-1 3.4).
    /// </summary>
    static class VisibilityConverters
    {
        public static readonly IValueConverter Value = Or(
            Assign(CssKeywords.All, CssKeywords.All),
            Assign(CssKeywords.Around, CssKeywords.Around),
            Assign(CssKeywords.Between, CssKeywords.Between),
            Assign(CssKeywords.Normal, CssKeywords.Normal));
    }

    static class ColumnRuleVisibilityItemsDeclaration
    {
        public static String Name = PropertyNames.ColumnRuleVisibilityItems;

        public static String[] Shorthands = new[] { PropertyNames.RuleVisibilityItems };

        public static IValueConverter Converter = VisibilityConverters.Value;

        public static ICssValue InitialValue = InitialValues.GapRuleVisibilityDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }

    static class RowRuleVisibilityItemsDeclaration
    {
        public static String Name = PropertyNames.RowRuleVisibilityItems;

        public static String[] Shorthands = new[] { PropertyNames.RuleVisibilityItems };

        public static IValueConverter Converter = VisibilityConverters.Value;

        public static ICssValue InitialValue = InitialValues.GapRuleVisibilityDecl;

        public static PropertyFlags Flags = PropertyFlags.None;
    }

    static class RuleVisibilityItemsDeclaration
    {
        public static String Name = PropertyNames.RuleVisibilityItems;

        public static String[] Longhands = new[]
        {
            PropertyNames.ColumnRuleVisibilityItems,
            PropertyNames.RowRuleVisibilityItems,
        };

        public static IValueConverter Converter = new SameValueAggregator(2, VisibilityConverters.Value);

        public static ICssValue InitialValue = null;

        public static PropertyFlags Flags = PropertyFlags.Shorthand;
    }
}
