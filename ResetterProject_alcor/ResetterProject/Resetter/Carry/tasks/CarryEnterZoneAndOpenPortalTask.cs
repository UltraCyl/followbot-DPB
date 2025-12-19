using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml.Linq;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.Loki.Game;
using log4net;
using Resetter.Extensions;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using SkillBar = DreamPoeBot.Loki.Game.LokiPoe.InGameState.SkillBarHud;
using DreamPoeBot.Loki.Game.Objects;
using System.Net;
using System.Text;
using DreamPoeBot.Loki.Controllers;
using System.Diagnostics;
using System.Windows.Forms;
using System;
namespace Resetter
{
    public class CarryEnterZoneAndOpenPortalTask : ITask
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
        private bool _forceEnterZone;
        private bool _forceOpenMapdevice;
        private bool _stashOpened;

        public string Author => "Allure_";
        public string Description => "Task for party.";
        public string Name => "EnterZoneTask";
        public string Version => "1.0";

        public string templar = "Timeless Templar Emblem";
        public string maraketh = "Timeless Maraketh Emblem";
        public string eternal = "Timeless Eternal Emblem";
        public string karui = "Timeless Karui Emblem";
        public string vaal = "Timeless Vaal Emblem";
        public bool _forceOpenStash;
        public bool _forceEnterHideout;
        public bool _shouldEnterZone;
        public string tabName = "frag";
        public bool _isInventoryClear;
        public bool _shouldOpenStash;
        public int _mapDeviceBuggedCounter;
        public DateTime _stuckTimeCounter;
        public bool _forceMuleCarry;



        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public MessageResult Message(DreamPoeBot.Loki.Bot.Message message)
        {
            if (message.Id == Messages.CARRY_ENTER_ZONE)
            {
                Log.Info("Enter Zone Button clicked. ");
                _forceEnterZone = true;
                return MessageResult.Processed;
            }

            return MessageResult.Unprocessed;
        }


        public void Start()
        {
            
            _mapDeviceBuggedCounter = 0;
            _shouldOpenStash = false;
            _forceEnterZone = false;
            _forceOpenMapdevice = false;
            _forceOpenStash = false;
            _shouldEnterZone = true;
            _isInventoryClear = false;
            _forceEnterHideout = false;
            _forceMuleCarry = ResetterSettings.Instance.CarryMule;
        Log.Info("Bot Started");
        }

        public void Stop()
        {
        }


