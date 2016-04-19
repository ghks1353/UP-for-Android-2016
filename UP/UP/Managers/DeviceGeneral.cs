
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Util;
using System;

namespace UP {
	public class DeviceGeneral {
		
		//Handling context
		static Context currentContext = null;

		//기기 해상도 bounds
		public static CGRect scrSize = null;
		static CGRect scrSizeForCalcuate = null;

		//기준 해상도 (기기: Galaxy Note 5, Nexus 6P / Res: 1440x2560 / xxhdpi.)
		static CGRect workSize = new CGRect(0, 0, 720, 1280); //이 수치는 해상도의 수치가 아닌 density를 반영한 수치임

		//기준에 대한 비율
		public static double scrRatio = 1; public static double maxScrRatio = 1;

		//낮은 해상도 사용
		public static bool usesLowQualityImage = false;
		//평균 Modal size
		public static CGRect defaultModalSizeRect = null;

		public static float deviceDensity = 1;

		public static void initialDeviceSize( DisplayMetrics metrics ) {
			deviceDensity = metrics.Density; //Android density

			//화면 사이즈를 얻어옴.
			//scrSize = new CGRect( 0, 0, ConvertPixelsToDp(metrics.WidthPixels, metrics), ConvertPixelsToDp(metrics.HeightPixels, metrics) );
			scrSize = new CGRect( 0, 0, metrics.WidthPixels, metrics.HeightPixels );
			
			scrSizeForCalcuate = scrSize;

			//가로로 init되는 경우, 세로로init되게 함. (왜냐면 디자인은 다 세로의 배율 기준이기 때문임)
			if ( scrSize.Width > scrSize.Height ) {
				scrSizeForCalcuate = new CGRect( 0, 0, scrSize.Height, scrSize.Width );
			}
			scrRatio = scrSizeForCalcuate.Width / workSize.Width;

			Log.Info("Device", "rectsize:" + scrSize.Width.ToString() + ", " + scrSize.Height.ToString() + ", ratio:" + scrRatio.ToString());

			//패드에서 거하게 커지는 현상방지
			maxScrRatio = Math.Min(1, scrRatio);
			//저퀄리티 사진사용 체크 (안드로이드 전용 값으로 다시 체크해야 할 필요 있음)
			usesLowQualityImage = scrSizeForCalcuate.Width <= 200 ? true : false;
			
			changeModalSize();
			
		}

		public static void setContext(Context context) {
			currentContext = context;
		}

		public static void changeModalSize() {

			if (isTablet()) {
				//패드의 경우, 크기를 미리 지정해줌
				defaultModalSizeRect = new CGRect( scrSize.Width - 320 / 2, scrSize.Height - 480 / 2, 320, 480 );
			} else {
				//기타 (폰)의 경우
				defaultModalSizeRect = new CGRect( 50 * scrRatio, (scrSizeForCalcuate.Height - (480 * scrRatio)) / 2, scrSizeForCalcuate.Width - (100 * scrRatio), 480 * scrRatio );
			}
			
			Log.Info("Device", "Modal size changed to width " + defaultModalSizeRect.Width.ToString() + " height " + defaultModalSizeRect.Height.ToString());
		}
		
		///// utils
		public static bool isTablet() {
			return (currentContext.Resources.Configuration.ScreenLayout & ScreenLayout.SizeMask)
				>= ScreenLayout.SizeLarge;
		}

		private static int ConvertPixelsToDp(float pixelValue, DisplayMetrics metrics ) {
			var dp = (int) ((pixelValue) / metrics.Density);
			return dp;
		}
		


	}


}