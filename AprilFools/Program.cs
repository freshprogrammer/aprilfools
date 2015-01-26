using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Windows.Forms;
using System.Media;

namespace AprilFools
{
    /*
     * --ideas
     * program wil launch and sit silently making noice every hour
     * make a accelerating beeping noise perioticly
     * start odd windows (like ads)
     * popup random error mssages for chrome & memmory
     * 
     */

    class Program
    {
        const bool _TESTING = true;

        public static Random _random = new Random();

        public static int _startupDelaySeconds = 0;
        public static int _totalDurationSeconds = 5;

        /// <summary>
        /// Entry point for prank application
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("April Fools Prank by: Dougie Fresh");

            // Check for command line arguments and assign the new values
            if (args.Length >= 2)
            {
                _startupDelaySeconds = Convert.ToInt32(args[0]);
                _totalDurationSeconds = Convert.ToInt32(args[1]);
            }

            if (_TESTING)
            {
                DialogResult r = MessageBox.Show(
                    "Running in testing mode. Press OK to start.",
                    "\"The\" App",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.None);

                if (r == DialogResult.Cancel)
                    return;
            }

            // Create all threads that manipulate all of the inputs and outputs to the system
            Thread drunkMouseThread = new Thread(new ThreadStart(DrunkMouseThread));
            Thread drunkKeyboardThread = new Thread(new ThreadStart(DrunkKeyboardThread));
            Thread drunkSoundThread = new Thread(new ThreadStart(DrunkSoundThread));
            Thread drunkPopupThread = new Thread(new ThreadStart(DrunkPopupThread));

            DateTime future = DateTime.Now.AddSeconds(_startupDelaySeconds);
            Console.WriteLine("Waiting " + _startupDelaySeconds + " seconds before starting threads");
            while (future > DateTime.Now)
            {
                Thread.Sleep(500);
            }
            Console.WriteLine("starting");



            // Start all of the threads
            //drunkMouseThread.Start();
            //drunkKeyboardThread.Start();
            //drunkSoundThread.Start();
            //drunkPopupThread.Start();

            if (_totalDurationSeconds > 0)
            {
                future = DateTime.Now.AddSeconds(_totalDurationSeconds);
                while (future > DateTime.Now)
                {
                    Thread.Sleep(500);
                }
            }

            Console.WriteLine("Terminating all threads");

            // Kill all threads and exit application
            drunkMouseThread.Abort();
            drunkKeyboardThread.Abort();
            drunkSoundThread.Abort();
            drunkPopupThread.Abort();
        }

        #region Thread Functions
        /// <summary>
        /// This thread will randomly affect the mouse movements to screw with the end user
        /// </summary>
        public static void DrunkMouseThread()
        {
            Console.WriteLine("DrunkMouseThread Started");

            int moveX = 0;
            int moveY = 0;

            while (true)
            {
                // Console.WriteLine(Cursor.Position.ToString());

                if (_random.Next(100) > 50)
                {
                    // Generate the random amount to move the cursor on X and Y
                    moveX = _random.Next(20) - 10;
                    moveY = _random.Next(20) - 10;

                    // Change mouse cursor position to new random coordinates
                    Cursor.Position = new System.Drawing.Point(
                        Cursor.Position.X + moveX,
                        Cursor.Position.Y + moveY);
                }

                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// This will generate random keyboard output to screw with the end user
        /// </summary>
        public static void DrunkKeyboardThread()
        {
            Console.WriteLine("DrunkKeyboardThread Started");

            while (true)
            {
                if (_random.Next(100) > 95)
                {
                    // Generate a random capitol letter
                    char key = (char)(_random.Next(25) + 65);

                    // 50/50 make it lower case
                    if (_random.Next(2) == 0)
                    {
                        key = Char.ToLower(key);
                    }

                    SendKeys.SendWait(key.ToString());
                }

                Thread.Sleep(_random.Next(500));
            }
        }

        /// <summary>
        /// This will play system sounds at random to screw with the end user
        /// </summary>
        public static void DrunkSoundThread()
        {
            Console.WriteLine("DrunkSoundThread Started");

            while (true)
            {
                // Determine if we're going to play a sound this time through the loop (20% odds)
                //if (_random.Next(100) > 80)
                bool rndSound = true;
                if (rndSound)
                {
                    // Randomly select a system sound
                    int sound = _random.Next(5);
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
                    Thread.Sleep(500);
                }
                else
                {
                    Console.Beep(400, 1000);
                    Thread.Sleep(101);
                }
            }
        }

        /// <summary>
        /// This thread will popup fake error notifications to make the user go crazy and pull their hair out
        /// </summary>
        public static void DrunkPopupThread()
        {
            Console.WriteLine("DrunkPopupThread Started");

            const int popupInterval = 1000 * 60 * 90;//90 minutes
            const int popupIntervalVariance = 1000 * 60 * 10;//10 minutes +/-

            while (true)
            {
                // Every 10 seconds roll the dice and 10% of the time show a dialog
                if (_random.Next(100) > 90)
                {
                    // Determine which message to show user
                    switch (_random.Next(2))
                    {
                        case 0:
                            MessageBox.Show(
                               "Chrome is dangerously low on resources.",
                                "Chrome",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                            break;
                        case 1:
                            MessageBox.Show(
                               "Your system is running low on resources",
                                "Microsoft Windows",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                            break;
                    }
                }

                int variance = _random.Next(popupIntervalVariance * 2) - popupIntervalVariance * 2 - popupIntervalVariance; //*2 for +/-
                Thread.Sleep(popupInterval + variance);
            }
        }
        #endregion
    }

    struct Possibility
    {
        static int Next_ID;

        public int ID;
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
            float val = (float)(Program._random.NextDouble() * TotalWeight);
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
            float val = (float)(Program._random.NextDouble() * 1);
            return val < pos.Weight;
        }

        public static bool SingleChoice(float percentage, string name)
        {
            return SingleChoice(new Possibility(percentage, name));
        }
    }
}