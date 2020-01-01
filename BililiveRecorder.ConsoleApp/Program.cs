using Terminal.Gui;

namespace BililiveRecorder.ConsoleApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            Application.Init();

            var path = args.Length > 0 ? args[0] : ".";
            var app = new App();
            app.Init(path);

            Application.Run();
        }
    }
}
