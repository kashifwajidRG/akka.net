// //-----------------------------------------------------------------------
// // <copyright file="RGRepointableActorRef.cs" company="Akka.NET Project">
// //     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
// //     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// // </copyright>
// //-----------------------------------------------------------------------

using Akka.Actor;
using Akka.Actor.Internal;
using Akka.Dispatch;

namespace RGLoggingAndTracing.Actor
{
    public class RGRepointableActorRef : RepointableActorRef
    {
        protected readonly MailboxType MailboxType;
        
        public RGRepointableActorRef(ActorSystemImpl system, Props props, MessageDispatcher dispatcher, MailboxType mailboxType, IInternalActorRef supervisor, ActorPath path)
        : base(system, props, dispatcher, mailboxType, supervisor, path)
        {
            MailboxType = mailboxType;
        }
        
        protected override ActorCell NewCell()
        {
            var actorCell = new RGActorCell(System, this, Props, Dispatcher, Supervisor);
            actorCell.Init(false, MailboxType);
            return actorCell;
        }
    }
}