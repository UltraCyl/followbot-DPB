using System.Threading.Tasks;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using log4net;

namespace Resetter.Carry.tasks
{
    public class LevelGemsTask : ITask
    {
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
            
            
            if (!ResetterSettings.Instance.CarryEnableLevelGems)
                return false;
            
            if (LokiPoe.InGameState.SkillGemHud.AreIconsDisplayed)
            {
                await Coroutines.CloseBlockingWindows();

                LokiPoe.InGameState.SkillGemHud.HandlePendingLevelUps((x, y, z) => true);
                return true;
            }
            
            return false;
        }

        public void Tick()
        {
        }
    }
}