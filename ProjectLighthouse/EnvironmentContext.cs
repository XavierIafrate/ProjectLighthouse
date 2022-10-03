using ProjectLighthouse.ViewModel.Helpers;
using System.IO;

namespace ProjectLighthouse
{
    public class EnvironmentContext
    {
        public static bool Setup()
        {
#if DEBUG
            App.ROOT_PATH = ApplicationRootPaths.DEBUG_ROOT;
            DatabaseHelper.DatabasePath = $"{ApplicationRootPaths.DEBUG_ROOT}{ApplicationRootPaths.DEBUG_DB_NAME}";
            App.DevMode = true;
#else
            App.ROOT_PATH = ApplicationRootPaths.RELEASE_ROOT;
            DatabaseHelper.DatabasePath = $"{ApplicationRootPaths.RELEASE_ROOT}{ApplicationRootPaths.RELEASE_DB_NAME}";
            App.DevMode = false;
#endif

            if (!Directory.Exists(App.ROOT_PATH))
            {
                return false;
            }

            return true;
        }
    }
}
