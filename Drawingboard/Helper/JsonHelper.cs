using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Drawingboard.Helper
{
    public class JsonHelper
    {
        public static string Serialize<T>(T obj)
        {
            System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());
            MemoryStream ms = new MemoryStream();
            serializer.WriteObject(ms, obj);
            byte[] dataBytes = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(dataBytes, 0, (int)ms.Length);
            string retVal = Encoding.UTF8.GetString(dataBytes);
          //  string retVal = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            ms.Dispose();
            return retVal;
        }

        public static T Deserialize<T>(string json)
        {
            T obj = Activator.CreateInstance<T>();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());
            obj = (T)serializer.ReadObject(ms);
            ms.Close();
            ms.Dispose();
            return obj;
        }
    }
}
