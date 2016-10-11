using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Ink;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using Microsoft.Surface.Presentation.Controls;
using System.Windows.Media;
using System.Diagnostics;
using Drawingboard.controls;
using SpringRoll.Utility;
using Drawingboard.controls.MendStrokes;
using Drawingboard.Helper;


namespace SpringRoll.Whiteboard.MendStrokes
{
    class StrokeInkAnalyzer
    {
        SurfaceInkCanvas inkCanvas;

        const int maxAnalyzeCount = 4;
        System.Windows.Ink.InkAnalyzer theInkAnalyzer;
        StrokeCollection strokesCache = new StrokeCollection();
        DateTime lastTime = DateTime.Now;
        DrawingAttributes lastDrawingAttributes = null;
        object synObj = new object();
        IShapeHost drawingHost;
        public StrokeInkAnalyzer(SurfaceInkCanvas inkHost, IShapeHost host)
        {
            this.drawingHost = host;
            this.theInkAnalyzer = new System.Windows.Ink.InkAnalyzer();
            this.inkCanvas = inkHost;

            this.inkCanvas.StrokeCollected += inkHost_StrokeCollected;
            this.inkCanvas.Strokes.StrokesChanged += Strokes_StrokesChanged;
            theInkAnalyzer.ResultsUpdated += theInkAnalyzer_ResultsUpdated;

            IsWork = false;
        }



        private void inkHost_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            if (e.Stroke != null && this.IsWork)
            {
                if (this.theInkAnalyzer.IsAnalyzing)
                {
                    this.theInkAnalyzer.Abort();
                }

                if (this.lastDrawingAttributes != null)
                {
                    bool isSmae = this.IsSameDrawingAttributes(lastDrawingAttributes, e.Stroke.DrawingAttributes);
                    if (isSmae == false)
                    {
                        lastDrawingAttributes = e.Stroke.DrawingAttributes;
                        ReduceAnalyzeStrokeTo(0);

                    }
                }
                else
                {
                    lastDrawingAttributes = e.Stroke.DrawingAttributes;
                }

                e.Stroke.DrawingAttributes.FitToCurve = true;
                string strokeId = Guid.NewGuid().ToString();
                Stroke newStrok = e.Stroke;
                newStrok.SetMendID(strokeId);

                this.AddStrokes(newStrok);

                ReduceAnalyzeStrokeTo(maxAnalyzeCount);
                theInkAnalyzer.SetStrokeType(newStrok, StrokeType.Drawing);
                if (this.strokesCache.Count > 0)
                {
                    lock (synObj)
                    {
                        try
                        {
                            //theInkAnalyzer.BackgroundAnalyze();
                            this.theInkAnalyzer.Analyze();
                            string resultStr = this.theInkAnalyzer.GetRecognizedString();
                            List<string> resultStrArray = resultStr.Split(' ').ToList();
                            this.AnalyzResult(resultStrArray);

                        }
                        catch (Exception ex)
                        {

                            theInkAnalyzer.Abort();
                            this.ReduceAnalyzeStrokeTo(0);
                            Log.Instance().Error("WhiteBoard", "StrokeInkAnalyzer", ex);
                        }
                    }
                }
#if DEBUG
                lastTime = DateTime.Now;
#endif

            }
        }



