using System;
using System.Linq;
using QuestScript.Parser.Tokens;
using Xunit;

namespace QuestScript.Parser.Tests
{
    public class LexerTests
    {
        [Fact]
        public void Can_recognize_identifier()
        {
            var tokens = ScriptLexer.Instance.Tokenize("ThisIsATest");
            
            int count = 0;
            foreach (var _ in tokens) 
                count++;
            Assert.Equal(1,count);

            var token = tokens.ConsumeToken();
            
            Assert.Null(token.ErrorMessage);
            Assert.Equal(ScriptToken.Identifier, token.Value.Kind);
        }

        [Fact]
        public void Can_recognize_identifier_with_other_tokens()
        {
            var tokens = ScriptLexer.Instance.Tokenize("23 ThisIsATest(1,2)");
            tokens = tokens.ConsumeToken().Remainder; //skip the first token
            var token = tokens.ConsumeToken();
            Assert.Null(token.ErrorMessage);
            Assert.Equal(ScriptToken.Identifier, token.Value.Kind);
            Assert.Equal("ThisIsATest", token.Value.ToStringValue());
        }

        [Fact]
        public void Can_recognize_special_identifier()
        {
            var tokens = ScriptLexer.Instance.Tokenize("23 a b()");
            
            int count = 0;
            foreach (var _ in tokens) 
                count++;
            Assert.Equal(4,count); //the tokens also
            tokens = tokens.ConsumeToken().Remainder; //skip the first token

            var token = tokens.ConsumeToken();
            Assert.Null(token.ErrorMessage);
            Assert.Equal(ScriptToken.Identifier, token.Value.Kind);
            Assert.Equal("a b", token.Value.ToStringValue());
        }      
    }
}
