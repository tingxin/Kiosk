using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Drawingboard.DataContracts
{
    [DataContract]
    public class StrokeData
    {
        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string Type { get; set; }
        
        [DataMember]
        public List<PointData> Points { get; set; }

        [DataMember]
        public DrawingAttributesData Atts { get; set; }
    }
}
