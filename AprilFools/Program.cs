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
     * Alt+Shfit+F4 kill application - non reversable without manual restart
     * Alt+Shfit+1 Test Button - only runs in test mode
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
        private static bool _applicationRunning = true;
        private static bool _allPrankingEnabled = false;// set false if delayed start

        private static int _mainThreadPollingInterval = 50;//sleep time for main thread

        private static DateTime _delayedStartTime;
        private static bool _delayedStartEnabled = false;

        private static bool _eraticMouseThreadRunning = false;
        private static bool _eraticKeyboardThreadRunning = false;
        private static bool _randomSoundThreadRunning = true;
        private static bool _randomPopupThreadRunning = false;

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
            Thread.CurrentThread.Name = "Pranker Main Thread";
            Console.WriteLine("April Fools Prank by: Dougie Fresh");

#if _TESTING
            //if (MessageBox.Show("Running in testing mode. Press OK to start.","\"The\" App",MessageBoxButtons.OKCancel,MessageBoxIcon.Warning) == DialogResult.Cancel)return;
            GenericsClass.Beep(BeepPitch.Medium, BeepDurration.Long);
            GenericsClass.Beep(BeepPitch.Medium, BeepDurration.Long);
#endif

            //register hotkey(s)
            HotKeyManager.RegisterHotKey((KeyModifiers.Control | KeyModifiers.Windows), Keys.F2);
            HotKeyManager.RegisterHotKey((KeyModifiers.Control | KeyModifiers.Windows), Keys.F4);
            HotKeyManager.RegisterHotKey((KeyModifiers.Alt | KeyModifiers.Shift), Keys.F4);
            HotKeyManager.RegisterHotKey((KeyModifiers.Alt | KeyModifiers.Shift), Keys.D1);
            HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(HotKeyManager_HotKeyPressed);

            // Check for command line arguments
            int startDelay = 0;
            if (args.Length >= 1)
            {
                startDelay = Convert.ToInt32(args[0]);
            }

            //setup delayed start
            if (startDelay > 0)
            {
                _allPrankingEnabled = false;
                _delayedStartEnabled = true;
                _delayedStartTime = DateTime.Now.AddSeconds(startDelay);
            }

            Console.WriteLine("Starting");
            
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


            StartMainBackgroundThread();
        }

        static void StartMainBackgroundThread()
        {
            //dont start a new thread, just use the base thread
            while (_applicationRunning)
            {
                //check for delated start
                if (_delayedStartEnabled && _delayedStartTime <= DateTime.Now)
                {
                    _allPrankingEnabled = true;
                    _delayedStartEnabled = false;
                }
                
                //check for timed events here



                Thread.Sleep(_mainThreadPollingInterval);
            }
            
            ExitApplication();
        }

        static void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
        {

            if (e.Modifiers == (KeyModifiers.Control | KeyModifiers.Windows) && e.Key == Keys.F2)
            {
                Console.WriteLine("HotKeyManager_HotKeyPressed() - " + e.Modifiers + "+" + e.Key + " - Start Pranking");
                StartPranking();
            }
            else if (e.Modifiers == (KeyModifiers.Control | KeyModifiers.Windows) && e.Key == Keys.F4)
            {
                Console.WriteLine("HotKeyManager_HotKeyPressed() - " + e.Modifiers + "+" + e.Key + " - Stop Pranking");
                StopPranking();
            }
            else if (e.Modifiers == (KeyModifiers.Alt | KeyModifiers.Shift) && e.Key == Keys.F4)
            {
                Console.WriteLine("HotKeyManager_HotKeyPressed() - " + e.Modifiers + "+" + e.Key + " - Kill Application");
                //stop everything and kill application
                _applicationRunning = false;
            }
            else if (e.Modifiers == (KeyModifiers.Alt | KeyModifiers.Shift) && e.Key == Keys.D1)
            {
#if _TESTING
                Console.WriteLine("HotKeyManager_HotKeyPressed() - " + e.Modifiers + "+" + e.Key + " - Test Key");
                TestCode();
#else
                Console.WriteLine("HotKeyManager_HotKeyPressed() - " + e.Modifiers + "+" + e.Key + " - Test Key - disabled without test mode");
#endif
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
                GenericsClass.Beep(BeepPitch.High, BeepDurration.Short);
                GenericsClass.Beep(BeepPitch.High, BeepDurration.Short);
            }
            GenericsClass.Beep(BeepPitch.High, BeepDurration.Short);
#endif

            _allPrankingEnabled = true;
        }

        public static void StopPranking()
        {
#if _TESTING
            if (_allPrankingEnabled)
            {
                GenericsClass.Beep(BeepPitch.Low, BeepDurration.Short);
                GenericsClass.Beep(BeepPitch.Low, BeepDurration.Short);
            }
            GenericsClass.Beep(BeepPitch.Low, BeepDurration.Short);
#endif

            _allPrankingEnabled = false;
        }

        public static void ExitApplication()
        {

#if _TESTING
            //MessageBox.Show("Exiting application.","\"The\" App",MessageBoxButtons.OK,MessageBoxIcon.None);

            GenericsClass.Beep(BeepPitch.Low, BeepDurration.Long);
            GenericsClass.Beep(BeepPitch.Low, BeepDurration.Long);
#endif

            Console.WriteLine("Terminating all threads");
            // Kill all threads and exit application
            eraticMouseThread.Abort();
            eraticKeyboardThread.Abort();
            randomSoundThread.Abort();
            randomPopupThread.Abort();
        }

        public static void TestCode()
        {
            //BombBeepCountdown();
        }

        #region Action functions
        /// <summary>
        /// Beep with less as less pause like a bomb. NOTE: this is a blocking proceedure that wont stop till its done.
        /// </summary>
        public static void BombBeepCountdown()
        {
            int pause = 2000;
            while (pause > 400)
            {
                GenericsClass.Beep(BeepPitch.High, BeepDurration.Medium);
                Thread.Sleep(pause -= (int)(pause * 0.15));
            }
            while (pause >= 25)
            {
                GenericsClass.Beep(BeepPitch.High, BeepDurration.Medium);
                Thread.Sleep(pause -= 25);
            }
            GenericsClass.Beep(BeepPitch.High, BeepDurration.Medium);
            GenericsClass.Beep(BeepPitch.High, BeepDurration.Medium);
            GenericsClass.Beep(BeepPitch.High, BeepDurration.Short);
            GenericsClass.Beep(BeepPitch.High, BeepDurration.Short);
            GenericsClass.Beep(BeepPitch.High, BeepDurration.Short);
            GenericsClass.Beep(BeepPitch.High, BeepDurration.Short);
            GenericsClass.Beep(BeepPitch.High, BeepDurration.Short);

            GenericsClass.Beep(BeepPitch.High, 100);
            GenericsClass.Beep(BeepPitch.High, 100);
            GenericsClass.Beep(BeepPitch.High, 100);

            GenericsClass.Beep(BeepPitch.High, 50);
            GenericsClass.Beep(BeepPitch.High, 50);
            GenericsClass.Beep(BeepPitch.High, 50);
        }
        #endregion

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