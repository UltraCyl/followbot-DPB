using DreamPoeBot.Loki.Game;
using Resetter.Extensions;
using System.Linq;
using System.Threading.Tasks;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game.Objects;
using log4net;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using DreamPoeBot.Loki.Game.GameData;
using System.Windows.Forms;
using DreamPoeBot.Loki.Controllers;
using DreamPoeBot.Framework;
using DreamPoeBot.Loki.Coroutine;

namespace Resetter.tasks
{
    public delegate bool ProcessNotificationEx(NotificationData data, NotificationType type);

    public class MuleTask : ITask
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
        private bool _forceUnsocketGems;
        private bool _forceSocketGemsIntoItem;

        public string Author => "Alcor75";
        public string Description => "Task for removing gems.";
        public string Name => "UnsocketAllGemsTask";
        public string Version => "1.0";


        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }
        public MessageResult Message(DreamPoeBot.Loki.Bot.Message message)
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
            if (!LokiPoe.IsInGame || !LokiPoe.Me.IsInHideout)
            {
                
                return false;
            }
          //var timelessPortal = LokiPoe.ObjectManager.Portals.FirstOrDefault(x => x.Name == "Domain of Timeless Conflict");
            var portal = LokiPoe.ObjectManager.Portals.FirstOrDefault(x => x.Name != "Domain of Timeless Conflict");
            if (portal != null )
            {
                Log.Info("Portal is up, cannot mule");
                return false;
            }else if(portal == null)
            {
                Log.Info("Portal is not up, can mule");
                //mule task start now
                //first: get mule leader name
                var muleLeaderName = ResetterSettings.Instance.MuleLeaderCharacterName.ToString();
                //now, scan trade request
                if (LokiPoe.InGameState.NotificationHud.IsOpened)
                {
                    if(await HandleNotificationWrapper(muleLeaderName, NotificationType.Trade))//trade request from muleLeader accepeted successfully
                    {
                        //now we have to give all item in inventory and hover all muleLeader given item in trade panel
                        Log.Info("Scanning inventory");
                        return true;
                    }
                    return false;

                }
                else
                {
                    return false;
                }

            }
            return false;

        }
        private static async Task<bool> HandleNotificationWrapper(string accountNameToBeAccepted, LokiPoe.InGameState.NotificationType acceptedNotificationType)
        {
            if (LokiPoe.InGameState.NotificationHud.IsOpened)
            {
                LokiPoe.InGameState.ProcessNotificationEx isTradeRequestToBeAccepted = (x, y) =>
                {
                    Log.WarnFormat("[ServeCurrencyCustomer] Detected {0} request from {1}",
                        y.ToString(), x);
                    return x.AccountName == accountNameToBeAccepted && y == acceptedNotificationType;
                };
                bool anyVis = LokiPoe.InGameState.NotificationHud.NotificationList.Any(x => x.IsVisible);
                if (anyVis)
                {
                    Log.Info("Request Detected");
                    await DreamPoeBot.Loki.Coroutine.Coroutine.Sleep(1000);
                }

                LokiPoe.InGameState.HandleNotificationResult result =
                    LokiPoe.InGameState.NotificationHud.HandleNotificationEx(isTradeRequestToBeAccepted);
                await Coroutines.LatencyWait();
                if (result == LokiPoe.InGameState.HandleNotificationResult.Accepted)
                {
                    await Coroutines.ReactionWait();
                    return true;
                }

                await Coroutines.ReactionWait();
            }

            return false;
        }


        public void Tick()
        {
        }

        public static InventoryControlWrapper GetInventoryByItem(Item item)
        {
            return LokiPoe.InGameState.InventoryUi.AllInventoryControls.FirstOrDefault(x => x.Inventory.Items.Contains(item));
        }

        public MessageResult MessageMessege(DreamPoeBot.Loki.Bot.Message message)
        {
            throw new System.NotImplementedException();
        }
    }
}
