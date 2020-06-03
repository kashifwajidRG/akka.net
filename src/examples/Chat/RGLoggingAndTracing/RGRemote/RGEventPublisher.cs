// //-----------------------------------------------------------------------
// // <copyright file="RGEventPublisher.cs" company="Akka.NET Project">
// //     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
// //     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// // </copyright>
// //-----------------------------------------------------------------------

using Akka.Actor;
using Akka.Event;
using Akka.Remote;

namespace RGLoggingAndTracing.RGRemote
{
    public class RGEventPublisher
    {
        public ActorSystem System { get; private set; }
        
        public ILoggingAdapter Log { get; private set; }

        public readonly LogLevel LogLevel;
        
        public RGEventPublisher(ActorSystem system, ILoggingAdapter log, LogLevel logLevel)
        {
            System = system;
            Log = log;
            LogLevel = logLevel;
        }
        
        public void NotifyListeners(RemotingLifecycleEvent message)
        {
            System.EventStream.Publish(message);
            if (message.LogLevel() >= LogLevel) Log.Log(message.LogLevel(), message.ToString());
        }
    }
}