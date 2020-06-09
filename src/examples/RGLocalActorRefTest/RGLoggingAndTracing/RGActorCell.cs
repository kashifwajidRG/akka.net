// //-----------------------------------------------------------------------
// // <copyright file="RGActorCell.cs" company="Akka.NET Project">
// //     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
// //     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// // </copyright>
// //-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using Akka.Actor;
using Akka.Actor.Internal;
using Akka.Dispatch;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Internal.Entities;
using RGLocalActorRefTest;

namespace RGLoggingAndTracing.Actor
{

    public struct RGMessage
    {
        public RGMessage(object message, string traceID, string parentSegmentID)
        {
            Message = message;
            TraceID = traceID;
            ParentSegmentID = parentSegmentID;
        }
        
        public string TraceID { get; set; }
        public string ParentSegmentID { get; set; }
        public object Message { get; set; }
        
        public override string ToString()
        {
            return "<" + (Message ?? "null") + "> with TraceID " + TraceID + " and ParentSegment " + ParentSegmentID;
        }
    }
    public class RGActorCell : ActorCell
    {
        [ThreadStatic] static string _traceID = "";
        [ThreadStatic] static string _parentSegmentID = "";
        public RGActorCell(ActorSystemImpl systemImpl, IInternalActorRef self, Props props,
            MessageDispatcher dispatcher, IInternalActorRef parent)
            : base(systemImpl, self, props, dispatcher, parent)
        {
            
        }
        public override void SendMessage(IActorRef sender, object message)
        {
             RGMessage toSend = new RGMessage();
             toSend.Message = message;
             toSend.TraceID = String.IsNullOrEmpty(_traceID) ? "" : _traceID;
             toSend.ParentSegmentID = String.IsNullOrEmpty(_parentSegmentID) ? "" : _parentSegmentID;
             Console.WriteLine("RGActorCell : SendMessage : Message is: {0}", toSend.ToString());
             base.SendMessage(sender, toSend);
        }

        protected override void ReceiveMessage(object message)
        {
            //Start segment
            if (message is RGMessage)
            {
                RGMessage msg = (RGMessage)message;
                string traceId = null, parentSegmentId = null;
                if (String.IsNullOrEmpty(msg.TraceID))
                {
                    traceId = TraceId.NewId();
                }
                else
                {
                    traceId = msg.TraceID;
                }

                if (!String.IsNullOrEmpty(msg.ParentSegmentID))
                {
                    parentSegmentId = msg.ParentSegmentID;
                }
                else
                {
                    parentSegmentId = null;
                }

                if(String.IsNullOrEmpty(parentSegmentId))
                    AWSXRayRecorder.Instance.BeginSegment(this.Props.Type.Name + "-" + msg.Message.GetType().Name, traceId);
                else
                {
                    AWSXRayRecorder.Instance.BeginSegment(Props.Type.Name + "-" + msg.Message.GetType().Name, traceId, parentSegmentId);
                }
                _traceID = AWSXRayRecorder.Instance.GetEntity().TraceId;
                _parentSegmentID = AWSXRayRecorder.Instance.GetEntity().Id;
                Console.WriteLine("RGActorCell : RecieveMessage : Message is: ", msg.ToString());
                
                try
                {
                    base.ReceiveMessage(msg.Message);
                }
                finally
                {
                    AWSXRayRecorder.Instance.EndSegment();
                    _traceID = null;
                    _parentSegmentID = null;
                }
                //End segment
            }
            else
            {
                Console.WriteLine("RGActorCell : RecieveMessage : Message is: ", message.ToString());
                base.ReceiveMessage(message);
            }
        }
    }
}