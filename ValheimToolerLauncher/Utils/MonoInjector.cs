using System;
using System.IO;
using System.Reflection;
using SharpMonoInjector;

namespace ValheimToolerLauncher.Utils
{
    public class MonoInjector
    {
        private readonly string _launcherPath;
        public string LastErrorMsg { get; private set; }

        public MonoInjector()
        {
            _launcherPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        public bool Inject()
        {
            string assemblyPath = Path.Combine(_launcherPath, "ValheimTooler.dll");

            if (!File.Exists(assemblyPath))
            {
                return DoError("Cannot found ValheimTooler.dll!");
            }

            Injector injector;

            try
            {
                injector = new Injector("valheim");
            }
            catch (Exception exc)
            {
                return DoError(exc.Message);
            }

            byte[] assembly;

            try
            {
                assembly = File.ReadAllBytes(assemblyPath);
            }
            catch (Exception exc)
            {
                return DoError(exc.Message);
            }

            using (injector)
            {
                IntPtr remoteAssembly = IntPtr.Zero;

                try
                {
                    remoteAssembly = injector.Inject(assembly, "ValheimTooler", "Loader", "Init");
                }
                catch (InjectorException ie)
                {
                    return DoError(ie.Message);
                }
                catch (Exception exc)
                {
                    return DoError(exc.Message);
                }

                if (remoteAssembly == IntPtr.Zero)
                {
                    return DoError("Unknown reason");
                }

                return true;
            }
        }

        private bool DoError(string message)
        {
            LastErrorMsg = message;
            return false;
        }
    }
}
