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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class MainForm : Form
    {
        const string DEFAULT_FILE = @"E:\Program Files (x86)\Steam\SteamApps\common\Awesomenauts\Data\Replays\Replay_2014_08_02_20_07_58\Replay_50.0049_60.0052.blockData"; //Replay_2014_05_14_19_34_41\Replay_30.0015_40.002.blockData";
        static readonly string[] DEFAULT_DIRS = new string[] {
            @"C:\Program Files (x86)\Steam\SteamApps\common\Awesomenauts\Data\Replays",
            @"E:\Program Files (x86)\Steam\SteamApps\common\Awesomenauts\Data\Replays",
            @"C:\Program Files\Steam\SteamApps\common\Awesomenauts\Data\Replays",
            @"E:\Program Files\Steam\SteamApps\common\Awesomenauts\Data\Replays"
        };

        const int MAX = MoveInfo.MAX_COORD;
        static readonly Rectangle MAP_BOUNDS = new Rectangle((int)(MAX * 0.40f), (int)(MAX * 0.1f), (int)(MAX * 0.4f), (int)(MAX * 0.4f));

        private CharacterBlock[] blocks;
        private BitData data;

        private Rectangle animBox = new Rectangle(0, 0, 50, 50);

        private Dictionary<string, Bitmap> iconMap = new Dictionary<string, Bitmap>();

        public MainForm()
        {
            InitializeComponent();
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
            load(DEFAULT_FILE);
            foreach (string dir in DEFAULT_DIRS)
            {
                if (Directory.Exists(dir))
                {
                    this.openFileDialog1.InitialDirectory = dir;
                }
            }
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

            Bitmap bmp = new Bitmap(this.pictureBoxMovement.Width, this.pictureBoxMovement.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(255, 255, 255, 255));
            this.pictureBoxMovement.Image = bmp;

            List<CharacterBlock> segs = new List<CharacterBlock>();

            ActorInfo currentInfo = null;
            string currentPlayer = null;
            CharacterBlock currentBlock = null;
            int currentAbilityStart = -1;
            for (int i = 0; i < data.Length; i++)
            {
                string alias = data.ReadString(i);
                ActorInfo info = ActorInfo.GetFromAlias(alias);
                if (info != null)
                {
                    if (currentInfo != null)
                    {
                        if (currentBlock == null)
                        {
                            Console.WriteLine(data.ReadBlock(currentAbilityStart, i - currentAbilityStart) + " - " + currentInfo);
                        }
                        else
                        {
                            currentBlock.info = currentInfo;
                            currentBlock.playerName = currentPlayer;
                            currentBlock.abilityStart = currentAbilityStart;
                            currentBlock.nameStart = currentAbilityStart - (currentInfo.alias.Length + 1) * 8;
                            if (currentPlayer != null) currentBlock.nameStart -= (currentPlayer.Length + 1) * 8;
                            segs.Add(currentBlock);
                        }
                        currentBlock = null;
                    }

                    currentInfo = info;
                    currentPlayer = null;
                    if (info.isHero)
                    {
                        int j;
                        for (j = 2; j < 20; j++)
                        {
                            int index = i - j * 8;
                            if (index < 0) break;
                            char c = (char) data.ReadInt(i - j * 8, 8);
                            if (c < 32 || c > 126) break;
                        }
                        j--;
                        j--; // Because this strategy always seems to overestimate by one character
                        currentPlayer = data.ReadString(i - j * 8);
                    }
                    currentAbilityStart = i + 8 * (alias.Length + 1);
                    i = currentAbilityStart;
                }

                if (currentInfo != null)
                {
                    int v = data.ReadInt(i, 22) & ~0x1FC;
                    if (true || v == 0x3F8003)
                    {
                        CharacterBlock moves = CharacterBlock.Read(data, i + 22);
                        //if (moves == null) moves = CharacterBlock.Read(data, i + 32);
                        if (moves != null && (currentBlock == null || moves.bitLength > currentBlock.bitLength))
                        {
                            currentBlock = moves;
                            //i = moves.RightBit;
                        }
                    }
                }
            }
            if (currentInfo != null && currentBlock  != null)
            {
                currentBlock.info = currentInfo;
                currentBlock.playerName = currentPlayer;
                currentBlock.abilityStart = currentAbilityStart;
                currentBlock.nameStart = currentAbilityStart - (currentInfo.alias.Length + 1) * 8;
                if (currentPlayer != null) currentBlock.nameStart -= (currentPlayer.Length + 1) * 8;
                segs.Add(currentBlock);
                currentBlock = null;
            }
            blocks = segs.ToArray();

            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (CharacterBlock block in blocks)
            {
                string iconName = block.info.icon;
                if (iconMap.ContainsKey(iconName)) continue;

                Bitmap icon = null;
                try
                {
                    Stream stream = assembly.GetManifestResourceStream("AwesomeReplays.Resources.Avatars." + iconName);
                    icon = new Bitmap(stream);
                }
                catch (Exception e) { }

                iconMap.Add(iconName, icon);
            }

            for (int i = 0; i < blocks.Length; i++)
                if (blocks[i].playerName == "dud88_dud") 
                    charIndex = i;

            searchTest();
            //gcdTest();
            refreshAnimations();

            refreshMovement();
        }

        private void searchTest()
        {

            //int index = 1;
            //int start = index == 0 ? 0 : blocks[index - 1].RightBit;
            //Console.WriteLine(data.ReadBlock(start, blocks[index].nameStart - start) + " - " + blocks[index].playerName);

            foreach (CharacterBlock block in blocks)
            {
                int end = block.abilityStart;
                int diff = block.index - block.abilityStart;
                Console.WriteLine(data.ReadBlock(end, Math.Min(512, diff)) + " " + block.info.name + ": " + diff);
            }

            //for (int i = 0; i < blocks.Length - 1; i++)
            //{
            //    CharacterBlock block = blocks[i];
            //    int nextAliasStart = blocks[i + 1].abilityStart - (blocks[i + 1].name.Length + 1) * 8;
            //    int dif = nextAliasStart - block.RightBit;
            //    Console.WriteLine(data.ReadBlock(block.RightBit, Math.Min(512, dif)) + " " + block.name + ": " + dif);
            //}

            //Console.WriteLine(s + ": " + i);
            //Console.WriteLine(data.ReadBlock(i - 128, 128));
            //Console.Write(data.ReadBlock(i + (s.Length + 1) * 8, 128));
            //Console.WriteLine();
            //int diff = (blocks[idx].index - end);
            //int last = idx == 0 ? 0 : (blocks[idx - 1].index + blocks[idx - 1].bitLength);
            //int next = data.ReadInt(end, 7);
            //Console.WriteLine(i - last);
            //if (i - last == 470)
            //{
            //    Console.WriteLine(data.ReadBlock(last, i - last));
            //}

        }

        private void printStrings(string path)
        {
            string s = "";
            for (int j = 0; j < 8; j++)
            {
                for (int i = j; i < data.Length - 7; i += 8)
                {
                    char c = (char)data.ReadInt(i, 8);
                    if (c >= 32 && c <= 126) s += c;
                }
                s += "\n";
            }
            File.WriteAllText(path, s);
        }

        private static void gcdTest()
        {
            for (int d = -1; d <= 1; d += 2)
            {
                int[] ns = new int[] {
                    151, 30, 85
                };

                int min = ns.Min();
                for (int i = 0; i < min; i++)
                {
                    int g = gcd(ns);
                    if (g > 1)
                    {
                        Console.WriteLine(g);
                    }
                    for (int j = 0; j < ns.Length; j++)
                    {
                        ns[j] += d;
                    }
                }
            }
        }

        int filter = 2, numberBits = 13, charIndex = 0;
        private void refreshAnimations(bool refreshList = true)
        {
            filter %= 29;
            numberBits %= 28;
            if (blocks.Length < 2) return;
            Bitmap bmp = new Bitmap(this.pictureBoxAnimations.Width, this.pictureBoxAnimations.Height);
            Graphics g = Graphics.FromImage(bmp);
            charIndex %= (blocks.Length - 1);
            int start = blocks[charIndex].RightBit;
            int length = blocks[charIndex + 1].abilityStart - (blocks[charIndex + 1].info.name.Length + 1) * 8 - start;
            //Console.WriteLine(data.ReadBlock(start, length));

            List<string> points = new List<string>();
            for (int i = 0; i < length; i++)
            {
                if (i % 29 != filter) continue;
                //if (data.ReadInt(start + i + 25, 4) != 3) continue;
                int number = data.ReadInt(start + i, numberBits);
                int full = data.ReadInt(start + i, 29);
                int x = bmp.Width * i / length;
                int y = (int)((long)bmp.Height * number / ((1 << numberBits) + 1));
                //int y = (int)((long)bmp.Height * full / (1 << 29));
                if (animBox.Contains(x, y))
                {
                    int rest = data.ReadInt(start + i + numberBits, 29 - numberBits);
                    points.Add(string.Format("{0:D4}: {1:F4} - {2} - {3}", i, (float)number / (1 << numberBits), number.ToBinary(numberBits), rest.ToBinary(29 - numberBits)));
                    if (points.Count - 1 == this.listBoxAnimations.SelectedIndex)
                        g.DrawEllipse(Pens.Red, new Rectangle(x - 2, y - 2, 5, 5));
                }
                bmp.SetPixel(x, y, Color.Black);
            }

            g.DrawRectangle(Pens.Black, animBox);

            if (refreshList)
            {
                this.listBoxAnimations.Items.Clear();
                this.listBoxAnimations.Items.AddRange(points.Select(p => p.ToString()).ToArray());
            }

            this.pictureBoxAnimations.Image = bmp;
            this.pictureBoxAnimations.Refresh();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }

        private void refreshMovement()
        {
            Bitmap bmp = (Bitmap)this.pictureBoxMovement.Image;
            if (bmp == null) return;
            if (bmp.Width != this.pictureBoxMovement.Width || bmp.Height != this.pictureBoxMovement.Height)
            {
                bmp = new Bitmap(this.pictureBoxMovement.Width, this.pictureBoxMovement.Height);
                this.pictureBoxMovement.Image = bmp;
            }
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);

            int time = this.trackBarMovement.Value;
            int index = 0;
            List<RectangleF> labels = new List<RectangleF>();
            foreach (CharacterBlock block in blocks)
            {
                Bitmap icon = iconMap[block.info.icon];

                int r = index * 255 / blocks.Length;
                Color color = Color.FromArgb(255, r, 0, 255 - r);
                Color alphaColor = Color.FromArgb(100, color);

                int iconSize = 31;

                PointF last = new PointF();
                foreach (MoveInfo move in block.moves)
                {
                    PointF pos = convertMove(move, bmp);
                    if (!last.IsEmpty) g.DrawLine(new Pen(alphaColor), last.X, last.Y, pos.X, pos.Y);
                    last = pos;
                    if (move.time >= time)
                    {
                        if (icon == null)
                        {
                            g.FillEllipse(new SolidBrush(color), new RectangleF(pos.X - 2, pos.Y - 2, 5, 5));
                        }
                        else
                        { 
                            RectangleF rect = new RectangleF(pos.X - iconSize / 2, pos.Y - iconSize / 2, iconSize, iconSize);
                            g.DrawRectangle(new Pen(alphaColor, 3), rect.X, rect.Y, rect.Width, rect.Height);
                            g.DrawImage(icon, rect);
                        }

                        break;
                    }
                }

                if (block.info.isHero || icon == null)
                {
                    //string labelText = block.playerName == null ? block.info.name : block.playerName;
                    string labelText = block.info.name;
                    Font labelFont = new Font("Arial", 8);
                    SizeF labelSize = g.MeasureString(labelText, labelFont);
                    PointF labelPos = last;
                    labelPos.X -= labelSize.Width / 2;
                    labelPos.Y += iconSize / 2 + 1;
                    RectangleF labelRect = new RectangleF(labelPos, labelSize);
                    bool repos = false;
                    while (true)
                    {
                        bool brk = true;
                        foreach (RectangleF rect in labels)
                        {
                            if (rect.IntersectsWith(labelRect))
                            {
                                labelRect.Y = rect.Bottom + 1;
                                repos = true;
                                brk = false;
                            }
                        }
                        if (brk) break;
                    }
                    labels.Add(labelRect);
                    labelPos.X = labelRect.X; labelPos.Y = labelRect.Y;
                    //if (repos)
                    //{
                    //    g.DrawLine(new Pen(Color.FromArgb(30, color)), labelRect.X, labelRect.Y, last.X - 16, last.Y + 16);
                    //}
                    g.DrawString(labelText, labelFont, new SolidBrush(color), labelPos);
                }

                index++;
            }

            this.pictureBoxMovement.Refresh();
        }

        private PointF convertMove(MoveInfo move, Bitmap bmp)
        {
            float x = (move.x - MAP_BOUNDS.X) / (float)MAP_BOUNDS.Width, y = (move.y - MAP_BOUNDS.Y) / (float)MAP_BOUNDS.Height;
            float px = ((bmp.Width - 1) * x), py = bmp.Height - 1 - ((bmp.Width - 1) * y);
            return new PointF(px, py);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            refreshMovement();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            refreshMovement();
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

        private void pictureBoxAnimations_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                animBox.X = e.X;
                animBox.Y = e.Y;
                refreshAnimations();
            }
        }

        private void listBoxAnimations_SelectedIndexChanged(object sender, EventArgs e)
        {
            refreshAnimations(false);
        }

        private void pictureBoxAnimations_Click(object sender, EventArgs e)
        {
        }

        private void pictureBoxAnimations_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                filter++;
                //numberBits++;
                //charIndex++;
                refreshAnimations(false);
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
