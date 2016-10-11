using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Drawingboard.DataContracts
{
    [DataContract]
    public class PointData
    {
        [DataMember]
        public double X { get; set; }

        [DataMember]
        public double Y { get; set; }

        public PointData()
        { 
        }

        public PointData(double x, double y)
        {
            X = x;
            Y = y;
        }
    }    
}
