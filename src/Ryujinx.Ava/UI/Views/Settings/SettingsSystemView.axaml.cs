using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Ryujinx.Ava.UI.ViewModels;
using System;
using System.Linq;
using TimeZone = Ryujinx.Ava.UI.Models.TimeZone;

namespace Ryujinx.Ava.UI.Views.Settings
{
    public partial class SettingsSystemView : UserControl
    {
        public SettingsViewModel ViewModel;

        public SettingsSystemView()
        {
            InitializeComponent();
        
            FuncMultiValueConverter<string, string> converter = new(parts => string.Format("{0}  {1}   {2}", parts.ToArray()).Trim());
            MultiBinding tzMultiBinding = new() { Converter = converter };

            tzMultiBinding.Bindings.Add(new Binding("UtcDifference"));
            tzMultiBinding.Bindings.Add(new Binding("Location"));
            tzMultiBinding.Bindings.Add(new Binding("Abbreviation"));

            TimeZoneBox.ValueMemberBinding = tzMultiBinding;
        }

        private void TimeZoneBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                if (e.AddedItems[0] is TimeZone timeZone)
                {
                    e.Handled = true;

                    ViewModel.ValidateAndSetTimeZone(timeZone.Location);
                }
            }
        }

        private void TimeZoneBox_OnTextChanged(object sender, EventArgs e)
        {
            if (sender is AutoCompleteBox box && box.SelectedItem is TimeZone timeZone)
            {
                {
                    ViewModel.ValidateAndSetTimeZone(timeZone.Location);
                }
            }
        }
    }
}