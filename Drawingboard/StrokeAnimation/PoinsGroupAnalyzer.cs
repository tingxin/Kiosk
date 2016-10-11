using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StrokesAnimationEngine
{
    internal class PoinsGroupAnalyzer<T> where T : class
    {
        double gap = 50;
        public List<List<T>> Analyze(List<T> source, Func<T, Point> getTCenter)
        {
            List<List<T>> result = new List<List<T>>();

            foreach (var T in source)
            {
                Point center = getTCenter(T);
            }

            return result;
        }
    }
}
