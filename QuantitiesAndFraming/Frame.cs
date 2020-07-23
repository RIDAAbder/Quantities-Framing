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

            var levels = Helpers.FindAndSortLevels(doc);

            try
            {
                var wallFilter = new WallFilter();
                var wallselection_reference = uidoc.Selection.PickObject(ObjectType.Element, wallFilter, "Select the wall you want to frame. ESC for cancel.");
                wall_Id = wallselection_reference.ElementId;
            }
            catch (Exception)
            {
                return Result.Cancelled;
            }
            
            try
            {
                var faceselection_reference = uidoc.Selection.PickObject(ObjectType.Element,new FaceFilter(doc), "Select the face of the wall. ESC for cancel.");
                face_Id = faceselection_reference.ElementId;
                GeometryObject geoObject = doc.GetElement(faceselection_reference).GetGeometryObjectFromReference(faceselection_reference);
                planarFace = geoObject as PlanarFace;
            }
            catch (Exception)
            {
                return Result.Cancelled;
            }

            if (face_Id == null || face_Id == ElementId.InvalidElementId)
            {
                TaskDialog.Show("Selection Error", "Invalid element is selected. Please try again.");
                return Result.Cancelled;
            }

            if (wall_Id == null || wall_Id == ElementId.InvalidElementId)
            {
                TaskDialog.Show("Selection Error", "Invalid element is selected. Please try again.");
                return Result.Cancelled;
            }

            var selectedWall = doc.GetElement(wall_Id);
            var selectedFace = doc.GetElement(face_Id);

            var structuralColumn = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralColumns)
                .Where(o => Helpers.GetParameterValue(o.LookupParameter("Width")).Equals(Helpers.GetParameterValue(selectedWall.LookupParameter("Width"))))
                .FirstOrDefault();

            var structuralFraming = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming)
                .FirstOrDefault();


            using (var tx = new Transaction(doc, "Frame Walls"))
            {
                tx.Start("Frame Walls");
                var parametersList = new List<double>()
                {
                    selectedWall.LookupParameter("Width").AsDouble(),
                     selectedWall.LookupParameter("Height").AsDouble()
                    
                };
                var widths = new List<double>()
                {
                    selectedWall.LookupParameter("Width").AsDouble()
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

                var curves = curvesGroupedByLenghts.Values.Select(o => o).ToList();
                var keys = curvesGroupedByLenghts.Keys.Select(o => o).ToList();
                var curvesLenghts = new List<List<double>>();
                     
                var lines = new List<List<Curve>>();
                var linesLenght = new List<List<double>>();

                for (int i = 0; i < curvesLenghts.Count; i++)
                {
                    var list = curvesLenghts[i];          
                    var subLines = new List<Curve>();
                    var curve = curves[i];

                    for (int k = 0; k < list.Count; k++)
                    {
                        var c = Math.Round(0.5 * (list[k] - parametersList[k]) / keys[k], 7);
                    
                        subLines.Add(Line.CreateBound(curve[i].PointAtParameter(c), curve[i].PointAtParameter(1 - c)));
                    }

                    lines.Add(subLines);

                    linesLenght.Add(subLines.Select(o => o.Length).ToList());
                }
                // the two list that i am going to compare 

                var lenghtsList = linesLenght.SelectMany(o => o).Distinct().ToList().Select(o => Math.Round(o, 2)).ToList() ;
                // the second list is widths 

                tx.Commit();

            }

            return Result.Succeeded;

        }
    }
}
