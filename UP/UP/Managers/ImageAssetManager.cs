using Android.Content;
using Android.Graphics;
using System.IO;

namespace UP {

	public class ImageAssetManager {
		
		public static string SKINS_DIR = "Skins/";
		public static string DEFAULT_SKIN = "Default/";

		public static string IMAGE_ASSETS_NUMBERS_DIR = "DigitalClock/";
		public static string IMAGE_ASSETS_ANALOG_DIR = "AnalogClock/";
		public static string IMAGE_ASSETS_BACKGROUNDS_DIR = "Backgrounds/";
		public static string IMAGE_ASSETS_GROUND_DIR = "GroundAssets/";

		//Bitmaps from assets. decodes.
		//Code ported java to C# by exfl.
		public static Bitmap getBitmap(Context context, string filePath) {
			Bitmap bitmap = null;

			Stream istr = context.Assets.Open(filePath);
			bitmap = BitmapFactory.DecodeStream(istr);

			return bitmap;
		}

	}

}