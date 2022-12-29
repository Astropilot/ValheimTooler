from pathlib import Path
import configparser
import re
import sys
from typing import Dict, List, Set


VALHEIMTOOLER_PATH = Path("../ValheimTooler")
VALHEIMTOOLER_TRANSLATIONS_PATH = Path("../ValheimTooler/Resources/Localization/translations.cfg")
REGEX_TRANSLATION = re.compile("\$(vt_[a-zA-Z_]+)")

if not VALHEIMTOOLER_PATH.exists():
  print("[ERROR] Cannot find the path to ValheimTooler!")
  sys.exit(-1)

if not VALHEIMTOOLER_TRANSLATIONS_PATH.exists():
  print("[ERROR] Cannot find translations.cfg file!")
  sys.exit(-1)

valheim_translations_config = configparser.ConfigParser()

valheim_translations_config.read(VALHEIMTOOLER_TRANSLATIONS_PATH)

if (len(valheim_translations_config.sections()) == 0):
  print("[ERROR] Cannot open file translations.cfg!")
  sys.exit(-1)

translations: Dict[str, Set[str]] = {}

for language in valheim_translations_config.sections():
  translations[language] = set()

  for key in valheim_translations_config[language]:
    translations[language].add(key)

for cs_filepath in VALHEIMTOOLER_PATH.rglob('*.cs'):
  file_line_idx = 1
  with open(cs_filepath) as f:
    for line in f:
      translation_code_matches: List[str] = re.findall(REGEX_TRANSLATION, line)

      for translation_code in translation_code_matches:
        for language in translations.keys():
          if translation_code not in translations[language]:
            print(f"Missing translation for key {translation_code} for language {language} in file {cs_filepath} on line {file_line_idx}")

      file_line_idx += 1
