using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Friend
{
    class IFriendService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        private FriendServicePermissionLevel _permissionLevel;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IFriendService(FriendServicePermissionLevel permissionLevel)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
              //{ 0,     GetCompletionEvent                                       },
              //{ 1,     Cancel                                                   },
                { 10100, GetFriendListIds                                         },
                { 10101, GetFriendList                                            },
              //{ 10102, UpdateFriendInfo                                         },
              //{ 10110, GetFriendProfileImage                                    },
              //{ 10200, SendFriendRequestForApplication                          },
              //{ 10211, AddFacedFriendRequestForApplication                      },
              //{ 10400, GetBlockedUserListIds                                    },
              //{ 10500, GetProfileList                                           },
                { 10600, DeclareOpenOnlinePlaySession                             },
                { 10601, DeclareCloseOnlinePlaySession                            },
                { 10610, UpdateUserPresence                                       },
              //{ 10700, GetPlayHistoryRegistrationKey                            },
              //{ 10701, GetPlayHistoryRegistrationKeyWithNetworkServiceAccountId },
              //{ 10702, AddPlayHistory                                           },
              //{ 11000, GetProfileImageUrl                                       },
              //{ 20100, GetFriendCount                                           },
              //{ 20101, GetNewlyFriendCount                                      },
              //{ 20102, GetFriendDetailedInfo                                    },
              //{ 20103, SyncFriendList                                           },
              //{ 20104, RequestSyncFriendList                                    },
              //{ 20110, LoadFriendSetting                                        },
              //{ 20200, GetReceivedFriendRequestCount                            },
              //{ 20201, GetFriendRequestList                                     },
              //{ 20300, GetFriendCandidateList                                   },
              //{ 20301, GetNintendoNetworkIdInfo                                 }, // 3.0.0+
              //{ 20302, GetSnsAccountLinkage                                     }, // 5.0.0+
              //{ 20303, GetSnsAccountProfile                                     }, // 5.0.0+
              //{ 20304, GetSnsAccountFriendList                                  }, // 5.0.0+
              //{ 20400, GetBlockedUserList                                       },
              //{ 20401, SyncBlockedUserList                                      },
              //{ 20500, GetProfileExtraList                                      },
              //{ 20501, GetRelationship                                          },
              //{ 20600, GetUserPresenceView                                      },
              //{ 20700, GetPlayHistoryList                                       },
              //{ 20701, GetPlayHistoryStatistics                                 },
              //{ 20800, LoadUserSetting                                          },
              //{ 20801, SyncUserSetting                                          },
              //{ 20900, RequestListSummaryOverlayNotification                    },
              //{ 21000, GetExternalApplicationCatalog                            },
              //{ 30100, DropFriendNewlyFlags                                     },
              //{ 30101, DeleteFriend                                             },
              //{ 30110, DropFriendNewlyFlag                                      },
              //{ 30120, ChangeFriendFavoriteFlag                                 },
              //{ 30121, ChangeFriendOnlineNotificationFlag                       },
              //{ 30200, SendFriendRequest                                        },
              //{ 30201, SendFriendRequestWithApplicationInfo                     },
              //{ 30202, CancelFriendRequest                                      },
              //{ 30203, AcceptFriendRequest                                      },
              //{ 30204, RejectFriendRequest                                      },
              //{ 30205, ReadFriendRequest                                        },
              //{ 30210, GetFacedFriendRequestRegistrationKey                     },
              //{ 30211, AddFacedFriendRequest                                    },
              //{ 30212, CancelFacedFriendRequest                                 },
              //{ 30213, GetFacedFriendRequestProfileImage                        },
              //{ 30214, GetFacedFriendRequestProfileImageFromPath                },
              //{ 30215, SendFriendRequestWithExternalApplicationCatalogId        },
              //{ 30216, ResendFacedFriendRequest                                 },
              //{ 30217, SendFriendRequestWithNintendoNetworkIdInfo               }, // 3.0.0+
              //{ 30300, GetSnsAccountLinkPageUrl                                 }, // 5.0.0+
              //{ 30301, UnlinkSnsAccount                                         }, // 5.0.0+
              //{ 30400, BlockUser                                                },
              //{ 30401, BlockUserWithApplicationInfo                             },
              //{ 30402, UnblockUser                                              },
              //{ 30500, GetProfileExtraFromFriendCode                            },
              //{ 30700, DeletePlayHistory                                        },
              //{ 30810, ChangePresencePermission                                 },
              //{ 30811, ChangeFriendRequestReception                             },
              //{ 30812, ChangePlayLogPermission                                  },
              //{ 30820, IssueFriendCode                                          },
              //{ 30830, ClearPlayLog                                             },
              //{ 49900, DeleteNetworkServiceAccountCache                         },
            };

            _permissionLevel = permissionLevel;
        }

        // nn::friends::GetFriendListIds(int offset, nn::account::Uid userUUID, nn::friends::detail::ipc::SizedFriendFilter friendFilter, ulong pidPlaceHolder, pid) -> int outCount, array<nn::account::NetworkServiceAccountId, 0xa>
        public long GetFriendListIds(ServiceCtx context)
        {
            int offset = context.RequestData.ReadInt32();

            // Padding
            context.RequestData.ReadInt32();

            UInt128      uuid   = context.RequestData.ReadStruct<UInt128>();
            FriendFilter filter = context.RequestData.ReadStruct<FriendFilter>();

            // Pid placeholder
            context.RequestData.ReadInt64();

            if (uuid.IsNull)
            {
                return MakeError(ErrorModule.Friends, FriendError.InvalidArgument);
            }

            // There are no friends online, so we return 0 because the nn::account::NetworkServiceAccountId array is empty.
            context.ResponseData.Write(0);

            Logger.PrintStub(LogClass.ServiceFriend, new
            {
                UserId = uuid.ToString(),
                offset,
                filter.PresenceStatus,
                filter.IsFavoriteOnly,
                filter.IsSameAppPresenceOnly,
                filter.IsSameAppPlayedOnly,
                filter.IsArbitraryAppPlayedOnly,
                filter.PresenceGroupId,
            });

            return 0;
        }

        // nn::friends::GetFriendList(int offset, nn::account::Uid userUUID, nn::friends::detail::ipc::SizedFriendFilter friendFilter, ulong pidPlaceHolder, pid) -> int outCount, array<nn::friends::detail::FriendImpl, 0x6>
        public long GetFriendList(ServiceCtx context)
        {
            int offset = context.RequestData.ReadInt32();

            // Padding
            context.RequestData.ReadInt32();

            UInt128      uuid   = context.RequestData.ReadStruct<UInt128>();
            FriendFilter filter = context.RequestData.ReadStruct<FriendFilter>();

            // Pid placeholder
            context.RequestData.ReadInt64();

            if (uuid.IsNull)
            {
                return MakeError(ErrorModule.Friends, FriendError.InvalidArgument);
            }

            // There are no friends online, so we return 0 because the nn::account::NetworkServiceAccountId array is empty.
            context.ResponseData.Write(0);

            Logger.PrintStub(LogClass.ServiceFriend, new {
                UserId = uuid.ToString(),
                offset,
                filter.PresenceStatus,
                filter.IsFavoriteOnly,
                filter.IsSameAppPresenceOnly,
                filter.IsSameAppPlayedOnly,
                filter.IsArbitraryAppPlayedOnly,
                filter.PresenceGroupId,
            });

            return 0;
        }

        // nn::friends::DeclareOpenOnlinePlaySession(nn::account::Uid)
        public long DeclareOpenOnlinePlaySession(ServiceCtx context)
        {
            UInt128 uuid = context.RequestData.ReadStruct<UInt128>();

            if (uuid.IsNull)
            {
                return MakeError(ErrorModule.Friends, FriendError.InvalidArgument);
            }

            if (context.Device.System.State.Account.TryGetUser(uuid, out UserProfile profile))
            {
                profile.OnlinePlayState = AccountState.Open;
            }

            Logger.PrintStub(LogClass.ServiceFriend, new { UserId = uuid.ToString(), profile.OnlinePlayState });

            return 0;
        }

        // nn::friends::DeclareCloseOnlinePlaySession(nn::account::Uid)
        public long DeclareCloseOnlinePlaySession(ServiceCtx context)
        {
            UInt128 uuid = context.RequestData.ReadStruct<UInt128>();

            if (uuid.IsNull)
            {
                return MakeError(ErrorModule.Friends, FriendError.InvalidArgument);
            }

            if (context.Device.System.State.Account.TryGetUser(uuid, out UserProfile profile))
            {
                profile.OnlinePlayState = AccountState.Closed;
            }

            Logger.PrintStub(LogClass.ServiceFriend, new { UserId = uuid.ToString(), profile.OnlinePlayState });

            return 0;
        }

        // nn::friends::UpdateUserPresence(nn::account::Uid, u64, pid, buffer<nn::friends::detail::UserPresenceImpl, 0x19>)
        public long UpdateUserPresence(ServiceCtx context)
        {
            UInt128 uuid = context.RequestData.ReadStruct<UInt128>();

            // Pid placeholder
            context.RequestData.ReadInt64();

            long position = context.Request.PtrBuff[0].Position;
            long size     = context.Request.PtrBuff[0].Size;

            byte[] bufferContent = context.Memory.ReadBytes(position, size);

            if (uuid.IsNull)
            {
                return MakeError(ErrorModule.Friends, FriendError.InvalidArgument);
            }

            int elementCount = bufferContent.Length / Marshal.SizeOf<UserPresence>();

            using (BinaryReader bufferReader = new BinaryReader(new MemoryStream(bufferContent)))
            {
                UserPresence[] userPresenceInputArray = bufferReader.ReadStructArray<UserPresence>(elementCount);

                Logger.PrintStub(LogClass.ServiceFriend, new { UserId = uuid.ToString(), userPresenceInputArray });
            }

            return 0;
        }
    }
}
