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
using UP.Services;
using UP.Managers;

namespace UP {
	[Activity(Label = "UP", MainLauncher = true, Icon = "@drawable/icon",
		ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
	public class MainActivity : Activity {

		bool AppStartedInit = false;

		RelativeLayout currentView = null;
		Bitmap[] digitalClockIndicators = new Bitmap[10];
		Bitmap[] backgroundImageBitmaps = new Bitmap[4]; //시간대별로 총 4개.

		//디지털시계 숫자
		UIImage DigitalNumber0; UIImage DigitalNumber1; UIImage DigitalNumber2; UIImage DigitalNumber3;
		UIImage DigitalNumberCol;

		//아날로그 시계
		UIImage AnalogBody; UIImage AnalogHours; UIImage AnalogMinutes;

		//땅 부분
		UIImage GroundObj;


		//뒷 배경 사진
		UIImage backgroundImage; 
		string currentBackgroundImage = "a"; //default background

		//Main update timer
		System.Timers.Timer mainUpdTimer;

		private void init() {
			//App initial function
			currentView = new RelativeLayout(this); //좌표값으로 객체를 배치하는 레이아웃 방식
			RequestWindowFeature(WindowFeatures.NoTitle); //제목 표시줄의 삭제
			Window.AddFlags(WindowManagerFlags.TranslucentStatus); //상태바의 투명화
			currentView.SetBackgroundColor(Color.Black);

			//Device metris send
			DisplayMetrics dmetr = new DisplayMetrics();
			WindowManager.DefaultDisplay.GetMetrics(dmetr);

			//Set device factor
			Dev.setContext(ApplicationContext);
			Dev.initialDeviceSize( dmetr );
			
			//Add 0~9 images to array
			for (int i = 0; i < digitalClockIndicators.Length; ++i) {
				digitalClockIndicators[i] = 
					ImageAssetManager.getBitmap( this, 
					ImageAssetManager.SKINS_DIR + ImageAssetManager.DEFAULT_SKIN + ImageAssetManager.IMAGE_ASSETS_NUMBERS_DIR + i.ToString() + ".png" );
			}

			//Add background images to array
			for (int i = 0; i < backgroundImageBitmaps.Length; ++i) {
				string filename = "";
				switch (i) {
					case 0: filename = "a"; break;
					case 1: filename = "b"; break;
					case 2: filename = "c"; break;
					case 3: filename = "d"; break;
				}
				filename += "_back"; //나중에 태블릿이나 기타 배율을 가진 디바이스가 있으면 고체

				backgroundImageBitmaps[i] = 
					ImageAssetManager.getBitmap( this, 
					ImageAssetManager.SKINS_DIR + ImageAssetManager.DEFAULT_SKIN + ImageAssetManager.IMAGE_ASSETS_BACKGROUNDS_DIR + filename + ".png" );
			} //end for

			//디지털시계 이미지 제작
			DigitalNumber0 = new UIImage(this); DigitalNumber1 = new UIImage(this);
			DigitalNumber2 = new UIImage(this); DigitalNumber3 = new UIImage(this); DigitalNumberCol = new UIImage(this);

			DigitalNumberCol.Image = ImageAssetManager.getBitmap( this, 
				ImageAssetManager.SKINS_DIR + ImageAssetManager.DEFAULT_SKIN + ImageAssetManager.IMAGE_ASSETS_NUMBERS_DIR + "col.png" );
			DigitalNumber0.Image = digitalClockIndicators[0]; DigitalNumber1.Image = digitalClockIndicators[0]; 
			DigitalNumber2.Image = digitalClockIndicators[0]; DigitalNumber3.Image = digitalClockIndicators[0]; 
			
			//아날로그 시계 제작
			AnalogBody = new UIImage(this); AnalogHours = new UIImage(this); AnalogMinutes = new UIImage(this);
			AnalogBody.Image = ImageAssetManager.getBitmap( this, 
				ImageAssetManager.SKINS_DIR + ImageAssetManager.DEFAULT_SKIN + ImageAssetManager.IMAGE_ASSETS_ANALOG_DIR + "time_body.png" );
			AnalogHours.Image = ImageAssetManager.getBitmap( this, 
				ImageAssetManager.SKINS_DIR + ImageAssetManager.DEFAULT_SKIN + ImageAssetManager.IMAGE_ASSETS_ANALOG_DIR + "time_hh.png" );
			AnalogMinutes.Image = ImageAssetManager.getBitmap( this, 
				ImageAssetManager.SKINS_DIR + ImageAssetManager.DEFAULT_SKIN + ImageAssetManager.IMAGE_ASSETS_ANALOG_DIR + "time_mh.png" );

			//땅 부분에 대한 제작
			GroundObj = new UIImage(this);
			GroundObj.Image = ImageAssetManager.getBitmap( this, 
				ImageAssetManager.SKINS_DIR + ImageAssetManager.DEFAULT_SKIN + ImageAssetManager.IMAGE_ASSETS_GROUND_DIR + "ground.png" );
			GroundObj.Scale = ImageView.ScaleType.FitXy;

			//뒷 배경 제작
			backgroundImage = new UIImage(this);
			backgroundImage.Image = backgroundImageBitmaps[0];
			backgroundImage.Scale = ImageView.ScaleType.CenterCrop;
			currentView.AddView( backgroundImage ); //여기서 먼저 생성하여 element의 뒤로 가게 해야함

			fitViewControllerElementsToScreen();
			
			//testImgView.Image = digitalClockIndicators[0];
			//testImgView.Scale = ImageView.ScaleType.FitXy;

			currentView.AddView(DigitalNumberCol);
			currentView.AddView(DigitalNumber0); currentView.AddView(DigitalNumber1); currentView.AddView(DigitalNumber2); currentView.AddView(DigitalNumber3);
			currentView.AddView(AnalogBody); currentView.AddView(AnalogHours); currentView.AddView(AnalogMinutes);

			currentView.AddView(GroundObj);

			SetContentView(currentView); //컨텐츠 뷰를 무엇으로 할지 설정.

			//타이머 설정
			mainUpdTimer = new System.Timers.Timer(500);
			mainUpdTimer.Elapsed += Update;
			mainUpdTimer.AutoReset = true; mainUpdTimer.Enabled = true;

			Update(null, null); //타이머 업데이트 전에 한번 실행

			//서비스 중지 후 시작
			StopService( new Intent(this, typeof(UPAlarmService)) );
			StartService( new Intent(this, typeof(UPAlarmService)) );

			//init off
			AppStartedInit = true;
		} //end func

		public override void OnConfigurationChanged(Configuration newConfig) {
			base.OnConfigurationChanged(newConfig);
			//회전 등으로인한 액티비티 변경시

			DisplayMetrics dmetr = new DisplayMetrics();
			WindowManager.DefaultDisplay.GetMetrics(dmetr);
			Dev.initialDeviceSize( dmetr );
			fitViewControllerElementsToScreen();
		}

		protected override void OnCreate(Bundle bundle) {
			base.OnCreate(bundle);

			Log.Info("UP", "Oncreate activated. inited:", AppStartedInit.ToString());
			UPDataManager.initDataManager(this); //dbmanager init
			UPAlarmManager.MergeAlarm(); //실행 첫 merge 실행
			init();
			
		} //end func

		protected override void OnStart () {
            base.OnStart ();


		}
		protected override void OnDestroy () {
            base.OnDestroy ();
			
        }

		
		//Main update
		public void Update(object source, ElapsedEventArgs e) {
			// * 중요 * 이 함수는 백그라운드에서도 작동하므로 백그라운드에선 무시하게 만들어야 함

			
			//Set time in Main
			Calendar c = Calendar.Instance;
			int hours = c.Get(CalendarField.HourOfDay);
			int minutes = c.Get(CalendarField.Minute);
			string hoursStr = hours.ToString();
			string minsStr = minutes.ToString();

			RunOnUiThread( delegate() { //Thread에서 UI를 업데이트하려면 이렇게 블럭 안에서 시도해야함개병맛.
				
				//////// 숫자 1이 나타났을 때에 대한 처리. 우선 시간부터
				if (hoursStr.Length == 1) {
					DigitalNumber0.Image = digitalClockIndicators[0];
					DigitalNumber1.Image = digitalClockIndicators[int.Parse(hoursStr[0].ToString())];

					if (hoursStr[0] == '1') {
						//오른쪽으로 당김
						DigitalNumber0.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) - DigitalNumberCol.Width * 2 - Dev.CvD(30) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );
						DigitalNumber1.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) - DigitalNumberCol.Width - Dev.CvD(16) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );

					} else {
						//원래 위치로
						DigitalNumber0.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) - DigitalNumberCol.Width * 2 - Dev.CvD(36) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );
						DigitalNumber1.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) - DigitalNumberCol.Width - Dev.CvD(22) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );

					} //end if
				} else { //첫자리 밑 둘째자리는 각 시간에 맞게
					DigitalNumber0.Image = digitalClockIndicators[int.Parse(hoursStr[0].ToString())];
					DigitalNumber1.Image = digitalClockIndicators[int.Parse(hoursStr[1].ToString())];

					double movesRightOffset = 0;
					if (hoursStr[0] == '1') {
						//오른쪽으로 당김
						movesRightOffset += 6;
					}
					if (hoursStr[1] == '1') {
						//가능한 경우 최대 두번 당김
						DigitalNumber0.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) - DigitalNumberCol.Width * 2 - Dev.CvD(24 - movesRightOffset) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );
						movesRightOffset += 6;
						DigitalNumber1.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) - DigitalNumberCol.Width - Dev.CvD(10 - movesRightOffset) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );
					} else {
						DigitalNumber0.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) - DigitalNumberCol.Width * 2 - Dev.CvD(36 - movesRightOffset) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );
						DigitalNumber1.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) - DigitalNumberCol.Width - Dev.CvD(22) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );

					}
				} //end if of hours

				//그다음, 분 처리
				if (minsStr.Length == 1) {
					DigitalNumber2.Image = digitalClockIndicators[0];
					DigitalNumber3.Image = digitalClockIndicators[int.Parse(minsStr[0].ToString())];

					if (minsStr[0] == '1') {
						//숫자1의경우 왼쪽으로 당김.
						DigitalNumber2.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) + Dev.CvD(22) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );
						DigitalNumber3.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) + DigitalNumberCol.Width + Dev.CvD(30) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );
					} else {
						//원래 위치로
						DigitalNumber2.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) + Dev.CvD(22) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );
						DigitalNumber3.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) + DigitalNumberCol.Width + Dev.CvD(36) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );
					}
				} else { //첫자리 밑 둘째자리는 각 시간에 맞게
					DigitalNumber2.Image = digitalClockIndicators[int.Parse(minsStr[0].ToString())];
					DigitalNumber3.Image = digitalClockIndicators[int.Parse(minsStr[1].ToString())];

					double movesLeftOffset = 0;
					if (minsStr[1] == '1') {
						//가능한 경우 최대 두번 당김
						movesLeftOffset += 6;
					}
					if (minsStr[0] == '1') {
						//왼쪽으로 당김
						DigitalNumber2.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) + Dev.CvD(16) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );
						movesLeftOffset += 6;
						DigitalNumber3.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) + DigitalNumberCol.Width + Dev.CvD(30 - movesLeftOffset) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );
					} else {
						DigitalNumber2.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) + Dev.CvD(22) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );
						DigitalNumber3.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) + DigitalNumberCol.Width + Dev.CvD(36 - movesLeftOffset) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );
					}

				} //end if

				// : 깜빡임 효과 및 시침 분침 회전
				if (DigitalNumberCol.Visibility == ViewStates.Visible) {
					DigitalNumberCol.Visibility = ViewStates.Invisible;
					//1초 주기 실행을 위해
					float secondMovement = minutes / 60.0f / 12.0f;
					
					AnalogHours.Rotation = (((hours / 12.0f) + secondMovement) * 360.0f); 
					AnalogMinutes.Rotation = ((minutes / 60.0f) * 360.0f); 
					
				} else {
					DigitalNumberCol.Visibility = ViewStates.Visible;
				}

				//시간대에 따른 배경 이미지
				backgroundImage.Image = backgroundImageBitmaps[ getBackgroundFileNumFromTime(hours) ];
			});
			

		} //end update.

		//get str from time
		private int getBackgroundFileNumFromTime(int timeHour) {
			if (timeHour >= 0 && timeHour < 6) {
				return 3; // d
			} else if (timeHour >= 6 && timeHour < 12) {
				return 0; // a
			} else if (timeHour >= 12 && timeHour < 18) {
				return 1; // b
			} else if (timeHour >= 18 && timeHour <= 23) {
				return 2; // c
			}
			return 0; //a
		}
		

		//Element fit to screen 
		public void fitViewControllerElementsToScreen () {

			//버그로 인해 먼저 이미지 스케일부터 조절
			DigitalNumberCol.Frame = new CGRect(0, 0, Dev.CvD(78.55) * Dev.maxScrRatio, Dev.CvD(110) * Dev.maxScrRatio);

			double scrX = (Dev.scrSize.Width * Dev.deviceDensity) / 2 - (DigitalNumberCol.Width / 2);
			double digiClockYAxis = Dev.CvD(170) * Dev.maxScrRatio;
			//scrX += 4 * Dev.maxscrRatio;
			Log.Info("loc", scrX.ToString());

			//디지털시계 이미지 스케일 조정
			DigitalNumberCol.Frame = new CGRect(scrX, digiClockYAxis, DigitalNumberCol.Width, DigitalNumberCol.Height);
			Log.Info("wid", DigitalNumberCol.Width.ToString());
			DigitalNumber0.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) - DigitalNumberCol.Width * 2 - Dev.CvD(36) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );
			DigitalNumber1.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) - DigitalNumberCol.Width - Dev.CvD(22) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );
			
			DigitalNumber2.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) + Dev.CvD(22) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );
			DigitalNumber3.Frame = new CGRect( (DigitalNumberCol.X + (DigitalNumberCol.Width / 2)) + DigitalNumberCol.Width + Dev.CvD(36) * Dev.maxScrRatio, DigitalNumberCol.Y, DigitalNumberCol.Width, DigitalNumberCol.Height );

			double clockScrX = Dev.CvD(Dev.scrSize.Width) / 2 - ( Dev.CvD(450) * Dev.maxScrRatio ) / 2;
			double clockScrY = Dev.CvD(Dev.scrSize.Height) / 2 - ( Dev.CvD(450) * Dev.maxScrRatio ) / 2;
			//아날로그시계 조정
			AnalogBody.Frame = new CGRect(clockScrX, clockScrY, Dev.CvD(450) * Dev.maxScrRatio, Dev.CvD(450) * Dev.maxScrRatio);
			AnalogHours.Frame = new CGRect( clockScrX, clockScrY, AnalogBody.Width, AnalogBody.Height );
			AnalogMinutes.Frame = AnalogHours.Frame;

			//땅 조정
			GroundObj.Frame = new CGRect( 0, Dev.CvD(Dev.scrSize.Height) - Dev.CvD(131 - 2) * Dev.maxScrRatio, Dev.CvD(Dev.scrSize.Width), Dev.CvD(131) * Dev.maxScrRatio );

			//백그라운드 크기 조절
			backgroundImage.Frame = new CGRect(0, 0, Dev.CvD(Dev.scrSize.Width), Dev.CvD(Dev.scrSize.Height));


		} //end func






	}
}

