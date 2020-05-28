// //-----------------------------------------------------------------------
// // <copyright file="RGLocalActorRefProvider.cs" company="Akka.NET Project">
// //     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
// //     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// // </copyright>
// //-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Internal;
using Akka.Configuration;
using Akka.Dispatch;
using Akka.Dispatch.SysMsg;
using Akka.Event;
using Akka.Routing;
using Akka.Serialization;
using Akka.Util;
using Akka.Util.Internal;

namespace ChatClient.RGLoggingAndTracing
{
    //Cannot directly inherit from the sealed class LocalActoRefProvider.
    //Therefore need to make a copy
    public class RGLocalActorRefProvider : IActorRefProvider
    {
        private readonly Settings _settings;
        private readonly EventStream _eventStream;
        private readonly Deployer _deployer;
        private readonly IInternalActorRef _deadLetters;
        private readonly RootActorPath _rootPath;
        private readonly ILoggingAdapter _log;
        private readonly AtomicCounterLong _tempNumber;
        private readonly ActorPath _tempNode;
        private ActorSystemImpl _system;
        private readonly Dictionary<string, IInternalActorRef> _extraNames = new Dictionary<string, IInternalActorRef>();
        private readonly TaskCompletionSource<Status> _terminationPromise = new TaskCompletionSource<Status>();
        private readonly SupervisorStrategy _systemGuardianStrategy;
        private readonly SupervisorStrategyConfigurator _userGuardianStrategyConfigurator;
        private RGVirtualPathContainer _tempContainer;
        private RootGuardianActorRef _rootGuardian;
        private RGLocalActorRef _userGuardian;    //This is called guardian in Akka
        private MailboxType _defaultMailbox;
        private RGLocalActorRef _systemGuardian;
        
        public RGLocalActorRefProvider(string systemName, Settings settings, EventStream eventStream)
            : this(systemName, settings, eventStream, null, null)
        {
            //Intentionally left blank as to mimic the LocalActorRefProvider Class
            
        }

        public RGLocalActorRefProvider(string systemName, Settings settings, EventStream eventStream, Deployer deployer, Func<ActorPath, IInternalActorRef> deadLettersFactory)
        {
            _settings = settings;
            _eventStream = eventStream;
            _deployer = deployer ?? new Deployer(settings);
            _rootPath = new RootActorPath(new Address("akka", systemName));
            _log = Logging.GetLogger(eventStream, "RGLocalActorRefProvider(" + _rootPath.Address + ")");
            if (deadLettersFactory == null)
                deadLettersFactory = p => new DeadLetterActorRef(this, p, _eventStream);
            _deadLetters = deadLettersFactory(_rootPath / "deadLetters");
            _tempNumber = new AtomicCounterLong(1);
            _tempNode = _rootPath / "temp";

            _systemGuardianStrategy = SupervisorStrategy.DefaultStrategy;
            _userGuardianStrategyConfigurator = SupervisorStrategyConfigurator.CreateConfigurator(Settings.SupervisorStrategyClass);
        }

        public IActorRef DeadLetters { get { return _deadLetters; } }

        public Deployer Deployer { get { return _deployer; } }

        public IInternalActorRef RootGuardian { get { return _rootGuardian; } }

        public ActorPath RootPath { get { return _rootPath; } }

        public Settings Settings { get { return _settings; } }

        public LocalActorRef SystemGuardian { get { return _systemGuardian; } }

        public IInternalActorRef TempContainer { get { return _tempContainer; } }

        public Task TerminationTask { get { return _terminationPromise.Task; } }

        public LocalActorRef Guardian { get { return _userGuardian; } }
        
        public EventStream EventStream { get { return _eventStream; } }
        
        public ILoggingAdapter Log { get { return _log; } }
        
        public Address DefaultAddress { get { return _rootPath.Address; } }

        private Information _serializationInformationCache;


        private MessageDispatcher DefaultDispatcher { get { return _system.Dispatchers.DefaultGlobalDispatcher; } }

        private SupervisorStrategy UserGuardianSupervisorStrategy { get { return _userGuardianStrategyConfigurator.Create(); } }
        
