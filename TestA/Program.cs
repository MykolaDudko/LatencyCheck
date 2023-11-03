
using ClassC;
using Library.Consts;
using Library.Models;
using Library.Repositories;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

class Program
{
    static async Task Main()
    {
        Tools _tools = new Tools();
        string currentDirectory = "../../../../Latency.xml";
        while (true)
        {
            XDocument xmlDocument = XDocument.Load(currentDirectory);

            // Find the specific element by its tag name
            XElement mode = xmlDocument.Descendants("TestAMode").FirstOrDefault();          
            
            if(mode.Value == "Find")
            {

            }
                        
        }
    }
}