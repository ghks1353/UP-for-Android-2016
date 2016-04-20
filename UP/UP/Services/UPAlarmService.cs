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
			
			//������ ����Ʈ ����
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
     * ���񽺰� �ý��ۿ� ���ؼ� �Ǵ� ���������� ����Ǿ��� �� ȣ��Ǿ�
     * �˶��� ����ؼ� 10�� �Ŀ� ���񽺰� ����ǵ��� �Ѵ�.
     */
    private void registerRestartAlarm() {
 
        Log.Info("PersistentService", "registerRestartAlarm()");
 
        Intent intent = new Intent(this, typeof(BootReceiver));
        intent.SetAction(BootReceiver.ACTION_RESTART_UPALARMSERVICE);
        PendingIntent sender = PendingIntent.GetBroadcast(ApplicationContext, 0, intent, 0);
 
        long firstTime = SystemClock.ElapsedRealtime();
        firstTime += 1000; // 5�� �Ŀ� �˶��̺�Ʈ �߻�
 
        AlarmManager am = (AlarmManager) GetSystemService(AlarmService);
        am.SetRepeating(AlarmType.ElapsedRealtimeWakeup, firstTime, 10000, sender);
    }
 
 
    /**
     * ���� ��ϵǾ��ִ� �˶��� �����Ѵ�.
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