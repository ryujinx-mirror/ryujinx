using LibHac;
using LibHac.Fs;
using Ryujinx.HLE.HOS.Ipc;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.FspSrv
{
    class IFile : IpcService, IDisposable
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private LibHac.Fs.IFile _baseFile;

        public IFile(LibHac.Fs.IFile baseFile)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, Read    },
                { 1, Write   },
                { 2, Flush   },
                { 3, SetSize },
                { 4, GetSize }
            };

            _baseFile = baseFile;
        }

        // Read(u32 readOption, u64 offset, u64 size) -> (u64 out_size, buffer<u8, 0x46, 0> out_buf)
        public long Read(ServiceCtx context)
        {
            long position = context.Request.ReceiveBuff[0].Position;

            ReadOption readOption = (ReadOption)context.RequestData.ReadInt32();
            context.RequestData.BaseStream.Position += 4;

            long offset = context.RequestData.ReadInt64();
            long size   = context.RequestData.ReadInt64();

            byte[] data = new byte[size];
            int readSize;

            try
            {
                readSize = _baseFile.Read(data, offset, readOption);
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            context.Memory.WriteBytes(position, data);

            context.ResponseData.Write((long)readSize);

            return 0;
        }

        // Write(u32 writeOption, u64 offset, u64 size, buffer<u8, 0x45, 0>)
        public long Write(ServiceCtx context)
        {
            long position = context.Request.SendBuff[0].Position;

            WriteOption writeOption = (WriteOption)context.RequestData.ReadInt32();
            context.RequestData.BaseStream.Position += 4;

            long offset = context.RequestData.ReadInt64();
            long size   = context.RequestData.ReadInt64();

            byte[] data = context.Memory.ReadBytes(position, size);

            try
            {
                _baseFile.Write(data, offset, writeOption);
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        // Flush()
        public long Flush(ServiceCtx context)
        {
            try
            {
                _baseFile.Flush();
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        // SetSize(u64 size)
        public long SetSize(ServiceCtx context)
        {
            try
            {
                long size = context.RequestData.ReadInt64();

                _baseFile.SetSize(size);
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        // GetSize() -> u64 fileSize
        public long GetSize(ServiceCtx context)
        {
            try
            {
                context.ResponseData.Write(_baseFile.GetSize());
            }
            catch (HorizonResultException ex)
            {
                return ex.ResultValue.Value;
            }

            return 0;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _baseFile?.Dispose();
            }
        }
    }
}