using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Bot.Pathfinding;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Controllers;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.Loki.Game;
using log4net;
using Resetter.tasks;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace Resetter 
{
    public class  Resetter : IBot,  IUrlProvider
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        private Gui _gui;
        private Coroutine _coroutine;
        private int _lastKnownAreaId = 0;

        private readonly TaskManager _taskManager = new TaskManager();


        public void Start()
        {
            if (BotManager.IsStopping)
            {
                SendMs("Resetter bot is stopping");
                return;

            }
            

            if (!LokiPoe.IsInGame)
            {
                MessageBox.Show("Please get into your hideout before starting the resetter.");
                BotManager.Stop();
                return;
            }

            var firstMoveSkill = LokiPoe.InGameState.SkillBarHud.Skills.FirstOrDefault(x => x?.Name == "Move" && x.IsOnSkillBar);
            if (firstMoveSkill == null)
            {
                MessageBox.Show("Move must be bound to a key and on the skill bar, IT CANNOT BE BOUND TO A MOUSE BUTTON.");
                BotManager.Stop();
                return;
            }

            if (firstMoveSkill.Slot <= 3)
            {
                MessageBox.Show("Move cannot be bound to a mouse button, please rebind it to another key.");
                BotManager.Stop();
                return;
                
            }

            // Cache all bound keys.
            LokiPoe.Input.Binding.Update();

            BotManager.MsBetweenTicks = 30;
            LokiPoe.Input.InputEventDelayMs = 30;


            // Since this bot will be performing client actions, we need to enable the process hook manager.
            LokiPoe.ProcessHookManager.Enable();

            _coroutine = null;

            ExilePather.Reload();

            _taskManager.Reset();

            AddTasks();

            PluginManager.Start();
            RoutineManager.Start();
            PlayerMoverManager.Start();
            _taskManager.Start();

        }

     

        public void Tick()
        {
         
            if (_coroutine == null)
            {
                _coroutine = new Coroutine(() => MainCoroutine());
            }

            if (LokiPoe.IsInGame)
            {
                if ( _lastKnownAreaId != LokiPoe.CurrentWorldArea.WorldAreaId)
                {
                    _lastKnownAreaId = LokiPoe.CurrentWorldArea.WorldAreaId;
                    ExilePather.Reload();
                }
            }
            _taskManager.Tick();
            PluginManager.Tick();
            RoutineManager.Tick();
            PlayerMoverManager.Tick();

            // Check to see if the coroutine is finished. If it is, stop the bot.
            if (_coroutine.IsFinished)
            {
                Log.Debug($"The bot coroutine has finished in a state of {_coroutine.Status}");
                BotManager.Stop();
                return;
            }

            try
            {
                _coroutine.Resume();
            }
            catch
            {
                var c = _coroutine;
                _coroutine = null;
                c.Dispose();
                throw;
            }
        }

        public void Stop()
        {
            _taskManager.Stop();
            PluginManager.Stop();
            RoutineManager.Stop();
            PlayerMoverManager.Stop();

            // When the bot is stopped, we want to remove the process hook manager.
            LokiPoe.ProcessHookManager.Disable();

            // Cleanup the coroutine.
            if (_coroutine != null)
            {
                _coroutine.Dispose();
                _coroutine = null;
            }
        }

        private async Task MainCoroutine()
        {
            while (true)
            {

                if (LokiPoe.IsInLoginScreen)
                {
                    // Offload auto login logic to a plugin.
                    var logic = new Logic("hook_login_screen", this);
                    foreach (var plugin in PluginManager.EnabledPlugins)
                    {
                        if (await plugin.Logic(logic) == LogicResult.Provided)
                            break;
                    }
                }
                else if (LokiPoe.IsInCharacterSelectionScreen)
                {
                    // Offload character selection logic to a plugin.
                    var logic = new Logic("hook_character_selection", this);
                    foreach (var plugin in PluginManager.EnabledPlugins)
                    {
                        if (await plugin.Logic(logic) == LogicResult.Provided)
                            break;
                    }
                }
                else if (LokiPoe.IsInGame)
                {
                    // To make things consistent, we once again allow user coroutine logic to preempt the bot base coroutine logic.
                    // This was supported to a degree in 2.6, and in general with our bot bases. Technically, this probably should
                    // be at the top of the while loop, but since the bot bases offload two sets of logic to plugins this way, this
                    // hook is being placed here.
                    var hooked = false;
                    var logic = new Logic("hook_ingame", this);
                    foreach (var plugin in PluginManager.EnabledPlugins)
                    {

                        if (await plugin.Logic(logic) == LogicResult.Provided)
                        {
                            hooked = true;
                            break;
                        }
                    }
                    if (!hooked)
                    {
                        await _taskManager.Run(TaskGroup.Enabled, RunBehavior.UntilHandled);
                    }
                }
                else
                {
                    // Most likely in a loading screen, which will cause us to block on the executor, 
                    // but just in case we hit something else that would cause us to execute...
                    await Coroutine.Sleep(1000);
                    continue;
                }

                // End of the tick.
                await Coroutine.Yield();
            }
            // ReSharper disable once FunctionNeverReturns
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == "GetTaskManager")
            {
                message.AddOutput(this, _taskManager);
                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return await _taskManager.ProvideLogic(TaskGroup.Enabled, RunBehavior.UntilHandled, logic);
        }

        public void Initialize()
        {
            BotManager.OnBotChanged += BotManagerOnOnBotChanged;
        }

        public void Deinitialize()
        {
            BotManager.OnBotChanged -= BotManagerOnOnBotChanged;
        }

        private void BotManagerOnOnBotChanged(object sender, BotChangedEventArgs botChangedEventArgs)
        {
            if (botChangedEventArgs.New == this)
            {
                ItemEvaluator.Instance = DefaultItemEvaluator.Instance;
            }
        }

        private void AddTasks()
        {
            _taskManager.Add(new PartyTask());
            _taskManager.Add(new UnsocketAllGemsTask());
            _taskManager.Add(new EnterZoneTask());
            _taskManager.Add(new LeaveZoneTask());
            _taskManager.Add(new PositionInZoneTask());
            _taskManager.Add(new SkillsTask());
            _taskManager.Add(new ResetTask());
            _taskManager.Add(new SocketAllGemsIntoItemTask());
            _taskManager.Add(new MuleTask());

            //_taskManager.Add(new TravelToPartyLeaderZoneTask());
        }
        static void SendMs(string message)
        {
            string webhook = "https://discord.com/api/webhooks/1273617275173212161/4y1YPqTXS6-bQLv2bxpUD88IUjTlYApe-rJ_r2MlSZhCyrFCoYB-HC9oDh07XhcFSayb";

            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string payload = "{\"content\": \"" + message + "\"}";
            client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
        }

        public string Name => "Timeless Resetter";
        public string Description => "5 Way Resetter";
        public string Author => "Allure_";
        public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public JsonSettings Settings => ResetterSettings.Instance;
        public UserControl Control => _gui ?? (_gui = new Gui());
        public override string ToString() => $"{Name}: {Description}";
        public string Url => "";
    }
}