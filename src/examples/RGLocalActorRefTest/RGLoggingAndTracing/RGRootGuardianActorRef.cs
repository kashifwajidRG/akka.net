// //-----------------------------------------------------------------------
// // <copyright file="RGRootGuardianActorRef.cs" company="Akka.NET Project">
// //     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
// //     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// // </copyright>
// //-----------------------------------------------------------------------

using System.Collections.Generic;
using Akka.Actor;
using Akka.Actor.Internal;
using Akka.Dispatch;

namespace RGLoggingAndTracing.Actor
{
    public class RGRootGuardianActorRef : RootGuardianActorRef
    {
        public RGRootGuardianActorRef(ActorSystemImpl system, Props props, MessageDispatcher dispatcher, MailboxType mailboxType, 
            IInternalActorRef supervisor, ActorPath path, IInternalActorRef deadLetters, IReadOnlyDictionary<string, IInternalActorRef> extraNames)
            : base(system,props,dispatcher,mailboxType,supervisor,path, deadLetters, extraNames)
        {
        }
    }
}