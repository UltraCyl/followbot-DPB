using System.Linq;
using System.Threading.Tasks;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.Loki.Game.Objects;
using log4net;
using DreamPoeBot.Loki.Controllers;
using Resetter.Extensions;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using System.Collections.Generic;
using System.Runtime;
using System;
using DreamPoeBot.Framework.Helpers;
using DreamPoeBot.Loki.Components;
using DreamPoeBot.Loki.Game.Objects;
using System.Net;
using System.Text;
using DreamPoeBot.Loki.Game.GameData;
using System.Diagnostics;



namespace Resetter.Carry.tasks
{
    public class CarryPositionInZoneTask : ITask
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
        public bool _forceLeaveZone = false;
        private readonly HashSet<int> _processedItems = new HashSet<int>();
        List<NetworkObject> lootFilter= new List<NetworkObject>();
        public bool _lootIsUp = false;
        public bool _notifyDiscord = false;
        public int wins, loses = 0;
        public DateTime stuckTimeCounter ;
        public DateTime lootTimeCounter;
        public DateTime criticalTimeCounter;
        public string Author => "Allure_";
        public string Description => "";
        public string Name => "CarryPositionInZoneTask";
        public string Version => "1.0";


        public async Task<LogicResult> Logic(Logic logic)
        {
            
            return LogicResult.Unprovided;
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == Messages.CARRY_LEAVE_ZONE)
            {
                Log.Info("Leave button clicked. Leaving Zone");
                _forceLeaveZone = true;
                return MessageResult.Processed;
            }

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
            if (BotManager.IsStopping)
            {
                SendMs("Carry bot stopping") ; return false;
            }
            
            bool shouldLoot = _lootIsUp;
            bool shouldleave = _forceLeaveZone;
            var worldItemCount = LokiPoe.ObjectManager.GetObjectsByType<DreamPoeBot.Loki.Game.Objects.WorldItem>().Count();
            var areaName = LokiPoe.CurrentWorldArea.Name;
            var players = LokiPoe.ObjectManager.GetObjectsByType<DreamPoeBot.Loki.Game.Objects.Player>(); 
            var hash = LokiPoe.LocalData.AreaHash;
            var portal = LokiPoe.ObjectManager.Portals.FirstOrDefault();
            var partyMember = LokiPoe.InstanceInfo.PartyMembers;
            var insidePos = new Vector2i(ResetterSettings.Instance.InsideX, ResetterSettings.Instance.InsideY);
            // Log.Debug($"count member {partyMember.Count}");
            var discordUp = _notifyDiscord;
            // summon flame golem
            
        

            foreach (var m in partyMember)
            {
                if(m.MemberStatus == PartyStatus.PartyMember)
                {

                   // Log.Debug($"Show member name {m.PlayerEntry.Name}");
                    if (!m.PlayerEntry.IsOnline && discordUp == false)
                    {
                        SendMs($"{m.PlayerEntry.Name} is offline");
                        _notifyDiscord = true;
                        if (!m.PlayerEntry.IsOnline)
                        {
                            Log.Debug("[] somebody died. casting portal");
                            var portalSkill = SkillBarHud.Skills.FirstOrDefault(s => s.Name == "Portal" && s.IsOnSkillBar);
                            if (portalSkill != null && portalSkill.IsCastable)
                            {
                                await Coroutines.FinishCurrentAction();
                                await Coroutine.Sleep(100);
                                var err = SkillBarHud.Use(portalSkill.Slot, false);
                                await Coroutine.Sleep(500);
                                if (err != UseResult.None)
                                {
                                    Log.Error($"[CreateTownPortal] Fail to cast portal skill. Error: \"{err}\".");
                                    return true;
                                }

                                await Coroutine.Sleep(1000);
                            }
                            portal = LokiPoe.ObjectManager.Portals.FirstOrDefault();
                            await Coroutines.InteractWith(portal);
                            await Wait.ForAreaChange(hash);
                            SendMsCarry($" Somebody in {LokiPoe.Me.Name} just died. {loses++} loss ");

                            _forceLeaveZone = false;
                            _lootIsUp = false;
                            break;
                        }
                            return true;
                    }
                   
                }
                continue;


            }
            
            
            //Log.Debug($"Show member name {memberEntry.Name}");
            var discord = _notifyDiscord;
            if (!LokiPoe.IsInGame)
                return false;
            if (LokiPoe.Me.IsDead)
            {
                for (int i = 1; i <= 3; ++i)
                {
                    Log.Debug($"[Resurrect] Attempt: {i}");

                    if (!LokiPoe.IsInGame)
                    {
                        Log.Debug("[Resurrect] Now exiting this logic because we are no longer in game.");
                        return true;
                    }
                    if (!LokiPoe.Me.IsDead)
                    {
                        Log.Debug("[Resurrect] Now exiting this logic because we are no longer dead.");
                        return true;
                    }
                    var _hash = LokiPoe.LocalData.AreaHash;


                    var err =  LokiPoe.InGameState.ResurrectPanel.ResurrectToCheckPoint();
                        
                    
                    if (err == LokiPoe.InGameState.ResurrectResult.None)
                    {

                        await Wait.ForAreaChange(_hash);
                        SendMsCarry($" {LokiPoe.Me.Name} Carry just died. {loses++} loss ");
                        _forceLeaveZone = false;
                        _lootIsUp = false;
                        Log.Debug("[Resurrect] Player has been successfully resurrected.");

                        await Wait.SleepSafe(250);
                        return true;
                    }
                    Log.Error($"[Resurrect] Fail to resurrect. Error: \"{err}\".");
                    await Wait.SleepSafe(1000, 1500);
                    BotManager.Stop();
                }
            }
            //checking for dead members to portal out
            
