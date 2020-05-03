using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Input;

namespace WpfApplication2
{
    public static class Config // Alternative: see app.xaml
    {
        public static double Width = 700;
        public static double Height = 300;
        public static int BorderSize = 4;
        public static Brush BorderColor = Brushes.DarkGray;
        public static Brush BgColor = Brushes.Cyan;

        public static Brush BallBg = Brushes.Yellow;
        public static Brush BallLine = Brushes.Red;
        public static Brush PadBg = Brushes.Maroon;
        public static Brush PadLine = Brushes.Black;

        public static Brush EnemyBg = Brushes.Blue; // hazi
        public static Brush EnemyLine = Brushes.Blue; // hazi

        public static int BallSize = 20;
        public static int PadWidth = 100;
        public static int PadHeight = 20;

        public static double EnemySize = 20; // hazi
    }

    class MyShape
    {
        Rect area;
        public Rect Area
        {
            get { return area; } // NO! get;
        }

        public int Dx { get; set; }
        public int Dy { get; set; }

        public MyShape(double x, double y, double w, double h)
        {
            area = new Rect(x, y, w, h);
            Dx = 5;
            Dy = 5;
        }
        public void ChangeX(double diff)
        {
            // Area.X += diff; // Not a variable!
            // Area = new Rect(Area.X+diff, xxxx) // Slow!
            area.X += diff;
        }
        public void ChangeY(double diff)
        {
            area.Y += diff;
        }
        public void SetXY(double x, double y)
        {
            area.X = x;
            area.Y = y;
        }
    }

    class PongModel
    {
        public int Errors { get; set; }
        public MyShape Pad { get; set; }
        public MyShape Ball { get; set; }

        public List<Enemy> Enemies { get; set; } // hazi

        public List<Star> Stars { get; set; }  // Phase 2 - No time?

        public PongModel()
        {
            Pad = new MyShape(Config.Width / 2, Config.Height - 20, 100, 20);
            Ball = new MyShape(Config.Width / 2, Config.Height / 2, 20, 20);
            Stars = new List<Star>(); // Phase 2
            Enemies = new List<Enemy>(); // hazi
        }
    }

    class PongLogic
    {
        PongModel model;
        public enum Direction { Left, Right }
        public event EventHandler RefreshScreen; // instead of NotifyPropertyChanged

        public PongLogic(PongModel model)
        {
            this.model = model;
        }
        public void MovePad(Direction d)
        {
            if (d == Direction.Left)
            {
                model.Pad.ChangeX(-10);
            }
            else
            {
                model.Pad.ChangeX(10);
            }
            RefreshScreen?.Invoke(this, EventArgs.Empty);
        }

        public void JumpPad(double x)
        {
            model.Pad.SetXY(x, model.Pad.Area.Y);
            RefreshScreen?.Invoke(this, EventArgs.Empty);
        }

        public bool MoveShape(MyShape shape) // hazi
        {
            bool faulted = false;
            shape.ChangeX(shape.Dx);
            shape.ChangeY(shape.Dy);

            if (shape.Area.Left < 0 || shape.Area.Right > Config.Width)
            {
                shape.Dx = -shape.Dx;
            }

            if (shape.Area.Top < 0 || shape.Area.IntersectsWith(model.Pad.Area))
            {
                shape.Dy = -shape.Dy;
            }
            if (shape == model.Ball && model.Enemies.Any(t => t.Area.IntersectsWith(shape.Area))) // hazi
            {
                Random rnd = new Random();
                int temp = rnd.Next(1, 4);
                switch (temp)
                {
                    case 1:
                        model.Ball.Dx = -model.Ball.Dx;
                        break;
                    case 2:
                        model.Ball.Dy = -model.Ball.Dy;
                        break;
                    case 3:
                        model.Ball.Dx = -model.Ball.Dx;
                        model.Ball.Dy = -model.Ball.Dy;
                        break;
                }
            }
            if (shape.Area.Bottom > Config.Height)
            {
                shape.SetXY(shape.Area.X, Config.Height / 2);
                faulted = true;
            }

            RefreshScreen?.Invoke(this, EventArgs.Empty);
            return faulted;
        }
        public bool MoveEnemy(int id) // hazi
        {
            bool smash = false;
            MyShape shape = model.Enemies.ElementAt(id);
            shape.ChangeX(shape.Dx);
            shape.ChangeY(shape.Dy);

            if (shape.Area.Left < 0 || shape.Area.Right > Config.Width)
            {
                shape.Dx = -shape.Dx;
            }

            if (shape.Area.Top < 0 || shape.Area.Bottom > Config.Height)
            {
                shape.Dy = -shape.Dy;
            }
            if (shape.Area.IntersectsWith(model.Ball.Area))
            {
                smash = true;
            }
            RefreshScreen?.Invoke(this, EventArgs.Empty);
            return smash;
        }
        public void MoveBall() // hazi
        {
                if (MoveShape(model.Ball)) model.Errors++;
            RefreshScreen?.Invoke(this, EventArgs.Empty);
        }

