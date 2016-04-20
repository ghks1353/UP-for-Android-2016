using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Preferences;
using Android.Util;
using UP.Debug;

namespace UP.Managers {
	public class UPDataManager {
		
		public static string KEY_ALARMS = "alarmListArray";
		
		////////////////////////

		public static ISharedPreferences upSharedPrefs;
		
		public static void initDataManager(Context context) {
			upSharedPrefs = PreferenceManager.GetDefaultSharedPreferences(context);
			
			Log.Debug(UPLog.TAG, "Datamanger inited");
		}

		public static void SaveDataJObject(string key, string jObject) {
			ISharedPreferencesEditor editor = upSharedPrefs.Edit();
			editor.PutString(key, jObject);
			editor.Commit(); //saved
		}
		public static string LoadDataJObject(string key) {
			return upSharedPrefs.GetString(key, "");
		}


	}
}