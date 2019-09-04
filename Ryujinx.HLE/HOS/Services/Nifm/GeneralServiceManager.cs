using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.HLE.HOS.Services.Nifm
{
    static class GeneralServiceManager
    {
        private static List<GeneralServiceDetail> _generalServices = new List<GeneralServiceDetail>();

        public static int Count
        {
            get => _generalServices.Count;
        }

        public static void Add(GeneralServiceDetail generalServiceDetail)
        {
            _generalServices.Add(generalServiceDetail);
        }

        public static void Remove(int index)
        {
            _generalServices.RemoveAt(index);
        }

        public static GeneralServiceDetail Get(int clientId)
        {
            return _generalServices.First(item => item.ClientId == clientId);
        }
    }
}