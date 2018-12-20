using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using DrawPanelLibrary;

namespace MinesweeperFixed
{
    class MineSweeper
    {
        static Enum[,] map = new Enum[16, 16];
        static List<int> cleared = new List<int>();
        static Random rand = new Random();
        static DrawingPanel p;

        public static MineSweeper instance;

        public bool playing = true;

        const int boardsize = 256;

        enum SquareFill { Nothing, Hidden, Hit, Revealed, WrongMark, BombMark, FinishedWrong, FinishedCorrect }

        public MineSweeper()
        {
            instance = this;
            GameDone won = new GameDone(true, rand);
            GameDone lost = new GameDone(false, rand);

            p = new DrawingPanel(490, 490);
            while (playing)
            {
                Create();
                p.RefreshDisplay();
                while (!LostGame() && !HasWon())
                {
                    Play();
                    DrawMap();
                    try
                    {
                        p.RefreshDisplay();
                    }
                    catch (System.Threading.ThreadAbortException) { }
                }
                if (HasWon())
                {
                    won.Display();
                }
                else if (LostGame())
                {
                    DrawMap();
                    lost.Display();
                }
            }
            p.HideWindow();
        }
        
        private void Create()
        {
            Graphics g = p.GetGraphics();
            for (int r = 0; r < 16; r++)
                for (int c = 0; c < 16; c++)
                {
                    map[r, c] = SquareFill.Nothing;
                    g.FillRectangle(Brushes.Gray, r * 30 + 5, c * 30 + 5, 25, 25);
                }
            int addedBombs = 0;
            int neededBombs = 17;
            while (addedBombs < neededBombs)
            {
                for (int i = 0; i < boardsize; i++)
                {
                    int tryPlace = rand.Next(0, 6);
                    if (tryPlace == 5)
                    {
                        int y = i / 16;
                        int x = i % 16;
                        map[x, y] = SquareFill.Hidden;
                        addedBombs++;
                    }
                }
            }
            p.RefreshDisplay();
        }

        private int GetBombsNextTo(int x, int y)
        {
            int bombsnextto = 0;
            //List<int> bombNumbers = new List<int>() { 1, 2, 5, 7 };
            List<Enum> bombNumbers = new List<Enum>() { SquareFill.Hidden, SquareFill.Hit, SquareFill.BombMark, SquareFill.FinishedCorrect };
            if (y != 0 && x != 0 && bombNumbers.Contains(map[x - 1, y - 1]))
                bombsnextto++;
            if (y != 0 && bombNumbers.Contains(map[x, y - 1]))
                bombsnextto++;
            if (x != 15 && y != 0 && bombNumbers.Contains(map[x + 1, y - 1]))
                bombsnextto++;
            if (x != 0 && bombNumbers.Contains(map[x - 1, y]))
                bombsnextto++;
            if (bombNumbers.Contains(map[x, y]))
                bombsnextto++;
            if (x != 15 && bombNumbers.Contains(map[x + 1, y]))
                bombsnextto++;
            if (y != 15 && x != 0 && bombNumbers.Contains(map[x - 1, y + 1]))
                bombsnextto++;
            if (y != 15 && bombNumbers.Contains(map[x, y + 1]))
                bombsnextto++;
            if (x != 15 && y != 15 && bombNumbers.Contains(map[x + 1, y + 1]))
                bombsnextto++;
            return bombsnextto;
        }

        private void DrawMap()
        {
            Graphics g = p.GetGraphics();
            for (int cs = 0; cs < boardsize; cs++)
            {
                int xcord = cs % 16;
                int ycord = cs / 16;
                if (map[xcord, ycord].Equals(SquareFill.Nothing) || map[xcord, ycord].Equals(SquareFill.Hidden))
                    g.FillRectangle(Brushes.Gray, xcord * 30 + 5, ycord * 30 + 5, 25, 25);
                else if (map[xcord, ycord].Equals(SquareFill.Hit))
                    g.FillRectangle(Brushes.Black, xcord * 30 + 5, ycord * 30 + 5, 25, 25);
                else if (map[xcord, ycord].Equals(SquareFill.Revealed))
                {
                    g.FillRectangle(Brushes.LightGray, xcord * 30 + 5, ycord * 30 + 5, 25, 25);
                    // Count the bombs around the square
                    int bombsnextto = GetBombsNextTo(xcord, ycord);

                    if (bombsnextto != 0)
                    {
                        Font font = new Font("Arial", 16);
                        Brush b = BombNumberColor(bombsnextto);
                        g.DrawString(bombsnextto.ToString(), font, b, xcord * 30 + 8, ycord * 30 + 7);
                    }
                }
                else if (map[xcord, ycord].Equals(SquareFill.WrongMark) || map[xcord, ycord].Equals(SquareFill.BombMark))
                    g.FillRectangle(Brushes.Red, xcord * 30 + 5, ycord * 30 + 5, 25, 25);
                else if (map[xcord, ycord].Equals(SquareFill.FinishedWrong))
                    g.FillRectangle(Brushes.Red, xcord * 30 + 5, ycord * 30 + 5, 25, 25);
                else if (map[xcord, ycord].Equals(SquareFill.FinishedCorrect))
                    g.FillRectangle(Brushes.LightGreen, xcord * 30 + 5, ycord * 30 + 5, 25, 25);
            }
        }

