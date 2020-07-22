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

            ElementId wall_Id = null;
            ElementId face_Id = null;
            PlanarFace planarFace = null;
            try
            {
                var wallFilter = new WallFilter();
                var wallselection_reference = uidoc.Selection.PickObject(ObjectType.Element, wallFilter, "Select the wall you want to frame. ESC for cancel.");
                wall_Id = wallselection_reference.ElementId;
            }
            catch (Exception)
            {

            }

            if (wall_Id == null || wall_Id == ElementId.InvalidElementId)
            {
                TaskDialog.Show("Selection Error", "Invalid element is selected. Please try again.");
                return Result.Cancelled;
            }

            try
            {
                var faceFilter = new FaceFilter();
                var faceselection_reference = uidoc.Selection.PickObject(ObjectType.Face, faceFilter, "Select the face of the wall. ESC for cancel.");
                face_Id = faceselection_reference.ElementId;
                GeometryObject geoObject = doc.GetElement(faceselection_reference).GetGeometryObjectFromReference(faceselection_reference);
                planarFace = geoObject as PlanarFace;
            }
            catch (Exception)
            {

            }

            if (face_Id == null || face_Id == ElementId.InvalidElementId)
            {
                TaskDialog.Show("Selection Error", "Invalid element is selected. Please try again.");
                return Result.Cancelled;
            }

            var selectedWall = doc.GetElement(wall_Id);
            var selectedFace = doc.GetElement(face_Id);
            using (var tx = new Transaction(doc, "Frame Walls"))
            {
                tx.Start("Frame Walls");
                var parametersList = new List<List<string>>()
                {
                    new List<string>(){Helpers.GetParameterValue(selectedWall.LookupParameter("Width")),Helpers.GetParameterValue( selectedWall.LookupParameter("Height")) },
                    new List<string>(){Helpers.GetParameterValue(selectedWall.LookupParameter("Type")) }
                };

                parametersList.Sort();

                var edges = selectedWall.GetElementCurves(doc);

                var normal = planarFace.FaceNormal;
                var origin = planarFace.Origin;
                var plan = Plane.CreateByNormalAndOrigin(normal, origin);

                var projectedCurves = edges.Select(o => plan.ProjectOntoCurve(o)).ToList();
                var curveLenghts = projectedCurves.Select(o => o.Length).ToList();

                var curvesGroupedByLenghts = new Dictionary<double, List<Curve>>();

                foreach (var lenght in curveLenghts)
                {
                    var list = new List<Curve>();
                    foreach (var curve in projectedCurves)
                    {
                        if (curve.Length == lenght)
                        {
                            list.Add(curve);
                        }

                    }
                    curvesGroupedByLenghts.Add(lenght, list);
                }


                tx.Commit();

            }

            return Result.Succeeded;

        }
    }
}
