using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Shim;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.UI.Models;
using Ryujinx.HLE.FileSystem;
using Ryujinx.Ui.App.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UserProfile = Ryujinx.Ava.UI.Models.UserProfile;

namespace Ryujinx.Ava.UI.Controls
{
    public partial class SaveManager : UserControl
    {
        private readonly UserProfile _userProfile;
        private readonly HorizonClient _horizonClient;
        private readonly VirtualFileSystem _virtualFileSystem;
        private int _sortIndex;
        private int _orderIndex;
        private ObservableCollection<SaveModel> _view = new ObservableCollection<SaveModel>();
        private string _search;

        public ObservableCollection<SaveModel> Saves { get; set; } = new ObservableCollection<SaveModel>();

        public ObservableCollection<SaveModel> View
        {
            get => _view;
            set => _view = value;
        }

        public int SortIndex
        {
            get => _sortIndex;
            set
            {
                _sortIndex = value;
                Sort();
            }
        }

        public int OrderIndex
        {
            get => _orderIndex;
            set
            {
                _orderIndex = value;
                Sort();
            }
        }

        public string Search
        {
            get => _search;
            set
            {
                _search = value;
                Sort();
            }
        }

        public SaveManager()
        {
            InitializeComponent();
        }

        public SaveManager(UserProfile userProfile, HorizonClient horizonClient, VirtualFileSystem virtualFileSystem)
        {
            _userProfile = userProfile;
            _horizonClient = horizonClient;
            _virtualFileSystem = virtualFileSystem;
            InitializeComponent();

            DataContext = this;

            Task.Run(LoadSaves);
        }

        public void LoadSaves()
        {
            Saves.Clear();
            var saveDataFilter = SaveDataFilter.Make(programId: default, saveType: SaveDataType.Account,
                new UserId((ulong)_userProfile.UserId.High, (ulong)_userProfile.UserId.Low), saveDataId: default, index: default);

            using var saveDataIterator = new UniqueRef<SaveDataIterator>();

            _horizonClient.Fs.OpenSaveDataIterator(ref saveDataIterator.Ref(), SaveDataSpaceId.User, in saveDataFilter).ThrowIfFailure();

            Span<SaveDataInfo> saveDataInfo = stackalloc SaveDataInfo[10];

            while (true)
            {
                saveDataIterator.Get.ReadSaveDataInfo(out long readCount, saveDataInfo).ThrowIfFailure();

                if (readCount == 0)
                {
                    break;
                }

                for (int i = 0; i < readCount; i++)
                {
                    var save = saveDataInfo[i];
                    if (save.ProgramId.Value != 0)
                    {
                        var saveModel = new SaveModel(save, _horizonClient, _virtualFileSystem);
                        Saves.Add(saveModel);
                        saveModel.DeleteAction = () => { Saves.Remove(saveModel); };
                    }
                    
                    Sort();
                }
            }
        }

        private void Sort()
        {
            Saves.AsObservableChangeSet()
                .Filter(Filter)
                .Sort(GetComparer())
                .Bind(out var view).AsObservableList();
            
            _view.Clear();
            _view.AddRange(view);
        }

        private IComparer<SaveModel> GetComparer()
        {
            switch (SortIndex)
            {
                case 0:
                    return OrderIndex == 0
                        ? SortExpressionComparer<SaveModel>.Ascending(save => save.Title)
                        : SortExpressionComparer<SaveModel>.Descending(save => save.Title);
                case 1:
                    return OrderIndex == 0
                        ? SortExpressionComparer<SaveModel>.Ascending(save => save.Size)
                        : SortExpressionComparer<SaveModel>.Descending(save => save.Size);
                default:
                    return null;
            }
        }

        private bool Filter(object arg)
        {
            if (arg is SaveModel save)
            {
                return string.IsNullOrWhiteSpace(_search) || save.Title.ToLower().Contains(_search.ToLower());
            }

            return false;
        }
    }
}