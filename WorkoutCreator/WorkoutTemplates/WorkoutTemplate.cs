using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace WorkoutCreator.WorkoutTemplates
{
    [Serializable]
    public class WorkoutBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
    }

    [Serializable]
    public class WorkoutTemplate : WorkoutBase
    {
        [XmlIgnore]
        public string WorkoutTemplatePath { get; set; }

        public List<WorkoutTemplatePart> WorkoutTemplateParts { get; set; }

        public TreeNode GetTemplateTree()
        {
            string title = GetTitle(Title, MinLength, MaxLength);

            TreeNode node = new TreeNode(title);
            node.Tag = this;

            foreach (WorkoutTemplatePart part in WorkoutTemplateParts)
            {
                TreeNode partNode = new TreeNode(GetTitle(part.Title, part.MinLength, part.MaxLength));
                partNode.Tag = part;

                node.Nodes.Add(partNode);
            }

            return node;
        }

        private string GetTitle(string title, int minLength, int maxLength)
        {
            if (minLength == maxLength)
            {
                return string.Format("{0} ({1} min)", title, minLength);
            }
            else
            {
                return string.Format("{0} ({1} - {2} min)", title, minLength, maxLength);
            }
        }


        public static WorkoutTemplate LoadTemplate(string templatePath)
        {
            WorkoutTemplate result = null;
            XmlSerializer serializer = new XmlSerializer(typeof(WorkoutTemplate));

            using (StreamReader sr = new StreamReader(templatePath))
            {
                try
                {
                    string str = sr.ReadToEnd();
                    using (StringReader sReader = new StringReader(str))
                    {
                        result = (WorkoutTemplate)serializer.Deserialize(sReader);
                        result.Description = result.Description.Replace("\n", "\r\n");

                        foreach (WorkoutTemplatePart part in result.WorkoutTemplateParts)
                        {
                            part.Description = part.Description.Replace("\n", "\r\n");
                        }
                    }
                }
                catch (Exception exc)
                {
                    System.Windows.Forms.MessageBox.Show(exc.ToString());
                }

                sr.Close();
                sr.Dispose();
            }

            result.WorkoutTemplatePath = templatePath;

            return result;
        }
    }

    [Serializable]
    public class WorkoutTemplatePart : WorkoutBase
    {
    }

    public class WorkoutTemplates
    {
        public List<WorkoutTemplate> Templates { get; set; }

        public WorkoutTemplates()
        {
            Templates = new List<WorkoutTemplate>();
        }

        public static WorkoutTemplates LoadTemplates(string templateDirectory)
        {
            WorkoutTemplates templates = new WorkoutTemplates();

            if (Directory.Exists(templateDirectory))
            {
                string[] files = Directory.GetFiles(templateDirectory, "*.xml");

                foreach (string file in files)
                {
                    WorkoutTemplate template = WorkoutTemplate.LoadTemplate(file);

                    if (template != null)
                        templates.Templates.Add(template);
                }
            }

            return templates;
        }

        public TreeNode GetTemplateTree(int templateIndex)
        {
            WorkoutTemplate template = Templates[templateIndex];

            return template.GetTemplateTree();
        }
    }
}
