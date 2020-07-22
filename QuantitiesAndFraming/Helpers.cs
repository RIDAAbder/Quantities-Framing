using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace QuantitiesAndFraming
{
    class Helpers
    {
        public static string GetParameterValue(Parameter p)
        {
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
    }
}
