// //-----------------------------------------------------------------------
// // <copyright file="RGEndpoint.cs" company="Akka.NET Project">
// //     Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
// //     Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
// // </copyright>
// //-----------------------------------------------------------------------

using System;
using Akka.Actor;
using Akka.Remote.Transport;

namespace RGLoggingAndTracing.RGRemote
{
    public class RGEndpointException : AkkaException
    {
        public RGEndpointException(string message, Exception cause = null) : base(message, cause) { }

#if SERIALIZATION
        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected RGEndpointException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
    
    public interface IRGAssociationProblem { }

    /// <summary>
    /// INTERNAL API
    /// </summary>
    public sealed class RGShutDownAssociation : RGEndpointException, IRGAssociationProblem
    {
        public RGShutDownAssociation(string message, Address localAddress, Address remoteAddress, Exception cause = null)
            : base(message, cause)
        {
            RemoteAddress = remoteAddress;
            LocalAddress = localAddress;
        }

        /// <summary>
        /// TBD
        /// </summary>
        public Address LocalAddress { get; private set; }

        /// <summary>
        /// TBD
        /// </summary>
        public Address RemoteAddress { get; private set; }
    }

    /// <summary>
    /// TBD
    /// </summary>
    public sealed class RGInvalidAssociation : RGEndpointException, IRGAssociationProblem
    {
        public RGInvalidAssociation(string message, Address localAddress, Address remoteAddress, Exception cause = null, DisassociateInfo? disassociateInfo = null)
            : base(message, cause)
        {
            RemoteAddress = remoteAddress;
            LocalAddress = localAddress;
            DisassociationInfo = disassociateInfo;
        }

        /// <summary>
        /// TBD
        /// </summary>
        public Address LocalAddress { get; private set; }

        /// <summary>
        /// TBD
        /// </summary>
        public Address RemoteAddress { get; private set; }

        /// <summary>
        /// TBD
        /// </summary>
        public DisassociateInfo? DisassociationInfo { get; private set; }
    }

    /// <summary>
    /// INTERNAL API
    /// </summary>
    public sealed class RGHopelessAssociation : RGEndpointException, IRGAssociationProblem
    {
        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="localAddress">TBD</param>
        /// <param name="remoteAddress">TBD</param>
        /// <param name="uid">TBD</param>
        /// <param name="cause">TBD</param>
        public RGHopelessAssociation(Address localAddress, Address remoteAddress, int? uid = null, Exception cause = null)
            : base("Catastrophic association error.", cause)
        {
            RemoteAddress = remoteAddress;
            LocalAddress = localAddress;
            Uid = uid;
        }

        /// <summary>
        /// TBD
        /// </summary>
        public Address LocalAddress { get; private set; }

        /// <summary>
        /// TBD
        /// </summary>
        public Address RemoteAddress { get; private set; }

        /// <summary>
        /// TBD
        /// </summary>
        public int? Uid { get; private set; }
    }

    /// <summary>
    /// INTERNAL API
    /// </summary>
    public sealed class RGEndpointDisassociatedException : RGEndpointException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RGEndpointDisassociatedException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RGEndpointDisassociatedException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// INTERNAL API
    /// </summary>
    public sealed class RGEndpointAssociationException : RGEndpointException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RGEndpointAssociationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RGEndpointAssociationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RGEndpointAssociationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public RGEndpointAssociationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// INTERNAL API
    /// </summary>
    public sealed class RGOversizedPayloadException : RGEndpointException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RGOversizedPayloadException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RGOversizedPayloadException(string message)
            : base(message)
        {
        }
    }
}