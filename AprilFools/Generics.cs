﻿using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.IO;
using System.Text;

namespace Generics
{
    public class GenericsClass
    {
        public static Random _random = new Random();

        #region Beeps vaiables and functions
        public static void Beep(BeepPitch p, BeepDurration d)   { Beep((int)p, (int)d); }
        public static void Beep(BeepPitch p, int d)             { Beep((int)p, d); }
        public static void Beep(int p, BeepDurration d)         { Beep(p, (int)d); }
        public static void Beep(int p, int d)                   { Console.Beep(p, d); }

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

        #region Web Crawling
        private const int DOWNLOAD_HTML_TIMEOUT = 1000 * 60 * 5;

        public static string DownloadHTML(string url, string cookie = null, bool reportExceptions = false)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
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
            catch (WebException e)
            {
                if (reportExceptions)
                {
                    Console.WriteLine("DownloadHTML(string,string,bool) - Caught web exception from " + url + " - " + e);
                    return null;
                }
                else
                    throw e;
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

    #endregion keyboard hotkeys

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
            float val = (float)(GenericsClass._random.NextDouble() * TotalWeight);
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
            float val = (float)(GenericsClass._random.NextDouble() * 1);
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

        public void RemoveEventsByType(T t)
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
        public void AddEvent(T evnt, int deley)
        {
            ScheduledEvent<T> newEvent = new ScheduledEvent<T>(DateTime.Now.AddMilliseconds(deley), evnt);
            AddEvent(newEvent);
        }

        public void RemoveNextEvent()
        {
            lock (Lock)
            {
                schedule.RemoveAt(0);
            }
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
}
