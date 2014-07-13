using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        //const string file = @"E:\Program Files (x86)\Steam\SteamApps\common\Awesomenauts\Data\Replays\Replay_2014_05_14_19_34_41\Replay_Continuous_0_10.006.continuousData";
        //const string file = @"E:\Program Files (x86)\Steam\SteamApps\common\Awesomenauts\Data\Replays\Replay_2014_05_14_19_34_41\Replay_0_10.006.blockData";
        //const string file = @"E:\Program Files (x86)\Steam\SteamApps\common\Awesomenauts\Data\Replays\Replay_2014_05_14_19_34_41\Replay_10.006_20.0129.blockData";
        //const string file = @"E:\Program Files (x86)\Steam\SteamApps\common\Awesomenauts\Data\Replays\Replay_2014_05_14_19_34_41\Replay_20.0129_30.0015.blockData";
        const string file = @"E:\Program Files (x86)\Steam\SteamApps\common\Awesomenauts\Data\Replays\Replay_2014_05_14_19_34_41\Replay_30.0015_40.002.blockData";
        const string dir = @"E:\Program Files (x86)\Steam\SteamApps\common\Awesomenauts\Data\Replays";
        const int NS = 13, MAX = 1 << NS;

        Rectangle map = new Rectangle((int)(MAX * 0.4f), (int)(MAX * 0.1f), (int)(MAX * 0.5f), (int)(MAX * 0.5f));

        MoveInfo[][] segments;
        bool[] bits;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            load(file);
            if (Directory.Exists(dir))
            {
                this.openFileDialog1.FileName = dir;
            }
            //int[] starts = new int[] { 424, 10301, 25965, 31130, 42331, 58854 };
            //int[] ends = new int[] { 2944, 13031, 28310, 34385, 44781, 62319 };
            //int[] lengths = new int[] { 72, 78, 67, 93, 70, 99 };
            //for (int i = 0; i < starts.Length; i++)
            //{ 
            //    int sp = starts[i];
            //    long data = valueL(bits, sp - 64, 64);
            //    string line = Convert.ToString(data, 2);
            //    while (line.Length < 64) line = "0" + line;
            //    int l = value(bits, sp - 7, 7);
            //    Console.WriteLine(line + ": " + l);
            //}
            //Console.WriteLine();
            //foreach (int sp in starts)
            //{ 
            //    long data = valueL(bits, sp + 35, 64);
            //    string line = Convert.ToString(data, 2);
            //    while (line.Length < 64) line = "0" + line;
            //    Console.WriteLine(line);
            //}

        }

        void load(string path)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                BitArray barray = new BitArray(bytes);
                bits = new bool[barray.Length];
                barray.CopyTo(bits, 0);
            }
            catch
            {
                return;
            }

            Bitmap bmp = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(255, 255, 255, 255));
            this.pictureBox1.Image = bmp;

            List<MoveInfo[]> segs = new List<MoveInfo[]>();
            for (int i = 0; i < bits.Length; i++)
            {
                int v = value(bits, i, 24);
                if (v == 16646159)
                {
                    MoveInfo[] moves = readMoves(i + 42);
                    if (moves == null) continue;
                    int id = value(bits, i + 24, 18);
                    Console.WriteLine(id.ToBinary(18));
                    segs.Add(moves);
                }
            }
            segments = segs.ToArray();
            go();
        }

        MoveInfo[] readMoves(int index)
        {
            int length = value(bits, index, 7);
            index += 7;
            int time = 0;
            List<MoveInfo> infos = new List<MoveInfo>();
            for (int i = 0; i < length; i++)
            {
                if (index >= bits.Length) return null;
                MoveInfo info = readMove(index);
                if (info.time < time) return null;
                time = info.time;
                index += 35;
                infos.Add(info);
            }
            return infos.ToArray();
        }

        static int value(bool[] bits, int index, int length)
        {
            if (length > 32) throw new Exception("Too large for int");
            return (int)valueL(bits, index, length);
        }

        static long valueL(bool[] bits, int index, int length)
        {
            long v = 0;
            for (int i = index + length - 1; i >= index; i--)
            {
                v <<= 1;
                if (i < bits.Length && bits[i]) v |= 1;
            }
            return v;
        }

        static byte cut(byte b, int start, int length)
        {
            if (start > 0) b = (byte)(b << start);
            int mask = 0;
            for (int i = 0; i < length; i++) mask |= 1 << i;
            b = (byte)(b & mask);
            return b;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }

        //private int getTime()

        private void go()
        {
            Bitmap bmp = (Bitmap)this.pictureBox1.Image;
            if (bmp.Width != this.pictureBox1.Width || bmp.Height != this.pictureBox1.Height)
            {
                bmp = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height);
                this.pictureBox1.Image = bmp;
            }
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);

            int time = this.trackBar1.Value;
            int index = 0;
            foreach (MoveInfo[] segment in segments)
            {
                int r = index * 255 / segments.Length;
                Color color = Color.FromArgb(255, r, 0, 255 - r);
                PointF last = new PointF();
                foreach (MoveInfo move in segment)
                {
                    float x = (move.x - map.X) / (float)map.Width, y = (move.y - map.Y) / (float)map.Height;
                    int px = (int)((bmp.Width - 1) * x), py = bmp.Height - 1 - (int)((bmp.Width - 1) * y);
                    if (!last.IsEmpty) g.DrawLine(new Pen(Color.FromArgb(100, color)), last.X, last.Y, px, py);
                    if (move.time >= time)
                    {
                        g.FillEllipse(new SolidBrush(color), new RectangleF(px - 2, py - 2, 5, 5));
                        g.DrawString("" + index, new Font("Arial", 8), Brushes.Black, px - 10, py - 5);
                        break;
                    }
                    last.X = px; last.Y = py;
                }
                index++;
            }

            this.pictureBox1.Refresh();
        }

        private void go_old(bool updateRect = false)
        {
            int start = (int)((float)this.trackBar1.Value / this.trackBar1.Maximum * bits.Length);
            int length = 4000;

            Bitmap bmp = (Bitmap)this.pictureBox1.Image;
            if (bmp.Width != this.pictureBox1.Width || bmp.Height != this.pictureBox1.Height)
            {
                bmp = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height);
                this.pictureBox1.Image = bmp;
            }
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(255, 255, 255, 255));

            int[] offsetWeights = new int[35];
            for (int i = start; i + NS < bits.Length && i < start + length; i++)
            {
                int time = getTime(i);
                int lastTime = getTime(i - 35);

                int delta = time - lastTime;
                if (delta < 0) continue;

                offsetWeights[i % 35]++;
            }
            int maxIndex = -1;
            int maxWeight = -1;
            for (int i = 0; i < offsetWeights.Length; i++)
            {
                if (offsetWeights[i] > maxWeight)
                {
                    maxIndex = i;
                    maxWeight = offsetWeights[i];
                }
            }
            Console.WriteLine(maxIndex);

            //if (drawRect) g.DrawRectangle(Pens.Black, rect);

            List<string> indices = new List<string>();

            for (int i = start; i + NS < bits.Length && i < start + length; i++)
            {
                //if (filter && i % 35 != maxIndex) continue;

                MoveInfo move = readMove(i);

                float x = move.x / (float)MAX;
                float y = move.y / (float)MAX;
                int r = move.time / 2;
                int b = 255 - r;
                
                int px = (int)((bmp.Width - 1) * x), py = bmp.Height - 1 - (int)((bmp.Width - 1) * y);

                Color color = Color.FromArgb(50, r, 0, b);
                //bmp.SetPixel(px, py, color);
                g.FillEllipse(new SolidBrush(color), new RectangleF(px - 2, py - 2, 5, 5));
            }
            this.pictureBox1.Refresh();

            //magic++;
        }

        private int getTime(int i)
        {
            int timeIndex = i - 9;
            int time = 0;
            if (timeIndex >= 0) time = value(bits, timeIndex, 9);
            return time;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            go();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            go();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //if (e.Button == System.Windows.Forms.MouseButtons.Left)
            //{
            //    special.X = (float)e.X / this.pictureBox1.Width;
            //    special.Y = (float)e.Y / this.pictureBox1.Height;
            //    go(true);
            //}
        }

        private void opemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.segments = new MoveInfo[][] { };
                load(this.openFileDialog1.FileName);
            }
        }

        MoveInfo readMove(int i)
        {
            return new MoveInfo() 
            { 
                index = i,
                time = value(bits, i, 9),
                x = value(bits, i + 9, 13),
                y = value(bits, i + 22, 13)
            };
        }

        struct MoveInfo
        {
            public int index, x, y, time;
            public override string ToString()
            {
                return string.Format("{0:D5} [{3:D3}]: ({1:D4}, {2:D4})", index, x, y, time);
            }
        }
    }

    public static class BitmapExtensions
    {
        public static Bitmap SetOpacity(this Image image, float opacity)
        {
            var colorMatrix = new ColorMatrix();
            colorMatrix.Matrix33 = opacity;
            var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(
                colorMatrix,
                ColorMatrixFlag.Default,
                ColorAdjustType.Bitmap);
            var output = new Bitmap(image.Width, image.Height);
            using (var gfx = Graphics.FromImage(output))
            {
                gfx.SmoothingMode = SmoothingMode.AntiAlias;
                gfx.DrawImage(
                    image,
                    new Rectangle(0, 0, image.Width, image.Height),
                    0,
                    0,
                    image.Width,
                    image.Height,
                    GraphicsUnit.Pixel,
                    imageAttributes);
            }
            return output;
        }

        public static string ToBinary(this int i, int digits)
        {
            string s = Convert.ToString(i, 2);
            while (s.Length < digits) s = "0" + s;
            return s;
        }
    }
}
