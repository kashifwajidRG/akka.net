// //-----------------------------------------------------------------------
// // <copyright file="RGRemotedeploymentWatcher.cs" company="Akka.NET Project">
// //     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
// //     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// // </copyright>
// //-----------------------------------------------------------------------

using System.Collections.Generic;
using Akka.Actor;
using Akka.Dispatch;
using Akka.Dispatch.SysMsg;

namespace RGLoggingAndTracing.RGRemote
{
    public class RGRemoteDeploymentWatcher : ReceiveActor, IRequiresMessageQueue<IUnboundedMessageQueueSemantics>
    {

        private readonly IDictionary<IActorRef, IInternalActorRef> _supervisors =
            new Dictionary<IActorRef, IInternalActorRef>();
        
        public RGRemotedeploymentWatcher()
        {
            Receive<WatchRemote>(w =>
            {
                _supervisors.Add(w.Actor, w.Supervisor);
                Context.Watch(w.Actor);
            });

            Receive<Terminated>(t =>
            {
                if (_supervisors.TryGetValue(t.ActorRef, out var supervisor))
                {
                    // send extra DeathWatchNotification to the supervisor so that it will remove the child
                    supervisor.SendSystemMessage(new DeathWatchNotification(t.ActorRef, t.ExistenceConfirmed,
                        t.AddressTerminated));
                    _supervisors.Remove(t.ActorRef);
                }
            });
        }
        
        internal class WatchRemote
        {
            /// <summary>
            /// TBD
            /// </summary>
            /// <param name="actor">TBD</param>
            /// <param name="supervisor">TBD</param>
            public WatchRemote(IActorRef actor, IInternalActorRef supervisor)
            {
                Actor = actor;
                Supervisor = supervisor;
            }

            /// <summary>
            /// TBD
            /// </summary>
            public IActorRef Actor { get; private set; }
            /// <summary>
            /// TBD
            /// </summary>
            public IInternalActorRef Supervisor { get; private set; }
        }
    }
}