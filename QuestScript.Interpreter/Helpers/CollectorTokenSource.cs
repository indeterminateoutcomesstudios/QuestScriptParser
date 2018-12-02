using System.Collections.Generic;
using Antlr4.Runtime;
using QuestScript.Parser;

namespace QuestScript.Interpreter.Helpers
{
    //credit for implementation ideas : http://meri-stuff.blogspot.com/2012/09/tackling-comments-in-antlr-compiler.html
    public class CollectorTokenSource : ITokenSource
    {
        private readonly HashSet<IToken> _commentTokens = new HashSet<IToken>();
        private readonly ITokenSource _source;

        public CollectorTokenSource(ITokenSource source)
        {
            _source = source;
        }

        public IReadOnlyCollection<IToken> CommentTokens => _commentTokens;

        public IToken NextToken()
        {
            var next = _source.NextToken();
            //collect the tokens, but don't send them to parser
            while (next.Type == QuestScriptLexer.Comment ||
                   next.Type == QuestScriptLexer.LineComment)
            {
                _commentTokens.Add(next);
                next = _source.NextToken();
            }

            return next;
        }

        public int Line => _source.Line;
        public int Column => _source.Column;
        public ICharStream InputStream => _source.InputStream;
        public string SourceName => _source.SourceName;

        public ITokenFactory TokenFactory
        {
            get => _source.TokenFactory;
            set => _source.TokenFactory = value;
        }
    }
}