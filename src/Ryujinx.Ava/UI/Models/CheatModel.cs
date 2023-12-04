using Ryujinx.Ava.UI.ViewModels;
using System;

namespace Ryujinx.Ava.UI.Models
{
    public class CheatModel : BaseModel
    {
        private bool _isEnabled;

        public event EventHandler<bool> EnableToggled;

        public CheatModel(string name, string buildId, bool isEnabled)
        {
            Name = name;
            BuildId = buildId;
            IsEnabled = isEnabled;
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;

                EnableToggled?.Invoke(this, _isEnabled);

                OnPropertyChanged();
            }
        }

        public string BuildId { get; }

        public string BuildIdKey => $"{BuildId}-{Name}";

        public string Name { get; }

        public string CleanName => Name[1..^7];
    }
}
