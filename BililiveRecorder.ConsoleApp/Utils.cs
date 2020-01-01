using System.Linq;

namespace BililiveRecorder.ConsoleApp
{
    public static class Utils
    {
        public static string FixWideChar(string text) => text.Select((c) => c > 127 ? c + " " : c.ToString()).Aggregate("", (a, b) => $"{a}{b}");
    }
}
