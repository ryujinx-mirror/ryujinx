using Ryujinx.Graphics.Shader.Translation;

namespace Ryujinx.Graphics.Shader
{
    public readonly struct ResourceReservationCounts
    {
        public readonly int ReservedConstantBuffers { get; }
        public readonly int ReservedStorageBuffers { get; }
        public readonly int ReservedTextures { get; }
        public readonly int ReservedImages { get; }

        public ResourceReservationCounts(bool isTransformFeedbackEmulated, bool vertexAsCompute)
        {
            ResourceReservations reservations = new(isTransformFeedbackEmulated, vertexAsCompute);

            ReservedConstantBuffers = reservations.ReservedConstantBuffers;
            ReservedStorageBuffers = reservations.ReservedStorageBuffers;
            ReservedTextures = reservations.ReservedTextures;
            ReservedImages = reservations.ReservedImages;
        }
    }
}
