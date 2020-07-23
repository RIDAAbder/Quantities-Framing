using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;


namespace QuantitiesAndFraming
{
    class FaceFilter : ISelectionFilter
    {
        Document doc = null;
        public FaceFilter(Document document)
        {
            doc = document;
        }
        public bool AllowElement(Element e)
        {
            return true;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            if (doc.GetElement(reference).GetGeometryObjectFromReference(reference) is PlanarFace)
            {
                return true; 
            }
            return false;
        }
    }
}
