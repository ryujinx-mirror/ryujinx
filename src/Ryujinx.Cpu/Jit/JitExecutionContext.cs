using ARMeilleure.Memory;
using ARMeilleure.State;

namespace Ryujinx.Cpu.Jit
{
    class JitExecutionContext : IExecutionContext
    {
        private readonly ExecutionContext _impl;
        internal ExecutionContext Impl => _impl;

        /// <inheritdoc/>
        public ulong Pc => _impl.Pc;

        /// <inheritdoc/>
        public long TpidrEl0
        {
            get => _impl.TpidrEl0;
            set => _impl.TpidrEl0 = value;
        }

        /// <inheritdoc/>
        public long TpidrroEl0
        {
            get => _impl.TpidrroEl0;
            set => _impl.TpidrroEl0 = value;
        }

        /// <inheritdoc/>
        public uint Pstate
        {
            get => _impl.Pstate;
            set => _impl.Pstate = value;
        }

        /// <inheritdoc/>
        public uint Fpcr
        {
            get => (uint)_impl.Fpcr;
            set => _impl.Fpcr = (FPCR)value;
        }

        /// <inheritdoc/>
        public uint Fpsr
        {
            get => (uint)_impl.Fpsr;
            set => _impl.Fpsr = (FPSR)value;
        }

        /// <inheritdoc/>
        public bool IsAarch32
        {
            get => _impl.IsAarch32;
            set => _impl.IsAarch32 = value;
        }

        /// <inheritdoc/>
        public bool Running => _impl.Running;

        private readonly ExceptionCallbacks _exceptionCallbacks;

        public JitExecutionContext(IJitMemoryAllocator allocator, ICounter counter, ExceptionCallbacks exceptionCallbacks)
        {
            _impl = new ExecutionContext(
                allocator,
                counter,
                InterruptHandler,
                BreakHandler,
                SupervisorCallHandler,
                UndefinedHandler);

            _exceptionCallbacks = exceptionCallbacks;
        }

        /// <inheritdoc/>
        public ulong GetX(int index) => _impl.GetX(index);

        /// <inheritdoc/>
        public void SetX(int index, ulong value) => _impl.SetX(index, value);

        /// <inheritdoc/>
        public V128 GetV(int index) => _impl.GetV(index);

        /// <inheritdoc/>
        public void SetV(int index, V128 value) => _impl.SetV(index, value);

        private void InterruptHandler(ExecutionContext context)
        {
            _exceptionCallbacks.InterruptCallback?.Invoke(this);
        }

        private void BreakHandler(ExecutionContext context, ulong address, int imm)
        {
            _exceptionCallbacks.BreakCallback?.Invoke(this, address, imm);
        }

        private void SupervisorCallHandler(ExecutionContext context, ulong address, int imm)
        {
            _exceptionCallbacks.SupervisorCallback?.Invoke(this, address, imm);
        }

        private void UndefinedHandler(ExecutionContext context, ulong address, int opCode)
        {
            _exceptionCallbacks.UndefinedCallback?.Invoke(this, address, opCode);
        }

        /// <inheritdoc/>
        public void RequestInterrupt()
        {
            _impl.RequestInterrupt();
        }

        /// <inheritdoc/>
        public void StopRunning()
        {
            _impl.StopRunning();
        }

        public void Dispose()
        {
            _impl.Dispose();
        }
    }
}
