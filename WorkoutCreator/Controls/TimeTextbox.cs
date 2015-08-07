using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WorkoutCreator.Extensions;

namespace WorkoutCreator.Controls
{
    public partial class TimeTextbox : UserControl
    {
        public int Minutes { get; set; }
        public int Seconds { get; set; }

        public int TotalSeconds
        {
            get
            {
                return (Minutes * 60) + Seconds;
            }
            set
            {
                int temp = value % 60;

                Minutes = value / 60;
                Seconds = temp;

                timeDisplayTextBox.Text = GetDisplayString();
            }
        }

        public TimeTextbox()
        {
            InitializeComponent();

            Minutes = 0;
            Seconds = 0;

            timeDisplayTextBox.Text = GetDisplayString();
        }

        private void TimeTextBox_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private void timeDisplayTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            //int currentPos = timeDisplayTextBox.SelectionStart;

            //if (e.KeyCode == Keys.Left)
            //{
            //    //timeDisplayTextBox.Select(0, 2);
            //}

            //if (e.KeyCode == Keys.Right)
            //{
            //    //timeDisplayTextBox.Select(3, 2);
            //}

            //if (e.KeyCode == Keys.Up)
            //{
            //    if (timeDisplayTextBox.SelectionStart < 2)
            //    {
            //        timeDisplayTextBox.Text = GetDisplayString();

            //        timeDisplayTextBox.Select(0, 2);
            //    }
            //    else
            //    {
            //        timeDisplayTextBox.Text = GetDisplayString();

            //        timeDisplayTextBox.Select(3, 2);
            //    }
            //}

            //if (e.KeyCode == Keys.Down)
            //{
            //    if (timeDisplayTextBox.SelectionStart < 2)
            //    {
            //        timeDisplayTextBox.Text = GetDisplayString();

            //        timeDisplayTextBox.Select(0, 2);
            //    }
            //    else
            //    {
            //        timeDisplayTextBox.Text = GetDisplayString();

            //        timeDisplayTextBox.Select(3, 2);
            //    }
            //}
        }

        private string GetDisplayString()
        {
            return string.Format("{0:00}:{1:00}", Minutes, Seconds);
        }

        private void timeDisplayTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            int currentPos = timeDisplayTextBox.SelectionStart;

            string[] splitString = timeDisplayTextBox.Text.Split(':');

            if (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0 ||
                e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1 ||
                e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2 ||
                e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3 ||
                e.KeyCode == Keys.D4 || e.KeyCode == Keys.NumPad4 ||
                e.KeyCode == Keys.D5 || e.KeyCode == Keys.NumPad5 ||
                e.KeyCode == Keys.D6 || e.KeyCode == Keys.NumPad6 ||
                e.KeyCode == Keys.D7 || e.KeyCode == Keys.NumPad7 ||
                e.KeyCode == Keys.D8 || e.KeyCode == Keys.NumPad8 ||
                e.KeyCode == Keys.D9 || e.KeyCode == Keys.NumPad9 ||
                e.KeyCode == Keys.Back ||
                e.KeyCode == Keys.Delete)
            {
                SetValue(e.KeyCode);
                timeDisplayTextBox.Text = GetDisplayString();

                if (currentPos + 1 == 2)
                    timeDisplayTextBox.SelectionStart = currentPos + 2;
                else
                    timeDisplayTextBox.SelectionStart = currentPos + 1;
                
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (timeDisplayTextBox.SelectionStart < 3)
                {
                    Minutes = splitString[0].ToInt() + 1;

                    if (Minutes > 99)
                        Minutes = 0;
                }
                else
                {
                    Seconds = splitString[1].ToInt() + 1;

                    if (Seconds > 59)
                        Seconds = 0;
                }

                timeDisplayTextBox.Text = GetDisplayString();

                timeDisplayTextBox.SelectionStart = currentPos;

                e.SuppressKeyPress = true;

                //timeDisplayTextBox.SelectionStart = currentPos;
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (timeDisplayTextBox.SelectionStart < 3)
                {
                    Minutes = splitString[0].ToInt() - 1;

                    if (Minutes < 0)
                        Minutes = 99;
                }
                else
                {
                    Seconds = splitString[1].ToInt() - 1;

                    if (Seconds < 0)
                        Seconds = 59;
                }

                timeDisplayTextBox.Text = GetDisplayString();

                timeDisplayTextBox.SelectionStart = currentPos;

                e.SuppressKeyPress = true;
            }
            else
            {
                e.SuppressKeyPress = true;
            }
        }

        private void SetValue(Keys key)
        {
            if (key == Keys.D0 || key == Keys.NumPad0)
                SetValueInPlace(0);

            if (key == Keys.D1 || key == Keys.NumPad1)
            {
                SetValueInPlace(1);
            }
            if (key == Keys.D2 || key == Keys.NumPad2)
            {
                SetValueInPlace(2);
            }
            if (key == Keys.D3 || key == Keys.NumPad3)
            {
                SetValueInPlace(3);
            }
            if (key == Keys.D4 || key == Keys.NumPad4)
            {
                SetValueInPlace(4);
            }
            if (key == Keys.D5 || key == Keys.NumPad5)
            {
                SetValueInPlace(5);
            }
            if (key == Keys.D6 || key == Keys.NumPad6)
            {
                SetValueInPlace(6);
            }
            if (key == Keys.D7 || key == Keys.NumPad7)
            {
                SetValueInPlace(7);
            }
            if (key == Keys.D8 || key == Keys.NumPad8)
            {
                SetValueInPlace(8);
            }
            if (key == Keys.D9 || key == Keys.NumPad9)
            {
                SetValueInPlace(9);
            }
        }

        private void SetValueInPlace(int value)
        {
            string[] splitString = timeDisplayTextBox.Text.Split(':');

            int minutes = splitString[0].ToInt();
            int seconds = splitString[1].ToInt();

            int tempMin = minutes % 10;
            int tempSec = seconds % 10;

            switch (timeDisplayTextBox.SelectionStart)
            {
                case 0:
                    Minutes = value * 10 + tempMin;
                    break;
                case 1:
                case 2:
                    Minutes = Minutes - tempMin + value;
                    break;
                case 3:
                    Seconds = value * 10 + tempSec;
                    break;
                case 4:
                case 5:
                    Seconds = Seconds - tempSec + value;
                    break;
            }

            if (Seconds > 59)
                Seconds = 59;
        }

    }
}
