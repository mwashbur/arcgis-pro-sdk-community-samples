/*

   Copyright 2020 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ChromiumWebBrowserSample.UI
{
    internal class ChromePaneViewModel : ViewStatePane
	{
		private const string _viewPaneID = "ChromiumWebBrowserSample_UI_ChromePane";

		/// <summary>
		/// Consume the passed in CIMView. Call the base constructor to wire up the CIMView.
		/// </summary>
		public ChromePaneViewModel(CIMView view)
			: base(view) { }

		/// <summary>
		/// Create a new instance of the pane.
		/// </summary>
		internal static ChromePaneViewModel Create()
		{
			var view = new CIMGenericView();
			view.ViewType = _viewPaneID;
			return FrameworkApplication.Panes.Create(_viewPaneID, new object[] { view }) as ChromePaneViewModel;
		}

		#region Properties

		private string _browserAddress;
		public string BrowserAddress
		{
			get
			{
				return _browserAddress;
			}
			set
			{
				SetProperty(ref _browserAddress, value, () => BrowserAddress);
			}
		}

		private ICommand _openResourceCmd = null;
		private ICommand _openEmbeddedResourceCmd = null;
		private ICommand _openFileResourceCmd = null;

		public ICommand OpenResourceCmd
		{
			get
			{
				if (_openResourceCmd == null)
				{
					_openResourceCmd = new RelayCommand(new Action<Object>((sender) =>
					{
						var menu_item = sender as System.Windows.Controls.MenuItem;
						var text = menu_item.Header;
						var address = $@"resource:/{text}";
						this.BrowserAddress = address;
					}), () => { return true; });
				}
				return _openResourceCmd;
			}
		}

		public ICommand OpenEmbeddedResourceCmd
		{
			get
			{
				if (_openEmbeddedResourceCmd == null)
				{
					_openEmbeddedResourceCmd = new RelayCommand(new Action<Object>((sender) =>
					{
						var menu_item = sender as System.Windows.Controls.MenuItem;
						var text = menu_item.Header;
						var address = $@"embeddedresource:/{text}";
						this.BrowserAddress = address;
					}), () => { return true; });
				}
				return _openEmbeddedResourceCmd;
			}
		}

		public ICommand OpenFileResourceCmd
		{
			get
			{
				if (_openFileResourceCmd == null)
				{
					_openFileResourceCmd = new RelayCommand(new Action<Object>((sender) =>
					{
						var fileFilter = BrowseProjectFilter.GetFilter("esri_browseDialogFilters_browseFiles");
						fileFilter.FileExtension = "*.*";
						fileFilter.BrowsingFilesMode = true;

						var dlg = new OpenItemDialog() { BrowseFilter = fileFilter,
							Title = "Browse Content"
						};
						if (!dlg.ShowDialog().Value)
							return;
						var item = dlg.Items.First();
						//format the URL
						var address = $@"file:///{item.Path.Replace(@"\", @"/")}";
						this.BrowserAddress = address;
					}), () => { return true; });
				}
				return _openFileResourceCmd;
			}
		}

		#endregion

		#region Pane Overrides

		/// <summary>
		/// Must be overridden in child classes used to persist the state of the view to the CIM.
		/// </summary>
		public override CIMView ViewState
		{
			get
			{
				_cimView.InstanceID = (int)InstanceID;
				return _cimView;
			}
		}

		/// <summary>
		/// Called when the pane is initialized.
		/// </summary>
		protected async override Task InitializeAsync()
		{
			await base.InitializeAsync();
		}

		/// <summary>
		/// Called when the pane is uninitialized.
		/// </summary>
		protected async override Task UninitializeAsync()
		{
			await base.UninitializeAsync();
		}

		#endregion Pane Overrides
	}

	/// <summary>
	/// Button implementation to create a new instance of the pane and activate it.
	/// </summary>
	internal class ChromePane_OpenButton : Button
	{
		protected override void OnClick()
		{
            try
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
                //Process.Start(url);
                var vm = ChromePaneViewModel.Create();
                vm.BrowserAddress = url;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }


        }
    }
}
