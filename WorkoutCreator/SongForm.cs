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
    public partial class SongForm : Form
    {
        private SongManequins Manequins
        {
            get
            {
                return workoutSong.Manequins;
            }
            set
            {
                workoutSong.Manequins = value;
            }
        }

        private Workout.Workout workout;
        public Workout.Workout WorkoutInfo
        {
            get
            {
                return workout;
            }
            set
            {
                workout = value;
            }
        }

        private Workout.WorkoutSong currentSong = null;
        private Workout.WorkoutPart currentPart = null;

        public void LoadPlacement(WorkoutCreator.Workout.Workout workout)
        {
            this.workout = workout;

            workoutPartComboBox.Items.Clear();
            workoutPartComboBox.DisplayMember = "Title";
            workoutPartComboBox.ValueMember = "Title";

            fromToComboBox.Items.Clear();

            fromToComboBox.DisplayMember = "Title";
            fromToComboBox.ValueMember = "PlacementValue";

            //fromToComboBox.Items.Add(new SongPlacement() { PlacementValue = 0, Title = "Flytta inte" });
            fromToComboBox.Items.Add(new SongPlacement() { PlacementValue = 1, Title = "Före vald låt" });
            fromToComboBox.Items.Add(new SongPlacement() { PlacementValue = 2, Title = "Efter vald låt" });
            fromToComboBox.SelectedIndex = 0;

            foreach (Workout.WorkoutPart part in workout.WorkoutParts)
            {
                workoutPartComboBox.Items.Add(part);

                Workout.WorkoutSong song = part.Songs.FirstOrDefault(x =>
                    x.Album == albumTextBox.Text &&
                    x.Artist == artistTextBox.Text &&
                    x.Bpm1 == bpm1TextBox.Text.ToInt() &&
                    x.Bpm2 == bpm2TextBox.Text.ToInt() &&
                    x.Description == descriptionTextBox.Text &&
                    x.FromTime == fromTimeTextBox.TotalSeconds &&
                    x.ToTime == toTimeTextBox.TotalSeconds &&
                    x.Title == titleTextBox.Text);

                if (song != null)
                {
                    currentSong = song;
                    currentPart = part;
                    workoutPartComboBox.SelectedItem = part;
                }
            }



        }

        private Workout.WorkoutSong workoutSong;
        public Workout.WorkoutSong WorkoutSong
        {
            get
            {
                workoutSong.Title = titleTextBox.Text;
                workoutSong.Artist = artistTextBox.Text;
                workoutSong.Album = albumTextBox.Text;
                workoutSong.Bpm1 = bpm1TextBox.Text.ToInt();
                workoutSong.Bpm2 = bpm2TextBox.Text.ToInt();
                workoutSong.OriginalLength = originalLenghtTimeTextBox.TotalSeconds;
                workoutSong.FromTime = fromTimeTextBox.TotalSeconds;
                workoutSong.ToTime = toTimeTextBox.TotalSeconds;
                workoutSong.Description = descriptionTextBox.Text;
                return workoutSong;
            }
            set
            {
                workoutSong.Manequins = value.Manequins;

                if (manequinsPictureBox.Image != null)
                    manequinsPictureBox.Image.Dispose();

                manequinsPictureBox.Image = workoutSong.Manequins.DrawManequins();

                titleTextBox.Text = value.Title;
                artistTextBox.Text = value.Artist;
                albumTextBox.Text = value.Album;
                bpm1TextBox.Text = value.Bpm1.ToString();
                bpm2TextBox.Text = value.Bpm2 == 0 ? string.Empty : value.Bpm2.ToString();
                originalLenghtTimeTextBox.TotalSeconds = value.OriginalLength;
                fromTimeTextBox.TotalSeconds = value.FromTime;
                toTimeTextBox.TotalSeconds = value.ToTime;
            }
        }

        List<DateTime> tapList = new List<DateTime>();
        private const int tapListLength = 20;

        public SongForm()
        {
            InitializeComponent();

            workoutSong = new Workout.WorkoutSong();
            Manequins = workoutSong.Manequins;
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

        private void doubleButton_Click(object sender, EventArgs e)
        {
            double dbl = bpm1TextBox.Text.ToDouble();

            bpm2TextBox.Text = (2 * dbl).ToString(GetDoubleFormat(bpm1TextBox.Text));
        }

        private void halveButton_Click(object sender, EventArgs e)
        {
            double dbl = bpm1TextBox.Text.ToDouble();

            bpm2TextBox.Text = (dbl / 2).ToString(GetDoubleFormat(bpm1TextBox.Text));
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

        private void manequinButton_Click(object sender, EventArgs e)
        {
            using (ManequinForm form = new ManequinForm(Manequins))
            {

                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Manequins = form.Manequins;
                    if (manequinsPictureBox.Image != null)
                        manequinsPictureBox.Image.Dispose();
                    manequinsPictureBox.Image = Manequins.DrawManequins();
                }
            }
        }

        private void workoutPartComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Workout.WorkoutPart part = ((Workout.WorkoutPart)((ComboBox)sender).SelectedItem);

            songComboBox.Items.Clear();
            songComboBox.DisplayMember = "Title";

            songComboBox.Items.AddRange(part.Songs.ToArray<object>());

            if (songComboBox.Items.Contains(currentSong))
            {
                songComboBox.SelectedItem = currentSong;
            }
            else
            {
                if (songComboBox.Items.Count > 0)
                    songComboBox.SelectedIndex = 0;
            }
        }

        private void moveSongButton_Click(object sender, EventArgs e)
        {
            int selectedValue = ((SongPlacement)fromToComboBox.SelectedItem).PlacementValue;

            if (selectedValue == 0)
            {
                //Dont move
            }
            else if (selectedValue == 1)
            {
                //Move to before
                if (currentPart != null && currentSong != null)
                {
                    Workout.WorkoutPart part = ((Workout.WorkoutPart)workoutPartComboBox.SelectedItem);
                    Workout.WorkoutSong song = ((Workout.WorkoutSong)songComboBox.SelectedItem);

                    int currentIndex = part.Songs.IndexOf(song);

                    int currentPlacement = currentPart.Songs.IndexOf(currentSong);

                    currentPart.Songs.Remove(currentSong);

                    if (currentPart == part)
                    {
                        if (currentPlacement < currentIndex)
                        {
                            part.Songs.Insert(currentIndex - 1, currentSong);
                        }
                        else
                        {
                            part.Songs.Insert(currentIndex, currentSong);
                        }
                    }
                    else
                    {
                        if (currentIndex == -1)
                            part.Songs.Add(currentSong);
                        else
                            part.Songs.Insert(currentIndex, currentSong);
                    }
                    currentPart = part;

                    workoutPartComboBox_SelectedIndexChanged(workoutPartComboBox, EventArgs.Empty);
                }
            }
            else
            {

                Workout.WorkoutPart part = ((Workout.WorkoutPart)workoutPartComboBox.SelectedItem);
                Workout.WorkoutSong song = ((Workout.WorkoutSong)songComboBox.SelectedItem);

                int currentIndex = part.Songs.IndexOf(song);

                currentPart.Songs.Remove(currentSong);

                if (part == currentPart)
                {
                    if (currentIndex >= part.Songs.Count)
                        part.Songs.Add(currentSong);
                    else
                        part.Songs.Insert(currentIndex, currentSong);
                }
                else
                {
                    if (currentIndex >= part.Songs.Count)
                        part.Songs.Add(currentSong);
                    else
                        part.Songs.Insert(currentIndex + 1, currentSong);
                }
                currentPart = part;

                workoutPartComboBox_SelectedIndexChanged(workoutPartComboBox, EventArgs.Empty);
            }
        }

    }

    public class SongPlacement
    {
        public int PlacementValue { get; set; }
        public string Title { get; set; }
    }
}