            foreach (var p in players)
            {
                
                if (p.IsDead)
                {
                    Log.Debug("[] somebody died. casting portal");
                    var portalSkill = SkillBarHud.Skills.FirstOrDefault(s => s.Name == "Portal" && s.IsOnSkillBar);
                    if (portalSkill != null && portalSkill.IsCastable)
                    {
                        await Coroutines.FinishCurrentAction();
                        await Coroutine.Sleep(100);
                        var err = SkillBarHud.Use(portalSkill.Slot, false);
                        await Coroutine.Sleep(500);
                        if (err != UseResult.None)
                        {
                            Log.Error($"[CreateTownPortal] Fail to cast portal skill. Error: \"{err}\".");
                            return true;
                        }

                        await Coroutine.Sleep(1000);
                    }
                    portal = LokiPoe.ObjectManager.Portals.FirstOrDefault();
                    await Coroutines.InteractWith(portal);
                    await Wait.ForAreaChange(hash);
                    SendMsCarry($" Somebody in {LokiPoe.Me.Name} just died. {loses++} loss ");

                    _forceLeaveZone = false;
                    _lootIsUp = false;
                    break;

                }          
            }
            
           


            if (LokiPoe.InGameState.SkillGemHud.AreIconsDisplayed)
            {
                await Coroutines.CloseBlockingWindows();

                LokiPoe.InGameState.SkillGemHud.HandlePendingLevelUps((x, y, z) => true);
                return true;
            }
            if (worldItemCount >= 30 && shouldLoot == false && shouldleave==false)
            {

                _lootIsUp = true;
                lootTimeCounter = DateTime.Now;
                var portalSkill = SkillBarHud.Skills.FirstOrDefault(s => s.Name == "Portal" && s.IsOnSkillBar);
                
                if (portalSkill != null && portalSkill.IsCastable)
                {
                    await Coroutines.FinishCurrentAction();
                    await Coroutine.Sleep(100);
                    var err = SkillBarHud.Use(portalSkill.Slot, false);
                    if (err != UseResult.None)
                    {
                        Log.Error($"[CreateTownPortal] Fail to cast portal skill. Error: \"{err}\".");
                        return true;
                    }

                    await Coroutine.Sleep(1000);
                }
                await Coroutine.Sleep(5000);
                return true;
            }
            

