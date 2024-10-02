using ConvexHull.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ConvexHull
{
    public partial class MainWindow : Window
    {
        private List<PointModel> _points;
        private List<PointModel> _convexHullSteps;

        private int _stepIndex;
        private bool _isSolving;
        public TimeSpan ElapsedTime { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            _points = new List<PointModel>();
            _convexHullSteps = new List<PointModel>();
            elapsed.Text = TimeSpan.Zero.ToString();
            _stepIndex = 0;
            _isSolving = false;
        }

        public void GeneratePoints(object sender, RoutedEventArgs e)
        {
            Random rand = new Random();

            for (int i = 0; i < int.Parse(pointsAmount.Text); i++)
            {
                double x = rand.NextDouble() * canvas.Width;
                double y = rand.NextDouble() * canvas.Height;

                _points.Add(new PointModel(x, y));
            }
            
            DrawPoints();
            
            _stepIndex = 0;
        }

        public void SolveQuickHull(object sender, RoutedEventArgs e)
        {
            if (_isSolving) return;

            _isSolving = true;
            Stopwatch sw = Stopwatch.StartNew();
            
            List<PointModel> hull = new List<PointModel>();

            _convexHullSteps = Solve();

            if (visualMode.IsChecked == true)
                DrawLine(_convexHullSteps, Brushes.IndianRed);

            _isSolving = false;
            elapsed.Text = sw.Elapsed.ToString();
            sw.Stop();
        }

        public void StepQuickHull(object sender, RoutedEventArgs e)
        {
            if (_convexHullSteps == null || _stepIndex >= _convexHullSteps.Count) return;

            _stepIndex++;
        }

        public void DeleteAllPoints(object sender, RoutedEventArgs e)
        {
            _points.Clear();
            _convexHullSteps.Clear();
            canvas.Children.Clear();
            elapsed.Text = TimeSpan.Zero.ToString();
            _stepIndex = 0;
        }

        private void DrawPoints()
        {
            foreach (var point in _points)
            {
                Ellipse ellipse = new Ellipse
                {
                    Width = 4,
                    Height = 4,
                    Fill = Brushes.Black
                };
                
                Canvas.SetLeft(ellipse, point.X);
                Canvas.SetTop(ellipse, point.Y);
                canvas.Children.Add(ellipse);
            }
        }

        private void DrawLine(List<PointModel> points, SolidColorBrush colorBrush)
        {
            if(points.Count < 2) return;

            for (int i = 0; i < points.Count-1; i++)
            {
                var pointA = points[i];
                var pointB = points[i+1];
                if(i == points.Count - 2)
                {
                    pointB = points[0];
                }

                Line line = new Line
                {
                    X1 = pointA.X,
                    Y1 = pointA.Y,
                    X2 = pointB.X,
                    Y2 = pointB.Y,
                    Stroke = colorBrush,
                    StrokeThickness = 2
                };

                canvas.Children.Add(line);
            }
        }

        private List<PointModel> Solve()
        {
            if (_points.Count < 3) return _points;

            List<PointModel> convexHull = new List<PointModel>();

            // search for min and max pooints
            _points = _points.OrderBy(p => p.X).ToList();
            PointModel minXPoint = _points.First();
            PointModel maxXPoint = _points.Last();

            // min point on index 0 and max point on index 1
            convexHull.Add(minXPoint);
            convexHull.Add(maxXPoint);

            // devides all points into left and right set (imaginary line between min and max point)
            List<PointModel> leftSet = _points.Where(p => IsLeft(minXPoint, maxXPoint, p) > 0).ToList();
            List<PointModel> rightSet = _points.Where(p => IsLeft(minXPoint, maxXPoint, p) < 0).ToList();

            if (visualMode.IsChecked == true)
                DrawLine(convexHull, Brushes.LightYellow);

            FindHull(minXPoint, maxXPoint, leftSet, convexHull);
            FindHull(maxXPoint, minXPoint, rightSet, convexHull);

            return convexHull;
        }

        private void FindHull(PointModel pointA, PointModel pointB, List<PointModel> set, List<PointModel> hull)
        {
            if (set.Count == 0)
                return;

            // get farthest point in set
            PointModel farthestPoint = set.OrderByDescending(p => DistanceFromLine(pointA, pointB, p)).First();
            hull.Insert(hull.IndexOf(pointB), farthestPoint);

            //divide & conquer
            List<PointModel> leftSetAP = set.Where(point => IsLeft(pointA, farthestPoint, point) > 0).ToList();
            List<PointModel> leftSetPB = set.Where(point => IsLeft(farthestPoint, pointB, point) > 0).ToList();

            if (visualMode.IsChecked == true)
                DrawLine(hull, Brushes.LightYellow);

            FindHull(pointA, farthestPoint, leftSetAP, hull);
            FindHull(farthestPoint, pointB, leftSetPB, hull);
        }

        private double IsLeft(PointModel pointA, PointModel pointB, PointModel pointC)
        {
            return (pointB.X - pointA.X) * (pointC.Y - pointA.Y) - (pointB.Y - pointA.Y) * (pointC.X - pointA.X);
        }

        private double DistanceFromLine(PointModel A, PointModel B, PointModel P)
        {
            // formular for calculating the distance of point P to the line AB
            double numerator = Math.Abs((B.Y - A.Y) * P.X - (B.X - A.X) * P.Y + B.X * A.Y - B.Y * A.X);
            double denominator = Math.Sqrt(Math.Pow(B.Y - A.Y, 2) + Math.Pow(B.X - A.X, 2));
            return numerator / denominator;
        }
    }
}