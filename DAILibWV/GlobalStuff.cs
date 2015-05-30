using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAILibWV
{
    public static class GlobalStuff
    {
        public static Dictionary<string, string> settings;

        public static string FindSetting(string name)
        {
            foreach (KeyValuePair<string, string> setting in settings)
                if (setting.Key == name)
                    return setting.Value;
            return "";
        }

        public static void AssignSetting(string key, string value)
        {
            settings[key] = value;
            DBAccess.SaveSettings();
        }
    }
}
