// //-----------------------------------------------------------------------
// // <copyright file="RGLocalActorRef.cs" company="Akka.NET Project">
// //     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
// //     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// // </copyright>
// //-----------------------------------------------------------------------

using System;
using Akka.Actor;
using Akka.Actor.Internal;
using Akka.Dispatch;
using RGLoggingAndTracing.RGActor;

namespace RGLoggingAndTracing.RGLocal
{
    public class RGLocalActorRef : LocalActorRef
    {
        public RGLocalActorRef(ActorSystemImpl systemImpl, Props props, MessageDispatcher dispatcher,
            MailboxType mailboxType, IInternalActorRef supervisor, ActorPath path)
            : base(systemImpl, props, dispatcher, mailboxType, supervisor, path)
        {
            
        }

        protected override ActorCell NewActorCell(ActorSystemImpl systemImpl, IInternalActorRef self, Props props,
            MessageDispatcher dispatcher, IInternalActorRef supervisor)
        {
            Console.WriteLine("RGLocalActorRef : NewActorCell" + self.Path);
            return new RGActorCell(systemImpl, self, props, dispatcher, supervisor);
        }
    }
}