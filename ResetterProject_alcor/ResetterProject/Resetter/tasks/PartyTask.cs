using System.Linq;
using System.Threading.Tasks;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.Loki.Game;
using log4net;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using SkillBar = DreamPoeBot.Loki.Game.LokiPoe.InGameState.SkillBarHud;

namespace Resetter
{
    public class PartyTask : ITask
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();

        public string Author => "Allure_";
        public string Description => "Task for party.";
        public string Name => "PartyTask";
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

            if (!ResetterSettings.Instance.EnableAcceptPartyInvites)
                return false;

            if (!LokiPoe.Me.IsInHideout)
                return false;

            if (string.IsNullOrWhiteSpace(ResetterSettings.Instance.ControllerCharacterNameWhitelist))
                return false;

            var cleanedNames = ResetterSettings.Instance.ControllerCharacterNameWhitelist.Split(',')
                .Select(x => x.Trim().ToLower()).ToList();

            if (NotificationHud.NotificationList.Any(x => x.IsVisible))
                await Coroutine.Sleep(500);

            NotificationHud.HandleNotificationEx((x, y) =>
            {
                Log.Info("Notification: " + y + " " + x.CharacterName + " " + x.AccountName + " " + y + "");
                if (y != NotificationType.Party || y != NotificationType.Trade) return false;
                if (!cleanedNames.Contains(x.CharacterName.ToLower()) &&
                    !cleanedNames.Contains(x.AccountName.ToLower())) return false;
                return true;
            });

            return false;
        }

        public void Tick()
        {
        }
    }
}