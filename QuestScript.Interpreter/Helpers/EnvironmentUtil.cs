using Microsoft.Win32;

namespace QuestScript.Interpreter.Helpers
{
    public static class EnvironmentUtil
    {
        public static string GetQuestInstallationPath()
        {
            const string uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (var rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
            {
                if (rk == null)
                    return null;

                foreach (var skName in rk.GetSubKeyNames())
                {
                    if (string.IsNullOrWhiteSpace(skName)) //just in case
                        continue;
                    using (var sk = rk.OpenSubKey(skName))
                    {
                        if ((sk?.GetValue("DisplayName")?.ToString() ?? string.Empty).StartsWith("Quest"))
                            return (string) sk?.GetValue("InstallLocation");
                    }
                }
            }

            return null;
        }
    }
}