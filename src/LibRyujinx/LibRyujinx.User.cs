using Ryujinx.HLE.HOS.Services.Account.Acc;
using System;
using System.Linq;

namespace LibRyujinx
{
    public static partial class LibRyujinx
    {
        public static string GetOpenedUser()
        {
            var lastProfile = SwitchDevice?.AccountManager.LastOpenedUser;

            return lastProfile?.UserId.ToString() ?? "";
        }

        public static string GetUserPicture(string userId)
        {
            var uid = new UserId(userId);

            var user = SwitchDevice?.AccountManager.GetAllUsers().FirstOrDefault(x => x.UserId == uid);

            if (user == null)
                return "";

            var pic = user.Image;

            return pic != null ? Convert.ToBase64String(pic) : "";
        }

        public static void SetUserPicture(string userId, string picture)
        {
            var uid = new UserId(userId);

            SwitchDevice?.AccountManager.SetUserImage(uid, Convert.FromBase64String(picture));
        }

        public static string GetUserName(string userId)
        {
            var uid = new UserId(userId);

            var user = SwitchDevice?.AccountManager.GetAllUsers().FirstOrDefault(x => x.UserId == uid);

            return user?.Name ?? "";
        }

        public static void SetUserName(string userId, string name)
        {
            var uid = new UserId(userId);

            SwitchDevice?.AccountManager.SetUserName(uid, name);
        }

        public static string[] GetAllUsers()
        {
            return SwitchDevice?.AccountManager.GetAllUsers().Select(x => x.UserId.ToString()).ToArray() ??
                   Array.Empty<string>();
        }

        public static void AddUser(string userName, string picture)
        {
            SwitchDevice?.AccountManager.AddUser(userName, Convert.FromBase64String(picture));
        }

        public static void DeleteUser(string userId)
        {
            var uid = new UserId(userId);
            SwitchDevice?.AccountManager.DeleteUser(uid);
        }

        public static void OpenUser(string userId)
        {
            var uid = new UserId(userId);
            SwitchDevice?.AccountManager.OpenUser(uid);
        }

        public static void CloseUser(string userId)
        {
            var uid = new UserId(userId);
            SwitchDevice?.AccountManager.CloseUser(uid);
        }
    }
}
