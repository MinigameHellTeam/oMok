using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Security.Policy;
using oMok.Properties;
using System.Windows.Forms.VisualStyles;

enum STONE
{
    none,
    black,
    white
};

namespace oMok
{
    public partial class Form1 : Form
    {
        STONE[,] oMokBoard = new STONE[19, 19];
        bool flag = false;
        bool imageFlag = true;

        bool startFlag = false;
        bool pauseFlag = false;

        int margin = 40;
        int gridSize = 30;
        int stoneSize = 28;
        int pointSize = 10;
        int bStoneCnt = 0;
        int wStoneCnt = 0;

        List<Save> listRevive = new List<Save>();
        int sequence = 0;
        bool reviveFlag = false;

        Graphics g;
        Pen pen;
        Brush whiteStone, blackStone;

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            DrawBoard();
            DrawStone();
        }

        public Form1()
        {
            InitializeComponent();

            pen = new Pen(Color.Black);
            blackStone = new SolidBrush(Color.Black);
            whiteStone = new SolidBrush(Color.White);

            this.BackColor = Color.NavajoWhite;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            timer.Interval = 1000;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (startFlag == false)
            {
                MessageBox.Show("시작 버튼을 눌러야 게임을 시작할수 있습니다!");
                return;
            }
            if (pauseFlag == true)
            {
                MessageBox.Show("게임을 다시 시작해야 합니다!");
                return;
            }

            int x = (e.X - margin + gridSize / 2) / gridSize;
            int y = (e.Y - margin + gridSize / 2) / gridSize;

            try
            {
                if (oMokBoard[x, y] != STONE.none)
                    return;
            }
            catch
            {
                MessageBox.Show("오목판 안에만 클릭해주세요!");
                return;
            }
            

            Rectangle r = new Rectangle(margin + gridSize * x - stoneSize / 2, margin + gridSize * y - stoneSize / 2, stoneSize, stoneSize);

            if (flag == false)
            {
                if (imageFlag == false)
                    g.FillEllipse(blackStone, r);
                else
                {
                    Bitmap bmp = new Bitmap(Resources.black);
                    g.DrawImage(bmp, r);
                }
                lblOrder.Text = "백돌";
                lblTime.Text = "10";
                bStoneCnt++;
                flag = true;
                oMokBoard[x, y] = STONE.black;
            }
            else
            {
                if (imageFlag == false)
                    g.FillEllipse(whiteStone, r);
                else
                {
                    Bitmap bmp = new Bitmap(Resources.white);
                    g.DrawImage(bmp, r);
                }
                lblOrder.Text = "흑돌";
                lblTime.Text = "10";
                wStoneCnt++;
                flag = false;
                oMokBoard[x, y] = STONE.white;
            }

            checkOmok(x, y);
        }

        private void DrawBoard()
        {
            g = panel1.CreateGraphics();

            for (int i = 0; i < 19; i++)
            {
                g.DrawLine(pen, new Point(margin + i * gridSize, margin), new Point(margin + i * gridSize, margin + 18 * gridSize));
            }

            for (int i = 0; i < 19; i++)
            {
                g.DrawLine(pen, new Point(margin, margin + i * gridSize), new Point(margin + 18 * gridSize, margin + i * gridSize));
            }

            for (int x = 3; x <= 15; x += 6)
                for (int y = 3; y <= 15; y += 6)
                {
                    g.FillEllipse(blackStone, margin + gridSize * x - pointSize / 2, margin + gridSize * y - pointSize / 2, pointSize, pointSize);
                }
        }

        private void DrawStone()
        {
            for (int x = 0; x < 19; x++)
                for (int y = 0; y < 19; y++)
                    if (oMokBoard[x, y] == STONE.white)
                    {
                        if (imageFlag == false)
                            g.FillEllipse(whiteStone, margin + x * gridSize - stoneSize / 2, margin + y * gridSize - stoneSize / 2, stoneSize, stoneSize);
                        else
                        {
                            Bitmap bmp = new Bitmap(Resources.white);
                            g.DrawImage(bmp, margin + x * gridSize - stoneSize / 2, margin + y * gridSize - stoneSize / 2, stoneSize, stoneSize);
                        }
                    }
                    else if (oMokBoard[x, y] == STONE.black)
                    {
                        if (imageFlag == false)
                            g.FillEllipse(blackStone, margin + x * gridSize - stoneSize / 2, margin + y * gridSize - stoneSize / 2, stoneSize, stoneSize);
                        else
                        {
                            Bitmap bmp = new Bitmap(Resources.black);
                            g.DrawImage(bmp, margin + x * gridSize - stoneSize / 2, margin + y * gridSize - stoneSize / 2, stoneSize, stoneSize);
                        }
                    }


        }

