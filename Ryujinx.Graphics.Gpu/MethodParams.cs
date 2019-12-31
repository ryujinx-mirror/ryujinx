namespace Ryujinx.Graphics
{
    /// <summary>
    /// Method call parameters.
    /// </summary>
    struct MethodParams
    {
        /// <summary>
        /// Method offset.
        /// </summary>
        public int Method { get; }

        /// <summary>
        /// Method call argument.
        /// </summary>
        public int Argument { get; }

        /// <summary>
        /// Sub-channel where the call should be sent.
        /// </summary>
        public int SubChannel { get; }

        /// <summary>
        /// For multiple calls to the same method, this is the remaining calls count.
        /// </summary>
        public int MethodCount { get; }

        /// <summary>
        /// Indicates if the current call is the last one from a batch of calls to the same method.
        /// </summary>
        public bool IsLastCall => MethodCount <= 1;

        /// <summary>
        /// Constructs the method call parameters structure.
        /// </summary>
        /// <param name="method">Method offset</param>
        /// <param name="argument">Method call argument</param>
        /// <param name="subChannel">Optional sub-channel where the method should be sent (not required for macro calls)</param>
        /// <param name="methodCount">Optional remaining calls count (not required for macro calls)</param>
        public MethodParams(
            int method,
            int argument,
            int subChannel  = 0,
            int methodCount = 0)
        {
            Method      = method;
            Argument    = argument;
            SubChannel  = subChannel;
            MethodCount = methodCount;
        }
    }
}