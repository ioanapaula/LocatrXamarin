using System;
using System.Collections.Generic;
using Android;
using Android.Content.PM;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;
using LocatrXamarin.Listeners;
using LocatrXamarin.Models;
using static LocatrXamarin.Fragments.RationaleDialogFragment;

namespace LocatrXamarin.Fragments
{
    public class LocatrFragment : Fragment, GoogleApiClient.IConnectionCallbacks, IRationaleCallback
    {
        private new const string Tag = "LocatrFragment";
        private const string DialogRationale = "DialogRationale";
        private const int RequestLocationPermissions = 0;
        private const int RequestRationale = 1;

        private string[] _locationPermissions = new string[] { Manifest.Permission.AccessFineLocation, Manifest.Permission.AccessCoarseLocation };

        private ImageView _imageView;
        private GoogleApiClient _client;

        public static LocatrFragment NewInstance()
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

            HasOptionsMenu = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_locatr, container, false);
            _imageView = view.FindViewById<ImageView>(Resource.Id.image);

            return view;
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
                        if (ShouldShowRequestPermissionRationale(_locationPermissions[0]) == false)
                        {
                            RequestPermissions(_locationPermissions, RequestLocationPermissions);
                        }
                        else
                        {
                            var manager = Activity.SupportFragmentManager;
                            var dialog = new RationaleDialogFragment();
                            dialog.SetTargetFragment(this, RequestRationale);
                            dialog.Show(manager, DialogRationale);
                        }
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

        public void OnRationaleDialogDismissed()
        {
            RequestPermissions(_locationPermissions, RequestLocationPermissions);
        }

        private void OnLocationChanged(Location location)
        {
            Log.Info(Tag, $"Got a fix: {location}");
            new SearchTask()
            {
                OnPostExecuteImpl = OnBitmapFetched
            }
            .Execute(location);
        }

        private void OnBitmapFetched(Bitmap obj)
        {
            _imageView.SetImageBitmap(obj);
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

            protected override Bitmap DoInBackground(params Location[] parameters)
            {
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
        }
    }
}
