using System.Windows;
using System.Windows.Controls;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Game;
using UserControl = System.Windows.Controls.UserControl;

namespace Resetter.Carry
{
    /// <summary>
    ///     Interaction logic for FlaskPluginGui.xaml
    /// </summary>
    public partial class Gui : UserControl
    {

        public Gui()
        {
            InitializeComponent();
            SetPositionTexts();
        }

        private void SetPositionTexts()
        {
            CarryDefaultPositionLabel.Content = $"{ResetterSettings.Instance.CarryDefaultX}, {ResetterSettings.Instance.CarryDefaultY}";

        }

        
        private void SetCurrentPositionAsCarryPositionButton_Click(object sender, RoutedEventArgs e)
        {
            var pos = LokiPoe.MyPosition;
            ResetterSettings.Instance.CarryDefaultX = pos.X;
            ResetterSettings.Instance.CarryDefaultY = pos.Y;
            ResetterSettings.Instance.Save();
            SetPositionTexts();
        }

        private void UnsocketGems_Click(object sender, RoutedEventArgs e)
        {
            var bot = BotManager.Current;
            var msg = new Message("GetTaskManager");
            bot.Message(msg);
            var taskManager = msg.GetOutput<TaskManager>();
            taskManager.SendMessage(TaskGroup.Enabled, new Message(Messages.CARRY_UNSOCKET_GEMS));
        }

       
        private void EnterZone_Click(object sender, RoutedEventArgs e)
        {
            var bot = BotManager.Current;
            var msg = new Message("GetTaskManager");
            bot.Message(msg);
            var taskManager = msg.GetOutput<TaskManager>();
            taskManager.SendMessage(TaskGroup.Enabled, new Message(Messages.CARRY_ENTER_ZONE));

        }

        private void LeaveZone_Click(object sender, RoutedEventArgs e)
        {
            var bot = BotManager.Current;
            var msg = new Message("GetTaskManager");
            bot.Message(msg);
            var taskManager = msg.GetOutput<TaskManager>();
            taskManager.SendMessage(TaskGroup.Enabled, new Message(Messages.CARRY_LEAVE_ZONE));
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {

        }
    }
}