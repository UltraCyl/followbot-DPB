using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DreamPoeBot.BotFramework;
using DreamPoeBot.Common;
using DreamPoeBot.Framework;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Bot.Pathfinding;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using log4net;
using Resetter;
using DreamPoeBot.Loki.Game.Objects;
using Resetter.Extensions;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using SkillBar = DreamPoeBot.Loki.Game.LokiPoe.InGameState.SkillBarHud;


namespace CarryRoutine 
{

    public class CarryRoutine : IRoutine
    {
        // List of Routine specific Members and Methods:
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        private Dictionary<string, Func<Tuple<object, string>[], object>> _exposedSettings;
        private CarryRoutineGui _gui;


        public JsonSettings Settings => RoutineSettings.Instance;
        public UserControl Control => _gui ?? (_gui = new CarryRoutineGui());
        public string Author => "Allure_";
        public string Description => "Routine for Timeless Conflict Carry";
        public string Name => "Timeless Conflict Carry Routine";
        public string Version => "0.9";
        public bool _forceLeave;
        public void Deinitialize()
        {
        }

        public void Initialize()
        {
            // The Initialization function is called during the Bot loading stage, as soon all components are been integrated in the bot. here we execute basic component initializations.

        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            if (logic.Id != "hook_combat")
                return LogicResult.Unprovided;
            /*
            var skillBarSkills = LokiPoe.Me.SkillBarSkills.Where(x => x != null).ToList();
            var primarySkill = skillBarSkills.FirstOrDefault(x => x.Slot == RoutineSettings.Instance.FallBackSkillSlot);
            if (primarySkill == null)
            {
                Log.Warn($"Skill in slot {RoutineSettings.Instance.FallBackSkillSlot} not found, cannot attack!");
                return LogicResult.Unprovided;
            }
            */
            var summonFlameGolem = SkillBarHud.SkillBarSkills.FirstOrDefault(x => x != null && x.InternalName == "SummonFireElemental");
            var prideAura = SkillBarHud.SkillBarSkills.FirstOrDefault(x => x != null && x.InternalName == "CastAuraPhysicalDamage");
            var heraldOfAsh = SkillBarHud.SkillBarSkills.FirstOrDefault(x => x != null && x.InternalName == "HeraldOfAsh");
            var heraldOfPurity = SkillBarHud.SkillBarSkills.FirstOrDefault(x => x != null && x.InternalName == "HeraldOfPurity");

            if (LokiPoe.Me.HasBuff("Flame Golem") != true && summonFlameGolem != null && summonFlameGolem.IsCastable)
            {
                await Coroutines.FinishCurrentAction();

                LokiPoe.Input.SimulateKeyEvent(summonFlameGolem.BoundKey, true, false, false);
                await Coroutines.FinishCurrentAction();

            }
            if (LokiPoe.Me.HasBuff("Pride") != true && prideAura != null && prideAura.IsCastable)
            {
                await Coroutines.FinishCurrentAction();

                LokiPoe.Input.SimulateKeyEvent(prideAura.BoundKey, true, false, false);
                await Coroutines.FinishCurrentAction();

            }
            if (LokiPoe.Me.HasBuff("Herald Of Ash") != true && heraldOfAsh != null && heraldOfAsh.IsCastable)
            {
                await Coroutines.FinishCurrentAction();

                LokiPoe.Input.SimulateKeyEvent(heraldOfAsh.BoundKey, true, false, false);
                await Coroutines.FinishCurrentAction();

            }
            if (LokiPoe.Me.HasBuff("Herald Of Purity") != true && heraldOfPurity != null && heraldOfPurity.IsCastable)
            {
                await Coroutines.FinishCurrentAction();

                LokiPoe.Input.SimulateKeyEvent(heraldOfPurity.BoundKey, true, false, false);
                await Coroutines.FinishCurrentAction();

            }
            var monolithState = LokiPoe.ObjectManager.Objects.FirstOrDefault(o => o.Metadata == "Metadata/Terrain/Leagues/Legion/Objects/LegionEndlessInitiator")
                .Components.StateMachineComponent.StageStates.FirstOrDefault(m => m.Name == "obelisk_state");
            var skillBarSkills = LokiPoe.Me.SkillBarSkills.Where(x => x != null).ToList();
            var primarySkill = skillBarSkills.FirstOrDefault(x => x.Slot == RoutineSettings.Instance.FallBackSkillSlot);
            //rimarySkill.BoundKey;

           
            
            SkillBarHud.BeginUse(RoutineSettings.Instance.FallBackSkillSlot, true);

            var monsters = LokiPoe.ObjectManager.GetObjectsByType<Monster>()
                .Where(d => d.Rarity.ToString() == "Unique")
               .OrderBy(m => m.DistanceSqr);
            var closestMonster = monsters.FirstOrDefault(m => LokiPoe.Me.Position.Distance(m.Position) <100);
            var carryPos = new Vector2i(ResetterSettings.Instance.CarryDefaultX, ResetterSettings.Instance.CarryDefaultY);
           
           if(LokiPoe.Me.Position.Distance(carryPos)<=15 &&closestMonster!=null && monolithState.IsActive)
           {
               //Log.Info($"Moving Mouse cursor to Monster :{closestMonster.Name.ToString()} ");
              // MouseManager.SetMousePosition(closestMonster.Position);
                
            }
            

            //SkillBar.BeginUseAt(RoutineSettings.Instance.FallBackSkillSlot, true, LokiPoe.MyPosition);
            //hold down primary skill
            // LokiPoe.Input.SimulateKeyEvent(primarySkill.BoundKey, true, false, false);

            return LogicResult.Provided;
        }

        public MessageResult Message(Message message)
        {
           
            return MessageResult.Unprocessed;
        }

        public void Start()
        {
            if (RoutineSettings.Instance.FallBackSkillSlot == -1)
            {
                Log.ErrorFormat(
                    "[Start] Please configure the routine settings before starting!");
                BotManager.Stop();
            }
            var skillBarSkills = LokiPoe.Me.SkillBarSkills.Where(x => x != null).ToList();
            var primarySkill = skillBarSkills.FirstOrDefault(x => x.Slot == RoutineSettings.Instance.FallBackSkillSlot);
            if (primarySkill == null)
            {
                Log.ErrorFormat(
                    "[Start] Skill in slot {0} not found, cannot attack!", RoutineSettings.Instance.FallBackSkillSlot);
                BotManager.Stop();
            }
        }

        public void Stop()
        {
        }

        public void Tick()
        {

        }

        public override string ToString()
        {
            return Name;
        }

    }
}