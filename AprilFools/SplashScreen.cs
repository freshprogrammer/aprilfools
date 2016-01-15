using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace AprilFools
{
    public partial class SplashScreen : Form
    {
        /*
         * NOTE that running this form will add a link back the main pocess - for total anonymity a seperate process should be lauched to launch the window
         */
        public int CloseClickCount = 0;
        public int CloseClickDelayLimit = 3;

        NotifyIcon icon = new NotifyIcon();

        public SplashScreen()
        {
            InitializeComponent();
            icon.Icon = this.Icon;
            icon.BalloonTipText = "Installing Windows 10...";
            //icon.BalloonTipTitle = "BT title";
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SplashClicked();
        }

        private void SplashClicked()
        {
            //NOTE: running this sets the icon on the main process to this icon in taskmanager...
            icon.Visible = true;
            icon.ShowBalloonTip(1000);
            //icon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);//reset parent process icon to what it was (in task manager)
            icon.Visible = false;
            icon.Dispose();

            this.Hide();
        }

        private void SplashScreen_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (++CloseClickCount < CloseClickDelayLimit)
                e.Cancel = true;
        }
    }
}
