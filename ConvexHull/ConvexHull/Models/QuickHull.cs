using ConvexHull.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvexHull.Models
{
    public class QuickHullSolver
    {
        public List<PointModel> Points { get; set; }

        public QuickHullSolver(List<PointModel> points)
        {
            Points = points;
        }

        // This function performs QuickHull and returns the convex hull points
        public List<PointModel> Solve()
        {
            if (Points.Count < 3) return Points;

            List<PointModel> convexHull = new List<PointModel>();

            PointModel minXPoint = Points.OrderBy(p => p.X).First();
            PointModel maxXPoint = Points.OrderBy(p => p.X).Last();

            convexHull.Add(minXPoint);
            convexHull.Add(maxXPoint);

            List<PointModel> leftSet = Points.Where(p => IsLeft(minXPoint, maxXPoint, p) > 0).ToList();
            List<PointModel> rightSet = Points.Where(p => IsLeft(minXPoint, maxXPoint, p) < 0).ToList();

            FindHull(minXPoint, maxXPoint, leftSet, convexHull);
            FindHull(maxXPoint, minXPoint, rightSet, convexHull);

            return convexHull;
        }

        private void FindHull(PointModel A, PointModel B, List<PointModel> set, List<PointModel> hull)
        {
            if (set.Count == 0)
                return;

            PointModel farthestPoint = set.OrderByDescending(p => DistanceFromLine(A, B, p)).First();
            hull.Insert(hull.IndexOf(B), farthestPoint);

            List<PointModel> leftSetAP = set.Where(p => IsLeft(A, farthestPoint, p) > 0).ToList();
            List<PointModel> leftSetPB = set.Where(p => IsLeft(farthestPoint, B, p) > 0).ToList();

            FindHull(A, farthestPoint, leftSetAP, hull);
            FindHull(farthestPoint, B, leftSetPB, hull);
        }

        private double IsLeft(PointModel A, PointModel B, PointModel P)
        {
            return (B.X - A.X) * (P.Y - A.Y) - (B.Y - A.Y) * (P.X - A.X);
        }

        private double DistanceFromLine(PointModel A, PointModel B, PointModel P)
        {
            double numerator = Math.Abs((B.Y - A.Y) * P.X - (B.X - A.X) * P.Y + B.X * A.Y - B.Y * A.X);
            double denominator = Math.Sqrt(Math.Pow(B.Y - A.Y, 2) + Math.Pow(B.X - A.X, 2));
            return numerator / denominator;
        }
    }
}