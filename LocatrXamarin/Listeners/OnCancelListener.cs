using System;
using Android.Content;

namespace LocatrXamarin.Listeners
{
    public class OnCancelListener : Java.Lang.Object, IDialogInterfaceOnCancelListener
    {
        public OnCancelListener(Action action)
        {
            OnCanceled = action;
        }

        public Action OnCanceled { get; set; }

        public void OnCancel(IDialogInterface dialog)
        {
            OnCanceled?.Invoke();
        }
    }
}
