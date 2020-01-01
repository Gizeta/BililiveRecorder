using Autofac;
using BililiveRecorder.Core;
using BililiveRecorder.FlvProcessor;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Gui;
using IContainer = Autofac.IContainer;

namespace BililiveRecorder.ConsoleApp
{
    public class App
    {
        private const int MAX_LOG_ROW = 25;

        private IContainer Container { get; set; }
        private ILifetimeScope RootScope { get; set; }

        public IRecorder Recorder { get; set; }

        public List<string> Logs { get; } = new List<string>();

        public static void AddLog(string message) => _AddLog?.Invoke(message);
        private static Action<string> _AddLog;

        private TextView logText;
        private TextView roomList;

        public App()
        {
            _AddLog = (message) =>
            {
                Logs.Add(Utils.FixWideChar(message));
                while (Logs.Count > MAX_LOG_ROW)
                {
                    Logs.RemoveAt(0);
                }

                Application.MainLoop.Invoke(() =>
                {
                    logText.Text = Logs.ToList().Aggregate("", (a, b) => $"{a}\n{b}");
                    logText.ScrollTo(Logs.Count - logText.Frame.Height);
                });
            };

            InitializeComponent();
        }

        public void Init(string path)
        {
            Task.Run(() => InitlaizeRecorder(path));
        }

        private void InitializeComponent()
        {
            // keyboard event may not work in SSH, use F9 instead
            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem("_App", new MenuItem [] {
                    new MenuItem("_Quit", "", () => {
                        Recorder.Shutdown();
                        Application.RequestStop();
                    })
                }),
            });

            addRoomView();
            addLogView();

            Application.Top.Add(menu);
        }

        private void addRoomView()
        {
            var frame = new FrameView("Rooms")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Percent(50)
            };

            // There is a bug when contains chinese character in ListView
            // so use TextView here 
            roomList = new TextView()
            {
                X = 1,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ReadOnly = true
            };

            frame.Add(roomList);
            Application.Top.Add(frame);
        }

        private void addLogView()
        {
            var frame = new FrameView("Logs")
            {
                X = 0,
                Y = Pos.Center() + 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            logText = new TextView()
            {
                X = 1,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                Text = "",
                ReadOnly = true
            };

            frame.Add(logText);
            Application.Top.Add(frame);
        }

        private void InitlaizeRecorder(string path)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<FlvProcessorModule>();
            builder.RegisterModule<CoreModule>();
            Container = builder.Build();
            RootScope = Container.BeginLifetimeScope("recorder_root");
            Recorder = RootScope.Resolve<IRecorder>();

            Recorder.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "Item[]")
                {
                    refreshRoomList();
                    foreach (var room in Recorder)
                    {
                        room.PropertyChanged -= RecordRoom_PropertyChanged;
                        room.PropertyChanged += RecordRoom_PropertyChanged;
                    }
                }
            };

            Recorder.Initialize(path);
        }

        private void RecordRoom_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsRecording" || e.PropertyName == "DownloadSpeedMegaBitps" || e.PropertyName == "StreamerName")
                refreshRoomList();
        }

        private void refreshRoomList()
        {

            Application.MainLoop.Invoke(() =>
            {
                roomList.Text = Recorder.ToList().Select(x =>
                    string.Format(
                        "{0,10} | {1} | {2,6:0.00} Mbps | {3}",
                        x.RoomId,
                        x.IsRecording ? "rec" : "idl",
                        x.DownloadSpeedMegaBitps,
                        Utils.FixWideChar(x.StreamerName)
                    )
                ).Aggregate("", (a, b) => $"{a}\n{b}");
            });
        }
    }
}
