using System;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using LocatrXamarin.Fragments;
using LocatrXamarin.Listeners;

namespace LocatrXamarin
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class LocatrActivity : SingleFragmentActivity
    {
        private const int RequestError = 0;

        protected override Android.Support.V4.App.Fragment CreateFragment()
        {
            return LocatrFragment.NewInstance();
        }

        protected override void OnResume()
        {
            base.OnResume();

            var apiAvailability = GoogleApiAvailability.Instance;
            var errorCode = apiAvailability.IsGooglePlayServicesAvailable(this);

            if (errorCode != ConnectionResult.Success)
            {
                var errorDialog = apiAvailability.GetErrorDialog(this, errorCode, RequestError, new OnCancelListener(CanceledDialog));
                errorDialog.Show();
            }
        }

        private void CanceledDialog()
        {
            Finish();
        }
    }
}