using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace parameters
{
    public static class launch_parameters
    {
        public static bool newFile;
        public static string launchFile;
        public static bool launch;
    }
}

namespace Launcher
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            parameters.launch_parameters.launch = false;
            parameters.last_added_file.ready = false;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Launcher());
            if (parameters.launch_parameters.launch) Application.Run(new DiscController.DiscController(parameters.launch_parameters.launchFile, parameters.launch_parameters.newFile));
        }
    }
}
