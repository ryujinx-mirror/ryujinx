using Ryujinx.Ava.UI.ViewModels;
using Ryujinx.Common;
using Ryujinx.Common.Utilities;
using Ryujinx.Ui.Common.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace Ryujinx.Ava.Common.Locale
{
    class LocaleManager : BaseModel
    {
        private const string DefaultLanguageCode = "en_US";

        private Dictionary<LocaleKeys, string> _localeStrings;
        private ConcurrentDictionary<LocaleKeys, object[]> _dynamicValues;

        public static LocaleManager Instance { get; } = new LocaleManager();
        public Dictionary<LocaleKeys, string> LocaleStrings { get => _localeStrings; set => _localeStrings = value; }


        public LocaleManager()
        {
            _localeStrings = new Dictionary<LocaleKeys, string>();
            _dynamicValues = new ConcurrentDictionary<LocaleKeys, object[]>();

            Load();
        }

        public void Load()
        {
            string localeLanguageCode = CultureInfo.CurrentCulture.Name.Replace('-', '_');

            if (Program.PreviewerDetached)
            {
                if (!string.IsNullOrEmpty(ConfigurationState.Instance.Ui.LanguageCode.Value))
                {
                    localeLanguageCode = ConfigurationState.Instance.Ui.LanguageCode.Value;
                }
            }

            // Load english first, if the target language translation is incomplete, we default to english.
            LoadDefaultLanguage();

            if (localeLanguageCode != DefaultLanguageCode)
            {
                LoadLanguage(localeLanguageCode);
            }
        }

        public string this[LocaleKeys key]
        {
            get
            {
                if (_localeStrings.TryGetValue(key, out string value))
                {
                    if (_dynamicValues.TryGetValue(key, out var dynamicValue))
                    {
                        return string.Format(value, dynamicValue);
                    }

                    return value;
                }

                return key.ToString();
            }
            set
            {
                _localeStrings[key] = value;

                OnPropertyChanged();
            }
        }

        public void UpdateDynamicValue(LocaleKeys key, params object[] values)
        {
            _dynamicValues[key] = values;

            OnPropertyChanged("Item");
        }

        public void LoadDefaultLanguage()
        {
            LoadLanguage(DefaultLanguageCode);
        }

        public void LoadLanguage(string languageCode)
        {
            string languageJson = EmbeddedResources.ReadAllText($"Ryujinx.Ava/Assets/Locales/{languageCode}.json");

            if (languageJson == null)
            {
                return;
            }

            var strings = JsonHelper.Deserialize<Dictionary<string, string>>(languageJson);

            foreach (var item in strings)
            {
                if (Enum.TryParse<LocaleKeys>(item.Key, out var key))
                {
                    this[key] = item.Value;
                }
            }

            if (Program.PreviewerDetached)
            {
                ConfigurationState.Instance.Ui.LanguageCode.Value = languageCode;
                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);
            }
        }
    }
}