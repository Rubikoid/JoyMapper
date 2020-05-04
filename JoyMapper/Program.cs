using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JoyMapper {
    static class Program {
        public static NLog.Logger logger = NLog.LogManager.GetLogger("CoreLog");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            // AppDomain.CurrentDomain.AppendPrivatePath(@"C:\Program Files\vJoy\x86");
            Utils.initializeAssembly();

            LoggingConfiguration config = new LoggingConfiguration();

            FileTarget logfile = new FileTarget("logfile") { FileName = "log.txt" };
            logfile.Layout = "[${longdate}][${level:uppercase=true}][${logger}]    ${message}";
            ConsoleTarget logconsole = new ConsoleTarget("logconsole");
            logconsole.Layout = "[${longdate}][${level:uppercase=true}][${logger}]    ${message}";

            /*config.AddRule(LogLevel.Trace, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);*/
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public static class Utils {
        private static NLog.Logger logger = Program.logger;
        public static object CloneObject(object o) {
            Type t = o.GetType();
            PropertyInfo[] properties = t.GetProperties();

            Object p = t.InvokeMember("", System.Reflection.BindingFlags.CreateInstance, null, o, null);

            foreach (PropertyInfo pi in properties) {
                if (pi.CanWrite) {
                    pi.SetValue(p, pi.GetValue(o, null), null);
                }
            }
            return p;
        }

        /// <summary>
        /// Here is the list of authorized assemblies (DLL files)
        /// You HAVE TO specify each of them and call InitializeAssembly()
        /// </summary>
        private static string[] LOAD_ASSEMBLIES = { "vJoyInterfaceWrap.dll" };
        private static string[] LOAD_PATHS = { @"C:\Program Files\vJoy\x86", @"C:\Program Files\vJoy\x64" };

        /// <summary>
        /// Call this method at the beginning of the program
        /// </summary>
        public static void initializeAssembly() {
            AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs args) {
                string assemblyFile = (args.Name.Contains(',')) ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name;

                assemblyFile += ".dll";

                // Forbid non handled dll's
                if (!LOAD_ASSEMBLIES.Contains(assemblyFile)) {
                    return null;
                }

                bool is64 = System.Environment.Is64BitOperatingSystem;
                string absoluteFolder = is64 ? LOAD_PATHS[1] : LOAD_PATHS[0];
                logger.Info($"Loading {assemblyFile} from {absoluteFolder} (is64={is64})");
                //string absoluteFolder = new FileInfo((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath).Directory.FullName;
                string targetPath = Path.Combine(absoluteFolder, assemblyFile);

                try {
                    return Assembly.LoadFile(targetPath);
                } catch (Exception ex) {
                    logger.Fatal(ex, "File loading");
                    return null;
                }
            };
        }

    }
}
