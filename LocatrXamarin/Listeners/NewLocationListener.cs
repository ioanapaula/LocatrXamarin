using System;
using Android.Locations;

namespace LocatrXamarin.Listeners
{
    public class NewLocationListener : Java.Lang.Object, Android.Gms.Location.ILocationListener
    {
        public NewLocationListener(Action<Location> action)
        {
            LocationChanged = action;
        }

        public Action<Location> LocationChanged { get; set; }
           
        public void OnLocationChanged(Location location)
        {
            LocationChanged?.Invoke(location);
        }
    }
}
