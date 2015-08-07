using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WorkoutCreator.WorkoutTemplates;

namespace WorkoutCreator
{
    public partial class NewWorkoutForm : Form
    {
        public NewWorkoutForm()
        {
            InitializeComponent();

            LoadTemplates();
        }

        public WorkoutTemplate Template
        {
            get
            {
                WorkoutTemplate template = (WorkoutTemplate)WorkoutTemplatesComboBox.SelectedItem;
                template.Title = titleTextBox.Text;
                return template;
            }
        }

        private void LoadTemplates()
        {
            WorkoutCreator.WorkoutTemplates.WorkoutTemplates templates = WorkoutCreator.WorkoutTemplates.WorkoutTemplates.LoadTemplates(".\\WorkoutTemplates");

            WorkoutTemplatesComboBox.DataSource = templates.Templates;
            WorkoutTemplatesComboBox.DisplayMember = "Title";
        }

        private void WorkoutTemplatesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            WorkoutTemplate template = ((WorkoutTemplate)WorkoutTemplatesComboBox.SelectedItem);

            DateTime time = DateTime.Now;

            if (time.Month < 2)
                titleTextBox.Text = string.Format("{0} - (VT {1})", template.Title, time.Year);
            else if (time.Month >= 8)
                titleTextBox.Text = string.Format("{0} - (VT {1})", template.Title, time.Year + 1);
            else
                titleTextBox.Text = string.Format("{0} - (HT {1})", template.Title, time.Year);

            DescriptionTextBox.Text = template.Description;

            TemplateTreeView.Nodes.Clear();
            TemplateTreeView.Nodes.Add(template.GetTemplateTree());
            TemplateTreeView.ExpandAll();
        }

        private void TemplateTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            WorkoutBase wb = (WorkoutBase)e.Node.Tag;
            DescriptionTextBox.Text = wb.Description;
        }
    }
}
