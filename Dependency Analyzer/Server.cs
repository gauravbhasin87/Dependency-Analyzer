///////////////////////////////////////////////////////////////////////////////
// Server.cs - Document Vault Server prototype                               //
//                                                                           //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2013           //
///////////////////////////////////////////////////////////////////////////////
/*
 *  Package Contents:
 *  -----------------
 *  This package defines four classes:
 *  Server:
 *    Provides prototype behavior for the DocumentVault server.
 *  EchoCommunicator:
 *    Simply diplays its messages on the server Console.
 *  QueryCommunicator:
 *    Serves as a placeholder for query processing.  You should be able to
 *    invoke your Project #2 query processing from the ProcessMessages function.
 *  NavigationCommunicator:
 *    Serves as a placeholder for navigation processing.  You should be able to
 *    invoke your navigation processing from the ProcessMessages function.
 * 
 *  Required Files:
 *  - Server:      Server.cs, Sender.cs, Receiver.cs
 *  - Components:  ICommLib, AbstractCommunicator, BlockingQueue
 *  - CommService: ICommService, CommService
 *
 *  Required References:
 *  - System.ServiceModel
 *  - System.RuntimeSerialization
 *
 *  Build Command:  devenv Project4HelpF13.sln /rebuild debug
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

namespace DocumentVault
{
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
    protected override void ProcessMessages()
    {
      while (true)
      {
        ServiceMessage msg = bq.deQ();
        Console.Write("\n  {0} Recieved Message:\n", msg.TargetCommunicator);
        msg.ShowMessage();
        Console.Write("\n  Navigation processing is an exercise for students\n");
        if (msg.Contents == "quit")
          break;
        ServiceMessage reply = ServiceMessage.MakeMessage("client-echo", "nav", "reply from nav");
        reply.TargetUrl = msg.SourceUrl;
        reply.SourceUrl = msg.TargetUrl;
        AbstractMessageDispatcher dispatcher = AbstractMessageDispatcher.GetInstance();
        dispatcher.PostMessage(reply);
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

      string ClientUrl = "http://localhost:8001/CommService";

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

      Console.Write("\n  Started CommService - Press key to exit:\n ");
      Console.ReadKey();
    }
  }
}