        public ActorPath TempPath()
        {
            return _tempNode / GetNextTempName();
        }

        private string GetNextTempName()
        {
            return _tempNumber.GetAndIncrement().Base64Encode();
        }
        
        public void RegisterExtraName(string name, IInternalActorRef actor)
        {
            _extraNames.Add(name, actor);
        }

        private RootGuardianActorRef CreateRootGuardian(ActorSystemImpl system)
        {
            var supervisor = new RootGuardianSupervisor(_rootPath, this, _terminationPromise, _log);
            var rootGuardianStrategy = new OneForOneStrategy(ex =>
            {
                _log.Error(ex, "Guardian failed. Shutting down system");
                return Directive.Stop;
            });
            var props = Props.Create<GuardianActor>(rootGuardianStrategy);
            var rootGuardian = new RGRootGuardianActorRef(system, props, DefaultDispatcher, _defaultMailbox, supervisor, _rootPath, _deadLetters, _extraNames);
            return rootGuardian;
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="address">TBD</param>
        /// <returns>TBD</returns>
        public IActorRef RootGuardianAt(Address address)
        {
            return address == _rootPath.Address ? _rootGuardian : _deadLetters;
        }

        private RGLocalActorRef CreateUserGuardian(LocalActorRef rootGuardian, string name)   //Corresponds to Akka's: override lazy val guardian: LocalActorRef
        {
            var cell = rootGuardian.Cell;
            cell.ReserveChild(name);
            var props = Props.Create<GuardianActor>(UserGuardianSupervisorStrategy);

            var userGuardian = new RGLocalActorRef(_system, props, DefaultDispatcher, _defaultMailbox, rootGuardian, RootPath / name);
            cell.InitChild(userGuardian);
            userGuardian.Start();
            return userGuardian;
        }

        private RGLocalActorRef CreateSystemGuardian(LocalActorRef rootGuardian, string name, LocalActorRef userGuardian)     //Corresponds to Akka's: override lazy val guardian: systemGuardian
        {
            var cell = rootGuardian.Cell;
            cell.ReserveChild(name);
            var props = Props.Create(() => new SystemGuardianActor(userGuardian), _systemGuardianStrategy);

            var systemGuardian = new RGLocalActorRef(_system, props, DefaultDispatcher, _defaultMailbox, rootGuardian, RootPath / name);
            cell.InitChild(systemGuardian);
            systemGuardian.Start();
            return systemGuardian;
        }
        
        public void RegisterTempActor(IInternalActorRef actorRef, ActorPath path)
        {
            if (path.Parent != _tempNode)
                throw new InvalidOperationException("Cannot RegisterTempActor() with anything not obtained from tempPath()");
            _tempContainer.AddChild(path.Name, actorRef);
        }
        
        public void UnregisterTempActor(ActorPath path)
        {
            if (path.Parent != _tempNode)
                throw new InvalidOperationException("Cannot UnregisterTempActor() with anything not obtained from tempPath()");
            _tempContainer.RemoveChild(path.Name);
        }
        
        public void Init(ActorSystemImpl system)
        {
            _system = system;
            //The following are the lazy val statements in Akka
            var defaultDispatcher = system.Dispatchers.DefaultGlobalDispatcher;
            _defaultMailbox = system.Mailboxes.Lookup(Mailboxes.DefaultMailboxId);
            _rootGuardian = CreateRootGuardian(system);
            _tempContainer = new RGVirtualPathContainer(system.Provider, _tempNode, _rootGuardian, _log);
            _rootGuardian.SetTempContainer(_tempContainer);
            _userGuardian = CreateUserGuardian(_rootGuardian, "user");
            _systemGuardian = CreateSystemGuardian(_rootGuardian, "system", _userGuardian);
            //End of lazy val

            _rootGuardian.Start();
            // chain death watchers so that killing guardian stops the application
            _systemGuardian.SendSystemMessage(new Watch(_userGuardian, _systemGuardian));
            _rootGuardian.SendSystemMessage(new Watch(_systemGuardian, _rootGuardian));
            //_eventStream.StartDefaultLoggers(_system);
        }

        
        public IActorRef ResolveActorRef(string path)
        {
            ActorPath actorPath;
            if (ActorPath.TryParse(path, out actorPath) && actorPath.Address == _rootPath.Address)
                return ResolveActorRef(_rootGuardian, actorPath.Elements);
            _log.Debug("Resolve of unknown path [{0}] failed. Invalid format.", path);
            return _deadLetters;
        }

        
        public IActorRef ResolveActorRef(ActorPath path)
        {
            if (path.Root == _rootPath)
                return ResolveActorRef(_rootGuardian, path.Elements);
            _log.Debug("Resolve of foreign ActorPath [{0}] failed", path);
            return _deadLetters;
        }

        
        internal IInternalActorRef ResolveActorRef(IInternalActorRef actorRef, IReadOnlyCollection<string> pathElements)
        {
            if (pathElements.Count == 0)
            {
                _log.Debug("Resolve of empty path sequence fails (per definition)");
                return _deadLetters;
            }
            var child = actorRef.GetChild(pathElements);
            if (child.IsNobody())
            {
                _log.Debug("Resolve of path sequence [/{0}] failed", ActorPath.FormatPathElements(pathElements));
                return new EmptyLocalActorRef(_system.Provider, actorRef.Path / pathElements, _eventStream);
            }
            return (IInternalActorRef)child;
        }

        /// <summary>
        /// Actor factory with create-only semantics: will create an actor as
        /// described by <paramref name="props" /> with the given <paramref name="supervisor" /> and <paramref name="path" /> (may be different
        /// in case of remote supervision). If <paramref name="systemService" /> is true, deployment is
        /// bypassed (local-only). If a value for<paramref name="deploy" /> is passed in, it should be
        /// regarded as taking precedence over the nominally applicable settings,
        /// but it should be overridable from external configuration; the lookup of
        /// the latter can be suppressed by setting "lookupDeploy" to "false".
        /// </summary>
        /// <param name="system">TBD</param>
        /// <param name="props">TBD</param>
        /// <param name="supervisor">TBD</param>
        /// <param name="path">TBD</param>
        /// <param name="systemService">TBD</param>
        /// <param name="deploy">TBD</param>
        /// <param name="lookupDeploy">TBD</param>
        /// <param name="async">TBD</param>
        /// <exception cref="ConfigurationException">
        /// This exception can be thrown for a number of reasons. The following are some examples:
        /// <dl>
        /// <dt><b>non-routers</b></dt>
        /// <dd>The dispatcher in the given <paramref name="props"/> is not configured for the given <paramref name="path"/>.</dd>
        /// <dd>or</dd>
        /// <dd>There was a configuration problem while creating the given <paramref name="path"/> with the dispatcher and mailbox from the given <paramref name="props"/></dd>
        /// <dt><b>routers</b></dt>
        /// <dd>The dispatcher in the given <paramref name="props"/> is not configured for routees of the given <paramref name="path"/></dd>
        /// <dd>or</dd>
        /// <dd>The dispatcher in the given <paramref name="props"/> is not configured for router of the given <paramref name="path"/></dd>
        /// <dd>or</dd>
        /// <dd>$There was a configuration problem while creating the given <paramref name="path"/> with router dispatcher and mailbox and routee dispatcher and mailbox.</dd>
        /// </dl>
        /// </exception>
        /// <returns>TBD</returns>
        public IInternalActorRef ActorOf(ActorSystemImpl system, Props props, IInternalActorRef supervisor, ActorPath path, bool systemService, Deploy deploy, bool lookupDeploy, bool async)
        {
            // if (props.Deploy.RouterConfig is NoRouter)
            // {
                if (Settings.DebugRouterMisconfiguration)
                {
                    var d = Deployer.Lookup(path);
                    if (d != null && !(d.RouterConfig is NoRouter))
                        Log.Warning("Configuration says that [{0}] should be a router, but code disagrees. Remove the config or add a RouterConfig to its Props.",
                                    path);
                }

                var props2 = props;

                // mailbox and dispatcher defined in deploy should override props
                var propsDeploy = lookupDeploy ? Deployer.Lookup(path) : deploy;
                if (propsDeploy != null)
                {
                    if (propsDeploy.Mailbox != Deploy.NoMailboxGiven)
                        props2 = props2.WithMailbox(propsDeploy.Mailbox);
                    if (propsDeploy.Dispatcher != Deploy.NoDispatcherGiven)
                        props2 = props2.WithDispatcher(propsDeploy.Dispatcher);
                }

                if (!system.Dispatchers.HasDispatcher(props2.Dispatcher))
                {
                    throw new ConfigurationException($"Dispatcher [{props2.Dispatcher}] not configured for path {path}");
                }

                try
                {
                    // for consistency we check configuration of dispatcher and mailbox locally
                    var dispatcher = _system.Dispatchers.Lookup(props2.Dispatcher);
                    var mailboxType = _system.Mailboxes.GetMailboxType(props2, dispatcher.Configurator.Config);

                    if (async)
                        return
                            new RepointableActorRef(system, props2, dispatcher,
                                mailboxType, supervisor,
                                path).Initialize(async);
                    return new LocalActorRef(system, props2, dispatcher,
                        mailboxType, supervisor, path);
                }
                catch (Exception ex)
                {
                    throw new ConfigurationException(
                        $"Configuration problem while creating [{path}] with dispatcher [{props.Dispatcher}] and mailbox [{props.Mailbox}]", ex);
                }
            //}
            // else //routers!!!
            // {
            //     var lookup = (lookupDeploy ? Deployer.Lookup(path) : null) ?? Deploy.None;
            //     var fromProps = new List<Deploy>() { props.Deploy, deploy, lookup };
            //     var d = fromProps.Where(x => x != null).Aggregate((deploy1, deploy2) => deploy2.WithFallback(deploy1));
            //     var p = props.WithRouter(d.RouterConfig);
            //
            //
            //     if (!system.Dispatchers.HasDispatcher(p.Dispatcher))
            //         throw new ConfigurationException($"Dispatcher [{p.Dispatcher}] not configured for routees of path [{path}]");
            //     if (!system.Dispatchers.HasDispatcher(d.RouterConfig.RouterDispatcher))
            //         throw new ConfigurationException($"Dispatcher [{p.RouterConfig.RouterDispatcher}] not configured for router of path [{path}]");
            //
            //     var routerProps = Props.Empty.WithRouter(p.Deploy.RouterConfig).WithDispatcher(p.RouterConfig.RouterDispatcher);
            //     var routeeProps = props.WithRouter(NoRouter.Instance);
            //
            //     try
            //     {
            //         var routerDispatcher = system.Dispatchers.Lookup(p.RouterConfig.RouterDispatcher);
            //         var routerMailbox = system.Mailboxes.GetMailboxType(routerProps, routerDispatcher.Configurator.Config);
            //
            //         // routers use context.actorOf() to create the routees, which does not allow us to pass
            //         // these through, but obtain them here for early verification
            //         var routeeDispatcher = system.Dispatchers.Lookup(p.Dispatcher);
            //
            //         var routedActorRef = new RoutedActorRef(system, routerProps, routerDispatcher, routerMailbox, routeeProps,
            //             supervisor, path);
            //         routedActorRef.Initialize(async);
            //         return routedActorRef;
            //     }
            //     catch (Exception ex)
            //     {
            //         throw new ConfigurationException(
            //             $"Configuration problem while creating [{path}] with router dispatcher [{routerProps.Dispatcher}] and mailbox [{routerProps.Mailbox}] and routee dispatcher [{routeeProps.Dispatcher}] and mailbox [{routeeProps.Mailbox}].", ex);
            //     }
            // }
        }

        
        public Address GetExternalAddressFor(Address address)
        {
            return address == _rootPath.Address ? address : null;
        }
        public Information SerializationInformation
        {
            get
            {
                if (_serializationInformationCache != null)
                    return _serializationInformationCache;

                if (_system == null)
                    throw new InvalidOperationException("Too early access of SerializationInformation");

                var info = new Information(_rootPath.Address, _system);
                _serializationInformationCache = info;
                return info;
            }
        } 
    }
}