using System.ComponentModel;
using DreamPoeBot.Loki;
using DreamPoeBot.Loki.Common;

namespace Resetter 
{
    public class ResetterSettings : JsonSettings, INotifyPropertyChanged
    {
        private static ResetterSettings _instance;
        public static ResetterSettings Instance => _instance ?? (_instance = new ResetterSettings());
        public event PropertyChangedEventHandler PropertyChanged;

        public int ResetIntervalMilliSeconds { get; set; } = 2300;
        public int InsideX { get; set; } = 628;
        public int InsideY { get; set; } = 648;
        public int OutsideX { get; set; } = 619;
        public int OutsideY { get; set; } = 619;
        
        public bool AutoLevelGems{ get; set; } = true;
        public bool CarryMule { get; set; } = false;
        public bool EnableVaalHaste { get; set; } = false;
        public bool EnableVaalDiscipline { get; set; } = false;
        public bool EnableVaalClarity { get; set; } = false;
        
        public bool EnableDivineBlessingWrath { get; set; } = false;
        public bool EnableDivineBlessingHatred { get; set; } = false;
        
        public int PostDashMsDelay { get; set; } = 550;
        public int PostShieldChargeMsDelay { get; set; } = 6000;

        public string MuleLeaderCharacterName { get; set; } = "JIMMyNevvTon#0177";

        public string ControllerCharacterNameWhitelist { get; set; } = "";
        public bool EnableAcceptPartyInvites { get; set; } = false;
        public bool EnableChatCommands { get; set; } = true;
        public string StartStopResetKeywords { get; set; } = "";
        public bool EnableAutoEnterZone { get; set; } = true;
        public bool EnableAutoLeaveZone { get; set; } = true;
        public bool EnableAutoToggleReset { get; set; } = true;

        public bool CarryAutoLeaveZone { get; set; } = true;
        
        //carry
        public int CarryDefaultX { get; set; } = 705;
        public int CarryDefaultY { get; set; } = 611;
        public int MaxCarryPositionDistance { get; set; } = 30;
        public bool CarryEnableLevelGems { get; set; } = true;

        private ResetterSettings()
            : base(GetSettingsFilePath(Configuration.Instance.Name, "Resetter", "Settings.json"))
        {
        }

    }

}