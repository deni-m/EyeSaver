using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Threading;

namespace HealthBreakInformer
{


    public partial class Form1 : Form
    {
        delegate void SetTextCallback(string text);

        private DateTime LastSessionStart = DateTime.Now;
        private DateTime LastSessionFinish;
        private TimeSpan TotalWorkingTime;
        private TimeSpan TotalBreakTime;

        private int WarningMinutes = 45;
        private int ShowBalloonTime = 5*60*1000;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            WorkingTime();
            LastSessionStart = DateTime.Now;
            this.TopMost = true;
            listBox1.Items.Add("SessionStart " + LastSessionStart.ToString());
        }

        void SystemEvents_SessionSwitch (Object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                listBox1.Items.Add("SessionLength " + GetTimeSubstraction(LastSessionStart));
                LastSessionFinish = DateTime.Now;
                TotalWorkingTime += LastSessionFinish - LastSessionStart;
            }
            else
            {
                TotalBreakTime += DateTime.Now - LastSessionFinish;
                listBox1.Items.Add("BreakLength " + GetTimeSubstraction(LastSessionFinish));
                listBox1.Items.Add(" --- ");
                
                LastSessionStart = DateTime.Now;
                listBox1.Items.Add("SessionStart " + LastSessionStart);
                
            }
        }

        void SetText (string text)
        {
            if (this.label1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.label1.Text = text;
                this.notifyIcon1.Text = text;
                UpdateStatistic();
            }
        }

        void WorkingTime()
        {
            Thread t = new Thread(new ThreadStart(this.DisplaySessionTime));
            t.IsBackground = true;
            t.Start();
        }

        string GetTimeSubstraction (DateTime datetime)
        {
            TimeSpan SessionLength = DateTime.Now.Subtract(datetime);
           
            return FormatTimeSpan(SessionLength);   
        }
        

        string FormatTimeSpan(TimeSpan TimeSpan)
        {
            //return string.Format("{0} {1}:{2}:{3}", TimeSpan.Days, TimeSpan.Hours, TimeSpan.Minutes, TimeSpan.Seconds);
            return string.Format("{0}:{1}:{2}", TimeSpan.Hours, TimeSpan.Minutes, TimeSpan.Seconds);
        }

        void DisplaySessionTime ()
        {
            while (true)
            {
                this.SetText(GetTimeSubstraction(LastSessionStart));   
                Thread.Sleep(5000);
            }
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            string message = "Are you sure you would like to close application?";
            string caption = "Application Closing";
            var result = MessageBox.Show(message, caption,
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);

            if (result==DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                Hide();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void UpdateStatistic()
        {
            label3.Text = string.Format("Total working time: {0}", FormatTimeSpan(TotalWorkingTime + (DateTime.Now - LastSessionStart)));
            label2.Text = string.Format("Total breaks time:  {0}", FormatTimeSpan(TotalBreakTime));

            TimeSpan SessionLength = DateTime.Now - LastSessionStart;

            if (DateTime.Now.Day != LastSessionStart.Day)
            {
                 ResetAllTimers();    
            }
            
            if (SessionLength.Minutes >= WarningMinutes)
            {   
               notifyIcon1.BalloonTipText = "Пора отдохнуть";
               notifyIcon1.ShowBalloonTip(ShowBalloonTime);
               label1.ForeColor = Color.Red;

            } else
            {
                if (label1.ForeColor == Color.Red)
                {
                    label1.ForeColor = Color.Black;
                }
            }
        }

        private void ResetAllTimers()
        {
            listBox1.Items.Add("-----Reset due to new day started----");
            this.LastSessionStart = DateTime.Now;
            this.LastSessionFinish = DateTime.Now;
            this.TotalBreakTime = TimeSpan.MinValue;
            this.TotalWorkingTime = TimeSpan.MinValue;

        }

        

        
    }

    
}
