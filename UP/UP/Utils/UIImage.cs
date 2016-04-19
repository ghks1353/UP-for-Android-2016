using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Widget;

namespace UP {
	public class UIImage:ImageView {

		public UIImage(Context context): base(context) {
			
		}
		
		public Bitmap Image {
			set {
				SetImageBitmap(value);
			}
		} //value.image ~ 식으로 접근 가능하게 만듬. 근데 왜 get는 안되냐. 병맛.
		public ScaleType Scale {
			set {
				SetScaleType(value);
			}
		}

		//Set frame
		public CGRect Frame {
			set {
				SetX( (int) value.X );
				SetY( (int) value.Y );
				LayoutParameters = new RelativeLayout.LayoutParams((int) value.Width, (int) value.Height);
			}
			get {
				return new CGRect(GetX(), GetY(), LayoutParameters.Width, LayoutParameters.Height );
			}
		} //swift의 CGRect / Frame과 같이 제작가능하게 만듬.

		public new int Width {
			get {
				return LayoutParameters == null ? 0 : LayoutParameters.Width;
			}
		} //end w
		public new int Height {
			get {
				return LayoutParameters == null ? 0 : LayoutParameters.Height;
			}
		} //end h
		public new float X {
			get {
				return GetX();
			}
			set {
				SetX(value);
			}
		} //end x
		public new float Y {
			get {
				return GetY();
			}
			set {
				SetY(value);
			}
		} //end x

		
	} //end class

}