using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Generics;

namespace AprilFools
{
    class ScreenCoverForm : Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        protected override bool ShowWithoutActivation { get { return true; } }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        public ScreenCoverForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            GenericsClass.CalcAllSceensBounds();
            this.SuspendLayout();
            
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            //this.TopMost = true;//this seems to require focus on show

            this.ClientSize = GenericsClass.ScreenBounds.Size;
            this.Location = GenericsClass.ScreenBounds.Location;
        }

        /// <summary>
        /// displays black form over everything on the screen (maybe not the taskbar). Flashes X times for Xms each time on X monitor
        /// </summary>
        /// <param name="flickerCount">Number of times the screen will flicker</param>
        /// <param name="delay">time in ms flicker is on screen and how long between flickers</param>
        /// <param name="screenNumber">screen number to flicker (-1 for all)</param>
        public void Flicker(int flickerCount, int delay=50, int screenNumber=0)
        {
            if (screenNumber >= Screen.AllScreens.Length)
                screenNumber = 0;
            if (screenNumber < -1)//all screens
            {
                this.ClientSize = GenericsClass.ScreenBounds.Size;
                this.Location = GenericsClass.ScreenBounds.Location;
            }
            else
            {
                this.Location = Screen.AllScreens[screenNumber].Bounds.Location;
                this.Size = Screen.AllScreens[screenNumber].Bounds.Size;
            }
            for (int i = 0; i < flickerCount; i++)
            {
                Show();
                DateTime future = DateTime.Now.AddMilliseconds(delay);
                while (DateTime.Now < future) ;
                
                Hide();
                future = DateTime.Now.AddMilliseconds(delay);
                while (DateTime.Now < future) ;
            }
        }

        public new void Hide()
        {
            base.Hide();
            Cursor.Show();
        }

        public new void Show()
        {
            base.Show();
            Cursor.Hide();
        }
    }
}
