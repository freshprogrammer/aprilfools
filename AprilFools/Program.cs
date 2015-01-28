#define _TESTING

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Windows.Forms;
using System.Media;
using Generics;

namespace AprilFools
{
    /*
     * This application will pose as java update application jucheck.exe with app decriptions and icon to match
     * 
     * Hotkeys:
     * CTRL+WIN+F2 Gloabl start
     * CTRL+WIN+F4 Gloabl pause
     * 
     * --ideas
     * program wil launch and sit silently 
     * make noise every hour
     * make a accelerating beeping noise perioticly
     * start odd windows (like ads)
     * popup random error mssages for chrome & memmory
     * 
     */

    public class Pranker
    {
        public static int _startupDelaySeconds = 0;

        private static bool _allPrankingEnabled = true;

        public static bool _eraticMouseThreadRunning = false;
        public static bool _eraticKeyboardThreadRunning = false;
        public static bool _randomSoundThreadRunning = true;
        public static bool _randomPopupThreadRunning = false;

        private static Thread eraticMouseThread;
        private static Thread eraticKeyboardThread;
        private static Thread randomSoundThread;
        private static Thread randomPopupThread;

        /// <summary>
        /// Entry point for prank application
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("April Fools Prank by: Dougie Fresh");

#if _TESTING
            //if (MessageBox.Show("Running in testing mode. Press OK to start.","\"The\" App",MessageBoxButtons.OKCancel,MessageBoxIcon.Warning) == DialogResult.Cancel)return;
            GenericsClass.Beep(BeepPitch.Medium, Generics.BeepDurration.Shrt);
            GenericsClass.Beep(BeepPitch.Medium, BeepDurration.Shrt);
#endif

            //register hotkey(s)
            HotKeyManager.RegisterHotKey((KeyModifiers.Control | KeyModifiers.Windows), Keys.F2);
            HotKeyManager.RegisterHotKey((KeyModifiers.Control | KeyModifiers.Windows), Keys.F4);
            HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(HotKeyManager_HotKeyPressed);

            // Check for command line arguments and assign the new values
            if (args.Length >= 2)
            {
                _startupDelaySeconds = Convert.ToInt32(args[0]);
            }


            DateTime future = DateTime.Now.AddSeconds(_startupDelaySeconds);
            Console.WriteLine("Waiting " + _startupDelaySeconds + " seconds before starting threads");
            while (future > DateTime.Now)
            {
                Thread.Sleep(500);
            }
            Console.WriteLine("starting");
            
            // Create all threads that manipulate all of the inputs and outputs to the system
            eraticMouseThread = new Thread(new ThreadStart(EraticMouseThread));
            eraticKeyboardThread = new Thread(new ThreadStart(EraticKeyboardThread));
            randomSoundThread = new Thread(new ThreadStart(RandomSoundThread));
            randomPopupThread = new Thread(new ThreadStart(RandomPopupThread));
            // Start all of the threads
            eraticMouseThread.Start();
            eraticKeyboardThread.Start();
            randomSoundThread.Start();
            randomPopupThread.Start();

            Console.WriteLine("Terminating all threads");

            Thread.Sleep(5000000);
            //ExitApplication();
        }

        static void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            Console.WriteLine("HotKeyManager_HotKeyPressed() - " + e.Modifiers + "+" + e.Key);

