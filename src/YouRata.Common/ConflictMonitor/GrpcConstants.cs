using System;
using System.IO;
using System.IO.Compression;

namespace YouRata.ConflictMonitor;

public static class GrpcConstants
{
    public static readonly string UnixSocketPath = Path.Combine(Path.GetTempPath(), "yourata.sock");
    public static readonly CompressionLevel ResponseCompressionLevel = CompressionLevel.NoCompression;
}
