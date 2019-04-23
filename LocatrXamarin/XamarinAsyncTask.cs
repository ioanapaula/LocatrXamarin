using System.Linq;
using Android.OS;
using Java.Lang;
using JavaObject = Java.Lang.Object;

namespace LocatrXamarin
{
    public abstract class XamarinAsyncTask<TInput, TResult> : AsyncTask<TInput, Void, TResult> where TInput : JavaObject
    {
        public System.Action<TResult> OnPostExecuteImpl { get; set; }

        protected sealed override void OnPostExecute(JavaObject result)
        {
            var unwrappedResult = (result as JavaObjectWrapper<TResult>).ContainedObject;

            base.OnPostExecute(unwrappedResult);

            OnPostExecuteImpl?.Invoke(unwrappedResult);
        }

        protected sealed override JavaObject DoInBackground(params JavaObject[] native_parms)
        {
            if (native_parms.Any())
            {
                return new JavaObjectWrapper<TResult>(DoInBackground(native_parms.First() as TInput));
            }

            return new JavaObjectWrapper<TResult>(DoInBackground());
        }

        protected abstract TResult DoInBackground(params TInput[] parameters);

        protected override TResult RunInBackground(params TInput[] @params)
        {
            return default(TResult);
        }
    }

    public class JavaObjectWrapper<T> : Java.Lang.Object
    {
        public JavaObjectWrapper(T containedObject)
        {
            ContainedObject = containedObject;
        }

        public T ContainedObject { get; }
    }
}