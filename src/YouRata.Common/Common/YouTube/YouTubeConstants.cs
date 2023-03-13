using System;

namespace YouRata.Common.YouTube;

public static class YouTubeConstants
{
    public static string ProjectClientSecretsVariable = "PROJECT_CLIENT_SECRET";

    public static string ProjectClientIdVariable = "PROJECT_CLIENT_ID";

    public static string ProjectApiKeyVariable = "PROJECT_API_KEY";

    public static string RedirectCodeVariable = "CODE";

    public static string RequestApplicationName = "YouRata";

    public static string RequestIdPart = "id";

    public static string RequestSnippetPart = "snippet";

    public static string RequestStatusPart = "status";

    public static string RequestContentDetailsPart = "contentDetails";

    public static string VideoKind = "youtube#video";

    public static string PrivacyStatusPrivate = "private";

    public static string PrivacyStatusUnlisted = "unlisted";

    public static string PrivacyStatusPublic = "public";

    public static string VideoType = "video";

    public static int SearchListQuotaCost = 100;

    public static int VideoListQuotaCost = 1;

    public static int PlaylistItemListQuotaCost = 1;

    public static int VideosResourceUpdateQuotaCost = 50;

    public static readonly TimeSpan RequestTimeout = TimeSpan.FromMilliseconds(1000 * 45);
}
