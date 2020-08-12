using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Bluetooth.BluetoothDriver;
using Ryujinx.HLE.HOS.Services.Settings;
using System;

namespace Ryujinx.HLE.HOS.Services.Bluetooth
{
    [Service("btdrv")]
    class IBluetoothDriver : IpcService
    {
#pragma warning disable CS0414
        private string _unknownLowEnergy;
#pragma warning restore CS0414

        public IBluetoothDriver(ServiceCtx context) { }

        [Command(46)]
        // InitializeBluetoothLe() -> handle<copy>
        public ResultCode InitializeBluetoothLe(ServiceCtx context)
        {
            NxSettings.Settings.TryGetValue("bluetooth_debug!skip_boot", out object debugMode);

            int initializeEventHandle;

            if ((bool)debugMode)
            {
                if (BluetoothEventManager.InitializeBleDebugEventHandle == 0)
                {
                    BluetoothEventManager.InitializeBleDebugEvent = new KEvent(context.Device.System.KernelContext);

                    if (context.Process.HandleTable.GenerateHandle(BluetoothEventManager.InitializeBleDebugEvent.ReadableEvent, out BluetoothEventManager.InitializeBleDebugEventHandle) != KernelResult.Success)
                    {
                        throw new InvalidOperationException("Out of handles!");
                    }
                }

                if (BluetoothEventManager.UnknownBleDebugEventHandle == 0)
                {
                    BluetoothEventManager.UnknownBleDebugEvent = new KEvent(context.Device.System.KernelContext);

                    if (context.Process.HandleTable.GenerateHandle(BluetoothEventManager.UnknownBleDebugEvent.ReadableEvent, out BluetoothEventManager.UnknownBleDebugEventHandle) != KernelResult.Success)
                    {
                        throw new InvalidOperationException("Out of handles!");
                    }
                }

                if (BluetoothEventManager.RegisterBleDebugEventHandle == 0)
                {
                    BluetoothEventManager.RegisterBleDebugEvent = new KEvent(context.Device.System.KernelContext);

                    if (context.Process.HandleTable.GenerateHandle(BluetoothEventManager.RegisterBleDebugEvent.ReadableEvent, out BluetoothEventManager.RegisterBleDebugEventHandle) != KernelResult.Success)
                    {
                        throw new InvalidOperationException("Out of handles!");
                    }
                }

                initializeEventHandle = BluetoothEventManager.InitializeBleDebugEventHandle;
            }
            else
            {
                _unknownLowEnergy = "low_energy";

                if (BluetoothEventManager.InitializeBleEventHandle == 0)
                {
                    BluetoothEventManager.InitializeBleEvent = new KEvent(context.Device.System.KernelContext);

                    if (context.Process.HandleTable.GenerateHandle(BluetoothEventManager.InitializeBleEvent.ReadableEvent, out BluetoothEventManager.InitializeBleEventHandle) != KernelResult.Success)
                    {
                        throw new InvalidOperationException("Out of handles!");
                    }
                }

                if (BluetoothEventManager.UnknownBleEventHandle == 0)
                {
                    BluetoothEventManager.UnknownBleEvent = new KEvent(context.Device.System.KernelContext);

                    if (context.Process.HandleTable.GenerateHandle(BluetoothEventManager.UnknownBleEvent.ReadableEvent, out BluetoothEventManager.UnknownBleEventHandle) != KernelResult.Success)
                    {
                        throw new InvalidOperationException("Out of handles!");
                    }
                }

                if (BluetoothEventManager.RegisterBleEventHandle == 0)
                {
                    BluetoothEventManager.RegisterBleEvent = new KEvent(context.Device.System.KernelContext);

                    if (context.Process.HandleTable.GenerateHandle(BluetoothEventManager.RegisterBleEvent.ReadableEvent, out BluetoothEventManager.RegisterBleEventHandle) != KernelResult.Success)
                    {
                        throw new InvalidOperationException("Out of handles!");
                    }
                }

                initializeEventHandle = BluetoothEventManager.InitializeBleEventHandle;
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(initializeEventHandle);

            return ResultCode.Success;
        }
    }
}