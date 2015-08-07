using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WorkoutCreator.Extensions;

namespace WorkoutCreator
{
    public partial class MusicForm : Form
    {
        private string loginToken = string.Empty;
        List<DateTime> tapList = new List<DateTime>();
        private const int tapListLength = 20;

        public MusicForm()
        {
            InitializeComponent();

            //LoadSongTypes();
        }

        //private void LoadSongTypes()
        //{
        //    MusicService.MusicServiceSoapClient client = new MusicService.MusicServiceSoapClient();
        //    if ((loginToken = client.Login("muffiz@hotmail.com", "kyr5Seda")) != string.Empty)
        //    {
        //        var songTypes = client.GetSongTypes(loginToken);

        //        songTypes.ForEach(x => musicTypesUnselectedListBox.Items.Add(x));

        //        //musicTypesUnselectedListBox.DataSource = client.GetSongTypes(loginToken);
        //        client.LogOut(loginToken);
        //    }
        //}

        private void tapButton_Click(object sender, EventArgs e)
        {
            bpm2TextBox.Text = "0";

            if (tapList.Count > 0 && (DateTime.Now - tapList[tapList.Count - 1]).TotalSeconds > 2)
                tapList.Clear();

            tapList.Add(DateTime.Now);

            if (tapList.Count > tapListLength)
                tapList.RemoveAt(0);

            if (tapList.Count < 2)
                bpm1TextBox.Text = string.Format("Calculating... {0}", tapListLength - tapList.Count);
            else
                bpm1TextBox.Text = string.Format("{0}", Math.Round(CalculateBPM()));
        }

        private double CalculateBPM()
        {
            TimeSpan ts = tapList[tapList.Count - 1] - tapList[0];

            if (ts.Milliseconds > 0)
            {
                double tempBPM = (60 / (((ts.TotalMilliseconds / 1000) / (tapList.Count))));
                return tempBPM - Math.Floor((tempBPM * 1 / tapList.Count));
            }
            return 0;
        }

        private void doubleButton_Click(object sender, EventArgs e)
        {
            double dbl = bpm1TextBox.Text.ToDouble();

            bpm2TextBox.Text = (2 * dbl).ToString(GetDoubleFormat(bpm1TextBox.Text));
        }


        private string GetDoubleFormat(string originalFormat)
        {
            StringBuilder sb = new StringBuilder();
            double temp = originalFormat.ToDouble(double.MinValue);

            //Value is not a double
            if (temp == double.MinValue)
            {
                return "0";
            }
            else
            {
                int index = 0;
                int found = -1;
                foreach (var ch in originalFormat)
                {
                    if (ch == ',' || ch == '.')
                    {
                        found = index;
                        sb.Append(".");
                    }
                    else
                        sb.Append("0");

                    index += 1;
                }

                if (found != -1)
                    return sb.ToString().Substring(index);
            }

            return sb.ToString().ToDouble().ToString();
        }

        private void halveButton_Click(object sender, EventArgs e)
        {
            double dbl = bpm1TextBox.Text.ToDouble();

            bpm2TextBox.Text = (dbl / 2).ToString(GetDoubleFormat(bpm1TextBox.Text));
        }

        private void musicTypesUnselectedListBox_DoubleClick(object sender, EventArgs e)
        {
            if (musicTypesUnselectedListBox.SelectedItem != null)
            {
                InsertSorted(musicTypesSelectedListBox, musicTypesUnselectedListBox.SelectedItem);
                musicTypesUnselectedListBox.Items.Remove(musicTypesUnselectedListBox.SelectedItem);
            }
        }

        private void musicTypesSelectedListBox_DoubleClick(object sender, EventArgs e)
        {
            if (musicTypesSelectedListBox.SelectedItem != null)
            {
                InsertSorted(musicTypesUnselectedListBox, musicTypesSelectedListBox.SelectedItem);
                musicTypesSelectedListBox.Items.Remove(musicTypesSelectedListBox.SelectedItem);
            }
        }

        private void InsertSorted(ListBox box, object listItem)
        {
            string temp = listItem.ToString();

            for (int i = 0; i < box.Items.Count; i++)
            {
                if (box.Items[i].ToString().CompareTo(temp) > 0)
                {
                    if (i == 0)
                        box.Items.Insert(0, listItem);
                    else
                        box.Items.Insert(i, listItem);

                    return;
                }
            }

            box.Items.Add(listItem);
        }
    }
}
