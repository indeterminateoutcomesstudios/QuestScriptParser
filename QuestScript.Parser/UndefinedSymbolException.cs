using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestScript.Parser
{
    public class UndefinedSymbolException : Exception
    {
        public UndefinedSymbolException(string symbol, int row, int col) : base($"I see a symbol ({symbol}) that I don't recognize on line {row} and column {col}. Please make sure that you have defined it properly and didn't mistype the name.")
        {
        }
    }
}
