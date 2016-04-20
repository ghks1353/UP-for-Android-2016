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
using Android.Util;

namespace UP.Services {

	[BroadcastReceiver(Enabled = true, Exported = true, Process = ".remote", Permission = "android.permission.RECEIVE_BOOT_COMPLETED")]
	[IntentFilter(new[] { Intent.ActionBootCompleted, "ACTION.RESTART.UPALARMSERVICE" })]
	class BootReceiver:BroadcastReceiver {

		public static string ACTION_RESTART_UPALARMSERVICE 
                                   = "ACTION.RESTART.UPALARMSERVICE";


		public override void OnReceive(Context context, Intent intent) {

			Log.Info("RestartService", "doing restart");
			 /* 서비스 죽일때 알람으로 다시 서비스 등록 */
			if (intent.Action == ACTION_RESTART_UPALARMSERVICE) {
 
				Log.Info("RestartService", "Service dead, but resurrection");
 
				Intent i = new Intent(context, typeof(UPAlarmService));
				context.StartService(i);
			}
 
			/* 폰 재부팅할때 서비스 등록 */
			if (intent.Action == Intent.ActionBootCompleted || intent.Action == "android.intent.action.QUICKBOOT_POWERON") {
 
				Log.Info("RestartService", "ACTION_BOOT_COMPLETED");
 
				Intent i = new Intent(context, typeof(UPAlarmService));
				context.StartService(i);
			}

		}
		
	}
}