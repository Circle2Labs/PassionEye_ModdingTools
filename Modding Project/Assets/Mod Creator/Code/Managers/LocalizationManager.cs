#define LOCALIZATIONMANAGER_STANDALONE

using System.Collections.Generic;
using System.IO;
using Code.Managers.Events;
using Code.Tools;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Code.Managers
{
	public class LocalizationManager
	{
		private static LocalizationManager instance;
		public static LocalizationManager Instance
		{
			get
			{
				if (instance != null) 
					return instance;
				
				instance = new LocalizationManager();
				instance.LoadAllLanguages();

				return instance;
			}
		}
		
		public LanguageChangedEvent LanguageChangedEvent = new ();

		private Dictionary<string, LanguageData> languages = new ();
		
#if LOCALIZATIONMANAGER_STANDALONE
		private string filePath = "Assets/Mod Creator/data/languages";
#else
		private string filePath = "data/languages";
#endif
		private string currentLanguage;

		public LanguageDataEntry GetLocalizedString(string language, string key, params string[] parameters)
		{
			if (string.IsNullOrEmpty(language) || !languages.TryGetValue(language, out var languageData) || !languageData.Entries.TryGetValue(key, out var localized))
				return new LanguageDataEntry { Value = key, File = null, FontSizeOffset = 0 };

			if (parameters == null || parameters.Length == 0) 
				return localized;
			
			var entry = new LanguageDataEntry { Value = localized.Value, File = localized.File, FontSizeOffset = localized.FontSizeOffset };

			for (var i = 0; i < parameters.Length; i++)
				entry.Value = entry.Value.Replace("{{" + i + "}}", parameters[i]);
			
			return entry;
		}

		public LanguageDataEntry GetLocalizedString(string key, params string[] parameters)
		{
			return GetLocalizedString(GetCurrentLanguage(), key, parameters);
		}
		
		public LanguageData GetLanguageData(string language)
		{
			if (!languages.TryGetValue(language, out var languageData))
				return null;
			
			return languageData;
		}

		public List<string> GetAvailableLanguages()
		{
			var list = new List<string>();
			
			foreach (var language in languages.Keys)
				list.Add(language);
			
			return list;
		}

		public string GetCurrentLanguage()
		{
			return currentLanguage;
		}
		
		public void SetLanguage(string language)
		{
			if (string.IsNullOrEmpty(language) || !languages.ContainsKey(language))
				return;

			var previous = currentLanguage;
			currentLanguage = language;
			
			LanguageChangedEvent.Invoke(previous, language);
		}
		
		public void LoadAllLanguages()
		{
			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			foreach (var directory in Directory.GetDirectories(filePath))
				loadLanguage(Path.GetFileNameWithoutExtension(directory));

			createMissingFilesAndEntries();
		}

		private void loadLanguage(string language, string directory = null)
		{
			directory ??= Path.Combine(filePath, language);
			
			if (!Directory.Exists(directory))
			{
				Debug.LogError($"[LocalizationManager] Failed to load language {language} because its directory does not exist");
				return;
			}
			
			var data = new LanguageData();
			data.Entries = new Dictionary<string, LanguageDataEntry>();
			data.Language = language;

			var files = 0;
				
			foreach (var file in Directory.GetFiles(directory, "*.tsv", SearchOption.AllDirectories))
			{
				if (!FileTools.ReadAllLinesSafe(file, out var lines))
				{
					Debug.LogError($"[LocalizationManager] Failed to read language file {file} for language {language}, skipping");
					continue;
				}
					
				if (lines.Length == 0)
				{
					Debug.LogWarning($"[LocalizationManager] Skipping language file {file} for language {language} because it is empty");
					continue;
				}

				var entries = parseLanguageEntries(language, file, lines);
				if (entries == null)
				{
					Debug.LogWarning($"[LocalizationManager] Skipping language file {file} for language {language} because it is corrupt");
					continue;
				}

				foreach (var entry in entries)
					data.Entries[entry.Key] = entry.Value;
					
				files++;
			}

			languages[data.Language] = data;
			Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, $"[LocalizationManager] Loaded language {data.Language} with {data.Entries.Count} entries from {files} files");
		}

		private Dictionary<string, LanguageDataEntry> parseLanguageEntries(string language, string file, string[] lines)
		{
			var start = filePath.Length + 1 + language.Length + 1;
			var length = file.Length - start;
			
			var relativePath = file.Substring(start, length);
			
			var entries = new Dictionary<string, LanguageDataEntry>();
			
			for (var i = 0; i < lines.Length; i++)
			{
				if (lines[i].StartsWith("#") || lines[i] == "")
					continue;
				
				var split = lines[i].Split("	");
				if (split == null || split.Length < 2)
				{
					Debug.LogWarning($"[LocalizationManager] Skipping line {i} for language {language} file {file} because it failed to parse");
					continue;
				}

				var entry = new LanguageDataEntry
				{
					Value = split[1],
					File = relativePath
				};

				if (split.Length > 2)
				{
					if (float.TryParse(split[2], out var fontSizeOffset))
					{
						entry.FontSizeOffset = fontSizeOffset;
					}
					else
					{
						Debug.LogWarning($"[LocalizationManager] Skipping font size offset for file {file} and line {i} and language {language} because it failed to parse");
					}
				}
				
				if (!entries.ContainsKey(split[0]))
					entries.Add(split[0], entry);
				else
					entries[split[0]] = entry;
			}

			return entries;
		}
		
		private void createMissingFilesAndEntries()
		{
			var englishData = GetLanguageData("English");
			if (englishData == null)
			{
				Debug.LogError("[LocalizationManager] Failed missing localization files creation due to English being missing");
				return;
			}
			
			var englishFiles = new Dictionary<string, List<string>>();
			
			foreach (var pair in englishData.Entries)
			{
				if (!englishFiles.ContainsKey(pair.Value.File))
					englishFiles.Add(pair.Value.File, new List<string>());
				
				englishFiles[pair.Value.File].AddUnique(pair.Key);
			}
			
			var languageFiles = new List<string>();
			var reloadLanguages = new List<string>();

			foreach (var language in languages)
			{
				if (language.Key == "English")
					continue;
				
				var data = language.Value;
				if (data == null || data.Entries == null)
				{
					Debug.LogWarning($"[LocalizationManager] Skipping missing localization files creation for language {language} due to it being null");
					continue;
				}

				languageFiles.Clear();
				
				foreach (var pair in data.Entries)
					languageFiles.AddUnique(pair.Value.File);
				
				// create missing directories
				foreach (var pair in englishFiles)
				{
					if (languageFiles.Contains(pair.Key))
						continue;
					
					var createPath = Path.Combine(filePath, language.Key, pair.Key);
					var createDirectoryPath = Path.GetDirectoryName(createPath);

					if (Directory.Exists(createDirectoryPath))
						continue;
					
					if (FileTools.CreateDirectorySafe(createDirectoryPath, out var dir))
					{
#if UNITY_EDITOR
						Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, $"[LocalizationManager] Created localization direction {createDirectoryPath} for language {language.Key}");
#endif
						reloadLanguages.AddUnique(language.Key);
					}
					else
					{
						Debug.LogWarning($"[LocalizationManager] Failed to create localization directory {createDirectoryPath} for language {language.Key}");
					}
				}
				
				// create missing entries
				foreach (var pair in englishFiles)
				{
					var createPath = Path.Combine(filePath, language.Key, pair.Key);

					foreach (var englishKey in pair.Value)
					{
						var contains = false;
						
						foreach (var entries in language.Value.Entries)
						{
							if (englishKey != entries.Key) 
								continue;
							
							contains = true;
							break;
						}

						if (contains)
							continue;

						var englishDataEntry = GetLocalizedString("English", englishKey);
						
						var line = $"{englishKey}	{englishDataEntry.Value}";
						
						if (englishDataEntry.FontSizeOffset != 0)
							line += $"	{englishDataEntry.FontSizeOffset}";
						
						line += "\n";

						if (FileTools.AppendAllTextSafe(createPath, line))
						{
#if UNITY_EDITOR
							Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, $"[LocalizationManager] Appended localization entry {englishKey} in path {createPath} for language {language.Key}");
#endif
							reloadLanguages.AddUnique(language.Key);
						}
						else
						{
							Debug.LogWarning($"[LocalizationManager] Failed to append localization entry {englishKey} in path {createPath} for language {language.Key}");
						}
					}
				}
			}

			foreach (var language in reloadLanguages)
				loadLanguage(language);
		}

		public class LanguageData
		{
			public string Language;
			public Dictionary<string, LanguageDataEntry> Entries;
		}

		public class LanguageDataEntry
		{
			public string Value;
			public string File;
			
			public float FontSizeOffset;
		}
	}
}