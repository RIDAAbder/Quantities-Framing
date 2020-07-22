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
        public bool AllowElement(Element e)
        {
            return (e.GetType().Equals(typeof(Face)));
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
