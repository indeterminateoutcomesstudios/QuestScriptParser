using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;
using QuestScript.Interpreter.Exceptions;

namespace QuestScript.Interpreter
{
    public class ParseErrorGatherer : BaseErrorListener
    {
        public HashSet<BaseParserErrorException> Errors { get; }

        public ParseErrorGatherer(HashSet<BaseParserErrorException> errors)
        {
            Errors = errors ?? new HashSet<BaseParserErrorException>();
        }

        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg,
            RecognitionException e)
        {            
            Errors.Add(new SyntaxErrorException($"Syntax error while parsing '{offendingSymbol.Text}'. ",e));
        }

        public override void ReportAmbiguity(Antlr4.Runtime.Parser recognizer, DFA dfa, int startIndex, int stopIndex, bool exact, BitSet ambigAlts,
            ATNConfigSet configs)
        {            
            base.ReportAmbiguity(recognizer, dfa, startIndex, stopIndex, exact, ambigAlts, configs);
        }
    }
}