        public void AddStar() // Phase 2
        {
            model.Stars.Add(new Star(Config.Width / 2, Config.Height / 2, 10, 8));
            RefreshScreen?.Invoke(this, EventArgs.Empty);
        }

        public void AddEnemy() // hazi
        {
            Random rnd = new Random();
            model.Enemies.Add(new Enemy(rnd.Next(0, (int)Config.Width), 0, Config.EnemySize,Config.EnemySize));
            RefreshScreen?.Invoke(this, EventArgs.Empty);
        }
        public void RemoveEnemy(MyShape shape) // hazi
        {
            model.Enemies.Remove((Enemy)shape);
        }
        public void RelocateEnemy(MyShape shape)
        {
            Random rnd = new Random();
            shape.SetXY(rnd.Next(0, (int)Config.Width), 0);
        }
        public void MoveStars() // Phase 2
        {
            foreach (Star star in model.Stars)
            {
                if (MoveShape(star)) model.Errors++;
            }
            RefreshScreen?.Invoke(this, EventArgs.Empty);
        }
        public void MoveEnemies2() // hazi, listabol kiszedve halálkor
        {
            for (int i = 0; i < model.Enemies.Count(); i++)
            {
                if(MoveEnemy(i)) RemoveEnemy(model.Enemies.ElementAt(i));
            }
            RefreshScreen?.Invoke(this, EventArgs.Empty);
        }
        public void MoveEnemies() // hazi, csak áthelyezve halálkor
        {
            for (int i = 0; i < model.Enemies.Count(); i++)
            {
                if (MoveEnemy(i)) RelocateEnemy(model.Enemies.ElementAt(i));
            }
            RefreshScreen?.Invoke(this, EventArgs.Empty);
        }
        public int NumberOfEnemies() // hazi
        {
            return model.Enemies.Count();
        }
    }

    class PongRenderer
    {
        PongModel model;

        public PongRenderer(PongModel model)
        {
            this.model = model;
        }

        public void DrawThings(DrawingContext ctx) // hazi
        {
            DrawingGroup dg = new DrawingGroup();

            GeometryDrawing background = new GeometryDrawing(Config.BgColor,
                new Pen(Config.BorderColor, Config.BorderSize),
                new RectangleGeometry(new Rect(0, 0, Config.Width, Config.Height)));
            GeometryDrawing ball = new GeometryDrawing(Config.BallBg,
                new Pen(Config.BallLine, 1),
                new EllipseGeometry(model.Ball.Area));
            GeometryDrawing pad = new GeometryDrawing(Config.PadBg,
                new Pen(Config.PadLine, 1),
                new RectangleGeometry(model.Pad.Area));
            FormattedText formattedText = new FormattedText(model.Errors.ToString(),
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                16,
                Brushes.Black);
            GeometryDrawing text = new GeometryDrawing(null, new Pen(Brushes.Red, 2),
                formattedText.BuildGeometry(new Point(5, 5)));

            dg.Children.Add(background);
            dg.Children.Add(ball);
            dg.Children.Add(pad);
            dg.Children.Add(text);

            foreach (Star star in model.Stars)
            {
                GeometryDrawing starGeo = new GeometryDrawing(Config.BallBg, new Pen(Config.BallLine, 1),
                    star.GetGeometry());
                dg.Children.Add(starGeo);
            }

            foreach (Enemy enemy in model.Enemies) // hazi
            {
                GeometryDrawing enemyGeo = new GeometryDrawing(Config.EnemyBg, new Pen(Config.EnemyLine, 1),
                    enemy.GetGeometry());
                dg.Children.Add(enemyGeo);
            }

            ctx.DrawDrawing(dg);
        }
    }

