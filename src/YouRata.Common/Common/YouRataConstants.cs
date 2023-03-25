using System;
using System.IO;
using System.IO.Compression;

namespace YouRata.Common;

public static class YouRataConstants
{
    public const string StoredTokenResponseVariable = "TOKEN_RESPONSE";

    public const string InitialSetupCompleteVariable = "INITIAL_SETUP_COMPLETE";

    public const string CopyDirectionsReadmeVariable = "COPY_DIRECTIONS_README";

    public const string ProjectClientSecretsVariable = "PROJECT_CLIENT_SECRET";

    public const string ProjectClientIdVariable = "PROJECT_CLIENT_ID";

    public const string ProjectApiKeyVariable = "PROJECT_API_KEY";

    public const string RedirectCodeVariable = "CODE";

    public const string ActionReportFileName = "action-report.json";

    public const string PacificTimeZone = "US/Pacific";

    public const string ActionReportMessage = "ActionReport update";

    public const string ActionTokenVariable = "ACTION_TOKEN";

    public const string GitHubWorkspaceVariable = "GITHUB_WORKSPACE";

    public const string ApiTokenVariable = "API_TOKEN";

    public const string SettingsFileName = "yourata-settings.json";

    public const string ZuluTimeFormat = "yyyy-MM-dd HHmmZ";

    public const string ErrataRootDirectory = "errata/";

    public const int MilestoneLifetimeCheckInterval = 200;

    public static readonly string GrpcUnixSocketPath = Path.Combine(Path.GetTempPath(), "yourata.sock");

    public const CompressionLevel GrpcResponseCompressionLevel = CompressionLevel.NoCompression;
}