        private void Strokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (e.Removed != null)
            {
                StrokeCollection removedStrokeCollection = new StrokeCollection();
                foreach (Stroke stroke in e.Removed)
                {
                    string mendId = stroke.GetMendID();
                    if (string.IsNullOrEmpty(mendId))
                    {
                        Stroke removedStroke = this.strokesCache.FirstOrDefault(item => item.GetMendID() == mendId);
                        if (removedStroke != null)
                        {
                            removedStrokeCollection.Add(removedStroke);
                        }
                    }

                }
                if (removedStrokeCollection.Count > 0)
                {
                    if (this.theInkAnalyzer.IsAnalyzing)
                    {
                        this.theInkAnalyzer.Abort();

                    }
                    this.RemoveStrokes(removedStrokeCollection);
                }
            }
        }

        public bool IsWork { set; get; }


        void theInkAnalyzer_ResultsUpdated(object sender, System.Windows.Ink.ResultsUpdatedEventArgs e)
        {
            AnalysisStatus status = e.Status;
            if (status.Successful)
            {
                string resultStr = this.theInkAnalyzer.GetRecognizedString();
                List<string> resultStrArray = resultStr.Split(' ').ToList();

                if (resultStrArray.Count > 0)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        AnalyzResult(resultStrArray);
                    }));
                }
            }
        }

        private void AnalyzResult(List<string> resultStrArray)
        {
            bool isContainDoubleKey = this.IsContainDoubleKey(resultStrArray);
            if (isContainDoubleKey)
            {
                this.ReduceAnalyzeStrokeTo(0);
                return;
            }
            foreach (var shapeKey in resultStrArray)
            {
                StrokeCollection[] strokeCollectionArray = this.theInkAnalyzer.Search(shapeKey);
                if (strokeCollectionArray.Length > 0)
                {
                    if (ShapeType.IsSupportShape(shapeKey))
                    {
                        StylusPointCollection targetCollection = this.GetCombineStylusPoints(strokeCollectionArray);

                        #region Mend
                        this.MendStroke(shapeKey, strokeCollectionArray, this.lastDrawingAttributes, targetCollection);
                        #endregion
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(shapeKey) && shapeKey != ShapeType.Straight)
                        {
                            foreach (StrokeCollection strokeCollection in strokeCollectionArray)
                            {
                                this.RemoveStrokes(strokeCollection);
                            }
                        }
                        else
                        {
                            //line or can not be mended
                            this.AddLine(strokeCollectionArray);
                        }
                    }
                }

            }
        }



        private void AddLine(StrokeCollection[] strokeCollectionArray)
        {
#if DEBUG
            if (this.lastDrawingAttributes.Color == Colors.Transparent)
            {
                throw new Exception("Color should not be Transparent");
            }
#endif
            foreach (StrokeCollection strokeCollection in strokeCollectionArray)
            {
                foreach (var stroke in strokeCollection)
                {
                    bool strokeIsMend = stroke.GetIsMend();
                    if (strokeIsMend == false)
                    {
                        stroke.SetIsMend(true);
                        ShapeStroke line = GetAdjustShapeStroke.Get(stroke.StylusPoints, "Other", this.lastDrawingAttributes);
                        if (line != null)
                        {
                            //line:
                            string strokeId = stroke.GetMendID();
                            line.SetMendID(strokeId);

                            this.inkCanvas.Strokes.Remove(stroke);
                            this.SetMendedStrokeAttribute(line);
                            this.drawingHost.Add(line);
                            //sync
                            List<string> originalStrokeIDs = new List<string> { stroke.GetID() };
                        }
                    }
                }
            }
        }

        private StylusPointCollection GetCombineStylusPoints(StrokeCollection[] strokeCollectionArray)
        {
            StylusPointCollection targetCollection = new StylusPointCollection();

            #region Shape collection
            if (strokeCollectionArray != null && strokeCollectionArray.Count() > 0)
            {
                foreach (StrokeCollection strokeCollection in strokeCollectionArray)
                {

                    foreach (Stroke stroke in strokeCollection)
                    {
                        if (stroke.StylusPoints != null)
                        {
                            foreach (StylusPoint stylusPoint in stroke.StylusPoints)
                            {
                                targetCollection.Add(stylusPoint);
                            }
                        }
                    }
                }
            }
            #endregion
            return targetCollection;
        }

        private void MendStroke(string shapeKey, StrokeCollection[] strokeCollectionArray, DrawingAttributes defaultDrawingAttributes, StylusPointCollection targetCollection)
        {
            if (targetCollection.Count > 0)
            {
                List<string> originalStrokeIDs = new List<string>();
                ShapeStroke combineStroke = GetAdjustShapeStroke.Get(targetCollection, shapeKey, defaultDrawingAttributes);
                if (combineStroke != null)
                {
                    foreach (StrokeCollection strokeCollection in strokeCollectionArray)
                    {
                        foreach (var stroke in strokeCollection)
                        {
                            string id = stroke.GetMendID();
                            if (string.IsNullOrEmpty(id) == false)
                            {
                                Stroke inkStroke = this.inkCanvas.Strokes.FirstOrDefault(item => item.GetMendID() == id);
                                Debug.Assert(inkStroke != null, "inkStroke should not be null in MendStroke method");
                                if (inkStroke != null)
                                {
                                    this.inkCanvas.Strokes.Remove(inkStroke);
                                    originalStrokeIDs.Add(inkStroke.GetID());
                                }
                            }

                        }
                        this.RemoveStrokes(strokeCollection);
                    }
                    this.SetMendedStrokeAttribute(combineStroke);

                    Log.Instance().Info("Whiteboard", "drawing shape is " + shapeKey);
                    //if we add the stroke to the inkcanvas, the incanvas will add some point automatic,so we need send the data before add it to local 
                   this.drawingHost.Add(combineStroke);
                }
                else
                {

                    Trace("Can not mend the stroke");
                    foreach (StrokeCollection strokeCollection in strokeCollectionArray)
                    {
                        this.RemoveStrokes(strokeCollection);
                    }
                }
            }
        }

        private bool IsContainDoubleKey(List<string> resultStrArray)
        {
            for (int i = 0; i < resultStrArray.Count - 1; i++)
            {
                for (int j = i + 1; j < resultStrArray.Count; j++)
                {

                    if (ShapeType.IsSupportShape(resultStrArray[j]))
                    {
                        if (resultStrArray[j].Length >= resultStrArray[i].Length)
                        {
                            if (resultStrArray[j].Contains(resultStrArray[i]))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (resultStrArray[i].Contains(resultStrArray[j]))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private void SetMendedStrokeAttribute(Stroke mendedStroke)
        {
            string newStrokeId = Guid.NewGuid().ToString();
            mendedStroke.SetID(newStrokeId);
            mendedStroke.SetIsSelected(false);
            mendedStroke.SetIsRemote(false);
            mendedStroke.SetIsCollected(true);
            mendedStroke.DrawingAttributes.FitToCurve = false;
        }

        private void ReduceAnalyzeStrokeTo(int count)
        {
            while (strokesCache.Count > count)
            {
                Stroke one = this.strokesCache[0];
                strokesCache.Remove(one);
                try
                {
                    this.theInkAnalyzer.RemoveStroke(one);
                }
                catch
                {
                    continue;
                }
            }
        }

        private void RemoveStrokes(StrokeCollection strokeCollection)
        {
            lock (synObj)
            {
                this.strokesCache.Remove(strokeCollection);
                this.theInkAnalyzer.RemoveStrokes(strokeCollection);
            }
        }

        private void AddStrokes(Stroke newStrok)
        {
            strokesCache.Add(newStrok);
            theInkAnalyzer.AddStroke(newStrok);
        }

        private bool IsSameDrawingAttributes(DrawingAttributes one, DrawingAttributes two)
        {
            if (one.Color == two.Color)
            {
                if (one.Width == two.Width)
                {
                    if (one.IsHighlighter == two.IsHighlighter)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void Trace(string resultStr)
        {
            System.Diagnostics.Trace.WriteLine(resultStr);
        }

    }
}
