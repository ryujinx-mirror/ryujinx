using Avalonia.Data;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;

namespace Ryujinx.Ava.Ui.Controls
{
    public class GlyphValueConverter : MarkupExtension
    {
        private string _key;

        private static Dictionary<Glyph, string> _glyphs = new Dictionary<Glyph, string>
        {
            { Glyph.List, char.ConvertFromUtf32((int)Symbol.List).ToString() },
            { Glyph.Grid, char.ConvertFromUtf32((int)Symbol.ViewAll).ToString() },
            { Glyph.Chip, char.ConvertFromUtf32(59748).ToString() }
        };

        public GlyphValueConverter(string key)
        {
            _key = key;
        }

        public string this[string key]
        {
            get
            {
                if (_glyphs.TryGetValue(Enum.Parse<Glyph>(key), out var val))
                {
                    return val;
                }

                return string.Empty;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            Avalonia.Markup.Xaml.MarkupExtensions.ReflectionBindingExtension binding = new($"[{_key}]")
            {
                Mode = BindingMode.OneWay,
                Source = this
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}