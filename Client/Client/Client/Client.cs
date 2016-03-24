///////////////////////////////////////////////////////////////////////
// MT2Q2-Client.cs - Project #4 Service Client prototype             //
//                                                                   //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2014   //
// Updated by:  Nagendran sankaralignam                              //
///////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Xml.Linq;

namespace CodeAnalysis
{
    public class ElemRelation  // holds scope information
    {
        public string fromClass { get; set; }
        public string fromClassNamespace { get; set; }
        public string fromClassFilename { get; set; }
        public string toClass { get; set; }
        public string toClassNamespace { get; set; }
        public string toClassFilename { get; set; }
    }

    public class RelationshipRepository     //holds data to be displayed at parse2
    {
        static List<ElemRelation> relationship_ = new List<ElemRelation>();

        public List<ElemRelation> relationshipStorage
        {
            get { return relationship_; }
        }
    }

    public class List_Project  // holds scope information
    {
        public string projectName { get; set; }
    }

    public class Repository         //used to find the types in the file
    {
        static List<List_Project> ProjOutputList = new List<List_Project>();
        static Repository instance;
        public Repository()
        {
            instance = this;
        }
        public static Repository getInstance()
        {
            return instance;
        }
        // provides all actions access to current semiExp
        public List<List_Project> ProjOutputList_
        {
            get { return ProjOutputList; }
        }
    }

    public class MessageClient
    {
        static public IMessageService CreateClientChannel(string url)
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            EndpointAddress address = new EndpointAddress(url);
            ChannelFactory<IMessageService> factory = new ChannelFactory<IMessageService>(binding, address);
            return factory.CreateChannel();
        }

        static ServiceHost CreateServiceChannel(string url)
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            Uri baseAddress = new Uri(url);
            Type service = typeof(MessageService);
            ServiceHost host = new ServiceHost(service, baseAddress);
            host.AddServiceEndpoint(typeof(IMessageService), binding, baseAddress);
            return host;
        }

        public static void callMethod(Object msg)
        {
            Thread.Sleep(10000);
        }

        static bool t = true;
        public void connectWPF(string remoteAddr, string proj)
        {
            ServiceHost host = CreateServiceChannel("http://localhost:6001/MessageService");

            if (t == true)
            {
                host.Open();
                t = false;
            }

            if (proj == "ProjectList")
            {
                IMessageService proxy = CreateClientChannel("http://localhost:" + remoteAddr + "/MessageService");
                SvcMsg msg = new SvcMsg();
                msg.cmd = SvcMsg.Command.ProjectList;
                msg.src = new Uri("http://localhost:6001/MessageService");
                msg.dst = new Uri("http://localhost:" + remoteAddr + "/MessageService");
                msg.body = "Client connected";
                proxy.PostMessage(msg);
            }

            else
            {
                IMessageService proxy = CreateClientChannel("http://localhost:" + remoteAddr + "/MessageService");
                SvcMsg msg = new SvcMsg();
                msg.cmd = SvcMsg.Command.Dependency;
                msg.src = new Uri("http://localhost:6001/MessageService");
                msg.dst = new Uri("http://localhost:" + remoteAddr + "/MessageService");
                msg.body = "Client connected";
                proxy.PostMessage(msg);
            }
            Console.Write("\n  press key to terminate service");
            Console.Write("\n");
        }

        public static List<string> getProj()
        {
            List<String> projList_ = new List<String>();
            projList_ = projList;
            return projList_;
        }
        
        public static bool flag = false;
        static object locker_ = new object();
        public static string rMsg = "ABC";
        static int temp1 = 1;
        static List<string> projList = new List<string>();
        RelationshipRepository repo_ = new RelationshipRepository();

        public void ShowMessage(SvcMsg msg)
        {
            lock (locker_)
            {
                temp1 += 1;

                if (msg.cmd.ToString() == ("ProjectList"))
                {
                    List_Project p = new List_Project();

                    XDocument doc = XDocument.Parse(msg.body);
                    var q3 = from e in
                                 doc.Elements("ProjectListServer").Elements("ProjectName")
                             select e;
                    int numFuncs = q3.Count();

                    for (int i = 0; i < numFuncs; ++i)
                    {
                        projList.Add(q3.ElementAt(i).Value);
                        Console.Write("\n    {0}", q3.ElementAt(i).Value);
                    }

                    rMsg = msg.body;
                    flag = true;
                }

                if (msg.cmd.ToString() == ("Dependency"))
                {
                    repo_.relationshipStorage.Clear();

                    XDocument doc = XDocument.Parse(msg.body);
                    Console.Write("\n\n");
                    var entries = from e in
                                      doc.Elements("Relationships").Elements("TypeDependency")
                                  select e;
                    foreach (var entry in entries)
                    {
                        var q3 = from e in entry.Elements() select e;

                        int numFuncs = q3.Count() / 6;
                        for (int i = 0; i < numFuncs; ++i)
                        {
                            ElemRelation eR = new ElemRelation();

                            eR.fromClass = q3.ElementAt(6 * i).Value;
                            eR.fromClassFilename = q3.ElementAt(6 * i + 1).Value;
                            eR.fromClassNamespace = q3.ElementAt(6 * i + 2).Value;
                            eR.toClass = q3.ElementAt(6 * i + 3).Value;
                            eR.toClassFilename = q3.ElementAt(6 * i + 4).Value;
                            eR.toClassNamespace = q3.ElementAt(6 * i + 5).Value;
                            repo_.relationshipStorage.Add(eR);
                        }
                    }

                    var entries1 = from e in
                                       doc.Elements("Relationships").Elements("PackageDependency")
                                   select e;
                    foreach (var entry in entries1)
                    {
                        var q3 = from e in entry.Elements() select e;

                        int numFuncs = q3.Count() / 4;
                        for (int i = 0; i < numFuncs; ++i)
                        {
                            Console.Write("\n    {0}", q3.ElementAt(4 * i).Value);
                            Console.Write("\n    {0}", q3.ElementAt(4 * i + 1).Value);
                            Console.Write("\n    {0}", q3.ElementAt(4 * i + 2).Value);
                            Console.Write("\n    {0}", q3.ElementAt(4 * i + 3).Value);
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            "Client".Title();
            "Starting Message Service on Client".Title('-', false);

            ServiceHost host = CreateServiceChannel("http://localhost:6001/MessageService");
            host.Open();
            for (int i = 0; i < 1; i++)
            {
                IMessageService proxy = CreateClientChannel("http://localhost:6062/MessageService");
                SvcMsg msg = new SvcMsg();
                msg.cmd = SvcMsg.Command.ProjectList;
                msg.src = new Uri("http://localhost:6001/MessageService");
                msg.dst = new Uri("http://localhost:6062/MessageService");
                msg.body = "Client connected";
                proxy.PostMessage(msg);
            }
            Console.Write("\n  press key to terminate service");
            Console.ReadKey();
            Console.Write("\n");
        }
    }
}