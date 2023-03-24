using System;

namespace YouRata.Common.YouTube;

public static class YouTubeConstants
{
    public static readonly string RequestApplicationName = "YouRata";

    public static readonly string RequestSnippetPart = "snippet";

    public static readonly string RequestStatusPart = "status";

    public static readonly string RequestContentDetailsPart = "contentDetails";

    public static readonly string VideoKind = "youtube#video";

    public static readonly string PrivacyStatusPrivate = "private";

    public static readonly string VideoType = "video";

    public static readonly int SearchListQuotaCost = 100;

    public static readonly int VideoListQuotaCost = 1;

    public static readonly int PlaylistItemListQuotaCost = 1;

    public static readonly int VideosResourceUpdateQuotaCost = 50;

    public static readonly TimeSpan RequestTimeout = TimeSpan.FromMilliseconds(1000 * 45);
}
