using System;
using System.Collections.Generic;
using Android;
using Android.Content.PM;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Java.IO;
using LocatrXamarin.Listeners;
using LocatrXamarin.Models;

namespace LocatrXamarin.Fragments
{
    public class LocatrFragment : SupportMapFragment, GoogleApiClient.IConnectionCallbacks, IOnMapReadyCallback
    {
        private new const string Tag = "LocatrFragment";
        private const int RequestLocationPermissions = 0;
        private string[] _locationPermissions = new string[] { Manifest.Permission.AccessFineLocation, Manifest.Permission.AccessCoarseLocation };

        private GoogleApiClient _client;
        private GoogleMap _map;
        private Bitmap _mapImage;
        private GalleryItem _mapItem;
        private Location _currentLocation;

        public static new LocatrFragment NewInstance()
        {
            return new LocatrFragment();
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _client = new GoogleApiClient.Builder(Activity)
                .AddApi(LocationServices.API)
                .AddConnectionCallbacks(this)
                .Build();

            GetMapAsync(this);

            HasOptionsMenu = true;
        }

        public override void OnStart()
        {
            base.OnStart();

            Activity.InvalidateOptionsMenu();
            _client.Connect();
        }

        public override void OnStop()
        {
            base.OnStop();

            _client.Disconnect();
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            base.OnCreateOptionsMenu(menu, inflater);

            inflater.Inflate(Resource.Menu.fragment_locatr, menu);
            var searchItem = menu.FindItem(Resource.Id.action_locate);
            searchItem.SetEnabled(_client.IsConnected);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_locate:
                    if (HasLocationPermission())
                    {
                        FindImage();
                    }
                    else
                    {
                        RequestPermissions(_locationPermissions, RequestLocationPermissions);
                    }

                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            switch (requestCode)
            {
                case RequestLocationPermissions:
                    if (HasLocationPermission())
                    {
                        FindImage();
                    }

                    break;
                default:
                    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                    break;
            }
        }

        public void OnConnected(Bundle connectionHint)
        {
            Activity.InvalidateOptionsMenu();
        }

        public void OnConnectionSuspended(int cause)
        {
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            _map = googleMap;
            UpdateUI();
        }

        private void OnLocationChanged(Location location)
        {
            Log.Info(Tag, $"Got a fix: {location}");
            new SearchTask()
            {
                OnTaskCompleted = OnBitmapFetched
            }
            .Execute(location);
        }

        private void OnBitmapFetched(GalleryItem galleryItem, Bitmap bitmap, Location location)
        {
            _mapItem = galleryItem;
            _mapImage = bitmap;
            _currentLocation = location;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_map == null || _mapImage == null)
            {
                return;
            }

            var itemPoint = new LatLng(_mapItem.Latitude, _mapItem.Longitude);
            var myPoint = new LatLng(_currentLocation.Latitude, _currentLocation.Longitude);

            var itemBitmap = BitmapDescriptorFactory.FromBitmap(_mapImage);
            var itemMarker = new MarkerOptions()
                .SetPosition(itemPoint)
                .SetIcon(itemBitmap);
            var myMarker = new MarkerOptions()
                .SetPosition(myPoint);

            _map.Clear();
            _map.AddMarker(itemMarker);
            _map.AddMarker(myMarker);

            var bounds = new LatLngBounds.Builder()
                .Include(itemPoint)
                .Include(myPoint)
                .Build();
            var margin = Resources.GetDimensionPixelSize(Resource.Dimension.map_inset_margin);
            var update = CameraUpdateFactory.NewLatLngBounds(bounds, margin);
            _map.AnimateCamera(update);
        }

        private void FindImage()
        {
            var request = LocationRequest.Create();
            request.SetPriority(LocationRequest.PriorityHighAccuracy);
            request.SetNumUpdates(1);
            request.SetInterval(0);
            LocationServices.FusedLocationApi.RequestLocationUpdates(_client, request, new NewLocationListener(OnLocationChanged));
        }

        private bool HasLocationPermission()
        {
            var result = ContextCompat.CheckSelfPermission(Activity, _locationPermissions[0]);

            return result == Permission.Granted;
        }

        private class SearchTask : XamarinAsyncTask<Location, Bitmap>
        {
            private GalleryItem _galleryItem;
            private Bitmap _bitmap;
            private Location _location;

            public SearchTask()
            {
                OnPostExecuteImpl = InvokeOnCompleted;
            }

            public Action<GalleryItem, Bitmap, Location> OnTaskCompleted { get; set; }

            protected override Bitmap DoInBackground(params Location[] parameters)
            {
                _location = parameters[0];
                var fetchr = new FlickrFetchr();
                List<GalleryItem> items = fetchr.SearchPhotos(parameters[0]);

                if (items.Count == 0)
                {
                    return null;
                }

                _galleryItem = items[0];

                try
                {
                    byte[] bytes = fetchr.GetUrlBytes(_galleryItem.Url);
                    _bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
                }
                catch (IOException ioe)
                {
                    Log.Info(Tag, "Unable to download bitmap", ioe);
                }

                return _bitmap;
            }

            private void InvokeOnCompleted(Bitmap obj)
            {
                OnTaskCompleted?.Invoke(_galleryItem, _bitmap, _location);
            }
        }
    }
}
