using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ryujinx.Ui.Common.Models.Amiibo
{
    public struct AmiiboApi : IEquatable<AmiiboApi>
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("head")]
        public string Head { get; set; }
        [JsonPropertyName("tail")]
        public string Tail { get; set; }
        [JsonPropertyName("image")]
        public string Image { get; set; }
        [JsonPropertyName("amiiboSeries")]
        public string AmiiboSeries { get; set; }
        [JsonPropertyName("character")]
        public string Character { get; set; }
        [JsonPropertyName("gameSeries")]
        public string GameSeries { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("release")]
        public Dictionary<string, string> Release { get; set; }

        [JsonPropertyName("gamesSwitch")]
        public List<AmiiboApiGamesSwitch> GamesSwitch { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public string GetId()
        {
            return Head + Tail;
        }

        public bool Equals(AmiiboApi other)
        {
            return Head + Tail == other.Head + other.Tail;
        }

        public override bool Equals(object obj)
        {
            return obj is AmiiboApi other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Head, Tail);
        }
    }
}