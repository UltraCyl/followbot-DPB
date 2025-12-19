using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DreamPoeBot.BotFramework;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Components;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.Loki.Elements;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.Objects;
using log4net;
using Newtonsoft.Json.Serialization;
using Resetter.Extensions;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using static Resetter.Extensions.CursorHelper;
using Message = DreamPoeBot.Loki.Bot.Message;
using SkillBar = DreamPoeBot.Loki.Game.LokiPoe.InGameState.SkillBarHud;

namespace Resetter
{
    public delegate bool ProcessNotificationEx(NotificationData data, NotificationType type);
    public class EnterZoneTask : ITask
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
        public bool _openStash;
        private bool _forceEnterZone;
        private bool _mule;
        private bool _forceStashing;
        private bool _forceSocketGems;
        private bool _socketGemsNow;

        public string Author => "Allure_";
        public string Description => "Task for party.";
        public string Name => "EnterZoneTask";
        public string Version => "1.0";


        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public MessageResult Message( Message message)
        {
            if (message.Id == Messages.ENTER_ZONE)
            {
                _forceEnterZone = true;
                return MessageResult.Processed;
            }

            return MessageResult.Unprocessed;
        }


        public void Start()
        {
            _openStash = false;
            _forceEnterZone = false;
            _mule = true;
            _forceStashing = false;
            _forceSocketGems = false;
            _socketGemsNow = false;
            
        }

        public void Stop()
        {
        }

