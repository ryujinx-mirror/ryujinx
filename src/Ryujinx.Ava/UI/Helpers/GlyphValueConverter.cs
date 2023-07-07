using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;

namespace Ryujinx.Ava.UI.Helpers
{
    public class GlyphValueConverter : MarkupExtension
    {
        private readonly string _key;

        private static readonly Dictionary<Glyph, string> _glyphs = new()
        {
            { Glyph.List, char.ConvertFromUtf32((int)Symbol.List) },
            { Glyph.Grid, char.ConvertFromUtf32((int)Symbol.ViewAll) },
            { Glyph.Chip, char.ConvertFromUtf32(59748) },
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
            ReflectionBindingExtension binding = new($"[{_key}]")
            {
                Mode = BindingMode.OneWay,
                Source = this,
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}
