// //-----------------------------------------------------------------------
// // <copyright file="RGActorRefResolveThreadLocalCache.cs" company="Akka.NET Project">
// //     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
// //     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// // </copyright>
// //-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using Akka.Actor;
using Akka.Remote;

namespace RGLoggingAndTracing.RGRemote
{
    public sealed class RGActorRefResolveThreadLocalCache : ExtensionIdProvider<RGActorRefResolveThreadLocalCache>, IExtension
    {
        private readonly IRemoteActorRefProvider _provider;

        public RGActorRefResolveThreadLocalCache() { }

        public RGActorRefResolveThreadLocalCache(IRemoteActorRefProvider provider)
        {
            _provider = provider;
            _current = new ThreadLocal<RGActorRefResolveCache>(() => new RGActorRefResolveCache(_provider));
        }

        public override RGActorRefResolveThreadLocalCache CreateExtension(ExtendedActorSystem system)
        {
            return new RGActorRefResolveThreadLocalCache((IRemoteActorRefProvider)system.Provider);
        }

        private readonly ThreadLocal<RGActorRefResolveCache> _current;

        public RGActorRefResolveCache Cache => _current.Value;

        public static RGActorRefResolveThreadLocalCache For(ActorSystem system)
        {
            return system.WithExtension<RGActorRefResolveThreadLocalCache, RGActorRefResolveThreadLocalCache>();
        }
    }
    
    public sealed class RGActorRefResolveCache : RGLruBoundedCache<string, IActorRef>
    {
        private readonly IRemoteActorRefProvider _provider;

        public RGActorRefResolveCache(IRemoteActorRefProvider provider, int capacity = 1024, int evictAgeThreshold = 600) : base(capacity, evictAgeThreshold)
        {
            _provider = provider;
        }

        protected override IActorRef Compute(string k)
        {
            return _provider.InternalResolveActorRef(k);
        }

        protected override int Hash(string k)
        {
            return RGFastHash.OfStringFast(k);
        }

        protected override bool IsCacheable(IActorRef v)
        {
            return !(v is EmptyLocalActorRef);
        }
    }
    
}

