using Microsoft.Owin.Hosting;
using System;
using System.Configuration;
using System.Runtime.InteropServices;

namespace WinampServer
{
    public class Program
    {
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);
        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        public static Player Player;

        static void Main(string[] args)
        {
            if (Player != null)
            {
                Console.WriteLine("Server is already running!");
            }

            // react to close window event and cleanup
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            Player = new Player();
            Player.LoadUserConfig("Data/UsersConfig.json");
            Player.LoadRadioConfig("Data/Radios.json");
            Player.LoadStatus("Data/PlayerStatus.json");
            Player.LoadYoutubeHistory("Data/YoutubeHistory.json");

            string serverUrl = ConfigurationManager.AppSettings["Bindings"] ?? "http://*:80";

            try
            {
                using (WebApp.Start<Startup>(serverUrl))
                {
                    Console.WriteLine($"Server running at {serverUrl}");
                    Console.Write("Enter command: ");

                    bool notExit = true;
                    while (notExit)
                    {
                        // read command and clear that console line
                        string commandString = Console.ReadLine();
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        Console.Write(new string(' ', Console.BufferWidth));
                        Console.SetCursorPosition(0, Console.CursorTop - 1);

                        // call player to handle commands
                        Player.HandleCommand(commandString, new Models.Client { Name = "Server" });

                        // got exit command, so exit server
                        if (commandString == "exit")
                        {
                            Player.Hub.Clients.All.ConsoleLog("Server closed. Refresh browser.");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message + ex.InnerException?.Message);
            }
            finally
            {
                if (Player != null)
                {
                    Player.SaveStatus();
                    Player.Dispose();
                    Player = null;
                }
            }
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.WriteLine("Server closed. Press any key to exit...");
            Console.ReadKey();
        }

        public static void ConsoleLog(string logEntry)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine(logEntry);
            Console.Write("Enter command: ");
        }

        private static bool Handler(CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                default:
                    if (Player != null)
                    {
                        Player.SaveStatus();
                        Player.Dispose();
                        Player = null;
                    }
                    return false;
            }
        }
    }

    enum CtrlType
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }
}
