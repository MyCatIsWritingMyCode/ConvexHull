using ConvexHull.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ConvexHull.ViewModels
{
    public class QuickHullViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<PointModel> _points;
        private ObservableCollection<PointModel> _convexHullPoints;
        private QuickHullSolver _quickHullSolver;
        private int _stepIndex;
        private List<PointModel> _convexHullSteps;
        private bool _isSolving;
        private DateTime _startTime;
        private TimeSpan _elapsedTime;
        private Timer _timer;

        public ObservableCollection<PointModel> Points
        {
            get => _points;
            set { _points = value; OnPropertyChanged(nameof(Points)); }
        }

        public ObservableCollection<PointModel> ConvexHullPoints
        {
            get => _convexHullPoints;
            set { _convexHullPoints = value; OnPropertyChanged(nameof(ConvexHullPoints)); }
        }

        public TimeSpan ElapsedTime
        {
            get => _elapsedTime;
            set { _elapsedTime = value; OnPropertyChanged(nameof(ElapsedTime)); }
        }

        public ICommand GeneratePointsCommand { get; }
        public ICommand SolveQuickHullCommand { get; }
        public ICommand StepQuickHullCommand { get; }
        public ICommand DeleteAllPointsCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public QuickHullViewModel()
        {
            Points = new ObservableCollection<PointModel>();
            ConvexHullPoints = new ObservableCollection<PointModel>();

            GeneratePointsCommand = new RelayCommand(GeneratePoints);
            SolveQuickHullCommand = new RelayCommand(SolveQuickHullAsync);
            StepQuickHullCommand = new RelayCommand(StepQuickHull);
            DeleteAllPointsCommand = new RelayCommand(DeleteAllPoints);

            _stepIndex = 0;
            _isSolving = false;
        }

        private void GeneratePoints()
        {
            Random rand = new Random(); // Random number generator
            Points.Clear(); // Clear the old points

            // Generate 100 random points with floating-point precision within a 400x400 canvas
            for (int i = 0; i < 5; i++)
            {
                double x = rand.NextDouble() * 400; // X-coordinate (0 to 400 with decimal precision)
                double y = rand.NextDouble() * 400; // Y-coordinate (0 to 400 with decimal precision)

                Points.Add(new PointModel(x, y)); // Add the new random point to the collection
            }

            ConvexHullPoints.Clear(); // Clear convex hull when generating new points
            _stepIndex = 0; // Reset step index
        }

        private async void SolveQuickHullAsync()
        {
            if (_isSolving) return;

            _isSolving = true;
            _startTime = DateTime.Now;

            _timer = new Timer(UpdateElapsedTime, null, 0, 100);

            _quickHullSolver = new QuickHullSolver(Points.ToList());
            List<PointModel> hull = new List<PointModel>();

            _convexHullSteps = _quickHullSolver.Solve(); // Get all the convex hull points in one go.

            ConvexHullPoints.Clear();
            foreach (var point in _convexHullSteps)
            {
                ConvexHullPoints.Add(point);
                await Task.Delay(200); // Short delay between each point.
            }

            _timer.Dispose();
            _isSolving = false;
        }

        private void StepQuickHull()
        {
            if (_convexHullSteps == null || _stepIndex >= _convexHullSteps.Count) return;

            ConvexHullPoints.Add(_convexHullSteps[_stepIndex]);
            _stepIndex++;
        }

        private void DeleteAllPoints()
        {
            Points.Clear();
            ConvexHullPoints.Clear(); 
            _stepIndex = 0;         
        }

        private void UpdateElapsedTime(object state)
        {
            ElapsedTime = DateTime.Now - _startTime;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
