using DreamPoeBot.Loki.Game;
using Resetter.Extensions;
using System.Linq;
using System.Threading.Tasks;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game.Objects;
using log4net;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using System.Net.Sockets;

namespace Resetter.tasks
{
    public class SocketAllGemsIntoItemTask : ITask
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
        public bool _forceSocketGems;

        public string Author => "Lesun";
        public string Description => "Task for socketting gems from inventory into equipped items.";
        public string Name => "UnsocketAllGemsTask";
        public string Version => "1.0";


        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }
        public MessageResult Message(Message message)
        {
            
            return MessageResult.Unprocessed;
        }


        public static async Task<bool> SocketAllGemsIntoItem(InventoryControlWrapper control)
        {
            await CursorHelper.OpenInventory(true);
            Log.Info("Openning inventory");

            while (true)
            {
                var thisItem = control.Inventory.Items.FirstOrDefault();
                if (thisItem == null)
                {
                    break;
                }

                var count = thisItem.SocketedGems.Count();
                var skippedGemsCount = 0;
                Log.Info($"Show count : {count}");

                Log.Info("Start unsocket all gems. Part 1");
                var index = -1;
                if (count == 0)
                {
                    break;
                }

                for (int i = 0; i < count; i++)
                {
                    if (thisItem.SocketedGems.Count(g => g != null) == count) return false;
                    index++;
                    Log.Info($"Show i : {i}");
                    Log.Info($"SHOW INDEX: {index}");

                    Log.Info($"Real Gem Count: {skippedGemsCount}");

                    // checking item socket indexes
                    foreach (var socket in thisItem.SocketedGems)
                    {
                        return false;
                    }


                    if (!await Wait.For(() => LokiPoe.InGameState.CursorItemOverlay.Item != null,
                        "Gem to appear on cursor.", 100, 6000))
                    {
                        continue;
                    }


                    await CursorHelper.ClearCursorTask();
                    // if (index+skippedGemsCount > count-1 )return true;
                    thisItem = control.Inventory.Items.FirstOrDefault();
                    if (thisItem == null)
                        break;
                }
            }

            return true;
        }
        public void Start()
        {
            _forceSocketGems = false;
        }

        public void Stop()
        {
        }


        public async Task<bool> Run()
        {
            if (_forceSocketGems)
            {
                return false;
            }


            _forceSocketGems = false;

            var meEquippedItem = LokiPoe.Me.EquippedItems;
            foreach (var it in meEquippedItem)
            {
                
                var control = GetInventoryByItem(it);
                if (control.Inventory.Items.FirstOrDefault() == null || control == LokiPoe.InGameState.InventoryUi.InventoryControl_TherdRing)
                {
                    continue;
                }
                // Unsoket all gems.
                Log.Info($"Start socketing gems to item: {it.FullName} ");
                await SocketAllGemsIntoItem(control);

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
