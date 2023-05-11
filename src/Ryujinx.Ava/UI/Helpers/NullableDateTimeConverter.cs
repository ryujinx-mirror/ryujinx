using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Ryujinx.Ava.Common.Locale;
using System;
using System.Globalization;

namespace Ryujinx.Ava.UI.Helpers
{
    internal class NullableDateTimeConverter : MarkupExtension, IValueConverter
    {
        private static readonly NullableDateTimeConverter _instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return LocaleManager.Instance[LocaleKeys.Never];
            }

            if (value is DateTime dateTime)
            {
                return dateTime.ToLocalTime().ToString(culture);
            }

            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance;
        }
    }
}