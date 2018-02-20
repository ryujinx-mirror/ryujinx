using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Objects;
using Ryujinx.Core.OsHle.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ryujinx.Core.OsHle.Ipc
{
    static class IpcHandler
    {
        private static Dictionary<(string, int), ServiceProcessRequest> ServiceCmds =
                   new Dictionary<(string, int), ServiceProcessRequest>()
        {
            { ( "acc:u0",      3), Service.AccU0ListOpenUsers                       },
            { ( "acc:u0",      5), Service.AccU0GetProfile                          },
            { ( "acc:u0",    100), Service.AccU0InitializeApplicationInfo           },
            { ( "acc:u0",    101), Service.AccU0GetBaasAccountManagerForApplication },
            { ( "apm",         0), Service.ApmOpenSession                           },
            { ( "apm:p",       0), Service.ApmOpenSession                           },
            { ( "appletOE",    0), Service.AppletOpenApplicationProxy               },
            { ( "audout:u",    0), Service.AudOutListAudioOuts                      },
            { ( "audout:u",    1), Service.AudOutOpenAudioOut                       },
            { ( "audren:u",    0), Service.AudRenOpenAudioRenderer                  },
            { ( "audren:u",    1), Service.AudRenGetAudioRendererWorkBufferSize     },
            { ( "friend:a",    0), Service.FriendCreateFriendService                },
            { ( "fsp-srv",     1), Service.FspSrvInitialize                         },
            { ( "fsp-srv",    18), Service.FspSrvMountSdCard                        },
            { ( "fsp-srv",    51), Service.FspSrvMountSaveData                      },
            { ( "fsp-srv",   200), Service.FspSrvOpenDataStorageByCurrentProcess    },
            { ( "fsp-srv",   203), Service.FspSrvOpenRomStorage                     },
            { ( "fsp-srv",  1005), Service.FspSrvGetGlobalAccessLogMode             },
            { ( "hid",         0), Service.HidCreateAppletResource                  },
            { ( "hid",        11), Service.HidActivateTouchScreen                   },
            { ( "hid",       100), Service.HidSetSupportedNpadStyleSet              },
            { ( "hid",       102), Service.HidSetSupportedNpadIdType                },
            { ( "hid",       103), Service.HidActivateNpad                          },
            { ( "hid",       120), Service.HidSetNpadJoyHoldType                    },
            { ( "lm",          0), Service.LmInitialize                             },
            { ( "nvdrv",       0), Service.NvDrvOpen                                },
            { ( "nvdrv",       1), Service.NvDrvIoctl                               },
            { ( "nvdrv",       2), Service.NvDrvClose                               },
            { ( "nvdrv",       3), Service.NvDrvInitialize                          },
            { ( "nvdrv",       4), Service.NvDrvQueryEvent                          },
            { ( "nvdrv",       8), Service.NvDrvSetClientPid                        },
            { ( "nvdrv:a",     0), Service.NvDrvOpen                                },
            { ( "nvdrv:a",     1), Service.NvDrvIoctl                               },
            { ( "nvdrv:a",     2), Service.NvDrvClose                               },
            { ( "nvdrv:a",     3), Service.NvDrvInitialize                          },
            { ( "nvdrv:a",     4), Service.NvDrvQueryEvent                          },
            { ( "nvdrv:a",     8), Service.NvDrvSetClientPid                        },
            { ( "pctl:a",      0), Service.PctlCreateService                        },
            { ( "pl:u",        1), Service.PlGetLoadState                           },
            { ( "pl:u",        2), Service.PlGetFontSize                            },
            { ( "pl:u",        3), Service.PlGetSharedMemoryAddressOffset           },
            { ( "pl:u",        4), Service.PlGetSharedMemoryNativeHandle            },
            { ( "set",         1), Service.SetGetAvailableLanguageCodes             },
            { ( "sm:",         0), Service.SmInitialize                             },
            { ( "sm:",         1), Service.SmGetService                             },
            { ( "time:u",      0), Service.TimeGetStandardUserSystemClock           },
            { ( "time:u",      1), Service.TimeGetStandardNetworkSystemClock        },
            { ( "time:u",      2), Service.TimeGetStandardSteadyClock               },
            { ( "time:u",      3), Service.TimeGetTimeZoneService                   },
            { ( "time:u",      4), Service.TimeGetStandardLocalSystemClock          },
            { ( "time:s",      0), Service.TimeGetStandardUserSystemClock           },
            { ( "time:s",      1), Service.TimeGetStandardNetworkSystemClock        },
            { ( "time:s",      2), Service.TimeGetStandardSteadyClock               },
            { ( "time:s",      3), Service.TimeGetTimeZoneService                   },
            { ( "time:s",      4), Service.TimeGetStandardLocalSystemClock          },
            { ( "vi:m",        2), Service.ViGetDisplayService                      },
        };

        private const long SfciMagic = 'S' << 0 | 'F' << 8 | 'C' << 16 | 'I' << 24;
        private const long SfcoMagic = 'S' << 0 | 'F' << 8 | 'C' << 16 | 'O' << 24;

        public static void IpcCall(
            Switch     Ns,
            AMemory    Memory,
            HSession   Session,
            IpcMessage Request,
            long       CmdPtr,
            int        HndId)
        {
            IpcMessage Response = new IpcMessage(Request.IsDomain);

            using (MemoryStream Raw = new MemoryStream(Request.RawData))
            {
                BinaryReader ReqReader = new BinaryReader(Raw);

                if (Request.Type == IpcMessageType.Request)
                {
                    string ServiceName = Session.ServiceName;

                    ServiceProcessRequest ProcReq = null;

                    bool IgnoreNullPR = false;

                    string DbgServiceName = string.Empty;

                    if (Session is HDomain Dom)
                    {
                        if (Request.DomCmd == IpcDomCmd.SendMsg)
                        {
                            long Magic =      ReqReader.ReadInt64();
                            int  CmdId = (int)ReqReader.ReadInt64();

                            object Obj = Dom.GetObject(Request.DomObjId);

                            if (Obj is HDomain)
                            {
                                ServiceCmds.TryGetValue((ServiceName, CmdId), out ProcReq);

                                DbgServiceName = $"{ServiceName} {ProcReq?.Method.Name ?? CmdId.ToString()}";
                            }
                            else if (Obj != null)
                            {
                                ((IIpcInterface)Obj).Commands.TryGetValue(CmdId, out ProcReq);

                                DbgServiceName = $"{ServiceName} {Obj.GetType().Name} {ProcReq?.Method.Name ?? CmdId.ToString()}";
                            }
                        }
                        else if (Request.DomCmd == IpcDomCmd.DeleteObj)
                        {
                            Dom.DeleteObject(Request.DomObjId);

                            Response = FillResponse(Response, 0);

                            IgnoreNullPR = true;
                        }
                    }
                    else
                    {
                        long Magic =      ReqReader.ReadInt64();
                        int  CmdId = (int)ReqReader.ReadInt64();

                        if (Session is HSessionObj)
                        {
                            object Obj = ((HSessionObj)Session).Obj;

                            ((IIpcInterface)Obj).Commands.TryGetValue(CmdId, out ProcReq);

                            DbgServiceName = $"{ServiceName} {Obj.GetType().Name} {ProcReq?.Method.Name ?? CmdId.ToString()}";
                        }
                        else
                        {
                            ServiceCmds.TryGetValue((ServiceName, CmdId), out ProcReq);

                            DbgServiceName = $"{ServiceName} {ProcReq?.Method.Name ?? CmdId.ToString()}";
                        }
                    }

                    Logging.Debug($"IpcMessage: {DbgServiceName}");

                    if (ProcReq != null)
                    {
                        using (MemoryStream ResMS = new MemoryStream())
                        {
                            BinaryWriter ResWriter = new BinaryWriter(ResMS);

                            ServiceCtx Context = new ServiceCtx(
                                Ns,
                                Memory,
                                Session,
                                Request,
                                Response,
                                ReqReader,
                                ResWriter);

                            long Result = ProcReq(Context);

                            Response = FillResponse(Response, Result, ResMS.ToArray());
                        }
                    }
                    else if (!IgnoreNullPR)
                    {   
                        throw new NotImplementedException(DbgServiceName);
                    }
                }
                else if (Request.Type == IpcMessageType.Control)
                {
                    long Magic = ReqReader.ReadInt64();
                    long CmdId = ReqReader.ReadInt64();

                    switch (CmdId)
                    {
                        case 0: Request = IpcConvertSessionToDomain(Ns, Session, Response, HndId); break;
                        case 3: Request = IpcQueryBufferPointerSize(Response);                     break;
                        case 4: Request = IpcDuplicateSessionEx(Ns, Session, Response, ReqReader); break;

                        default: throw new NotImplementedException(CmdId.ToString());
                    }
                }
                else if (Request.Type == IpcMessageType.Unknown2)
                {
                    //TODO
                }
                else
                {
                    throw new NotImplementedException(Request.Type.ToString());
                }

                AMemoryHelper.WriteBytes(Memory, CmdPtr, Response.GetBytes(CmdPtr));
            }
        }

        private static IpcMessage IpcConvertSessionToDomain(
            Switch     Ns,
            HSession   Session,
            IpcMessage Response,
            int        HndId)
        {
            HDomain Dom = new HDomain(Session);

            Ns.Os.Handles.ReplaceData(HndId, Dom);

            return FillResponse(Response, 0, Dom.GenerateObjectId(Dom));
        }

        private static IpcMessage IpcDuplicateSessionEx(
            Switch       Ns,
            HSession     Session,
            IpcMessage   Response,
            BinaryReader ReqReader)
        {
            int Unknown = ReqReader.ReadInt32();

            int Handle = Ns.Os.Handles.GenerateId(Session);

            Response.HandleDesc = IpcHandleDesc.MakeMove(Handle);

            return FillResponse(Response, 0);
        }

        private static IpcMessage IpcQueryBufferPointerSize(IpcMessage Response)
        {
            return FillResponse(Response, 0, 0x500);
        }

        private static IpcMessage FillResponse(IpcMessage Response, long Result, params int[] Values)
        {
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                foreach (int Value in Values)
                {
                    Writer.Write(Value);
                }

                return FillResponse(Response, Result, MS.ToArray());
            }
        }

        private static IpcMessage FillResponse(IpcMessage Response, long Result, byte[] Data = null)
        {
            Response.Type = IpcMessageType.Response;

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                Writer.Write(SfcoMagic);
                Writer.Write(Result);

                if (Data != null)
                {
                    Writer.Write(Data);
                }

                Response.RawData = MS.ToArray();
            }

            return Response;
        }
    }
}