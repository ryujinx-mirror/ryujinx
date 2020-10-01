using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Ryujinx.Common
{
    public static class EmbeddedResources
    {
        private readonly static Assembly ResourceAssembly;

        static EmbeddedResources()
        {
            ResourceAssembly = Assembly.GetAssembly(typeof(EmbeddedResources));
        }

        public static byte[] Read(string filename)
        {
            var (assembly, path) = ResolveManifestPath(filename);

            return Read(assembly, path);
        }

        public static Task<byte[]> ReadAsync(string filename)
        {
            var (assembly, path) = ResolveManifestPath(filename);

            return ReadAsync(assembly, path);
        }

        public static byte[] Read(Assembly assembly, string filename)
        {
            using (var stream = GetStream(assembly, filename))
            {
                if (stream == null)
                {
                    return null;
                }

                using (var mem = new MemoryStream())
                {
                    stream.CopyTo(mem);

                    return mem.ToArray();
                }
            }
        }

        public async static Task<byte[]> ReadAsync(Assembly assembly, string filename)
        {
            using (var stream = GetStream(assembly, filename))
            {
                if (stream == null)
                {
                    return null;
                }

                using (var mem = new MemoryStream())
                {
                    await stream.CopyToAsync(mem);

                    return mem.ToArray();
                }
            }
        }

        public static string ReadAllText(string filename)
        {
            var (assembly, path) = ResolveManifestPath(filename);

            return ReadAllText(assembly, path);
        }

        public static Task<string> ReadAllTextAsync(string filename)
        {
            var (assembly, path) = ResolveManifestPath(filename);

            return ReadAllTextAsync(assembly, path);
        }

        public static string ReadAllText(Assembly assembly, string filename)
        {
            using (var stream = GetStream(assembly, filename))
            {
                if (stream == null)
                {
                    return null;
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public async static Task<string> ReadAllTextAsync(Assembly assembly, string filename)
        {
            using (var stream = GetStream(assembly, filename))
            {
                if (stream == null)
                {
                    return null;
                }

                using (var reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        public static Stream GetStream(string filename)
        {
            var (assembly, path) = ResolveManifestPath(filename);

            return GetStream(assembly, path);
        }

        public static Stream GetStream(Assembly assembly, string filename)
        {
            var namespace_ = assembly.GetName().Name;
            var manifestUri = namespace_ + "." + filename.Replace('/', '.');

            var stream = assembly.GetManifestResourceStream(manifestUri);

            return stream;
        }

        private static (Assembly, string) ResolveManifestPath(string filename)
        {
            var segments = filename.Split(new[] { '/' }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length >= 2)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name == segments[0])
                    {
                        return (assembly, segments[1]);
                    }
                }
            }

            return (ResourceAssembly, filename);
        }
    }
}