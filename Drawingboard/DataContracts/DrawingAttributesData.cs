using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Drawingboard.DataContracts
{
    [DataContract] 
    public class DrawingAttributesData
    {
        [DataMember]
        public string Color { get; set; }

        [DataMember]
        public double Width { get; set; }

        [DataMember]
        public double Height { get; set; }

        [DataMember]
        public bool IsHighlighter { get; set; }
    }
}