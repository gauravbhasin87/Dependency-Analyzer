/////////////////////////////////////////////////////////////////////
// Window1.xaml.cs - WPF User Interface for WCF Communicator       //
// ver 2.2                                                         //
// Jim Fawcett, CSE681 - Software Modeling & Analysis, Summer 2008 //
// Updated by:  Nagendran sankaralignam                            //
/////////////////////////////////////////////////////////////////////
/*
 * Maintenance History:
 * ====================
 * ver 1.0 : 15 Nov 14
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace CodeAnalysis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string rcvdMsg = "";
        int MaxMsgCount = 100;
        Thread rcvThrd = null;
        delegate void NewMessage(string msg);
        event NewMessage OnNewMessage;

        void ThreadProc()
        {
            while (true)
            {
                // call window functions on UI thread
                this.Dispatcher.BeginInvoke(
                  System.Windows.Threading.DispatcherPriority.Normal, OnNewMessage, rcvdMsg);
            }
        }

        void OnNewMessageHandler(string msg)
        {
            ResultListBox.Items.Insert(0, msg);
            if (ResultListBox.Items.Count > MaxMsgCount)
                ResultListBox.Items.RemoveAt(ResultListBox.Items.Count - 1);
        }

        public MainWindow()
        {
            InitializeComponent();
            Title = "Client GUI";
            OnNewMessage += new NewMessage(OnNewMessageHandler);
            ConnectServer1.IsEnabled = true;
            ConnectServer2.IsEnabled = true;
            Filebtn.IsEnabled = false;
        }

        private void Connect1(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageClient C = new MessageClient();
                string remoteAddr = "6060";
                string proj = "ProjectList";
                C.connectWPF(remoteAddr, proj);
                //C.connectWPF(remoteAddr);
                Filebtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Window temp = new Window();
                StringBuilder msg = new StringBuilder(ex.Message);
                msg.Append("\nport = ");
                msg.Append("6060".ToString());
                temp.Content = msg.ToString();
                temp.Height = 200;
                temp.Width = 500;
                temp.Show();
            }
        }

        private void Connect2(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageClient C = new MessageClient();
                string remoteAddr = "6062";
                string proj = "ProjectList";
                C.connectWPF(remoteAddr, proj);
                Filebtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Window temp = new Window();
                StringBuilder msg = new StringBuilder(ex.Message);
                msg.Append("\nport = ");
                msg.Append("6062".ToString());
                temp.Content = msg.ToString();
                temp.Height = 200;
                temp.Width = 500;
                temp.Show();
            }
        }

        private void Filebtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ResultListBox.Items.Clear();
                List<string> msg = MessageClient.getProj();
                foreach (string m in msg)
                {
                    ResultListBox.Items.Insert(0, m);
                }
                //ProjectSendClick.IsEnabled = true;
                Filebtn.IsEnabled = false;

            }
            catch (Exception ex)
            {
                Window temp = new Window();
                temp.Content = ex.Message;
                temp.Height = 200;
                temp.Width = 500;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageClient C = new MessageClient();
                string remoteAddr = "6060";
                string proj = "Dependency";
                C.connectWPF(remoteAddr, proj);

                Resultlist.Items.Clear();
                List<ElemRelation> temp = new List<ElemRelation>();
                RelationshipRepository repo_ = new RelationshipRepository();
                temp = repo_.relationshipStorage;
                
                foreach (ElemRelation m in temp)
                {
                    Resultlist.Items.Insert(0, m.fromClass);
                    Resultlist.Items.Insert(0, m.fromClassFilename);
                    Resultlist.Items.Insert(0, m.fromClassNamespace);
                    Resultlist.Items.Insert(0, m.toClass);
                    Resultlist.Items.Insert(0, m.toClassFilename);
                    Resultlist.Items.Insert(0, m.toClassNamespace);
                }

                if (ResultListBox.Items.Count > MaxMsgCount)
                    ResultListBox.Items.RemoveAt(ResultListBox.Items.Count - 1);
            }

            catch (Exception ex)
            {
                Window temp = new Window();
                temp.Content = ex.Message;
                temp.Height = 200;
                temp.Width = 500;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Resultlist.Items.Clear();
                List<string> msg = MessageClient.getProj();
                foreach (string m in msg)
                {
                    Resultlist.Items.Insert(0, m);
                }
                //ProjectSendClick.IsEnabled = true;
                Filebtn.IsEnabled = false;

            }
            catch (Exception ex)
            {
                Window temp = new Window();
                temp.Content = ex.Message;
                temp.Height = 200;
                temp.Width = 500;
            }
        }
    }
}