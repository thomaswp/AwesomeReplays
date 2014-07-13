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
        const string file = @"E:\Program Files (x86)\Steam\SteamApps\common\Awesomenauts\Data\Replays\Replay_2014_05_14_19_34_41\Replay_30.0015_40.002.blockData";
        const string dir = @"E:\Program Files (x86)\Steam\SteamApps\common\Awesomenauts\Data\Replays";
        const int MAX = MoveInfo.MAX_COORD;

        Rectangle map = new Rectangle((int)(MAX * 0.45f), (int)(MAX * 0.1f), (int)(MAX * 0.4f), (int)(MAX * 0.4f));

        CharacterBlock[] blocks;
        BitData data;

        public Form1()
        {
            InitializeComponent();
            gcdTest();
        }

        private static void gcdTest()
        {
            int[] ns = new int[] {
                7297, 
9788, 
871, 
1115, 
2760, 
6554, 
747, 
14013, 
6749, 
2146, 
656, 
1792, 
720
            };

            for (int i = 0; i < 690; i++)
            {
                int g = gcd(ns);
                if (g > 1)
                {
                    Console.WriteLine(g);
                }
                for (int j = 0; j < ns.Length; j++)
                {
                    ns[j]--;
                }
            }
        }

        static int gcd(int a, int b)
        {
            if (b == 0) return a;
            return gcd(b, a % b);
        }

        static int gcd(int[] ns, int n = 0)
        {
            if (n == ns.Length - 1) return ns[n];
            int gcdRest = gcd(ns, n + 1);
            return gcd(ns[n], gcdRest);
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
                data = new BitData(path);
            }
            catch
            {
                return;
            }

            Bitmap bmp = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(255, 255, 255, 255));
            this.pictureBox1.Image = bmp;

            List<CharacterBlock> segs = new List<CharacterBlock>();
            for (int i = 0; i < data.Length; i++)
            {
                int v = data.ReadInt(i, 24);
                if (v == 16646159)
                {
                    CharacterBlock moves = CharacterBlock.Read(data, i + 24);
                    if (moves == null) continue;
                    segs.Add(moves);
                }
            }
            blocks = segs.ToArray();

            for (int i = 0; i < blocks.Length; i++)
            {
                CharacterBlock block = blocks[i];
                CharacterBlock next = i + 1 < blocks.Length ? blocks[i + 1] : null;
                int after = data.ReadInt(block.index + block.bitLength, 7);
                //Console.Write(after + ": ");//.ToBinary(7));
                //if (next != null)
                //    Console.WriteLine((next.index - block.index - block.bitLength) + ", ");
            }

            go();
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
            foreach (CharacterBlock block in blocks)
            {
                int r = index * 255 / blocks.Length;
                Color color = Color.FromArgb(255, r, 0, 255 - r);
                PointF last = new PointF();
                foreach (MoveInfo move in block.moves)
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
                blocks = new CharacterBlock[] { };
                load(this.openFileDialog1.FileName);
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
