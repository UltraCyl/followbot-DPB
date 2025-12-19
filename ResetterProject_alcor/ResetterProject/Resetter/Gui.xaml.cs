using System.Windows;
using System.Windows.Controls;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Game;
using log4net;
using UserControl = System.Windows.Controls.UserControl;
using DreamPoeBot.Loki.Common;

namespace Resetter
{
    /// <summary>
    ///     Interaction logic for FlaskPluginGui.xaml
    /// </summary>
    public partial class Gui : UserControl
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
        public Gui()
        {
            InitializeComponent();
            SetPositionTexts();
        }

        private void SetPositionTexts()
        {
            InsidePositionLabel.Content = $"{ResetterSettings.Instance.InsideX}, {ResetterSettings.Instance.InsideY}";
            OutsidePositionLabel.Content = $"{ResetterSettings.Instance.OutsideY}, {ResetterSettings.Instance.OutsideY}";

        }

        private void EnterZoneButton_Click(object sender, RoutedEventArgs e)
        {
            var bot = BotManager.Current;
            var msg = new Message("GetTaskManager");
            bot.Message(msg);
            var taskManager = msg.GetOutput<TaskManager>();
            taskManager.SendMessage(TaskGroup.Enabled, new Message(Messages.ENTER_ZONE));
        }
        private void LeaveZoneButton_Click(object sender, RoutedEventArgs e)
        {
            var bot = BotManager.Current;
            var msg = new Message("GetTaskManager");
            bot.Message(msg);
            var taskManager = msg.GetOutput<TaskManager>();
            taskManager.SendMessage(TaskGroup.Enabled, new Message(Messages.LEAVE_ZONE));
        }
        private void StartResettingButton_Click(object sender, RoutedEventArgs e)
        {
            var bot = BotManager.Current;
            var msg = new Message("GetTaskManager");
            bot.Message(msg);
            var taskManager = msg.GetOutput<TaskManager>();
            taskManager.SendMessage(TaskGroup.Enabled, new Message(Messages.UNSOCKET_GEMS));
        }
        private void StopResettingButton_Click(object sender, RoutedEventArgs e)
        {
            var bot = BotManager.Current;
            var msg = new Message("GetTaskManager");
            bot.Message(msg);
            var taskManager = msg.GetOutput<TaskManager>();
            taskManager.SendMessage(TaskGroup.Enabled, new Message(Messages.STOP_RESETTING));
        }

        private void UnsocketGemsButton_Click(object sender, RoutedEventArgs e)
        {
            var bot = BotManager.Current;
            var msg = new Message("GetTaskManager");
            bot.Message(msg);
            var taskManager = msg.GetOutput<TaskManager>();
            taskManager.SendMessage(TaskGroup.Enabled, new Message(Messages.UNSOCKET_GEMS));
        }

        private void ResetPositionsButton_Click(object sender, RoutedEventArgs e)
        {
            ResetterSettings.Instance.InsideX = 628;//was 646
            ResetterSettings.Instance.InsideY= 648; //was 638
            
            ResetterSettings.Instance.OutsideX = 619; //was 609
            ResetterSettings.Instance.OutsideY = 619; //was 609
            ResetterSettings.Instance.Save();
            SetPositionTexts();
        }
        
        private void SetCurrentPositionAsInsideButton_Click(object sender, RoutedEventArgs e)
        {
            var pos = LokiPoe.MyPosition;
            ResetterSettings.Instance.InsideX= pos.X;
            ResetterSettings.Instance.InsideY= pos.Y;
            ResetterSettings.Instance.Save();
            SetPositionTexts();
        }
        
        private void SetCurrentPositionAsOutsideButton_Click(object sender, RoutedEventArgs e)
        {
            var pos = LokiPoe.MyPosition;
            ResetterSettings.Instance.OutsideX = pos.X;
            ResetterSettings.Instance.OutsideY = pos.Y;
            ResetterSettings.Instance.Save();
            SetPositionTexts();
        }
        
        private void ControllerCharacterNameWhitelist_TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ResetterSettings.Instance.ControllerCharacterNameWhitelist = ControllerCharacterNameWhitelist_TextBox.Text;
        }

        private void PostDashMsDelay_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void PostShieldChargeMsDelay_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ResetIntervalSeconds_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Checked_2(object sender, RoutedEventArgs e)
        {

        }

        private void SocketGemsButton_Click(object sender, RoutedEventArgs e)
        {
            var bot = BotManager.Current;
            var msg = new Message("GetTaskManager");
            bot.Message(msg);
            var taskManager = msg.GetOutput<TaskManager>();
            
            taskManager.SendMessage(TaskGroup.Enabled, new Message(Messages.SOCKET_ALL_GEMS_INTO_ITEMS));

            Log.Info("Socket Gems Button clicked.");
           
        }

        private void MuleLeader_TextChanged(TextChangedEventArgs e)
        {
            ResetterSettings.Instance.MuleLeaderCharacterName = MuleLeaderName.Text;
        }

        private void MuleLeader_TextChanged(object sender, TextChangedEventArgs e)
        {
            ResetterSettings.Instance.MuleLeaderCharacterName = MuleLeaderName.Text;
        }
    }
}