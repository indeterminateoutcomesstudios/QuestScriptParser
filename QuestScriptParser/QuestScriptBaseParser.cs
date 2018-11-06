using Antlr4.Runtime;

namespace QuestScriptParser
{
    //credit : adapted from https://github.com/antlr/grammars-v4/blob/master/javascript/CSharpSharwell/JavaScriptBaseParser.cs
    public abstract class QuestScriptBaseParser : Parser
    {
        /// <summary>
        /// Short form for Prev(String str)
        /// </summary>
        public bool P(string str)
        {
            return Prev(str);
        }

        /// <summary>
        /// Whether the Previous token value equals to str
        /// </summary>
        public bool Prev(string str)
        {
            return _input.Lt(-1).Text.Equals(str);
        }

        // Short form for Next(String str)
        public bool N(string str)
        {
            return Next(str);
        }

        // Whether the next token value equals to @param str
        public bool Next(string str)
        {
            return _input.Lt(1).Text.Equals(str);
        }

        public bool NotLineTerminator()
        {
            return !Here(QuestScriptLexer.Newline);
        }

        public bool NotOpenBrace()
        {
            int nextTokenType = _input.Lt(1).Type;
            return nextTokenType != QuestScriptLexer.OpenCurlyBraceToken;
        }

        public bool CloseBrace()
        {
            return _input.Lt(1).Type == QuestScriptLexer.CloseCurlyBraceToken;
        }

        /// <summary>Returns true if on the current index of the parser's
        /// token stream a token of the given type exists on the
        /// Hidden channel.
        /// </summary>
        /// <param name="type">
        /// The type of the token on the Hidden channel to check.
        /// </param>
        public bool Here(int type)
        {
            // Get the token ahead of the current index.
            int possibleIndexEosToken = CurrentToken.TokenIndex - 1;
            IToken ahead = _input.Get(possibleIndexEosToken);

            // Check if the token resides on the Hidden channel and if it's of the
            // provided type.
            return ahead.Channel == Lexer.Hidden && ahead.Type == type;
        }

        /// <summary>
        /// Returns true if on the current index of the parser's
        /// token stream a token exists on the Hidden channel which
        /// either is a line terminator, or is a multi line comment that
        /// contains a line terminator.
        /// </summary>
        public bool LineTerminatorAhead()
        {
            // Get the token ahead of the current index.
            int possibleIndexEosToken = CurrentToken.TokenIndex - 1;
            IToken ahead = _input.Get(possibleIndexEosToken);

            if (ahead.Channel != Lexer.Hidden)
            {
                // We're only interested in tokens on the Hidden channel.
                return false;
            }

            if (ahead.Type == QuestScriptLexer.Newline)
            {
                // There is definitely a line terminator ahead.
                return true;
            }

            if (ahead.Type == QuestScriptLexer.Whitespace)
            {
                // Get the token ahead of the current whitespaces.
                possibleIndexEosToken = CurrentToken.TokenIndex - 2;
                ahead = _input.Get(possibleIndexEosToken);
            }

            // Get the token's text and type.
            string text = ahead.Text;
            int type = ahead.Type;

            // Check if the token is, or contains a line terminator.
            return (type == QuestScriptLexer.BlockComment && (text.Contains("\r") || text.Contains("\n"))) ||
                   (type == QuestScriptLexer.LineTerminator);
        }

        protected QuestScriptBaseParser(ITokenStream input) : base(input)
        {
        }
    }

}
