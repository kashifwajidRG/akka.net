// //-----------------------------------------------------------------------
// // <copyright file="RGActorPathCache.cs" company="Akka.NET Project">
// //     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
// //     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// // </copyright>
// //-----------------------------------------------------------------------

using System.Threading;
using Akka.Actor;

namespace RGLoggingAndTracing.RGRemote
{
    public sealed class RGActorPathThreadLocalCache : ExtensionIdProvider<RGActorPathThreadLocalCache>, IExtension
    {
        private readonly ThreadLocal<RGActorPathCache> _current = new ThreadLocal<RGActorPathCache>(() => new RGActorPathCache());

        public RGActorPathCache Cache => _current.Value;

        public override RGActorPathThreadLocalCache CreateExtension(ExtendedActorSystem system)
        {
            return new RGActorPathThreadLocalCache();
        }

        public static RGActorPathThreadLocalCache For(ActorSystem system)
        {
            return system.WithExtension<RGActorPathThreadLocalCache, RGActorPathThreadLocalCache>();
        }
    }
    
    public sealed class RGActorPathCache : RGLruBoundedCache<string, ActorPath>
    {
        public RGActorPathCache(int capacity = 1024, int evictAgeThreshold = 600) : base(capacity, evictAgeThreshold)
        {
        }

        protected override int Hash(string k)
        {
            return RGFastHash.OfStringFast(k);
        }

        protected override ActorPath Compute(string k)
        {
            if (ActorPath.TryParse(k, out var actorPath))
                return actorPath;
            return null;
        }

        protected override bool IsCacheable(ActorPath v)
        {
            return v != null;
        }
    }
}