        public async Task<bool> Run()
        {

           

            var stashIsUp = _stashOpened;
            var templarE = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory.Items.FirstOrDefault(i => i.FullName == templar);
            var karuiE = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory.Items.FirstOrDefault(i => i.FullName == karui);
            var vaalE = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory.Items.FirstOrDefault(i => i.FullName == vaal);
            var eternalE = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory.Items.FirstOrDefault(i => i.FullName == eternal);
            var marakethE = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory.Items.FirstOrDefault(i => i.FullName == maraketh);
            var Waypoint = LokiPoe.ObjectManager.Waypoint;
            var shouldEnter = _forceEnterZone;
            var mapDevice = LokiPoe.ObjectManager.MapDevice;
            var hash = LokiPoe.LocalData.AreaHash;
            var mule = _forceMuleCarry;

            var portal = LokiPoe.ObjectManager.Portals.FirstOrDefault(x => x.Name == "Domain of Timeless Conflict");
            if (!LokiPoe.IsInGame || LokiPoe.Me.IsInTown && _forceEnterHideout == false)
            {
                Log.Info("We are not In Hideout. force enter hideout ");
                _forceEnterZone = false;
                _forceEnterHideout = true; 
                return false;
            }
            if (_forceEnterHideout == true && LokiPoe.Me.IsInTown)
            {

                Log.Info("Now proceed to interact with waypoint and tele to hideout");
                Log.Info($"Show waypoint disant {Waypoint.Distance.ToString()}");
                do
                {
                    Log.Info("Waypoint too far, moving to waypoint");
                    PlayerMoverManager.MoveTowards(Waypoint.Position);
                } while (Waypoint.Distance > 10);
                await Coroutines.InteractWith(Waypoint);
                await Coroutine.Sleep(4000);
                if (!LokiPoe.InGameState.WorldUi.IsOpened)
                {
                    Log.Info("Hideout panel is not showned, something bugged. please check");
                    SendMs($"{LokiPoe.Me.Name} Carry's hideout panel is not showned, something bugged. please check!!!!!");
                    BotManager.Stop();
                }
                else
                {
                    Log.Info("Hideout panel is up. Going to Hideout");
                    LokiPoe.InGameState.WorldUi.GoToHideout();
                    if (await Wait.ForAreaChange(hash))
                        Log.Info("successfully went to hideout");
                    await Coroutine.Sleep(3000);
                    _forceEnterHideout = false;
                    return true;
                }
            }
            //we are in hideout

          





            if (portal == null && _forceOpenMapdevice == false && _shouldOpenStash == false&& LokiPoe.Me.IsInHideout == true)//should open map device
            {
                Log.Info("No portal, open map device");
                _forceOpenMapdevice = true;
                return false;
            }
            if (portal == null && _forceOpenMapdevice == true && _forceOpenStash == false &&LokiPoe.Me.IsInHideout==true)

            {
               
                if (!LokiPoe.InGameState.MasterDeviceUi.IsOpened)
                {
                    if(_mapDeviceBuggedCounter >= 2)
                    {
                        Log.Info("Map device is bugged, Take a look");
                        SendMs($"{LokiPoe.Me.Name}carry party is bugged!!!!!");
                        BotManager.Stop();

                    }
                    Log.Info("Map device is not up.Interacting with map device");
                    if(await Coroutines.InteractWith(mapDevice))
                    {
                        Log.Info("Map device detected");
                        await Coroutines.LatencyWait();
                        await Coroutines.ReactionWait();
                        await Coroutine.Sleep(2000);
                        _mapDeviceBuggedCounter = 0;
                        return true;
                    }


                    Log.Info("Map device is bugged, try again");
                    _mapDeviceBuggedCounter++;
                    
                    return false;
                } 
                else
                {
                    if (templarE == null || karuiE == null || vaalE == null || eternalE == null || marakethE == null)
                    {
                        //Could not find any emblem, now will try to open stash and find emblem
                        //   Log.Info("Could not find 5 emblems. openning stash");
                        //  await Coroutines.InteractWith(LokiPoe.ObjectManager.Stash);


                        SendMs($"No Frag, carry {LokiPoe.Me.Name} stopped");

                        Log.Info("Could not find 5 emblems. Need to open stash");
                        _shouldOpenStash = true;
                        _forceOpenStash = true;
                        _forceOpenMapdevice = false;
                        await Coroutines.CloseBlockingWindows();
                        await Coroutines.LatencyWait();
                        return true;

                        //  BotManager.Stop();
                        //  await Coroutines.CloseBlockingWindows();
                        // await Coroutines.LatencyWait();

                    }
                    Log.Info("Found 5 emblems. putting them into map device inventory");
                    // place all 5 emblems into map device with fastmove
                    LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMove(templarE.LocalId);
                    await Coroutine.Sleep(300);
                    LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMove(karuiE.LocalId);
                    await Coroutine.Sleep(300);
                    LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMove(vaalE.LocalId);
                    await Coroutine.Sleep(300);
                    LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMove(eternalE.LocalId);
                    await Coroutine.Sleep(300);
                    LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMove(marakethE.LocalId);
                    await Coroutine.Sleep(300);
                    if (LokiPoe.InGameState.MasterDeviceUi.FiveSlotInventoryControl.Count() >= 5)
                    {
                        LokiPoe.InGameState.MasterDeviceUi.Activate();

                        _stuckTimeCounter = DateTime.Now;
                        _forceOpenMapdevice = false;

                        _forceEnterZone = true;
                        await Coroutine.Sleep(5000);
                        return true;

                    }
                }


            }

            if (portal != null)
            {

                Log.Info("Portals detected, waiting until 1 portal left to enter.");
                var portalCount = LokiPoe.ObjectManager.Portals.Count(x => x.Name == "Domain of Timeless Conflict");
                _stuckTimeCounter = DateTime.Now;
                do
                {
                    if (!LokiPoe.IsInGame || LokiPoe.Me.IsDead || BotManager.IsStopping)
                        return false;
                    if (portalCount == 1)
                    {
                        Log.Info("Only 1 portal left, entering portal");
                        portal = LokiPoe.ObjectManager.Portals.FirstOrDefault(x => x.Name == "Domain of Timeless Conflict");

                        if (await Coroutines.InteractWith(portal))
                        {
                            _stuckTimeCounter = new DateTime();
                            if (await Wait.ForAreaChange(hash))

                                break;
                        }

                    }
                    else
                    {

                        Log.Info("Portals detected, waiting until 1 portal left to enter.");

                        if (_stuckTimeCounter + TimeSpan.FromMilliseconds(60000) < DateTime.Now)
                        {
                            SendMs($"{LokiPoe.Me.Name}carry party stayed in HO for too long, please check!!!!!");
                            Log.Error("Taking too long to join 5ways, please check if something is wrong in the party !");
                            BotManager.Stop();
                            break;
                        }


                    }
                    portalCount = LokiPoe.ObjectManager.Portals.Count(x => x.Name == "Domain of Timeless Conflict");

                    continue;
                }
                while (portalCount <= 6 && portal != null);
            
            }

            if (_forceOpenStash == true && _forceOpenMapdevice == false && _shouldOpenStash == true && stashIsUp == false)
            {
                Log.Info("Looking for stash");
                if (StashUi.IsOpened)
                    return true;

                var stashPos = LokiPoe.ObjectManager.Stash.Position;
                if (LokiPoe.CurrentWorldArea.IsTown)
                {

                    var stashObj = LokiPoe.ObjectManager.Stash;
                    if (stashObj == null)
                    {
                        Log.Error("[OpenStash] Fail to find any Stash nearby.");
                        return false;
                    }

                    stashPos = stashObj.Position;

                }



                if (LokiPoe.ConfigManager.IsAlwaysHighlightEnabled) LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.highlight_toggle, true, false, false);
                await Coroutines.CloseBlockingWindows();
                await Coroutines.LatencyWait();

                if (!await Coroutines.InteractWith(LokiPoe.ObjectManager.Stash))
                {
                    Log.Info("stash not openned? ");
                    return false;
                }
                await Coroutine.Sleep(2000);
                _stashOpened = true;

                Log.Info("stash opened ");
                await Wait.SleepSafe(LokiPoe.Random.Next(200, 400));
                await Wait.Sleep(100);
                return true;

            }
            else if (stashIsUp && _forceOpenStash == true)
            {

                // move fragment in this 
                Log.Info("Scanning Stash ");
                LokiPoe.InGameState.StashUi.TabControl.SwitchToTabKeyboard(tabName);
                await Wait.Sleep(500);
                //now check if fragment stash have fragments 
                var templarInStash = LokiPoe.InGameState.StashUi.FragmentTab.General.TimelessTemplarEmblem;
                var karuiInStash = LokiPoe.InGameState.StashUi.FragmentTab.General.TimelessKaruiEmblem;
                var marakethInStash = LokiPoe.InGameState.StashUi.FragmentTab.General.TimelessMarakethEmblem;
                var eternalInStash = LokiPoe.InGameState.StashUi.FragmentTab.General.TimelessEternalEmblem;
                var vaalInStash = LokiPoe.InGameState.StashUi.FragmentTab.General.TimelessVaalEmblem;
                if (templarInStash == null || karuiInStash == null || marakethInStash == null || eternalInStash == null || vaalInStash == null)
                {

                    SendMs($"No Frag ins stash, carry {LokiPoe.Me.Name} stopped");
                    BotManager.Stop();
                   
                }
                else if (LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory.InventorySpacePercent < 100 && _isInventoryClear == false)
                {
                    SendMs($"Inventory is not cleared on carry {LokiPoe.Me.Name}"); //inventory is not cleared
                    bool _isStashfull = false;
                    for (int i = 1; i <= 4; i++)
                    {


                        LokiPoe.InGameState.StashUi.TabControl.SwitchToTabKeyboard(i.ToString());  // switch to tab stash name 1 to be our dump stash.
                                                                                                   //clear inventory stask 
                        var inv = LokiPoe.InGameState.InventoryUi.InventoryControl_Main;
                        foreach (var item in inv.Inventory.Items)
                        {
                            if (item != null && _isStashfull == false)
                            {
                                _isInventoryClear = false;
                                if (!LokiPoe.InGameState.StashUi.InventoryControl.Inventory.CanFitItem(item))
                                {
                                    Log.ErrorFormat($" No room for  item in stash {i}, switching tab");

                                    _isStashfull = true;

                                }
                                LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMove(item.LocalId);

                            } else
                            {
                                break;
                            }

                        }
                        if (_isStashfull)
                        {
                            _isStashfull = false;
                            continue;
                        }
                        if (i == 4 && _isStashfull)
                        {
                            SendMsCarry($"{LokiPoe.Me.Name} Carry has no stash slot left. please check your stash");
                            BotManager.Stop();
                        }



                    }



                    _isInventoryClear = true;
                    return true;

                }
                else// now whe have fragments in stash, inventory is cleared, move emblems into inventory
                {
                    if (templarInStash == null || karuiInStash == null || marakethInStash == null || eternalInStash == null || vaalInStash == null)
                    {
                        SendMs($"{LokiPoe.Me.Name} carry doesnt have enough 5way emblems in stash. stopping bot");
                        BotManager.Stop();
                    }
                    for (int i = 0; i <= 9; i++)
                    {
                        Log.Info("Taking Emblems from stash");
                        templarInStash.FastMove();
                        await Wait.SleepSafe(LokiPoe.Random.Next(30, 100));
                        karuiInStash.FastMove();
                        await Wait.SleepSafe(LokiPoe.Random.Next(30, 100));
                        marakethInStash.FastMove();
                        await Wait.SleepSafe(LokiPoe.Random.Next(30, 100));
                        eternalInStash.FastMove();
                        await Wait.SleepSafe(LokiPoe.Random.Next(30, 100));
                        vaalInStash.FastMove();
                        await Wait.SleepSafe(LokiPoe.Random.Next(30, 100));

                    }
                    if (LokiPoe.ConfigManager.IsAlwaysHighlightEnabled) LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.highlight_toggle, true, false, false);
                    await Coroutines.CloseBlockingWindows();
                    await Coroutines.LatencyWait();
                    _mapDeviceBuggedCounter = 0;
                    _isInventoryClear = false;
                    _shouldOpenStash = false;
                    _forceOpenStash = false;
                    _forceOpenMapdevice = true;
                    _stashOpened = false;
                    _forceEnterZone = true;
                    return true;

                    // 
                }


            


