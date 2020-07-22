#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

namespace QuantitiesAndFraming
{
    class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            var tabName = "Matechi";
            RevitUi.AddRibbonTab(a, tabName);
            var FramingPanel = RevitUi.AddRibbonPanel(a, tabName, "Framing");

            var FramingBtn = RevitUi.AddPushButton(FramingPanel, "Process DWG", typeof(Frame), Properties.Resources.Wall, Properties.Resources.Wall, typeof(AvailableIfOpenDoc));

            FramingBtn.ToolTip = "Click to create framings for wall";

            ContextualHelp contextHelp = new ContextualHelp(
            ContextualHelpType.ChmFile,
            "mailto:info@matechi.com");

            FramingBtn.SetContextualHelp(contextHelp);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
        private class AvailableIfOpenDoc : IExternalCommandAvailability
        {
            public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
            {
                if (applicationData.ActiveUIDocument != null && applicationData.ActiveUIDocument.Document != null)
                    return true;
                return false;
            }
        }
    }
}
