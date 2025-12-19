using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.BotFramework;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;

namespace Resetter.Extensions
{
    public static class CursorHelper
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
        public static async Task<ClearCursorResults> ClearCursorTask(int maxTries = 3)
        {
            var cursMode = LokiPoe.InGameState.CursorItemOverlay.Mode;

            if (cursMode == LokiPoe.InGameState.CursorItemModes.None)
            {
                Log.DebugFormat("[ClearCursorTask] Nothing is on cursor, continue execution");
                return ClearCursorResults.None;
            }

            if (cursMode == LokiPoe.InGameState.CursorItemModes.VirtualMove || cursMode == LokiPoe.InGameState.CursorItemModes.VirtualUse)
            {
                Log.DebugFormat("[ClearCursorTask] VirtualMode detected, pressing escape to clear");
                LokiPoe.Input.SimulateKeyEvent(Keys.Escape, true, false, false);
                return ClearCursorResults.None;
            }

            var cursorhasitem = LokiPoe.InGameState.CursorItemOverlay.Item;
            // there is a item on the cursor let clear it
            int attempts = 0;
            while (cursorhasitem != null && attempts < maxTries)
            {
                if (attempts > maxTries)
                    return ClearCursorResults.MaxTriesReached;

                if (!LokiPoe.InGameState.InventoryUi.IsOpened)
                {
                    await OpenInventory();
                    //await LibCoroutines.OpenInventoryPanel();
                    await Coroutines.LatencyWait();
                    await Coroutines.ReactionWait();
                    if (!LokiPoe.InGameState.InventoryUi.IsOpened)
                        return ClearCursorResults.InventoryNotOpened;
                }

                int col, row;
                if (!LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory.CanFitItem(cursorhasitem.Size, out col, out row))
                {
                    Log.ErrorFormat("[ClearCursorTask] No room for cursor item in inventory Dropping/Destroing Item.");
                    await DropItem();
                }
                else
                {
                    var res = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.PlaceCursorInto(col, row);
                    if (res == PlaceCursorIntoResult.None)
                    {
                        if (!await WaitForCursorToBeEmpty())
                        {
                            Log.ErrorFormat("[ClearCursorTask] WaitForCursorToBeEmpty failed.");
                        }
                        else
                        {
                            await Coroutines.ReactionWait();
                            return ClearCursorResults.None;
                        }
                    }

                    Log.DebugFormat("[ClearCursorTask] Placing item into inventory failed, Err : {0}", res);
                    switch (res)
                    {
                        case PlaceCursorIntoResult.ItemWontFit:
                            //return Results.ClearCursorResults.NoSpaceInInventory;
                            break;
                        case PlaceCursorIntoResult.NoItemToMove:
                            return ClearCursorResults.None;
                    }
                }

                await Coroutine.Sleep(3000);
                await Coroutines.LatencyWait();
                await Coroutines.ReactionWait();
                cursorhasitem = LokiPoe.InGameState.CursorItemOverlay.Item;
                attempts++;
            }

            return ClearCursorResults.None;
        }

        public static async Task DropItem()
        {
            if (LokiPoe.InGameState.CursorItemOverlay.Item == null) return;
            if (LokiPoe.Me.IsInTown || LokiPoe.Me.IsInHideout)
            {
                if (LokiPoe.InGameState.CursorItemOverlay.Item.Rarity != Rarity.Quest)
                {
                    if (LokiPoe.InGameState.CursorItemOverlay.Item.Name == "Mirror of Kalandra" || LokiPoe.InGameState.CursorItemOverlay.Item.FullName == "Headhunter")
                    {
                        Log.ErrorFormat("[ClearCursorTask] No room for cursor item in inventory.");
                        Log.ErrorFormat("[ClearCursorTask] The cursor is to much precious to destroy, Stop bot.");
                        BotManager.Stop();
                        return;
                    }
                    LokiPoe.InGameState.ChatPanel.Chat("/destroy");
                    await Coroutines.LatencyWait();
                }

                await CursorHelper.ClearCursorTask();
            }
            else
            {
                var flaskName = LokiPoe.InGameState.CursorItemOverlay.Item.FullName;
                var flasklevel = LokiPoe.InGameState.CursorItemOverlay.Item.BaseRequiredLevel;
                var flaskQuality = LokiPoe.InGameState.CursorItemOverlay.Item.Quality;

                while (LokiPoe.InGameState.CursorItemOverlay.Item != null)
                {
                    MouseManager.SetMousePos("DropItem", LokiPoe.Me.Position);

                    await Coroutines.LatencyWait((float)MathEx.Random(1.5, 2.5));

                    MouseManager.ClickLMB();

                    await Coroutines.LatencyWait((float)MathEx.Random(1.5, 2.5));

                    Stopwatch sw = Stopwatch.StartNew();

                    while (LokiPoe.InGameState.CursorItemOverlay.Item != null)
                    {
                        if (sw.ElapsedMilliseconds > 5000)
                            break;
                    }
                }
                await Coroutines.LatencyWait();                
            }
        }

        public static async Task<bool> OpenInventory(bool closeBlocking = true)
        {
            if (LokiPoe.InGameState.InventoryUi.IsOpened && !LokiPoe.InGameState.PurchaseUi.IsOpened &&
                !LokiPoe.InGameState.SellUi.IsOpened)
            {
                return true;
            }

            if (closeBlocking)
            {
                await Coroutines.CloseBlockingWindows();
            }

            LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.open_inventory_panel);

            if (!await Wait.For(() => LokiPoe.InGameState.InventoryUi.IsOpened && InventoryUi.InventoryControl_Main != null, "inventory panel opening"))
            {
                return false;
            }

            await Wait.Sleep(20);
            return true;
        }

        public static async Task<bool> WaitForCursorToBeEmpty(int timeout = 5000)
        {
            var sw = Stopwatch.StartNew();
            while (LokiPoe.InstanceInfo.GetPlayerInventoryItemsBySlot(InventorySlot.Cursor).Any())
            {
                Log.InfoFormat("[WaitForCursorToBeEmpty] Waiting for the cursor to be empty.");
                await Coroutines.LatencyWait();
                if (sw.ElapsedMilliseconds > timeout)
                {
                    Log.InfoFormat("[[WaitForCursorToBeEmpty] Timeout while waiting for the cursor to become empty.");
                    return false;
                }
            }
            return true;
        }

        public static async Task<bool> WaitForCursorToHaveItem(int timeout = 5000)
        {
            var sw = Stopwatch.StartNew();
            while (!LokiPoe.InstanceInfo.GetPlayerInventoryItemsBySlot(InventorySlot.Cursor).Any())
            {
                Log.InfoFormat("[WaitForCursorToHaveItem] Waiting for the cursor to have an item.");
                await Coroutines.LatencyWait();
                if (sw.ElapsedMilliseconds > timeout)
                {
                    Log.InfoFormat("[WaitForCursorToHaveItem] Timeout while waiting for the cursor to contain an item.");
                    return false;
                }
            }
            return true;
        }

        public enum ClearCursorResults
        {
            None,
            InventoryNotOpened,
            NoSpaceInInventory,
            MaxTriesReached
        }

    }
}
