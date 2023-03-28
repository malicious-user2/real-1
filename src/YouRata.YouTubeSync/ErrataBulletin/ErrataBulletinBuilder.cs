// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

using System;
using System.Globalization;
using YouRata.Common.Configuration.ErrataBulletin;
using YouRata.Common.Milestone;

namespace YouRata.YouTubeSync.ErrataBulletin;

/// <summary>
/// Builds an errata bulletin string from a video snippet
/// </summary>
internal sealed class ErrataBulletinBuilder
{
    // --- GitHub flavored markdown templates ---
    // Editing instructions template
    private const string MdTemplateInstructions1 = "{0}";
    // Time separator
    private const string MdTemplateTimeValueMark1 = "{0} ------------------------------------------------";
    // Title marker
    private const string MdTemplateTitleBottom1 = "#";
    // Title template
    private const string MdTemplateTitleBottom2 = "{0}";
    // Title marker
    private const string MdTemplateTitleBottom3 = "===";
    // Title template
    private const string MdTemplateTitleTop1 = "# {0}";
    private readonly ErrataBulletinConfiguration _configuration;
    private readonly TimeSpan _contentDuration;

    public ErrataBulletinBuilder(ErrataBulletinConfiguration config, string snippetTitle, TimeSpan contentDuration)
    {
        ArgumentException.ThrowIfNullOrEmpty(snippetTitle);
        _configuration = config;
        SnippetTitle = snippetTitle;
        _contentDuration = contentDuration;
    }

    public string Build()
    {
        if (string.IsNullOrEmpty(SnippetTitle))
        {
            throw new MilestoneException("No title specified to build errata bulletin");
        }
        string errataBulletin = string.Empty;
        AddInstructions(ref errataBulletin);
        AddTimeValueMarks(ref errataBulletin);
        AddTitle(ref errataBulletin);

        return errataBulletin;
    }

    private void AddInstructions(ref string bulletinTemplate)
    {
        string instructions = string.Format(CultureInfo.InvariantCulture, MdTemplateInstructions1,
            _configuration.Instructions);
        bulletinTemplate = $"{instructions}{GitHubNewLine}{bulletinTemplate}";
    }

    private void AddTimeValueMarks(ref string bulletinTemplate)
    {
        int secondCounter = 0;
        // Iterate through every second of the video duration
        while (secondCounter < _contentDuration.TotalSeconds)
        {
            // Check if the second is divisible by SecondsPerMark
            if (secondCounter % _configuration.SecondsPerMark == 0)
            {
                TimeSpan secondValue = TimeSpan.FromSeconds(secondCounter);
                if (secondCounter >= 3600)
                {
                    // Include hours in the time
                    bulletinTemplate += string.Format(CultureInfo.InvariantCulture, MdTemplateTimeValueMark1,
                        secondValue.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture));
                }
                else
                {
                    bulletinTemplate += string.Format(CultureInfo.InvariantCulture, MdTemplateTimeValueMark1,
                        secondValue.ToString(@"m\:ss", CultureInfo.InvariantCulture));
                }

                for (var emptyLines = 0; emptyLines <= _configuration.EmptyLinesPerMark; emptyLines++)
                {
                    // Add blank lines after marks
                    bulletinTemplate += Environment.NewLine;
                }
            }

            secondCounter++;
        }
    }

    private void AddTitle(ref string bulletinTemplate)
    {
        switch (_configuration.TitleLocation)
        {
            case "top":
                bulletinTemplate = string.Format(CultureInfo.InvariantCulture, MdTemplateTitleTop1, SnippetTitle)
                                   + GitHubNewLine + GitHubNewLine + bulletinTemplate;
                break;

            case "bottom":
                bulletinTemplate += MdTemplateTitleBottom1;
                bulletinTemplate += GitHubNewLine;
                bulletinTemplate += string.Format(CultureInfo.InvariantCulture, MdTemplateTitleBottom2, SnippetTitle);
                bulletinTemplate += GitHubNewLine;
                bulletinTemplate += MdTemplateTitleBottom3;
                break;
        }
    }

    public string SnippetTitle { get; }

    // For a new line to appear in markdown
    private string GitHubNewLine => "  " + Environment.NewLine;
}
