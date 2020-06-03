// //-----------------------------------------------------------------------
// // <copyright file="RGRemoteDeployer.cs" company="Akka.NET Project">
// //     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
// //     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// // </copyright>
// //-----------------------------------------------------------------------

using System.Linq;
using Akka.Actor;
using Akka.Configuration;
using Akka.Remote.Routing;
using Akka.Routing;
using Akka.Util.Internal;

namespace RGLoggingAndTracing.RGRemote
{
    public class RGRemoteDeployer : Deployer
    {
       
        public RGRemoteDeployer(Settings settings) : base(settings)
        {
        }
        
        public override Deploy ParseConfig(string key, Config config)
        {
            var deploy = base.ParseConfig(key, config);
            if (deploy == null) return null;

            var remote = deploy.Config.GetString("remote", null);

            ActorPath actorPath;
            if(ActorPath.TryParse(remote, out actorPath))
            {
                var address = actorPath.Address;
                //can have remotely deployed routers that remotely deploy routees
                return CheckRemoteRouterConfig(deploy.WithScope(scope: new RemoteScope(address)));
            }
            
            if (!string.IsNullOrWhiteSpace(remote))
                throw new ConfigurationException($"unparseable remote node name [{remote}]");

            return CheckRemoteRouterConfig(deploy);
        }

        private static Deploy CheckRemoteRouterConfig(Deploy deploy)
        {
            var nodes = deploy.Config.GetStringList("target.nodes", new string[] { }).Select(Address.Parse).ToList();
            if (nodes.Any() && deploy.RouterConfig != null)
            {
                if (deploy.RouterConfig is Pool)
                    return
                        deploy.WithRouterConfig(new RemoteRouterConfig(deploy.RouterConfig.AsInstanceOf<Pool>(), nodes));
                return deploy.WithScope(scope: Deploy.NoScopeGiven);
            }
            else
            {
                //TODO: return deploy;
                return deploy;
            }
        }
    }
}