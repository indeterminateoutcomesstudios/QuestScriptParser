using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace QuestScriptParser
{
    //credit : https://codereview.stackexchange.com/questions/97293/unit-test-code-for-antlr-grammar
    internal static class ParseTreeHelper
    {
        internal static void TreesAreEqual(string expected, string actual)
        {
            if (expected == null || actual == null)
            {
                Assert.True(false, "Expected and/or Actual are null.");
            }
            var filteredExpected = RemoveWhiteSpace(expected);
            var filteredActual = RemoveWhiteSpace(actual);
            Assert.Equal(filteredExpected, filteredActual);
        }

        private static string RemoveWhiteSpace(string input)
        {
            // the final \\t replacement is necessary because antlr seems to add it to the ToStringTree method. 
            return input.Replace("\t", "").Replace(" ", "").Replace("\\t", "");
        }
    }}