            if (shouldLoot == true && shouldleave == false) // add looting task completed conditions later
            {

               if(lootTimeCounter + TimeSpan.FromMilliseconds(30000)< DateTime.Now) // if the bot is still in 5way for more than 60s after loot dropped, trigger leave map discipline
                {
                    Log.Info("Stuck in 5way too long, leaving zone");
                    //no loot available, can exit map now. makes forceleavezone true.
                    SendMsCarry($"{LokiPoe.Me.Name} Carry is stuck in 5way after loot dropped , will try to leave 5way now. {wins++} Win ");
                    lootTimeCounter = new DateTime();
                    _notifyDiscord = true;
                    _lootIsUp = false;
                    _forceLeaveZone = true;
                    return true;
                }
                // RoutineManager.Current.Stop(); 
                var jewel = LokiPoe.ObjectManager.Objects.FirstOrDefault(x => x.Name == "Timeless Jewel");
                if (jewel != null) lootFilter.Add(jewel );
                var divine = LokiPoe.ObjectManager.Objects.FirstOrDefault(x => x.Name == "Divine Orb");
                if (divine != null) lootFilter.Add(divine  );
                var gold = LokiPoe.ObjectManager.Objects.FirstOrDefault(x => x.Name == "Gold");
                if (gold != null) lootFilter.Add(gold);
                var exa = LokiPoe.ObjectManager.Objects.FirstOrDefault(x => x.Name == "Exalted Orb");
                if (exa != null) lootFilter.Add(exa);       
                var chaos = LokiPoe.ObjectManager.Objects.FirstOrDefault(x => x.Name == "Chaos Orb");
                if (chaos != null) lootFilter.Add(chaos );
                var ornateIncubator = LokiPoe.ObjectManager.Objects.FirstOrDefault(x => x.Name == "Ornate Incubator");
                if (ornateIncubator != null) lootFilter.Add(ornateIncubator);
                var divIncubator = LokiPoe.ObjectManager.Objects.FirstOrDefault(x => x.Name == "Diviner's Incubator");
                if (divIncubator != null) lootFilter.Add(divIncubator);

                var gcp = LokiPoe.ObjectManager.Objects.FirstOrDefault(x => x.Name == "Gemcutter's Prism");
                if (LokiPoe.Me.IsDead)
                {
                    for (int i = 1; i <= 3; ++i)
                    {
                        Log.Debug($"[Resurrect] Attempt: {i}");

                        if (!LokiPoe.IsInGame)
                        {
                            Log.Debug("[Resurrect] Now exiting this logic because we are no longer in game.");
                            return true;
                        }
                        if (!LokiPoe.Me.IsDead)
                        {
                            Log.Debug("[Resurrect] Now exiting this logic because we are no longer dead.");
                            return true;
                        }
                        var _hash = LokiPoe.LocalData.AreaHash;


                        var err = LokiPoe.InGameState.ResurrectPanel.ResurrectToCheckPoint();


                        if (err == LokiPoe.InGameState.ResurrectResult.None)
                        {

                            await Wait.ForAreaChange(_hash);
                            _forceLeaveZone = false;
                            _lootIsUp = false;
                            Log.Debug("[Resurrect] Player has been successfully resurrected.");
                            await Wait.SleepSafe(250);
                            return true;
                        }
                        Log.Error($"[Resurrect] Fail to resurrect. Error: \"{err}\".");
                        await Wait.SleepSafe(1000, 1500);
                    }
                }


                // loot time

                if (divine != null)
                {
                    do
                    {
                        if (LokiPoe.Me.IsDead) return true;
                        Log.Info("Moving toward Loot position");
                        PlayerMoverManager.MoveTowards(divine.Position);
                    } while (divine.Distance > 10);

                    Log.Info($"Debugging LootScanner. looting {divine.Name}");
                    await Coroutines.InteractWith(divine);
                    await Coroutine.Sleep(300);
                    return true;
                }

                
               /* if(jewel != null)
                 {

                     // Log.Info($"Show int i:{i} show lootfilter {lootFilter.Last().Name} ");

                     do
                     {
                         if (LokiPoe.Me.IsDead) return true;
                         Log.Info("Moving toward Loot position");
                         PlayerMoverManager.MoveTowards(jewel.Position);
                     } while (jewel.Distance > 10);
                     Log.Info($"Debugging LootScanner. looting {jewel.Name}");
                     await Coroutines.InteractWith(jewel);
                     await Coroutine.Sleep(300);
                     return true;
                 }*/
                if (ornateIncubator != null)
                {
                    do
                    {
                        if (LokiPoe.Me.IsDead)
                        {
                            for (int i = 1; i <= 3; ++i)
                            {
                                Log.Debug($"[Resurrect] Attempt: {i}");

                                if (!LokiPoe.IsInGame)
                                {
                                    Log.Debug("[Resurrect] Now exiting this logic because we are no longer in game.");
                                    return true;
                                }
                                if (!LokiPoe.Me.IsDead)
                                {
                                    Log.Debug("[Resurrect] Now exiting this logic because we are no longer dead.");
                                    return true;
                                }
                                var _hash = LokiPoe.LocalData.AreaHash;


                                var err = LokiPoe.InGameState.ResurrectPanel.ResurrectToCheckPoint();


                                if (err == LokiPoe.InGameState.ResurrectResult.None)
                                {

                                    await Wait.ForAreaChange(_hash);
                                    _forceLeaveZone = false;
                                    _lootIsUp = false;
                                    Log.Debug("[Resurrect] Player has been successfully resurrected.");
                                    await Wait.SleepSafe(250);
                                    return true;
                                }
                                Log.Error($"[Resurrect] Fail to resurrect. Error: \"{err}\".");
                                await Wait.SleepSafe(1000, 1500);
                            }
                        }
                        Log.Info("Moving toward Loot position");
                        PlayerMoverManager.MoveTowards(ornateIncubator.Position);
                    } while (ornateIncubator.Distance > 10);
                    await Coroutines.InteractWith(ornateIncubator);
                    await Coroutine.Sleep(100);
                    return true;
                }
                if (divIncubator != null)
                {
                    do
                    {
                        if (LokiPoe.Me.IsDead)
                        {
                            for (int i = 1; i <= 3; ++i)
                            {
                                Log.Debug($"[Resurrect] Attempt: {i}");

                                if (!LokiPoe.IsInGame)
                                {
                                    Log.Debug("[Resurrect] Now exiting this logic because we are no longer in game.");
                                    return true;
                                }
                                if (!LokiPoe.Me.IsDead)
                                {
                                    Log.Debug("[Resurrect] Now exiting this logic because we are no longer dead.");
                                    return true;
                                }
                                var _hash = LokiPoe.LocalData.AreaHash;


                                var err = LokiPoe.InGameState.ResurrectPanel.ResurrectToCheckPoint();


                                if (err == LokiPoe.InGameState.ResurrectResult.None)
                                {

                                    await Wait.ForAreaChange(_hash);
                                    _forceLeaveZone = false;
                                    _lootIsUp = false;
                                    Log.Debug("[Resurrect] Player has been successfully resurrected.");
                                    await Wait.SleepSafe(250);
                                    return true;
                                }
                                Log.Error($"[Resurrect] Fail to resurrect. Error: \"{err}\".");
                                await Wait.SleepSafe(1000, 1500);
                            }
                        }
                        Log.Info("Moving toward Loot position");
                        PlayerMoverManager.MoveTowards(divIncubator.Position);
                    } while (divIncubator.Distance > 10);
                    await Coroutines.InteractWith(divIncubator);
                    await Coroutine.Sleep(100);
                    return true;
                }

                Log.Info("No more Loot available. Leaving Zone");
                //no loot available, can exit map now. makes forceleavezone true.
                SendMsCarry($"{LokiPoe.Me.Name} Carry 5way Run completed. {wins++} Win ");
                _notifyDiscord = true;
                _lootIsUp = false;
                _forceLeaveZone = true;
                return true;
                //no loot available, can exit map now. makes forceleavezone true.
              
           
            }
            if (shouldleave == false)
            {
                

                if (areaName != "Domain of Timeless Conflict")
                    return false;
                //
                if (!VisibleTimersUi.IsOpened)
                {
                    
                    Log.Info("5Way is not Activated, moving into inside pos to activate 5way.");
                    
                    
                    PlayerMoverManager.MoveTowards(insidePos);

                    return true;
                }
                stuckTimeCounter = DateTime.Now;
                var targetPos = new Vector2i(
                    ResetterSettings.Instance.CarryDefaultX,
                    ResetterSettings.Instance.CarryDefaultY);


                var distance = targetPos.Distance(LokiPoe.MyPosition);

                if (targetPos.Distance(LokiPoe.MyPosition) < ResetterSettings.Instance.MaxCarryPositionDistance  )
                    return false;
                else if(targetPos.Distance(LokiPoe.MyPosition) >= ResetterSettings.Instance.MaxCarryPositionDistance || insidePos.Distance(LokiPoe.MyPosition) < 15)
                {
                    Log.Info($"[Resetter] Moving towards carry position. Distance: {distance}");
                    // PlayerMoverManager.MoveTowards(targetPos); 
                    //test new moving stuff
                    var move = SkillBarHud.LastBoundMoveSkill;
                    //Log.Debug("[PerformDashReset] Unable to path to the inside position, using move skill.");
                    SkillBarHud.UseAt(move.Slots.Last(), false, targetPos);
                    LokiPoe.ProcessHookManager.ClearAllKeyStates();
                    return true;
                }
                 
            }

