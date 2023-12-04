namespace Ryujinx.Graphics.Nvdec.Vp9
{
    internal enum CodecErr
    {
        /*!\brief Operation completed without error */
        CodecOk,

        /*!\brief Unspecified error */
        CodecError,

        /*!\brief Memory operation failed */
        CodecMemError,

        /*!\brief ABI version mismatch */
        CodecAbiMismatch,

        /*!\brief Algorithm does not have required capability */
        CodecIncapable,

        /*!\brief The given bitstream is not supported.
         *
         * The bitstream was unable to be parsed at the highest level. The decoder
         * is unable to proceed. This error \ref SHOULD be treated as fatal to the
         * stream. */
        CodecUnsupBitstream,

        /*!\brief Encoded bitstream uses an unsupported feature
         *
         * The decoder does not implement a feature required by the encoder. This
         * return code should only be used for features that prevent future
         * pictures from being properly decoded. This error \ref MAY be treated as
         * fatal to the stream or \ref MAY be treated as fatal to the current GOP.
         */
        CodecUnsupFeature,

        /*!\brief The coded data for this stream is corrupt or incomplete
         *
         * There was a problem decoding the current frame.  This return code
         * should only be used for failures that prevent future pictures from
         * being properly decoded. This error \ref MAY be treated as fatal to the
         * stream or \ref MAY be treated as fatal to the current GOP. If decoding
         * is continued for the current GOP, artifacts may be present.
         */
        CodecCorruptFrame,

        /*!\brief An application-supplied parameter is not valid.
         *
         */
        CodecInvalidParam,

        /*!\brief An iterator reached the end of list.
         *
         */
        CodecListEnd,
    }
}
