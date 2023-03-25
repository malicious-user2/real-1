using System;
using System.Globalization;
using YouRata.Common.Configuration.ErrataBulletin;
using YouRata.Common.Milestone;

namespace YouRata.YouTubeSync.ErrataBulletin;

internal sealed class ErrataBulletinBuilder
{
    private const string MdTemplateTitleTop1 = "# {0}";
    private const string MdTemplateTitleBottom1 = "#";
    private const string MdTemplateTitleBottom2 = "{0}";
    private const string MdTemplateTitleBottom3 = "===";
    private const string MdTemplateTimeValueMark1 = "{0} ------------------------------------------------";
    private const string MdTemplateInstructions1 = "{0}";
    private readonly ErrataBulletinConfiguration _configuration;
    private readonly TimeSpan _contentDuration;

    public ErrataBulletinBuilder(ErrataBulletinConfiguration config, string snippetTitle, TimeSpan contentDuration)
    {
        ArgumentException.ThrowIfNullOrEmpty(snippetTitle);
        _configuration = config;
        SnippetTitle = snippetTitle;
        _contentDuration = contentDuration;
    }

    public string SnippetTitle { get; }

    private string GitHubNewLine => "  " + Environment.NewLine;

    private void AddInstructions(ref string bulletinTemplate)
    {
        var instructions = string.Format(CultureInfo.InvariantCulture, MdTemplateInstructions1,
            _configuration.Instructions);
        bulletinTemplate = $"{instructions}{GitHubNewLine}{bulletinTemplate}";
    }

    private void AddTimeValueMarks(ref string bulletinTemplate)
    {
        var secondCounter = 0;
        while (secondCounter < _contentDuration.TotalSeconds)
        {
            if (secondCounter % _configuration.SecondsPerMark == 0)
            {
                var secondValue = TimeSpan.FromSeconds(secondCounter);
                if (secondCounter >= 3600)
                {
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

    public string Build()
    {
        var errataBulletin = string.Empty;
        AddInstructions(ref errataBulletin);
        AddTimeValueMarks(ref errataBulletin);
        AddTitle(ref errataBulletin);
        if (string.IsNullOrEmpty(SnippetTitle))
        {
            throw new MilestoneException("No title specified to build errata bulletin");
        }

        return errataBulletin;
    }
}
