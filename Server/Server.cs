///////////////////////////////////////////////////////////////////////////////
// Server.cs - Dependency Analyser Server                                    //
//                                                                           //
// Gaurav Bhasin, CSE681 - Software Modeling and Analysis, Fall 2013         //
///////////////////////////////////////////////////////////////////////////////
/*
 *  Package Contents:
 *  -----------------
 *  This package defines following communicators
 *  1. Navigation Communicator
 *  2. TypeAnalysis Communicator
 *  3. RelationCommunicator
 *  4. Echo Communicator
 *  Required Files:
 *  - Server:      Server.cs, Sender.cs, Receiver.cs
 *  - Components:  ICommLib, AbstractCommunicator, BlockingQueue
 *  - CommService: ICommService, CommService
 *
 *  Required References:
 *  - System.ServiceModel
 *  - System.RuntimeSerialization
 *
 *  Build Command:  devenv DependencyAnalyzer.sln /rebuild debug
 *
 *  Maintenace History:
 *  ver 2.1 : Nov 7, 2013
 *  - replaced ServerSender with a merged Sender class
 *  ver 2.0 : Nov 5, 2013
 *  - fixed bugs in the message routing process
 *  ver 1.0 : Oct 29, 2013
 *  - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using ManagedCodeAnalyzer;
using RepositoryNS;
using DisplayPkg;

namespace DependencyAnalyzer
{
    class RelationCommunicator : AbstractCommunicator
    {
        Repository<Elem>[] repoArray;
        Repository<Elem>[] tempArray = new Repository<Elem>[0];
        List<string> files = new List<String>();
        Repository<ElemForRel> repForRel;
        List<String> SelectedProjects = new List<string>();

        public void findFiles(string path, bool recurse, string pattern)
        {
            try
            {
                string[] newFiles = Directory.GetFiles(path, pattern);
                for (int i = 0; i < newFiles.Length; ++i)
                    newFiles[i] = Path.GetFullPath(newFiles[i]);
                files.AddRange(newFiles);
                if (recurse)
                {
                    string[] dirs = Directory.GetDirectories(path);
                    foreach (string dir in dirs)
                        findFiles(dir, recurse, pattern);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public List<String> convertXmltoList(string content, string elemName, string attributeName)
        {
            List<String> inList = new List<String>();
            XElement elem = XElement.Parse(content);
            var query = from x in elem.Descendants()
                        where x.Name == elemName
                        select x;
            foreach (XElement x in query)
            {
                inList.Add(x.Attribute(attributeName).Value.ToString());
            }
            return inList;
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

        public void findallfiles(List<String> projects)
        {
            foreach(string s in projects)
            {
                findFiles(s, true, "*.cs");
            }
            foreach (string s in files)
                Console.WriteLine(s);
        }

        public string convertRepositoryforReltoXML(Repository<ElemForRel> repoforRel)
        {

            string xml;
            int i = 0;
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode rootNode = xmlDoc.CreateElement("ManagedCodeAnalyzer");
                xmlDoc.AppendChild(rootNode);
                XmlNode relationAnalysis = xmlDoc.CreateElement("relationshipLocations");
                rootNode.AppendChild(relationAnalysis);
                foreach (ElemForRel e in repoforRel.Locations)
                {
                    XmlNode Type = xmlDoc.CreateElement("Relation");
                    XmlAttribute filename = xmlDoc.CreateAttribute("filename");
                    filename.Value = e.fileName;
                    XmlAttribute relation = xmlDoc.CreateAttribute("relation");
                    relation.Value = e.Relation;
                    XmlAttribute type1 = xmlDoc.CreateAttribute("type1");
                    type1.Value = e.type1.ToString();
                    XmlAttribute type1name = xmlDoc.CreateAttribute("type1name");
                    type1name.Value = e.type1Name.ToString();
                    XmlAttribute text = xmlDoc.CreateAttribute("text");
                    text.Value = e.text.ToString();
                    XmlAttribute type2 = xmlDoc.CreateAttribute("type2");
                    type2.Value = e.type2.ToString();
                    XmlAttribute type2name= xmlDoc.CreateAttribute("type2name");
                    type2name.Value = e.type2Name.ToString();
                    Type.Attributes.Append(filename);
                    Type.Attributes.Append(relation);
                    Type.Attributes.Append(type1);
                    Type.Attributes.Append(type1name);
                    relationAnalysis.AppendChild(Type);
                
                }
                xml = xmlDoc.OuterXml;
                return xml;
        }

        public string[] convertRepositoryArraytoXML(Repository<Elem>[] repoArray)
        {
            string[] xmlArray = new String[repoArray.Length];
            int i = 0;
            foreach (Repository<Elem> rep in repoArray)
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode rootNode = xmlDoc.CreateElement("ManagedCodeAnalyzer");
                xmlDoc.AppendChild(rootNode);
                XmlNode typeAnalysis = xmlDoc.CreateElement("typeLocations");
                rootNode.AppendChild(typeAnalysis);
                int index = rep.AttachedFile.LastIndexOf("\\");
                string fName = rep.AttachedFile.Substring(index + 1, rep.AttachedFile.Length - 1 - index);
                XmlNode File = xmlDoc.CreateElement("File");
                XmlAttribute fname = xmlDoc.CreateAttribute("name");
                fname.Value = fName;
                File.Attributes.Append(fname);
                typeAnalysis.AppendChild(File);
                foreach (Elem e in rep.Locations)
                {
                    XmlNode Type = xmlDoc.CreateElement(e.name);
                    XmlAttribute type = xmlDoc.CreateAttribute("type");
                    type.Value = e.type;
                    XmlAttribute name = xmlDoc.CreateAttribute("name");
                    name.Value = e.type;
                    XmlAttribute begin = xmlDoc.CreateAttribute("begin");
                    begin.Value = e.begin.ToString();
                    XmlAttribute end = xmlDoc.CreateAttribute("end");
                    end.Value = e.end.ToString();
                    XmlAttribute complexity = xmlDoc.CreateAttribute("complexity");
                    complexity.Value = e.funcComplexity.ToString();

                    Type.Attributes.Append(type);
                    Type.Attributes.Append(begin);
                    Type.Attributes.Append(end);
                    Type.Attributes.Append(complexity);
                    File.AppendChild(Type);
                    typeAnalysis.AppendChild(File);
                }
                xmlArray[i] = xmlDoc.OuterXml;
                i++;
            }
           return xmlArray;
        }
        public string convertRelationListtoXml(List<ElemForRel> list)
        {
            string xml;
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("Relations");
            xmlDoc.AppendChild(rootNode);
            foreach (ElemForRel e in list)
            {
                XmlNode Type = xmlDoc.CreateElement("Relation");
                XmlAttribute filename = xmlDoc.CreateAttribute("filename");
                filename.Value = e.fileName;
                XmlAttribute relation = xmlDoc.CreateAttribute("relation");
                relation.Value = e.Relation;
                XmlAttribute type1 = xmlDoc.CreateAttribute("type1");
                type1.Value = e.type1.ToString();
                XmlAttribute type1name = xmlDoc.CreateAttribute("type1name");
                type1name.Value = e.type1Name.ToString();
                XmlAttribute text = xmlDoc.CreateAttribute("text");
                text.Value = e.text.ToString();
                XmlAttribute type2 = xmlDoc.CreateAttribute("type2");
                type2.Value = e.type2.ToString();
                XmlAttribute type2name = xmlDoc.CreateAttribute("type2name");
                type2name.Value = e.type2Name.ToString();
                XmlAttribute filename2 = xmlDoc.CreateAttribute("filename2");
                filename2.Value = e.fileName2.ToString();

                Type.Attributes.Append(filename);
                Type.Attributes.Append(relation);
                Type.Attributes.Append(type1);
                Type.Attributes.Append(type1name);
                Type.Attributes.Append(text);
                Type.Attributes.Append(type2);
                Type.Attributes.Append(type2);
                Type.Attributes.Append(type2name);
                Type.Attributes.Append(filename2);
                rootNode.AppendChild(Type);
            }
            xml = xmlDoc.OuterXml;
            return xml;
        }

        public List<ElemForRel> findRelationship(Repository<ElemForRel> repoForRel)
        {
            List<ElemForRel> relLocations = new List<ElemForRel>();
            foreach (ElemForRel e in repoForRel.Locations)
            {
                if (e.fileName != e.fileName2)
                    relLocations.Add(e);
            }
            return relLocations;
        }
        protected override void ProcessMessages()
        {
            int i=0;
            while (true)
            {
                ServiceMessage msg = bq.deQ();
                Console.Write("\n  {0} Recieved Message:\n", msg.TargetCommunicator);
                msg.ShowMessage();

                if (msg.ResourceName == "SelectedProjects")
                {
                    i++;
                    SelectedProjects.AddRange(convertXmltoList(msg.Contents, "Project", "name"));
                    findallfiles(SelectedProjects);
                    Console.WriteLine("\n\n\nMessage Recieved in RelAnalyzer    "+i+"\n\n\n");
                }
                if(msg.ResourceName == "TypeTablefromServer1")
                {
                    Console.WriteLine("TypeTable from own Server Received ");
                    foreach (string s in msg.Array)
                        Console.WriteLine(s);
                    repoArray = convertXmltoRepArray(msg.Array);
                    i++;
                    Console.WriteLine("\n\n\nMessage Recieved in RelAnalyzer    " + i + "\n\n\n");
                }
                if (msg.ResourceName == "TypeTablefromServer2")
                {
                    Console.WriteLine("TypeTable from Server2 Received ");
                    foreach (string s in msg.Array)
                        Console.WriteLine(s);
                    tempArray = convertXmltoRepArray(msg.Array);
                    i++;
                    Console.WriteLine("\n\n\nMessage Recieved in RelAnalyzer    " + i + "\n\n\n");
                }
                if(i==3)
                {
                    var z = new Repository<Elem>[tempArray.Length + repoArray.Length];
                    tempArray.CopyTo(z, 0);
                    repoArray.CopyTo(z, tempArray.Length);
                    DisplayRepository.displayTypeAnalysis(z, true);
                    Console.WriteLine("\n\n\n\n\n\n");
                    string[] xml = convertRepositoryArraytoXML(z);
                    ServiceMessage reply3 = ServiceMessage.MakeMessage("client-echo", "relAnalyzer", "", Array: xml, resName: "TypeTablefromServer1");
                    reply3.TargetUrl = "http://localhost:8005/CommService";
                    reply3.SourceUrl = msg.TargetUrl;
                    AbstractMessageDispatcher dispatcher = AbstractMessageDispatcher.GetInstance();
                    dispatcher.PostMessage(reply3);
                    Analyzer.RepoArray = z;
                    repForRel = Analyzer.doAnalyse(files);
                    List<ElemForRel> relations = findRelationship(repForRel);
                    string content = convertRelationListtoXml(relations);
                    //string contents = convertRepositoryforReltoXML(repForRel);
                    ServiceMessage reply4 = ServiceMessage.MakeMessage("client-echo", "relAnalyzer", content, resName: "RelationsTablefromServer1");
                    reply4.TargetUrl = "http://localhost:8005/CommService";
                    reply4.SourceUrl = msg.TargetUrl;
                    dispatcher.PostMessage(reply4);
                    i++;
                }
                if(i>3)
                {

                }
                if (msg.Contents == "quit")
                    break;
            }
        }
    }

  class TypeAnalysisCommunicator : AbstractCommunicator
  {
      //List<String> InputPrjList = new List<string>();
      //Repository<Elem>[] repoArray;
      List<String> files = new List<string>();

      public void findFiles(string path, bool recurse, string pattern)
      {
          try
          {
              string[] newFiles = Directory.GetFiles(path, pattern);
              for (int i = 0; i < newFiles.Length; ++i)
                  newFiles[i] = Path.GetFullPath(newFiles[i]);
              files.AddRange(newFiles);
              if (recurse)
              {
                  string[] dirs = Directory.GetDirectories(path);
                  foreach (string dir in dirs)
                      findFiles(dir, recurse, pattern);
              }
          }
          catch (Exception e)
          {
              Console.WriteLine(e.Message);
          }
      }

      public List<String> convertXmltoList(string content, string elemName, string attributeName)
      {
          List<String> inList = new List<String>();
          XElement elem = XElement.Parse(content);
          var query = from x in elem.Descendants()
                      where x.Name == elemName
                      select x;
          foreach (XElement x in query)
          {
              inList.Add(x.Attribute(attributeName).Value.ToString());
          }
          return inList;
      }

      public string[] convertRepositoryArraytoXML(Repository<Elem>[] repoArray)
      {

          string[] xmlArray = new String[repoArray.Length];
          int i = 0;
                foreach (Repository<Elem> rep in repoArray)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    XmlNode rootNode = xmlDoc.CreateElement("ManagedCodeAnalyzer");
                    xmlDoc.AppendChild(rootNode);
                    XmlNode typeAnalysis = xmlDoc.CreateElement("typeLocations");
                    rootNode.AppendChild(typeAnalysis);
                    int index = rep.AttachedFile.LastIndexOf("\\");
                    string fName = rep.AttachedFile.Substring(index + 1, rep.AttachedFile.Length - 1 - index);
                    XmlNode File = xmlDoc.CreateElement("File");
                    XmlAttribute fname = xmlDoc.CreateAttribute("name");
                    fname.Value = fName;
                    File.Attributes.Append(fname);
                    typeAnalysis.AppendChild(File);
                    foreach (Elem e in rep.Locations)
                    {
                        XmlNode Type = xmlDoc.CreateElement(e.name);
                        XmlAttribute type = xmlDoc.CreateAttribute("type");
                        type.Value = e.type;
                        XmlAttribute name = xmlDoc.CreateAttribute("name");
                        name.Value = e.type;
                        XmlAttribute begin = xmlDoc.CreateAttribute("begin");
                        begin.Value = e.begin.ToString();
                        XmlAttribute end = xmlDoc.CreateAttribute("end");
                        end.Value = e.end.ToString();
                        XmlAttribute complexity = xmlDoc.CreateAttribute("complexity");
                        complexity.Value = e.funcComplexity.ToString();

                        Type.Attributes.Append(type);
                        Type.Attributes.Append(begin);
                        Type.Attributes.Append(end);
                        Type.Attributes.Append(complexity);
                        File.AppendChild(Type);
                        typeAnalysis.AppendChild(File);
                    }
                    xmlArray[i] = xmlDoc.OuterXml;
                    i++;
                   // xmlDoc.Save("Typetable"+i+".xml");
                }
                //xmlDoc.Save("Typetable.xml");
                return xmlArray;
      }

      protected override void ProcessMessages()
      {
          while (true)
          {
              ServiceMessage msg = bq.deQ();
              Console.Write("\n  {0} Recieved Message:\n", msg.TargetCommunicator);
              msg.ShowMessage();
              findFiles("./ProjectsDirectory",true,"*.cs");
              //InputPrjList = convertXmltoList(msg.Contents,"File","name");
              RepositoryNS.Repository<Elem>[] repoElemArray = Analyzer.doParse(files);
              string[] xml = convertRepositoryArraytoXML(repoElemArray);
              foreach (string s in xml)
                  Console.WriteLine(s);
              //post to other server
              ServiceMessage reply = ServiceMessage.MakeMessage("relAnalyzer", "typAnalyzer", "", Array: xml, resName: "TypeTablefromServer1");
              reply.TargetUrl = "http://localhost:8002/CommService";
              reply.SourceUrl = msg.TargetUrl;
              AbstractMessageDispatcher dispatcher = AbstractMessageDispatcher.GetInstance();
              dispatcher.PostMessage(reply);
              
              //post message to relAnalyzerCommunicator
              ServiceMessage reply2 = ServiceMessage.MakeMessage("relAnalyzer", "typAnalyzer", "", Array: xml, resName:"TypeTablefromServer1");
                reply2.TargetUrl = msg.TargetUrl;
                reply2.SourceUrl = msg.TargetUrl;
               // dispatcher = AbstractMessageDispatcher.GetInstance();
                dispatcher.PostMessage(reply2);
                
              if (msg.Contents == "quit")
                  break;
          }
      }
  }
  // Echo Communicator

  class EchoCommunicator : AbstractCommunicator
  {
    protected override void ProcessMessages()
    {
      while (true)
      {
        ServiceMessage msg = bq.deQ();
        Console.Write("\n  {0} Recieved Message:\n", msg.TargetCommunicator);
        msg.ShowMessage();
        Console.Write("\n  Echo processing completed\n");
        if (msg.Contents == "quit")
          break;
      }
    }
  }
 
  // Query Communicator

  class QueryCommunicator : AbstractCommunicator
  {
    protected override void ProcessMessages()
    {
      while (true)
      {
        ServiceMessage msg = bq.deQ();
        Console.Write("\n  {0} Recieved Message:\n", msg.TargetCommunicator);
        msg.ShowMessage();
        Console.Write("\n  Query processing is an exercise for students\n");
        if (msg.Contents == "quit")
          break;
        ServiceMessage reply = ServiceMessage.MakeMessage("client-echo", "query", "reply from query");
        reply.TargetUrl = msg.SourceUrl;
        reply.SourceUrl = msg.TargetUrl;
        AbstractMessageDispatcher dispatcher = AbstractMessageDispatcher.GetInstance();
        dispatcher.PostMessage(reply);
      }
    }
  }


  // Navigate Communicator

  class NavigationCommunicator : AbstractCommunicator
  {
      private List<string> files = new List<string>();
      private List<String> PrjList = new List<string>();
      public void findProjects(string path)
      {
          string[] dirs = Directory.GetDirectories(path);
          PrjList.AddRange(dirs);
      }
      public void findFiles(string path, bool recurse, string pattern)
      {
          try
          {
             string[] newFiles = Directory.GetFiles(path, pattern);
             for (int i = 0; i < newFiles.Length; ++i)
                 newFiles[i] = Path.GetFullPath(newFiles[i]);
             files.AddRange(newFiles);
              if (recurse)
              {
                  string[] dirs = Directory.GetDirectories(path);
                  foreach (string dir in dirs)
                      findFiles(dir, recurse, pattern);
              }
          }
          catch (Exception e)
          {
              Console.WriteLine(e.Message);
          }
       }
   
    public XElement convertListtoXml(List<String> list, string rootName ,string elem, string attribute)
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
    protected override void ProcessMessages()
    {
      while (true)
      {
        ServiceMessage msg = bq.deQ();
        Console.Write("\n  {0} Recieved Message:\n", msg.TargetCommunicator);
        msg.ShowMessage();
        if (msg.Contents == "quit")
            break;
        
        XElement root = null;
        ServiceMessage reply = null;
        if (msg.ResourceName == "Projects")
        {
            findProjects("./ProjectsDirectory");
            root = convertListtoXml(PrjList, "Projects", "Project", "name");
            reply = ServiceMessage.MakeMessage("client-echo", "nav", root.ToString(), resName:"Projects");
        }
        if(msg.ResourceName == "Files")
        {
            findFiles("./ProjectsDirectory", true, "*.cs");
            root = convertListtoXml(files, "Files", "File", "name");
            reply = ServiceMessage.MakeMessage("client-echo", "nav", root.ToString(), resName:"Files");
        }
        reply.TargetUrl = msg.SourceUrl;
        reply.SourceUrl = msg.TargetUrl;
        AbstractMessageDispatcher dispatcher = AbstractMessageDispatcher.GetInstance();
        dispatcher.PostMessage(reply);
        Console.Write("\n  Navigation processing is an exercise for students\n");
      }
    }
  }
  // Server

  class Server
  {
    static void Main(string[] args)
    {
      Console.Write("\n  Starting CommService");
      Console.Write("\n ======================\n");

      string ServerUrl = "http://localhost:8000/CommService";
      Receiver receiver = new Receiver(ServerUrl);

      string ClientUrl = "http://localhost:8005/CommService";

      Sender sender = new Sender();
      sender.Name = "sender";
      sender.Connect(ClientUrl);
      receiver.Register(sender);
      sender.Start();

      // Test Component that simply echos message

      EchoCommunicator echo = new EchoCommunicator();
      echo.Name = "echo";
      receiver.Register(echo);
      echo.Start();

      // Placeholder for query processor

      QueryCommunicator query = new QueryCommunicator();
      query.Name = "query";
      receiver.Register(query);
      query.Start();

      // Placeholder for component that searches for and returns 
      // parent/child relationships

      NavigationCommunicator nav = new NavigationCommunicator();
      nav.Name = "nav";
      receiver.Register(nav);
      nav.Start();

      TypeAnalysisCommunicator typAnalyzer = new TypeAnalysisCommunicator();
      typAnalyzer.Name = "typAnalyzer";
      receiver.Register(typAnalyzer);
      typAnalyzer.Start();

      RelationCommunicator relAnalyzer = new RelationCommunicator();
      relAnalyzer.Name = "relAnalyzer";
      receiver.Register(relAnalyzer);
      relAnalyzer.Start();

      //GetPrjectListCommunicator prjList = new GetPrjectListCommunicator();
      //prjList.Name = "projectList";
      //receiver.Register(prjList);
      //prjList.Start();

      ServiceMessage toTypCom = ServiceMessage.MakeMessage("typAnalyzer", "Server", "");
      toTypCom.TargetUrl = ServerUrl;
      toTypCom.SourceUrl = ServerUrl;
      sender.PostMessage(toTypCom);

      Console.Write("\n  Started CommService - Press key to exit:\n ");
      Console.ReadKey();
    }
  }
}











//GetProjectList Communicator
//class GetPrjectListCommunicator : AbstractCommunicator
//{
//    private List<String> PrjList = new List<string>();

//    public void findProjects(string path)
//    {
//        string[] dirs = Directory.GetDirectories(path);
//        PrjList.AddRange(dirs);
//    }
//    protected override void ProcessMessages()
//    {
//      while (true)
//      {
//          ServiceMessage msg = bq.deQ();
//          Console.Write("\n  {0} Recieved Message:\n", msg.TargetCommunicator);
//          msg.ShowMessage();
//          if (msg.Contents == "quit")
//            break;

//          findProjects("./ProjectsDirectory");

//          XElement root = new XElement("Projects");
//          foreach (String s in PrjList)
//          {
//              XElement project = new XElement("Project");
//              XAttribute name = new XAttribute("name", s);
//              project.Add(name);
//              root.Add(project);
//          }

//          //Console.Write("\n  Navigation processing is an exercise for students\n");

//          ServiceMessage reply = ServiceMessage.MakeMessage("client-echo", "GetPrjectListCommunicator", root.ToString(), "ProjectsList");
//          reply.TargetUrl = msg.SourceUrl;
//          reply.SourceUrl = msg.TargetUrl;
//          AbstractMessageDispatcher dispatcher = AbstractMessageDispatcher.GetInstance();
//          dispatcher.PostMessage(reply);
//      }
//   }
//}