            if (e.Modifiers == (KeyModifiers.Control | KeyModifiers.Windows) && e.Key == Keys.F2)
            {
                StartPranking();
            }
            else if (e.Modifiers == (KeyModifiers.Control | KeyModifiers.Windows) && e.Key == Keys.F4)
            {
                StopPranking();
            }
            else
            {
                //uncaught hotkey
                Console.WriteLine("HotKeyManager_HotKeyPressed() - UnActioned");
            }
        }

        public static void StartPranking()
        {
#if _TESTING
            if (!_allPrankingEnabled)
            {
                GenericsClass.Beep(BeepPitch.High, BeepDurration.Shrt);
                GenericsClass.Beep(BeepPitch.High, BeepDurration.Shrt);
            }
            GenericsClass.Beep(BeepPitch.High, BeepDurration.Shrt);
#endif

            _allPrankingEnabled = true;
        }

        public static void StopPranking()
        {
#if _TESTING
            if (_allPrankingEnabled)
            {
                GenericsClass.Beep(BeepPitch.Low, BeepDurration.Shrt);
                GenericsClass.Beep(BeepPitch.Low, BeepDurration.Shrt);
            }
            GenericsClass.Beep(BeepPitch.Low, BeepDurration.Shrt);
#endif

            _allPrankingEnabled = false;
        }

        public static void ExitApplication()
        {

#if _TESTING
            //MessageBox.Show("Exiting application.","\"The\" App",MessageBoxButtons.OK,MessageBoxIcon.None);

            GenericsClass.Beep(BeepPitch.Low, BeepDurration.Shrt);
            GenericsClass.Beep(BeepPitch.Low, BeepDurration.Shrt);
#endif

            // Kill all threads and exit application
            eraticMouseThread.Abort();
            eraticKeyboardThread.Abort();
            randomSoundThread.Abort();
            randomPopupThread.Abort();
        }


        #region Thread Functions
        /// <summary>
        /// This thread will randomly affect the mouse movements to screw with the end user
        /// </summary>
        public static void EraticMouseThread()
        {
            Console.WriteLine("EraticMouseThread Started");
            Thread.CurrentThread.Name = "EraticMouseThread";
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            int moveX = 0;
            int moveY = 0;

            while (true)
            {
                if (_allPrankingEnabled && _eraticMouseThreadRunning)
                {
                    // Console.WriteLine(Cursor.Position.ToString());

                    if (GenericsClass._random.Next(100) > 50)
                    {
                        // Generate the random amount to move the cursor on X and Y
                        moveX = GenericsClass._random.Next(20 + 1) - 10;
                        moveY = GenericsClass._random.Next(20 + 1) - 10;

                        // Change mouse cursor position to new random coordinates
                        Cursor.Position = new System.Drawing.Point(
                            Cursor.Position.X + moveX,
                            Cursor.Position.Y + moveY);
                    }
                }
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// This will generate random keyboard output to screw with the end user
        /// </summary>
        public static void EraticKeyboardThread()
        {
            Console.WriteLine("EraticKeyboardThread Started");
            Thread.CurrentThread.Name = "EraticKeyboardThread";
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            while (true)
            {
                if (_allPrankingEnabled && _eraticKeyboardThreadRunning)
                {
                    if (GenericsClass._random.Next(100) >= 95)
                    {
                        // Generate a random capitol letter
                        char key = (char)(GenericsClass._random.Next(26) + 65);

                        // 50/50 make it lower case
                        if (GenericsClass._random.Next(2) == 0)
                        {
                            key = Char.ToLower(key);
                        }

                        SendKeys.SendWait(key.ToString());
                    }
                }
                Thread.Sleep(GenericsClass._random.Next(500));
            }
        }

        /// <summary>
        /// This will play system sounds at random to screw with the end user
        /// </summary>
        public static void RandomSoundThread()
        {
            Console.WriteLine("RandomSoundThread Started");
            Thread.CurrentThread.Name = "RandomSoundThread";
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            while (true)
            {
                if (_allPrankingEnabled && _randomSoundThreadRunning)
                {
                    // Determine if we're going to play a sound this time through the loop (20% odds)
                    //if (_random.Next(100) >= 80)

                    // Randomly select a system sound
                    int sound = GenericsClass._random.Next(5);
                    sound = 3;
                    switch (sound)
                    {
                        case 0:
                            SystemSounds.Asterisk.Play();
                            break;
                        case 1:
                            SystemSounds.Beep.Play();
                            break;
                        case 2:
                            SystemSounds.Exclamation.Play();
                            break;
                        case 3:
                            SystemSounds.Hand.Play();
                            break;
                        case 4:
                            SystemSounds.Question.Play();
                            break;
                    }
                }
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// This thread will popup fake error notifications to make the user go crazy and pull their hair out
        /// </summary>
        public static void RandomPopupThread()
        {
            Console.WriteLine("RandomPopupThread Started");
            Thread.CurrentThread.Name = "RandomPopupThread";
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            const int popupInterval = 1000 * 60 * 90;//90 minutes
            const int popupIntervalVariance = 1000 * 60 * 10;//10 minutes +/-
            const int oddsOfSeeingAPopupEachInterval = 10;

            Choice popup = new Choice();
            Possibility pos_chrome = popup.AddPossibility(5, "Chrome");
            Possibility pos_mem = popup.AddPossibility(20, "Memory");
            //Possibility pos_ie = popup.AddPossibility(20, "IE");
            //Possibility pos_calc = popup.AddPossibility(1, "Calc");

            while (true)
            {
                if (_allPrankingEnabled && _randomPopupThreadRunning)
                {
                    // Every 10 seconds roll the dice and 10% of the time show a dialog
                    if (GenericsClass._random.Next(100) >= (100 - oddsOfSeeingAPopupEachInterval))
                    {
                        // Determine which message to show user
                        if (popup.RandomChoice().ID == pos_chrome.ID)
                            MessageBox.Show(
                               "Chrome is dangerously low on resources.",
                                "Chrome",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        else if (popup.RandomChoice().ID == pos_mem.ID)
                            MessageBox.Show(
                               "Your system is running low on resources",
                                "Microsoft Windows",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                    }
                }
                int variance = GenericsClass._random.Next(popupIntervalVariance * 2) - popupIntervalVariance * 2 - popupIntervalVariance + 1; //*2 for +/- then +1 to include the Next() MAX
                Thread.Sleep(popupInterval + variance);
            }
        }
        #endregion
    }
}