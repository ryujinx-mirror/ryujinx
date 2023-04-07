using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using System;

namespace Ryujinx.Ava.Common.Locale
{
    internal class LocaleExtension : MarkupExtension
    {
        public LocaleExtension(LocaleKeys key)
        {
            Key = key;
        }

        public LocaleKeys Key { get; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            LocaleKeys keyToUse = Key;

            ReflectionBindingExtension binding = new($"[{keyToUse}]")
            {
                Mode   = BindingMode.OneWay,
                Source = LocaleManager.Instance
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}