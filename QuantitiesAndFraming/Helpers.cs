using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace QuantitiesAndFraming
{
    public static class Helpers
    {
        public static string GetParameterValue(Parameter p)
        {
            if (null == p) return "";
            switch (p.StorageType)
            {
                case StorageType.Double:
                    return p.AsValueString();
                case StorageType.String:
                    return p.AsString();
                case StorageType.Integer:
                    return p.AsValueString();
                case StorageType.ElementId:
                    return p.AsElementId().IntegerValue.ToString();
                default:
                    return "";
            }

        }
        public static List<Curve> GetElementCurves(this Element element,Document document)
        {

            var edges = new List<Curve>();
            var bbox = element.get_BoundingBox(document.ActiveView);

            var pt0 = new XYZ(bbox.Min.X, bbox.Min.Y, bbox.Min.Z);
            var pt1 = new XYZ(bbox.Max.X, bbox.Min.Y, bbox.Min.Z);
            var pt2 = new XYZ(bbox.Max.X, bbox.Max.Y, bbox.Min.Z);
            var pt3 = new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z);

            var edge0 = Line.CreateBound(pt0, pt1);
            var edge1 = Line.CreateBound(pt1, pt2);
            var edge2 = Line.CreateBound(pt2, pt3);
            var edge3 = Line.CreateBound(pt3, pt0);         

            edges.Add(edge0);
            edges.Add(edge1);
            edges.Add(edge2);
            edges.Add(edge3);

            return edges;
        }
        public static List<Face> GetElementFaces(this Element element,Document document)
        {
            var faces = new List<Face>();

            var op = new Options() { ComputeReferences = true, View = document.ActiveView };
            var geom = element.get_Geometry(op);

            if (geom == null)
                return faces;

            foreach (GeometryObject obj in geom)
            {
                if (obj is Solid)
                    faces.AddRange((obj as Solid).GetFaces());
                else if (obj is GeometryInstance)
                {
                    var s = obj as GeometryInstance;
                    var geomSymbol = s.GetInstanceGeometry();
                    foreach (var geomS in geomSymbol)
                    {
                        if (geomS is Solid)
                        {
                            faces.AddRange((geomS as Solid).GetFaces());
                            continue;
                        }

                        if (!(geomS is GeometryInstance))
                            continue;

                        var item2 = geomS as GeometryInstance;
                        foreach (GeometryObject item3 in item2.GetInstanceGeometry())
                        {
                            if (item3 is Solid)
                                faces.AddRange((item3 as Solid).GetFaces());
                        }
                    }
                }
            }
            return faces;
        }
        public static List<Face> GetFaces(this Solid solid)
        {
            var list = new List<Face>();
            if (solid != null)
            {
                foreach (Face face in solid.Faces)
                {
                    list.Add(face);
                }
            }
            return list;
        }
        public static Curve ProjectOntoCurve(this Plane plane, Curve curve)
        {
            return Line.CreateBound(plane.ProjectOnto(curve.GetEndPoint(0)), plane.ProjectOnto(curve.GetEndPoint(1)));
        }
        internal static XYZ ProjectOnto( this Plane plane,XYZ p)
        {
            double d = plane.SignedDistanceTo(p);
            XYZ q = p - d * plane.Normal;
            return q;
        }
        internal static double SignedDistanceTo(this Plane plane,XYZ p)
        {
        
            XYZ v = p - plane.Origin;
            return plane.Normal.DotProduct(v);
        }
        public static XYZ PointAtParameter(this Curve curve , double p)
        {
            var start = curve.GetEndPoint(0);
            var end = curve.GetEndPoint(1);

            return new XYZ(start.X + (end.X - start.X) * p, start.Y + (end.Y - start.Y) * p, start.Z + (end.Z - start.Z) * p);
        }
        public static List<Level> FindAndSortLevels(Document doc)
        {
            return new FilteredElementCollector(doc)
            .WherePasses(new ElementClassFilter(typeof(Level), false))
            .Cast<Level>()
            .OrderBy(e => e.Elevation).ToList() ;
        }
        public static FamilyInstance CreateFraming(Document document,List<Curve> curves, Level level,FamilySymbol familySymbol,int i)
        {

            return document.Create
                  .NewFamilyInstance(curves[i], familySymbol, level, StructuralType.Beam);

        }
        public static FamilyInstance CreateColumn(Document document, List<Curve> curves, Level level, FamilySymbol familySymbol, int i)
        {

            return document.Create
                 .NewFamilyInstance(curves[i], familySymbol, level, StructuralType.Column);

        }
    }
}
