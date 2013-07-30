/*
 * This file is part of the WhiteRain project (C) 2013 Finn Grimpe
 * Copyright 2013 Finn Grimpe, All Rights Reserved
 * 
 * Github:  https://github.com/finndev/WhiteFish/
 * Website: https://finn.lu/whitefish/
 * License: https://finn.lu/license/
 *
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using WhiteRainNS;

namespace WhiteFish.GUI
{
    public partial class Main : Form
    {
        Thread ObjectManagerThread { get; set; }
        internal static bool IsExiting { get; set; }

        public Main()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Process wowProc = Process.GetProcessesByName("Wow").FirstOrDefault();

            try
            {
                Debug.Initialize(this);
                Memory.Initialize(wowProc);
                WhiteRain.Initialize(wowProc, new WhiteRain.LaunchParameters { UpdateAutomatically = true });
                WhiteRain.Pulse();
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }

            if (!WhiteRain.Initialized) return;

            Debug.Log("WhiteFish V. 1.0 launched.");

            ObjectManagerThread = new Thread(delegate() {
                while (!IsExiting)
                {
                    WhiteRain.Pulse();
                    Thread.Sleep(250);
                }
            });
            ObjectManagerThread.Start();
        }

        private void LogBox_TextChanged(object sender, EventArgs e)
        {
            LogBox.ScrollToCaret();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            IsExiting = true;
            Engine.Exit();
        }

        private void StartStopBtn_Click(object sender, EventArgs e)
        {
            if (fishingSpell.Text.Length == 0)
            {
                MessageBox.Show("Error: Please insert your fishing name spell!");
                return;
            }

            if (StartStopBtn.Text == "Start")
            {
                StartStopBtn.Text = "Stop";
                Engine.Run();
            }
            else
            {
                StartStopBtn.Text = "Start";
                Engine.Exit();
            }
        }

        private void linkLabel_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.wowhead.com/items=0.-3");
        }
    }
}
