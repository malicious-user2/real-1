// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;

namespace YouRata.Common.YouTube;

public static class YouTubeConstants
{
    public const int MaxDescriptionLength = 5000;
    public const int PlaylistItemListQuotaCost = 1;
    public const string PrivacyStatusPrivate = "private";
    public const string RequestApplicationName = "YouRata";
    public const string RequestContentDetailsPart = "contentDetails";
    public const string RequestSnippetPart = "snippet";
    public const string RequestStatusPart = "status";
    public const int SearchListQuotaCost = 100;
    public const string VideoKind = "youtube#video";
    public const int VideoListQuotaCost = 1;
    public const int VideosResourceUpdateQuotaCost = 50;
    public const string VideoType = "video";
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromMilliseconds(1000 * 45);
}
