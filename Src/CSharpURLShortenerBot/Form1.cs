using System;
using System.Windows.Forms;

namespace CSharpURLShortenerBot
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ShortenerBot.MessageReceived += OnMessageReceived; 
            ShortenerBot.MessageSent += OnMessageSent;
        }

        private void OnMessageSent(object sender, ChatEventArgs e)
        {
            WriteLog($"Bot response: {e.Message}");
        }

        private void OnMessageReceived(object sender, ChatEventArgs e)
        {
            WriteLog($"Received message: {e.Message}");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //                                        Your bot token 
            ShortenerBot.InitializeBot("706187293:AAFbcRwwRq9JJ6_oXtovFaCt-fF20PDlvZY");
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            BtnStop.Enabled = true;
            BtnStart.Enabled = false;
            ShortenerBot.Start();
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            BtnStop.Enabled = false;
            BtnStart.Enabled = true;
            ShortenerBot.Stop();
        }

        private void WriteLog(string logContent)
        {
            TxtLogs.Invoke(new Action(()
                => TxtLogs.Text += logContent + Environment.NewLine));
        }
    }
}