        private Brush BombNumberColor(int bombsnextto)
        {
            Brush b = Brushes.Black;
            if (bombsnextto == 1)
                b = Brushes.Blue;
            else if (bombsnextto == 2)
                b = Brushes.Green;
            else if (bombsnextto == 3)
                b = Brushes.Red;
            else if (bombsnextto == 4)
                b = Brushes.Purple;
            else if (bombsnextto == 5)
                b = Brushes.Maroon;
            else if (bombsnextto == 6)
                b = Brushes.Turquoise;
            else if (bombsnextto == 7)
                b = Brushes.Black;
            else if (bombsnextto == 8)
                b = Brushes.Gray;
            return b;
        }

        private bool HasWon()
        {
            for (int i = 0; i < boardsize; i++)
            {
                int x = i % 16;
                int y = i / 16;
                if (map[x, y].Equals(SquareFill.Nothing))
                    return false;
            }
            return true;
        }

        private bool LostGame()
        {
            bool hitbomb = false;
            foreach (Enum e in map)
                if (e.Equals(SquareFill.Hit))
                    hitbomb = true;
            if (hitbomb == true)
            {
                for (int i = 0; i < boardsize; i++)
                {
                    int x = i % 16;
                    int y = i / 16;
                    if (map[x, y].Equals(SquareFill.WrongMark))
                        map[x, y] = SquareFill.FinishedWrong;
                    else if (map[x, y].Equals(SquareFill.BombMark))
                        map[x, y] = SquareFill.FinishedCorrect;
                    else if (map[x, y].Equals(SquareFill.Hidden))
                        map[x, y] = SquareFill.Hit;
                }
                DrawMap();
            }
            p.RefreshDisplay();

            return hitbomb;
        }

        private void Play()
        {
            Graphics g = p.GetGraphics();
            if (p.Input.ClickAvailable)
            {
                UI.ClickInfo click = p.Input.ReadClick();
                if (click.LeftClick)
                    RevealSquare(click);
                else if (click.RightClick)
                    MarkSquare(click);
            }
        }

        private void MarkSquare(UI.ClickInfo click)
        {
            try
            {
                int y = click.Y;
                int x = click.X;
                int ySquare = y / 30;
                int xSquare = x / 30;
                if (map[xSquare, ySquare].Equals(SquareFill.Nothing))
                    map[xSquare, ySquare] = SquareFill.WrongMark;
                else if (map[xSquare, ySquare].Equals(SquareFill.Hidden))
                    map[xSquare, ySquare] = SquareFill.BombMark;
                else if (map[xSquare, ySquare].Equals(SquareFill.WrongMark))
                    map[xSquare, ySquare] = SquareFill.Nothing;
                else if (map[xSquare, ySquare].Equals(SquareFill.BombMark))
                    map[xSquare, ySquare] = SquareFill.Hidden;
            }
            catch (IndexOutOfRangeException) { }
        }

        private void RevealSquare(UI.ClickInfo click)
        {
            try
            {
                int y = click.Y;
                int x = click.X;
                int ySquare = y / 30;
                int xSquare = x / 30;
                if (!map[xSquare, ySquare].Equals(SquareFill.Hidden) && !map[xSquare, ySquare].Equals(SquareFill.Hit))
                {
                    map[xSquare, ySquare] = SquareFill.Revealed;
                    if (GetBombsNextTo(xSquare, ySquare) == 0)
                        ClearBlanks(xSquare, ySquare);
                    else
                        cleared.Add(ySquare * 16 + xSquare);
                }
                else
                {
                    map[xSquare, ySquare] = SquareFill.Hit;
                }
            }
            catch (IndexOutOfRangeException) { }
        }

        private void ClearBlanks(int xSquare, int ySquare)
        {
            bool changed = true;
            while (changed == true)
            {
                bool haschanged = false;
                for (int i = 0; i < boardsize; i++)
                {
                    int x = i % 16;
                    int y = i / 16;

                    if (map[x, y].Equals(SquareFill.Revealed) && GetBombsNextTo(x, y) == 0)
                    {
                        if (x != 0 && y != 0 && !map[x - 1, y - 1].Equals(SquareFill.Revealed))
                        {
                            map[x - 1, y - 1] = SquareFill.Revealed;
                            haschanged = true;
                        }
                        if (x != 0 && !map[x - 1, y].Equals(SquareFill.Revealed))
                        {
                            map[x - 1, y] = SquareFill.Revealed;
                            haschanged = true;
                        }
                        if (x != 0 && y != 15 && !map[x - 1, y + 1].Equals(SquareFill.Revealed))
                        {
                            map[x - 1, y + 1] = SquareFill.Revealed;
                            haschanged = true;
                        }
                        if (y != 0 && !map[x, y - 1].Equals(SquareFill.Revealed))
                        {
                            map[x, y - 1] = SquareFill.Revealed;
                            haschanged = true;
                        }
                        if (!map[x, y].Equals(SquareFill.Revealed))
                        {
                            map[x, y] = SquareFill.Revealed;
                            haschanged = true;
                        }
                        if (y != 15 && !map[x, y + 1].Equals(SquareFill.Revealed))
                        {
                            map[x, y + 1] = SquareFill.Revealed;
                            haschanged = true;
                        }
                        if (x != 15 && y != 0 && !map[x + 1, y - 1].Equals(SquareFill.Revealed))
                        {
                            map[x + 1, y - 1] = SquareFill.Revealed;
                            haschanged = true;
                        }
                        if (x != 15 && !map[x + 1, y].Equals(SquareFill.Revealed))
                        {
                            map[x + 1, y] = SquareFill.Revealed;
                            haschanged = true;
                        }
                        if (x != 15 && y != 15 && !map[x + 1, y + 1].Equals(SquareFill.Revealed))
                        {
                            map[x + 1, y + 1] = SquareFill.Revealed;
                            haschanged = true;
                        }
                    }
                }
                if (haschanged == false)
                    changed = false;
            }
        }

    }
}
