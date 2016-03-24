using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using DependencyAnalyzer;
using System.Threading;

namespace DependencyAnalyzerUI
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    /// 
    //public class prolist
    //{
    //    public String 
    //}
    class EchoCommunicator : AbstractCommunicator
    {
        //lists for saving info on the client side
        List<String> projectList = new List<string>();
        List<String> filesList = new List<string>();
        List<String> projectList2 = new List<string>();
        List<String> filesList2 = new List<string>();
        //list properties
        public List<String> ProjectList { get { return projectList; } set { projectList = value; } }
        
        public List<String> FileList { get { return filesList; } set { filesList = value; } }
        public List<String> ProjectList2 { get { return projectList; } set { projectList = value; } }
        public List<String> FileList2 { get { return filesList; } set { filesList = value; } }

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

        protected override void ProcessMessages()
        {
            while (true)
            {
                ServiceMessage msg = bq.deQ();
                Console.Write("\n  {0} Recieved Message:\n", msg.TargetCommunicator);
                msg.ShowMessage();
                if (msg.SourceUrl == "http://localhost:8000/CommService")
                {
                    if (msg.ResourceName == "Projects")
                        ProjectList.AddRange(convertXmltoList(msg.Contents, "Project", "name"));
                    //if (msg.ResourceName == "Files")
                    //    FileList.AddRange(convertXmltoList(msg.Contents, "File", "name"));

                }
                if (msg.SourceUrl == "http://localhost:8002/CommService")
                {
                    if (msg.ResourceName == "Projects")
                        ProjectList2.AddRange(convertXmltoList(msg.Contents, "Project", "name"));
                    ////if (msg.ResourceName == "Files")
                    ////  FileList2.AddRange(convertXmltoList(msg.Contents, "File", "name"));
                }
                ServiceMessage reply = ServiceMessage.MakeMessage("relAnalyzer", "client-echo", msg.Contents, resName: "SelectedProjects");
                reply.TargetUrl = msg.SourceUrl;
                reply.SourceUrl = msg.TargetUrl;
                AbstractMessageDispatcher dispatcher = AbstractMessageDispatcher.GetInstance();
                dispatcher.PostMessage(reply);
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

    public partial class Page1 : Page
    {
        Test t;
        List<String> ServerList1;
        Sender sender;
        Receiver receiver;
        EchoCommunicator echo;
        string ServerUrl = "http://localhost:8000/CommService";
        string ServerUrl2 = "http://localhost:8002/CommService";
        string ClientUrl = "http://localhost:8005/CommService";

        //public List<String> ServerList1
        //{
        //    get { return ServerList; }
        //    set { ServerList = value; }
        //}
        public Page1()
        {
            InitializeComponent();
            Console.Write("\n  Starting CommService Client");
            Console.Write("\n =============================\n");
            sender = null;

            //Console.Write("\n  Press key to start client: ");
            //Console.ReadKey();

            sender = new Sender();
            //sender.Connect(ServerUrl);
            sender.Start();

            receiver = new Receiver(ClientUrl);
            receiver.Register(sender);

            // Don't need to start receiver unless you want
            // to send it messages, which we won't as all
            // our messages go to the server
            //receiver.Start();

            echo = new EchoCommunicator();
            echo.Name = "client-echo";
            receiver.Register(echo);
            echo.Start();

            //rcvThrd = new Thread(new ThreadStart(this.ThreadProc));
            //rcvThrd.IsBackground = true;
            //rcvThrd.Start();


            //t = new Test();
            //ServerList1 = t.getServerInfo();
            //foreach (String x in ServerList1) ;
           // LB_AvailableServers.Items.Add(new Test { RightLBitems = x });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           // Test abcd = LB_AvailableServers.SelectedItem as Test;
           //// LB_SelectedServers.Items.Add()
           // LB_SelectedServers.Items.Add(new Test { RightLBitems = abcd.RightLBitems });
           // //LB_SelectedServers.ItemsSource = ServerList1;
            //t.RightLBitems1.Add(LB_AvailableServers.SelectedItem.ToString());
            //LB_SelectedServers.ItemsSource = (IEnumerable<String>)t.RightLBitems1; 

        }

        class Test
        {
            public List<String> SelectedServersList;
            public List<String> _rightLBitems1;
            public string RightLBitems
            {
                get;
                set;

            }

            public Test()
            {
                _rightLBitems1 = new List<string>();
                SelectedServersList = new List<string>();
            }

           // private List<String> _RightLBitems;

            public List<String> getServerInfo()
            {
                XElement elem = XElement.Load(@"./Server.xml");
                var query = from x in elem.Descendants()
                                where x.Name == "Server"
                                select x;
                foreach(XElement x in query)
                {
                    _rightLBitems1.Add(x.Attribute("Name").Value.ToString());
                }

                return _rightLBitems1;
       
            }         
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }  

        private void Server1_CnctBtn_Click(object sender, RoutedEventArgs e)
        {
            int j=0;
            try
            {
                if (!this.sender.Connect(ServerUrl))
                    j = 10 / j;
               // this.sender.Connect(ServerUrl);
                ServiceMessage msg1 = ServiceMessage.MakeMessage("nav", "ServiceClient", "<root>some query stuff</root>", "Projects");
                msg1.SourceUrl = ClientUrl;
                msg1.TargetUrl = ServerUrl;
                Console.Write("\n  Posting message to \"{0}\" component", msg1.TargetCommunicator);
                this.sender.PostMessage(msg1);
                Thread.Sleep(1000);
                LB_ProjectsList.ItemsSource = echo.ProjectList;

            } 
            catch(Exception ex)
            {
                Window temp = new Window();
                StringBuilder msg = new StringBuilder("Unable to connect ");
                msg.Append("\nport = ");
                msg.Append(ServerUrl.ToString());
                temp.Content = msg.ToString();
                temp.Height = 100;
                temp.Width = 500;
                temp.Show();
            }

        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            this.sender.Stop();  // this function sends a quit message to client-echo
            this.sender.Wait();
            echo.Stop();
            echo.Wait();
            receiver.Close();   
        }

        private void Server2_CnctBtn_Click(object sender, RoutedEventArgs e)
        {
            int j = 0;
            try
            {
                if (!this.sender.Connect(ServerUrl))
                    j = 10 / j;
                // this.sender.Connect(ServerUrl);
                ServiceMessage msg1 = ServiceMessage.MakeMessage("nav", "ServiceClient", "<root>some query stuff</root>", "Projects");
                msg1.SourceUrl = ClientUrl;
                msg1.TargetUrl = ServerUrl;
                Console.Write("\n  Posting message to \"{0}\" component", msg1.TargetCommunicator);
                this.sender.PostMessage(msg1);
                Thread.Sleep(1000);
                LB_ProjectsList.ItemsSource = echo.ProjectList;

            }
            catch (Exception ex)
            {
                Window temp = new Window();
                StringBuilder msg = new StringBuilder("Unable to connect ");
                msg.Append("\nport = ");
                msg.Append(ServerUrl.ToString());
                temp.Content = msg.ToString();
                temp.Height = 100;
                temp.Width = 500;
                temp.Show();
            }

        }
      }
      
}

