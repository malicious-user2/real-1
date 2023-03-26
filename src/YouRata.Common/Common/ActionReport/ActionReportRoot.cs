// Copyright (c) 2023 battleship-systems.
// Licensed under the MIT license.

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
