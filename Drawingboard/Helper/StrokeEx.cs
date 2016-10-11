using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;

namespace Drawingboard.controls
{
    static class WbStrokeEx
    {
        private static readonly Guid IsEnabled = new Guid("640B5E0C-E9D8-4dd5-8249-59795CD4B271");
        private static readonly Guid IsSelected = new Guid("8457982D-B0DE-40A3-B19D-BA3B66776E13");
        private static readonly Guid IsOperating = new Guid("A3E68EFC-0834-4709-98D0-E3BCF2CDCD0E");
        private static readonly Guid syncBehaviorId = new Guid("0A49C0EA-0C11-4EF5-9E1C-0D973E2DBEC0");
        private static readonly Guid MendID = new Guid("80E5B276-0C19-472C-B66C-AD7A8ED89399");
        private static readonly Guid IsMend = new Guid("C1F463B8-560D-4632-82DE-F8E280C76A9C");


        public static string GetBehaviorId(this Stroke stroke)
        {
            return stroke.GetPropertyData(syncBehaviorId) as string;
        }

        public static void SetBehaviorId(this Stroke stroke, string id)
        {
            stroke.AddPropertyData(syncBehaviorId, id);
        }

        public static bool GetIsOperating(this Stroke stroke)
        {
            if (stroke.GetPropertyDataIds().Contains(IsOperating))
            {
                return (bool)stroke.GetPropertyData(IsOperating);
            }
            return false;
        }

        public static void SetIsOperating(this Stroke stroke, bool isOperating)
        {
            stroke.AddPropertyData(IsOperating, isOperating);
        }

        public static bool GetIsEnabled(this Stroke stroke)
        {
            if (stroke.GetPropertyDataIds().Contains(IsEnabled))
            {
                return (bool)stroke.GetPropertyData(IsEnabled);
            }
            return true;
        }

        public static void SetIsEnabled(this Stroke stroke, bool isEnabled)
        {
            stroke.AddPropertyData(IsEnabled, isEnabled);
        }


        public static bool GetIsMend(this Stroke stroke)
        {
            if (stroke.ContainsPropertyData(IsMend))
            {
                return (bool)stroke.GetPropertyData(IsMend);
            }
            return false;
        }

        public static void SetIsMend(this Stroke stroke, bool isCollected)
        {
            stroke.AddPropertyData(IsMend, isCollected);
        }


        public static string GetMendID(this Stroke stroke)
        {
            if (stroke.ContainsPropertyData(MendID))
            {
                return stroke.GetPropertyData(MendID) as string;
            }
            return string.Empty;
        }

        public static void SetMendID(this Stroke stroke, string id)
        {
            stroke.AddPropertyData(MendID, id);
        }
    }

    public static class StrokeEx
    {
        public static readonly Guid ID = new Guid("8457982D-B0DE-40A3-B19D-BA3B66776E11");
        private static readonly Guid IsCollected = new Guid("8457982D-B0DE-40A3-B19D-BA3B66776E12");
        private static readonly Guid IsSelected = new Guid("8457982D-B0DE-40A3-B19D-BA3B66776E13");
        public static readonly Guid IsRemote = new Guid("8457982D-B0DE-40A3-B19D-BA3B66776E14");
        public static readonly Guid PageNumber = new Guid("8457982D-B0DE-40A3-B19D-BA3B66776E15");
        private static readonly Guid FirstBatPointCount = new Guid("1457982D-B0DE-40A3-B19D-BA3B66776E11");
        private static readonly Guid SyncCount = new Guid("1457982D-B0DE-40A3-B19D-BA3B66776E12");

        public static string GetID(this Stroke stroke)
        {
            if (stroke.ContainsPropertyData(ID))
            {
                return stroke.GetPropertyData(ID) as string;
            }
            else
            {
                string id = Guid.NewGuid().ToString();
                stroke.SetID(id);
                return id;
            }

        }

        public static void SetID(this Stroke stroke, string id)
        {
            stroke.AddPropertyData(ID, id);
        }

        public static bool HasID(this Stroke stroke)
        {
            return stroke.ContainsPropertyData(ID);
        }

        public static bool GetIsCollected(this Stroke stroke)
        {
            return (bool)stroke.GetPropertyData(IsCollected);
        }

        public static void SetIsCollected(this Stroke stroke, bool isCollected)
        {
            stroke.AddPropertyData(IsCollected, isCollected);
        }

        public static bool GetIsSelected(this Stroke stroke)
        {
            return (bool)stroke.GetPropertyData(IsSelected);
        }

        public static void SetIsSelected(this Stroke stroke, bool isSelected)
        {
            stroke.AddPropertyData(IsSelected, isSelected);
        }

        public static int GetPageNumber(this Stroke stroke)
        {
            return (int)stroke.GetPropertyData(PageNumber);
        }

        public static void SetPageNumber(this Stroke stroke, int pageNumber)
        {
            stroke.AddPropertyData(PageNumber, pageNumber);
        }

        public static bool GetIsRemote(this Stroke stroke)
        {
            return (bool)stroke.GetPropertyData(IsRemote);
        }

        public static void SetIsRemote(this Stroke stroke, bool isRemote)
        {
            stroke.AddPropertyData(IsRemote, isRemote);
        }

        public static int GetFirstBatPointCount(this Stroke stroke)
        {
            return (int)stroke.GetPropertyData(FirstBatPointCount);
        }

        public static void SetFirstBatPointCount(this Stroke stroke, int id)
        {
            stroke.AddPropertyData(FirstBatPointCount, id);
        }

        public static bool ContainsFirstBatPointCount(this Stroke stroke)
        {
            return stroke.ContainsPropertyData(FirstBatPointCount);
        }

        public static void SetSyncCount(this Stroke stroke, int id)
        {
            stroke.AddPropertyData(SyncCount, id);
        }

        public static int GetSyncCount(this Stroke stroke)
        {
            return (int)stroke.GetPropertyData(SyncCount);
        }

        public static bool IsTransparentStroke(this Stroke stroke)
        {
            return stroke.DrawingAttributes.Color == Colors.Transparent;
        }

        public static Stroke FindNewestLocalValidTapStroke(this StrokeCollection coll, Point point)
        {
            const int MaxFindCount = 5;
            int findCount = 0;
            for (int i = coll.Count - 1; i >= 0; i--)
            {
                //
                if (findCount >= MaxFindCount)
                {
                    return null;
                }

                // 
                Stroke stroke = coll[i];
                if (stroke.GetIsRemote() || stroke.IsTransparentStroke())
                {
                    continue;
                }
                findCount++;

                //
                if (IsShortStorke(stroke) && StrokeHitTest(stroke, point))
                {
                    return stroke;
                }
            }

            return null;
        }

        private static bool IsShortStorke(this Stroke stroke)
        {
            return stroke.StylusPoints.Count < 5;
        }

        private static bool StrokeHitTest(Stroke stroke, Point point)
        {
            foreach (var i in stroke.StylusPoints)
            {
                var v = i.ToPoint() - point;
                if (Math.Abs(v.Length) < 1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
