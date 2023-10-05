using Avalonia;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.UI.Helpers;
using Ryujinx.Ava.UI.Windows;
using Ryujinx.Common;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Common.Utilities;
using Ryujinx.Ui.Common.Models.Amiibo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ryujinx.Ava.UI.ViewModels
{
    public class AmiiboWindowViewModel : BaseModel, IDisposable
    {
        private const string DefaultJson = "{ \"amiibo\": [] }";
        private const float AmiiboImageSize = 350f;

        private readonly string _amiiboJsonPath;
        private readonly byte[] _amiiboLogoBytes;
        private readonly HttpClient _httpClient;
        private readonly StyleableWindow _owner;

        private Bitmap _amiiboImage;
        private List<AmiiboApi> _amiiboList;
        private AvaloniaList<AmiiboApi> _amiibos;
        private ObservableCollection<string> _amiiboSeries;

        private int _amiiboSelectedIndex;
        private int _seriesSelectedIndex;
        private bool _enableScanning;
        private bool _showAllAmiibo;
        private bool _useRandomUuid;
        private string _usage;

        private static readonly AmiiboJsonSerializerContext _serializerContext = new(JsonHelper.GetDefaultSerializerOptions());

        public AmiiboWindowViewModel(StyleableWindow owner, string lastScannedAmiiboId, string titleId)
        {
            _owner = owner;

            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30),
            };

            LastScannedAmiiboId = lastScannedAmiiboId;
            TitleId = titleId;

            Directory.CreateDirectory(Path.Join(AppDataManager.BaseDirPath, "system", "amiibo"));

            _amiiboJsonPath = Path.Join(AppDataManager.BaseDirPath, "system", "amiibo", "Amiibo.json");
            _amiiboList = new List<AmiiboApi>();
            _amiiboSeries = new ObservableCollection<string>();
            _amiibos = new AvaloniaList<AmiiboApi>();

            _amiiboLogoBytes = EmbeddedResources.Read("Ryujinx.Ui.Common/Resources/Logo_Amiibo.png");

            _ = LoadContentAsync();
        }

        public AmiiboWindowViewModel() { }

        public string TitleId { get; set; }
        public string LastScannedAmiiboId { get; set; }

        public UserResult Response { get; private set; }

        public bool UseRandomUuid
        {
            get => _useRandomUuid;
            set
            {
                _useRandomUuid = value;

                OnPropertyChanged();
            }
        }

        public bool ShowAllAmiibo
        {
            get => _showAllAmiibo;
            set
            {
                _showAllAmiibo = value;

                ParseAmiiboData();

                OnPropertyChanged();
            }
        }

        public AvaloniaList<AmiiboApi> AmiiboList
        {
            get => _amiibos;
            set
            {
                _amiibos = value;

                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> AmiiboSeries
        {
            get => _amiiboSeries;
            set
            {
                _amiiboSeries = value;
                OnPropertyChanged();
            }
        }

        public int SeriesSelectedIndex
        {
            get => _seriesSelectedIndex;
            set
            {
                _seriesSelectedIndex = value;

                FilterAmiibo();

                OnPropertyChanged();
            }
        }

        public int AmiiboSelectedIndex
        {
            get => _amiiboSelectedIndex;
            set
            {
                _amiiboSelectedIndex = value;

                EnableScanning = _amiiboSelectedIndex >= 0 && _amiiboSelectedIndex < _amiibos.Count;

                SetAmiiboDetails();

                OnPropertyChanged();
            }
        }

        public Bitmap AmiiboImage
        {
            get => _amiiboImage;
            set
            {
                _amiiboImage = value;

                OnPropertyChanged();
            }
        }

        public string Usage
        {
            get => _usage;
            set
            {
                _usage = value;

                OnPropertyChanged();
            }
        }

        public bool EnableScanning
        {
            get => _enableScanning;
            set
            {
                _enableScanning = value;

                OnPropertyChanged();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _httpClient.Dispose();
        }

        private async Task LoadContentAsync()
        {
            string amiiboJsonString = DefaultJson;

            if (File.Exists(_amiiboJsonPath))
            {
                amiiboJsonString = await File.ReadAllTextAsync(_amiiboJsonPath);

                if (await NeedsUpdate(JsonHelper.Deserialize(amiiboJsonString, _serializerContext.AmiiboJson).LastUpdated))
                {
                    amiiboJsonString = await DownloadAmiiboJson();
                }
            }
            else
            {
                try
                {
                    amiiboJsonString = await DownloadAmiiboJson();
                }
                catch (Exception ex)
                {
                    Logger.Error?.Print(LogClass.Application, $"Failed to download amiibo data: {ex}");

                    ShowInfoDialog();
                }
            }

            _amiiboList = JsonHelper.Deserialize(amiiboJsonString, _serializerContext.AmiiboJson).Amiibo;
            _amiiboList = _amiiboList.OrderBy(amiibo => amiibo.AmiiboSeries).ToList();

            ParseAmiiboData();
        }

        private void ParseAmiiboData()
        {
            _amiiboSeries.Clear();
            _amiibos.Clear();

            for (int i = 0; i < _amiiboList.Count; i++)
            {
                if (!_amiiboSeries.Contains(_amiiboList[i].AmiiboSeries))
                {
                    if (!ShowAllAmiibo)
                    {
                        foreach (AmiiboApiGamesSwitch game in _amiiboList[i].GamesSwitch)
                        {
                            if (game != null)
                            {
                                if (game.GameId.Contains(TitleId))
                                {
                                    AmiiboSeries.Add(_amiiboList[i].AmiiboSeries);

                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        AmiiboSeries.Add(_amiiboList[i].AmiiboSeries);
                    }
                }
            }

            if (LastScannedAmiiboId != "")
            {
                SelectLastScannedAmiibo();
            }
            else
            {
                SeriesSelectedIndex = 0;
            }
        }

        private void SelectLastScannedAmiibo()
        {
            AmiiboApi scanned = _amiiboList.Find(amiibo => amiibo.GetId() == LastScannedAmiiboId);

            SeriesSelectedIndex = AmiiboSeries.IndexOf(scanned.AmiiboSeries);
            AmiiboSelectedIndex = AmiiboList.IndexOf(scanned);
        }

        private void FilterAmiibo()
        {
            _amiibos.Clear();

            if (_seriesSelectedIndex < 0)
            {
                return;
            }

            List<AmiiboApi> amiiboSortedList = _amiiboList
                .Where(amiibo => amiibo.AmiiboSeries == _amiiboSeries[SeriesSelectedIndex])
                .OrderBy(amiibo => amiibo.Name).ToList();

            for (int i = 0; i < amiiboSortedList.Count; i++)
            {
                if (!_amiibos.Contains(amiiboSortedList[i]))
                {
                    if (!_showAllAmiibo)
                    {
                        foreach (AmiiboApiGamesSwitch game in amiiboSortedList[i].GamesSwitch)
                        {
                            if (game != null)
                            {
                                if (game.GameId.Contains(TitleId))
                                {
                                    _amiibos.Add(amiiboSortedList[i]);

                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        _amiibos.Add(amiiboSortedList[i]);
                    }
                }
            }

            AmiiboSelectedIndex = 0;
        }

        private void SetAmiiboDetails()
        {
            ResetAmiiboPreview();

            Usage = string.Empty;

            if (_amiiboSelectedIndex < 0)
            {
                return;
            }

            AmiiboApi selected = _amiibos[_amiiboSelectedIndex];

            string imageUrl = _amiiboList.Find(amiibo => amiibo.Equals(selected)).Image;

            StringBuilder usageStringBuilder = new();

            for (int i = 0; i < _amiiboList.Count; i++)
            {
                if (_amiiboList[i].Equals(selected))
                {
                    bool writable = false;

                    foreach (AmiiboApiGamesSwitch item in _amiiboList[i].GamesSwitch)
                    {
                        if (item.GameId.Contains(TitleId))
                        {
                            foreach (AmiiboApiUsage usageItem in item.AmiiboUsage)
                            {
                                usageStringBuilder.Append($"{Environment.NewLine}- {usageItem.Usage.Replace("/", Environment.NewLine + "-")}");

                                writable = usageItem.Write;
                            }
                        }
                    }

                    if (usageStringBuilder.Length == 0)
                    {
                        usageStringBuilder.Append($"{LocaleManager.Instance[LocaleKeys.Unknown]}.");
                    }

                    Usage = $"{LocaleManager.Instance[LocaleKeys.Usage]} {(writable ? $" ({LocaleManager.Instance[LocaleKeys.Writable]})" : "")} : {usageStringBuilder}";
                }
            }

            _ = UpdateAmiiboPreview(imageUrl);
        }

        private async Task<bool> NeedsUpdate(DateTime oldLastModified)
        {
            try
            {
                HttpResponseMessage response =
                    await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, "https://amiibo.ryujinx.org/"));

                if (response.IsSuccessStatusCode)
                {
                    return response.Content.Headers.LastModified != new DateTimeOffset(oldLastModified.Ticks - (oldLastModified.Ticks % TimeSpan.TicksPerSecond), TimeSpan.Zero);
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Error?.Print(LogClass.Application, $"Failed to check for amiibo updates: {ex}");

                ShowInfoDialog();

                return false;
            }
        }

        private async Task<string> DownloadAmiiboJson()
        {
            HttpResponseMessage response = await _httpClient.GetAsync("https://amiibo.ryujinx.org/");

            if (response.IsSuccessStatusCode)
            {
                string amiiboJsonString = await response.Content.ReadAsStringAsync();

                using (FileStream amiiboJsonStream = File.Create(_amiiboJsonPath, 4096, FileOptions.WriteThrough))
                {
                    amiiboJsonStream.Write(Encoding.UTF8.GetBytes(amiiboJsonString));
                }

                return amiiboJsonString;
            }

            Logger.Error?.Print(LogClass.Application, $"Failed to download amiibo data. Response status code: {response.StatusCode}");

            await ContentDialogHelper.CreateInfoDialog(LocaleManager.Instance[LocaleKeys.DialogAmiiboApiTitle],
                LocaleManager.Instance[LocaleKeys.DialogAmiiboApiFailFetchMessage],
                LocaleManager.Instance[LocaleKeys.InputDialogOk],
                "",
                LocaleManager.Instance[LocaleKeys.RyujinxInfo]);

            Close();

            return DefaultJson;
        }

        private void Close()
        {
            Dispatcher.UIThread.Post(_owner.Close);
        }

        private async Task UpdateAmiiboPreview(string imageUrl)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(imageUrl);

            if (response.IsSuccessStatusCode)
            {
                byte[] amiiboPreviewBytes = await response.Content.ReadAsByteArrayAsync();
                using MemoryStream memoryStream = new(amiiboPreviewBytes);

                Bitmap bitmap = new(memoryStream);

                double ratio = Math.Min(AmiiboImageSize / bitmap.Size.Width,
                        AmiiboImageSize / bitmap.Size.Height);

                int resizeHeight = (int)(bitmap.Size.Height * ratio);
                int resizeWidth = (int)(bitmap.Size.Width * ratio);

                AmiiboImage = bitmap.CreateScaledBitmap(new PixelSize(resizeWidth, resizeHeight));
            }
            else
            {
                Logger.Error?.Print(LogClass.Application, $"Failed to get amiibo preview. Response status code: {response.StatusCode}");
            }
        }

        private void ResetAmiiboPreview()
        {
            using MemoryStream memoryStream = new(_amiiboLogoBytes);

            Bitmap bitmap = new(memoryStream);

            AmiiboImage = bitmap;
        }

        private static async void ShowInfoDialog()
        {
            await ContentDialogHelper.CreateInfoDialog(LocaleManager.Instance[LocaleKeys.DialogAmiiboApiTitle],
                LocaleManager.Instance[LocaleKeys.DialogAmiiboApiConnectErrorMessage],
                LocaleManager.Instance[LocaleKeys.InputDialogOk],
                "",
                LocaleManager.Instance[LocaleKeys.RyujinxInfo]);
        }
    }
}
