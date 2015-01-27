using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

namespace Generics
{
    #region BeepsGeneric vaiables and functions
    public class GenericsClass
    {
        public static Random _random = new Random();

        public static void Beep(BeepPitch p, BeepDurration d) { Console.Beep((int)p, (int)d); }
    }
    #endregion

    #region Keyboard Hooks
    //http://stackoverflow.com/questions/2450373/set-global-hotkeys-using-c-sharp
    public sealed class KeyboardHook : IDisposable
    {
        // Registers a hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        // Unregisters the hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        /// Represents the window that is used internally to get the messages.
        /// </summary>
        private class Window : NativeWindow, IDisposable
        {
            private static int WM_HOTKEY = 0x0312;

            public Window()
            {
                // create the handle for the window.
                this.CreateHandle(new CreateParams());
            }

            /// <summary>
            /// Overridden to get the notifications.
            /// </summary>
            /// <param name="m"></param>
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                // check if we got a hot key pressed.
                if (m.Msg == WM_HOTKEY)
                {
                    // get the keys.
                    Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                    ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                    // invoke the event to notify the parent.
                    if (KeyPressed != null)
                        KeyPressed(this, new KeyPressedEventArgs(modifier, key));
                }
            }

            public event EventHandler<KeyPressedEventArgs> KeyPressed;

            #region IDisposable Members

            public void Dispose()
            {
                this.DestroyHandle();
            }

            #endregion
        }

        private Window _window = new Window();
        private int _currentId;

        public KeyboardHook()
        {
            // register the event of the inner native window.
            _window.KeyPressed += delegate(object sender, KeyPressedEventArgs args)
            {
                if (KeyPressed != null)
                    KeyPressed(this, args);
            };
        }

        /// <summary>
        /// Registers a hot key in the system.
        /// </summary>
        /// <param name="modifier">The modifiers that are associated with the hot key.</param>
        /// <param name="key">The key itself that is associated with the hot key.</param>
        public void RegisterHotKey(ModifierKeys modifier, Keys key)
        {
            // increment the counter.
            _currentId = _currentId + 1;

            // register the hot key.
            if (!RegisterHotKey(_window.Handle, _currentId, (uint)modifier, (uint)key))
                throw new InvalidOperationException("Couldn’t register the hot key.");
        }

        /// <summary>
        /// A hot key has been pressed.
        /// </summary>
        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        #region IDisposable Members

        public void Dispose()
        {
            // unregister all the registered hot keys.
            for (int i = _currentId; i > 0; i--)
            {
                UnregisterHotKey(_window.Handle, i);
            }

            // dispose the inner native window.
            _window.Dispose();
        }

        #endregion
    }

    /// <summary>
    /// Event Args for the event that is fired after the hot key has been pressed.
    /// </summary>
    public class KeyPressedEventArgs : EventArgs
    {
        private ModifierKeys _modifier;
        private Keys _key;

        internal KeyPressedEventArgs(ModifierKeys modifier, Keys key)
        {
            _modifier = modifier;
            _key = key;
        }

        public ModifierKeys Modifier
        {
            get { return _modifier; }
        }

        public Keys Key
        {
            get { return _key; }
        }
    }

    /// <summary>
    /// The enumeration of possible modifiers.
    /// </summary>
    [Flags]
    public enum ModifierKeys : uint
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }
    #endregion

    #region Beeps
    public enum BeepPitch { High = 800, Medium = 600, Low = 400 };
    public enum BeepDurration { Shrt = 150, Medium = 250, Long = 500 };
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
}