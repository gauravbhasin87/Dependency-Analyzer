///////////////////////////////////////////////////////////////////////////////
// Client.cs - Document Vault client prototype                               //
//                                                                           //
// Gaurav Bhasin - Software Modeling and Analysis, Fall 2014                 //
///////////////////////////////////////////////////////////////////////////////
/*
 *  Package Contents:
 *  -----------------
 *  This package defines two classes:
 *  Client
 *    Defines the behavior of a DocumentVault prototype client.
 *  EchoCommunicator
 *    Defines behavior for processing client received messages.
 *    
 *    
 *  
 * Required Files:
 * - Client:      Client.cs, Sender.cs, Receiver.cs
 * - Components:  ICommLib, AbstractCommunicator, BlockingQueue
 * - CommService: ICommService, CommService
 * 
 * 
 *
 *  Required References:
 *  - System.ServiceModel
 *  - System.RuntimeSerialization
 *
 *  Build Command:  devenv DependencyAnalyzer.sln /rebuild debug
 *
 *  Maintenace History:
 *  ver 2.1 : Nov 7, 2013
 *  - replaced ClientSender with a merged Sender class
 *  ver 2.0 : Nov 5, 2013
 *  - fixed bugs in the client shutdown process
 *  ver 1.0 : Oct 29, 2013
 *  - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml.Linq;
using RepositoryNS;
using DisplayPkg;

namespace DependencyAnalyzer
{
  // Echo Communicator displays reply message

  class EchoCommunicator : AbstractCommunicator
      {
          //lists for saving info on the client side
          List<String> projectList = new List<string>();
          List<String> filesList = new List<string>();
          List<String> projectList2 = new List<string>();
          List<String> filesList2 = new List<string>();
          //list properties
          public List<String> ProjectList {  get { return projectList; }  set { projectList = value; }}
          public List<String> FileList { get { return filesList; } set { filesList = value; } }
          public List<String> ProjectList2 { get { return projectList; } set { projectList = value; } }
          public List<String> FileList2 { get { return filesList; } set { filesList = value; } }
          Repository<Elem>[] repoArray;

        public XElement convertListtoXml(List<String> list, string rootName, string elem, string attribute)
        {
            XElement root = new XElement(rootName);
            foreach (String s in list)
            {
                XElement project = new XElement(elem);
                XAttribute name = new XAttribute(attribute, s);
                project.Add(name);
                root.Add(project);
            }
            return root;
        }

        public List<String> convertXmltoList(string content, string elemName, string attributeName)
        {
            List<String> inList = new List<String>();
            XElement elem = XElement.Parse(content);
            var query = from x in elem.Descendants()
                        where x.Name == elemName
                        select x;
            foreach (XElement x in query)
                inList.Add(x.Attribute(attributeName).Value.ToString());
            return inList;
        }

        public List<ElemForRel> convertXmltoRelationList(string xml)
        {
            List<ElemForRel> Relations = new List<ElemForRel>();
            XElement elem = XElement.Parse(xml);
            var query = from x in elem.Descendants()
                        where x.Name == "Relation"
                        select x;
            foreach (XElement x in query)
            {
                ElemForRel e = new ElemForRel();
                e.fileName = x.Attribute("filename").Value.ToString();
                e.Relation = x.Attribute("relation").Value.ToString();
                e.type1 = x.Attribute("type1").Value.ToString();
                e.type1Name = x.Attribute("type1name").Value.ToString();
                e.text = x.Attribute("text").Value.ToString();
                e.fileName2 = x.Attribute("filename2").Value.ToString();
                e.type2 = x.Attribute("type2").Value.ToString();
                e.type2Name = x.Attribute("type2name").Value.ToString();
                Relations.Add(e);
            }
            return Relations;
        }
        public Repository<Elem>[] convertXmltoRepArray(string[] xmlArray)
        {
            repoArray = new Repository<Elem>[xmlArray.Length];
            int i = 0;
            foreach (string s in xmlArray)
            {

                XElement typeTable = XElement.Parse(s);
                var query = from x in typeTable.Descendants()
                            where x.Name == "File"
                            select x;
                foreach (XElement x in query)
                    repoArray[i] = new Repository<Elem>(x.Attribute("name").Value.ToString());

                foreach (XElement type in query.Descendants())
                {
                    Elem elem = new Elem();
                    elem.begin = Convert.ToInt32(type.Attribute("begin").Value.ToString());
                    elem.end = Convert.ToInt32(type.Attribute("end").Value.ToString());
                    //      elem.filename = type.Attribute("file_name").Value.ToString();
                    elem.name = type.Name.ToString();
                    elem.type = type.Attribute("type").Value.ToString();
                    elem.funcComplexity = Convert.ToInt16(type.Attribute("complexity").Value);
                    repoArray[i].Locations.Add(elem);
                }
                i++;
            }
            return repoArray;
        }

        protected override void ProcessMessages()
        {
          while (true)
          {
            ServiceMessage msg = bq.deQ();
            Console.Write("\n  {0} Recieved Message:\n", msg.TargetCommunicator);
            msg.ShowMessage();
            if (msg.SourceUrl == "http://localhost:8000/CommService" && msg.ResourceName == "Projects")
            {
              ProjectList.AddRange(convertXmltoList(msg.Contents, "Project", "name"));
                ServiceMessage reply = ServiceMessage.MakeMessage("relAnalyzer", "client-echo", msg.Contents, resName: "SelectedProjects");
                reply.TargetUrl = msg.SourceUrl;
                reply.SourceUrl = msg.TargetUrl;
                AbstractMessageDispatcher dispatcher = AbstractMessageDispatcher.GetInstance();
                dispatcher.PostMessage(reply);
                
            }
            if (msg.SourceUrl == "http://localhost:8002/CommService" && msg.ResourceName == "Projects")
            {
                ProjectList2.AddRange(convertXmltoList(msg.Contents, "Project", "name"));
                ServiceMessage reply = ServiceMessage.MakeMessage("relAnalyzer", "client-echo", msg.Contents, resName: "SelectedProjects");
                reply.TargetUrl = msg.SourceUrl;
                reply.SourceUrl = msg.TargetUrl;
                AbstractMessageDispatcher dispatcher = AbstractMessageDispatcher.GetInstance();
                dispatcher.PostMessage(reply);
            }

            if (msg.ResourceName == "TypeTablefromServer1") 
            {
                Console.WriteLine("Merged Type Table from Server1 and Server2 :");
                Console.WriteLine("********************************************\n\n\n\n");
                RepositoryNS.Repository<Elem>[] repoArray = convertXmltoRepArray(msg.Array);
                DisplayPkg.DisplayRepository.displayTypeAnalysis(repoArray,true);
            }
            if (msg.ResourceName == "RelationsTablefromServer1")
            {
                Console.WriteLine("\n\n\nRelations found on Server1:");
                Console.WriteLine("*********************************\n\n\n\n");
                List<ElemForRel> list= convertXmltoRelationList(msg.Contents);
                XDocument x = XDocument.Parse(msg.Contents);
                x.Save("RelationsonServer1.xml");
                foreach (ElemForRel e in list)
                    Console.WriteLine(e.ToString());
            }
            if (msg.ResourceName == "RelationsTablefromServer2")
            {
                Console.WriteLine("\n\n\nRelations found on Server2:");
                Console.WriteLine("*********************************\n\n\n\n");
                List<ElemForRel> list = convertXmltoRelationList(msg.Contents);
                XDocument x = XDocument.Parse(msg.Contents);
                x.Save("RelationsonServer2.xml");
                foreach (ElemForRel e in list)
                    Console.WriteLine(e.ToString());
            }
            if (msg.Contents == "quit")
            {
                if (Verbose)
                Console.Write("\n  Echo shutting down");
                // shut down dispatcher
                msg.TargetCommunicator = "dispatcher";
                AbstractMessageDispatcher.GetInstance().PostMessage(msg);
                break;
            }
            }
        }
      }
  class Client
  {
    static void Main(string[] args)
    {
      Console.Write("\n  Starting CommService Client");
      Console.Write("\n =============================\n");

      string ServerUrl = "http://localhost:8000/CommService";
      string ServerUrl2 = "http://localhost:8002/CommService";
      Sender sender = null;

      Console.Write("\n  Press key to start client: ");
      Console.ReadKey();

      sender = new Sender();
      //sender.Connect(ServerUrl);
      sender.Start();

      string ClientUrl = "http://localhost:8005/CommService";
      Receiver receiver = new Receiver(ClientUrl);
      receiver.Register(sender);  

      // Don't need to start receiver unless you want
      // to send it messages, which we won't as all
      // our messages go to the server
      //receiver.Start();

      EchoCommunicator echo = new EchoCommunicator();
      echo.Name = "client-echo";
      receiver.Register(echo);
      echo.Start();
      
      //get projects list
      ServiceMessage msg1 =
      ServiceMessage.MakeMessage("nav", "ServiceClient", "<root>some query stuff</root>", "Projects");
      msg1.SourceUrl = ClientUrl;
      msg1.TargetUrl = ServerUrl;
      Console.WriteLine("\n  Posting message to \"{0}\" component", msg1.TargetCommunicator);
      sender.PostMessage(msg1);

      Console.Write("\n  Wait for Server replies, then press a key to exit: ");
      Console.ReadKey();
            
      ServiceMessage msg4 =
      ServiceMessage.MakeMessage("nav", "ServiceClient", "<root>some query stuff</root>", "Projects");
      msg4.SourceUrl = ClientUrl;
      msg4.TargetUrl = ServerUrl2;
      Console.Write("\n  Posting message to \"{0}\" component", msg4.TargetCommunicator);
      sender.PostMessage(msg4);

      Console.Write("\n  Wait for Server replies, then press a key to exit: ");
      Console.ReadKey();

      Console.Write("\n  Wait for Server replies, then press a key to exit: ");
      Console.ReadKey();

      Console.Write("\n  Wait for Server replies, then press a key to exit: ");
      Console.ReadKey();

      sender.Stop();  // this function sends a quit message to client-echo
      sender.Wait();
      echo.Stop();
      echo.Wait();
      receiver.Close();   
      Console.Write("\n\n");
    }
  }
}
