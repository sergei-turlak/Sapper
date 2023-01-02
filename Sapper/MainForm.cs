using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace Sapper
{
    public partial class MainForm : Form
    {
        static Random rnd = new Random();
        bool FirstClick { get; set; } = true;
        static int MapSize { get; set; } = 9;
        static int CountBomb { get; set; } = MapSize + 1;
        static bool[] isBomb = new bool[MapSize * MapSize]; // if true, there bomb (in POSITION)
        int counter { get; set; } = 0;
        int counterTip { get; set; } = 0;
        int[] BombPosition = new int[CountBomb];
        bool[] PsevdoMap = new bool[isBomb.Length]; // this psevdo map is for Tip(view method on Click "Tip")
        enum State
        {
            Game,
            Win,
            Death
        }

        Color[] colors = new Color[9]
        {
            Color.Black,
            Color.Blue,
            Color.Green,
            Color.Red,
            Color.DarkBlue,
            Color.DarkRed,
            Color.Black,
            Color.Black,
            Color.Black
        };

        public MainForm()
        {
            InitializeComponent();
        }

        async void tableLayoutPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            int position = (sender as Button).TabIndex;
            if (FirstClick)
            {
                await Task.Run(() => MiningMap(position));
                подсказка.Visible = true;
            }
            // Make Flag and Delete it...
            if (e.Button == MouseButtons.Left && button(position).BackColor == SystemColors.ActiveCaption) Step(position);
            else if (e.Button == MouseButtons.Right)
            {
                if (button(position).BackColor == SystemColors.ActiveCaption)
                {
                    button(position).FlatStyle = FlatStyle.Flat;
                    button(position).BackColor = Color.Yellow;
                }
                else if (button(position).BackColor == Color.Yellow)
                {
                    button(position).FlatStyle = FlatStyle.System;
                    button(position).BackColor = SystemColors.ActiveCaption;
                }
            }
        }

        void tmr_Tick(object sender, EventArgs e)
        {
            if (lblLine.Location.Y <= 0)
            {
                for (int i = 0; i < 81; i++)
                    button(i).Enabled = true;
                lblLine.Visible = false;
                tmr.Enabled = false;
                return;
            }

            lblLine.Location = new Point(lblLine.Location.X, lblLine.Location.Y - 5);

            for (int i = 0; i < 81; i++) // Прориcовка всех кнопок no покоторым полоса уже прошлась
            {
                if (lblLine.Location.Y < button(i).Location.Y + button(i).Size.Height)
                {
                    if (isBomb[i])
                    {
                        button(i).FlatStyle = FlatStyle.Flat;
                        button(i).BackColor = Color.Red;
                        button(i).Text = "";
                    }
                    else
                    {
                        button(i).FlatStyle = FlatStyle.System;
                        button(i).BackColor = SystemColors.ActiveCaption;
                        button(i).Text = "";
                    }
                }
                button(i).Enabled = false;
            }
        }

        void MiningMap(int position) // растановка мин 
        {
            int bomb = rnd.Next(MapSize * MapSize);
            var Neighbour = GetNeighbour(GetPoint(position));

            for (counter = 0; counter != CountBomb;) // while bomb on Map != CountBomb
            {
                bomb = rnd.Next(81);
                if (bomb != position && !isBomb[position] && CheckBomb(Neighbour, bomb)) isBomb[bomb] = true;   //Check button up , down ...
                counter = 0;
                for (int j = 0; j < isBomb.Length; j++) if (isBomb[j]) counter++; // Set Counter (Count of Min)
            }
            FirstClick = false;
            counter = 0;
        }

        void Step(int position)  // Click at Button
        {
            if (CheckState(position) == State.Death)
            {
                Death();
                return;
            }
            // Get number on button , which around with bomb(one or more)
            int num = GetNumber(GetPoint(position));

            button(position).FlatStyle = FlatStyle.Popup;
            button(position).BackColor = SystemColors.ActiveBorder;
            button(position).ForeColor = colors[num];

            if (num != 0) button(position).Text = num.ToString();
            else GetSpace(GetPoint(position));
            if (CheckState(position) == State.Win)
            {
                TimerOn();
                Victory();
            }
        }

        int GetNumber(Point point)   // get number on button , which around bomb (one or more)
        {
            counter = 0;
            var Neighbour = GetNeighbour(point);

            for (int i = 0; i < Neighbour.Length; i++)
            {
                int pos = GetPosition(Neighbour[i]);
                if (isBomb[pos]) counter++;  // if button position not on frame
            }

            return counter;
        }

        void GetSpace(Point point) // if click on empty but delete all empty but around
        {
            void GetSides(Point p, int n)
            {
                if (p.X < 0 || p.X >= MapSize - 1 || p.Y < 0 || p.Y >= MapSize - 1) return;

                button(GetPosition(p)).FlatStyle = FlatStyle.Popup;
                button(GetPosition(p)).BackColor = SystemColors.ActiveBorder;
                button(GetPosition(p)).Text = GetNumber(p) == 0 ? "" : GetNumber(p).ToString();
                button(GetPosition(p)).ForeColor = colors[GetNumber(p)];

                if (GetNumber(p) > 0) return;

                for (int x = -1; x <= 1; x++)
                    for (int y = 1; y <= 1; y++)
                        GetSides(new Point(p.X + x * n, p.Y + y * n), n);
            }
            void GetSidesInersion(Point p, int n)
            {
                if (p.X < 0 || p.X >= MapSize - 1 || p.Y < 0 || p.Y >= MapSize - 1) return;

                button(GetPosition(p)).FlatStyle = FlatStyle.Popup;
                button(GetPosition(p)).BackColor = SystemColors.ActiveBorder;
                button(GetPosition(p)).Text = GetNumber(p) == 0 ? "" : GetNumber(p).ToString();
                button(GetPosition(p)).ForeColor = colors[GetNumber(p)];

                if (GetNumber(p) > 0) return;

                for (int x = 1; x <= 1; x++)
                    for (int y = -1; y <= 1; y++)
                        GetSidesInersion(new Point(p.X + x * n, p.Y + y * n), n);
            }

            GetSides(point, 1);
            GetSidesInersion(point, 1);
            GetSides(point, -1);
            GetSidesInersion(point, -1);
        }

        State CheckState(int position)  // return different state of game
        {
            if (isBomb[position] == true)
            {
                button(position).FlatStyle = FlatStyle.Flat;
                button(position).BackColor = Color.Red;
                return State.Death;
            }

            counter = 0; // Checking at Victory
            for (int i = 0; i < 81; i++)
            {
                if (button(i).BackColor == SystemColors.ActiveBorder) counter++;
            }

            if (counter == MapSize * MapSize - CountBomb)
            {
                return State.Win;
            }
            counter = 0;
            return State.Game;
        }

        bool CheckBomb(Point[] neighbour, int bomb) //Check neighbour button on..
        {
            for (int i = 0; i < neighbour.Length; i++)
            {
                int position = GetPosition(neighbour[i]);
                if (bomb == position) return false;
            }
            return true;
        }

        void Victory()
        {
            for (int i = 0; i < 81; i++)
                if (isBomb[i])
                {
                    button(i).FlatStyle = FlatStyle.Flat;
                    button(i).BackColor = Color.Red;
                }

            MessageBox.Show("ВЫ ВЫИГРАЛИ!!!", "Поздравление!!");
            ClearMap();
        }

        void Death()
        {
            for (int i = 0; i < 81; i++)
                if (isBomb[i])
                {
                    button(i).FlatStyle = FlatStyle.Flat;
                    button(i).BackColor = Color.Red;
                }
            MessageBox.Show("Вы проиграли(");
            ClearMap();
        }

        void ClearMap()
        {
            for (int i = 0; i < 81; i++)
            {
                button(i).FlatStyle = FlatStyle.System;
                button(i).BackColor = SystemColors.ActiveCaption;
                isBomb[i] = false;
                button(i).Text = "";
                button(i).ForeColor = Color.Black;
            }
            lblLine.Visible = false;
            FirstClick = true;
            подсказка.Visible = false;
            counterTip = 0;
            подсказка.Text = $"Подсказка ({CountBomb - counterTip})";
            TimerOn();
        } // return early interface of map and delete bomb

        Point[] GetNeighbour(Point point)
        {
            Point[] p = new Point[8];   // simple neighbour(8)
            p[0] = new Point(point.X - 1, point.Y - 1);
            p[1] = new Point(point.X, point.Y - 1);
            p[2] = new Point(point.X + 1, point.Y - 1);
            p[3] = new Point(point.X - 1, point.Y);
            p[4] = new Point(point.X + 1, point.Y);
            p[5] = new Point(point.X - 1, point.Y + 1);
            p[6] = new Point(point.X, point.Y + 1);
            p[7] = new Point(point.X + 1, point.Y + 1);


            if (point.X > 0 && point.X < 8 && point.Y > 0 && point.Y < 8) // simple neighbour(8)
            {
                return p;
            }
            #region If Button on frame...

            else if (point.X == 0 && point.Y == 0) // frame neighbour(3)
            {
                Point[] p1 = new Point[3];
                p1[0] = p[4];
                p1[1] = p[6];
                p1[2] = p[7];
                return p1;
            }
            else if (point.X == 8 && point.Y == 0)

            {
                Point[] p1 = new Point[3];
                p1[0] = p[3];
                p1[1] = p[5];
                p1[2] = p[6];
                return p1;
            }
            else if (point.X == 0 && point.Y == 8)
            {
                Point[] p1 = new Point[3];
                p1[0] = p[1];
                p1[1] = p[2];
                p1[2] = p[4];
                return p1;
            }
            else if (point.X == 8 && point.Y == 8)
            {
                Point[] p1 = new Point[3];
                p1[0] = p[0];
                p1[1] = p[1];
                p1[2] = p[3];
                return p1;
            }
            else if (point.X == 0) // on Side Neighbour(5)
            {
                Point[] p1 = new Point[5];
                p1[0] = p[1];
                p1[1] = p[2];
                p1[2] = p[4];
                p1[3] = p[6];
                p1[4] = p[7];
                return p1;
            }
            else if (point.X == 8) // on Side Neighbour(5)
            {
                Point[] p1 = new Point[5];
                p1[0] = p[0];
                p1[1] = p[1];
                p1[2] = p[3];
                p1[3] = p[5];
                p1[4] = p[6];
                return p1;
            }
            else if (point.Y == 0) // on Side Neighbour(5)
            {
                Point[] p1 = new Point[5];
                p1[0] = p[3];
                p1[1] = p[4];
                p1[2] = p[5];
                p1[3] = p[6];
                p1[4] = p[7];
                return p1;
            }
            else if (point.Y == 8)// on Side Neighbour(5)
            {
                Point[] p1 = new Point[5];
                p1[0] = p[0];
                p1[1] = p[1];
                p1[2] = p[2];
                p1[3] = p[3];
                p1[4] = p[4];
                return p1;
            }
            return p;
            #endregion
        }  // get coords of neighbour

        Point GetPoint(int position)
        {
            int x = position % 9;
            int y = position / 9;
            return new Point(x, y);
        }

        int GetPosition(Point point)
        {
            return point.Y * 9 + point.X;
        }

        public Button button(int position)
        {
            switch (position)
            {
                case 0: return button0;
                case 1: return button1;
                case 2: return button2;
                case 3: return button3;
                case 4: return button4;
                case 5: return button5;
                case 6: return button6;
                case 7: return button7;
                case 8: return button8;
                case 9: return button9;
                case 10: return button10;
                case 11: return button11;
                case 12: return button12;
                case 13: return button13;
                case 14: return button14;
                case 15: return button15;
                case 16: return button16;
                case 17: return button17;
                case 18: return button18;
                case 19: return button19;
                case 20: return button20;
                case 21: return button21;
                case 22: return button22;
                case 23: return button23;
                case 24: return button24;
                case 25: return button25;
                case 26: return button26;
                case 27: return button27;
                case 28: return button28;
                case 29: return button29;
                case 30: return button30;
                case 31: return button31;
                case 32: return button32;
                case 33: return button33;
                case 34: return button34;
                case 35: return button35;
                case 36: return button36;
                case 37: return button37;
                case 38: return button38;
                case 39: return button39;
                case 40: return button40;
                case 41: return button41;
                case 42: return button42;
                case 43: return button43;
                case 44: return button44;
                case 45: return button45;
                case 46: return button46;
                case 47: return button47;
                case 48: return button48;
                case 49: return button49;
                case 50: return button50;
                case 51: return button51;
                case 52: return button52;
                case 53: return button53;
                case 54: return button54;
                case 55: return button55;
                case 56: return button56;
                case 57: return button57;
                case 58: return button58;
                case 59: return button59;
                case 60: return button60;
                case 61: return button61;
                case 62: return button62;
                case 63: return button63;
                case 64: return button64;
                case 65: return button65;
                case 66: return button66;
                case 67: return button67;
                case 68: return button68;
                case 69: return button69;
                case 70: return button70;
                case 71: return button71;
                case 72: return button72;
                case 73: return button73;
                case 74: return button74;
                case 75: return button75;
                case 76: return button76;
                case 77: return button77;
                case 78: return button78;
                case 79: return button79;
                case 80: return button80;
                default: return button40;
            }
        } // Find Button with index 

        void TimerOn()
        {
            lblLine.Location = new Point(0, base.Size.Height);
            lblLine.Visible = true;
            tmr.Enabled = true;
        }

        void MainForm_SizeChanged(object sender, EventArgs e)
        {
            Height = Width;
        }

        void MainForm_Load(object sender, EventArgs e)
        {
            TimerOn();
        }

        void новаяИграToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TimerOn();
            ClearMap();
        }

        void подсказкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < PsevdoMap.Length && counterTip == 0; i++)
                PsevdoMap[i] = isBomb[i];

            for (int i = 0; i < BombPosition.Length && counterTip == 0; i++) // Запись в массив позиций бомб
                for (int j = 0; j < PsevdoMap.Length; j++)
                    if (PsevdoMap[j])
                    {
                        BombPosition[i] = j;
                        PsevdoMap[j] = false;
                        break;
                    }

            for (int i = BombPosition.Length - 1; i > 0 && counterTip == 0; i--) // mix array
            {
                int j = rnd.Next(i + 1);
                int temp = BombPosition[i];
                BombPosition[i] = BombPosition[j];
                BombPosition[j] = temp;
            }
            // Мы получили смешаный массив позиций всех бомб...

            button(BombPosition[counterTip]).FlatStyle = FlatStyle.Flat;
            button(BombPosition[counterTip++]).BackColor = Color.Red;

            if (counterTip == CountBomb)
            {
                MessageBox.Show("Подсказки закончились");
                counterTip = 0;
                подсказка.Visible = false;
            }
            подсказка.Text = $"Подсказка ({CountBomb - counterTip})";
        }

        private void tbChangeCountMin_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;
            if (!(ch >= '0' && ch <= '9') && ch != (char)Keys.Back) e.Handled = true;
        }
    }
}