using System;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Documents;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Bot.Pathfinding;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.RemoteMemoryObjects;
using log4net;
using DreamPoeBot.Loki.Game.Objects;
using Resetter.Extensions;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using SkillBar = DreamPoeBot.Loki.Game.LokiPoe.InGameState.SkillBarHud;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Components;
using System.Text;

namespace Resetter
{
    public class ResetTask : ITask
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();

        private DateTime _lastResetAt = DateTime.MinValue;

        public string Author => "Allure_";
        public string Description => "Task for party.";
        public string Name => "ResetTask";
        public string Version => "1.0";

        public int t = 0; // t is how many times that the bot reset since gems leveled up.

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

            var areaName = LokiPoe.CurrentWorldArea.Name;
            if (areaName != "Domain of Timeless Conflict")
                return false;

            if (!VisibleTimersUi.IsOpened)
                return false;
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
                      //  _stuckTimeCounter = DateTime.Now;
                      //  _forceLeaveZone = false;
                      //  _forceJump = false;
                        //_lootIsUp = false;
                        Log.Debug("[Resurrect] Player has been successfully resurrected.");
                        await Wait.SleepSafe(250);
                        return true;
                    }
                    Log.Error($"[Resurrect] Fail to resurrect. Error: \"{err}\".");
                    await Wait.SleepSafe(1000, 1500);
                    BotManager.Stop();
                }
                return false;
            }





            await Reset();
            return true;
        }

        public void Tick()
        {
        }
        private Skill GetFrostBlink()
        {
            return SkillBarHud.SkillBarSkills.FirstOrDefault(x => x != null && x.InternalName == "IceDash");
        }
      

        private Skill GetFlamedash()
        {
            return SkillBarHud.SkillBarSkills.FirstOrDefault(x => x != null && x.InternalName == "FlameDash");
        }

        private Skill GetTemporalRift()
        {
            return SkillBarHud.SkillBarSkills.FirstOrDefault(x => x != null && x.InternalName == "Chronomancer");
        }
        private Skill GetDash()
        {
            return SkillBarHud.SkillBarSkills.FirstOrDefault(x => x != null && x.InternalName == "QuickDodge");
        }
        private Skill GetWhirl()
        {
            return SkillBarHud.SkillBarSkills.FirstOrDefault(x => x != null && x.InternalName == "BladeFlurry");
        }

        private Skill GetCharge()
        {
            return SkillBarHud.SkillBarSkills.FirstOrDefault(x => x != null && x.InternalName == "NewNewShieldCharge");
        }

        private async Task Reset()
        {

          
            var dash = GetDash();
            var whirl = GetWhirl();
            var charge = GetCharge();
            var flameDash = GetFlamedash();
            var rift = GetTemporalRift();
            var frostBlink = GetFrostBlink();
            
            if (frostBlink != null && whirl != null)
            {
                Log.Debug("frost blink detected");
                await PerformFrostBlinkReset(frostBlink, whirl);
                return;
            }
            if(flameDash != null && rift != null)
            {
                Log.Debug("Flame dash + Rift Detected");
                await PerformFlameDashReset(flameDash, rift);
                return;
            }
            if (dash != null && whirl != null)// (dash != null && charge != null)
            {
                 // await PerformDashReset  (dash, whirl);
                await PerformDashResetNoTimer(dash, whirl);
                return;
            }
            if (charge != null && dash != null)// (dash != null && charge != null)
            {
                await PerformChargeReset(dash, charge);
                return;
            }
            if (whirl != null && flameDash!= null && dash==null)
            {
                await PerformFlameDashReset(flameDash, whirl);
                return;
            }


            await PerformLegacyReset();
        }

        private Vector2i GetDashTargetPosition()
        {
            return GetOutsidePosition().GetPointAtDistanceAfterEnd(GetInsidePosition(), 30);
        }

        private Vector2i GetFlameDashTargetPosition()
        {
            
            return GetInsidePosition().GetPointAtDistanceAfterEnd(GetOutsidePosition(), 30);
        }



        private async Task<bool> PerformDashReset(Skill dash, Skill whirl)
        {
            Log.Debug("[PerformDashReset] Performing dash reset");
            var insidePos = GetInsidePosition();

            // t is how many times that the bot reset since gems leveled up.
            if (LokiPoe.MyPosition.Distance(insidePos) >= 40)
            {
                // Budget player mover since we can't trust the settings on the default one not to overshoot and start ping ponging
                var pathFindingcommand = new PathfindingCommand(LokiPoe.MyPosition, insidePos);
                var move = SkillBarHud.LastBoundMoveSkill;
                //Log.Debug("[PerformDashReset] Unable to path to the inside position, using move skill.");
                SkillBarHud.UseAt(move.Slots.Last(), false, insidePos);
                LokiPoe.ProcessHookManager.ClearAllKeyStates();
                return true;
            }


            //if monolith is activated, must start resetting now
           



            Log.Debug("[PerformDashReset] Using Dash");
            SkillBarHud.UseAt(dash.Slot, true, GetDashTargetPosition());
            await Coroutine.Sleep(ResetterSettings.Instance.PostDashMsDelay);
            Log.Debug("[PerformDashReset] Using Whirling Blade");
            SkillBarHud.UseAt(whirl.Slot, true, GetDashTargetPosition());
            _lastResetAt = DateTime.Now;

            //leveling gems

            await Coroutine.Sleep(ResetterSettings.Instance.PostShieldChargeMsDelay);

            //await Coroutine.Sleep(1000 * 19);


            return true;
        }
        private async Task<bool> PerformDashResetNoTimer(Skill dash, Skill whirl)
        {

            Log.Debug("[PerformDashReset] Performing dash reset no timer");
            var insidePos = GetInsidePosition();
            var mono = LokiPoe.ObjectManager.Objects.FirstOrDefault(o => o.Metadata == "Metadata/Terrain/Leagues/Legion/Objects/LegionEndlessInitiator");
            //if (mono == null) { Log.Debug("Cant find monolith"); return false; }
            var obelisk = mono.Components.StateMachineComponent.StageStates.FirstOrDefault(m => m.Name == "obelisk_state" );
            
            if (obelisk.IsActive)
            {
               // Log.Debug("Obelisk is active");
                var resetRing = mono.Components.StateMachineComponent.StageStates.FirstOrDefault(m => m.Name == "checking_control_zone");
                if (resetRing == null) { Log.Debug("Cant find reset ring"); return false; }
                if (resetRing.IsActive)// resetRing active = must stay inside
                {
                    // Log.Debug("resetRing is active");
                    if (LokiPoe.MyPosition.Distance(insidePos) >= 15)
                    {
                        //  Log.Debug("Whirling into the Ring");
                        //reset ring is up, we are too far , now we need to dash in
                        SkillBarHud.UseAt(whirl.Slot, true, GetDashTargetPosition());
                    }
                    if (SkillGemHud.AreIconsDisplayed)
                    {
                        await Coroutines.CloseBlockingWindows();
                        SkillGemHud.HandlePendingLevelUps((x, y, z) => true);
                    }
                    return true;
                }
                else if (!resetRing.IsActive)// resetRing is not active = must dash out of the ring to reset
                {
                   // Log.Debug("resetRing is not active");
                    
                      //  Log.Debug("Dashing out of the ring");
                        SkillBarHud.UseAt(dash.Slot, true, GetDashTargetPosition());
                    
                    return true;
                } 

            }

                // t is how many times that the bot reset since gems leveled up.
                if (LokiPoe.MyPosition.Distance(insidePos) >= 40)
            {
                // Budget player mover since we can't trust the settings on the default one not to overshoot and start ping ponging
                var pathFindingcommand = new PathfindingCommand(LokiPoe.MyPosition, insidePos);
                var move = SkillBarHud.LastBoundMoveSkill;
                //Log.Debug("[PerformDashReset] Unable to path to the inside position, using move skill.");
                SkillBarHud.UseAt(move.Slots.Last(), false, insidePos);
                LokiPoe.ProcessHookManager.ClearAllKeyStates();
                return true;
            }

            




               


            return true;
        }
        private async Task<bool> PerformChargeReset(Skill dash, Skill charge)
        {
            Log.Debug("[PerformDashReset] Performing dash reset");
            var insidePos = GetInsidePosition();

            // t is how many times that the bot reset since gems leveled up.
            if (LokiPoe.MyPosition.Distance(insidePos) >= 40)
            {
                // Budget player mover since we can't trust the settings on the default one not to overshoot and start ping ponging
                var pathFindingcommand = new PathfindingCommand(LokiPoe.MyPosition, insidePos);
                var move = SkillBarHud.LastBoundMoveSkill;
                //Log.Debug("[PerformDashReset] Unable to path to the inside position, using move skill.");
                SkillBarHud.UseAt(move.Slots.Last(), false, insidePos);
                LokiPoe.ProcessHookManager.ClearAllKeyStates();
                return true;
            }






            Log.Debug("[PerformDashReset] Using Dash");
            SkillBarHud.UseAt(dash.Slot, true, GetDashTargetPosition());
            await Coroutine.Sleep(ResetterSettings.Instance.PostDashMsDelay);
            Log.Debug("[PerformDashReset] Using Shield Charge");
            SkillBarHud.UseAt(charge.Slot, true, GetDashTargetPosition());
            _lastResetAt = DateTime.Now;

            //leveling gems

            await Coroutine.Sleep(ResetterSettings.Instance.PostShieldChargeMsDelay);

            //await Coroutine.Sleep(1000 * 19);


            return true;
        }
        private async Task<bool> PerformFlameDashReset(Skill flameDash, Skill rift)
        {

            var insidePos = GetInsidePosition();
            var outsidePos = GetOutsidePosition();

            // t is how many times that the bot reset since gems leveled up.
            if (LokiPoe.MyPosition.Distance(insidePos) >= 15)
            {
                //flamedash into insidePos if we are out of the pos

                SkillBarHud.UseAt(flameDash.Slot, true, GetDashTargetPosition());
                // move into insidePos if we are out of the ring
                // Budget player mover since we can't trust the settings on the default one not to overshoot and start ping ponging
                
                /*var pathFindingcommand = new PathfindingCommand(LokiPoe.MyPosition, insidePos);
                var move = SkillBarHud.LastBoundMoveSkill;
                //Log.Debug("[PerformDashReset] Unable to path to the inside position, using move skill.");
                SkillBarHud.UseAt(move.Slots.Last(), false, insidePos);*/
                await Coroutine.Sleep(4500);
                LokiPoe.ProcessHookManager.ClearAllKeyStates();
                return true;
            }
            if(LokiPoe.MyPosition.Distance(insidePos) <= 10 )
            {
                
                Log.Debug("[PerformFlameDashReset] Using flame Dash");
                SkillBarHud.UseAt(flameDash.Slot, true, GetFlameDashTargetPosition());
                await Coroutine.Sleep(ResetterSettings.Instance.PostDashMsDelay);
                Log.Debug("[PerformFlameDashReset] Using Rift");
                SkillBarHud.UseAt(rift.Slot, true, GetFlameDashTargetPosition());
                await Coroutine.Sleep(ResetterSettings.Instance.PostShieldChargeMsDelay);

                return true;
            }


            
        
           
            //_lastResetAt = DateTime.Now;

            //leveling gems

         

            //await Coroutine.Sleep(1000 * 19);


            return true;
        }
        private async Task<bool> PerformFrostBlinkReset(Skill frostBlink, Skill whirl)
        {
            Log.Debug("[PerformDashReset] Performing forstblink reset");
            var insidePos = GetInsidePosition();

            // t is how many times that the bot reset since gems leveled up.
            if (LokiPoe.MyPosition.Distance(insidePos) >= 40)
            {
                // Budget player mover since we can't trust the settings on the default one not to overshoot and start ping ponging
                var pathFindingcommand = new PathfindingCommand(LokiPoe.MyPosition, insidePos);
                var move = SkillBarHud.LastBoundMoveSkill;
                //Log.Debug("[PerformDashReset] Unable to path to the inside position, using move skill.");
                SkillBarHud.UseAt(move.Slots.Last(), false, insidePos);
                LokiPoe.ProcessHookManager.ClearAllKeyStates();
                return true;
            }
           


            Log.Debug("[PerformDashReset] Using Whirling Blade");
            SkillBarHud.UseAt(whirl.Slot, true, GetDashTargetPosition());
            await Coroutine.Sleep(ResetterSettings.Instance.PostShieldChargeMsDelay);

            Log.Debug("[PerformDashReset] Using Frost Blink");
            SkillBarHud.UseAt(frostBlink.Slot, true, GetDashTargetPosition());
            await Coroutine.Sleep(ResetterSettings.Instance.PostDashMsDelay);


            //_lastResetAt = DateTime.Now;

            //leveling gems



            //await Coroutine.Sleep(1000 * 19);


            return true;
        }
        private Vector2i GetInsidePosition()
        {
            return new Vector2i(
                ResetterSettings.Instance.InsideX,
                ResetterSettings.Instance.InsideY);
        }

        private Vector2i GetOutsidePosition()
        {
            return new Vector2i(
                ResetterSettings.Instance.OutsideX,
                ResetterSettings.Instance.OutsideY);
        }

        private async Task<bool> PerformLegacyReset()
        {
            // Log.Debug("[LegacyReset] Performing legacy reset");
            if (SkillGemHud.AreIconsDisplayed)
            {
                await Coroutines.CloseBlockingWindows();
                SkillGemHud.HandlePendingLevelUps((x, y, z) => true);
            }

            if (_lastResetAt + TimeSpan.FromMilliseconds(ResetterSettings.Instance.ResetIntervalMilliSeconds) >
                DateTime.Now)
            {
             //   Log.Debug("[LegacyReset] Reset interval not reached yet, checking for gems");
                await LevelSingleGem();
                return true;
            }


            while (GetOutsidePosition().Distance(LokiPoe.MyPosition) > 5)
            {
                if (BotManager.IsStopping)
                    break;
                PlayerMoverManager.MoveTowards(GetOutsidePosition());
                await Coroutine.Sleep(50);
            }


            while (GetInsidePosition().Distance(LokiPoe.MyPosition) > 5)
            {
                if (BotManager.IsStopping)
                    break;
                PlayerMoverManager.MoveTowards(GetInsidePosition());
                await Coroutine.Sleep(50);
            }

            _lastResetAt = DateTime.Now;
            return true;
        }

        private async Task LevelSingleGem()
        {
            // Level gems if enabled
            
                if (SkillGemHud.AreIconsDisplayed)
                {
                    await Coroutines.CloseBlockingWindows();
                    SkillGemHud.HandlePendingLevelUps((x, y, z) => true);
                }
        }

        static void SendMs(string message)
        {
            string webhook = "https://discord.com/api/webhooks/1273617275173212161/4y1YPqTXS6-bQLv2bxpUD88IUjTlYApe-rJ_r2MlSZhCyrFCoYB-HC9oDh07XhcFSayb";

            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string payload = "{\"content\": \"" + message + "\"}";
            client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
        }
    }
}