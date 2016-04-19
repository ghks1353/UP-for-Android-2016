using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using Android.Graphics;
using Android.Content.Res;
using System.Timers;
using Java.Util;
using Java.Lang;

namespace UP {
	[Activity(Label = "UP", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity {

		RelativeLayout currentView = null;

		Bitmap[] digitalClockIndicators = new Bitmap[10];

		//디지털시계 숫자
		UIImage DigitalNumber0; UIImage DigitalNumber1; UIImage DigitalNumber2; UIImage DigitalNumber3;
		UIImage DigitalNumberCol;

		//Main update timer
		System.Timers.Timer mainUpdTimer;

		protected override void OnCreate(Bundle bundle) {
			base.OnCreate(bundle);
			currentView = new RelativeLayout(this); //좌표값으로 객체를 배치하는 레이아웃 방식
			RequestWindowFeature(WindowFeatures.NoTitle); //제목 표시줄의 삭제
			Window.AddFlags(WindowManagerFlags.TranslucentStatus); //상태바의 투명화
			currentView.SetBackgroundColor(Color.Black);

			//Device metris send
			DisplayMetrics dmetr = new DisplayMetrics();
			WindowManager.DefaultDisplay.GetMetrics(dmetr);

			//Set device factor
			DeviceGeneral.setContext(ApplicationContext);
			DeviceGeneral.initialDeviceSize( dmetr );
			
			//Add 0~9 images to array
			for (int i = 0; i < digitalClockIndicators.Length; ++i) {
				digitalClockIndicators[i] = ImageAssetManager.getBitmap( this, ImageAssetManager.IMAGE_ASSETS_NUMBERS_DIR + i.ToString() + ".png" );
			}

			//디지털시계 이미지 제작
			DigitalNumber0 = new UIImage(this); DigitalNumber1 = new UIImage(this);
			DigitalNumber2 = new UIImage(this); DigitalNumber3 = new UIImage(this); DigitalNumberCol = new UIImage(this);

			DigitalNumberCol.Image = ImageAssetManager.getBitmap( this, ImageAssetManager.IMAGE_ASSETS_NUMBERS_DIR + "col.png" );
			DigitalNumber0.Image = digitalClockIndicators[0]; DigitalNumber1.Image = digitalClockIndicators[0]; 
			DigitalNumber2.Image = digitalClockIndicators[0]; DigitalNumber3.Image = digitalClockIndicators[0]; 
			
			fitViewControllerElementsToScreen();


			//testImgView.Image = digitalClockIndicators[0];
			//testImgView.Scale = ImageView.ScaleType.FitXy;

			currentView.AddView(DigitalNumberCol);
			currentView.AddView(DigitalNumber0); currentView.AddView(DigitalNumber1); currentView.AddView(DigitalNumber2); currentView.AddView(DigitalNumber3);
			
			SetContentView(currentView); //컨텐츠 뷰를 무엇으로 할지 설정.

			//타이머 설정
			mainUpdTimer = new System.Timers.Timer(500);
			mainUpdTimer.Elapsed += Update;
			mainUpdTimer.AutoReset = true; mainUpdTimer.Enabled = true;

		} //end func
		
		//Main update
		public void Update(object source, ElapsedEventArgs e) {

			//Set time in Main
			Calendar c = Calendar.Instance;
			int hours = c.Get(CalendarField.HourOfDay);
			int minutes = c.Get(CalendarField.Minute);
			string hoursStr = hours.ToString();
			string minsStr = minutes.ToString();

			RunOnUiThread( delegate() { //Thread에서 UI를 업데이트하려면 이렇게 블럭 안에서 시도해야함개병맛.
				DigitalNumber0.Image = hoursStr.Length == 1 ? digitalClockIndicators[0] : digitalClockIndicators[int.Parse(hoursStr[0].ToString())];
				DigitalNumber1.Image = hoursStr.Length == 1 ? digitalClockIndicators[int.Parse(hoursStr[0].ToString())] : digitalClockIndicators[int.Parse(hoursStr[1].ToString())];

				DigitalNumber2.Image = minsStr.Length == 1 ? digitalClockIndicators[0] : digitalClockIndicators[int.Parse(minsStr[0].ToString())];
				DigitalNumber3.Image = minsStr.Length == 1 ? digitalClockIndicators[int.Parse(minsStr[0].ToString())] : digitalClockIndicators[int.Parse(minsStr[1].ToString())];

				DigitalNumberCol.Visibility = DigitalNumberCol.Visibility == ViewStates.Visible ? ViewStates.Invisible : ViewStates.Visible;
			});
			

		}
		

		//Element fit to screen 
		public void fitViewControllerElementsToScreen () {

			//버그로 인해 먼저 이미지 스케일부터 조절
			DigitalNumberCol.Frame = new CGRect(0, 0, 78.55 * DeviceGeneral.maxScrRatio, 110 * DeviceGeneral.maxScrRatio);

			double scrX = DeviceGeneral.scrSize.Width / 2 - (DigitalNumberCol.Width / 2);
			double digiClockYAxis = 120 * DeviceGeneral.scrRatio;
			//scrX += 4 * DeviceGeneral.maxscrRatio;
			Log.Info("loc", scrX.ToString());

			//디지털시계 이미지 스케일 조정
			DigitalNumberCol.Frame = new CGRect(scrX, digiClockYAxis, DigitalNumberCol.Width, DigitalNumberCol.Height);
			Log.Info("wid", DigitalNumberCol.Width.ToString());
			DigitalNumber0.Frame = new CGRect( 
				(DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) - DigitalNumberCol.Width * 2 - 36 * DeviceGeneral.maxScrRatio,
				DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height
				);
			DigitalNumber1.Frame = new CGRect( 
				(DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) - DigitalNumberCol.Width - 22 * DeviceGeneral.maxScrRatio,
				DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height
				);
			
			DigitalNumber2.Frame = new CGRect( 
					(DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) + 22 * DeviceGeneral.maxScrRatio,
					DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height
					);
			DigitalNumber3.Frame = new CGRect( 
				(DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) + DigitalNumberCol.Width + 36 * DeviceGeneral.maxScrRatio,
				DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height
				);




		}






	}
}

