using System;
using System.IO;
using System.IO.Compression;

namespace YouRatta.ConflictMonitor;

public static class GrpcConstants
{
    public static readonly string UnixSocketPath = Path.Combine(Path.GetTempPath(), "youratta.sock");
    public static readonly CompressionLevel ResponseCompressionLevel = CompressionLevel.NoCompression;
}
