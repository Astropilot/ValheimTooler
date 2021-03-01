using IniParser;
using IniParser.Model;
using IniParser.Parser;

namespace ValheimTooler.Utils
{
    public static class Translator
    {
        public static readonly string s_cheatOn = "$vt_cheat_on";
        public static readonly string s_cheatOff = "$vt_cheat_off";
        public static void InitLocalization(string language)
        {
            var translationFile = ResourceUtils.GetEmbeddedResource("Resources.Localization.translations.ini", null);

            if (translationFile == null)
            {
                ZLog.Log("Failed to load the translation file!");
                return;
            }

            var translationFileContents = System.Text.Encoding.UTF8.GetString(translationFile, 0, translationFile.Length);
            var parser = new IniDataParser();
            IniData iniData = parser.Parse(translationFileContents);

            if (iniData.Sections.ContainsSection(language))
            {
                SectionData sectionData = iniData.Sections.GetSectionData(language);
                foreach (KeyData key in sectionData.Keys)
                {
                    Localization.instance.CallMethod("AddWord", key.KeyName, key.Value);
                }
            }
        }

        public static string Localize(string text)
        {
            return Localization.instance.Localize(text);
        }
    }
}
