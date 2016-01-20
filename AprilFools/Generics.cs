using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.IO;
using System.Text;
using System.Drawing;

namespace Generics
{
    public class GenericsClass
    {
        public static Random Random = new Random();

        #region Beep vaiables and functions
        public static void Beep(BeepPitch p, BeepDurration d)   { Beep((int)p, (int)d); }
        public static void Beep(BeepPitch p, int d)             { Beep((int)p, d); }
        public static void Beep(int p, BeepDurration d)         { Beep(p, (int)d); }
        public static void Beep(int p, int d)                   { Console.Beep(p, d); }

        /// <summary>
        /// Beep with less as less pause like a bomb. NOTE: this is a blocking proceedure that wont stop till its done.
        /// </summary>
        public static void PlayBombBeepCountdown()
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

        #region Web Crawling
        public static int DOWNLOAD_HTML_TIMEOUT = 1000 * 60 * 5;

        public static string DownloadHTML(string url, string cookie = null, bool reportExceptions = false)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                request.Timeout = DOWNLOAD_HTML_TIMEOUT;
                if (cookie != null)
                    request.Headers.Add(HttpRequestHeader.Cookie, cookie);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = null;
                    if (response.CharacterSet == null)
                        readStream = new StreamReader(receiveStream);
                    else
                        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                    string data = readStream.ReadToEnd();
                    response.Close();
                    readStream.Close();
                    return data;
                }
                return null;
            }
            catch (UriFormatException e)
            {
                GenericsClass.Log("DownloadHTML(string,string,bool) - Caught UriFormatException from url:\"" + url + "\" - " + e.Message);
                return null;
            }
            catch (WebException e)
            {
                if (reportExceptions)
                {
                    if(e.Message.IndexOf("timed out")>-1)
                    {
                        GenericsClass.Log("DownloadHTML(string,string,bool) - WebException - Call to page timed out.");
                    }
                    else
                    {
                        //track real errors too
                        if (url.IndexOf('?') > -1)//No need to send full url data - prevent endless repeating the nested failure call stack
                            GenericsClass.Log("DownloadHTML(string,string,bool) - Caught web exception from (trimmed) url:\"" + url.Substring(0,url.IndexOf('?')) + "\" - " + e.Message);
                        else
                            GenericsClass.Log("DownloadHTML(string,string,bool) - Caught web exception from url:\"" + url + "\" - " + e.Message);
                    }
                    return null;
                }
                else
                    throw e;
            }
        }
        #endregion

        #region Math
        public static double DegreesToRadians(double angleInDegrees)
        {
            return angleInDegrees * Math.PI / 180;
        }

        public static double RadiansToDegrees(double angleInRadians)
        {
            return angleInRadians * 180 / Math.PI;
        }

        /// <summary>
        /// Calculates angle from Point 1, to Point 2. Result returned in degrees
        /// </summary>
        /// <param name="x1">X cord of Point 1</param>
        /// <param name="y1">Y cord of Point 1</param>
        /// <param name="x2">X cord of Point 2</param>
        /// <param name="y2">Y cord of Point 2</param>
        /// <returns>Resulting angle in degrees</returns>
        public static float CalcHeading(float x1, float y1, float x2, float y2, bool degrees, bool cartesian)
        {
            if (x1 == x2 && y1 == y2)
                return 0;

            float xDist = x2 - x1;
            float yDist = y2 - y1;

            float xDif = Math.Abs(xDist);
            float yDif = Math.Abs(yDist);

            float result;
            if (degrees)
                result = (float)(Math.Atan(yDif / xDif) * 180 / Math.PI);
            else
                result = (float)Math.Atan(yDif / xDif);

            if (cartesian)
                result = ConvertAngleToCartersian(xDist, yDist, result, degrees);

            return result;
        }

        /// <summary>
        /// Converts angle where 0=Up and counter-clockwise to 0=Right and  clockwise.
        /// </summary>
        /// <param name="xDelta">xDir</param>
        /// <param name="yDelta">yDir</param>
        /// <param name="angle">current Angle</param>
        /// <param name="degrees">True for degrees, false for radians</param>
        /// <returns>new anlge</returns>
        public static float ConvertAngleToCartersian(float xDelta, float yDelta, float angle, bool degrees)
        {
            double quarterTurn;
            if (degrees)
                quarterTurn = 90;
            else
                quarterTurn = Math.PI / 2;

            double result = angle;
            if (yDelta < 0)
            {
                if (xDelta < 0)
                {
                    //System.out.println("Q3");
                    //quadrent 3
                    result += quarterTurn * 3;
                }
                else
                {
                    //System.out.println("Q4");
                    //quadrent 4

                    result = quarterTurn - angle;
                    //perfect already
                }
            }
            else
            {
                if (xDelta < 0)
                {
                    //System.out.println("Q2");
                    //quadrent 2
                    result = quarterTurn - angle;
                    result += quarterTurn * 2;
                }
                else
                {
                    //System.out.println("Q1");
                    //quadrent 1
                    result += quarterTurn * 1;
                }
            }
            return (float)result;
        }
        #endregion

        #region Cursor Controls
        public static Rectangle ScreenBounds { get; set; }

        /// <summary>
        /// calc giant square from top left to bottom right
        /// </summary>
        public static void CalcAllSceensBounds()
        {
            int x = 0, y = 0, right = 0, bottom = 0;
            foreach (Screen s in Screen.AllScreens)
            {
                if (s.Bounds.Height > bottom) bottom = s.Bounds.Height;
                if (s.Bounds.Width > right) right = s.Bounds.Width;
                if (s.Bounds.X < x) x = s.Bounds.X;
                if (s.Bounds.Y < y) y = s.Bounds.Y;
            }
            ScreenBounds = new Rectangle(x, y, right - x, bottom - y);
        }

        public static void MoveCursorToCorner(Corner c)
        {
            CalcAllSceensBounds();

            if (c == Corner.Random)
                c = (Corner)Random.Next(0, 4);
            else if (c == Corner.Random_NotBottomRight)
                c = (Corner)Random.Next(0, 3);

            switch(c)
            {
                case Corner.TopLeft:
                    Cursor.Position = new System.Drawing.Point(ScreenBounds.Left, ScreenBounds.Top);
                    break;
                case Corner.TopRight:
                    Cursor.Position = new System.Drawing.Point(ScreenBounds.Right, ScreenBounds.Top);
                    break;
                case Corner.BottomLeft:
                    Cursor.Position = new System.Drawing.Point(ScreenBounds.Left, ScreenBounds.Bottom);
                    break;
                case Corner.BottomRight:
                    Cursor.Position = new System.Drawing.Point(ScreenBounds.Right, ScreenBounds.Bottom);
                    break;
            }

        }

        public enum Corner
        {
            TopLeft=0,
            TopRight = 1,
            BottomLeft = 2,
            BottomRight = 3,
            Random = -1,
            Random_NotBottomRight = -2,//avoid show desktop hover in botom right on windows
        }
        #endregion

        #region LogData / System
        private static object logFileLock = new Object();
        private static string logFileName = @"log.txt";
        private const int LogHistoryCount = 40;
        private static List<string> logRecords = new List<string>(LogHistoryCount);
        public static void Log(string log)
        {
            string timeStamp = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss:ffff tt");
            log = timeStamp + "::" + log;
            AppendLog(log);
            Console.WriteLine(log);

            lock (logFileLock)
            {
                using (StreamWriter sw = (File.Exists(logFileName)) ? File.AppendText(logFileName) : File.CreateText(logFileName))
                {
                    sw.WriteLine(log);
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        public static void PrepLogFile(string logFileFullName)
        {
            logFileName = logFileFullName;
            //rolling logs - move log to next log file up (1 to 2) up to 9
            int logStorageMax = 9;//single digits
            int logIndex = logStorageMax;
            while (logIndex >= 1)
            {
                string logOlder = logFileName.Replace(".txt", "_" + logIndex + ".txt");
                string logNewer;
                if (logIndex > 1)
                    logNewer = logFileName.Replace(".txt", "_" + (logIndex - 1) + ".txt");
                else
                    logNewer = logFileName;
                MoveFileOverwrite(logNewer, logOlder);
                logIndex--;
            }
            //create any necisarry directories for logs
            Directory.CreateDirectory(Path.GetDirectoryName(logFileName));
        }

        public static int GetLogCount()
        {
            return logRecords.Count;
        }

        public static string GetLogData(int count)
        {
            string result = "";
            if (count == -1)//include all
            {
                foreach (string l in logRecords)
                {
                    result += l + "\n";
                }
            }
            else
            {
                if (count > logRecords.Count) count = logRecords.Count;
                for (int x = logRecords.Count - count; x<logRecords.Count; x++)
                {
                    result += logRecords[x] + "\n";
                }
            }
            return result.TrimEnd();
        }

        private static void AppendLog(string log)
        {
            //not a great implementation but it works. Total waste of ReShuffleing RAM when full and removing
            if (logRecords.Count == logRecords.Capacity)
                logRecords.RemoveAt(0);
            logRecords.Add(log);
        }

        public static void MoveFileOverwrite(string src, string dest)
        {
            if(File.Exists(src))
            {
                if (File.Exists(dest))
                    File.Delete(dest);
                File.Move(src, dest);
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
            }
        }
        #endregion
    }

    #region keyboard hooks
    //used implementation from here http://stackoverflow.com/questions/3654787/global-hotkey-in-console-application

    public static class HotKeyManager
    {
        public static event EventHandler<HotKeyEventArgs> HotKeyPressed;

        public static int RegisterHotKey(KeyModifiers modifiers, Keys key)
        {
            _windowReadyEvent.WaitOne();
            int id = System.Threading.Interlocked.Increment(ref _id);
            _wnd.Invoke(new RegisterHotKeyDelegate(RegisterHotKeyInternal), _hwnd, id, (uint)modifiers, (uint)key);
            return id;
        }

        public static void UnregisterHotKey(int id)
        {
            _wnd.Invoke(new UnRegisterHotKeyDelegate(UnRegisterHotKeyInternal), _hwnd, id);
        }

        delegate void RegisterHotKeyDelegate(IntPtr hwnd, int id, uint modifiers, uint key);
        delegate void UnRegisterHotKeyDelegate(IntPtr hwnd, int id);

        private static void RegisterHotKeyInternal(IntPtr hwnd, int id, uint modifiers, uint key)
        {
            RegisterHotKey(hwnd, id, modifiers, key);
        }

        private static void UnRegisterHotKeyInternal(IntPtr hwnd, int id)
        {
            UnregisterHotKey(_hwnd, id);
        }

        private static void OnHotKeyPressed(HotKeyEventArgs e)
        {
            if (HotKeyManager.HotKeyPressed != null)
            {
                HotKeyManager.HotKeyPressed(null, e);
            }
        }

        private static volatile MessageWindow _wnd;
        private static volatile IntPtr _hwnd;
        private static ManualResetEvent _windowReadyEvent = new ManualResetEvent(false);
        static HotKeyManager()
        {
            Thread messageLoop = new Thread(delegate()
            {
                Application.Run(new MessageWindow());
            });
            messageLoop.Name = "MessageLoopThread";
            messageLoop.IsBackground = true;
            messageLoop.Start();
        }

        private class MessageWindow : Form
        {
            public MessageWindow()
            {
                _wnd = this;
                _hwnd = this.Handle;
                _windowReadyEvent.Set();
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    HotKeyEventArgs e = new HotKeyEventArgs(m.LParam, m.WParam);
                    HotKeyManager.OnHotKeyPressed(e);
                }

                base.WndProc(ref m);
            }

            protected override void SetVisibleCore(bool value)
            {
                // Ensure the window never becomes visible
                base.SetVisibleCore(false);
            }

            private const int WM_HOTKEY = 0x312;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private static int _id = 0;
    }


    public class HotKeyEventArgs : EventArgs
    {
        public readonly Keys Key;
        public readonly KeyModifiers Modifiers;
        public readonly int ID;

        public HotKeyEventArgs(Keys key, KeyModifiers modifiers)
        {
            this.Key = key;
            this.Modifiers = modifiers;
            this.ID = -1;
        }

        public HotKeyEventArgs(IntPtr hotKeyParam, IntPtr wParam)
        {
            uint param = (uint)hotKeyParam.ToInt64();
            Key = (Keys)((param & 0xffff0000) >> 16);
            Modifiers = (KeyModifiers)(param & 0x0000ffff);
            this.ID = (int)wParam;
        }
    }

    [Flags]
    public enum KeyModifiers
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8,
        NoRepeat = 0x4000
    }

    #endregion keyboard hooks

    #region Beeps
    public enum BeepPitch { High = 800, Medium = 600, Low = 400 };
    public enum BeepDurration { Short = 150, Medium = 250, Long = 500 };
    #endregion

    #region Choices
    /// <summary>
    /// This is for use with the choice class. NOTE: this is intended for use within a since instance of a choice. multiple use can cause issues with both the ID values and offset used in calculations
    /// </summary>
    struct Possibility
    {
        static int Next_ID;

        public readonly int ID;
        public string Name;
        public float Weight;
        public float Offset;
        public float UpperBound { get { return Weight + Offset; } }

        public Possibility(float weight, string name)
        {
            ID = Next_ID++;
            Name = name;
            Weight = weight;
            Offset = 0;
        }

        public override string ToString()
        {
            return ToStringRange();
        }

        public string ToStringWeight()
        {
            return Name + " - " + Weight;
        }

        public string ToStringRange()
        {
            return Name + ": (" + Offset + " - " + (Weight + Offset) + ")";
        }
    }

    class Choice
    {
        private List<Possibility> possibilities;
        public float TotalWeight = 0;
        public static float LastValue = 0;

        public Choice()
        {
            possibilities = new List<Possibility>();
        }

        public Choice(Possibility pos)
            : base()
        {
            AddPossibility(pos);
        }

        public Possibility AddPossibility(float weight, string name)
        {
            Possibility pos = new Possibility(weight, name);
            return AddPossibility(pos);
        }

        public Possibility AddPossibility(Possibility pos)
        {
            if (pos.Weight > 0)
            {
                pos.Offset = TotalWeight;//offset at end of last choice
                TotalWeight += pos.Weight;
                possibilities.Add(pos);
            }
            return pos;
        }

        public Possibility RandomChoice()
        {
            float val = (float)(GenericsClass.Random.NextDouble() * TotalWeight);
            return GetChoice(val);
        }

        public Possibility GetChoice(float val)
        {
            val = val % TotalWeight;
            LastValue = val;
            foreach (Possibility pos in possibilities)
            {
                if (val >= pos.Offset && val < pos.UpperBound)
                {
                    return pos;
                }
            }

            return new Possibility();
        }


        public string ListPossibilities()
        {
            string result = "";
            foreach (Possibility pos in possibilities)
            {
                result += pos.ToStringRange() + "\n";
            }

            return result;
        }

        public static bool SingleChoice(Possibility pos)
        {
            float val = (float)(GenericsClass.Random.NextDouble() * 1);
            return val < pos.Weight;
        }

        public static bool SingleChoice(float percentage, string name)
        {
            return SingleChoice(new Possibility(percentage, name));
        }
    }
    #endregion

    #region Volume Control
    /// <summary>
    /// This controls the volume of this application but not master volume in windows vista, 7, 8, ?
    /// </summary>
    public class VolumeControl
    {
        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        /// <summary>
        /// Returns the current volume percentage for this application (0-100)
        /// </summary>
        /// <returns></returns>
        public static float GetVolume()
        {
            uint CurrVol = 0;
            // At this point, CurrVol gets assigned the volume
            waveOutGetVolume(IntPtr.Zero, out CurrVol);
            // Calculate the volume
            ushort CalcVol = (ushort)(CurrVol & 0x0000ffff);
            return 100f * CalcVol / ushort.MaxValue;
        }

        /// <summary>
        /// Sets the volume for this application
        /// </summary>
        /// <param name="vol">desired application volume from 0-100</param>
        public static void SetVolume(float vol)
        {
            // Calculate the volume that's being set
            int NewVolume = (int)((vol/100)*ushort.MaxValue);
            // Set the same volume for both the left and the right channels
            uint NewVolumeAllChannels = (((uint)NewVolume & 0x0000ffff) | ((uint)NewVolume << 16));
            // Set the volume
            waveOutSetVolume(IntPtr.Zero, NewVolumeAllChannels);
        }
    }
#endregion

    #region EventScheduler
    public class EventScheduler<T>
    {
        private static object Lock = new Object();
        private List<ScheduledEvent<T>> schedule;

        public ScheduledEvent<T> NextEvent
        {
            get { if (schedule.Count > 0)return schedule[0]; else return null; }
        }

        public EventScheduler()
        {
            lock (Lock)
            {
                schedule = new List<ScheduledEvent<T>>(10);
            }
        }

        public void AddEvent(ScheduledEvent<T> e)
        {
            lock (Lock)
            {
                schedule.Add(e);
                schedule.Sort();
            }
        }

        public void RemoveAllEventsOfType(T t)
        {
            lock (Lock)
            {
                int xx = 0;
                while (xx < schedule.Count)
                {
                    if (schedule[xx].Event.Equals(t))
                        schedule.RemoveAt(xx);
                    else
                        xx++;
                }
            }
        }

        public void ClearSchedule()
        {
            lock (Lock)
            {
                schedule.Clear();
            }
        }

        /// <summary>
        /// Create a new Event to occur in X miliseconds
        /// </summary>
        /// <param name="evnt">Event to occur</param>
        /// <param name="deley">Delay in miliseconds from now</param>
        public void AddEvent(T evnt, int deley, bool removeAllExisting=false)
        {
            if (removeAllExisting)
            {
                RemoveAllEventsOfType(evnt);
            }
            ScheduledEvent<T> newEvent = new ScheduledEvent<T>(DateTime.Now.AddMilliseconds(deley), evnt);
            AddEvent(newEvent);
        }

        public void RemoveNextEvent()
        {
            if (schedule.Count > 0)
            {
                lock (Lock)
                {
                    schedule.RemoveAt(0);
                }
            }
        }

        public override string ToString()
        {
            return GetNextEvents(-1);
        }

        public string GetNextEvents(int eventDisplayLimit)
        {
            //input -1 for all
            int count = 0;
            if(eventDisplayLimit==-1)eventDisplayLimit = schedule.Count;
            string result = "";
            foreach (ScheduledEvent<T> e in schedule)
            {
                result += e.Time.ToString("MM/dd/yyyy hh:mm:ss tt") + " " + e.Event + "\n";
                if (++count >= eventDisplayLimit)
                    break;
            }
            return result.TrimEnd();
        }
    }

    /// <summary>
    /// Data Structure for an Event at a specific time
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ScheduledEvent<T> : IComparable<ScheduledEvent<T>>
    {//this is a class so I can easily adjust the time and return null  if its not found
        public DateTime Time;
        public T Event;

        public ScheduledEvent(DateTime t, T e)
        {
            Time = t;
            Event = e;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            ScheduledEvent<T> objAsScheduledEvent = obj as ScheduledEvent<T>;
            if (objAsScheduledEvent == null) return false;
            else return Equals(objAsScheduledEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();//this is bogus
        }

        public int CompareTo(ScheduledEvent<T> comparePart)
        {
            return this.Time.CompareTo(comparePart.Time);
        }

        public override string ToString()
        {
            //update to show time if today, and time+date if not
            //string timeString = 
            return "ScheduledEvent(" + Event + " at " + Time + ")";
        }
    }
    #endregion

    #region Math Classes
    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public static Vector2 operator +(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector2 operator -(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector2 operator *(Vector2 v1, float m)
        {
            return new Vector2(v1.X * m, v1.Y * m);
        }

        public static float operator *(Vector2 v1, Vector2 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        public static Vector2 operator /(Vector2 v1, float m)
        {
            return new Vector2(v1.X / m, v1.Y / m);
        }

        public static float Distance(Vector2 v1, Vector2 v2)
        {
            return (float)Math.Sqrt(Math.Pow(v1.X - v2.X, 2) + Math.Pow(v1.Y - v2.Y, 2));
        }

        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }
    }
    #endregion

    #region WanderCursor
    public class CursorWanderAI
    {
        private const int touchingCloseness = 4;
        private const int updatesBetweenJitter = 1;
        private int cycle = updatesBetweenJitter - 1;

        protected float speed = 0;
        private float heading = 90;
        private float lastJitterHeading;
        private float targetRadius = 30;
        private float targetDistance = 12;
        private float targetMaxJitter = 5;

        public bool BouceOnBounds = false;

        public CursorWanderAI()
        {
            speed = 0;
            SetHeading(0);
            GenericsClass.CalcAllSceensBounds();
        }

        public CursorWanderAI(float speed, float startHeading)
        {
            this.speed = speed;
            SetHeading(startHeading);
            GenericsClass.CalcAllSceensBounds();
        }

        public void Wander(float delta)
        {
            Vector2 pos = new Vector2(Cursor.Position.X, Cursor.Position.Y);
            cycle++;
            if (cycle == updatesBetweenJitter)
            {
                cycle = 0;
                // SHOULDFIX adjust this code to not convert to/from degrees to
                // radians - if slow
                float targetJitter = (float)(GenericsClass.Random.NextDouble() * targetMaxJitter * 2 - targetMaxJitter);
                lastJitterHeading += targetJitter;
                lastJitterHeading %= 360;
                // dif to jitter on circle around object
                float jitterX1 = (float)(pos.X + Math.Sin((180 - lastJitterHeading) * Math.PI / 180) * targetRadius);
                float jitterY1 = (float)(pos.Y + Math.Cos((180 - lastJitterHeading) * Math.PI / 180) * targetRadius);
                // dif to jitter on circle around ant - shifted along current
                // heading
                float jitterX2 = (float)(jitterX1 + Math.Sin((180 - heading) * Math.PI / 180) * targetDistance);
                float jitterY2 = (float)(jitterY1 + Math.Cos((180 - heading) * Math.PI / 180) * targetDistance);

                float newHeading = GenericsClass.CalcHeading(pos.X, pos.Y, jitterX2, jitterY2, true, true);
                SetHeading(newHeading);
            }
            MoveCursorByHeading(delta, BouceOnBounds);
        }

        protected void MoveCursorByHeading(float delta, bool bounceEnabled=true)
        {
            // move by heading
            Vector2 origin = new Vector2(Cursor.Position.X, Cursor.Position.Y);
            float xSpeed = (float)(Math.Sin((180 - heading) * Math.PI / 180) * speed * delta);
            float ySpeed = (float)(Math.Cos((180 - heading) * Math.PI / 180) * speed * delta);
            
            float newX = origin.X + xSpeed;
            float newY = origin.Y + ySpeed;
            if (bounceEnabled)
            {
                bool hitWall = false;
                float normalAngle = 0;

                if (xSpeed > 0 && CursorTouchingRightWall())
                {
                    normalAngle = 0;
                    hitWall = true;
                }
                else if (xSpeed < 0 && CursorTouchingLeftWall())
                {
                    normalAngle = 180;
                    hitWall = true;
                }
                if (ySpeed > 0 && CursorTouchingBottomWall())
                {
                    normalAngle = 90;
                    hitWall = true;
                }
                else if (ySpeed < 0 && CursorTouchingTopWall())
                {
                    normalAngle = 270;
                    hitWall = true;
                }

                if (hitWall)
                {
                    //TODO evaluate this code - flipping twice???
                    SetHeading((float)Bounce(heading, normalAngle));
                    FlipHeading();
                }
            }
            Cursor.Position = new System.Drawing.Point((int)newX, (int)newY);
        }

        public static bool CursorTouchingRightWall()
        {
            return (GenericsClass.ScreenBounds.Right - Cursor.Position.X) < touchingCloseness;
        }

        public static bool CursorTouchingLeftWall()
        {
            return (Cursor.Position.X - GenericsClass.ScreenBounds.Left) < touchingCloseness;
        }

        public static bool CursorTouchingTopWall()
        {
            return (Cursor.Position.Y - GenericsClass.ScreenBounds.Top) < touchingCloseness;
        }

        public static bool CursorTouchingBottomWall()
        {
            return (GenericsClass.ScreenBounds.Bottom - Cursor.Position.Y) < touchingCloseness;
        }

        public static double Bounce(float headingAngle, float normalAngle)
        {
            double h = GenericsClass.DegreesToRadians(headingAngle);
            double n = GenericsClass.DegreesToRadians(normalAngle);
            double r = n + (n - ((h + Math.PI) % (2 * Math.PI)));
            return GenericsClass.RadiansToDegrees(r);
            // return (float)(normalAngle+(normalAngle-(headingAngle+180)));
        }

        public void FlipHeading()
        {
            SetHeading(heading + 180);
        }

        public void SetHeading(float headingDegrees)
        {
            heading = headingDegrees % 360;
            lastJitterHeading = heading;
        }

        public void SetWanderConfig(float targetRadius, float targetDistance, float targetMaxJitter)
        {
            this.targetRadius = targetRadius;
            this.targetDistance = targetDistance;
            this.targetMaxJitter = targetMaxJitter;
        }

        public void SetSpeed(float speed)
        {
            this.speed = speed;
        }
    }
    #endregion
}
