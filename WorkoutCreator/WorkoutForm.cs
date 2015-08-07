using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WorkoutCreator.Workout;

namespace WorkoutCreator
{
    public partial class WorkoutForm : Form
    {
        private WorkoutTemplates.WorkoutTemplate template = null;
        private Workout.Workout workout = null;
        private string filePath = null;


        public WorkoutForm()
        {
            InitializeComponent();
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            using (NewWorkoutForm form = new NewWorkoutForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    template = form.Template;

                    workout = Workout.Workout.FromTemplate(template);

                    RefreshWorkout();
                }
            }
        }

        private void RefreshWorkout()
        {
            WorkoutTreeView.SuspendLayout();
            WorkoutTreeView.Nodes.Clear();
            WorkoutTreeView.Nodes.Add(workout.ToTree());
            WorkoutTreeView.ExpandAll();
            WorkoutTreeView.ResumeLayout();

            CreateControls();

            //SetContextMenus(WorkoutTreeView.Nodes[0]);
        }

        private void SetContextMenus(TreeNode parent)
        {
            foreach (TreeNode node in parent.Nodes)
            {
                if (node.Tag is WorkoutPart)
                {
                    node.ContextMenuStrip = workoutPartContextMenuStrip;
                }

                if (node.Tag is WorkoutSong)
                {
                    node.ContextMenuStrip = workoutSongContextMenuStrip;
                }

                SetContextMenus(node);
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.FileName = string.Format("{0}.wko", workout.Title);
                dialog.Filter = "Workout|*.wko|All files|*.*";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string workoutFile = workout.ToString();

                    using (StreamWriter writer = new StreamWriter(dialog.FileName))
                    {
                        writer.Write(workoutFile);

                        writer.Flush();
                        writer.Close();
                    }
                }
            }

            //using (SongForm form = new SongForm())
            //{
            //    if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //    {
            //        WorkoutSong song = form.WorkoutSong;

            //        TextBox txt = new TextBox();
            //        txt.Name = "text";
            //        txt.Multiline = true;
            //        txt.Dock = DockStyle.Fill;
            //        txt.ScrollBars = ScrollBars.Both;
            //        txt.Text = song.ToString();

            //        MainPanel.Controls.Add(txt);
            //    }
            //}

            //WorkoutSong song = new WorkoutSong()
            //{
            //    Title = "Escape",
            //    Artist = "Technomancer",
            //    Album = "Mindspace",
            //    Bpm1 = 138,
            //    FromTime = 82,
            //    ToTime = 205,
            //    OriginalLength = 370
            //};

            //MessageBox.Show(song.ToString());

            //Random r = new Random();
            //MainPanel.Controls.Clear();
            //MainPanel.SuspendLayout();
            //for (int i = 0; i < 20; i++)
            //{
            //    Label l = new Label();
            //    l.Name = string.Format("l{0}", i);
            //    l.Text = string.Format("Panel {0}", i);

            //    Panel p = new Panel();
            //    p.Name = string.Format("panel{0}", i);

            //    p.Dock = DockStyle.Top;
            //    p.AutoSize = true;

            //    p.Controls.Add(l);

            //    MainPanel.Controls.Add(p);

