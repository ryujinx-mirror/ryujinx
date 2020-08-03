using ARMeilleure.State;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Kernel.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Ryujinx.HLE.HOS.Kernel.SupervisorCall
{
    static class SyscallTable
    {
        private const int SvcFuncMaxArguments64 = 8;
        private const int SvcFuncMaxArguments32 = 4;
        private const int SvcMax                = 0x80;

        public static Action<Syscall32, ExecutionContext>[] SvcTable32 { get; }
        public static Action<Syscall64, ExecutionContext>[] SvcTable64 { get; }

        static SyscallTable()
        {
            SvcTable32 = new Action<Syscall32, ExecutionContext>[SvcMax];
            SvcTable64 = new Action<Syscall64, ExecutionContext>[SvcMax];

            Dictionary<int, string> svcFuncs64 = new Dictionary<int, string>
            {
                { 0x01, nameof(Syscall64.SetHeapSize64)                    },
                { 0x03, nameof(Syscall64.SetMemoryAttribute64)             },
                { 0x04, nameof(Syscall64.MapMemory64)                      },
                { 0x05, nameof(Syscall64.UnmapMemory64)                    },
                { 0x06, nameof(Syscall64.QueryMemory64)                    },
                { 0x07, nameof(Syscall64.ExitProcess64)                    },
                { 0x08, nameof(Syscall64.CreateThread64)                   },
                { 0x09, nameof(Syscall64.StartThread64)                    },
                { 0x0a, nameof(Syscall64.ExitThread64)                     },
                { 0x0b, nameof(Syscall64.SleepThread64)                    },
                { 0x0c, nameof(Syscall64.GetThreadPriority64)              },
                { 0x0d, nameof(Syscall64.SetThreadPriority64)              },
                { 0x0e, nameof(Syscall64.GetThreadCoreMask64)              },
                { 0x0f, nameof(Syscall64.SetThreadCoreMask64)              },
                { 0x10, nameof(Syscall64.GetCurrentProcessorNumber64)      },
                { 0x11, nameof(Syscall64.SignalEvent64)                    },
                { 0x12, nameof(Syscall64.ClearEvent64)                     },
                { 0x13, nameof(Syscall64.MapSharedMemory64)                },
                { 0x14, nameof(Syscall64.UnmapSharedMemory64)              },
                { 0x15, nameof(Syscall64.CreateTransferMemory64)           },
                { 0x16, nameof(Syscall64.CloseHandle64)                    },
                { 0x17, nameof(Syscall64.ResetSignal64)                    },
                { 0x18, nameof(Syscall64.WaitSynchronization64)            },
                { 0x19, nameof(Syscall64.CancelSynchronization64)          },
                { 0x1a, nameof(Syscall64.ArbitrateLock64)                  },
                { 0x1b, nameof(Syscall64.ArbitrateUnlock64)                },
                { 0x1c, nameof(Syscall64.WaitProcessWideKeyAtomic64)       },
                { 0x1d, nameof(Syscall64.SignalProcessWideKey64)           },
                { 0x1e, nameof(Syscall64.GetSystemTick64)                  },
                { 0x1f, nameof(Syscall64.ConnectToNamedPort64)             },
                { 0x21, nameof(Syscall64.SendSyncRequest64)                },
                { 0x22, nameof(Syscall64.SendSyncRequestWithUserBuffer64)  },
                { 0x23, nameof(Syscall64.SendAsyncRequestWithUserBuffer64) },
                { 0x24, nameof(Syscall64.GetProcessId64)                   },
                { 0x25, nameof(Syscall64.GetThreadId64)                    },
                { 0x26, nameof(Syscall64.Break64)                          },
                { 0x27, nameof(Syscall64.OutputDebugString64)              },
                { 0x29, nameof(Syscall64.GetInfo64)                        },
                { 0x2c, nameof(Syscall64.MapPhysicalMemory64)              },
                { 0x2d, nameof(Syscall64.UnmapPhysicalMemory64)            },
                { 0x32, nameof(Syscall64.SetThreadActivity64)              },
                { 0x33, nameof(Syscall64.GetThreadContext364)              },
                { 0x34, nameof(Syscall64.WaitForAddress64)                 },
                { 0x35, nameof(Syscall64.SignalToAddress64)                },
                { 0x40, nameof(Syscall64.CreateSession64)                  },
                { 0x41, nameof(Syscall64.AcceptSession64)                  },
                { 0x43, nameof(Syscall64.ReplyAndReceive64)                },
                { 0x44, nameof(Syscall64.ReplyAndReceiveWithUserBuffer64)  },
                { 0x45, nameof(Syscall64.CreateEvent64)                    },
                { 0x65, nameof(Syscall64.GetProcessList64)                 },
                { 0x6f, nameof(Syscall64.GetSystemInfo64)                  },
                { 0x70, nameof(Syscall64.CreatePort64)                     },
                { 0x71, nameof(Syscall64.ManageNamedPort64)                },
                { 0x72, nameof(Syscall64.ConnectToPort64)                  },
                { 0x73, nameof(Syscall64.SetProcessMemoryPermission64)     },
                { 0x77, nameof(Syscall64.MapProcessCodeMemory64)           },
                { 0x78, nameof(Syscall64.UnmapProcessCodeMemory64)         },
                { 0x7B, nameof(Syscall64.TerminateProcess64)               }
            };

            foreach (KeyValuePair<int, string> value in svcFuncs64)
            {
                SvcTable64[value.Key] = GenerateMethod<Syscall64>(value.Value, SvcFuncMaxArguments64);
            }

            Dictionary<int, string> svcFuncs32 = new Dictionary<int, string>
            {
                { 0x01, nameof(Syscall32.SetHeapSize32)                   },
                { 0x03, nameof(Syscall32.SetMemoryAttribute32)            },
                { 0x04, nameof(Syscall32.MapMemory32)                     },
                { 0x05, nameof(Syscall32.UnmapMemory32)                   },
                { 0x06, nameof(Syscall32.QueryMemory32)                   },
                { 0x07, nameof(Syscall32.ExitProcess32)                   },
                { 0x08, nameof(Syscall32.CreateThread32)                  },
                { 0x09, nameof(Syscall32.StartThread32)                   },
                { 0x0a, nameof(Syscall32.ExitThread32)                    },
                { 0x0b, nameof(Syscall32.SleepThread32)                   },
                { 0x0c, nameof(Syscall32.GetThreadPriority32)             },
                { 0x0d, nameof(Syscall32.SetThreadPriority32)             },
                { 0x0e, nameof(Syscall32.GetThreadCoreMask32)             },
                { 0x0f, nameof(Syscall32.SetThreadCoreMask32)             },
                { 0x10, nameof(Syscall32.GetCurrentProcessorNumber32)     },
                { 0x11, nameof(Syscall32.SignalEvent32)                   },
                { 0x12, nameof(Syscall32.ClearEvent32)                    },
                { 0x13, nameof(Syscall32.MapSharedMemory32)               },
                { 0x14, nameof(Syscall32.UnmapSharedMemory32)             },
                { 0x15, nameof(Syscall32.CreateTransferMemory32)          },
                { 0x16, nameof(Syscall32.CloseHandle32)                   },
                { 0x17, nameof(Syscall32.ResetSignal32)                   },
                { 0x18, nameof(Syscall32.WaitSynchronization32)           },
                { 0x19, nameof(Syscall32.CancelSynchronization32)         },
                { 0x1a, nameof(Syscall32.ArbitrateLock32)                 },
                { 0x1b, nameof(Syscall32.ArbitrateUnlock32)               },
                { 0x1c, nameof(Syscall32.WaitProcessWideKeyAtomic32)      },
                { 0x1d, nameof(Syscall32.SignalProcessWideKey32)          },
                { 0x1e, nameof(Syscall32.GetSystemTick32)                 },
                { 0x1f, nameof(Syscall32.ConnectToNamedPort32)            },
                { 0x21, nameof(Syscall32.SendSyncRequest32)               },
                { 0x22, nameof(Syscall32.SendSyncRequestWithUserBuffer32) },
                { 0x24, nameof(Syscall32.GetProcessId32)                  },
                { 0x25, nameof(Syscall32.GetThreadId32)                   },
                { 0x26, nameof(Syscall32.Break32)                         },
                { 0x27, nameof(Syscall32.OutputDebugString32)             },
                { 0x29, nameof(Syscall32.GetInfo32)                       },
                { 0x2c, nameof(Syscall32.MapPhysicalMemory32)             },
                { 0x2d, nameof(Syscall32.UnmapPhysicalMemory32)           },
                { 0x32, nameof(Syscall32.SetThreadActivity32)             },
                { 0x33, nameof(Syscall32.GetThreadContext332)             },
                { 0x34, nameof(Syscall32.WaitForAddress32)                },
                { 0x35, nameof(Syscall32.SignalToAddress32)               },
                { 0x40, nameof(Syscall32.CreateSession32)                 },
                { 0x41, nameof(Syscall32.AcceptSession32)                 },
                { 0x43, nameof(Syscall32.ReplyAndReceive32)               },
                { 0x45, nameof(Syscall32.CreateEvent32)                   },
                { 0x5F, nameof(Syscall32.FlushProcessDataCache32)         },
                { 0x65, nameof(Syscall32.GetProcessList32)                },
                { 0x6f, nameof(Syscall32.GetSystemInfo32)                 },
                { 0x70, nameof(Syscall32.CreatePort32)                    },
                { 0x71, nameof(Syscall32.ManageNamedPort32)               },
                { 0x72, nameof(Syscall32.ConnectToPort32)                 },
                { 0x73, nameof(Syscall32.SetProcessMemoryPermission32)    },
                { 0x77, nameof(Syscall32.MapProcessCodeMemory32)          },
                { 0x78, nameof(Syscall32.UnmapProcessCodeMemory32)        },
                { 0x7B, nameof(Syscall32.TerminateProcess32)              }
            };

            foreach (KeyValuePair<int, string> value in svcFuncs32)
            {
                SvcTable32[value.Key] = GenerateMethod<Syscall32>(value.Value, SvcFuncMaxArguments32);
            }
        }

        private static Action<T, ExecutionContext> GenerateMethod<T>(string svcName, int registerCleanCount)
        {
            Type[] argTypes = new Type[] { typeof(T), typeof(ExecutionContext) };

            DynamicMethod method = new DynamicMethod(svcName, null, argTypes);

            MethodInfo methodInfo = typeof(T).GetMethod(svcName);

            ParameterInfo[] methodArgs = methodInfo.GetParameters();

            ILGenerator generator = method.GetILGenerator();

            void ConvertToArgType(Type sourceType)
            {
                CheckIfTypeIsSupported(sourceType, svcName);

                switch (Type.GetTypeCode(sourceType))
                {
                    case TypeCode.UInt32: generator.Emit(OpCodes.Conv_U4); break;
                    case TypeCode.Int32:  generator.Emit(OpCodes.Conv_I4); break;
                    case TypeCode.UInt16: generator.Emit(OpCodes.Conv_U2); break;
                    case TypeCode.Int16:  generator.Emit(OpCodes.Conv_I2); break;
                    case TypeCode.Byte:   generator.Emit(OpCodes.Conv_U1); break;
                    case TypeCode.SByte:  generator.Emit(OpCodes.Conv_I1); break;

                    case TypeCode.Boolean:
                        generator.Emit(OpCodes.Conv_I4);
                        generator.Emit(OpCodes.Ldc_I4_1);
                        generator.Emit(OpCodes.And);
                        break;
                }
            }

            void ConvertToFieldType(Type sourceType)
            {
                CheckIfTypeIsSupported(sourceType, svcName);

                switch (Type.GetTypeCode(sourceType))
                {
                    case TypeCode.UInt32:
                    case TypeCode.Int32:
                    case TypeCode.UInt16:
                    case TypeCode.Int16:
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.Boolean:
                        generator.Emit(OpCodes.Conv_U8);
                        break;
                }
            }

            RAttribute GetRegisterAttribute(ParameterInfo parameterInfo)
            {
                RAttribute argumentAttribute = (RAttribute)parameterInfo.GetCustomAttribute(typeof(RAttribute));

                if (argumentAttribute == null)
                {
                    throw new InvalidOperationException($"Method \"{svcName}\" is missing a {typeof(RAttribute).Name} attribute on parameter \"{parameterInfo.Name}\"");
                }

                return argumentAttribute;
            }

            // For functions returning output values, the first registers
            // are used to hold pointers where the value will be stored,
            // so they can't be used to pass argument and we must
            // skip them.
            int byRefArgsCount = 0;

            for (int index = 0; index < methodArgs.Length; index++)
            {
                if (methodArgs[index].ParameterType.IsByRef)
                {
                    byRefArgsCount++;
                }
            }

            BindingFlags staticNonPublic = BindingFlags.NonPublic | BindingFlags.Static;

            // Print all the arguments for debugging purposes.
            int inputArgsCount = methodArgs.Length - byRefArgsCount;

            if (inputArgsCount != 0)
            {
                generator.Emit(OpCodes.Ldc_I4, inputArgsCount);

                generator.Emit(OpCodes.Newarr, typeof(object));

                string argsFormat = svcName;

                for (int index = 0; index < methodArgs.Length; index++)
                {
                    Type argType = methodArgs[index].ParameterType;

                    // Ignore out argument for printing
                    if (argType.IsByRef)
                    {
                        continue;
                    }

                    RAttribute registerAttribute = GetRegisterAttribute(methodArgs[index]);

                    argsFormat += $" {methodArgs[index].Name}: 0x{{{index}:X8}},";

                    generator.Emit(OpCodes.Dup);
                    generator.Emit(OpCodes.Ldc_I4, index);

                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldc_I4, registerAttribute.Index);

                    MethodInfo info = typeof(ExecutionContext).GetMethod(nameof(ExecutionContext.GetX));

                    generator.Emit(OpCodes.Call, info);

                    generator.Emit(OpCodes.Box, typeof(ulong));

                    generator.Emit(OpCodes.Stelem_Ref);
                }

                argsFormat = argsFormat.Substring(0, argsFormat.Length - 1);

               generator.Emit(OpCodes.Ldstr, argsFormat);
            }
            else
            {
                generator.Emit(OpCodes.Ldnull);

                generator.Emit(OpCodes.Ldstr, svcName);
            }

            MethodInfo printArgsMethod = typeof(SyscallTable).GetMethod(nameof(PrintArguments), staticNonPublic);

            generator.Emit(OpCodes.Call, printArgsMethod);

            // Call the SVC function handler.
            generator.Emit(OpCodes.Ldarg_0);

            List<(LocalBuilder, RAttribute)> locals = new List<(LocalBuilder, RAttribute)>();

            for (int index = 0; index < methodArgs.Length; index++)
            {
                Type argType = methodArgs[index].ParameterType;
                RAttribute registerAttribute = GetRegisterAttribute(methodArgs[index]);

                if (argType.IsByRef)
                {
                    argType = argType.GetElementType();

                    LocalBuilder local = generator.DeclareLocal(argType);

                    locals.Add((local, registerAttribute));

                    if (!methodArgs[index].IsOut)
                    {
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Ldc_I4, registerAttribute.Index);

                        MethodInfo info = typeof(ExecutionContext).GetMethod(nameof(ExecutionContext.GetX));

                        generator.Emit(OpCodes.Call, info);

                        ConvertToArgType(argType);

                        generator.Emit(OpCodes.Stloc, local);
                    }

                    generator.Emit(OpCodes.Ldloca, local);
                }
                else
                {
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldc_I4, registerAttribute.Index);

                    MethodInfo info = typeof(ExecutionContext).GetMethod(nameof(ExecutionContext.GetX));

                    generator.Emit(OpCodes.Call, info);

                    ConvertToArgType(argType);
                }
            }

            generator.Emit(OpCodes.Call, methodInfo);

            Type retType = methodInfo.ReturnType;

            // Print result code.
            if (retType == typeof(KernelResult))
            {
                MethodInfo printResultMethod = typeof(SyscallTable).GetMethod(nameof(PrintResult), staticNonPublic);

                generator.Emit(OpCodes.Dup);
                generator.Emit(OpCodes.Ldstr, svcName);
                generator.Emit(OpCodes.Call, printResultMethod);
            }

            uint registerInUse = 0;

            // Save return value into register X0 (when the method has a return value).
            if (retType != typeof(void))
            {
                CheckIfTypeIsSupported(retType, svcName);

                LocalBuilder tempLocal = generator.DeclareLocal(retType);

                generator.Emit(OpCodes.Stloc, tempLocal);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldc_I4, 0);
                generator.Emit(OpCodes.Ldloc, tempLocal);

                ConvertToFieldType(retType);

                MethodInfo info = typeof(ExecutionContext).GetMethod(nameof(ExecutionContext.SetX));

                generator.Emit(OpCodes.Call, info);

                registerInUse |= 1u << 0;
            }

            for (int index = 0; index < locals.Count; index++)
            {
                (LocalBuilder local, RAttribute attribute) = locals[index];

                if ((registerInUse & (1u << attribute.Index)) != 0)
                {
                    throw new InvalidSvcException($"Method \"{svcName}\" has conflicting output values at register index \"{attribute.Index}\".");
                }

                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldc_I4, attribute.Index);
                generator.Emit(OpCodes.Ldloc, local);

                ConvertToFieldType(local.LocalType);

                MethodInfo info = typeof(ExecutionContext).GetMethod(nameof(ExecutionContext.SetX));

                generator.Emit(OpCodes.Call, info);

                registerInUse |= 1u << attribute.Index;
            }

            // Zero out the remaining unused registers.
            for (int i = 0; i < registerCleanCount; i++)
            {
                if ((registerInUse & (1u << i)) != 0)
                {
                    continue;
                }

                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Ldc_I8, 0L);

                MethodInfo info = typeof(ExecutionContext).GetMethod(nameof(ExecutionContext.SetX));

                generator.Emit(OpCodes.Call, info);
            }

            generator.Emit(OpCodes.Ret);

            return (Action<T, ExecutionContext>)method.CreateDelegate(typeof(Action<T, ExecutionContext>));
        }

        private static void CheckIfTypeIsSupported(Type type, string svcName)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.UInt64:
                case TypeCode.Int64:
                case TypeCode.UInt32:
                case TypeCode.Int32:
                case TypeCode.UInt16:
                case TypeCode.Int16:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Boolean:
                    return;
            }

            throw new InvalidSvcException($"Method \"{svcName}\" has a invalid ref type \"{type.Name}\".");
        }

        private static void PrintArguments(object[] argValues, string formatOrSvcName)
        {
            if (argValues != null)
            {
                Logger.Debug?.Print(LogClass.KernelSvc, string.Format(formatOrSvcName, argValues));
            }
            else
            {
                Logger.Debug?.Print(LogClass.KernelSvc, formatOrSvcName);
            }
        }

        private static void PrintResult(KernelResult result, string svcName)
        {
            if (result != KernelResult.Success   &&
                result != KernelResult.TimedOut  &&
                result != KernelResult.Cancelled &&
                result != KernelResult.InvalidState)
            {
                Logger.Warning?.Print(LogClass.KernelSvc, $"{svcName} returned error {result}.");
            }
            else
            {
                Logger.Debug?.Print(LogClass.KernelSvc, $"{svcName} returned result {result}.");
            }
        }
    }
}
