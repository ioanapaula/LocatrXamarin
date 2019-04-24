using System;
using Android.App;
using Android.Content;
using Android.OS;

namespace LocatrXamarin.Fragments
{
    public class RationaleDialogFragment : Android.Support.V4.App.DialogFragment
    {
        public interface IRationaleCallback
        {
            void OnRationaleDialogDismissed();
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            return new Android.Support.V7.App.AlertDialog.Builder(Activity)
                .SetTitle(Resource.String.rationale)
                .SetPositiveButton(Android.Resource.String.Ok, OkAction)
                .Create();
        }

        public override void OnCancel(IDialogInterface dialog)
        {
            base.OnCancel(dialog);

            (TargetFragment as IRationaleCallback)?.OnRationaleDialogDismissed();
        }

        private void OkAction(object sender, DialogClickEventArgs e)
        {
            (TargetFragment as IRationaleCallback)?.OnRationaleDialogDismissed();
        }
    }
}
