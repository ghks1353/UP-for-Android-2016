using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views.Animations;
using Android.Widget;

namespace UP {
	public class UIImage:ImageView {

		private Bitmap currentImageBitmap = null;
		private float currentRotation = 0;

		public UIImage(Context context): base(context) {
				

		}
		
		public Bitmap Image {
			set {
				if (currentImageBitmap == value) {
					//같을경우 업데이트 안함
				} else {
					currentImageBitmap = value;
					SetImageBitmap(value);
				}
			}
		} //value.image ~ 식으로 접근 가능하게 만듬. 근데 왜 get는 안되냐. 병맛.
		public ScaleType Scale {
			set {
				SetScaleType(value);
			}
		}
		/*public new float Rotation { //회전 시 사용
			set {
				/*if (currentRotation == value) {

				} else {
					Matrix mtx = new Matrix();
					mtx.PostRotate(value);
					Image = Bitmap.CreateBitmap(currentImageBitmap, 0, 0, currentImageBitmap.Width, currentImageBitmap.Height, mtx, true);
				}
				//RotateAnimation rotateAnim = new RotateAnimation(0.0f, value, Dimension.RelativeToSelf, 0.5f, Dimension.RelativeToSelf, 0.5f);
				//rotateAnim.Duration = 0; rotateAnim.FillAfter = true; StartAnimation(rotateAnim);
				

			}
			get {
				return currentRotation;
			}
		} */

		//Set frame
		public CGRect Frame {
			set {
				SetX( (int) value.X );
				SetY( (int) value.Y );
				LayoutParameters = new RelativeLayout.LayoutParams((int) value.Width, (int) value.Height);
				value = null; //gc
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