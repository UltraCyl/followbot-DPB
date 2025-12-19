using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Bot.Pathfinding;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Coroutine;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.RemoteMemoryObjects;
using log4net;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using Message = DreamPoeBot.Loki.Bot.Message;
using SkillBar = DreamPoeBot.Loki.Game.LokiPoe.InGameState.SkillBarHud;

namespace Resetter 
{
    public class SkillsTask: ITask
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();

        public string Author => "Allure_";
        public string Description => "Task for skills.";
        public string Name => "SkillsTask";
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

            var areaName = LokiPoe.CurrentWorldArea.Name;
            if (areaName != "Domain of Timeless Conflict")
                return false;

            foreach (var s in SkillBarHud.SkillBarSkills)
            {
                if (s == null)
                    continue;
                
                if (!s.CanUse()) continue;
                if (s.InternalId == "molten_shell_barrier" && s.Slot != -1)
                {
                    SkillBar.Use(s.Slot, true);
                    return true;
                }
                
                if (ResetterSettings.Instance.EnableVaalHaste && s.InternalId == "vaal_haste" && s.CanUse())
                {
                    SkillBar.Use(s.Slot, true);
                    return true;
                }
                
                if (ResetterSettings.Instance.EnableVaalClarity && s.InternalId == "vaal_clarity" && s.CanUse())
                {
                    SkillBar.Use(s.Slot, true);
                    return true;
                }
                
                if (ResetterSettings.Instance.EnableVaalDiscipline && s.InternalId == "vaal_discipline" && s.CanUse())
                {
                    SkillBar.Use(s.Slot, true);
                    return true;
                }

                if (
                    ResetterSettings.Instance.EnableDivineBlessingHatred &&
                    s.Name == "Hatred" &&
                    s.CanUse() &&
                    s.SkillTags.Contains("duration") &&
                    !LokiPoe.Me.HasBuff("Hatred Aura")
                )
                {
                    SkillBar.Use(s.Slot, false);
                    return true;
                }
                
                if (
                    ResetterSettings.Instance.EnableDivineBlessingWrath &&
                    s.Name == "Wrath" &&
                    s.CanUse() &&
                    s.SkillTags.Contains("duration") &&
                    !LokiPoe.Me.HasBuff("Wrath Aura")
                )
                {
                    SkillBar.Use(s.Slot, false);
                    return true;
                }
                
                
            }

            return false;
                
        }

        public void Tick()
        {
        }
    }
}