using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using WorkoutCreator.WorkoutTemplates;
using WorkoutCreator.Extensions;
using System.Drawing;
using System.Drawing.Imaging;

namespace WorkoutCreator.Workout
{
    [Serializable]
    public class Workout
    {
        public int? WorkoutId { get; set; }
        public string Title { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }

        public string WorkoutTemplatePath { get; set; }

        private WorkoutTemplate template;
        [XmlIgnore]
        public WorkoutTemplate Template
        {
            get
            {
                if (template == null)
                {
                    template = WorkoutTemplate.LoadTemplate(WorkoutTemplatePath);
                }

                return template;
            }
            set
            {
                template = value;
            }
        }

        public List<WorkoutPart> WorkoutParts { get; set; }

        public string TitleWithTime
        {
            get
            {
                TimeSpan total = new TimeSpan();

                foreach (WorkoutPart part in WorkoutParts)
                {
                    total = total.Add(part.GetPartTime());
                }

                if (total.Hours > 0)
                    return string.Format("{0} ({1:00}:{2:00}:{3:00})", Title, total.Hours, total.Minutes, total.Seconds);
                else
                    return string.Format("{0} ({1:00}:{2:00})", Title, total.Minutes, total.Seconds);
            }
        }

        public Workout()
        {
            WorkoutParts = new List<WorkoutPart>();
        }

        public static Workout FromTemplate(WorkoutTemplate workoutTemplate)
        {
            Workout workout = new Workout()
            {
                Title = workoutTemplate.Title,
                WorkoutTemplatePath = workoutTemplate.WorkoutTemplatePath,
                MinLength = workoutTemplate.MinLength,
                MaxLength = workoutTemplate.MaxLength,
                Template = workoutTemplate
            };

            foreach (WorkoutTemplatePart part in workoutTemplate.WorkoutTemplateParts)
            {
                workout.WorkoutParts.Add(new WorkoutPart()
                {
                    Title = part.Title,
                    Template = part,
                    MinLength = part.MinLength,
                    MaxLength = part.MaxLength
                });
            }

            return workout;
        }

        public TreeNode ToTree()
        {
            TreeNode node = new TreeNode(TitleWithTime);
            node.Tag = this;

            foreach (WorkoutPart part in WorkoutParts)
            {
                node.Nodes.Add(part.ToTree());
            }

            return node;
        }

        public static Workout LoadWorkout(string workoutPath)
        {
            string str = string.Empty;
            DataSet ds = new DataSet();

            using (StreamReader sr = new StreamReader(workoutPath))
            {
                str = sr.ReadToEnd();

                using (StringReader reader = new StringReader(str))
                {
                    ds.ReadXml(reader);
                }
            }

            Workout workout = Workout.FromTemplate(WorkoutTemplate.LoadTemplate(ds.Tables["Workout"].Rows[0]["Template"].ToString()));

            workout.Title = ds.Tables["Workout"].Rows[0]["Title"].ToString();

            foreach (DataRow row in ds.Tables["WorkoutPart"].Rows)
            {
                WorkoutPart part = workout.WorkoutParts.FirstOrDefault(x => x.Title == row["Title"].ToString());

                if (part != null)
                {
                    foreach (DataRow songsRows in row.GetChildRows("WorkoutPart_WorkoutSongs"))
                    {
                        foreach (DataRow songRow in songsRows.GetChildRows("WorkoutSongs_WorkoutSong"))
                        {
                            WorkoutSong song = new WorkoutSong()
                            {
                                Title = songRow["Title"].ToString(),
                                Artist = songRow["Artist"].ToString(),
                                Album = songRow["Album"].ToString(),
                                Bpm1 = songRow["Bpm1"].ToString().ToInt(),
                                Bpm2 = songRow["Bpm2"].ToString().ToInt(),
                                Description = songRow["Description"].ToString(),
                                FromTime = songRow["FromTime"].ToString().ToInt(),
                                ToTime = songRow["ToTime"].ToString().ToInt(),
                                OriginalLength = songRow["OriginalLength"].ToString().ToInt(),
                                Link = songRow["Link"].ToString()
                            };

                            string manequins = songRow["Manequins"].ToString();

                            manequins = manequins.Replace("&apos;", "'").Replace("&quot;", "\"").Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&").Trim();

                            song.Manequins = SongManequins.FromString(manequins);

                            part.Songs.Add(song);

                            //sb.AppendLine(Manequins.ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;"));

                        }
                    }
                }
            }

            return workout;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("<Workout Template=\"{0}\" Title=\"{1}\" MinLenght=\"{2}\" MaxLength=\"{3}\">", Template.WorkoutTemplatePath, Title, MinLength, MaxLength));
            sb.AppendLine("<WorkoutParts>");
            foreach (WorkoutPart part in WorkoutParts)
            {
                sb.AppendLine(part.ToString());
            }
            sb.AppendLine("</WorkoutParts>");
            sb.AppendLine("</Workout>");

