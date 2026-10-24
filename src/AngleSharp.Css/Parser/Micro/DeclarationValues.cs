#nullable disable
namespace AngleSharp.Css.Parser
{
    using System;

    /// <summary>
    /// The token-level reading of a declaration's value, as CSS Syntax 3 §7.2 defines it.
    /// </summary>
    /// <remarks>
    /// A <c>&lt;declaration-value&gt;</c> is any sequence of one or more tokens that holds no bad-string, no
    /// unmatched closing delimiter, and no top-level semicolon or bang. "Top level" means outside every group
    /// the value opens, and CSS Variables 1 §3.1 makes the argument of a <c>var()</c> reference its own top
    /// level - which is what tells a fallback holding a bang from one holding a parenthesis with a semicolon
    /// inside it. A condition asking about a declaration whose value names a variable is answered by this
    /// production alone rather than by the property's own grammar, because the value the substitution will
    /// come to is not known yet, and a custom property has no other grammar than this one.
    /// </remarks>
    public static class DeclarationValues
    {
        /// <summary>The deepest nesting of groups this keeps track of.</summary>
        const Int32 Depth = 32;

        /// <summary>Gets whether the whole of <paramref name="text"/> is a value a declaration may carry.</summary>
        public static Boolean IsDeclarationValue(this String text, Boolean allowEmpty = false) =>
            text is not null && IsStated(text, 0, text.Length, allowEmpty);

        /// <summary>Gets whether the text between two places is a value a declaration may carry.</summary>
        static Boolean IsStated(String text, Int32 at, Int32 end, Boolean allowEmpty = false)
        {
            Span<Boolean> whole = stackalloc Boolean[Depth + 1];

            // The level the value begins at is its own top level, where a semicolon would end the
            // declaration and a bang would start a priority.
            whole[0] = true;

            var level = 0;
            var tokens = 0;

            while (at < end)
            {
                var character = text[at];

                if (Char.IsWhiteSpace(character))
                {
                    at++;
                    continue;
                }

                if (character == '/' && at + 1 < end && text[at + 1] == '*')
                {
                    var closed = text.IndexOf("*/", at + 2, end - at - 2, StringComparison.Ordinal);

                    if (closed < 0)
                    {
                        // An unterminated comment runs to the end of the value and leaves no token.
                        break;
                    }

                    at = closed + 2;
                    continue;
                }

                if (character == '"' || character == '\'')
                {
                    if (!ReadString(text, ref at, end, character))
                    {
                        return false;
                    }

                    tokens++;
                    continue;
                }

                if (character == '\\')
                {
                    at = ReadEscape(text, at, end);
                    tokens++;
                    continue;
                }

                if (character == '<' && end - at >= 4 && text[at + 1] == '!' && text[at + 2] == '-' && text[at + 3] == '-')
                {
                    // A comment-open is one token, and the bang inside it is not the delimiter that would
                    // end a declaration. The same goes for the comment-close at the other end.
                    at += 4;
                    tokens++;
                    continue;
                }

                if (character == '-' && end - at >= 3 && text[at + 1] == '-' && text[at + 2] == '>')
                {
                    at += 3;
                    tokens++;
                    continue;
                }

                if (whole[level] && (character == ';' || character == '!'))
                {
                    // Both end a declaration, and so belong at the top of nothing written after a colon.
                    // Inside a group of the value they are nothing but punctuation.
                    return false;
                }

                if (character == ')' || character == ']' || character == '}')
                {
                    if (level == 0)
                    {
                        // Nothing opened this group, so the sequence holds an unmatched closing delimiter.
                        return false;
                    }

                    level--;
                    at++;
                    continue;
                }

                var start = at;

                while (at < end && IsIdent(text[at]))
                {
                    at++;
                }

                if (at < end && text[at] == '(')
                {
                    if (level >= Depth)
                    {
                        return true;
                    }

                    // A variable reference carries a value of this production as its argument; any other
                    // function's is a group of its own. Either way the function is a token of the value,
                    // even one written with nothing inside it.
                    level++;
                    whole[level] = at - start == 3 && IsVar(text, start);
                    tokens++;
                    at++;
                    continue;
                }

                if (at == start)
                {
                    if (character == '{' || character == '[' || character == '(')
                    {
                        if (level >= Depth)
                        {
                            return true;
                        }

                        level++;
                        whole[level] = false;
                    }

                    tokens++;
                    at++;
                    continue;
                }

                tokens++;
            }

            return allowEmpty || tokens > 0;
        }

        /// <summary>Reads a quoted string, leaving the cursor past its closing quote.</summary>
        static Boolean ReadString(String text, ref Int32 at, Int32 end, Char quote)
        {
            at++;

            while (at < end)
            {
                var character = text[at];

                if (character == '\\')
                {
                    at = ReadEscape(text, at, end);
                    continue;
                }

                at++;

                if (character == quote)
                {
                    return true;
                }

                if (IsBreak(character))
                {
                    // The line ended before the string did, which is a bad-string token.
                    return false;
                }
            }

            return false;
        }

        /// <summary>Gets whether the three characters here begin the name of a variable reference.</summary>
        static Boolean IsVar(String text, Int32 at) =>
            (text[at] == 'v' || text[at] == 'V') &&
            (text[at + 1] == 'a' || text[at + 1] == 'A') &&
            (text[at + 2] == 'r' || text[at + 2] == 'R');

        /// <summary>Moves the cursor over one escape: a backslash, its hex digits, and the space that ends them.</summary>
        static Int32 ReadEscape(String text, Int32 at, Int32 end)
        {
            at++;

            if (at >= end)
            {
                return at;
            }

            if (!IsHex(text[at]))
            {
                return at + 1;
            }

            var digits = 0;

            while (at < end && IsHex(text[at]) && digits < 6)
            {
                at++;
                digits++;
            }

            return at < end && text[at] == ' ' ? at + 1 : at;
        }

        static Boolean IsIdent(Char character) =>
            Char.IsLetterOrDigit(character) || character == '-' || character == '_' || character == '\\' ||
            character > '\u00A0';

        static Boolean IsHex(Char character) =>
            (character >= '0' && character <= '9') ||
            (character >= 'a' && character <= 'f') ||
            (character >= 'A' && character <= 'F');

        static Boolean IsBreak(Char character) =>
            character == '\n' || character == '\r' || character == '\f';
    }
}