            //    p.BringToFront();
            //}
            //MainPanel.ResumeLayout();
        }

        private void CreateControls()
        {
            MainPanel.SuspendLayout();
            MainPanel.Controls.Clear();

            MainPanel.Controls.Add(CreateLabelPanel("WorkoutTitle", workout.Title, "Arial", 16F, workout));

            foreach (WorkoutPart part in workout.WorkoutParts)
            {
                TimeSpan partSpan = part.GetPartTime();
                //Courier New
                //Arial
                Panel p = CreateLabelPanel("WorkoutPartTitle", string.Format("{0} ({1:00}:{2:00})", part.Title, partSpan.Minutes, partSpan.Seconds), "Arial", 14F, part);
                MainPanel.Controls.Add(p);
                p.BringToFront();

                foreach (WorkoutSong song in part.Songs)
                {
                    Panel s = CreateLabelPanel("WorkoutPartTitle", song.ToDisplayString(), "Arial", 12F, song, 20);
                    MainPanel.Controls.Add(s);
                    s.BringToFront();

                    if (song.Manequins.Count > 0)
                    {
                        Panel pb = CreateImagePanel("Mangeuins", song.Manequins.DrawManequins(75, Color.FromKnownColor(KnownColor.Control)), song, 20);

                        MainPanel.Controls.Add(pb);
                        pb.BringToFront();
                    }
                }
            }

            MainPanel.ResumeLayout();
        }

        private Panel CreateLabelPanel(string name, string text, string fontName, float fontSize, object tag, int left = 0)
        {
            Panel p = new Panel();
            p.Name = string.Format("{0}Panel", name);

            p.Dock = DockStyle.Top;
            p.AutoSize = true;

            Label label = new Label();
            label.Left = left;
            label.AutoSize = true;
            label.Name = string.Format("{0}Label", name);
            label.Text = text;
            label.Font = new System.Drawing.Font(fontName, fontSize);
            p.Controls.Add(label);
            label.Tag = tag;

            label.DoubleClick += new EventHandler(WorkoutLabel_DoubleClick);

            return p;
        }

        private Panel CreateImagePanel(string name, Bitmap bmp, object tag, int left = 0)
        {
            Panel p = new Panel();
            p.Name = string.Format("{0}Panel", name);

            p.Dock = DockStyle.Top;
            p.AutoSize = true;

            PictureBox pb = new PictureBox();
            pb.Name = string.Format("{0}PictureBox", name);
            pb.Image = bmp;
            pb.Tag = tag;
            pb.Left = left;
            pb.Height = bmp.Height;
            pb.Width = bmp.Width;
            p.Controls.Add(pb);


            pb.DoubleClick += new EventHandler(pb_DoubleClick);
            return p;
        }

        private void läggTillLåtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SongForm form = new SongForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    WorkoutPart part = (WorkoutPart)WorkoutTreeView.SelectedNode.Tag;

                    part.Songs.Add(form.WorkoutSong);

                    RefreshWorkout();
                }
            }
        }

        private void WorkoutTreeView_MouseDown(object sender, MouseEventArgs e)
        {
            TreeNode node = WorkoutTreeView.GetNodeAt(e.X, e.Y);

            //MessageBox.Show(string.Format("{0} {1}", e.X, e.Y));
            if (node != null)
            {
                WorkoutTreeView.SelectedNode = node;

                if (e.Button == MouseButtons.Right)
                {
                    if (node.Tag is Workout.Workout)
                    {
                        node.ContextMenuStrip = workoutContextMenuStrip;
                    }
                    else if (node.Tag is WorkoutPart)
                    {
                        node.ContextMenuStrip = workoutPartContextMenuStrip;
                    }
                    else if (node.Tag is WorkoutSong)
                    {
                        node.ContextMenuStrip = workoutSongContextMenuStrip;
                    }
                }
            }

        }

        private void WorkoutTreeView_DoubleClick(object sender, EventArgs e)
        {
            if (WorkoutTreeView.SelectedNode.Tag is WorkoutSong)
            {
                EditSong((WorkoutSong)WorkoutTreeView.SelectedNode.Tag);
            }
        }

        private void EditSong(WorkoutSong workoutSong)
        {
            using (SongForm form = new SongForm())
            {
                form.WorkoutSong = workoutSong;
                form.LoadPlacement(workout);

                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    WorkoutSong song = form.WorkoutSong;

                    workoutSong.Title = song.Title;
                    workoutSong.Artist = song.Artist;
                    workoutSong.Album = song.Album;
                    workoutSong.Bpm1 = song.Bpm1;
                    workoutSong.Bpm2 = song.Bpm2;
                    workoutSong.OriginalLength = song.OriginalLength;
                    workoutSong.FromTime = song.FromTime;
                    workoutSong.ToTime = song.ToTime;
                    workoutSong.Description = song.Description;
                    workoutSong.Manequins = song.Manequins;

                    RefreshWorkout();
                }
            }
        }

        public void WorkoutLabel_DoubleClick(object sender, EventArgs e)
        {
            if (sender is Label)
            {
                Label label = (Label)sender;

                if (label.Tag is WorkoutSong)
                {
                    EditSong((WorkoutSong)label.Tag);
                }

            }
        }

        void pb_DoubleClick(object sender, EventArgs e)
        {
            if (sender is PictureBox)
            {
                PictureBox pb = (PictureBox)sender;

                if (pb.Tag is WorkoutSong)
                {
                    EditSong((WorkoutSong)pb.Tag);
                }
            }
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Workout|*.wko|All files|*.*";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    workout = Workout.Workout.LoadWorkout(dialog.FileName);

                    RefreshWorkout();
                }
            }
        }

        private void saveAsButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Exportera pass till hemsida";


                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(string.Format("{0}.html", Path.Combine(dialog.SelectedPath, workout.Title))))
                    {
                        sw.Write(workout.ToHtml());

                        workout.SaveImages(dialog.SelectedPath, 100, Color.White);
                    }

                    //string workoutHtml = workout.ToHtml();

                    //MainPanel.Controls.Add(CreateLabelPanel("html", workoutHtml, "Arial", 10F, null));

                    //Clipboard.SetText(workoutHtml);
                }
            }
        }
    }
}
