using Ryujinx.Horizon.LogManager;
using System.Collections.Generic;

namespace Ryujinx.Horizon
{
    public static class ServiceTable
    {
        public static IEnumerable<ServiceEntry> GetServices(HorizonOptions options)
        {
            List<ServiceEntry> entries = new List<ServiceEntry>();

            void RegisterService<T>() where T : IService
            {
                entries.Add(new ServiceEntry(T.Main, options));
            }

            RegisterService<LmMain>();

            return entries;
        }
    }
}
