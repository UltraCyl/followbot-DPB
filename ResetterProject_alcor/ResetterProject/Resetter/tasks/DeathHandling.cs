using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.Objects;
using log4net;
using Resetter.Extensions;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using SkillBar = DreamPoeBot.Loki.Game.LokiPoe.InGameState.SkillBarHud;

namespace Resetter.tasks
{
    public class DeathHandlingTask : ITask
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
        private bool _forceLeaveZone;

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
            _forceLeaveZone = false;
        }

        public void Stop()
        {
        }


        public async Task<bool> Run()
        {
            // Log.Info("Leaving Zone Part 000");

            if (!await Resurrect(true))
            {
                if (!await Resurrect(false))
                {
                    Log.Error("[ResurrectionLogic] Resurrection failed. Now going to logout.");
                    if (!await Logout())
                    {
                        Log.Error("[ResurrectionLogic] Logout failed. Now stopping the bot because it cannot continue.");
                        BotManager.Stop();
                        return true;
                    }
                }
            }
            Log.Info("[Events] Player resurrected.");

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
    }
}
