/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
#region using
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;

using Java.IO;

using System;
using System.Net;
using System.Text;
#endregion
namespace NamaadMobile.Util
{
	/// <summary>
	/// TODO: Additional methods can be reduced.
	/// </summary>
	public class ExceptionHandler
	{
		// public static Boolean logging = true;

		private static string app = "aSQLMan";

		/// <summary>
		/// Write a debug message to the log
		/// </summary>
		/// <param name="msg">The message</param>
		/// <param name="logging">if set to <c>true</c> [logging].</param>
		public static void logD(string msg, bool logging)
		{
			if (logging)
				Log.Debug(app, msg);
		}
		/// <summary>
		/// Write an error message to the log
		/// </summary>
		/// <param name="msg">The message</param>
		/// <param name="logging">if set to <c>true</c> [logging].</param>
		public static void logE(string msg, bool logging)
		{
			if (logging)
				if (msg != null)
					Log.Error(app, msg);
				else
					Log.Error(app, "Unknown error");
		}
		/// <summary>
		/// Write a debug message to the log for activityName
		/// </summary>
		/// <param name="msg">The MSG.</param>
		/// <param name="logging">if set to <c>true</c> [logging].</param>
		/// <param name="activityName">Name of the activity.</param>
		public static void logDActivity(string msg, bool logging, string activityName)
		{
			if (logging)
				Log.Debug(activityName, msg);
		}
		/// <summary>
		/// Write an error message to the log for activityName
		/// </summary>
		/// <param name="msg">The MSG.</param>
		/// <param name="logging">if set to <c>true</c> [logging].</param>
		/// <param name="activityName">Name of the activity.</param>
		public static void logEActivity(string msg, bool logging, string activityName)
		{
			if (logging)
				if (msg != null)
					Log.Error(activityName, msg);
				else
					Log.Error(activityName, "Unknown error");
		}
		/// <summary>
		/// Show a dialog with an error message
		/// </summary>
		/// <param name="e">The message</param>
		/// <param name="cont">the content of the form to show the message on</param>
		public static void showException(string e, Context cont)
		{
			AlertDialog alertDialog = new AlertDialog.Builder(cont).Create();
			alertDialog.SetTitle(cont.GetText(Resource.String.Error));
			alertDialog.SetMessage(e);
			alertDialog.SetButton(cont.GetText(Resource.String.OK), new OnClickListener(delegate
				{
				}
			));
			alertDialog.Show();
		}
		private class OnClickListener : Java.Lang.Object, IDialogInterfaceOnClickListener
		{
			private Action on_Click_Listener;
			public OnClickListener(Action on_Click_Listener)
			{
				this.on_Click_Listener = on_Click_Listener;
			}
			public void OnClick(IDialogInterface dialog, int which)
			{
				on_Click_Listener();
				return;
			}
		}
		public static void showMessage(string title, string msg, Context cont)
		{
			showMessage(title, msg, 0, cont.GetText(Resource.String.OK), cont);
		}

		public static void showMessage(string title, string msg, string btnText,
				Context cont)
		{
			showMessage(title, msg, 0, btnText, cont);
		}

		public static void showMessage(string title, string msg, int icon,
				string btnText, Context cont)
		{
			AlertDialog alertDialog = new AlertDialog.Builder(cont).Create();
			if (icon != 0)
			{
				alertDialog.SetIcon(icon);
			}
			alertDialog.SetTitle(title);
			if (msg != null)
			{
				alertDialog.SetMessage(msg);
			}
			alertDialog.SetButton(btnText, new OnClickListener(delegate
			{
			}
			));
			alertDialog.Show();
		}

		public static void showModalDialog(string title, string msg, string posText,
				string negText, Context cont)
		{
			showModalDialog(title, msg, 0, posText, null, negText, null, cont);
		}

		public static void showModalDialog(string title, string msg, string posText,
				IDialogInterfaceOnClickListener posAction, string negText, Context cont)
		{
			showModalDialog(title, msg, 0, posText, posAction, negText, null, cont);
		}

		public static void showModalDialog(string title, string msg, string posText,
				string negText, IDialogInterfaceOnClickListener negAction, Context cont)
		{
			showModalDialog(title, msg, 0, posText, null, negText, negAction, cont);
		}

