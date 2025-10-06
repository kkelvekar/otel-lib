namespace UBS.AM.Observability.Enums
{
    /// <summary>
    /// Defines the OTLP export protocols.
    /// </summary>
    public enum OtlpProtocol
    {
        /// <summary>
        /// OTLP over gRPC.
        /// </summary>
        Grpc,

        /// <summary>
        /// OTLP over HTTP with Protobuf payloads.
        /// </summary>
        HttpProtobuf
    }
}
