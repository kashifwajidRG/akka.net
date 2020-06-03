// //-----------------------------------------------------------------------
// // <copyright file="RGRemoteConfigFactory.cs" company="Akka.NET Project">
// //     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
// //     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// // </copyright>
// //-----------------------------------------------------------------------

using System.Diagnostics;
using System.IO;
using System.Reflection;
using Akka.Configuration;
using Akka.Remote;

namespace RGLoggingAndTracing.RGRemote
{
    public class RGRemoteConfigFactory
    {
        public static Config Default()
        {
            return FromResource("Akka.Remote.Configuration.Remote.conf");
        }
        
        internal static Config FromResource(string resourceName)
        {
            var assembly = typeof(RemoteActorRef).GetTypeInfo().Assembly;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                Debug.Assert(stream != null, "stream != null");
                using (var reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();

                    return ConfigurationFactory.ParseString(result);
                }
            }
        }
    }
}