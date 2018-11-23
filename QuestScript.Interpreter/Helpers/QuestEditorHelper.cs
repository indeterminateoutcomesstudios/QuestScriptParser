using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace QuestScript.Interpreter.Helpers
{
    public static class QuestEditorHelper
    {        

        public static string GetQuestInstallationPath()
        {
            using (var searcher = new ManagementObjectSearcher(new ObjectQuery("")))
            {
                using (var results = searcher.Get())
                {
                }
            }
            return null;
        }
    }
}
