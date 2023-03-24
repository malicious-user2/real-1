using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using YouRata.Common;

namespace YouRata.ConflictMonitor.MilestoneInterface.WebHost;

public static class WebHostExtensions
{
    public static void AddUnixSocket(this ConfigureWebHostBuilder builder)
    {
        builder.ConfigureKestrel(opt =>
        {
            if (File.Exists(YouRataConstants.GrpcUnixSocketPath))
            {
                File.Delete(YouRataConstants.GrpcUnixSocketPath);
            }
            opt.ListenUnixSocket(YouRataConstants.GrpcUnixSocketPath);
        });
    }
}
