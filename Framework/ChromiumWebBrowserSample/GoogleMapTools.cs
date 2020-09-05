using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ChromiumWebBrowserSample.UI
{
    public class GoogleMapTools
    {
        public static string GetLocation()
        {
            QueuedTask.Run(() =>
            {
                //GET ACTIVE MAPVIEW
                MapView theView = MapView.Active;

                //get current spatial reference for debugger
                SpatialReference currentSR = theView.Map.SpatialReference;

                //GET MAP CENTER AND PROJECT TO GCS
                MapPoint mapCenter = theView.Extent.Center;
                MapPoint theCoords = GeometryEngine.Instance.Project(mapCenter, SpatialReferences.WGS84) as MapPoint;

                //GET MAP SCALE
                double theScale = theView.Camera.Scale;

                //CALC GOOGLE ZOOM LEVEL EQUIVALENT AND ROUND TO NEAREST 0.25
                double theZoom = Math.Round(
                    4 * (Math.Log(591657550.500000 / (theScale / 2)) / Math.Log(2)), MidpointRounding.ToEven) / 4;

                //OPEN IN BROWSER
                string googleURL = "https://www.google.com/maps/@";
                string url = string.Format(googleURL + "{0},{1},{2}z",
                    theCoords.Y, theCoords.X, theZoom);
                return url;

            });
        }
    }

}

