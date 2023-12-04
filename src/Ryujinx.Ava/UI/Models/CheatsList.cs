using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Ryujinx.Ava.UI.Models
{
    public class CheatsList : ObservableCollection<CheatModel>
    {
        public CheatsList(string buildId, string path)
        {
            BuildId = buildId;
            Path = path;

            CollectionChanged += CheatsList_CollectionChanged;
        }

        public string BuildId { get; }
        public string Path { get; }

        public bool IsEnabled
        {
            get
            {
                return this.ToList().TrueForAll(x => x.IsEnabled);
            }
            set
            {
                foreach (var cheat in this)
                {
                    cheat.IsEnabled = value;
                }

                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsEnabled)));
            }
        }

        private void CheatsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                (e.NewItems[0] as CheatModel).EnableToggled += Item_EnableToggled;
            }
        }

        private void Item_EnableToggled(object sender, bool e)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsEnabled)));
        }
    }
}
