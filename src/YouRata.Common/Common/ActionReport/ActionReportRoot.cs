using System;

namespace YouRata.Common.ActionReport;

public class ActionReportRoot
{
    public ActionReportRoot()
    {
        ActionReport = new ActionReportLayout();
    }

    public ActionReportLayout ActionReport { get; set; }
}
