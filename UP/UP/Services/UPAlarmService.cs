using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Java.Lang;
using System.Timers;
using Android.App;
using UP.Debug;

namespace UP.Services {

	[Service(Enabled = true, Exported = false, Label = "UP Background Service")]
	public class UPAlarmService : Service, IRunnable {
		
		public override IBinder OnBind(Intent intent) {
			return null;
		}

		private Handler serviceHandler;
		private bool serviceIsRunning = false;

		public override StartCommandResult OnStartCommand (Intent intent, StartCommandFlags flags, int startId) {
			Log.Debug(UPLog.TAG, "UP Alarm service started");
			
			base.OnStartCommand(intent, flags, startId);

			serviceHandler = new Handler();
			serviceHandler.PostDelayed(this, 2000);
			serviceIsRunning = true;
			
			//재실행시 인텐트 전달
			return StartCommandResult.Sticky ;
		}
		
		public void Run() {
			
			if (serviceIsRunning == false) {
				Log.Debug(UPLog.TAG, "UP service is not running.");
				return;
			} else {
				Log.Debug(UPLog.TAG, "UP is running");

				/// ... codes
				
				serviceHandler.PostDelayed(this, 2000);
				serviceIsRunning = true;
			}

		}
		

		public override void OnCreate() {
			unregisterRestartAlarm();
			base.OnCreate();
			serviceIsRunning = false;
		}

		public override void OnDestroy () {
			registerRestartAlarm();
			base.OnDestroy ();
			// cleanup code
			Log.Info("UP", "service Finishing");
			serviceIsRunning = false;

		}


		/**
     * 서비스가 시스템에 의해서 또는 강제적으로 종료되었을 때 호출되어
     * 알람을 등록해서 10초 후에 서비스가 실행되도록 한다.
     */
    private void registerRestartAlarm() {
 
        Log.Info("PersistentService", "registerRestartAlarm()");
 
        Intent intent = new Intent(this, typeof(BootReceiver));
        intent.SetAction(BootReceiver.ACTION_RESTART_UPALARMSERVICE);
        PendingIntent sender = PendingIntent.GetBroadcast(ApplicationContext, 0, intent, 0);
 
        long firstTime = SystemClock.ElapsedRealtime();
        firstTime += 1000; // 5초 후에 알람이벤트 발생
 
        AlarmManager am = (AlarmManager) GetSystemService(AlarmService);
        am.SetRepeating(AlarmType.ElapsedRealtimeWakeup, firstTime, 10000, sender);
    }
 
 
    /**
     * 기존 등록되어있는 알람을 해제한다.
     */
    private void unregisterRestartAlarm() {
 
        Log.Info("PersistentService", "unregisterRestartAlarm()");
        Intent intent = new Intent(this, typeof(BootReceiver));
        intent.SetAction(BootReceiver.ACTION_RESTART_UPALARMSERVICE);
        PendingIntent sender = PendingIntent.GetBroadcast(ApplicationContext, 0, intent, 0);
 
        AlarmManager am = (AlarmManager) GetSystemService(AlarmService);
        am.Cancel(sender);
    }

		
	}
}