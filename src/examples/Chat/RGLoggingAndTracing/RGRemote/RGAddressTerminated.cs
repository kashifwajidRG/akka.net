// //-----------------------------------------------------------------------
// // <copyright file="RGAddressTerminated.cs" company="Akka.NET Project">
// //     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
// //     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// // </copyright>
// //-----------------------------------------------------------------------

using Akka.Actor;
using Akka.Event;

namespace RGLoggingAndTracing.RGRemote
{
    public class RGAddressTerminated : IAutoReceivedMessage, IPossiblyHarmful, IDeadLetterSuppression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RGAddressTerminated" /> class.
        /// </summary>
        /// <param name="address">TBD</param>
        public RGAddressTerminated(Address address)
        {
            Address = address;
        }

        /// <summary>
        /// TBD
        /// </summary>
        public Address Address { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"<RGAddressTerminated>: {Address}";
        }
    }
}