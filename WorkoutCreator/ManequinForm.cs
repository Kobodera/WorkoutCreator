using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WorkoutCreator
{
    public partial class ManequinForm : Form
    {
        public SongManequins Manequins { get; set; }

        Shake shake = null;

        //List<Manequin> manequins = new List<Manequin>();
        string songName;
        string fileName = string.Empty;

        private TreeNode lastManequinNode;

        bool drawShake = false;
        bool editShake = false;

        public ManequinForm(string songName)
        {
            InitializeComponent();

            this.songName = songName;
            Manequins = new SongManequins(songName);

            ClearAll();
        }

        public ManequinForm(SongManequins manequins)
            : this(manequins, manequins.SongName)
        {

        }

        public ManequinForm(SongManequins manequins, string songName)
            : this(songName)
        {
            //InitializeComponent();

            Manequins.LoadValues(manequins.ToString());

            //Manequins = manequins;
            //this.songName = songName;

            manequinTreeView.Nodes.Clear();
            manequinTreeView.Nodes.Add(Manequins.GetManequinTree());
            manequinTreeView.SelectedNode = manequinTreeView.Nodes[0].Nodes[0].Nodes[0];

            manequinPictureBox.Image = Manequins[0].DrawManequin(200, 200);
            manequinPictureBox.Refresh();

            DrawManequins();
        }

        private void DrawManequins()
        {
            if (manequinsPictureBox.Image != null)
            {
                manequinsPictureBox.Image.Dispose();
                manequinsPictureBox.Image = null;
            }

            manequinsPictureBox.Image = Manequins.DrawManequins();
        }

        private void manequinPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (drawShake || editShake)
            {
                //Do nothing
            }
            else
            {
                if (manequinTreeView.SelectedNode != null)
                {
                    if (manequinTreeView.SelectedNode.Tag is Shake)
                        manequinTreeView.SelectedNode = lastManequinNode;

                    TreeNode temp = manequinTreeView.SelectedNode;
                    if (temp.Tag is ManequinPoint)
                    {
                        ManequinPoint point = (ManequinPoint)manequinTreeView.SelectedNode.Tag;

                        point.XRel = (double)e.X / manequinPictureBox.Width;
                        point.YRel = (double)e.Y / manequinPictureBox.Height;
                    }


                    while (temp != null)
                    {
                        temp = temp.Parent;

                        if (temp == null)
                            break;

                        if (temp.Tag is Manequin)
                        {
                            manequinPictureBox.Image.Dispose();
                            manequinPictureBox.Image = null;
                            manequinPictureBox.Image = ((Manequin)temp.Tag).DrawManequin(manequinPictureBox.Height, manequinPictureBox.Width);
                        }
                    }

                    SetNextNode(manequinTreeView.SelectedNode, manequinTreeView.SelectedNode.Index);

                    DrawManequins();
                }
            }
        }

        private void SetNextNode(TreeNode current, int index)
        {
            Manequin m = GetManequin(current);

            if (m == null)
                return;

            if (current.Tag is ManequinPoint)
            {
                manequinTreeView.SelectedNode = m.GetNextPoint(((ManequinPoint)current.Tag).PartId).PartNode;
            }

            lastManequinNode = manequinTreeView.SelectedNode;
        }

        private Manequin GetManequin(TreeNode current)
        {
            TreeNode temp = current;

            while (temp != null)
            {
                if (temp.Tag == null)
                    temp = temp.Nodes[0];

                if (temp.Tag is Manequin)
                    return (Manequin)temp.Tag;

                temp = temp.Parent;
            }

            return null;
        }

        private object GetManequinOrShake(TreeNode current)
        {
            TreeNode temp = current;

            while (temp != null)
            {
                if (temp.Tag == null)
                    temp = temp.Nodes[0];

                if (temp.Tag is Manequin)
                    return temp.Tag;

                if (temp.Tag is Shake)
                    return temp.Tag;

                temp = temp.Parent;
            }

            return null;
        }

        private TreeNode GetManequinNode(TreeNode current)
        {
            TreeNode temp = current;

            while (temp != null)
            {
                if (temp.Tag == null)
                    temp = temp.Nodes[0];

                if (temp.Tag is Manequin)
                    return temp;

                temp = temp.Parent;
            }

            return null;
        }

        private void nyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("VARNING! Om du väljer ny så kommer alla gubbar att tas bort! Är du säker på att du vill göra om allt från början?", "Ny", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                ClearAll();
        }

        private void manequinTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is ManequinPoint)
            {
                editShake = false;
                lastManequinNode = e.Node;
            }
            else if (e.Node.Tag is Shake)
            {
                editShake = true;
                shake = (Shake)e.Node.Tag;
            }


            Refresh(e.Node);
        }

        private void Refresh(TreeNode node)
        {
            Manequin m = GetManequin(node);

            if (m != null)
            {
                manequinTextBox.Text = m.ManequinText;

                manequinPictureBox.Image.Dispose();
                manequinPictureBox.Image = null;
                Bitmap bmp = m.DrawManequin(manequinPictureBox.Height, manequinPictureBox.Width);
                if (shake != null)
                    shake.Draw(bmp);
                manequinPictureBox.Image = bmp;
            }

            DrawManequins();
        }

        private void rensaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearAll();
        }

        private void ClearAll()
        {
            Manequins.Clear();
            manequinTreeView.Nodes.Clear();

            manequinTreeView.Nodes.Add(Manequins.GetManequinTree());
            manequinTreeView.SelectedNode = manequinTreeView.Nodes[0].Nodes[0].Nodes[0];

            manequinPictureBox.Image = Manequins[0].DrawManequin(200, 200);
            manequinPictureBox.Refresh();

            DrawManequins();

        }

        private void läggTillGubbeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Manequin m = new Manequin();
            Manequins.Add(m);
            TreeNode manequinTreeNode = m.GetManequinTree(string.Format("Gubbe {0}", Manequins.Count));
            manequinTreeView.Nodes[0].Nodes.Add(manequinTreeNode);
            manequinTreeView.SelectedNode = manequinTreeNode.Nodes[0];
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Manequin m = GetManequin(manequinTreeView.SelectedNode);

            if (m != null)
                m.ManequinText = manequinTextBox.Text;

            Refresh(manequinTreeView.SelectedNode);
        }

        private void clearCurrentButton_Click(object sender, EventArgs e)
        {
            Manequin m = GetManequin(manequinTreeView.SelectedNode);

            if (m != null)
            {
                m.Clear();
                Refresh(manequinTreeView.SelectedNode);
                manequinTextBox.Text = string.Empty;
                manequinTreeView.SelectedNode = m.GetPoint("Head").PartNode;
            }
        }

        private void addManequinButton_Click(object sender, EventArgs e)
        {
            Manequin m = new Manequin();
            Manequins.Add(m);
            TreeNode manequinTreeNode = m.GetManequinTree(string.Format("Gubbe {0}", Manequins.Count));
            manequinTreeView.Nodes[0].Nodes.Add(manequinTreeNode);
            manequinTreeView.SelectedNode = manequinTreeNode.Nodes[0];
        }

        private void manequinTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                object obj = GetManequinOrShake(manequinTreeView.SelectedNode);

                if (obj != null)
                {
                    if (obj is Manequin)
                        Manequins.Delete((Manequin)obj);
                    if (obj is Shake)
                    {
                        Manequin manequin = GetManequin(manequinTreeView.SelectedNode);

                        if (manequin != null)
                            manequin.Delete((Shake)obj);
                    }
                }
                manequinTreeView.Nodes.Clear();
                TreeNode manequinNode = Manequins.GetManequinTree();
                manequinTreeView.Nodes.Add(manequinNode);
                manequinTreeView.SelectedNode = manequinNode.Nodes[0].Nodes[0];

            }
        }

        private void öppnaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Sträckgubbar|*.wki|Alla filer|*.*";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Manequins = SongManequins.Open(dialog.FileName);

                    manequinTreeView.Nodes.Clear();
                    manequinTreeView.Nodes.Add(Manequins.GetManequinTree());
                    manequinTreeView.SelectedNode = manequinTreeView.Nodes[0].Nodes[0].Nodes[0];

                    manequinPictureBox.Image = Manequins[0].DrawManequin(200, 200);
                    manequinPictureBox.Refresh();

                    DrawManequins();
                }
            }
        }

        private void sparaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                sparaSomToolStripMenuItem_Click(sender, e);
            }
            else
            {
                Manequins.Save(fileName);
            }
        }

        private void sparaSomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //textBox1.Text = manequins.ToString();

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.FileName = string.Format("{0}.wki", songName);
                dialog.Filter = "Sträckgubbe|*.wki|Alla filer|*.*";

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    fileName = dialog.FileName;
                    Manequins.Save(dialog.FileName);
                }
            }
        }

        private void exporteraSomBildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "PNG|*.png|Alla filer|*.*";
                dialog.FileName = string.Format("{0}.png", songName);

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Bitmap bmp = Manequins.DrawManequins();

                    bmp.Save(dialog.FileName, ImageFormat.Png);
                }
            }
        }

        private void addShakeButton_Click(object sender, EventArgs e)
        {
            Manequin m = GetManequin(manequinTreeView.SelectedNode);

            shake = new Shake();
            drawShake = true;
        }

        private void manequinPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (drawShake)
            {
                drawShake = false;

                if (shake != null)
                {
                    shake.DestinationRelX = ((float)e.X) / manequinPictureBox.Width;
                    shake.DestinationRelY = ((float)e.Y) / manequinPictureBox.Height;

                    Manequin m = GetManequin(manequinTreeView.SelectedNode);
                    m.AddShake(shake);

                    shake = null;
                }

                Refresh(manequinTreeView.SelectedNode);
            }
            else if (editShake)
            {
                editShake = false;

                if (shake != null)
                {
                    manequinTreeView.SelectedNode.Tag = shake;

                    shake.DestinationRelX = ((float)e.X) / manequinPictureBox.Width;
                    shake.DestinationRelY = ((float)e.Y) / manequinPictureBox.Height;

                    shake = null;

                    Refresh(manequinTreeView.SelectedNode);
                }
            }
        }

        private void manequinPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (drawShake || editShake)
            {
                if (shake == null)
                    shake = new Shake();

                shake.OriginRelX = ((float)e.X) / manequinPictureBox.Width;
                shake.OriginRelY = ((float)e.Y) / manequinPictureBox.Height;

                if (editShake)
                {
                    shake.DestinationRelX = -1;
                    shake.DestinationRelY = -1;
                }

                Refresh(manequinTreeView.SelectedNode);
            }
        }

        private void manequinPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if ((drawShake || editShake) && shake != null && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                label2.Text = string.Format("X1: {0} Y1: {1} X2:{2} Y2: {3}", shake.OriginRelX, shake.OriginRelY, shake.DestinationRelX, shake.DestinationRelY);

                shake.DestinationRelX = ((float)e.X) / manequinPictureBox.Width;
                shake.DestinationRelY = ((float)e.Y) / manequinPictureBox.Height;
                manequinPictureBox.Invalidate();
            }
        }

        private void manequinPictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (shake != null && shake.OriginRelX != -1 && shake.OriginRelY != -1 & shake.DestinationRelX != -1 && shake.DestinationRelY != -1)
            {
                shake.Draw(manequinPictureBox.Height, manequinPictureBox.Width, e.Graphics);
            }
        }

        private void manequinsPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            int manequinIndex = e.X / 100;

            manequinTreeView.SelectedNode = manequinTreeView.Nodes[0].Nodes[manequinIndex].Nodes[0];
        }
    }
}
