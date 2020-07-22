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
            var wallselection_reference = uidoc.Selection.PickObject(ObjectType.Element, wallFilter);
            wall_Id = wallselection_reference.ElementId;

            var faceFilter = new FaceFilter();
            var faceselection_reference = uidoc.Selection.PickObject(ObjectType.Element, faceFilter);
            face_Id = faceselection_reference.ElementId;


            if (wall_Id == null || wall_Id == ElementId.InvalidElementId)
            {
                TaskDialog.Show("Selection Error", "Invalid element is selected. Please try again.");
                return Result.Cancelled;
            }

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

                var bbox = selectedWall.get_BoundingBox(doc.ActiveView);


                var pt0 = new XYZ(bbox.Min.X, bbox.Min.Y, bbox.Min.Z);
                var pt1 = new XYZ(bbox.Max.X, bbox.Min.Y, bbox.Min.Z);
                var pt2 = new XYZ(bbox.Max.X, bbox.Max.Y, bbox.Min.Z);
                var pt3 = new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z);


                var edge0 = Line.CreateBound(pt0, pt1);
                var edge1 = Line.CreateBound(pt1, pt2);
                var edge2 = Line.CreateBound(pt2, pt3);
                var edge3 = Line.CreateBound(pt3, pt0);


                var edges = new List<Curve>();

                edges.Add(edge0);
                edges.Add(edge1);
                edges.Add(edge2);
                edges.Add(edge3);

                tx.Commit();
                
            }

            return Result.Succeeded;

        }
    }
}
