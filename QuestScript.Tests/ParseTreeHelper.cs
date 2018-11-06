using System;
using Xunit;

namespace QuestScriptParser
{
    //credit : https://codereview.stackexchange.com/questions/97293/unit-test-code-for-antlr-grammar
    internal static class ParseTreeHelper
    {
        internal static void AssertTreesAreEqual(string expected, string actual)
        {
            if (expected == null || actual == null)
            {
                Assert.True(false, "Expected and/or Actual are null.");
            }
            var filteredExpected = RemoveLinebreaks(RemoveWhiteSpace(expected));
            var filteredActual = RemoveLinebreaks(RemoveWhiteSpace(actual));
            Assert.Equal(filteredExpected, filteredActual);
        }

        private static string RemoveLinebreaks(string input) => input.Replace("\r\n", string.Empty)
            .Replace("\n", String.Empty).Replace("\r", String.Empty);

        private static string RemoveWhiteSpace(string input)
        {
            // the final \\t replacement is necessary because antlr seems to add it to the ToStringTree method. 
            return input.Replace("\t", "").Replace(" ", "").Replace("\\t", "");
        }
    }}
