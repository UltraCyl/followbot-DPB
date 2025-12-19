using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Bot.Pathfinding;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.Loki.Game;
using log4net;
using Resetter.Carry.tasks;
using UserControl = System.Windows.Controls.UserControl;
using System.Text;
using Resetter.tasks;
using System.Windows;
using System.Net;

namespace Resetter.Carry
{
    
    public class Carry: IBot,  IUrlProvider
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        public int ProductId => 5;
        private Gui _gui;
        private Coroutine _coroutine;
        private int _lastKnownAreaId = 0;
        public bool _isInHideout = true;

        private readonly TaskManager _taskManager = new TaskManager();


        public void Start()
        {
            // if (!LokiPoe.IsInGame)
            //  {
            //     MessageBox.Show("Please get into the domain before starting the carry.");
            //     BotManager.Stop();
            //    return;
            // }
            if (BotManager.IsStopping){
                SendMs("Carry bot stopping");
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

            /* if (LokiPoe.Me.IsInHideout)
             {
                 Log.Debug("Character is in Hideout, proceed to start task in hideout");
                 _isInHideout = true;
                 LokiPoe.Input.Binding.Update();

                 BotManager.MsBetweenTicks = 30;
                 LokiPoe.Input.InputEventDelayMs = 30;


                 // Since this bot will be performing client actions, we need to enable the process hook manager.
                 LokiPoe.ProcessHookManager.Enable();

                 _coroutine = null;

                 ExilePather.Reload();

                 _taskManager.Reset();

                 AddTasksInHideout();

                 PluginManager.Start();
                 RoutineManager.Start();
                 PlayerMoverManager.Start();
                 _taskManager.Start();

                 foreach (var plugin in PluginManager.EnabledPlugins)
                 {
                     Log.Debug($"[Start] The plugin {plugin.Name} is enabled.");
                 }
                 return;
             }

             if (LokiPoe.CurrentWorldArea.Name == "Domain of Timeless Conflict" ) // If player is already inzone, we will run this script
             {
                 Log.Debug("Character is in 5way , proceed to start tasks for 5way carry");
                 LokiPoe.Input.Binding.Update();

                 BotManager.MsBetweenTicks = 30;
                 LokiPoe.Input.InputEventDelayMs = 30;


                 // Since this bot will be performing client actions, we need to enable the process hook manager.
                 LokiPoe.ProcessHookManager.Enable();

                 _coroutine = null;

                 ExilePather.Reload();

                 _taskManager.Reset();

                 AddTasksInFiveWays();

                 PluginManager.Start();
                 RoutineManager.Start();
                 PlayerMoverManager.Start();
                 _taskManager.Start();

                 foreach (var plugin in PluginManager.EnabledPlugins)
                 {
                     Log.Debug($"[Start] The plugin {plugin.Name} is enabled.");
                 }
                 return;
             }*/
            _isInHideout = true;
            LokiPoe.Input.Binding.Update();

            BotManager.MsBetweenTicks = 30;
            LokiPoe.Input.InputEventDelayMs = 30;


            // Since this bot will be performing client actions, we need to enable the process hook manager.
            LokiPoe.ProcessHookManager.Enable();

            _coroutine = null;

            ExilePather.Reload();

            _taskManager.Reset();

            AddTaskAll();

            PluginManager.Start();
            RoutineManager.Start();
            PlayerMoverManager.Start();
            _taskManager.Start();



            // Cache all bound keys.


            foreach (var plugin in PluginManager.EnabledPlugins)
            {
                Log.Debug($"[Start] The plugin {plugin.Name} is enabled.");
            }
        }

        private void AuthTick()
        {
            //var loaderPlugin = PluginManager.Plugins.FirstOrDefault(x => x.Name == "ExileLoaderPlugin");
            

           // var objectType = loaderPlugin.GetType();
            //MethodInfo theMethod = objectType.GetMethod("PluginAuthTick");
           // theMethod.Invoke(loaderPlugin, new object[] { ProductId });
        }

        public void Tick()
        {
           // AuthTick();
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

        private void AddTasksInFiveWays()
        {
            _taskManager.Add(new CarryPositionInZoneTask());
            _taskManager.Add(new LevelGemsTask());
            _taskManager.Add(new CombatTask());
            _taskManager.Add(new CarryDeathHandlingTask());

        }

        private void AddTaskAll()
        {
            _taskManager.Add(new CarryEnterZoneAndOpenPortalTask());
            _taskManager.Add(new CarryPositionInZoneTask());
            _taskManager.Add(new CombatTask());
            
            _taskManager.Add(new CarryUnsocketAllGemsTask());
            _taskManager.Add(new LevelGemsTask());
            _taskManager.Add(new LootTask());
           

        }

        private void AddTasksInHideout()
        {
            
            _taskManager.Add(new CarryUnsocketAllGemsTask());
            _taskManager.Add(new CarryEnterZoneAndOpenPortalTask ())   ;
            

        }

        static void SendMs(string message)
        {
            string webhook = "https://discord.com/api/webhooks/1273617275173212161/4y1YPqTXS6-bQLv2bxpUD88IUjTlYApe-rJ_r2MlSZhCyrFCoYB-HC9oDh07XhcFSayb";

            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string payload = "{\"content\": \"" + message + "\"}";
            client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
        }

        public string Name => "Timeless Carry";
        public string Description => "5 Way Carry";
        public string Author => "Allure_";
        public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public JsonSettings Settings => ResetterSettings.Instance;
        public UserControl Control => _gui ?? (_gui = new Gui());
        public override string ToString() => $"{Name}: {Description}";
        public string Url => "";
    }
}