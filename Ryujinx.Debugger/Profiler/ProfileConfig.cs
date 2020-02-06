using System;

namespace Ryujinx.Debugger.Profiler
{
    public struct ProfileConfig : IEquatable<ProfileConfig>
    {
        public string Category;
        public string SessionGroup;
        public string SessionItem;

        public int Level;

        // Private cached variables
        private string _cachedTag;
        private string _cachedSession;
        private string _cachedSearch;

        // Public helpers to get config in more user friendly format,
        // Cached because they never change and are called often
        public string Search
        {
            get
            {
                if (_cachedSearch == null)
                {
                    _cachedSearch = $"{Category}.{SessionGroup}.{SessionItem}";
                }

                return _cachedSearch;
            }
        }

        public string Tag
        {
            get
            {
                if (_cachedTag == null)
                    _cachedTag = $"{Category}{(Session == "" ? "" : $" ({Session})")}";
                return _cachedTag;
            }
        }

        public string Session
        {
            get
            {
                if (_cachedSession == null)
                {
                    if (SessionGroup != null && SessionItem != null)
                    {
                        _cachedSession = $"{SessionGroup}: {SessionItem}";
                    }
                    else if (SessionGroup != null)
                    {
                        _cachedSession = $"{SessionGroup}";
                    }
                    else if (SessionItem != null)
                    {
                        _cachedSession = $"---: {SessionItem}";
                    }
                    else
                    {
                        _cachedSession = "";
                    }
                }

                return _cachedSession;
            }
        }

        /// <summary>
        /// The default comparison is far too slow for the number of comparisons needed because it doesn't know what's important to compare
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns></returns>
        public bool Equals(ProfileConfig cmpObj)
        {
            // Order here is important.
            // Multiple entries with the same item is considerable less likely that multiple items with the same group.
            // Likewise for group and category.
            return (cmpObj.SessionItem  == SessionItem && 
                    cmpObj.SessionGroup == SessionGroup && 
                    cmpObj.Category     == Category);
        }
    }

    /// <summary>
    /// Predefined configs to make profiling easier,
    /// nested so you can reference as Profiles.Category.Group.Item where item and group may be optional
    /// </summary>
    public static class Profiles
    {
        public static class CPU
        {
            public static ProfileConfig TranslateTier0 = new ProfileConfig()
            {
                Category = "CPU",
                SessionGroup = "TranslateTier0"
            };

            public static ProfileConfig TranslateTier1 = new ProfileConfig()
            {
                Category = "CPU",
                SessionGroup = "TranslateTier1"
            };
        }

        public static class Input
        {
            public static ProfileConfig ControllerInput = new ProfileConfig
            {
                Category     = "Input",
                SessionGroup = "ControllerInput"
            };

            public static ProfileConfig TouchInput = new ProfileConfig
            {
                Category     = "Input",
                SessionGroup = "TouchInput"
            };
        }

        public static class GPU
        {
            public static class Engine2d
            {
                public static ProfileConfig TextureCopy = new ProfileConfig()
                {
                    Category     = "GPU.Engine2D",
                    SessionGroup = "TextureCopy"
                };
            }

            public static class Engine3d
            {
                public static ProfileConfig CallMethod = new ProfileConfig()
                {
                    Category     = "GPU.Engine3D",
                    SessionGroup = "CallMethod",
                };

                public static ProfileConfig VertexEnd = new ProfileConfig()
                {
                    Category     = "GPU.Engine3D",
                    SessionGroup = "VertexEnd"
                };

                public static ProfileConfig ClearBuffers = new ProfileConfig()
                {
                    Category     = "GPU.Engine3D",
                    SessionGroup = "ClearBuffers"
                };

                public static ProfileConfig SetFrameBuffer = new ProfileConfig()
                {
                    Category     = "GPU.Engine3D",
                    SessionGroup = "SetFrameBuffer",
                };

                public static ProfileConfig SetZeta = new ProfileConfig()
                {
                    Category     = "GPU.Engine3D",
                    SessionGroup = "SetZeta"
                };

                public static ProfileConfig UploadShaders = new ProfileConfig()
                {
                    Category     = "GPU.Engine3D",
                    SessionGroup = "UploadShaders"
                };

                public static ProfileConfig UploadTextures = new ProfileConfig()
                {
                    Category     = "GPU.Engine3D",
                    SessionGroup = "UploadTextures"
                };

                public static ProfileConfig UploadTexture = new ProfileConfig()
                {
                    Category     = "GPU.Engine3D",
                    SessionGroup = "UploadTexture"
                };

                public static ProfileConfig UploadConstBuffers = new ProfileConfig()
                {
                    Category     = "GPU.Engine3D",
                    SessionGroup = "UploadConstBuffers"
                };

                public static ProfileConfig UploadVertexArrays = new ProfileConfig()
                {
                    Category     = "GPU.Engine3D",
                    SessionGroup = "UploadVertexArrays"
                };

                public static ProfileConfig ConfigureState = new ProfileConfig()
                {
                    Category     = "GPU.Engine3D",
                    SessionGroup = "ConfigureState"
                };
            }

            public static class EngineM2mf
            {
                public static ProfileConfig CallMethod = new ProfileConfig()
                {
                    Category     = "GPU.EngineM2mf",
                    SessionGroup = "CallMethod",
                };

                public static ProfileConfig Execute = new ProfileConfig()
                {
                    Category     = "GPU.EngineM2mf",
                    SessionGroup = "Execute",
                };
            }

            public static class EngineP2mf
            {
                public static ProfileConfig CallMethod = new ProfileConfig()
                {
                    Category     = "GPU.EngineP2mf",
                    SessionGroup = "CallMethod",
                };

                public static ProfileConfig Execute = new ProfileConfig()
                {
                    Category     = "GPU.EngineP2mf",
                    SessionGroup = "Execute",
                };

                public static ProfileConfig PushData = new ProfileConfig()
                {
                    Category     = "GPU.EngineP2mf",
                    SessionGroup = "PushData",
                };
            }

            public static class Shader
            {
                public static ProfileConfig Decompile = new ProfileConfig()
                {
                    Category     = "GPU.Shader",
                    SessionGroup = "Decompile",
                };
            }
        }

        public static ProfileConfig ServiceCall = new ProfileConfig()
        {
            Category = "ServiceCall",
        };
    }
}
