// //-----------------------------------------------------------------------
// // <copyright file="RGAddressTerminatedTopic.cs" company="Akka.NET Project">
// //     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
// //     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// // </copyright>
// //-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Akka.Actor;

namespace RGLoggingAndTracing.RGRemote
{
    /// <summary>
    /// This class represents an <see cref="ActorSystem"/> provider used to create the <see cref="RGAddressTerminatedTopic"/> extension.
    /// </summary>
    public sealed class RGAddressTerminatedTopicProvider : ExtensionIdProvider<RGAddressTerminatedTopic>
    {
        public override RGAddressTerminatedTopic CreateExtension(ExtendedActorSystem system)
        {
            return new RGAddressTerminatedTopic();
        }
    }

    /// <summary>
    /// This class represents an <see cref="ActorSystem"/> extension used by remote and cluster death watchers
    /// to publish <see cref="RGAddressTerminated"/> notifications when a remote system is deemed dead.
    /// 
    /// <remarks>Note! Part of internal API. Breaking changes may occur without notice. Use at own risk.</remarks>
    /// </summary>
    public sealed class RGAddressTerminatedTopic : IExtension
    {
        private readonly HashSet<IActorRef> _subscribers = new HashSet<IActorRef>();

        /// <summary>
        /// Retrieves the extension from the specified actor system.
        /// </summary>
        /// <param name="system">The actor system from which to retrieve the extension.</param>
        /// <returns>The extension retrieved from the given actor system.</returns>
        public static RGAddressTerminatedTopic Get(ActorSystem system)
        {
            return system.WithExtension<RGAddressTerminatedTopic>(typeof(RGAddressTerminatedTopicProvider));
        }

        /// <summary>
        /// Registers the specified actor to receive <see cref="AddressTerminated"/> notifications.
        /// </summary>
        /// <param name="subscriber">The actor that is registering for notifications.</param>
        public void Subscribe(IActorRef subscriber)
        {
            lock (_subscribers)
                _subscribers.Add(subscriber);
        }

        /// <summary>
        /// Unregisters the specified actor from receiving <see cref="AddressTerminated"/> notifications.
        /// </summary>
        /// <param name="subscriber">The actor that is unregistering for notifications.</param>
        public void Unsubscribe(IActorRef subscriber)
        {
            lock (_subscribers)
                _subscribers.Remove(subscriber);
        }

        /// <summary>
        /// Sends alls registered subscribers an <see cref="AddressTerminated"/> notification.
        /// </summary>
        /// <param name="msg">The message that is sent to all subscribers.</param>
        public void Publish(RGAddressTerminated msg)
        {
            List<IActorRef> subscribers;
            lock(_subscribers)
                subscribers = _subscribers.ToList();

            foreach (var subscriber in subscribers)
            {
                subscriber.Tell(msg, ActorRefs.NoSender);
            }
        }
    }
}