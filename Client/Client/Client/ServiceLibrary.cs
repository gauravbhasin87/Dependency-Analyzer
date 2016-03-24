///////////////////////////////////////////////////////////////////////
// MT2Q2-ServiceLibrary.cs - Project #4 Service prototype            //
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
using System.Runtime.Serialization;
using System.ServiceModel.Web;
using System.Xml.Linq;

namespace CodeAnalysis
{
    [DataContract(Namespace = "CodeAnalysis")]
    public class SvcMsg
    {
        public enum Command { Dependency, ProjectList, Request, Response };  
        [DataMember]
        public Command cmd;
        [DataMember]
        public Uri src;
        [DataMember]
        public Uri dst;
        [DataMember]
        public string body;  // Used to send XML for structured data
        [DataMember]
        public string bodyExt;  // Used to send XML for structured data
    }

    [ServiceContract(Namespace = "CodeAnalysis")]
    public interface IMessageService
    {
        [OperationContract(IsOneWay = true)]
        void PostMessage(SvcMsg msg);
    }

    [ServiceBehavior(Namespace = "CodeAnalysis")]
    public class MessageService : IMessageService
    {
        public void PostMessage(SvcMsg msg)
        {
            MessageClient c = new MessageClient();
            c.ShowMessage(msg);
        }
    }
}