        private static async Task<bool> HandleNotificationWrapper(string accountNameToBeAccepted, LokiPoe.InGameState.NotificationType acceptedNotificationType)
        {
            if (LokiPoe.InGameState.NotificationHud.IsOpened)
            {
                LokiPoe.InGameState.ProcessNotificationEx isTradeRequestToBeAccepted = (x, y) =>
                {
                    Log.WarnFormat("[ServeCurrencyCustomer] Detected {0} request from {1}",
                        y.ToString(), x);
                    Log.Info($"testing account name {x.AccountName}");
                    return (x.AccountName.ToLower().Contains(accountNameToBeAccepted.ToLower()) || x.CharacterName.ToLower().Contains(accountNameToBeAccepted.ToLower()) && y == acceptedNotificationType);
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
                    Log.Info("Request Handled");
                    await Coroutines.ReactionWait();
                    return true;
                }

                await Coroutines.ReactionWait();
            }

            return false;
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
        public static InventoryControlWrapper GetInventoryByItem(Item item)
        {
            return LokiPoe.InGameState.InventoryUi.AllInventoryControls.FirstOrDefault(x => x.Inventory.Items.Contains(item));
        }
        private bool ShouldViewItem(Inventory inventory, Item item )
        {
            if (!LokiPoe.InGameState.TradeUi.IsOpened) return false;
            

            if (LokiPoe.InGameState.TradeUi.TradeControl.InventoryControl_OtherOffer.IsItemTransparent(item.LocalId)) return true;
            return false;

        }
        private bool IsOpened()
        {
           
            return LokiPoe.InGameState.TradeUi.IsOpened;

        }
        public async Task<bool> Run()
        {
           
            if (string.IsNullOrWhiteSpace(ResetterSettings.Instance.ControllerCharacterNameWhitelist))
                return false;

            var nameList =  ResetterSettings.Instance.ControllerCharacterNameWhitelist.Split(',')
                .Select(x => x.Trim().ToLower()).ToList();
            



            var shouldOpenStash = _openStash;
            var muleLeaderName = ResetterSettings.Instance.MuleLeaderCharacterName.ToString();
            var shouldStash = _forceStashing;
            var shouldSocketGems = _forceSocketGems;
            var shoulMule = _mule;
            var shouldEnter = _forceEnterZone;
            var timelessPortal = LokiPoe.ObjectManager.Portals.FirstOrDefault(x => x.Name == "Domain of Timeless Conflict");
            var portal = LokiPoe.ObjectManager.Portals.FirstOrDefault(x => x.Name != "Domain of Timeless Conflict");
            if (LokiPoe.InGameState.NotificationHud.IsOpened && LokiPoe.Me.IsInHideout == false)
            {
                foreach (var n in nameList)
                {
                    if (await HandleNotificationWrapper(n, NotificationType.Party))
                    {
                        Log.Info("Party Request Handled");

                        return true;
                    }
                    await Coroutine.Sleep(300);

                }
                return false;
            }
            if (!LokiPoe.IsInGame || !LokiPoe.Me.IsInHideout)
            {
                _forceEnterZone = false;
                _forceEnterZone = false;
                return false;
            }
            if (SkillGemHud.AreIconsDisplayed)
            {

                do
                {
                    await Coroutines.CloseBlockingWindows();
                    SkillGemHud.HandlePendingLevelUps((x, y, z) => true);

                } while (SkillGemHud.AreIconsDisplayed);

                await Coroutine.Sleep(1000);

                return true;


            }
            if (portal == null && timelessPortal == null)
            {

                if (LokiPoe.InGameState.NotificationHud.IsOpened)
                {
                    Log.Info($"Mule Leader Account: {muleLeaderName.ToString()}");
                    Log.Info("Request detected, handling request");
                    if ((await HandleNotificationWrapper(muleLeaderName, NotificationType.Trade)))   //trade request from muleLeader accepeted successfully
                    {
                        Log.Info("Trade Request Handled");
                        await Coroutine.Sleep(300);
                        return true;
                    }

                   


                }
                    //now tradeUI is openned
                while (LokiPoe.InGameState.TradeUi.IsOpened)
                        {
                    Log.Info("Trade Window Is up");
                    var inv = LokiPoe.InGameState.InventoryUi.InventoryControl_Main;

                            foreach (var item in inv.Inventory.Items)
                            {
                                if (item != null)
                                {

                            if (!LokiPoe.InGameState.InventoryUi.InventoryControl_Main.IsItemTransparent(item.LocalId))
                                {
                                LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMove(item.LocalId);
                                await Wait.SleepSafe(20, 50);

                                }
                            

                                }
                                else
                                {
                                    break;
                                }

                            }
                            Log.Info("successfully gave all items in our inventory");

                            //now scan giver inventory
                            Log.Info("now going to scan muleLeader inventory");
                            var muleLeaderOffer = TradeUi.TradeControl.InventoryControl_OtherOffer;

                            var acceptButton = TradeUi.TradeControl.AcceptButtonText;
                            var confirmVisible = TradeUi.TradeControl.IsConfirmLabelVisible;
                            var muleLeaderOfferInventory = TradeUi.TradeControl.InventoryControl_OtherOffer.Inventory;

                            if (TradeUi.TradeControl.OtherAcceptedTheOffert)
                            {
                                Log.Info("Mule Leader accepted the trade, now scanning mule Leader inventory items");
                                var muleLeaderOfferItems = TradeUi.TradeControl.InventoryControl_OtherOffer.Inventory?.Items;
                                
                                    if (muleLeaderOfferItems != null)
                                    {
                                        Log.Info("Hovering mule Leader items");
                                        //hover the item
                                        TradeUi.TradeControl.InventoryControl_OtherOffer.ViewItemsInInventory(ShouldViewItem, IsOpened);

                                         await Wait.SleepSafe(50, 200);


                        }
                                    else
                                    {
                                        Log.Info("Couldnt see any item in leader inventory");
                                        
                                    }
                                
                                Log.Info("finished hovering, accepting trade");
                                if (acceptButton == "accept")
                                {
                                    TradeUi.TradeControl.Accept();
                                    await Wait.SleepSafe(120, 1000);
                                    Log.Info("trade completed");
                                    _forceStashing = true;
                                    
                                   break ;
                                }
                                else
                                {
                                    Log.Info("trade button is not accept, something is wrong");
                                    return false;
                                }
                        }
                        else
                        {
                            Log.Info("muleLeader have not accepted the trade, keep waiting");
                            return false;
                        }
                        }
                if (shouldStash)
                {
                    //start stashing
                    //first close all blocking windows
                    //then open inventory
                    if (LokiPoe.ConfigManager.IsAlwaysHighlightEnabled) LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.highlight_toggle, true, false, false);
                   

                    await CursorHelper.OpenInventory();
                    await Coroutines.LatencyWait();
                    await Wait.SleepSafe(120, 200);
                    if (LokiPoe.InGameState.InventoryUi.IsOpened && shouldOpenStash == false)
                    {
                        Log.Info("Checking gems in inventory, if only gems left in inventory , dont open stash");
                         
                        var gems = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory?.Items.FirstOrDefault(i => i.Metadata.Contains("Metadata/Items/Gems"));
                        var notGems = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory?.Items.FirstOrDefault(i => i.Metadata.Contains("Metadata/Items/Gems") == false);
                        // Log.Info($"show gems :  {gems?.Name.ToString()} ");

                        if ( notGems != null ) // no gems detected , can stash now
                        {

                            Log.Info("non-gems item[s] detected, stashing now");
                            _openStash = true;
                                return true;
                        }
                        else if(gems != null && notGems == null)
                        {
                            Log.Info("discovered gems, dont stash!");
                            _forceStashing = false;
                            _openStash = false;
                            _socketGemsNow = true;
                            return false;
                        }

                                
                            

                        
                    }
                    if (shouldOpenStash == true)//interact with stash 
                    {
                        await Coroutines.CloseBlockingWindows();
                        await Coroutines.LatencyWait();
                        await Wait.SleepSafe(120, 150);
                        if (!await Coroutines.InteractWith(LokiPoe.ObjectManager.Stash))
                        {
                            Log.Info("stash not openned? ");
                            return false;
                        }
                        else
                        {
                            await Wait.SleepSafe(120, 150);
                           
                            List<Item> nonGemList = new List<Item>();
                            List<Item> fragList = new List<Item>();
                            //clear inventory stask 
                            //var nonGemItems = inv.Inventory?.Items.FirstOrDefault(n => n.Metadata.Contains("Metadata/Items/Gems") == false);
                            var inv = LokiPoe.InGameState.InventoryUi.InventoryControl_Main;
                            if (inv.Inventory?.Items == null)
                            {
                                //inventory is cleared
                                //finished stashing
                                Log.Info("no item in inventory");
                                _forceStashing = false;
                                _openStash = false;
                                
                            }
                            foreach (var it in inv.Inventory?.Items)
                            {
                                if (it.Metadata.Contains("Metadata/Items/Gems") == false && it.Metadata.Contains("Metadata/Items/MapFragments") == false)
                                {
                                    nonGemList.Add(it);
                                }
                                else if (it.Metadata.Contains("Metadata/Items/MapFragments"))
                                    fragList.Add(it);
                                //
                                Log.Info("Fragment item detected");



                            }

                            // stash opened
                            bool _isStashfull = false;
                            if (fragList != null)
                            {

                                LokiPoe.InGameState.StashUi.TabControl.SwitchToTabKeyboard("frag");

                                for (; ; )
                                {
                                    fragList = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory.Items
                                        .Where(ii => ii.Metadata.Contains("Metadata/Items/MapFragments"))
                                        .ToList();

                                    if (fragList.Count == 0)
                                        break;

                                    foreach (var x in fragList)
                                    {
                                        if (x.Metadata.Contains("Karui"))
                                        {
                                            LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMoveAll(x.LocalId);
                                            await Wait.SleepSafe(100, 250);
                                            break;
                                        }
                                        if (x.Metadata.Contains("Templar"))
                                        {
                                            Log.ErrorFormat("Moving Templar");

                                            LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMoveAll(x.LocalId);
                                            await Wait.SleepSafe(100, 250);
                                            break;

                                        }
                                        if (x.Metadata.Contains("Vaal"))
                                        {
                                            Log.ErrorFormat("Moving vaal");

                                            LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMoveAll(x.LocalId);
                                            await Wait.SleepSafe(100, 250);
                                            break;

                                        }
                                        if (x.Metadata.Contains("Maraketh"))
                                        {
                                            Log.ErrorFormat("Moving maraketh");

                                            LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMoveAll(x.LocalId);
                                            await Wait.SleepSafe(100, 250);
                                            break;

                                        }
                                        if (x.Metadata.Contains("Eternal"))
                                        {
                                            Log.ErrorFormat("Moving eternal");

                                            LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMoveAll(x.LocalId);
                                            await Wait.SleepSafe(100, 250);
                                            break;

                                        }

                                        /* repeat pattern for other types */
                                    }

                                }
                                Log.Info("finished stashing Fragments");


                            }
                            if (nonGemList != null )
                            {

                                for (int i = 1; i <= 4; i++)
                                {

                                    LokiPoe.InGameState.StashUi.TabControl.SwitchToTabKeyboard(i.ToString());




                                    foreach (var x in nonGemList)
                                    {

                                        if (!LokiPoe.InGameState.StashUi.InventoryControl.Inventory.CanFitItem(x))
                                        {
                                            Log.ErrorFormat($" No room for  item in stash {i}, switching tab");

                                            _isStashfull = true;
                                            break;

                                        }
                                        Log.Info($"show item class : {x.Class.ToString()} ");
                                        LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMove(x.LocalId);

                                    }
                                    if (_isStashfull)
                                    {
                                        _isStashfull = false;
                                        continue;
                                    }
                                    if (i == 4 && _isStashfull)
                                    {
                                        //SendMsCarry($"{LokiPoe.Me.Name} Carry has no stash slot left. please check your stash");
                                        BotManager.Stop();
                                    }

                                }


                            }
                            



                               
                                
                                
                                    //finished stashing

                                    
                               
                                    
                                




                            }


                        }
                    Log.Info("finished stashing");
                    _forceStashing = false;
                    _openStash = false;
                    await Coroutines.CloseBlockingWindows();
                    await Coroutines.LatencyWait();

                
                    

                    

                           
                        


                }
                if (_socketGemsNow)
                {
                    var bot = BotManager.Current;
                    var msg = new Message("GetTaskManager");
                    bot.Message(msg);
                    var taskManager = msg.GetOutput<TaskManager>();

                    taskManager.SendMessage(TaskGroup.Enabled, new Message(Messages.SOCKET_ALL_GEMS_INTO_ITEMS));

                    Log.Info("Socket Gems Button clicked.");
                    _socketGemsNow = false;

                }



                //LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMove(templarE.LocalId);
                // await Coroutine.Sleep(300);
                //





               
                _mule = true;
                Log.Info("Could not find portal to enter with, can mule");
                return true;
            } else if (portal != null || timelessPortal != null && _forceEnterZone==false)
            {
                await Coroutines.CloseBlockingWindows();
                await Coroutines.LatencyWait();
                _socketGemsNow = false;
                _mule = false;
                _forceEnterZone= true;
                Log.Info("Portals detected, cannot mule");
                return true;
            }
            //mule handle now

            
            
            
            if (shouldEnter==true)
            {

                var timelessPortalCount = LokiPoe.ObjectManager.Portals.Count(x => x.Name == "Domain of Timeless Conflict");
                var portalCount = LokiPoe.ObjectManager.Portals.Count(x => x.Name != "Domain of Timeless Conflict");
                if ( timelessPortalCount >= 2 )
                {
                    do
                    {
                        Log.Info("More than 2 5way portals detected preparing to enter zone.");
                        //refresh portal so if the previous portal is taken, the bot will try the next one
                        var portal1 = LokiPoe.ObjectManager.Portals.FirstOrDefault(x => x.Name == "Domain of Timeless Conflict");
                        await Wait.SleepSafe(500, 1150);
                        await Coroutines.InteractWith(portal1);
                        await Coroutine.Sleep(1000);
                        if (LokiPoe.CurrentWorldArea.Name == "Domain of Timeless Conflict")
                        {
                            _forceEnterZone = false;
                            return true;
                        }
                        
                    }
                    while (timelessPortalCount >= 2 && LokiPoe.CurrentWorldArea.Name != "Domain of Timeless Conflict");

                    return true;
                }else
                {
                    shouldEnter = true;
                    do
                    {
                        Log.Info("More than 2 Map portals detected preparing to enter zone.");
                        //refresh portal so if the previous portal is taken, the bot will try the next one
                        var portal1 = LokiPoe.ObjectManager.Portals.FirstOrDefault(x => x.Name != "Domain of Timeless Conflict");
                        await Wait.SleepSafe(500, 1150);
                        await Coroutines.InteractWith(portal1);
                        await Coroutine.Sleep(1000);
                        if (LokiPoe.CurrentWorldArea.Name != "Domain of Timeless Conflict")
                        {
                            _forceEnterZone = false;
                            return true;
                        }

                    }
                    while (timelessPortalCount >= 2 && LokiPoe.CurrentWorldArea.Name != "Domain of Timeless Conflict");
                    return true;

                }

            }
            if (shouldEnter == false) return false;
            //
            
                

            

          

            
            return true;
        }

        public void Tick()
        {
        }
    }
}