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
		public int alarmFireDate = 0; //Timestamp. Android�� �и���������� �����ϴ� ����� �״�� �����ϰ� ����Ҷ��� 1000�� ������ ����

		//alarm on-off toggle
		public bool alarmToggle = false;
	
		//Game clear check bool
		public bool alarmCleared = false; //false�� ���, merge ��󿡼� ����.

		//�⺻������ �� ������Ʈ �Ӽ��� ���ؼ� JSON���� ����Ǳ� ������ swift�� nsdata ���� ��ȯ�� �ʿ����
		public AlarmElement( string name, int game, bool[] repeats, string sound, int alarmDate, bool alarmTool, int id ) {
			alarmName = name; gameSelected = game; alarmRepeat = repeats;
			alarmSound = sound; alarmFireDate = alarmDate;
			alarmToggle = alarmTool; alarmID = id;
			alarmCleared = false; //Default game clear toggle is false.
		}

	}
}