// using System.Xml.Serialization;
// XmlSerializer serializer = new XmlSerializer(typeof(Test));
// using (StringReader reader = new StringReader(xml))
// {
//    var test = (Test)serializer.Deserialize(reader);
// }

using System.Xml.Serialization;

[XmlRoot(ElementName = "test")]
public class Test
{
    
}