            // stash management here
            if (StashUi.IsOpened)
                return true;

            var stashPos = LokiPoe.ObjectManager.Stash.Position;
            if (LokiPoe.CurrentWorldArea.IsTown)
            {

                var stashObj = LokiPoe.ObjectManager.Stash;
                if (stashObj == null)
                {
                    Log.Error("[OpenStash] Fail to find any Stash nearby.");
                   return false;
                }
                stashPos = stashObj.Position;
            }
            else
            {
                var stashObj = LokiPoe.ObjectManager.Stash;
                if (stashObj == null)
                {
                    Log.Error("[OpenStash] Fail to find any Stash nearby.");
                    return false;
                }
                stashPos = stashObj.Position;
            }

            if (LokiPoe.ConfigManager.IsAlwaysHighlightEnabled) LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.highlight_toggle, true, false, false);
            await Coroutines.CloseBlockingWindows();
            await Coroutines.LatencyWait();

            if (!await Coroutines.InteractWith(LokiPoe.ObjectManager.Stash))
            {
                Log.Info("stash not openned? ");
                return false;
            }

            _stashOpened = true;
            Log.Info("stash opened ");
            await Wait.SleepSafe(LokiPoe.Random.Next(200, 400));
            await Wait.Sleep(100);
            return true;

        }
           /* else
            {
                Log.Info("Found 5 emblems. putting them into map device inventory");
                // place all 5 emblems into map device with fastmove
                LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMove(templarE.LocalId);
                await Coroutine.Sleep(300);
                LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMove(karuiE.LocalId);
                await Coroutine.Sleep(300);
                LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMove(vaalE.LocalId);
                await Coroutine.Sleep(300);
                LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMove(eternalE.LocalId);
                await Coroutine.Sleep(300);
                LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMove(marakethE.LocalId);
                await Coroutine.Sleep(300);
                if (LokiPoe.InGameState.MasterDeviceUi.FiveSlotInventoryControl.Count() >= 5)
                {
                    LokiPoe.InGameState.MasterDeviceUi.Activate();


                    await Coroutine.Sleep(5000);
                    return false;

                }



            }
            */
            
            

            return false;




            //now we need cursorhelper to find emblem and put them into map device.

            // LokiPoe.InGameState.InventoryUi.InventoryControl_Main.FastMove();


        }


        //interact with map device and putting 5ways set in

       
        
            
        






            
        
        static void SendMs(string message)
        {
            string webhook = "https://discord.com/api/webhooks/1273617275173212161/4y1YPqTXS6-bQLv2bxpUD88IUjTlYApe-rJ_r2MlSZhCyrFCoYB-HC9oDh07XhcFSayb";

            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string payload = "{\"content\": \"" + message + "\"}";
            client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
        }

        public void Tick()
        {
        }
        static void SendMsCarry(string message)
        {
            string webhook = "https://discord.com/api/webhooks/1345295119728902144/qFSDzZ_xfHNX48dzV2v5oMW22prxr-2zdbVHeuiAFFpGT4tk21jNMy4HgJfS2d7gj3yE";

            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string payload = "{\"content\": \"" + message + "\"}";
            client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
        }

        public MessageResult Messenger(DreamPoeBot.Loki.Bot.Message message)
        {
            throw new System.NotImplementedException();
        }
    }
}

