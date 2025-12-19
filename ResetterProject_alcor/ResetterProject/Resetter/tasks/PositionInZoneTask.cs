using System.Linq;
using System.Threading.Tasks;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using log4net;
using Resetter.Extensions;

using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Bot.Pathfinding;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.RemoteMemoryObjects;
using SkillBar = DreamPoeBot.Loki.Game.LokiPoe.InGameState.SkillBarHud;
using DreamPoeBot.Loki.Game.Objects;

namespace Resetter
{
    public class PositionInZoneTask : ITask
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();

        public string Author => "Allure_";
        public string Description => "Task for party.";
        public string Name => "PositionInZoneTask";
        public string Version => "1.0";


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
            var areaName = LokiPoe.CurrentWorldArea.Name;
            if (areaName != "Domain of Timeless Conflict" && LokiPoe.Me.IsInHideout == false && LokiPoe.Me.IsInTown == false)// leecher is not in 5way, not in hideout and not in town => in others map to suicide
            {
                Log.Debug("We Are Not in 5way, bot will now suicide with closest monster");
                //proceed to follow leader
                var monsters = LokiPoe.ObjectManager.GetObjectsByType<Monster>()
                .Where(d => d.IsAliveHostile)
               .OrderBy(m => m.DistanceSqr);
                var closestMonster = monsters.FirstOrDefault();
                if (closestMonster == null)
                {
                    Log.Error("No alive monsters in object explorer's range");
                }
                else
                {
                    Log.Debug($"Closest monster is: {closestMonster.Name} {closestMonster.Position}({closestMonster.Distance})");
                    PlayerMoverManager.MoveTowards(closestMonster.Position);
                    
                }
                var leader = LokiPoe.InstanceInfo.PartyMembers.FirstOrDefault(x => x.MemberStatus == PartyStatus.PartyLeader);
                // PlayerMoverManager.MoveTowards(leader.PlayerEntry.p);
                return true;
                
            }
            else if(areaName == "Domain of Timeless Conflict")
            {

                var outsidePosition = new Vector2i(
                    ResetterSettings.Instance.OutsideX,
                    ResetterSettings.Instance.OutsideY);

                if (LokiPoe.Me.IsDead || BotManager.IsStopping)
                {
                    Log.Info("Character is dead. Need to resurrect");
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

                            //_lootIsUp = false;
                            Log.Debug("[Resurrect] Player has been successfully resurrected.");
                            await Wait.SleepSafe(250);
                            break;
                        }
                        Log.Error($"[Resurrect] Fail to resurrect. Error: \"{err}\".");
                        await Wait.SleepSafe(1000, 1500);
                        BotManager.Stop();
                        break;
                    }
                };


                    if (outsidePosition.Distance(LokiPoe.MyPosition) >= 50 && LokiPoe.Me.IsDead == false)
                {

                    while (outsidePosition.Distance(LokiPoe.MyPosition) > 10 )
                    {
                        

                           
                        
                        Log.Info("Moving towards outside position.");
                        if (!PlayerMoverManager.MoveTowards(outsidePosition))
                        {
                            break;
                        }
                        
                    }

                   
                }

                return false;
            }
            return false;
        }

        public void Tick()
        {
        }
    }
}