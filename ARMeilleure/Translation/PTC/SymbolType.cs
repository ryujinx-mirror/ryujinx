namespace ARMeilleure.Translation.PTC
{
    /// <summary>
    /// Types of <see cref="Symbol"/>.
    /// </summary>
    enum SymbolType : byte
    {
        /// <summary>
        /// Refers to nothing, i.e no symbol.
        /// </summary>
        None,

        /// <summary>
        /// Refers to an entry in <see cref="Delegates"/>.
        /// </summary>
        DelegateTable,

        /// <summary>
        /// Refers to an entry in <see cref="Translator.FunctionTable"/>.
        /// </summary>
        FunctionTable,

        /// <summary>
        /// Refers to a special symbol which is handled by <see cref="Ptc.PatchCode"/>.
        /// </summary>
        Special
    }
}
