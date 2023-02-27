using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace YouRata.ConflictMonitor.MilestoneInterface.WebHost;

public static class WebHostExtensions
{
    public static void AddUnixSocket(this ConfigureWebHostBuilder builder)
    {
        builder.ConfigureKestrel(opt =>
        {
            if (File.Exists(GrpcConstants.UnixSocketPath))
            {
                File.Delete(GrpcConstants.UnixSocketPath);
            }
            opt.ListenUnixSocket(GrpcConstants.UnixSocketPath);
        });
    }
}
