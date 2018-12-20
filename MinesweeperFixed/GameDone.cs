using DrawPanelLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MinesweeperFixed
{
    public class GameDone
    {
        bool won;
        Random rand;
        protected UII ui;
        protected DrawingPanel p;

        public static GameDone instance;

        public GameDone(bool won, Random rand)
        {
            instance = this;
            this.won = won;
            this.rand = rand;
            
            DrawingPanel p = new DrawingPanel(340, 160);
            this.p = p;
            p.HideWindow();
        }

        public void Display()
        {
            p.ShowWindow();
            p.GetGraphics().Clear(Color.White);
            UII ui = new UII(p);
            this.ui = ui;

            string t = "";

            if (won)
                t = "Congradulations! You won!";
            else
                t = "I'm sorry, you lost!";
            Label title = new Label(0, 0, 17, 3, t);
            Label playagain = new Label(0, 3, 17, 1, "Would you like to play again?");

            Button yes = new Button(1, 5, 7, 2, new Border(5, Color.Black), "Yes", true);
            Button no = new Button(9, 5, 7, 2, new Border(5, Color.Black), "No", false);

            ui.AddItem(title);
            ui.AddItem(playagain);
            ui.AddItem(yes);
            ui.AddItem(no);

            ui.Start();

            Graphics g = p.GetGraphics();

            p.RefreshDisplay();
        }

        public void Stop()
        {
            this.ui.Stop();
            p.HideWindow();
        }

        public class Border
        {
            public int width { get; set; }
            public Color color { get; set; }

            public Border(int width, Color c)
            {
                this.color = c;
                this.width = width;
            }

            public Border(int width)
            {
                this.width = width;
                this.color = Color.Black;
            }

        }

        public class Button : UIItem
        {
            bool result;

            public Button(int x, int y, int w, int h, Border b, string text, bool result) :
                base(x, y, w, h, b, text)
            {
                this.result = result;
            }

            public override void OnClick()
            {
                if (result)
                {
                    GameDone.instance.Stop();
                }
                else
                {
                    MineSweeper.instance.playing = false;
                    GameDone.instance.Stop();
                }
            }
        }

        public class Label : UIItem
        {
            public Label(int x, int y, int w, int h, string text) :
                base(x, y, w, h, new Border(0), text)
            { }

            public override void OnClick() {}
        }

        public class UII
        {
            List<UIItem> items;
            object[,] grid = new object[25, 25];
            protected bool enabled;
            DrawingPanel p;
            Graphics g;

            public UII(DrawingPanel p)
            {
                this.p = p;
                this.g = p.GetGraphics();
                this.items = new List<UIItem>();
            }

            public void Start()
            {
                enabled = true;
                DrawUI();
                EventLoop();
            }

            public void Stop()
            {
                enabled = false;
            }

            private void EventLoop()
            {
                while (enabled)
                    try
                    {
                        if (p.Input.ClickAvailable)
                        {
                            UI.ClickInfo click = p.Input.ReadClick();
                            if (click.LeftClick)
                            {
                                int xCord = click.X / 20;
                                int yCord = click.Y / 20;
                                foreach (UIItem item in items)
                                {
                                    Dictionary<int, List<int>> plots = item.GetPlots();
                                    if (plots.Keys.Contains(xCord))
                                        if (plots[xCord].Contains(yCord))
                                            item.OnClick();
                                }
                            }
                        }
                    }
                    catch (System.Threading.ThreadAbortException)
                    {
                        enabled = false;
                        continue;
                    }
            }

            public void AddItem(UIItem item)
            {
                items.Add(item);
                DrawItem(item);
            }

            private void DrawItem(UIItem item)
            {
                item.Draw(g);
                p.RefreshDisplay();
            }

            private void DrawUI()
            {
                foreach (UIItem item in items)
                    item.Draw(g);
                p.RefreshDisplay();
            }

        }

        public abstract class UIItem
        {
            protected PointF loc { get; set; }
            protected SizeF size { get; set; }
            protected string text { get; set; }
            protected Border border { get; set; }
            protected Dictionary<int, List<int>> plots = new Dictionary<int, List<int>>();

            public UIItem(int x, int y, int w, int h, Border b, string text)
            {
                this.loc = new PointF(x, y);
                this.size = new SizeF(w, h);
                this.border = b;
                this.text = text;
                for (int dy = 0; dy < h; dy++)
                    for (int dx = 0; dx < w; dx++)
                        if (plots.ContainsKey(x + dx))
                            plots[x + dx].Add(y + dy);
                        else
                            plots.Add(x + dx, new List<int> { y + dy });
            }

            public Dictionary<int, List<int>> GetPlots()
            {
                return plots;
            }

            public virtual void Draw(Graphics g)
            {
                // Border
                Pen p = new Pen(border.color);
                for (int i = 0; i < border.width; i++)
                    g.DrawRectangle(p, loc.X * 20 + i, loc.Y * 20 + i, size.Width * 20 - i * 2, size.Height * 20 - i * 2);
                // Text
                SolidBrush b = new SolidBrush(Color.Black);
                Font f = new Font("Arial", 12);
                StringFormat sf = new StringFormat();
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;
                g.DrawString(this.text, f, b, new Rectangle((int)loc.X * 20, (int)loc.Y * 20, (int)size.Width * 20, (int)size.Height * 20), sf);
            }

            public abstract void OnClick();

        }

    }
}
