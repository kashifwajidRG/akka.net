// //-----------------------------------------------------------------------
// // <copyright file="RGActorCell.cs" company="Akka.NET Project">
// //     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
// //     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// // </copyright>
// //-----------------------------------------------------------------------

using System;
using Akka.Actor;
using Akka.Actor.Internal;
using Akka.Dispatch;

namespace ChatClient.RGLoggingAndTracing
{
    public class RGActorCell : ActorCell
    {
        public RGActorCell(ActorSystemImpl systemImpl, IInternalActorRef self, Props props,
            MessageDispatcher dispatcher, IInternalActorRef parent)
            : base(systemImpl, self, props, dispatcher, parent)
        {
            
        }

        public void init(bool sendSupervise, MailboxType mailboxType)
        {
            base.Init(sendSupervise, mailboxType);
        }

        public override void SendMessage(Envelope message)
        {
            
            Console.WriteLine("RGActorCell : SendMessage : Message is: ", message.ToString());
            base.SendMessage(message);
        }

        public override void SendMessage(IActorRef sender, object message)
        {
            base.SendMessage(sender, message);
        }

        protected override void ReceiveMessage(object message)
        {
            //Start segment
            Console.WriteLine("RGActorCell : RecieveMessage : Message is: ", message.ToString());
            base.ReceiveMessage(message);
            //End segment
        }
    }
}