// //-----------------------------------------------------------------------
// // <copyright file="RGRemoteActorRef.cs" company="Akka.NET Project">
// //     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
// //     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// // </copyright>
// //-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Dispatch.SysMsg;
using Akka.Event;
using Akka.Remote;

namespace RGLoggingAndTracing.RGRemote
{
    public class RGRemoteActorRef : RemoteActorRef
    {

        public RGRemoteActorRef(RGRemoteTransport remote, Address localAddressToUse, ActorPath path, IInternalActorRef parent,
            Props props, Deploy deploy)
        : base(remote, localAddressToUse, path, parent, props, deploy)
        {
            
        }
    }
}