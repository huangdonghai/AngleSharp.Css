#nullable disable
namespace AngleSharp.Css.Parser
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Css.Parser.Tokens;
    using AngleSharp.Text;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// See http://dev.w3.org/csswg/css-syntax/#parsing for details.
    /// </summary>
    sealed class CssBuilder
    {
        #region Fields

        private static readonly Dictionary<String, Int32> AtRuleMap = new Dictionary<String, Int32>(StringComparer.OrdinalIgnoreCase)
        {
            { RuleNames.Media, 1 },
            { RuleNames.FontFace, 2 },
            { RuleNames.Keyframes, 3 },
            { RuleNames.Import, 4 },
            { RuleNames.Charset, 5 },
            { RuleNames.Namespace, 6 },
            { RuleNames.Page, 7 },
            { RuleNames.Supports, 8 },
            { RuleNames.ViewPort, 9 },
            { RuleNames.Document, 10 },
            { RuleNames.CounterStyle, 11 },
            { RuleNames.FontFeatureValues, 12 },
            { RuleNames.Container, 13 },
            { RuleNames.Layer, 14 },
            { RuleNames.Scope, 15 },
            { RuleNames.Property, 16 },
            { RuleNames.StartingStyle, 17 },
            { RuleNames.ViewTransition, 18 },
            { RuleNames.PositionTry, 19 },
            { RuleNames.FontPaletteValues, 20 },
            { RuleNames.ColorProfile, 21 },
        };

        private readonly CssTokenizer _tokenizer;
        private readonly CssParserOptions _options;
        private readonly IBrowsingContext _context;
        private CssRuleList _commentRuleTarget;

        #endregion

        #region ctor

        public CssBuilder(CssParserOptions options, CssTokenizer tokenizer, IBrowsingContext context)
        {
            _tokenizer = tokenizer;
            _options = options;
            _context = context;
        }

        #endregion

        #region Create Rules

        public ICssRule CreateRule(ICssStyleSheet sheet, CssToken token) => CreateRule(sheet, token, inGroup: false);

        /// <param name="sheet">The sheet the rule is being read for.</param>
        /// <param name="token">The at-keyword the dispatch begins at.</param>
        /// <param name="inGroup">
        /// Whether the rule is being read inside a conditional group - <c>@media</c>, <c>@supports</c> or
        /// any other block that holds rules. The at-rules that may only stand at the top of a sheet are
        /// not rules of a group: the grammar has no place for them there, so one is not read at all. Read
        /// anyway, a <c>@namespace</c> speaks for the sheet it stands in and the sheet it belongs to has
        /// already been read past the point it may be declared - which is how one invalid rule inside a
        /// <c>@supports</c> block came to cost the page its whole stylesheet (CSS Conditional 3 §3.1: an
        /// invalid rule inside a conditional group is ignored, and the rest of the group still counts).
        /// </param>
        private ICssRule CreateRule(ICssStyleSheet sheet, CssToken token, Boolean inGroup)
        {
            switch (token.Type)
            {
                case CssTokenType.AtKeyword:
                    return CreateAtRule(sheet, token, inGroup);

                case CssTokenType.CurlyBracketOpen:
                    RaiseErrorOccurred(CssParseError.InvalidBlockStart, token.Position);
                    JumpToRuleEnd(ref token);
                    return null;

                case CssTokenType.String:
                case CssTokenType.Url:
                case CssTokenType.BadUrl:
                case CssTokenType.CurlyBracketClose:
                case CssTokenType.RoundBracketClose:
                case CssTokenType.SquareBracketClose:
                    RaiseErrorOccurred(CssParseError.InvalidToken, token.Position);
                    JumpToRuleEnd(ref token);
                    return null;

                default:
                    return CreateStyleRule(sheet, token);
            }
        }

        private ICssRule CreateStyleRule(ICssStyleSheet sheet, CssToken token)
        {
            var rule = new CssStyleRule(sheet);
            return CreateStyle(rule, token);
        }

        private ICssRule CreateAtRule(ICssStyleSheet sheet, CssToken token, Boolean inGroup = false)
        {
            if (AtRuleMap.TryGetValue(token.Data, out var ruleId))
            {
                // A namespace or a charset states something about the sheet, not about the block it is
                // written in, and the grammar of either has it before every rule of that sheet.
                if (inGroup && ruleId is 5 or 6)
                {
                    RaiseErrorOccurred(CssParseError.UnknownAtRule, token.Position);
                    JumpToEnd(ref token);
                    return null;
                }

                switch (ruleId)
                {
                    case 1: return CreateMedia(new CssMediaRule(sheet), token);
                    case 2: return CreateFontFace(new CssFontFaceRule(sheet), token);
                    case 3: return CreateKeyframes(new CssKeyframesRule(sheet), token);
                    case 4: return CreateImport(new CssImportRule(sheet), token);
                    case 5: return CreateCharset(new CssCharsetRule(sheet), token);
                    case 6: return CreateNamespace(new CssNamespaceRule(sheet), token);
                    case 7: return CreatePage(new CssPageRule(sheet), token);
                    case 8: return CreateSupports(new CssSupportsRule(sheet), token);
                    case 9: return CreateViewport(new CssViewportRule(sheet), token);
                    case 10: return CreateDocument(new CssDocumentRule(sheet), token);
                    case 11: return CreateCounterStyle(new CssCounterStyleRule(sheet), token);
                    case 12: return CreateFontFeatureValues(new CssFontFeatureValuesRule(sheet), token);
                    case 13: return CreateContainer(new CssContainerRule(sheet), token);
                    case 14: return CreateLayer(new CssLayerRule(sheet), token);
                    case 15: return CreateScope(new CssScopeRule(sheet), token);
                    case 16: return CreateProperty(new CssPropertyRule(sheet), token);
                    case 17: return CreateStartingStyle(new CssStartingStyleRule(sheet), token);
                    case 18: return CreateViewTransition(new CssViewTransitionRule(sheet), token);
                    case 19: return CreatePositionTry(new CssPositionTryRule(sheet), token);
                    case 20: return CreateFontPaletteValues(new CssFontPaletteValuesRule(sheet), token);
                    case 21: return CreateColorProfile(new CssColorProfileRule(sheet), token);
                }
            }

            if (_options.IsIncludingUnknownRules)
            {
                return CreateUnknownAtRule(sheet, token);
            }

            RaiseErrorOccurred(CssParseError.UnknownAtRule, token.Position);
            JumpToRuleEnd(ref token);
            return null;
        }

        private CssCharsetRule CreateCharset(CssCharsetRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);

            if (token.Type == CssTokenType.String)
            {
                rule.CharacterSet = token.Data;
            }

            JumpToEnd(ref token);
            return rule;
        }

        private CssDocumentRule CreateDocument(CssDocumentRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);
            var functions = GetArgument(ref token);
            var result = rule.SetConditionText(functions, throwOnError: false);
            CollectTrivia(rule.Owner, ref token);

            if (token.Type != CssTokenType.CurlyBracketOpen)
            {
                SkipDeclarations(token);
            }
            else if (FillRules(rule) && result)
            {
                return rule;
            }

            return null;
        }

        private CssViewportRule CreateViewport(CssViewportRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);

            if (token.Type != CssTokenType.CurlyBracketOpen)
            {
                SkipDeclarations(token);
                return null;
            }

            FillDeclarations(rule);
            return rule;
        }

        private CssFontFaceRule CreateFontFace(CssFontFaceRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);

            if (token.Type != CssTokenType.CurlyBracketOpen)
            {
                SkipDeclarations(token);
                return null;
            }

            FillDeclarations(rule);
            return rule;
        }

        private CssImportRule CreateImport(CssImportRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);

            if (!token.Is(CssTokenType.String, CssTokenType.Url))
            {
                JumpToEnd(ref token);
                return null;
            }

            rule.Href = token.Data;
            token = NextToken();
            CollectTrivia(rule.Owner, ref token);
            var media = GetArgument(ref token);

            if (!String.IsNullOrEmpty(media))
            {
                rule.Media.SetMediaText(media, throwOnError: false);
            }

            CollectTrivia(rule.Owner, ref token);
            JumpToEnd(ref token);
            return rule;
        }

        private CssKeyframesRule CreateKeyframes(CssKeyframesRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);
            rule.Name = GetRuleName(ref token);
            CollectTrivia(rule.Owner, ref token);

            if (token.Type != CssTokenType.CurlyBracketOpen)
            {
                SkipDeclarations(token);
                return null;
            }

            FillKeyframeRules(rule);
            return rule;
        }

        private CssMediaRule CreateMedia(CssMediaRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);
            var media = GetArgument(ref token);
            rule.Media.SetMediaText(media, throwOnError: false);
            CollectTrivia(rule.Owner, ref token);

            while (token.IsNot(CssTokenType.EndOfFile, CssTokenType.CurlyBracketOpen))
            {
                if (token.Type == CssTokenType.Semicolon)
                {
                    return null;
                }

                token = NextToken();
            }

            FillRules(rule);
            return rule;
        }

        private CssNamespaceRule CreateNamespace(CssNamespaceRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);
            rule.Prefix = GetRuleName(ref token);
            CollectTrivia(rule.Owner, ref token);

            if (!token.Is(CssTokenType.String, CssTokenType.Url))
            {
                RaiseErrorOccurred(CssParseError.InvalidToken, token.Position);
                JumpToEnd(ref token);
                return null;
            }

            rule.NamespaceUri = token.Data;
            JumpToEnd(ref token);
            return rule;
        }

        private CssPageRule CreatePage(CssPageRule rule, CssToken current)
        {
            current = NextToken();
            var selectorText = GetArgument(ref current);

            rule.SelectorText = selectorText;

            if (rule.Selector is null && _options.IsToleratingInvalidSelectors)
            {
                rule.SetInvalidSelector(selectorText);
            }

            CollectTrivia(rule.Owner, ref current);

            if (current.Type != CssTokenType.CurlyBracketOpen)
            {
                SkipDeclarations(current);
                return null;
            }

            FillDeclarations(rule.Owner, rule.Style, NextToken());
            return rule;
        }

        private CssSupportsRule CreateSupports(CssSupportsRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);
            var conditions = GetArgument(ref token);
            var result = rule.SetConditionText(conditions, throwOnError: false);
            CollectTrivia(rule.Owner, ref token);

            if (token.Type != CssTokenType.CurlyBracketOpen)
            {
                SkipDeclarations(token);
            }
            else if (FillRules(rule) && result)
            {
                return rule;
            }

            return null;
        }

        private CssFontFeatureValuesRule CreateFontFeatureValues(CssFontFeatureValuesRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);
            rule.FamilyName = GetArgument(ref token);

            if (token.Type == CssTokenType.CurlyBracketOpen)
            {
                JumpToRuleEnd(ref token);
                return rule;
            }

            SkipDeclarations(token);
            return null;
        }

        private CssCounterStyleRule CreateCounterStyle(CssCounterStyleRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);
            rule.StyleName = GetArgument(ref token);

            if (token.Type != CssTokenType.CurlyBracketOpen)
            {
                SkipDeclarations(token);
                return null;
            }

            FillDeclarations(rule);
            return rule;
        }

        private CssContainerRule CreateContainer(CssContainerRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);
            var condition = GetArgument(ref token);
            var result = rule.SetConditionText(condition, throwOnError: false);
            CollectTrivia(rule.Owner, ref token);

            if (token.Type != CssTokenType.CurlyBracketOpen)
            {
                SkipDeclarations(token);
            }
            else if (FillRules(rule) && result)
            {
                return rule;
            }

            return null;
        }

        private CssLayerRule CreateLayer(CssLayerRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);
            rule.Name = GetArgument(ref token);
            CollectTrivia(rule.Owner, ref token);

            if (token.Type == CssTokenType.Semicolon)
            {
                rule.IsStatement = true;
                return rule;
            }

            if (token.Type != CssTokenType.CurlyBracketOpen)
            {
                SkipDeclarations(token);
                return null;
            }

            FillRules(rule);
            return rule;
        }

        private CssScopeRule CreateScope(CssScopeRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);
            rule.ScopeText = GetArgument(ref token);
            CollectTrivia(rule.Owner, ref token);

            if (token.Type != CssTokenType.CurlyBracketOpen)
            {
                SkipDeclarations(token);
                return null;
            }

            FillRules(rule);
            return rule;
        }

        private CssPropertyRule CreateProperty(CssPropertyRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);
            rule.Name = GetArgument(ref token);
            CollectTrivia(rule.Owner, ref token);

            if (token.Type != CssTokenType.CurlyBracketOpen)
            {
                SkipDeclarations(token);
                return null;
            }

            FillDescriptors(rule);
            return rule;
        }

        private CssStartingStyleRule CreateStartingStyle(CssStartingStyleRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);

            if (token.Type != CssTokenType.CurlyBracketOpen)
            {
                SkipDeclarations(token);
                return null;
            }

            FillRules(rule);
            return rule;
        }

        private CssViewTransitionRule CreateViewTransition(CssViewTransitionRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);

            if (token.Type != CssTokenType.CurlyBracketOpen)
            {
                SkipDeclarations(token);
                return null;
            }

            FillDescriptors(rule);
            return rule;
        }

        private CssPositionTryRule CreatePositionTry(CssPositionTryRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);
            rule.Name = GetArgument(ref token);
            CollectTrivia(rule.Owner, ref token);

            if (token.Type != CssTokenType.CurlyBracketOpen)
            {
                SkipDeclarations(token);
                return null;
            }

            FillDeclarations(rule.Owner, rule.Style, NextToken());
            return rule;
        }

        private CssFontPaletteValuesRule CreateFontPaletteValues(CssFontPaletteValuesRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);
            rule.Name = GetArgument(ref token);
            CollectTrivia(rule.Owner, ref token);

            if (token.Type != CssTokenType.CurlyBracketOpen)
            {
                SkipDeclarations(token);
                return null;
            }

            FillDescriptors(rule);
            return rule;
        }

        private CssColorProfileRule CreateColorProfile(CssColorProfileRule rule, CssToken current)
        {
            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);
            rule.Name = GetArgument(ref token);
            CollectTrivia(rule.Owner, ref token);

            if (token.Type != CssTokenType.CurlyBracketOpen)
            {
                SkipDeclarations(token);
                return null;
            }

            FillDescriptors(rule);
            return rule;
        }

        public CssStyleRule CreateStyle(CssStyleRule rule, CssToken current)
        {
            CollectTrivia(rule.Owner, ref current);
            var selectorText = GetArgument(ref current);

            rule.SelectorText = selectorText;

            if (rule.Selector is null && _options.IsToleratingInvalidSelectors)
            {
                rule.SetInvalidSelector(selectorText);
            }

            if (current.Type != CssTokenType.CurlyBracketOpen)
            {
                SkipDeclarations(current);
                return null;
            }

            FillDeclarations(rule.Owner, rule.Style, NextToken());
            return rule;
        }

        public CssKeyframeRule CreateKeyframeRule(CssKeyframeRule rule, CssToken current)
        {
            CollectTrivia(rule.Owner, ref current);
            var position = current.Position;
            rule.KeyText = GetArgument(ref current);

            if (rule.Key is null)
            {
                RaiseErrorOccurred(CssParseError.InvalidKeyframe, position);
                JumpToRuleEnd(ref current);
                return null;
            }

            FillDeclarations(rule.Owner, rule.Style, NextToken());
            return rule;
        }

        private CssKeyframesRule FillKeyframeRules(CssKeyframesRule parentRule)
        {
            var token = NextToken();
            CollectTrivia(parentRule.Owner, ref token);

            while (token.IsNot(CssTokenType.EndOfFile, CssTokenType.CurlyBracketClose))
            {
                var rule = CreateKeyframeRule(new CssKeyframeRule(parentRule.Owner), token);
                token = NextToken();
                CollectTrivia(parentRule.Owner, ref token);

                if (rule is not null)
                {
                    parentRule.Add(rule);
                }
            }

            return parentRule;
        }

        private CssDeclarationRule FillDeclarations(CssDeclarationRule rule)
        {
            var previousRuleTarget = _commentRuleTarget;

            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);

            while (token.IsNot(CssTokenType.EndOfFile, CssTokenType.CurlyBracketClose))
            {
                CreateDeclarationWith(rule.Owner, rule, ref token);
                CollectTrivia(rule.Owner, ref token);
            }

            _commentRuleTarget = previousRuleTarget;

            return rule;
        }

        private CssDescriptorRule FillDescriptors(CssDescriptorRule rule)
        {
            var previousRuleTarget = _commentRuleTarget;

            var token = NextToken();
            CollectTrivia(rule.Owner, ref token);

            while (token.IsNot(CssTokenType.EndOfFile, CssTokenType.CurlyBracketClose))
            {
                CreateDeclarationWith(rule.Owner, rule, ref token);
                CollectTrivia(rule.Owner, ref token);
            }

            _commentRuleTarget = previousRuleTarget;

            return rule;
        }

        private CssUnknownRule CreateUnknownAtRule(ICssStyleSheet sheet, CssToken current)
        {
            var token = NextToken();

            while (token.IsNot(CssTokenType.CurlyBracketOpen, CssTokenType.Semicolon, CssTokenType.EndOfFile))
            {
                token = NextToken();
            }

            if (token.Type == CssTokenType.CurlyBracketOpen)
            {
                var curly = 1;

                do
                {
                    token = NextToken();

                    switch (token.Type)
                    {
                        case CssTokenType.CurlyBracketOpen:
                            curly++;
                            break;
                        case CssTokenType.CurlyBracketClose:
                            curly--;
                            break;
                        case CssTokenType.EndOfFile:
                            curly = 0;
                            break;
                    }
                }
                while (curly != 0);
            }

            var range = new TextRange(current.Position, token.Position);
            return new CssUnknownRule(sheet, current.Data, new TextView(sheet.Source, range));
        }

        private Boolean FillRules(CssGroupingRule group)
        {
            var previousRuleTarget = _commentRuleTarget;
            _commentRuleTarget = group.Rules as CssRuleList;

            var token = NextToken();
            CollectTrivia(group.Owner, ref token);

            while (token.IsNot(CssTokenType.EndOfFile, CssTokenType.CurlyBracketClose))
            {
                var rule = CreateRule(group.Owner, token, inGroup: true);
                if (rule != null)
                {
                    group.Add(rule);
                }

                token = NextToken();
                CollectTrivia(group.Owner, ref token);
            }

            _commentRuleTarget = previousRuleTarget;

            return token.Type == CssTokenType.CurlyBracketClose;
        }

        #endregion

        #region API

        /// <summary>
        /// Creates as many rules as possible.
        /// </summary>
        /// <returns>The found rules.</returns>
        public TextPosition CreateRules(CssStyleSheet sheet)
        {
            var previousRuleTarget = _commentRuleTarget;
            _commentRuleTarget = sheet.Rules as CssRuleList;

            var token = NextToken();
            CollectTrivia(sheet, ref token);

            while (token.Type != CssTokenType.EndOfFile)
            {
                var rule = CreateRule(sheet, token);
                if (rule is not null)
                {
                    sheet.Add(rule);
                }

                token = NextToken();
                CollectTrivia(sheet, ref token);
            }

            _commentRuleTarget = previousRuleTarget;

            return token.Position;
        }

        /// <summary>
        /// Fills the given parent style with declarations given by the tokens.
        /// </summary>
        public CssStyleDeclaration FillDeclarations(ICssStyleSheet owner, CssStyleDeclaration style, CssToken token)
        {
            var previousRuleTarget = _commentRuleTarget;

            CollectTrivia(owner, ref token);

            while (token.IsNot(CssTokenType.EndOfFile, CssTokenType.CurlyBracketClose))
            {
                CreateDeclarationWith(owner, style, ref token);
                CollectTrivia(owner, ref token);
            }

            _commentRuleTarget = previousRuleTarget;
            return style;
        }

        /// <summary>
        /// Called before the property name has been detected.
        /// </summary>
        public void CreateDeclarationWith(ICssStyleSheet owner, ICssProperties properties, ref CssToken token)
        {
            CollectTrivia(owner, ref token);
            var start = token.Position;

            if (!_options.IsExcludingNesting && token.IsPotentiallyNested() && properties is ICssStyleDeclaration decl && decl.Parent is CssStyleRule style)
            {
                var factory = _context.GetService<DefaultAttributeSelectorFactory>();
                var rule = new CssStyleRule(style.Owner);
                var previous = factory.Unregister("&");
                factory.Register("&", (_, _, _, _) =>
                {
                    rule.IsNested = true;
                    return new ReferencedNestedSelector(style.Selector);
                });
                var result = CreateStyle(rule, token);
                factory.Unregister("&");

                if (previous is not null)
                {
                    factory.Register("&", previous);
                }

                if (result is not null)
                {
                    style.Add(result);
                    token = NextToken();
                    return;
                }
            }

            if (token.IsNot(CssTokenType.EndOfFile, CssTokenType.CurlyBracketClose, CssTokenType.Colon) &&
                token.IsNot(CssTokenType.Semicolon, CssTokenType.CurlyBracketOpen))
            {
                var name = token.Data;

                if (token.Type == CssTokenType.Delim)
                {
                    var sb = StringBuilderPool.Obtain();
                    sb.Append(name);

                    while (token.Type == CssTokenType.Delim)
                    {
                        token = NextToken();
                        sb.Append(token.Data);
                    }

                    name = sb.ToPool();
                }

                token = NextToken();
                CollectTrivia(owner, ref token);

                if (token.Type == CssTokenType.Colon)
                {
                    token = NextToken();
                    CollectValueLead(owner, ref token);
                    var value = CreateValue(ref token, out var important);

                    // A declaration's value is a list of component values that holds nothing a
                    // semicolon or a bang would have ended it at, and a custom property reads that
                    // production as its whole grammar - which is the one thing that lets its value be
                    // nothing at all (CSS Syntax 3 §5.4.3, CSS Variables 1 §3, §3.1).
                    var stated = value.IsDeclarationValue(
                        allowEmpty: name.StartsWith("--", StringComparison.Ordinal),
                        pastTheEnd: !_tokenizer.BadString);

                    if (!stated)
                    {
                        RaiseErrorOccurred(CssParseError.ValueMissing, token.Position);
                    }
                    else
                    {
                        properties.SetProperty(name, value, important ? CssKeywords.Important : null);
                    }
                }
                else
                {
                    RaiseErrorOccurred(CssParseError.ColonMissing, token.Position);
                }

                JumpToDeclEnd(ref token);
            }
            else if (token.Type != CssTokenType.EndOfFile)
            {
                RaiseErrorOccurred(CssParseError.IdentExpected, start);
                JumpToDeclEnd(ref token);
            }

            if (token.Type == CssTokenType.Semicolon)
            {
                token = NextToken();
            }
        }

        #endregion

        #region Helpers

        private void JumpToEnd(ref CssToken current)
        {
            while (current.IsNot(CssTokenType.EndOfFile, CssTokenType.Semicolon))
            {
                current = NextToken();
            }
        }

        private void JumpToRuleEnd(ref CssToken current)
        {
            var scopes = 0;

            while (current.Type != CssTokenType.EndOfFile)
            {
                if (current.Type == CssTokenType.CurlyBracketOpen)
                {
                    scopes++;
                }
                else if (current.Type == CssTokenType.CurlyBracketClose)
                {
                    scopes--;
                }

                if (scopes <= 0 && (current.Is(CssTokenType.CurlyBracketClose, CssTokenType.Semicolon)))
                {
                    break;
                }

                current = NextToken();
            }
        }

        private void JumpToArgEnd(ref CssToken current)
        {
            var arguments = 0;

            while (current.Type != CssTokenType.EndOfFile)
            {
                if (current.Type == CssTokenType.RoundBracketOpen)
                {
                    arguments++;
                }
                else if (arguments <= 0 && current.Type == CssTokenType.RoundBracketClose)
                {
                    break;
                }
                else if (current.Type == CssTokenType.RoundBracketClose)
                {
                    arguments--;
                }

                current = NextToken();
            }
        }

        private void JumpToDeclEnd(ref CssToken current)
        {
            var scopes = 0;

            while (current.Type != CssTokenType.EndOfFile)
            {
                if (current.Type == CssTokenType.CurlyBracketOpen)
                {
                    scopes++;
                }
                else if (scopes <= 0 && (current.Is(CssTokenType.CurlyBracketClose, CssTokenType.Semicolon)))
                {
                    break;
                }
                else if (current.Type == CssTokenType.CurlyBracketClose)
                {
                    scopes--;
                }

                current = NextToken();
            }
        }

        private CssToken NextToken() => _tokenizer.Get();

        private void CollectTrivia(ICssStyleSheet owner, ref CssToken token)
        {
            while (token.Type == CssTokenType.Whitespace || token.Type == CssTokenType.Comment || token.Type == CssTokenType.Cdc || token.Type == CssTokenType.Cdo)
            {
                if (token.Type == CssTokenType.Comment)
                {
                    _commentRuleTarget?.Add(new CssCommentRule(owner, token.Data));
                }

                token = _tokenizer.Get();
            }
        }

        /// <summary>
        /// Takes the white space and the comment in front of a declaration's value, and nothing else.
        /// </summary>
        /// <remarks>
        /// Between declarations a comment-open or a comment-close is the token that ends a statement
        /// list, so <see cref="CollectTrivia" /> steps over it; inside one it is a component value of
        /// the value itself, and the value begins where it stands (CSS Syntax 3 §5.4.3).
        /// </remarks>
        private void CollectValueLead(ICssStyleSheet owner, ref CssToken token)
        {
            while (token.Type == CssTokenType.Whitespace || token.Type == CssTokenType.Comment)
            {
                if (token.Type == CssTokenType.Comment)
                {
                    _commentRuleTarget?.Add(new CssCommentRule(owner, token.Data));
                }

                token = _tokenizer.Get();
            }
        }

        private void SkipDeclarations(CssToken token)
        {
            RaiseErrorOccurred(CssParseError.InvalidToken, token.Position);
            JumpToRuleEnd(ref token);
        }

        private void RaiseErrorOccurred(CssParseError code, TextPosition position) =>
            _tokenizer.RaiseErrorOccurred(code, position);

        #endregion

        #region Fill Inner

        /// <summary>
        /// Takes the declaration's value, with the important annotation off its end when it carries one.
        /// </summary>
        /// <remarks>
        /// CSS Syntax 3 §5.4.3 writes the annotation as an exclamation mark, any amount of white space,
        /// and the keyword, which is the keyword whatever its case. <c>!important</c> and
        /// <c>! important</c> are therefore the same annotation, and a value that merely ends in those
        /// letters - <c>serif important</c> - carries one only where the mark stands in front of them. A
        /// mark inside a quoted string cannot be the annotation either, since the string's closing quote
        /// then stands between the mark and the end of the value.
        /// </remarks>
        private String CreateValue(ref CssToken token, out Boolean important)
        {
            var value = _tokenizer.ContentFrom(token.Position.Position, inValue: true);
            var end = TrimEnd(value, value.Length);
            var keyword = CssKeywords.Important;
            var at = end - keyword.Length;

            important = false;

            if (at >= 0 &&
                String.Compare(value, at, keyword, 0, keyword.Length, StringComparison.OrdinalIgnoreCase) == 0)
            {
                var mark = TrimEnd(value, at);

                if (mark > 0 && value[mark - 1] == Symbols.ExclamationMark)
                {
                    important = true;
                    end = TrimEnd(value, mark - 1);
                }
            }

            token = NextToken();
            return value.Substring(0, end);
        }

        /// <summary>Gets how far the value reaches before the white space that ends at <paramref name="end"/>.</summary>
        private static Int32 TrimEnd(String value, Int32 end)
        {
            while (end > 0 && value[end - 1].IsSpaceCharacter())
            {
                end--;
            }

            return end;
        }

        private String GetArgument(ref CssToken token)
        {
            var argument = _tokenizer.ContentFrom(token.Position.Position);
            token = NextToken();
            return argument;
        }

        private String GetRuleName(ref CssToken token)
        {
            var name = String.Empty;

            if (token.Type == CssTokenType.Ident)
            {
                name = token.Data;
                token = NextToken();
            }

            return name;
        }

        #endregion
    }
}
