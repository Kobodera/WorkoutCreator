using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using WorkoutCreator.Extensions;

namespace WorkoutCreator
{
    [Serializable]
    public class SongManequins
    {
        //private static XmlSerializer serializer = new XmlSerializer(typeof(SongManequins));

        public List<Manequin> Manequins { get; set; }
        public string SongName { get; set; }

        public SongManequins(string songName)
            : this()
        {
            SongName = songName;
        }

        public SongManequins()
        {
            Manequins = new List<Manequin>();
        }

        public void LoadValues(string manequinXml)
        {
            this.Manequins.Clear();

            using (DataSet ds = new DataSet())
            {
                ds.ReadXml(new StringReader(manequinXml));

                SongName = ds.Tables["SongManequin"].Rows[0]["SongName"].ToString();

                if (ds.Tables.Contains("Manequin"))
                {
                    foreach (DataRow row in ds.Tables["Manequin"].Rows)
                    {
                        Manequin m = new Manequin();
                        m.ManequinText = row[0].ToString();

                        foreach (DataRow mp in row.GetChildRows(ds.Tables["Manequin"].ChildRelations["Manequin_ManequinPart"].RelationName))
                        {
                            ManequinPoint p = m.GetPoint(mp[0].ToString());
                            p.XRel = mp["XRel"].ToString().ToDouble();
                            p.YRel = mp["YRel"].ToString().ToDouble();
                        }

                        if (ds.Tables.Contains("Shakes"))
                        {
                            foreach (DataRow shakesRow in row.GetChildRows(ds.Tables["Manequin"].ChildRelations["Manequin_Shakes"].RelationName))
                            {

                                foreach (DataRow shakeRow in shakesRow.GetChildRows(ds.Tables["Shakes"].ChildRelations["Shakes_Shake"].RelationName))
                                {
                                    Shake shake = new Shake();
                                    shake.OriginRelX = shakeRow["OrigX"].ToString().ToFloat();
                                    shake.OriginRelY = shakeRow["OrigY"].ToString().ToFloat();
                                    shake.DestinationRelX = shakeRow["DestX"].ToString().ToFloat();
                                    shake.DestinationRelY = shakeRow["DestY"].ToString().ToFloat();

                                    m.Shakes.Add(shake);
                                }
                            }
                        }

                        this.Manequins.Add(m);
                    }
                }
            }
        }

        public static SongManequins FromString(string manequinData)
        {
            SongManequins manequins = new SongManequins();
            manequins.LoadValues(manequinData);

            return manequins;
        }

