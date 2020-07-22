#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;
#endregion

namespace QuantitiesAndFraming
{
    [Transaction(TransactionMode.Manual)]
    public class Frame : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            ElementId wall_Id ;
            ElementId face_Id;

            var wallFilter = new WallFilter();
            var wallselection_reference = uidoc.Selection.PickObject(ObjectType.Element, wallFilter, "Select the wall you want to frame") ;
            wall_Id = wallselection_reference.ElementId;

            if (wall_Id == null || wall_Id == ElementId.InvalidElementId)
            {
                TaskDialog.Show("Selection Error", "Invalid element is selected. Please try again.");
                return Result.Cancelled;
            }

            var faceFilter = new FaceFilter();
            var faceselection_reference = uidoc.Selection.PickObject(ObjectType.Element, faceFilter,"Select the face of the wall");
            face_Id = faceselection_reference.ElementId;         

            if (face_Id == null || face_Id == ElementId.InvalidElementId)
            {
                TaskDialog.Show("Selection Error", "Invalid element is selected. Please try again.");
                return Result.Cancelled;
            }

            var selectedWall = doc.GetElement(wall_Id);
            var selectedFace = doc.GetElement(face_Id);
            using ( var tx = new Transaction(doc,"Frame Walls"))
            {
                tx.Start("Frame Walls");
                var parametersList = new List<List<string>>()
                {
                    new List<string>(){Helpers.GetParameterValue(selectedWall.LookupParameter("Width")),Helpers.GetParameterValue( selectedWall.LookupParameter("Height")) },
                    new List<string>(){Helpers.GetParameterValue(selectedWall.LookupParameter("Type")) }
                };

                parametersList.Sort();

                var edges = selectedWall.GetElementCurves(doc);

                tx.Commit();
                
            }

            return Result.Succeeded;

        }
    }
}
