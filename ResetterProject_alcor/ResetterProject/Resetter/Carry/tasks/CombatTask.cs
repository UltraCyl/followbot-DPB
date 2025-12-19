using System.Threading.Tasks;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using log4net;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;

namespace Resetter.Carry.tasks
{
    public class CombatTask: ITask
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();

        public string Author => "Allure_";
        public string Description => "";
        public string Name => "CombatTask";
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
            
            var areaName = LokiPoe.CurrentWorldArea.Name;
            if (areaName != "Domain of Timeless Conflict")
                return false;
            
            var routine = RoutineManager.Current;
            var res = await routine.Logic(new Logic("hook_combat", this));
            if (VisibleTimersUi.IsOpened == true)
            {
                
                return res == LogicResult.Provided;
            }

            return res == LogicResult.Unprovided;
        }

        public void Tick()
        {
        }
    }
}