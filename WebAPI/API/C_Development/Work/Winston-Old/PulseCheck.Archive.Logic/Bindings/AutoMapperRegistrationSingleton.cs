using System;

namespace PulseCheck.Archive.Logic.Bindings
{

    public sealed class AutoMapperRegistrationSingleton
    {
        private static volatile AutoMapperRegistration instance;
        private static object syncRoot = new Object();

        private AutoMapperRegistrationSingleton() { }

        public static void Register()
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                    {
                        instance = new AutoMapperRegistration();
                        instance.RegisterMappings();
                    }
                }
            }

        }

    }
}
