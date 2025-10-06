using System;

namespace UBS.AM.Observability.Configuration
{
    internal static class OtlpEndpoints
    {
        /// <summary>
        /// Default OTLP HTTP (OTLP/HTTP) endpoint for the collector.
        /// Common default: http://localhost:4318
        /// </summary>
        internal const string HttpEndpoint = "http://localhost:4318";

        /// <summary>
        /// Default OTLP gRPC endpoint for the collector.
        /// Common default: http://localhost:4317
        /// </summary>
        internal const string GrpcEndpoint = "http://localhost:4317";
    }
}
