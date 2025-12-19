using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading.Tasks;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.Loki.Game.Objects;
using log4net;
using Resetter.Extensions;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using System.Runtime;
using System.Diagnostics;
namespace Resetter.Carry.tasks

{
    
        public class LootTask : ITask
        {
        public readonly List<WorldItem> Items = new List<WorldItem>();
        

        private readonly HashSet<int> _processedObjects = new HashSet<int>();

        //keep this in a separate collection to reset on ItemEvaluatorRefresh
        private readonly HashSet<int> _processedItems = new HashSet<int>();

        private readonly Stopwatch _lastAccessTime;

        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
            
        public string Author => "Allure_";
            public string Description => "";
            public string Name => "LevelGemsTask";
            public string Version => "1.0";


            public async Task<LogicResult> Logic(Logic logic)
            {
                return LogicResult.Unprovided;
            }

            public MessageResult Message(Message message)
            {
                return MessageResult.Unprocessed;
            }


            public void Start()
            {
            }

            public void Stop()
            {
            }


            public async Task<bool> Run()
            {
                if (!LokiPoe.IsInGame)
                    return false;



            await Loot();
            return true;
            }
        private async Task Loot()
        {
         
        }
        private void WorldItemScan()
        {
            
        }
        private void LootingDone()
        {
            
        }

       

        public void Tick()
            {
            }
        }
    
}