            return sb.ToString();
        }

        public string ToHtml()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta http-equiv=\"content-type\" content=\"text-html; charset=utf-8\">");
            sb.AppendLine(string.Format("<title>{0}</title>", Title));
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine(string.Format("<h1>{0}</h1>", TitleWithTime));

            int i = 0;
            foreach (WorkoutPart part in WorkoutParts)
            {
                //i += 1;
                sb.Append(part.ToHtml(ref i));
            }

            sb.AppendLine("</body>");

            return sb.ToString();
            //return sb.ToString().Replace("å", "%E5").Replace("ä", "%E4").Replace("ö", "%F6").Replace("Å", "%C5").Replace("Ä", "%C4").Replace("Ö", "%D6");
        }


        internal void SaveImages(string path, int width, Color color)
        {
            int i = 0;
            foreach (WorkoutPart part in WorkoutParts)
            {
                //i += 1;
                part.SaveImages(path, ref i, width, color);
            }
        }
    }

    [Serializable]
    public class WorkoutPart
    {
        public string Title { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }

        [XmlIgnore]
        public WorkoutTemplatePart Template { get; set; }

        public List<WorkoutSong> Songs { get; set; }

        public WorkoutPart()
        {
            Songs = new List<WorkoutSong>();
        }

        public TimeSpan GetPartTime()
        {
            using (WorkoutCreator.Controls.TimeTextbox tb = new Controls.TimeTextbox())
            {
                tb.TotalSeconds = 0;

                foreach (WorkoutSong song in Songs)
                {
                    tb.TotalSeconds += song.Length;
                }

                return TimeSpan.FromSeconds(tb.TotalSeconds);
            }
        }

        public string TitleWithTime
        {
            get
            {
                TimeSpan span = GetPartTime();
                return string.Format("{0} ({1:00}:{2:00})", Title, span.Minutes, span.Seconds);
            }
        }

        public TreeNode ToTree()
        {
            TimeSpan span = GetPartTime();
            TreeNode node = new TreeNode(string.Format("{0} ({1:00}:{2:00})", Title, span.Minutes, span.Seconds));
            node.Tag = this;

            foreach (WorkoutSong song in Songs)
            {
                node.Nodes.Add(song.ToTree());
            }

            return node;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("<WorkoutPart Title=\"{0}\" MinLenght=\"{1}\" MaxLength=\"{2}\">", Title, MaxLength, MinLength));
            sb.AppendLine("<WorkoutSongs>");
            foreach (WorkoutSong song in Songs)
            {
                sb.AppendLine(song.ToString());
            }
            sb.AppendLine("</WorkoutSongs>");
            sb.AppendLine("</WorkoutPart>");

            return sb.ToString();
        }

        public string ToHtml(ref int partIndex)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("<h2>{0}</h2>", TitleWithTime));

            //int i = partIndex * 100;
            //int i = partIndex * 100;
            foreach (WorkoutSong song in Songs)
            {
                partIndex = partIndex + 1;
                //i += 1;
                sb.AppendLine("<p>");
                sb.AppendLine("<table>");
                sb.Append(song.ToHtml(ref partIndex));
                sb.AppendLine("</table>");
                sb.AppendLine("</p>");
            }

            return sb.ToString();
        }

        internal void SaveImages(string path, ref int partIndex, int width, Color color)
        {
            //int i = partIndex * 100;
            foreach (WorkoutSong song in Songs)
            {
                partIndex = partIndex + 1;

                song.SaveImage(path, ref partIndex, width, color);
            }
        }
    }

    [Serializable]
    public class WorkoutSong
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public int Bpm1 { get; set; }
        public int Bpm2 { get; set; }
        public int OriginalLength { get; set; }
        public int FromTime { get; set; }
        public int ToTime { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }

        public SongManequins Manequins { get; set; }

        public WorkoutSong()
        {
            Manequins = new SongManequins();
        }

        public TreeNode ToTree()
        {
            TreeNode node = new TreeNode(this.ToDisplayString());
            node.Tag = this;

            return node;
        }

        public string ToDisplayString()
        {
            int toTime = ToTime == 0 ? OriginalLength : ToTime;

            TimeSpan span = TimeSpan.FromSeconds(toTime - FromTime);

            return string.Format("{0} - {1} - {2} ({3:00}:{4:00})", Title, BpmToString(), Artist, span.Minutes, span.Seconds);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("<WorkoutSong Title=\"{0}\" Artist=\"{1}\" Album=\"{2}\" Bpm1=\"{3}\" Bpm2=\"{4}\" OriginalLength=\"{5}\" FromTime=\"{6}\" ToTime=\"{7}\" Link=\"{8}\" Description=\"{9}\">", Title, Artist, Album, Bpm1, Bpm2, OriginalLength, FromTime, ToTime, Link, Description));
            if (Manequins != null)
            {
                sb.AppendLine("<Manequins>");
                sb.AppendLine(Manequins.ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;"));
                sb.AppendLine("</Manequins>");
            }

            sb.AppendLine("</WorkoutSong>");

            return sb.ToString();
        }

        private string BpmToString()
        {
            StringBuilder sb = new StringBuilder();

            if (Bpm1 != 0)
                sb.Append(Bpm1);

            if (sb.Length > 0 && Bpm2 > 0 && Bpm2 != Bpm1)
                sb.AppendFormat("_{0}", Bpm2);
            else if (Bpm2 > 0)
                sb.Append(Bpm2);

            if (sb.Length > 0)
                sb.Append(" Bpm");

            return sb.ToString();
        }


        public int Length
        {
            get
            {
                int toTime = ToTime == 0 ? OriginalLength : ToTime;

                return toTime - FromTime;
            }
        }

        public string ToHtml(ref int songIndex)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<tr>");
            sb.AppendLine("<td style=\"Width:20px;\">");
            sb.AppendLine(string.Format("<h3>{0:00}.</h3>", songIndex));
            sb.AppendLine("</td>");
            sb.AppendLine("<td>");
            sb.AppendLine(string.Format("<h3>{0}</h3>", ToDisplayString()));
            sb.AppendLine("</td>");
            sb.AppendLine("</tr>");

            sb.AppendLine("<tr>");
            sb.AppendLine("<td>");
            sb.AppendLine("</td>");
            sb.AppendLine("<td>");
            sb.AppendLine(string.Format("<img src=\"{0}\" />", GetImageFileName(songIndex)));
            sb.AppendLine("</td>");
            sb.AppendLine("</tr>");

            return sb.ToString();
        }

        private string GetImageFileName(int songIndex)
        {
            return string.Format("Manequins_{0}.png", songIndex);
        }

        internal void SaveImage(string path, ref int songIndex, int width, Color color)
        {
            Bitmap bmp = Manequins.DrawManequins(width, color);
            bmp.Save(Path.Combine(path, GetImageFileName(songIndex)), ImageFormat.Png);
        }
    }
}