        private void checkOmok(int x, int y)
        {
            int cnt = 1;

            for (int i = x + 1; i <= 18; i++)
                if (oMokBoard[i, y] == oMokBoard[x, y])
                    cnt++;
                else
                    break;

            for (int i = x - 1; i >= 0; i--)
                if (oMokBoard[i, y] == oMokBoard[x, y])
                    cnt++;
                else
                    break;

            if (cnt >= 5)
            {
                OmokComplete(x, y);
                return;
            }

            cnt = 1;

            for (int i = y + 1; i <= 18; i++)
                if (oMokBoard[x, i] == oMokBoard[x, y])
                    cnt++;
                else
                    break;

            for (int i = y - 1; i >= 0; i--)
                if (oMokBoard[x, i] == oMokBoard[x, y])
                    cnt++;
                else
                    break;

            if (cnt >= 5)
            {
                OmokComplete(x, y);
                return;
            }

            cnt = 1;

            for (int i = x + 1, j = y - 1; i <= 18 && j >= 0; i++, j--)
                if (oMokBoard[i, j] == oMokBoard[x, y])
                    cnt++;
                else
                    break;

            for (int i = x - 1, j = y + 1; i >= 0 && j <= 18; i--, j++)
                if (oMokBoard[i, j] == oMokBoard[x, y])
                    cnt++;
                else
                    break;

            for (int i = x - 1, j = y + 1; i >= 0 && j <= 18; i--, j++)
                if (oMokBoard[i, j] == oMokBoard[x, y])
                    cnt++;
                else
                    break;

            if (cnt >= 5)
            {
                OmokComplete(x, y);
                return;
            }

            cnt = 1;

            for (int i = x - 1, j = y - 1; i >= 0 && j >= 0; i--, j--)
                if (oMokBoard[i, j] == oMokBoard[x, y])
                    cnt++;
                else
                    break;

            for (int i = x + 1, j = y + 1; i <= 18 && j <= 18; i++, j++)
                if (oMokBoard[i, j] == oMokBoard[x, y])
                    cnt++;
                else
                    break;

            if (cnt >= 5)
            {
                OmokComplete(x, y);
                return;
            }
        }
        private void OmokComplete(int x, int y)
        {
            timer.Stop();
            DialogResult res = MessageBox.Show(oMokBoard[x, y].ToString().ToUpper()
                + "Wins!\n새로운 게임을 시작할까요?", "게임 종료", MessageBoxButtons.YesNo);
            
            if (res == DialogResult.Yes)
                NewGame();
            else if (res == DialogResult.No)
                this.Close();
        }

        private void NewGame()
        {
            timer.Start();
            lblOrder.Text = "흑돌";
            lblTime.Text = "10";

            imageFlag = true;
            flag = false;

            for (int x = 0; x < 19; x++)
                for (int y = 0; y < 19; y++)
                    oMokBoard[x, y] = STONE.none;

            panel1.Refresh();
            bStoneCnt = 0;
            wStoneCnt = 0;
            DrawBoard();
            DrawStone();
        }


        private void 저장ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void 다시시작ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewGame();
            pauseFlag = false;
        }

        private void 끝내기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            lblTime.Text = (int.Parse(lblTime.Text) - 1).ToString();

            if ((int.Parse(lblTime.Text) == 3 && lblTime.Text == "2" && lblTime.Text == "1"))
                lblTime.ForeColor = Color.Red;
            else
                lblTime.ForeColor = Color.Black;

            if (lblTime.Text == "0")
            {
                timer.Stop();
                if (lblOrder.Text == "백돌")
                {

                    DialogResult res = MessageBox.Show("Black Wins!\n새로운 게임을 시작할까요?", "게임 종료", MessageBoxButtons.YesNo);
                    if (res == DialogResult.Yes)
                        NewGame();
                    else if (res == DialogResult.No)
                        this.Close();
                }
                else if(lblOrder.Text =="흑돌")
                {
                    DialogResult res = MessageBox.Show("White Wins!\n새로운 게임을 시작할까요?", "게임 종료", MessageBoxButtons.YesNo);
                    if (res == DialogResult.Yes)
                        NewGame();
                    else if (res == DialogResult.No)
                        this.Close();
                }

            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            timer.Start();
            startFlag = true;
            pauseFlag = false;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            pauseFlag = true;
            timer.Stop();
        }
    }
}
