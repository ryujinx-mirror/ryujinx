using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    readonly struct IoDefinition : IEquatable<IoDefinition>
    {
        public StorageKind StorageKind { get; }
        public IoVariable IoVariable { get; }
        public int Location { get; }
        public int Component { get; }

        public IoDefinition(StorageKind storageKind, IoVariable ioVariable, int location = 0, int component = 0)
        {
            StorageKind = storageKind;
            IoVariable = ioVariable;
            Location = location;
            Component = component;
        }

        public override bool Equals(object other)
        {
            return other is IoDefinition ioDefinition && Equals(ioDefinition);
        }

        public bool Equals(IoDefinition other)
        {
            return StorageKind == other.StorageKind &&
                   IoVariable == other.IoVariable &&
                   Location == other.Location &&
                   Component == other.Component;
        }

        public override int GetHashCode()
        {
            return (int)StorageKind | ((int)IoVariable << 8) | (Location << 16) | (Component << 24);
        }

        public override string ToString()
        {
            return $"{StorageKind}.{IoVariable}.{Location}.{Component}";
        }
    }
}
