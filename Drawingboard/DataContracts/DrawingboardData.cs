using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Drawingboard.DataContracts
{
    [DataContract]
    class DrawingboardData
    {
        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public List<StrokeData> Strokes { get; set; }
    }
}
