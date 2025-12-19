using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.Loki.Game;

namespace Resetter.Extensions 
{
    public static class Wait
    {

        public static async Task<bool> For(Func<Task<bool>> condition, string desc, int step = 100, int timeout = 3000)
        {
            return await For(condition, desc, () => step, timeout);
        }

        public static async Task<bool> For(Func<bool> condition, string desc, int step = 100, int timeout = 3000)
        {
            return await For(async () => condition(), desc, () => step, timeout);
        }

        public static async Task<bool> For(Func<Task<bool>> condition, string desc, Func<int> step_ms,
            int timeout = 3000)
        {
            if (await condition())
                return true;

            var timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < timeout)
            {
                if (await condition())
                    return true;
            }

            return false;
        }

        public static async Task Sleep(int ms)
        {
            var timeout = Math.Max(LatencyTracker.Current, ms);
            await Coroutine.Sleep(timeout);
        }

        public static Task FinishCurrentAction(string initiator, bool resetInput = true)
        {
            if (resetInput)
                LokiPoe.ProcessHookManager.ClearAllKeyStates();
            return For(() => !LokiPoe.Me.HasCurrentAction, $"FinishCurrentAction for: {initiator}");
        }

        public static async Task SleepSafe(int ms)
        {
            var timeout = Math.Max(LatencyTracker.Current, ms);
            await Coroutine.Sleep(timeout);
        }

        public static async Task SleepSafe(int min, int max)
        {
            var latency = LatencyTracker.Current;
            if (latency > max)
            {
                await Coroutine.Sleep(latency);
            }
            else
            {
                await Coroutine.Sleep(LokiPoe.Random.Next(min, max + 1));
            }
        }


        public static async Task LatencySleep()
        {
            var timeout = Math.Max((int) (LatencyTracker.Current * 1.15), 25);
            await Coroutine.Sleep(timeout);
        }

        public static async Task<bool> ForAreaChange(uint areaHash, int timeout = 60000)
        {
            if (await For(() => LokiPoe.StateManager.IsAreaLoadingStateActive, "loading screen"))
            {
                return await For(() => LokiPoe.IsInGame, "is ingame", 200, timeout);
            }

            return false;
            //return await For(() => ExilePather.AreaHash != areaHash, "area change", 500, timeout);
        }

        public static async Task<bool> ForHOChange(int timeout = 60000)
        {
            if (await For(() => LokiPoe.StateManager.IsAreaLoadingStateActive, "loading screen"))
            {
                return await For(() => LokiPoe.IsInGame, "is ingame", 200, timeout);
            }

            return false;
        }
    }
}