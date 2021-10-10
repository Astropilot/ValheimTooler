using System.Collections.Generic;
using System.Text;
using IniParser.Model;
using IniParser.Parser;
using UnityEngine;

namespace ValheimTooler.Utils
{
    public class VTLocalization
    {
        public static readonly string s_cheatOn = "$vt_cheat_on";
        public static readonly string s_cheatOff = "$vt_cheat_off";

        private static VTLocalization s_instance;
        private static readonly char[] s_endChars = " (){}[]+-!?/\\\\&%,.:-=<>\n".ToCharArray();
        private Dictionary<string, string> m_translations = new Dictionary<string, string>();

        public static VTLocalization instance
        {
            get
            {
                if (s_instance == null)
                {
                    Initialize();
                }
                return s_instance;
            }
        }

        private static void Initialize()
        {
            if (s_instance == null)
            {
                s_instance = new VTLocalization();
            }
        }
        private VTLocalization()
        {
            SetupLanguage("English");
            string prefLanguage = PlayerPrefs.GetString("language", "");
            if (prefLanguage.Length > 0)
            {
                SetupLanguage(prefLanguage);
            }
        }
        public string Localize(string text)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int num = 0;
            string word;
            int num2;
            int num3;
            while (FindNextWord(text, num, out word, out num2, out num3))
            {
                stringBuilder.Append(text.Substring(num, num2 - num));
                stringBuilder.Append(Translate(word));
                num = num3;
            }
            stringBuilder.Append(text.Substring(num));
            return stringBuilder.ToString();
        }
        private bool FindNextWord(string text, int startIndex, out string word, out int wordStart, out int wordEnd)
        {
            if (startIndex >= text.Length - 1)
            {
                word = null;
                wordStart = -1;
                wordEnd = -1;
                return false;
            }
            wordStart = text.IndexOf('$', startIndex);
            if (wordStart != -1)
            {
                int num = text.IndexOfAny(s_endChars, wordStart);
                if (num != -1)
                {
                    word = text.Substring(wordStart + 1, num - wordStart - 1);
                    wordEnd = num;
                }
                else
                {
                    word = text.Substring(wordStart + 1);
                    wordEnd = text.Length;
                }
                return true;
            }
            word = null;
            wordEnd = -1;
            return false;
        }
        private string Translate(string word)
        {
            if (word.StartsWith("KEY_"))
            {
                string bindingName = word.Substring(4);
                return GetBoundKeyString(bindingName);
            }
            string result;
            if (m_translations.TryGetValue(word, out result))
            {
                return result;
            }
            return "[" + word + "]";
        }
        public string GetBoundKeyString(string bindingName)
        {
            string boundKeyString = ZInput.instance.GetBoundKeyString(bindingName);
            string result;
            if (boundKeyString.Length > 0 && boundKeyString[0] == '$' && m_translations.TryGetValue(boundKeyString.Substring(1), out result))
            {
                return result;
            }
            return boundKeyString;
        }
        private void AddWord(string key, string text)
        {
            m_translations.Remove(key);
            m_translations.Add(key, text);
        }

        private void Clear()
        {
            m_translations.Clear();
        }

        public bool SetupLanguage(string language)
        {
            if (!LoadINI(language))
            {
                return false;
            }
            return true;
        }

        public bool LoadINI(string language)
        {
            var translationFile = ResourceUtils.GetEmbeddedResource("Resources.Localization.translations.ini", null);

            if (translationFile == null)
            {
                ZLog.Log("Failed to load the translation file!");
                return false;
            }

            var translationFileContents = System.Text.Encoding.UTF8.GetString(translationFile, 0, translationFile.Length);
            var parser = new IniDataParser();
            IniData iniData = parser.Parse(translationFileContents);

            if (!iniData.Sections.ContainsSection(language))
            {
                ZLog.Log("Failed to find language: " + language);
                return false;
            }

            SectionData sectionData = iniData.Sections.GetSectionData(language);
            foreach (KeyData key in sectionData.Keys)
            {
                AddWord(key.KeyName, key.Value);
            }
            return true;
        }
    }
}
