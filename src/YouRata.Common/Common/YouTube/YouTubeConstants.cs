using System;

namespace YouRata.Common.YouTube;

public static class YouTubeConstants
{
    public const string RequestApplicationName = "YouRata";

    public const string RequestSnippetPart = "snippet";

    public const string RequestStatusPart = "status";

    public const string RequestContentDetailsPart = "contentDetails";

    public const string VideoKind = "youtube#video";

    public const string PrivacyStatusPrivate = "private";

    public const string VideoType = "video";

    public const int SearchListQuotaCost = 100;

    public const int VideoListQuotaCost = 1;

    public const int PlaylistItemListQuotaCost = 1;

    public const int VideosResourceUpdateQuotaCost = 50;

    public static readonly TimeSpan RequestTimeout = TimeSpan.FromMilliseconds(1000 * 45);

    public const int MaxDescriptionLength = 5000;
}
