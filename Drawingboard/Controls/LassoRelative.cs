using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Ink;

namespace Drawingboard.controls
{
    public class PositionEventArgs : EventArgs
    {
        public Point Position { get; private set; }

        public PositionEventArgs(Point position)
        {
            Position = position;
        }
    }

    public class SelectionChangeArgs : StrokesSelectedArgs
    {
        public Vector Offset { private set; get; }

        public SelectionChangeArgs(Guid behaviorId, IEnumerable<string> ids, Vector offset)
            : base(behaviorId, ids)
        {
            this.Offset = offset;
        }
    }

    public class SelectionChangedArgs : EventArgs
    {
        public Guid BehaviorId { private set; get; }
        public StrokeCollection Strokes { private set; get; }
        public Vector Offset { private set; get; }
        public SelectionChangedArgs(Guid behaviorId, StrokeCollection strokes, Vector offset)
        {
            this.BehaviorId = behaviorId;
            this.Strokes = strokes;
            this.Offset = offset;
        }
    }

    public class StrokesSelectedArgs : EventArgs
    {
        public IEnumerable<string> StrokeIds { private set; get; }

        public Guid BehaviorId { private set; get; }

        public StrokesSelectedArgs(Guid behaviorId, IEnumerable<string> ids)
            : base()
        {
            this.BehaviorId = behaviorId;
            this.StrokeIds = ids;
        }
    }

    public class StrokesSelectedChangedArgs : StrokesSelectedArgs
    {
        public IEnumerable<string> OldStrokeIds { private set; get; }

        public Guid OldBehaviorId { private set; get; }

        public StrokesSelectedChangedArgs(Guid behaviorId, Guid oldBehaviorid, IEnumerable<string> ids, IEnumerable<string> oldIds)
            : base(behaviorId, ids)
        {
            this.OldBehaviorId = oldBehaviorid;
            this.OldStrokeIds = oldIds;
        }
    }


    public class DrawingLassoArg : EventArgs
    {
        public List<Vector> Offsets { private set; get; }

        public Guid BehaviorId { private set; get; }

        public DrawingLassoArg(Guid behaviorId, List<Vector> offset)
            : base()
        {
            this.Offsets = offset;
            this.BehaviorId = behaviorId;
        }
    }

    public class StartDrawingLassoArg : EventArgs
    {
        public Vector Offset { private set; get; }

        public Guid BehaviorId { private set; get; }

        public StartDrawingLassoArg(Guid behaviorId, Vector offset)
            : base()
        {
            this.Offset = offset;
            this.BehaviorId = behaviorId;
        }
    }
}
