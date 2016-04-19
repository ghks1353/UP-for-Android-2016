using Android.Content;
using Android.Graphics;
using System.IO;

namespace UP {

	public class ImageAssetManager {
		
		public static string IMAGE_ASSETS_NUMBERS_DIR = "Numbers/";

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