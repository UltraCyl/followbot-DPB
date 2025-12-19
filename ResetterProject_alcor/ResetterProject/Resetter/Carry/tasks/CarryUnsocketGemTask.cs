using DreamPoeBot.Loki.Game;
using Resetter.Extensions;
using System.Linq;
using System.Threading.Tasks;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game.Objects;
using log4net;
using System.Windows;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;


namespace Resetter.tasks
{
    public class CarryUnsocketAllGemsTask : ITask
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
        private bool _forceUnsocketGems;

        public string Author => "Alcor75";
        public string Description => "Task for removing gems.";
        public string Name => "UnsocketAllGemsTask";
        public string Version => "1.0";


        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }
        public MessageResult Message(Message message)
        {
            if (message.Id == Messages.CARRY_UNSOCKET_GEMS)
            {
                Log.Info("Start unsocket all gems. Part 0");
                _forceUnsocketGems = true;





                return MessageResult.Processed;

            }

            return MessageResult.Unprocessed;
        }
        public static async Task<bool> RemoveAllGemsFromItem(InventoryControlWrapper control)
        {
            if (!LokiPoe.Me.IsInHideout)
            {
                MessageBox.Show("Please get into Hideout before starting Unsocket Plugin.");
                BotManager.Stop();
                return false; 
            }
            await CursorHelper.OpenInventory(true);
            Log.Info("Openning inventory");

            while (true)
            {
                var thisItem = control.Inventory.Items.FirstOrDefault();
                if (thisItem == null)
                {
                    break;
                }else if(thisItem.MaxLinkCount == 6)
                {
                    continue;
                }

                if (thisItem.SocketedGems.Count(g => g != null) == 0) break;
                Log.Info("Start unsocket all gems. Part 1");
                var index = -1;
                var count = thisItem.SocketedGems.Count();
                for (int i = 0; i < count; i++)
                {
                    index++;
                    var gemOldIndex = index;
                    if (thisItem.SocketedGems[i] == null) continue;
                    if (thisItem.SocketedGems[i].Name == "Whirling Blades") continue;
                    var un = control.UnequipSkillGem(gemOldIndex);
                    if (!await Wait.For(() => LokiPoe.InGameState.CursorItemOverlay.Item != null,
                        "Gem to appear on cursor.", 100, 6000))
                    {
                        continue;
                    }

                    await CursorHelper.ClearCursorTask();

                    thisItem = control.Inventory.Items.FirstOrDefault();
                    if (thisItem == null)
                        break;
                }
            }

            return true;
        }
        public void Start()
        {
            _forceUnsocketGems = false;
        }

        public void Stop()
        {
        }


        public async Task<bool> Run()
        {
            if (!_forceUnsocketGems)
            {
                return false;
            }


            _forceUnsocketGems = false;

            var meEquippedItem = LokiPoe.Me.EquippedItems;
            foreach (var it in meEquippedItem)
            {
                var control = GetInventoryByItem(it);
                if (control.Inventory.Items.FirstOrDefault() == null)
                {
                    continue;
                }
                // Unsoket all gems.
                await RemoveAllGemsFromItem(control);

            }

            return true;
        }

        public void Tick()
        {
        }

        public static InventoryControlWrapper GetInventoryByItem(Item item)
        {
            return LokiPoe.InGameState.InventoryUi.AllInventoryControls.FirstOrDefault(x => x.Inventory.Items.Contains(item));
        }
    }
}
