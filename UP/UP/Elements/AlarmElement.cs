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

namespace UP.Elements {
	public class AlarmElement {

		public string alarmName = "";
		public int gameSelected = 0;
		public int alarmID = 0;
		public bool[] alarmRepeat = new bool[7] { false, false, false, false, false, false, false };
		public string alarmSound = "";
		public int alarmFireDate = 0; //Timestamp. Android는 밀리세컨드까지 포함하는 관계로 그대로 저장하고 사용할때만 1000을 나눠서 쓰자

		//alarm on-off toggle
		public bool alarmToggle = false;
	
		//Game clear check bool
		public bool alarmCleared = false; //false인 경우, merge 대상에서 빠짐.

		//기본적으로 이 오브젝트 속성에 대해선 JSON으로 저장되기 때문에 swift의 nsdata 형식 변환은 필요없음
		public AlarmElement( string name, int game, bool[] repeats, string sound, int alarmDate, bool alarmTool, int id ) {
			alarmName = name; gameSelected = game; alarmRepeat = repeats;
			alarmSound = sound; alarmFireDate = alarmDate;
			alarmToggle = alarmTool; alarmID = id;
			alarmCleared = false; //Default game clear toggle is false.
		}

	}
}