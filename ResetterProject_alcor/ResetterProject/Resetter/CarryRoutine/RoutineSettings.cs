using System.Collections.Generic;
using System.ComponentModel;
using DreamPoeBot.Loki;
using DreamPoeBot.Loki.Common;
using Newtonsoft.Json;

namespace CarryRoutine 
{
    internal class RoutineSettings : JsonSettings
    {
        private static RoutineSettings _instance;

        // Settings static types mostly used in the Gui
        [JsonIgnore] private static List<int> _allSkillSlots;
        private bool _alwaysAttackInPlace;
        public int FallBackSkillSlot { get; set; } = -1; 
        
        private RoutineSettings()
            : base(GetSettingsFilePath(Configuration.Instance.Name, "CarryRoutineSettings.json"))
        {
            // You should initialize your collection in here, the component will read the json file and set the old settings
            // and after that execute this lines, so on first execution or adding new settings to future versions, you should always initialize collection to ensure they are not null.
            // Ex:
            // if (MyListOfSecretClass == null)
            // {
            //    MyListOfSecretClass = new ObservableCollection<SecretClass>();
            // }
        }

        public static RoutineSettings Instance => _instance ?? (_instance = new RoutineSettings());

        /// <summary>List of all available skill slots </summary>
        [JsonIgnore]
        public static List<int> AllSkillSlots => _allSkillSlots ?? (_allSkillSlots = new List<int>
        {
            -1,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            11,
            12,
            13
        });
    }
}