/* Copyright (C) 2014 Newcastle University
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 */
using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Bootleg.API;
using Bootleg.API.Model;

namespace Bootleg.Droid.UI
{
    public static class PermissionsDialog
    {
        public static Task AskPermissions(Activity context,Shoot eventid,bool retry=false)
        {
            if (WhiteLabelConfig.USE_RELEASE_DIALOG)
            {
                if (retry || !Bootlegger.BootleggerClient.CurrentUser.permissions.ContainsKey(eventid.id))
                {
                    TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

                    var dialog = new Android.Support.V7.App.AlertDialog.Builder(context);
                    dialog.SetTitle(Resource.String.sharename);

                    View di = context.LayoutInflater.Inflate(Resource.Layout.release_dialog, null);
                    if (eventid.release != null && eventid.release != "")
                        di.FindViewById<TextView>(Resource.Id.text).TextFormatted = (Android.Text.Html.FromHtml(eventid.release));

                    if (eventid.publicview)
                        di.FindViewById<TextView>(Resource.Id.publicview).Text = context.Resources.GetString(Resource.String.publicview);
                    else
                        di.FindViewById<TextView>(Resource.Id.publicview).Text = context.Resources.GetString(Resource.String.notpublicview);

                    if (eventid.publicshare)
                        di.FindViewById<TextView>(Resource.Id.publicshare).Text = context.Resources.GetString(Resource.String.publicshare);
                    else
                        di.FindViewById<TextView>(Resource.Id.publicshare).Text = context.Resources.GetString(Resource.String.nopublicshare);

                    dialog.SetView(di);

                    dialog.SetNegativeButton(Resource.String.yes_share, new EventHandler<DialogClickEventArgs>((oe, eo) =>
                    {
                        try
                        {
                            Bootlegger.BootleggerClient.SetUserPrivacy(false, eventid.id);
                            tcs.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            LoginFuncs.ShowError(context, ex);
                            tcs.SetResult(false);
                        }

                    }))
                    .SetPositiveButton(Resource.String.no_share, new EventHandler<DialogClickEventArgs>((oe, eo) =>
                    {
                        try
                        {
                            Bootlegger.BootleggerClient.SetUserPrivacy(true, eventid.id);
                            tcs.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            LoginFuncs.ShowError(context, ex);
                            tcs.SetResult(false);
                        }
                    }))
                    .SetOnCancelListener(new OnDismissListener(() =>
                    {
                        tcs.SetException(new NotGivenPermissionException(context.Resources.GetString(Resource.String.notacceptperms)));
                    }))
                    .SetCancelable(true)
                    .Show();
                    //continue
                    return tcs.Task;
                }
                else
                {
                   return Task.FromResult(true);
                }
            }
            else
            {
                return Task.FromResult(true);
            }
        }

        private sealed class OnDismissListener : Java.Lang.Object, IDialogInterfaceOnCancelListener
        {
            private readonly Action action;

            public OnDismissListener(Action action)
            {
                this.action = action;
            }

            public void OnCancel(IDialogInterface dialog)
            {
                this.action();
            }
        }

        [Serializable]
        public class NotGivenPermissionException : Exception
        {
            public NotGivenPermissionException()
            {
            }

            public NotGivenPermissionException(string message) : base(message)
            {
            }

            public NotGivenPermissionException(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected NotGivenPermissionException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }
    }
}