            //////////////////////////////////////////////////////////
            else if (shouldleave == true)
            { 
            Log.Info(" Leaving Zone Part 0");
            if (!LokiPoe.IsInGame)
                return false;


            if (areaName != "Domain of Timeless Conflict")
            {
                _forceLeaveZone = false;
                return false;
            }
           // Log.Info("Leave button clicked. Leaving Zone Part 1");
            var shouldLeave = _forceLeaveZone;

            

            if (!shouldLeave)
                return false;
           // Log.Info("Leave button clicked. Leaving Zone Part 3");
           
            var area = LokiPoe.CurrentWorldArea.Name;
            if (portal == null && area == "Domain of Timeless Conflict")
            {
                Log.Info("Could not find portal to exit with.");
                var portalSkill = SkillBarHud.Skills.FirstOrDefault(s => s.Name == "Portal" && s.IsOnSkillBar);
                if (portalSkill != null && portalSkill.IsCastable)
                {
                    await Coroutines.FinishCurrentAction();
                        await Coroutine.Sleep(100);
                        LokiPoe.ProcessHookManager.ClearAllKeyStates();
                        var err = SkillBarHud.Use(portalSkill.Slot, false);
                        await Coroutines.FinishCurrentAction();
                        if (err != UseResult.None)
                    {
                        Log.Error($"[CreateTownPortal] Fail to cast portal skill. Error: \"{err}\".");
                        return true;
                    }

                    await Coroutine.Sleep(1000);
                }
                portal = LokiPoe.ObjectManager.Portals.FirstOrDefault();

            }
           // Log.Info("Leave button clicked. Leaving Zone Part 4");

            if (portal == null)
            {
                Log.Error($"[Resetter] Failed to find or open portal to exit with.");

                return true;
            }


            
            while (area == "Domain of Timeless Conflict")
            {
                if (BotManager.IsStopping)
                    break;

                if (!LokiPoe.IsInGame)
                    break;

                

                if (LokiPoe.LocalData.AreaHash != hash)
                    break;
                    if (stuckTimeCounter + TimeSpan.FromMinutes(7) < DateTime.Now)
                    {
                        SendMsCarry($"{LokiPoe.Me.Name} Carry stayed in 5way for too long for some reason. stopping bot and alert. ");
                        BotManager.Stop();
                    }
                    if (LokiPoe.Me.IsDead)
                    {
                        for (int i = 1; i <= 3; ++i)
                        {
                            Log.Debug($"[Resurrect] Attempt: {i}");

                            if (!LokiPoe.IsInGame)
                            {
                                Log.Debug("[Resurrect] Now exiting this logic because we are no longer in game.");
                                return true;
                            }
                            if (!LokiPoe.Me.IsDead)
                            {
                                Log.Debug("[Resurrect] Now exiting this logic because we are no longer dead.");
                                return true;
                            }
                            var _hash = LokiPoe.LocalData.AreaHash;


                            var err = LokiPoe.InGameState.ResurrectPanel.ResurrectToCheckPoint();


                            if (err == LokiPoe.InGameState.ResurrectResult.None)
                            {

                                await Wait.ForAreaChange(_hash);
                                _forceLeaveZone = false;
                                _lootIsUp = false;
                                Log.Debug("[Resurrect] Player has been successfully resurrected.");
                                SendMsCarry($"{LokiPoe.Me.Name} Carry just died.{loses++} Loss ");
                                await Wait.SleepSafe(250);
                                return true;
                            }
                            Log.Error($"[Resurrect] Fail to resurrect. Error: \"{err}\".");
                            await Wait.SleepSafe(1000, 1500);
                        }
                    }
                    if (portal.Distance > 10)
                {
                   // Log.Info("Leave button clicked. Leaving Zone Part 5");
                    Log.Info("Moving towards exit portal.");
                    PlayerMoverManager.MoveTowards(portal.Position);
                    continue;
                }

                if (await Coroutines.InteractWith(portal))
                {
                        await Wait.ForAreaChange(hash);
                        _forceLeaveZone = false;
                        _lootIsUp = false;
                        break;
                }
            }
            return true;
        }
            return true;

        }
        private static async Task<bool> Resurrect(bool toCheckpoint, int attempts = 3)
        {
            Log.Debug($"[Resurrect] Now going to resurrect to {(toCheckpoint ? "checkpoint" : "town")}.");

            if (!await Wait.For(() => LokiPoe.InGameState.ResurrectPanel.IsOpened, "ResurrectPanel opening"))
                return false;

            await Wait.SleepSafe(100);

            // if (Settings.Instance.ArtificialDelays)
            //    await Wait.ArtificialDelay();

            for (int i = 1; i <= attempts; ++i)
            {
                Log.Debug($"[Resurrect] Attempt: {i}/{attempts}");

                if (!LokiPoe.IsInGame)
                {
                    Log.Debug("[Resurrect] Now exiting this logic because we are no longer in game.");
                    return true;
                }
                if (!LokiPoe.Me.IsDead)
                {
                    Log.Debug("[Resurrect] Now exiting this logic because we are no longer dead.");
                    return true;
                }

                var err = toCheckpoint
                    ? LokiPoe.InGameState.ResurrectPanel.ResurrectToCheckPoint()
                    : LokiPoe.InGameState.ResurrectPanel.ResurrectToTown();

                if (err == LokiPoe.InGameState.ResurrectResult.None)
                {
                    if (!await Wait.For(AliveInGame, "resurrection", 200, 5000))
                        continue;

                    Log.Debug("[Resurrect] Player has been successfully resurrected.");
                    SendMsCarry($"{LokiPoe.Me.Name} Carry just died. ");
                    await Wait.SleepSafe(250);
                    return true;
                }
                Log.Error($"[Resurrect] Fail to resurrect. Error: \"{err}\".");
                await Wait.SleepSafe(1000, 1500);
            }
            Log.Error("[Resurrect] All resurrection attempts have been spent.");
            return false;
        }
        private static async Task<bool> Logout(int attempts = 5)
        {
            for (int i = 1; i <= attempts; ++i)
            {
                Log.Debug($"[Logout] Attempt: {i}/{attempts}");

                if (!LokiPoe.IsInGame)
                {
                    Log.Debug("[Logout] Now exiting this logic because we are no longer in game.");
                    return true;
                }
                if (!LokiPoe.Me.IsDead)
                {
                    Log.Debug("[Logout] Now exiting this logic because we are no longer dead.");
                    return true;
                }

                var err = LokiPoe.EscapeState.LogoutToTitleScreen();
                if (err == LokiPoe.EscapeState.LogoutError.None)
                {
                    if (!await Wait.For(() => LokiPoe.IsInLoginScreen, "log out", 200, 5000))
                        continue;

                    Log.Debug("[Logout] Player has been successfully logged out.");
                    return true;
                }
                Log.Error($"[Logout] Fail to log out. Error: \"{err}\".");
                await Wait.SleepSafe(2000, 3000);
            }
            Log.Error("[Logout] All logout attempts have been spent.");
            return false;
        }
        private static bool AliveInGame()
        {
            if (LokiPoe.IsInLoginScreen)
            {
                Log.Error("[Resurrect] Disconnected while waiting for resurrection.");
                return true;
            }
            return !LokiPoe.Me.IsDead;
        }
        public void Tick()
        {
        }
        static void SendMs(string message)
        {
            string webhook = "https://discord.com/api/webhooks/1273617275173212161/4y1YPqTXS6-bQLv2bxpUD88IUjTlYApe-rJ_r2MlSZhCyrFCoYB-HC9oDh07XhcFSayb";

            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string payload = "{\"content\": \"" + message + "\"}";
            client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
        }

        static void SendMsCarry(string message)
        {
            string webhook = "https://discord.com/api/webhooks/1345295119728902144/qFSDzZ_xfHNX48dzV2v5oMW22prxr-2zdbVHeuiAFFpGT4tk21jNMy4HgJfS2d7gj3yE";

            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string payload = "{\"content\": \"" + message + "\"}";
            client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
        }

    }

}