    class PongControl : FrameworkElement
    {
        PongModel model;
        PongLogic logic;
        PongRenderer renderer;
        DispatcherTimer tickTimer;

        public PongControl()
        {
            Loaded += GameScreen_Loaded; // += <TAB><RET>
            // PongControl ctrl = new PongControl();
            // someWindow.Content = ctrl; ... XAML
        }

        private void GameScreen_Loaded(object sender, RoutedEventArgs e)
        {
            model = new PongModel();
            logic = new PongLogic(model);
            renderer = new PongRenderer(model);

            Window win = Window.GetWindow(this);
            if (win != null) // if (!IsInDesignMode)
            {

                tickTimer = new DispatcherTimer();
                tickTimer.Interval = TimeSpan.FromMilliseconds(25);
                tickTimer.Tick += timer_Tick;
                tickTimer.Start();

                win.KeyDown += Win_KeyDown; // += <TAB><RET>
                MouseLeftButtonDown += PongControl_MouseLeftButtonDown; // += <TAB><RET>
            }

            logic.RefreshScreen += (obj, args) => InvalidateVisual();
            InvalidateVisual();
        }

        private void PongControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            logic.JumpPad(e.GetPosition(this).X);
        }

        private void Win_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left: logic.MovePad(PongLogic.Direction.Left); break;
                case Key.Right: logic.MovePad(PongLogic.Direction.Right); break;
                case Key.Space: logic.AddStar(); break; // phase 2
                //case Key.Enter: logic.AddEnemy(); break;
            }
        }

        void timer_Tick(object sender, EventArgs e) // hazi
        {
            if (logic.NumberOfEnemies() < 3) logic.AddEnemy(); // hazi
            logic.MoveBall();
            logic.MoveStars(); // phase 2
            logic.MoveEnemies(); // hazi

        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (renderer != null) renderer.DrawThings(drawingContext);
        }
    }

    class Star : MyShape // Phase 2
    {
        double n;
        double r;
        public Star(double x, double y, double r, double n)
            : base(x, y, 2 * r, 2 * r)
        {
            this.n = n;
            this.r = r;
        }

        public Geometry GetGeometry()
        {
            List<Point> points = new List<Point>();
            for (int i = 0; i < n; i++)
            {
                double angle = i * 2 * Math.PI / n;
                Point P = new Point(r * Math.Cos(angle), r * Math.Sin(angle));
                if (i % 2 == 1)
                {
                    P.X *= 0.2;
                    P.Y *= 0.2;
                }
                P.X += r + Area.X;
                P.Y += r + Area.Y;
                points.Add(P);
            }

            StreamGeometry streamGeometry = new StreamGeometry();
            using (StreamGeometryContext geometryContext = streamGeometry.Open())
            {
                geometryContext.BeginFigure(points[0], true, true);
                geometryContext.PolyLineTo(points, true, true);
            }

            return streamGeometry;
        }
    }

    class Enemy : MyShape // hazi
    {
        double height;
        double width;
        public Enemy(double x, double y, double width, double height) : base(x,y,width,height)
        {
            this.height = height;
            this.width = width;
        }

        public Geometry GetGeometry()
        {


            List<Point> points = new List<Point>();
            
            for (int i = 0; i < 4; i++)
            {
                double angle =Math.PI/2 * i + Math.PI/4 ;
                Point P = new Point(height * Math.Cos(angle), height * Math.Sin(angle));
                P.X += Area.X;
                P.Y += Area.Y;
                points.Add(P);
            }

            StreamGeometry streamGeometry = new StreamGeometry();
            using (StreamGeometryContext geometryContext = streamGeometry.Open())
            {
                geometryContext.BeginFigure(points[0], true, true);
                geometryContext.PolyLineTo(points, true, true);
            }

            return streamGeometry;
        }

    }

}