		public static void showModalDialog(string title, string msg, int icon,
				string posText, string negText, Context cont)
		{
			showModalDialog(title, msg, icon, posText, null, negText, null, cont);
		}

		public static void showModalDialog(string title, string msg, int icon,
				string posText, IDialogInterfaceOnClickListener posAction,
				string negText, Context cont)
		{
			showModalDialog(title, msg, icon, posText, posAction, negText, null, cont);
		}

		public static void showModalDialog(string title, string msg, int icon,
				string posText, string negText,
				IDialogInterfaceOnClickListener negAction, Context cont)
		{
			showModalDialog(title, msg, icon, posText, null, negText, negAction, cont);
		}

		public static void showModalDialog(string title, string msg, int icon,
				string posText, IDialogInterfaceOnClickListener posAction,
				string negText, IDialogInterfaceOnClickListener negAction, Context cont)
		{
			AlertDialog.Builder dialog = new AlertDialog.Builder(cont);
			if (icon != 0)
			{
				dialog.SetIcon(icon);
			}
			dialog.SetTitle(title);
			dialog.SetMessage(msg);
			dialog.SetPositiveButton(posText, posAction);
			dialog.SetNegativeButton(negText, negAction);
			dialog.Show();
		}
		/// <summary>
		/// Display the message as a Long toast message
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="msg">The message to display</param>
		public static void toastMsg(Context context, string msg)
		{
			Toast toast = Toast.MakeText(context, msg, ToastLength.Long);
			toast.SetGravity(GravityFlags.Top | GravityFlags.Center, 0, 0);
			toast.Show();
		}

		/**
		 * Display the message in red text as a short toast message
		 * @param context
		 * @param msg - The message to display
		 */
		public static void redToastMsg(Context context, string msg)
		{
			Toast toast = Toast.MakeText(context, msg, ToastLength.Long);
			toast.SetGravity(GravityFlags.Top | GravityFlags.Center, 0, 0);
			TextView v = (TextView)toast.View.FindViewById(Android.Resource.Id.Message);
			v.SetTextColor(Color.Red);
			v.SetBackgroundColor(Color.White);
			toast.Show();
		}

		/**
		 * Test if a SDCard is available
		 * 
		 * @return true if a external SDCard is available
		 */
		public static bool isSDAvailable()
		{
			string state = Android.OS.Environment.ExternalStorageState;
			if (Android.OS.Environment.MediaMounted.Equals(state))
			{
				return true;
			}
			return false;
		}

		/**
		 * Write an exceptions stack trace to the log 
		 * @param e - The exception
		 * @param logging - Only write to the log if logging = true
		 */
		public static void printStackTrace(Exception e, bool logging)
		{
			if (logging)
				System.Console.Error.WriteLine(e.StackTrace);
		}

		/**
		 * Add a / to the end of a String if it not end with /
		 * @param str - The String
		 * @return - The String ending with /
		 */
		public static string addSlashIfNotEnding(string str)
		{
			if (str != null)
			{
				if (!str.EndsWith("/"))
					str += "/";
				//if (!str.Substring(str.Length - 1).ToLower().Equals("/"))
				//{
				//	str += "/";
				//}
			}
			return str;
		}

		/**
		 * Check a path if it is a valid existing directory
		 * @param path a path to test
		 * @return true if pat is a valid directory else false
		 */
		public static bool isPathAValidDirectory(string path)
		{
			if (path == null)
				return false;
			File file = new File(path);
			if (!file.IsDirectory)
				return false;
			else
				return true;
		}

		public static void toastAggregateException(Context context, AggregateException ae)
		{
			StringBuilder msg = new StringBuilder();
			ae.Handle((x) =>
			{
				if (x.InnerException is TimeoutException) // This we know how to handle.
					msg.AppendLine(context.GetString(Resource.String.TimeoutException));
				else if (x.InnerException is WebException)
					msg.AppendLine(context.GetString(Resource.String.WebException));
				else
					msg.AppendLine(context.GetString(Resource.String.AggregateException));

				return true;
			});
			toastMsg(context, msg.ToString());
		}
	}
}