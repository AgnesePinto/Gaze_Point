using System.Globalization;

namespace Gaze_Point.GPModel.GPRecord 
{ 
    
    /// <summary> 
    /// Provides utility methods for parsing raw XML data strings from the Gazepoint eye-tracker.  
    /// This class implements an extraction protocol to conver hardware-generated strings into structured GPData objects. 
    /// </summary> 
    /// <remarks> 
    /// Optimized for real-time applications by avoiding full XML parsing in favor og direct string manipulation and invariant culture numerical conversion. 
    /// </remarks> 
    /// <author>Agnese Pinto</author> 
    public class GPParser 
    { 
        public static GPData Parse(string xml) 
        { 
            if (string.IsNullOrWhiteSpace(xml) || !xml.StartsWith("<REC")) return null; 
            GPData data = new GPData 
            { 
                BPOGX = GetAttributeValue(xml, "BPOGX"), 
                BPOGY = GetAttributeValue(xml, "BPOGY"), 
                BPOGV = (int)GetAttributeValue(xml, "BPOGV"), 
                SACCADE_MAG = GetAttributeValue(xml, "SACCADE_MAG") 
            }; 
            return data; 
        } 
        private static double GetAttributeValue(string xml, string attributeName) 
        { 
            string searchTag = attributeName + "=\""; 
            int start = xml.IndexOf(searchTag); 

            if (start == -1) return 0.0; 

            start += searchTag.Length; 
            int end = xml.IndexOf("\"", start); 

            if (end == -1) return 0.0; 

            string valueString = xml.Substring(start, end - start); 

            double.TryParse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out double result); 

            return result; 
        } 
    }
}

//using System.Globalization;

//namespace Gaze_Point.GPModel.GPRecord
//{

//    /// <summary>
//    /// Provides utility methods for parsing raw XML data strings from the Gazepoint eye-tracker.
//    /// This class implements an extraction protocol to conver hardware-generated strings into structured GPData objects.
//    /// </summary>
//    /// <remarks>
//    /// Optimized for real-time applications by avoiding full XML parsing in favor og direct string manipulation and invariant culture numerical conversion.
//    /// </remarks>
//    /// <author>Agnese Pinto</author>


//    public class GPParser
//    {
//        public static GPData Parse(string xml)
//        {
//            if (string.IsNullOrWhiteSpace(xml) || !xml.StartsWith("<REC"))
//                return null;

//            GPData data = new GPData
//            {
//                BPOGX = GetAttributeValue(xml, "BPOGX"),
//                BPOGY = GetAttributeValue(xml, "BPOGY"),
//                BPOGV = (int)GetAttributeValue(xml, "BPOGV"),
//                SACCADE_MAG = GetAttributeValue(xml, "SACCADE_MAG")
//            };
//            return data;
//        }


//        private static double GetAttributeValue(string xml, string attributeName)
//        {
//            string searchTag = attributeName + "=\"";          
//            int start = xml.IndexOf(searchTag);                 

//            if (start == -1) return 0.0;                        

//            start += searchTag.Length;                          
//            int end = xml.IndexOf("\"", start);                 

//            if (end == -1) return 0.0;                          

//            string valueString = xml.Substring(start, end - start);         
//            double.TryParse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out double result);

//            return result;
//        }
//    }
//}




