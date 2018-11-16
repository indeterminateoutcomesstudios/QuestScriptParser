using System;
using Antlr4.Runtime;

namespace QuestScript.Interpreter.ValidationExceptions
{
    public class UndefinedSymbolException : Exception
    {
        public string Symbol { get; }
        public ParserRuleContext SymbolContext { get; }

        public UndefinedSymbolException(string symbol, ParserRuleContext symbolContext, string symbolDescription = null) : base($"At line {symbolContext.start.Line} and column {symbolContext.start.Column} I found {symbolDescription ?? "symbol"} I couldn't recognize. Can you make sure '{symbol}' is defined?")
        {                       
            Symbol = symbol;
            SymbolContext = symbolContext;
        }
    }
}
