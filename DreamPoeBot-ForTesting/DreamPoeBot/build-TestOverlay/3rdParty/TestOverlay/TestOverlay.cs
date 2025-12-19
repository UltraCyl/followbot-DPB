using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using GameOverlay.Drawing;
using log4net;
using Message = DreamPoeBot.Loki.Bot.Message;
using UserControl = System.Windows.Controls.UserControl;

namespace TestOverlay
{
    public class TestOverlay : IPlugin, IStartStopEvents, ITickEvents
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();
        
        public void Initialize()
        {
            var entryThread = new Thread(StartThread) { IsBackground = true };
            entryThread.Start();
        }

        public static async void StartThread()
        {
            Log.Debug($"[TestOverlay][StartThread] Start");
            while (!LokiPoe.IsBotFullyLoaded)
            {
                Log.Info("[TestOverlay][StartThread] Waiting for bot to fully load...");
                Thread.Sleep(1000);
            }
            Log.Debug($"[TestOverlay][StartThread] Starting example");
            GameOverlay.TimerService.EnableHighPrecisionTimers();
            using (var example = new Example())
            {
                example.Run();
            }
            Log.Debug($"[TestOverlay][StartThread] Example executed");
        }

        #region Unused

        public void Tick()
        {
        }
        public void Start()
        {
        }
        public void Enable()
        {
        }
        public void Disable()
        {
        }
        public void Deinitialize()
        {
        }
        public void Stop()
        {
        }
        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }
        public MessageResult Message(Message message)
        {
            return MessageResult.Unprocessed;
        }
        public UserControl Control => null;
        public JsonSettings Settings => null;

        #endregion
        
        #region Author
        public string Author => "Lajt";
        public string Description => "TestOverlay";
        public string Name => "TestOverlay";
        public string Version => "1.0";
        #endregion
        
    }
}