using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;

namespace StrokesAnimationEngine
{
    internal class StrokeAnalyzer
    {
        Dictionary<StrokeCollection, AnalyzerPar> cache;
        private StrokeAnalyzer()
        {
            cache = new Dictionary<StrokeCollection, AnalyzerPar>();
        }

        static StrokeAnalyzer current;

        public static StrokeAnalyzer Current
        {
            get
            {
                if (current == null)
                {
                    current = new StrokeAnalyzer();
                }
                return current;
            }
        }

        public void Call(Func<StrokeCollection, bool> callback)
        {
            foreach (KeyValuePair<StrokeCollection, AnalyzerPar> item in cache)
            {
                if (item.Value.IsAddAnimation == false)
                {
                    bool result = callback(item.Key);
                    if (result)
                    {
                        item.Value.IsAddAnimation = true;
                    }

                }
            }
        }


        public void Add(Stroke stroke)
        {
            AnalyzerPar par = new AnalyzerPar();
            par.IsAddAnimation = false;
            par.AddedTime = DateTime.Now;
            bool isIn = false;
            foreach (KeyValuePair<StrokeCollection, AnalyzerPar> item in cache)
            {
                if (item.Value.Equals(par))
                {
                    isIn = true;
                    item.Value.AddedTime = par.AddedTime;
                    item.Key.Add(stroke);
                    break;
                }
            }
            if (isIn == false)
            {
                StrokeCollection collection = new StrokeCollection();
                collection.Add(stroke);
                cache.Add(collection, par);
            }

        }


        class AnalyzerPar
        {
            public bool IsAddAnimation { set; get; }
            public DateTime AddedTime { set; get; }

            public override bool Equals(object obj)
            {
                AnalyzerPar other = obj as AnalyzerPar;
                if (other != null)
                {
                    if (Math.Abs((other.AddedTime - this.AddedTime).TotalMilliseconds) < 3000)
                    {
                        return true;
                    }
                }
                return false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}
