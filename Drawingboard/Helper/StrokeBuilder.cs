using Drawingboard.controls;
using Drawingboard.controls.MendStrokes;
using Drawingboard.DataContracts;
using Drawingboard.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Ink;
using System.Windows.Controls;
using KComponents;
using System.IO;

namespace Drawingboard.Helper
{
    static class StrokeTypes
    {
        public const string Arrow = "Arrow";
        public const string Circle = "Circle";
        public const string Line = "Line";
        public const string Rectangle = "Rectangle";
        public const string Triangle = "Triangle";
    }

    static class StrokeBuilder
    {
        internal const string FileDirectory = @"Drawingboard\Files";
        internal const string SnapshotDirectory = @"Drawingboard\Snapshots";
        public static StrokeCollection DataToStrokes(List<StrokeData> strokeList, System.Windows.Size size)
        {
            StrokeCollection coll = new StrokeCollection();

            foreach (var i in strokeList)
            {
                coll.Add(DataToStroke(i, size));
            }

            return coll;
        }

        public static List<StrokeData> StrokesToData(StrokeCollection coll, System.Windows.Size size)
        {
            List<StrokeData> list = new List<StrokeData>();

            foreach (var i in coll)
            {
                list.Add(StrokeToData(i, size));
            }

            return list;
        }

        public static void Clean()
        {
            string fileDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileDirectory);
            System.IO.Directory.Delete(fileDirectory, true);

            string snapDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SnapshotDirectory);
            System.IO.Directory.Delete(snapDirectory, true);

            Directory.CreateDirectory(fileDirectory);

            Directory.CreateDirectory(snapDirectory);
        }


        public static string GetSnapShotAddresss(string fileName)
        {
            string snapShotDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SnapshotDirectory);

            string snapShotAddress = System.IO.Path.Combine(snapShotDirectory, fileName + ".png");
            return snapShotAddress;
        }

        public static bool Save(DrawingboardData drawingboard)
        {
            try
            {
                string text = JsonHelper.Serialize<DrawingboardData>(drawingboard);

                string fileDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileDirectory);

                System.IO.File.WriteAllText(System.IO.Path.Combine(fileDirectory, drawingboard.ID + ".txt"), text);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static DrawingboardData Load(string drawingboardId)
        {

            try
            {
                string text = "";
                string fileDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileDirectory);

                string fileName = System.IO.Path.Combine(fileDirectory, drawingboardId + ".txt");
                if (System.IO.File.Exists(fileName))
                {
                    using (var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite))
                    {
                        var reader = new System.IO.StreamReader(fs);
                        text = reader.ReadToEnd();
                    }
                }

                if (string.IsNullOrEmpty(text) == false)
                {
                    DrawingboardData board = JsonHelper.Deserialize<DrawingboardData>(text);
                    return board;
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static Stroke DataToStroke(StrokeData data, System.Windows.Size size)
        {
            Stroke stroke = null;
            switch (data.Type)
            {
                case StrokeTypes.Arrow:
                    stroke = new ArrowStroke(StrokeDataEx.ToStylusPoints(data.Points, size));
                    break;
                case StrokeTypes.Circle:
                    CircleStroke cs = new CircleStroke(StrokeDataEx.ToStylusPoints(data.Points, size));
                    stroke = cs;
                    break;
                case StrokeTypes.Line:
                    stroke = new StraightStroke(StrokeDataEx.ToStylusPoints(data.Points, size));
                    break;
                case StrokeTypes.Rectangle:
                    stroke = new RectangleStroke(StrokeDataEx.ToStylusPoints(data.Points, size));
                    break;
                case StrokeTypes.Triangle:
                    stroke = new TriangleStroke(StrokeDataEx.ToStylusPoints(data.Points, size));
                    break;
                default:
                    stroke = new Stroke(StrokeDataEx.ToStylusPoints(data.Points, size));
                    break;
            }


            StrokeEx.SetID(stroke, data.ID);
            stroke.DrawingAttributes = DrawingAttributesDataEx.ToDrawingAttributes(data.Atts);
            if (stroke is ShapeStroke)
            {
                stroke.DrawingAttributes.FitToCurve = false;
            }
            return stroke;
        }

        private static StrokeData StrokeToData(Stroke stroke, System.Windows.Size size)
        {
            StrokeData data = new StrokeData();
            string type = "";
            if (stroke is ArrowStroke)
            {
                type = StrokeTypes.Arrow;
            }
            if (stroke is CircleStroke)
            {
                type = StrokeTypes.Circle;
            }
            if (stroke is StraightStroke)
            {
                type = StrokeTypes.Line;
            }
            if (stroke is RectangleStroke)
            {
                type = StrokeTypes.Rectangle;
            }
            if (stroke is TriangleStroke)
            {
                type = StrokeTypes.Triangle;
            }
            data.Type = type;

            data.Points = StrokeDataEx.ToPoints(stroke.StylusPoints, size);
            data.Atts = DrawingAttributesDataEx.ToData(stroke.DrawingAttributes);
            data.ID = StrokeEx.GetID(stroke);

            return data;
        }
    }
}
