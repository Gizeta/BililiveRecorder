using Autofac;
using BililiveRecorder.Core;
using BililiveRecorder.FlvProcessor;
using NLog;

namespace BililiveRecorder.ConsoleApp
{
    class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<FlvProcessorModule>();
            builder.RegisterModule<CoreModule>();
            var container = builder.Build();
            var rootScope = container.BeginLifetimeScope("recorder_root");
            var recorder = rootScope.Resolve<IRecorder>();
            recorder.Initialize(".");
            System.Console.ReadLine();
        }
    }
}
