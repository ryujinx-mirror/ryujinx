using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ryujinx.Common.Configuration.Hid
{
    public class InputConfig : INotifyPropertyChanged
    {
        /// <summary>
        /// The current version of the input file format
        /// </summary>
        public const int CurrentVersion = 1;

        public int Version { get; set; }

        public InputBackendType Backend { get; set; }

        /// <summary>
        /// Controller id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///  Controller's Type
        /// </summary>
        public ControllerType ControllerType { get; set; }

        /// <summary>
        ///  Player's Index for the controller
        /// </summary>
        public PlayerIndex PlayerIndex { get; set; }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}