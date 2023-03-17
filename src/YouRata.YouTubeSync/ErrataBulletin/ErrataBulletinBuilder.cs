using System;
using System.Globalization;
using YouRata.Common.Configurations;
using YouRata.Common.Milestone;

namespace YouRata.YouTubeSync.ErrataBulletin;

internal class ErrataBulletinBuilder
{
    private readonly ErrataBulletinConfiguration _configuration;
    private readonly string _snippetTitle;
    private readonly TimeSpan _contentDuration;

    private const string mdTemplateTitleTop1 = "# {0}";
    private const string mdTemplateTitleBottom1 = "#";
    private const string mdTemplateTitleBottom2 = "{0}";
    private const string mdTemplateTitleBottom3 = "===";
    private const string mdTemplateTimeValueMark1 = "{0} ------------------------------------------------";
    private const string mdTemplateInstructions1 = "{0}";

    public ErrataBulletinBuilder(ErrataBulletinConfiguration config, string snippetTitle, TimeSpan contentDuration)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(snippetTitle);
        _configuration = config;
        _snippetTitle = snippetTitle;
        _contentDuration = contentDuration;
    }

    private void AddInstructions(ref string bulletinTemplate)
    {
        string instructions = string.Format(CultureInfo.InvariantCulture, mdTemplateInstructions1, _configuration.Instructions);
        bulletinTemplate = $"{instructions}{GitHubNewLine}{bulletinTemplate}";
    }

    private void AddTimeValueMarks(ref string bulletinTemplate)
    {
        int secondCounter = 0;
        while (secondCounter < _contentDuration.TotalSeconds)
        {
            if (secondCounter % _configuration.SecondsPerMark == 0)
            {
                TimeSpan secondValue = TimeSpan.FromSeconds(secondCounter);
                if (secondCounter >= 3600)
                {
                    bulletinTemplate += string.Format(CultureInfo.InvariantCulture, mdTemplateTimeValueMark1, secondValue.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture));
                }
                else
                {
                    bulletinTemplate += string.Format(CultureInfo.InvariantCulture, mdTemplateTimeValueMark1, secondValue.ToString(@"m\:ss", CultureInfo.InvariantCulture));
                }
                
                for (int emptyLines = 0; emptyLines <= _configuration.EmptyLinesPerMark; emptyLines++)
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
                bulletinTemplate = string.Format(CultureInfo.InvariantCulture, mdTemplateTitleTop1, _snippetTitle)
                    + GitHubNewLine + GitHubNewLine + bulletinTemplate;
                break;
            case "bottom":
                bulletinTemplate += mdTemplateTitleBottom1;
                bulletinTemplate += GitHubNewLine;
                bulletinTemplate += string.Format(CultureInfo.InvariantCulture, mdTemplateTitleBottom2, _snippetTitle);
                bulletinTemplate += GitHubNewLine;
                bulletinTemplate += mdTemplateTitleBottom3;
                break;
        }
    }

    public string Build()
    {
        string errataBulletin = string.Empty;
        AddInstructions(ref errataBulletin);
        AddTimeValueMarks(ref errataBulletin);
        AddTitle(ref errataBulletin);
        if (string.IsNullOrEmpty(_snippetTitle)) throw new MilestoneException("No title specified to build errata bulletin");
        return errataBulletin;
    }

    private string GitHubNewLine => "  " + Environment.NewLine;
}