        public static SongManequins Open(string fileName)
        {
            if (File.Exists(fileName))
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    SongManequins manequins = new SongManequins();
                    manequins.LoadValues(sr.ReadToEnd());

                    return manequins;
                    //XmlDocument doc = new XmlDocument();
                    //doc.LoadXml(sr.ReadToEnd());
                }
            }
            return new SongManequins(Path.GetFileNameWithoutExtension(fileName));
        }

        public void Save(string fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName, false))
            {
                sw.Write(this.ToString());
                sw.Flush();
                sw.Close();
            }
        }

        public Bitmap DrawManequins()
        {
            return DrawManequins(100, Color.White);
        }

        public Bitmap DrawManequins(int width, Color bgColor)
        {
            if (Manequins.Count == 0)
                return null;

            Bitmap bmp = new Bitmap(Manequins.Count * width, width);

            for (int i = 0; i < Manequins.Count; i++)
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(Manequins[i].DrawManequin(width, width, bgColor), i * width, 0);
                }
            }

            return bmp;
        }

        public TreeNode GetManequinTree()
        {
            TreeNode songNode = new TreeNode(SongName);
            songNode.Tag = this;

            if (Manequins.Count == 0)
            {
                Manequin m = new Manequin();
                Manequins.Add(m);

                //manequinTreeView.Nodes[0].Nodes.Add(m.GetManequinTree(string.Format("Gubbe {0}", manequins.Count)));
                //manequinTreeView.SelectedNode = manequinTreeView.Nodes[0].Nodes[0].Nodes[0];
            }

            for (int i = 0; i < Manequins.Count; i++)
            {
                TreeNode node = Manequins[i].GetManequinTree(string.Format("Gubbe {0}", i + 1));
                songNode.Nodes.Add(node);
            }

            return songNode;
        }

        public Manequin this[int index]
        {
            get
            {
                return Manequins[index];
            }
        }

        public int Count
        {
            get
            {
                return Manequins.Count;
            }
        }

        public void Add(Manequin manequin)
        {
            Manequins.Add(manequin);
        }

        public void Clear()
        {
            Manequins.Clear();

        }

        public void Delete(Manequin manequin)
        {
            Manequins.Remove(manequin);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            sb.AppendLine("<SongManequin>");
            sb.AppendLine(string.Format("\t<SongName>{0}</SongName>", SongName));

            foreach (Manequin m in Manequins)
            {
                sb.AppendLine(m.ToString());
            }

            sb.AppendLine("</SongManequin>");

            return sb.ToString();
        }
    }

    [Serializable]
    public class StepOrder
    {
        public string From { get; set; }
        public string To { get; set; }

        public StepOrder()
        {
        }

        public StepOrder(string from, string to)
        {
            From = from;
            To = to;
        }
    }

    [Serializable]
    public class Manequin
    {
        //public Dictionary<string, ManequinPoint> Points = new Dictionary<string, ManequinPoint>();

        public List<ManequinPoint> Points = new List<ManequinPoint>();

        public List<ManequinPoint> ManequinPoints { get; set; }
        public List<StepOrder> ManequinOrder { get; set; }
        public List<Shake> Shakes { get; set; }

        TreeNode manequin = null;
        TreeNode shakesNode = null;

        //public Dictionary<string, string> ManequinOrder { get; set; }

        public string ManequinText { get; set; }

        public Manequin()
        {
            ManequinPoints = new List<ManequinPoint>();
            ManequinOrder = new List<StepOrder>();
            Shakes = new List<Shake>();

            ManequinPoint head = CreatePoint("Head", "Huvud", PointType.Circle, 10, null, "ExtraPoint1");
            ManequinPoint body = CreatePoint("Body", "Midja", PointType.Line, -1, head);
            ManequinPoint backBend = CreatePoint("ExtraPoint1", "Ryggböj", PointType.Line, -1, head);

            ManequinPoint upperLeg1 = CreatePoint("UpperLeg1", "Knä 1", PointType.Line, -1, body);
            ManequinPoint lowerLeg1 = CreatePoint("LowerLeg1", "Vrist 1", PointType.Line, -1, upperLeg1);
            ManequinPoint upperLeg2 = CreatePoint("UpperLeg2", "Knä 2", PointType.Line, -1, body);
            ManequinPoint lowerLeg2 = CreatePoint("LowerLeg2", "Vrist 2", PointType.Line, -1, upperLeg2);

            ManequinPoint upperFoot1 = CreatePoint("UpperFoot1", "Fot 1", PointType.Line, -1, lowerLeg1);
            ManequinPoint lowerFoot1 = CreatePoint("LowerFoot1", "Tåspets 1", PointType.Line, -1, upperFoot1);
            ManequinPoint upperFoot2 = CreatePoint("UpperFoot2", "Fot 2", PointType.Line, -1, lowerLeg2);
            ManequinPoint lowerFoot2 = CreatePoint("LowerFoot2", "Tåspets 2", PointType.Line, -1, upperFoot2);

            ManequinPoint upperArm1 = CreatePoint("UpperArm1", "Armbåge 1", PointType.Line, 75, body);
            ManequinPoint lowerArm1 = CreatePoint("LowerArm1", "Hand 1", PointType.Line, -1, upperArm1);
            ManequinPoint upperArm2 = CreatePoint("UpperArm2", "Armbåge 2", PointType.Line, 75, body);
            ManequinPoint lowerArm2 = CreatePoint("LowerArm2", "Hand 2", PointType.Line, -1, upperArm2);

            ManequinPoint upperHand1 = CreatePoint("UpperHand1", "Fingerspets 1", PointType.Line, -1, lowerArm1);
            ManequinPoint upperHand2 = CreatePoint("UpperHand2", "Fingerspets 2", PointType.Line, -1, lowerArm2);

            head.ManequinPoints.Add(body);
            head.ManequinPoints.Add(backBend);
            body.ManequinPoints.Add(upperLeg1);
            body.ManequinPoints.Add(upperLeg2);
            body.ManequinPoints.Add(upperArm1);
            body.ManequinPoints.Add(upperArm2);

            upperLeg1.ManequinPoints.Add(lowerLeg1);
            lowerLeg1.ManequinPoints.Add(upperFoot1);
            upperFoot1.ManequinPoints.Add(lowerFoot1);

            upperLeg2.ManequinPoints.Add(lowerLeg2);
            lowerLeg2.ManequinPoints.Add(upperFoot2);
            upperFoot2.ManequinPoints.Add(lowerFoot2);

            upperArm1.ManequinPoints.Add(lowerArm1);
            lowerArm1.ManequinPoints.Add(upperHand1);

            upperArm2.ManequinPoints.Add(lowerArm2);
            lowerArm2.ManequinPoints.Add(upperHand2);

            ManequinPoints.Add(head);

            ManequinOrder.Add(new StepOrder("Head", "Body"));
            ManequinOrder.Add(new StepOrder("Body", "UpperLeg1"));
            ManequinOrder.Add(new StepOrder("UpperLeg1", "LowerLeg1"));
            ManequinOrder.Add(new StepOrder("LowerLeg1", "UpperFoot1"));
            ManequinOrder.Add(new StepOrder("UpperFoot1", "UpperLeg2"));
            ManequinOrder.Add(new StepOrder("UpperLeg2", "LowerLeg2"));
            ManequinOrder.Add(new StepOrder("LowerLeg2", "UpperFoot2"));
            ManequinOrder.Add(new StepOrder("UpperFoot2", "UpperArm1"));
            ManequinOrder.Add(new StepOrder("UpperArm1", "LowerArm1"));
            ManequinOrder.Add(new StepOrder("LowerArm1", "UpperArm2"));
            ManequinOrder.Add(new StepOrder("UpperArm2", "LowerArm2"));
            ManequinOrder.Add(new StepOrder("LowerArm2", "Head"));
        }

        public TreeNode GetManequinTree(string manequinName)
        {
            manequin = new TreeNode(manequinName);
            manequin.Tag = this;

            foreach (var point in ManequinPoints)
            {
                CreateNode(point, manequin);
            }

            shakesNode = new TreeNode("Skakningar");
            shakesNode.Tag = Shakes;
            manequin.Nodes.Add(shakesNode);

            for (int i = 0; i < Shakes.Count; i++)
            {
                TreeNode node = new TreeNode(string.Format("Skakning {0}", i + 1));
                node.Tag = Shakes[i];
                shakesNode.Nodes.Add(node);
            }

            return manequin;
        }

        private void CreateNode(ManequinPoint point, TreeNode parent)
        {
            parent.Nodes.Add(point.PartNode);

            foreach (ManequinPoint child in point.ManequinPoints)
            {
                CreateNode(child, point.PartNode);
            }
        }

        public ManequinPoint GetPoint(string pointId)
        {
            return Points.FirstOrDefault(x => x.PartId == pointId);
        }

        public ManequinPoint GetNextPoint(string currentPointId)
        {
            StepOrder order = ManequinOrder.FirstOrDefault(x => x.From == currentPointId);
            if (order == null)
                return GetPoint(currentPointId);
            else
                return GetPoint(order.To);
        }

        public void AddShake(Shake shake)
        {
            Shakes.Add(shake);

            TreeNode node = new TreeNode(string.Format("Skakning {0}", Shakes.Count));
            node.Tag = shake;

            shakesNode.Nodes.Add(node);
        }

        private ManequinPoint CreatePoint(string partId, string title, PointType type, int data, ManequinPoint parent, string extension = "")
        {
            ManequinPoint point = new ManequinPoint()
            {
                PartId = partId,
                Title = title,
                PointType = type,
                Data = data,
                ParentId = parent == null ? null : parent.PartId,
                Extension = extension
            };

            Points.Add(point);

            return point;
        }

        public Bitmap DrawManequin(int height, int width)
        {
            return DrawManequin(height, width, Color.White);
        }

        public Bitmap DrawManequin(int height, int width, Color bgColor)
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            using (Brush b = new SolidBrush(Color.Black))
            using (Pen p = new Pen(b, width / 100))
            {
                g.Clear(bgColor);
                DrawManequinPoint(bmp, g, p, ManequinPoints[0], null);

                foreach (Shake shake in Shakes)
                {
                    shake.Draw(bmp.Height, bmp.Width, g);
                }
            }

            return bmp;
        }

        private void DrawManequinPoint(Bitmap bmp, Graphics g, Pen p, ManequinPoint point, ManequinPoint parent)
        {
            if (!string.IsNullOrWhiteSpace(ManequinText))
            {
                using (Font f = new Font("Times New Roman", bmp.Height / 12))
                {
                    SizeF size = g.MeasureString(ManequinText, f);

                    g.DrawString(ManequinText, f, Brushes.Black, (bmp.Width - size.Width) / 2, (bmp.Height - size.Height));
                }
            }

            if ((((int)point.XRel) != -1) && (((int)point.YRel) != -1))
            {
                float x = bmp.Width * (float)point.XRel;
                float y = bmp.Height * (float)point.YRel;

                switch (point.PointType)
                {
                    case PointType.Circle:

                        if (parent != null && parent.XRel != -1 && parent.YRel != -1)
                        {
                            float widthPar = (float)(bmp.Width * (parent.Data / 100));
                            float heightPar = (float)(bmp.Height * (parent.Data / 100));
                            float xPar = bmp.Width * (float)parent.XRel - widthPar / 2;
                            float yPar = bmp.Height * (float)parent.YRel - heightPar / 2;

                            g.DrawLine(p, xPar, yPar, x, y);
                        }

                        if (point.Data != -1)
                        {
                            float width = (float)(bmp.Width * (point.Data / 100));
                            float height = (float)(bmp.Height * (point.Data / 100));

                            x = x - width / 2;
                            y = y - height / 2;

                            g.FillEllipse(Brushes.White, x, y, width, height);
                            g.DrawEllipse(p, x, y, width, height);
                            //g.DrawEllipse(p, bmp.Width * (float)point.XRel, bmp.Height * (float)point.YRel, (float)bmp.Width * (point.Data / 100), (float)bmp.Height * (point.Data / 100));
                        }

                        break;
                    case PointType.FilledCircle:
                        break;
                    case PointType.Line:
                        //A parent point has been registered. Draw a line from that point to this point
                        if (parent != null && ((int)parent.XRel) != -1 && ((int)parent.YRel) != -1)
                        {
                            if (parent.Extension.Length > 0 && parent.Extension != point.PartId)
                            {
                                ManequinPoint extension = GetPoint(parent.Extension);
                                if (extension.XRel != -1 && extension.YRel != -1)
                                {
                                    DrawManequinPoint(bmp, g, p, extension, parent);
                                    parent = extension;
                                }
                            }

                            if (parent.PointType == PointType.Circle)
                            {
                                float widthPar = (float)(bmp.Width * (parent.Data / 100));
                                float heightPar = (float)(bmp.Height * (parent.Data / 100));
                                float xPar = bmp.Width * (float)parent.XRel;
                                float yPar = bmp.Height * (float)parent.YRel;

                                g.DrawLine(p, xPar, yPar, x, y);
                                g.FillEllipse(Brushes.White, xPar - widthPar / 2, yPar - heightPar / 2, widthPar, heightPar);
                                g.DrawEllipse(p, xPar - widthPar / 2, yPar - heightPar / 2, widthPar, heightPar);
                            }
                            else
                            {
                                //Needed in order to be able to save the manequin
                                ManequinPoint parentsParent = GetPoint(parent.ParentId);
                                if (parentsParent != null && parentsParent.Extension.Length > 0)
                                {
                                    ManequinPoint extension = GetPoint(parentsParent.Extension);
                                    if (extension.XRel != -1 && extension.YRel != -1 && parentsParent != null && point.Data != -1)
                                    {
                                        parent = extension;
                                    }
                                }

                                float xPar = bmp.Width * (float)parent.XRel;
                                float yPar = bmp.Height * (float)parent.YRel;

                                if (parentsParent != null && point.Data != -1)
                                {
                                    float xPar2 = bmp.Width * (float)parentsParent.XRel;
                                    float yPar2 = bmp.Height * (float)parentsParent.YRel;

                                    xPar = (float)(xPar + (xPar2 - xPar) * (point.Data / 100));
                                    yPar = (float)(yPar + (yPar2 - yPar) * (point.Data / 100));
                                }

                                g.DrawLine(p, xPar, yPar, x, y);
                            }
                        }

                        break;
                }

                foreach (ManequinPoint pt in point.ManequinPoints)
                {
                    DrawManequinPoint(bmp, g, p, pt, point);
                }

            }
        }

        internal void Clear()
        {
            ManequinText = string.Empty;

            foreach (ManequinPoint point in Points)
            {
                point.XRel = -1;
                point.YRel = -1;
            }

            Shakes.Clear();
            shakesNode.Nodes.Clear();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<Manequin>");
            sb.AppendLine(string.Format("<ManequinText>{0}</ManequinText>", ManequinText));
            foreach (ManequinPoint point in Points)
            {
                sb.Append(point.ToString());
            }

            sb.AppendLine("<Shakes>");

            foreach (Shake shake in Shakes)
                sb.AppendLine(shake.ToString());

            sb.AppendLine("</Shakes>");

            sb.AppendLine("</Manequin>");

            return sb.ToString();
        }

        internal void Delete(Shake shake)
        {
            Shakes.Remove(shake);
        }
    }

    [Serializable]
    public class ManequinPoint
    {
        public string PartId { get; set; }
        private TreeNode partNode;
        [XmlIgnore]
        public TreeNode PartNode
        {
            get
            {
                if (partNode == null)
                {
                    partNode = new TreeNode(Title);
                    partNode.Tag = this;
                }
                return partNode;
            }
            set
            {
                partNode = value;
            }
        }

        public string Title { get; set; }
        public double XRel { get; set; }
        public double YRel { get; set; }
        public PointType PointType { get; set; }
        public double Data { get; set; }
        public string ParentId { get; set; }
        public string Extension { get; set; }

        public List<ManequinPoint> ManequinPoints { get; set; }

        public ManequinPoint()
        {
            ManequinPoints = new List<ManequinPoint>();
            XRel = -1;
            YRel = -1;
            Extension = string.Empty;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("<ManequinPart PartId=\"{0}\" XRel=\"{1}\" YRel=\"{2}\" />", PartId, XRel, YRel));

            //if (ManequinPoints.Count == 0)
            //    sb.AppendLine("<MPs />");
            //else
            //{
            //    sb.AppendLine("<MPs>");

            foreach (ManequinPoint point in ManequinPoints)
            {
                sb.Append(point.ToString());
            }

            //sb.AppendLine("</MPs>");
            //}

            //sb.AppendLine("</MP>");

            return sb.ToString();
        }
    }

    public class Shake
    {
        public Shake()
        {
            OriginRelX = -1;
            OriginRelY = -1;
            DestinationRelX = -1;
            DestinationRelY = -1;
        }


        public float OriginRelX { get; set; }
        public float OriginRelY { get; set; }

        public float DestinationRelX { get; set; }
        public float DestinationRelY { get; set; }

        //private PointF? orgin;

        //public PointF? Orgin
        //{
        //    get { return orgin; }
        //    set { orgin = value; }
        //}

        //private PointF? destination;
        //public PointF? Destination
        //{
        //    get { return destination; }
        //    set { destination = value; }
        //}

        public void Draw(Image image)
        {
            using (Graphics g = Graphics.FromImage(image))
            {
                Draw(image.Height, image.Width, g);
            }
        }

        public void Draw(int height, int width, Graphics g)
        {
            Draw(height, width, g, Color.Black);
        }

        public void Draw(int height, int width, Graphics g, Color color)
        {
            if (OriginRelX != -1 && OriginRelY != -1 && DestinationRelX != -1 && DestinationRelY != -1)
            {
                float origX = (float)width * (OriginRelX);
                float origY = (float)height * (OriginRelY);
                float destX = (float)width * (DestinationRelX);
                float destY = (float)height * (DestinationRelY);

                double angle = Utils.CalculateAngle(new PointF(origX, origY), new PointF(destX, destY));

                using (Pen p = new Pen(color, (float)(width / 100)))
                {
                    g.DrawArc(p, new RectangleF(origX - (width / 40), origY - (height / 40), (width / 20), (width / 20)), 360 - (float)angle - 45, 90);
                    g.DrawArc(p, new RectangleF(origX - width / 20, origY - height / 20, width / 10, width / 10), 360 - (float)angle - 45, 90);
                }
            }
        }

        public override string ToString()
        {
            return string.Format("<Shake OrigX=\"{0}\" OrigY=\"{1}\" DestX=\"{2}\" DestY=\"{3}\" />", OriginRelX, OriginRelY, DestinationRelX, DestinationRelY);
        }

        //public override string ToString()
        //{
        //    if (orgin.HasValue && destination.HasValue)
        //    {
        //        return string.Format("Shake Pos: {0} Angle: {1}", orgin.Value.ToString(), Utils.CalculateAngle(orgin.Value, destination.Value));
        //    }
        //    return "New shake";
        //}

    }

    public class Utils
    {

        public static double CalculateAngle(PointF orgin, PointF destination)
        {
            double dx = destination.X - orgin.X;
            double dy = orgin.Y - destination.Y;

            double temp = 0;
            double degreeAddon = 0;

            if (dx < 0 && dy == 0)
            {
                return 180;
            }

            if (dx == 0 && dy < 0)
            {
                return 270;
            }

            if (dx > 0 && dy > 0)
            {
                degreeAddon = 0;
            }
            else if (dx < 0 && dy > 0)
            {
                degreeAddon = 90;
                temp = dx;
                dx = dy;
                dy = temp;
            }
            else if (dx < 0 && dy < 0)
            {
                degreeAddon = 180;
            }
            else if (dx > 0 && dy < 0)
            {
                degreeAddon = 270;
                temp = dx;
                dx = dy;
                dy = temp;
            }

            return degreeAddon + ToDegree(Math.Atan(Math.Abs(dy) / Math.Abs(dx)));
        }

        public static double ToDegree(double radians)
        {
            return radians * (180 / Math.PI);
        }

        public static double ToRadian(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

    }

    public enum PointType
    {
        Line = 0,
        Circle = 1,
        FilledCircle = 2,
    }
}
