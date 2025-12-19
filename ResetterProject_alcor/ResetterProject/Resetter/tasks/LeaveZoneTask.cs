using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml.Linq;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Components;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.Objects;
using log4net;
using Resetter.Extensions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using SkillBar = DreamPoeBot.Loki.Game.LokiPoe.InGameState.SkillBarHud;

namespace Resetter
{
    public class LeaveZoneTask : ITask
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
        private bool _forceLeaveZone;
        private bool _lootIsUp;
        private bool _forceJump;
        private bool _notifyDiscord;
        private DateTime _stuckTimeCounter;
        public string Author => "Allure_";
        public string Description => "Task for party.";
        public string Name => "LeaveZoneTask";
        public string Version => "1.0";
        

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == Messages.LEAVE_ZONE)
            {
                Log.Info("Start leaving zone. Part 00");
                _forceLeaveZone = true;
                return MessageResult.Processed;
            }

            return MessageResult.Unprocessed;
        }


        public void Start()
        {
            _forceLeaveZone = false;
            _forceJump = false;
            _notifyDiscord = false;
        }

        public void Stop()
        {
        }


        public async Task<bool> Run()
        {
            if (BotManager.IsStopping)
            {
               // SendMsLeecher($"{LokiPoe.Me.Name} Leecher bot is stopped. Please check!");
            }
            if (!LokiPoe.IsInGame)
            {
                _lootIsUp = false;
                _forceLeaveZone = false;
                _forceJump = false;
               
                _notifyDiscord = false;
                return false;
            }
               
            if (LokiPoe.Me.IsDead)
            {
                for (int i = 1; i <= 3; ++i)
                {
                    Log.Debug($"[Resurrect] Attempt [leave]: {i}");

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
                    var currentHash = LokiPoe.LocalData.AreaHash;


                    var err = LokiPoe.InGameState.ResurrectPanel.ResurrectToCheckPoint();


                    if (err == LokiPoe.InGameState.ResurrectResult.None)
                    {

                        await Wait.ForAreaChange(currentHash);
                        _stuckTimeCounter = DateTime.Now;
                        _forceLeaveZone = false;
                        _forceJump = false;
                        //_lootIsUp = false;
                        Log.Debug("[Resurrect] Player has been successfully resurrected.");
                        await Wait.SleepSafe(250);
                        return true;
                    }
                    Log.Error($"[Resurrect] Fail to resurrect. Error: \"{err}\".");
                    await Wait.SleepSafe(1000, 1500);
                    BotManager.Stop();
                }
            }

            var discordIsUp = _notifyDiscord;
            //Log.Error($"debugging Leave Zone. 0");
            var shouldLeave = _forceLeaveZone;
            var leader = LokiPoe.InstanceInfo.PartyMembers.FirstOrDefault(x => x.MemberStatus == PartyStatus.PartyLeader);
            var players = LokiPoe.ObjectManager.GetObjectsByType<DreamPoeBot.Loki.Game.Objects.Player>();
            var party = LokiPoe.InstanceInfo.PartyMembers.FirstOrDefault();

            if (leader == null) return false;
            var leaderPlayerEntry = leader.PlayerEntry;
            if (leaderPlayerEntry == null) return false;
            if (leaderPlayerEntry?.IsOnline != true && discordIsUp ==false)
            {

                SendMs($"Carry {leader.PlayerEntry.Name} is offline");
                _notifyDiscord = true;
                return false;
            }
            var currentZone = LokiPoe.CurrentWorldArea;
            var leadername = leaderPlayerEntry?.Name;
            Log.Warn($"Leader is {leadername} ");
            var leaderArea = leaderPlayerEntry?.Area;
            var curZone = LokiPoe.CurrentWorldArea;
            var _hash = LokiPoe.LocalData.AreaHash;
            var loot = _lootIsUp;
            var shouldJump = _forceJump;
            // if (!curZone.IsTown && !curZone.IsHideoutArea) return false;

            if (VisibleTimersUi.IsOpened && shouldJump == false)
            {
                //Log.Warn($"debugging jump. show should jump : {shouldJump.ToString()}");
                _forceJump = true;
                return true;
            }
          //  Log.Warn($"debugging before jump. show should jump : {shouldJump.ToString()}");

            // Log.Warn($"Leader is {pCount} ");
           // Log.Warn($"Leader is {leadername} ");
           if(!LokiPoe.InGameState.PartyHud.IsInSameZone(leadername) && (leaderArea.IsHideoutArea||leaderArea.Name=="Domain of Timeless Conflict") && currentZone.IsTown)
            {
                // if we are in town and leader is in hideout, jump to leader
                Log.Warn("We are in town, teleporting to leader HO");
                await Coroutine.Sleep(1000);
                await Coroutines.CloseBlockingWindows();
                await Coroutines.LatencyWait();

                var ret = LokiPoe.InGameState.PartyHud.FastGoToZone(leadername);
                
                await Wait.ForAreaChange(_hash);
                await Coroutines.LatencyWait();
                await Coroutines.ReactionWait();
                _lootIsUp = false;
                _forceLeaveZone = false;
                _forceJump = false;
               
                return true;

            }
            if (!LokiPoe.InGameState.PartyHud.IsInSameZone(leadername) && shouldJump == true && _hash == LokiPoe.LocalData.AreaHash)
            {

                // no loot up yet, leave if somebody die
                   
            
                //Log.Warn($"Leader is {leadername} ");
                
                    Log.Warn("Leader Left Area, waiting 3s then jump into Leader");
                    await Coroutine.Sleep(3000);
                    var newLeaderArea = leaderPlayerEntry?.Area;
                    var newHash = LokiPoe.LocalData.AreaHash;
                if (newLeaderArea.IsHideoutArea && _hash == newHash) // leader is in hideout, portal out   
                    {
                        await Coroutines.CloseBlockingWindows();
                        await Coroutines.LatencyWait();

                        var ret = LokiPoe.InGameState.PartyHud.FastGoToZone(leadername);
                        await Coroutines.LatencyWait();
                        await Coroutines.ReactionWait();
                    //Log.Warn($"debugging jump. show should jump : {shouldJump.ToString()}");
                    _lootIsUp = false;
                    _forceLeaveZone = false;
                    _forceJump = false;
                    shouldJump = false;
                    _notifyDiscord = false;
                    //Log.Warn($"debugging after jump. show should jump : {shouldJump.ToString()}");
                    await Wait.ForAreaChange(_hash);
                    
                    if (ret != LokiPoe.InGameState.FastGoToZoneResult.None)
                        {
                            Log.Error($"[FastGotoPartyZone] Returned Error: {ret}");
                            return false;
                        }
                    return true;
                   /* while (LokiPoe.InGameState.GlobalWarningDialog.IsOpened)
                        {
                        Log.Error("debugjump_start");
                        if (LokiPoe.InGameState.GlobalWarningDialog.ConfirmDialog())
                            {
                            _lootIsUp = false;
                            _forceLeaveZone = false;
                            _forceJump = false;
                            await Wait.ForAreaChange(_hash);
                            Log.Warn($"debugging jump. show should jump : {shouldJump.ToString()}");
                            Log.Error("debugjump_finished");
                                break;
                            
                            
                            }
                 
                        }*/
                   // return true;


                }
               
                
            }
            



            var areaName = LokiPoe.CurrentWorldArea.Name;
            if (areaName != "Domain of Timeless Conflict")
            {
                _forceLeaveZone = false;
                return false;
            }
            if (LokiPoe.Me.IsDead)
            {
                for (int i = 1; i <= 3; ++i)
                {
                    Log.Debug($"[Resurrect] Attempt [leave]: {i}");

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
                    var currentHash = LokiPoe.LocalData.AreaHash;


                    var err = LokiPoe.InGameState.ResurrectPanel.ResurrectToCheckPoint();


                    if (err == LokiPoe.InGameState.ResurrectResult.None)
                    {

                        await Wait.ForAreaChange(currentHash);
                        _stuckTimeCounter = DateTime.Now;
                        _forceLeaveZone = false;
                        _forceJump = false;
                        //_lootIsUp = false;
                        Log.Debug("[Resurrect] Player has been successfully resurrected.");
                        await Wait.SleepSafe(250);
                        return true;
                    }
                    Log.Error($"[Resurrect] Fail to resurrect. Error: \"{err}\".");
                    await Wait.SleepSafe(1000, 1500);
                    BotManager.Stop();
                }
            }
            


           // Log.Error($"debugging Leave Zone.1");

            if (ResetterSettings.Instance.EnableAutoLeaveZone &&shouldLeave==false)
            {
                var worldItemCount = LokiPoe.ObjectManager.GetObjectsByType<DreamPoeBot.Loki.Game.Objects.WorldItem>().Count();
                if (worldItemCount >= 30)
                {
                    Log.Info($"Detected {worldItemCount} items on the ground run is over, leaving zone after 1 second wait.");
                    _forceLeaveZone = true;
                    _lootIsUp = true;
                    await Coroutine.Sleep(7_000);
                    return true;
                }
                
            }
            

            if (!shouldLeave)
                return false;

            var portal = LokiPoe.ObjectManager.Portals.FirstOrDefault();
            if (portal == null)
            {
                Log.Info("Could not find portal to exit with.");
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

                    await Coroutine.Sleep(5000);
                }
                portal = LokiPoe.ObjectManager.Portals.FirstOrDefault();

            }


            if (portal == null)
            {
                Log.Error($"[Resetter] Failed to find or open portal to exit with.");
                return true;
            }
            Log.Error($"debugging Leave Zone.2");
            if (LokiPoe.ConfigManager.IsAlwaysHighlightEnabled) LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.highlight_toggle, true, false, false);
            var hash = LokiPoe.LocalData.AreaHash;

            while (shouldLeave==true)
            {
                Log.Error($"debugging Leave Zone.3");
                await Coroutine.Sleep(16);
                if (BotManager.IsStopping)
                    break;
                
                if (!LokiPoe.IsInGame)
                    break;
                if (LokiPoe.Me.IsDead)
                {
                    for (int i = 1; i <= 3; ++i)
                    {
                        Log.Debug($"[Resurrect] Attempt [leave]: {i}");

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
                        var currentHash = LokiPoe.LocalData.AreaHash;


                        var err = LokiPoe.InGameState.ResurrectPanel.ResurrectToCheckPoint();


                        if (err == LokiPoe.InGameState.ResurrectResult.None)
                        {

                            await Wait.ForAreaChange(currentHash);
                            _forceLeaveZone = false;
                            _forceJump = false;
                            _notifyDiscord = false;
                            //_lootIsUp = false;
                            Log.Debug("[Resurrect] Player has been successfully resurrected.");
                            await Wait.SleepSafe(250);
                            return true;
                        }
                        Log.Error($"[Resurrect] Fail to resurrect. Error: \"{err}\".");
                        await Wait.SleepSafe(1000, 1500);
                        BotManager.Stop();
                    }
                }
                if (LokiPoe.LocalData.AreaHash != hash)
                    break;
                
                if (portal.Distance > 10)
                {
                    Log.Info("Moving towards exit portal.");
                    PlayerMoverManager.MoveTowards(portal.Position);
                    continue;
                }

                if (await Coroutines.InteractWith(portal))
                {
                    _forceJump = false;
                    _forceLeaveZone = false;
                    _notifyDiscord = false;
                    if (await Wait.ForAreaChange(hash))
                        break;
                }
                else
                {
                    _forceJump = true;
                    _forceLeaveZone = true;
                    return false;
                }
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
        static void SendMs(string message)
        {
            string webhook = "https://discord.com/api/webhooks/1273617275173212161/4y1YPqTXS6-bQLv2bxpUD88IUjTlYApe-rJ_r2MlSZhCyrFCoYB-HC9oDh07XhcFSayb";

            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string payload = "{\"content\": \"" + message + "\"}";
            client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
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
        static void SendMsLeecher(string message)
        {
            string webhook = "https://discord.com/api/webhooks/1386569709524222012/4GhlZc5AvOehCkx6tyFVUvd6u0bvsaD3poyqlJtSGYn4wQvReFIDmD8S787V3JhULg7q";

            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string payload = "{\"content\": \"" + message + "\"}";
            client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
        }
    }
}