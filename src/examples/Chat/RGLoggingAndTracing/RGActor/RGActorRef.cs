// //-----------------------------------------------------------------------
// // <copyright file="RGActorRef.cs" company="Akka.NET Project">
// //     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
// //     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// // </copyright>
// //-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;

namespace RGLoggingAndTracing.RGActor
{
    public class RGVirtualPathContainer : MinimalActorRef
    {
        private readonly IInternalActorRef _parent;
        private readonly ILoggingAdapter _log;
        private readonly IActorRefProvider _provider;
        private readonly ActorPath _path;

        private readonly ConcurrentDictionary<string, IInternalActorRef> _children = new ConcurrentDictionary<string, IInternalActorRef>();
        
        public RGVirtualPathContainer(IActorRefProvider provider, ActorPath path, IInternalActorRef parent, ILoggingAdapter log)
        {
            _parent = parent;
            _log = log;
            _provider = provider;
            _path = path;
        }
        
        public override IActorRefProvider Provider
        {
            get { return _provider; }
        }
        
        public override IInternalActorRef Parent
        {
            get { return _parent; }
        }
        
        public override ActorPath Path
        {
            get { return _path; }
        }
        
        public ILoggingAdapter Log
        {
            get { return _log; }
        }
        
        protected bool TryGetChild(string name, out IInternalActorRef child)
        {
            return _children.TryGetValue(name, out child);
        }
        
        public void AddChild(string name, IInternalActorRef actor)
        {
            _children.AddOrUpdate(name, actor, (k, v) =>
            {
                Log.Warning("{0} replacing child {1} ({2} -> {3})", name, actor, v, actor);
                return v;
            });
        }
        
        public void RemoveChild(string name)
        {
            IInternalActorRef tmp;
            if (!_children.TryRemove(name, out tmp))
            {
                Log.Warning("{0} trying to remove non-child {1}", Path, name);
            }
        }
        
        public void RemoveChild(string name, IActorRef child)
        {
            IInternalActorRef tmp;
            if (!_children.TryRemove(name, out tmp))
            {
                Log.Warning("{0} trying to remove non-child {1}", Path, name);
            }
        }

        public override IActorRef GetChild(IEnumerable<string> name)
        {
            //Using enumerator to avoid multiple enumerations of name.
            var enumerator = name.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                //name was empty
                return this;
            }
            var firstName = enumerator.Current;
            if (string.IsNullOrEmpty(firstName))
                return this;
            if (_children.TryGetValue(firstName, out var child))
                return child.GetChild(new Enumerable<string>(enumerator));
            return ActorRefs.Nobody;
        }

        public bool HasChildren
        {
            get
            {
                return !_children.IsEmpty;
            }
        }
        
        public void ForEachChild(Action<IInternalActorRef> action)
        {
            foreach (IInternalActorRef child in _children.Values)
            {
                action(child);
            }
        }
        
        private class Enumerable<T> : IEnumerable<T>
        {
            private readonly IEnumerator<T> _enumerator;

            public Enumerable(IEnumerator<T> enumerator)
            {
                _enumerator = enumerator;
            }

            /// <inheritdoc/>
            public IEnumerator<T> GetEnumerator()
            {
                return _enumerator;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}