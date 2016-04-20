using System.Collections.Generic;
using UP.Elements;
using Java.Util;
using Android.Util;
using Newtonsoft.Json;
using UP.Debug;

namespace UP.Managers {
	public class UPAlarmManager {
		/*
			LocalNotification�� �̿��� �˶� ���� �� ����, ���� �˶� �ݺ� ���� ó��
			�˶� ������ ��ȭ��������� �ݺ��� ���� ������ userInfo�� ������.
			�� ���� / �˶� �߰� / ���� �� ���� ���� Ȯ��.
				- ���� �ݺ��� �ʿ����� üũ��
				- �ʿ��� ��� ��¥�� ���ؼ� (���� �˶��� ���ؼ���) �˶��� ������
				- �ݺ��� ������ ����Ʈ�� off���·� ��. �˶������ ����
				- �������� �˶����� �ƴ����� �ݺ��� �ִ°�� �� ���� ���ƿö����� �ϸ� �߰���
		*/
		public static List<AlarmElement> alarmsArray;
		public static bool isAlarmMergedFirst = false; //ùȸ merge üũ��
		public static int alarmMaxRegisterCount = 20; //�˶� �ִ� ��� ���� ����
	
		public static bool alarmRingActivated = false; //�˶� ��Ƽ��Ƽ.. �ƴ� �䰡 �߰� ���� �� true. (���� ���� ���� ��.)
		public static bool alarmSoundPlaying = false;

		// ~~ �˶� �︲�� �˶� ������ ���� �ٸ� ������ �����ϰų� �浵�� ã�ƺ����. ~~

		//�˶��� �����ִ� �� ���� ��� ���� �˶��� �︱ �ð��� ������
		public static int GetNextAlarmFireInSeconds() {
			//Merge�� �� ����Ǿ� ��
			if (!isAlarmMergedFirst) {
				MergeAlarm();
			} //merge first
		
			int alarmNextFireDate = -1;
			for (int i = 0; i < alarmsArray.Count; ++i) {
				if (alarmsArray[i].alarmToggle == false) {
					continue;
				} //ignores off
				if (alarmNextFireDate == -1 || alarmNextFireDate > alarmsArray[i].alarmFireDate) {
					//���� �ð� �켱���� ����
					alarmNextFireDate = alarmsArray[i].alarmFireDate;
				}
			}
		
			return alarmNextFireDate; //-1�� ������ ���, �����ִ� �˶��� ����
		} //end func

		//�︮�� �ִ� �˶��� ������. �������� ���, ù��° �˶��� ������
		public static AlarmElement GetRingingAlarm() {
			//���߿� ���� Ŭ���� �� �˶��� �� ��, �︮�� �ִ� �˶� ��ü�� �� �� �ְ� �ؾߵ�
			//(�׷��� ������ �︰ �˶���ŭ ������ ���ߵ�)
		
			//���� ���� �������϶� �� ��ü�� ���� �������̶�� üũ�� �ʿ��ϸ� �׷��� ���� ���
			//�����ϴٰ� ������ �����ٿԴµ� �˶� �︲ȭ������ �ٽ�..
			int currentDateInMilliSeconds = (int) Calendar.Instance.TimeInMillis;
			for (int i = 0; i < alarmsArray.Count; ++i) { 
				if (alarmsArray[i].alarmToggle == false) {
					continue;
				} //ignores off
			
				if (alarmsArray[i].alarmFireDate <= currentDateInMilliSeconds
					&& alarmsArray[i].alarmCleared == false) {
					/*  1. fired�� �˶��� ��.
						2. Ŭ��� ������ ��.
						3. toggled�� �˶��� ��. */
						return alarmsArray[i];
				}
			} //end for
			
			return null; //Element ����
		}

		//�˶� ���� Ŭ���� ���
		public static void GameClearToggleAlarm( int alarmID, bool cleared ) {
			AlarmElement modAlarmElement = GetAlarm(alarmID);
			modAlarmElement.alarmCleared = cleared;
		
			//save it
			//TODO 20160421 ������ ����
			UPDataManager.SaveDataJObject(UPDataManager.KEY_ALARMS, JsonConvert.SerializeObject(alarmsArray, Formatting.Indented));

			Log.Debug(UPLog.TAG, "Alarm clear toggle to ... " + cleared.ToString() + " to id " + alarmID.ToString());
		}
	
		public static bool CheckRegisterAlarm() {
			if (!isAlarmMergedFirst) {
				MergeAlarm();
			} //merge first
		
			if (alarmsArray.Count < alarmMaxRegisterCount) {
				return true;
			}
			return false;
		}

		public static void MergeAlarm() {
			//�����ٵ� �˶��� �����ͼ� �����͵� merge�ϰ�, �߻��� �� �ִ� ������ ���ؼ� üũ��
			
			int currentTime = (int) Calendar.Instance.TimeInMillis;
			int todayWeekday = Calendar.Instance.Get(CalendarField.DayOfWeek);

			Log.Debug(UPLog.TAG, "TODAY WEEKDAY IS " + todayWeekday.ToString());

			//TODO: ����� �˶��� �ϳ��� ���� ���, ������ �� �� �ִ� key�� ����� �� �迭�� �־��ش�.
			//���� ���, �迭 alarmsArray�� �޾ƿ´�.

			string loadedAlarmsArr = UPDataManager.LoadDataJObject( UPDataManager.KEY_ALARMS );
			alarmsArray = loadedAlarmsArr == "" ? new List<AlarmElement>() : JsonConvert.DeserializeObject<List<AlarmElement>>( loadedAlarmsArr );
			
			//UPDataManager.SaveDataJObject(UPDataManager.KEY_ALARMS, JsonConvert.SerializeObject(alarmsArray, Formatting.Indented));
			
			//Notification�� ������ �ȵ���̵忡�� �ʿ� �����Ƿ� ������

			Log.Debug(UPLog.TAG, "Scheduled alarm count" + alarmsArray.Count.ToString());
			for (int i = 0; i < alarmsArray.Count; ++i) { 
				//TODO: ���尡 ���� �Ϳ� ���� �� ���带 �⺻������Ʈ�� �ٲ����
				//������ ���� ����Ŵ����� �������� �ʾ����Ƿ�, �ϴ��� ����. 20160421. �۾��� todo �����ٶ�
				
				//�� ������, Toggle on�Ȱ� ������θ� �˻�
				if (alarmsArray[i].alarmToggle == false) {
					Log.Debug(UPLog.TAG, "Scheduled alarm" + alarmsArray[i].alarmID.ToString() + " state off. skipping");
					continue;
				}
				Log.Debug(UPLog.TAG, "alarm id " + alarmsArray[i].alarmID.ToString() + " firedate (millisec)" + alarmsArray[i].alarmFireDate);

				if (alarmsArray[i].alarmFireDate <= currentTime
					&& alarmsArray[i].alarmCleared == true ) { /* �ð��� �����, ������ Ŭ���� �ؾߵ�. ���� Ŭ����� true�� ������ merge �ѹ��� �ϸ�� */
					Log.Debug(UPLog.TAG, "Merge start " + alarmsArray[i].alarmID.ToString());
					//Repeat ����� �ִ��� üũ

					//1. ������ ������ ����. 2. ���� ��¥ �˶� üũ. 3. ��¥��ŭ ����.
					//��, ���ó�¥�� �ƴ϶� ������¥�� ����ؾ���. (�ֳĸ� ������ ������ϱ�.)
					int nextAlarmVaild = -1;
					for (int k = ( todayWeekday == 7 ? 0 : todayWeekday /* ������¥���� */ ); k < alarmsArray[i].alarmRepeat.Length; ++k) {
						//������(�����)���� ������ üũ
						nextAlarmVaild = alarmsArray[i].alarmRepeat[k] == true ? k : nextAlarmVaild;
						if (alarmsArray[i].alarmRepeat[k] == true) { break; }
					}
					if (todayWeekday != 7 && nextAlarmVaild == -1) { //ã�� �� ���°�� �տ������� �ٽ� �˻�
						//������� �����ϴ� ����: ������� �̹� �Ͽ��Ϻ��� �ٽ� ���� ����.
						for (int k = 0; k < alarmsArray[i].alarmRepeat.Length; ++k) {
							nextAlarmVaild = alarmsArray[i].alarmRepeat[k] == true ? k : nextAlarmVaild;
							if (alarmsArray[i].alarmRepeat[k] == true) { break; }
						}
					}
					Log.Debug(UPLog.TAG, "Next alarm day (0=sunday) " + nextAlarmVaild.ToString());
					alarmsArray[i].alarmCleared = false; // ����Ŭ���� ����
					
					//2
					//���� �˶� ��¥�� �˶� �߰�. (���� ���̳����� ���ؼ� day�� �����ָ��. ������ �߰����ϰ� �������)
					if (nextAlarmVaild == -1) {
						//�ݺ� ���� ��� �˶� ��� ����
						alarmsArray[i].alarmToggle = false;
						Log.Debug(UPLog.TAG, "Alarm toggle finished (no-repeat alarm)");
					} else {
						//�ݺ��� ��� ���� �ݺ��� ���
						Log.Debug(UPLog.TAG, "Alarm toggle will repeat");
						int fireAfterDay = 0;
						if (nextAlarmVaild - (todayWeekday - 1) > 0) {
							fireAfterDay = nextAlarmVaild - (todayWeekday - 1);
							Log.Debug(UPLog.TAG, "Firedate is over today: " + fireAfterDay.ToString());
						} else {
							fireAfterDay = (7 - (todayWeekday - 1)) + nextAlarmVaild;
							Log.Debug(UPLog.TAG, "Firedate is before today: " + fireAfterDay.ToString());
						}
					
						//alarmdate add
						Calendar tmpC = Calendar.Instance; tmpC.TimeInMillis = alarmsArray[i].alarmFireDate;
						tmpC.Add(CalendarField.Date, fireAfterDay);
						alarmsArray[i].alarmFireDate = (int) tmpC.TimeInMillis; //add result
					
						//LocalNotifi�� �ȵ���̵忡�� �ʿ䰡 ������ �����ϰ� ����

						//add new push for next alarm
						Log.Debug(UPLog.TAG, "Alarm added successfully.");
					
					} //end vaild chk
				
				//alarm merge check if end
				} else {
					//�˶��� ����������, �ð��� ������ �ʾҰų� ������ Ŭ�������� ���� ���
					Log.Debug(UPLog.TAG, "Alarm is on but not cleared (or not passed), id" + alarmsArray[i].alarmID.ToString() );
				
				}
			
			} //for end
			Log.Debug(UPLog.TAG, "Merge is done. time to save!");

			//todo 20160421 �����ؾ���
			UPDataManager.SaveDataJObject(UPDataManager.KEY_ALARMS, JsonConvert.SerializeObject(alarmsArray, Formatting.Indented));
			
			//Badge ǥ�ÿ�
			
			//�ȵ���̵嵵 ������ �����ϱ� ������, �̰� ��ó���� �ٸ����� Ư�� �÷��׸� �־���� �ϴ� �κ��� �־� ����
		
			isAlarmMergedFirst = true;
		} //merge end

		//Clear alarm all (for debug?)
		public static void ClearAlarm() {
			Log.Debug(UPLog.TAG, "Clearing saved alarm");
			alarmsArray.RemoveRange(0, alarmsArray.Count);

			//TODO: save to system
		}
	
		//Find alarm from array by ID
		public static AlarmElement GetAlarm(int alarmID) {
			for (int i = 0; i < alarmsArray.Count; ++i) {
				if (alarmsArray[i].alarmID == alarmID) {
					return alarmsArray[i];
				}
			}
		
			return null;
		}

		//Toggle alarm (on/off)
		public static void ToggleAlarm(int alarmID, bool alarmStatus, bool isListOn = false) {
			//- �˶��� �����ִ� ���¿��� �� ���, LocalNotification�� ���� ����
			//- �˶��� �����ִ� ���¿��� ų ���, ��Ȳ�� ���� (�ݺ�üũ��) LocalNotification �߰�
			if (!isAlarmMergedFirst) {
				MergeAlarm();
			} //merge first
			
			for (int i = 0; i < alarmsArray.Count; ++i) {
				if (alarmsArray[i].alarmID == alarmID) { //target found
					Log.Debug(UPLog.TAG, "Toggling. target:" + alarmID.ToString());

					if (alarmsArray[i].alarmToggle == alarmStatus) {
						Log.Debug(UPLog.TAG, "status already same..!!");
						break; //���°� �����Ƿ� ������ �ʿ� ����
					}
					
					//Notifi ���� �ʿ� ����

					if (alarmStatus == false) { //�˶� ����
						alarmsArray[i].alarmToggle = false; //alarm toggle to off.
					} else {
						//�˶� �ѱ� (addalarm ����)
						Calendar tmpC = Calendar.Instance;
						Calendar oldC = Calendar.Instance; oldC.TimeInMillis = alarmsArray[i].alarmFireDate;
						tmpC.Set(CalendarField.Hour, oldC.Get(CalendarField.Hour)); //move hour and min from old date
						tmpC.Set(CalendarField.Minute, oldC.Get(CalendarField.Minute));
						tmpC.Set(CalendarField.Second, 0); //remove second
						
						alarmsArray[i].alarmFireDate = (int) tmpC.TimeInMillis;
						Log.Debug(UPLog.TAG, "Comp changed date to", alarmsArray[i].alarmFireDate.ToString());
						
						AlarmElement alarmsArrTmpPointer = alarmsArray[i];
						alarmsArray.RemoveAt(i);
						AddAlarm(alarmsArrTmpPointer.alarmFireDate, alarmsArrTmpPointer.alarmName,
							alarmsArrTmpPointer.gameSelected, alarmsArrTmpPointer.alarmSound,
							alarmsArrTmpPointer.alarmRepeat, i, alarmsArrTmpPointer.alarmID,
							!isListOn);
					
						//return; //�ѹ��� �����ؾߵ�.
						break; //save
					} //end status
					break; //�ش� ID�� ó�������Ƿ� ���������� ������ ���ǹ�
				} //end alarmid target search
			} //end for
		
			//save it
			Log.Debug(UPLog.TAG, "Status change saving");

			//TODO: �ý��� ����
			UPDataManager.SaveDataJObject(UPDataManager.KEY_ALARMS, JsonConvert.SerializeObject(alarmsArray, Formatting.Indented));
		} //end func

		//Remove alarm from system
		public static void RemoveAlarm(int alarmID) {
			if (!isAlarmMergedFirst) {
				MergeAlarm();
			} //merge first
		
			//�ý��� ��Ƽ�� �ʿ�����Ƿ� ���� ����
			
			for (int i = 0; i < alarmsArray.Count; ++i) {
				if (alarmsArray[i].alarmID == alarmID) {
					alarmsArray.RemoveAt(i);
					break;
				}
			} //remove item from array

			//save it
			Log.Debug(UPLog.TAG, "Alarm removed from system. saving");

			//TODO: �ý��ۿ� ����
			UPDataManager.SaveDataJObject(UPDataManager.KEY_ALARMS, JsonConvert.SerializeObject(alarmsArray, Formatting.Indented));
		}

		//Edit alarm from system
		public static void EditAlarm(int alarmID, int funcDate, string alarmTitle, int gameID, string soundFile, bool[] repeatArr, bool toggleStatus) {
			Calendar date =  Calendar.Instance;
			date.TimeInMillis = funcDate;

			if (!isAlarmMergedFirst) {
				MergeAlarm();
			} //merge first
		
			int alarmArrayIndex = 0;
			
			//Notification ���� �ڵ尡 �־����� �̰� ���� �ȵ���̵忡�� ������

			for (int i = 0; i < alarmsArray.Count; ++i) {
				if (alarmsArray[i].alarmID == alarmID) {
					alarmsArray.RemoveAt(i);
					alarmArrayIndex = i;
					break;
				}
			} //remove item from array
			
			//Remove seconds
			date.Set(CalendarField.Second, 0);
			
			//addAlarm
			AddAlarm((int) date.TimeInMillis, funcAlarmTitle: alarmTitle, gameID: gameID, soundFile: soundFile, repeatArr: repeatArr, insertAt: alarmArrayIndex, alarmID:  alarmID, isToggled: toggleStatus, redrawList: true);
		}


		//Add alarm to system
		public static void AddAlarm(int funcDate, string funcAlarmTitle, int gameID, string soundFile, bool[] repeatArr,
			int insertAt = -1, int alarmID = -1, bool isToggled = true, bool redrawList = true) {
			//repeatarr�� ��,��,ȭ,��,��,��,�� ������ ä��
		
			Calendar date =  Calendar.Instance;
			date.TimeInMillis = funcDate;

			string alarmTitle = funcAlarmTitle;
			int currentDayOfWeek = date.Get(CalendarField.DayOfWeek);

			if(alarmTitle == "") { //�˶� Ÿ��Ʋ�� ������ �Ҹ��� �︮�� ��Ȳ�� �߻��ϹǷ� �⺻ �̸� ����
				alarmTitle = "�˶�";
			}
			
			int fireOnce = -1; /* �ݺ����� ������ ���°�� 1ȸ������ �Ǵ��ϰ� date ��ȭ ����) */
			bool fireSearched = false;
			for (int i = 0; i < repeatArr.Length; ++i) {
				if (repeatArr[i] == true) {
					fireOnce = i; break;
				}
			}

			if (fireOnce != -1) { //������ ����� �ϴ� ��� ������ �����ؼ� ���� fireDate������ ����
				Log.Debug(UPLog.TAG, "TODAY OF WEEK =>" + currentDayOfWeek.ToString());
				//�Ͽ����� 0���� �����ϴ��� 1���� �����ϴ��� Ȯ���� �ʿ䰡 ����. 1���� �����ϸ� DayOfWeek Ȯ���� 6���� �ؾ���
				for (int i = 0; i < (currentDayOfWeek == 7 ? 0 : currentDayOfWeek); ++i ) {
					if (repeatArr[i] == true) {
						fireOnce = i; fireSearched = true; break;
					}
				} //������� �����ַ� �Ѿ������ ġ�� �ѹ��� ����
				
				if (!fireSearched) {
					for (int i = 0; i < repeatArr.Length; ++i) {
						if (repeatArr[i] == true) {
							fireOnce = i; fireSearched = true; break;
						}
					}
				}
			}
		
			Log.Debug(UPLog.TAG, "Next alarm date is " + fireOnce.ToString() + " (-1: no repeat, 0=sunday)");
			int fireAfterDay = 0;
		
			if (fireOnce == -1 || (fireSearched && fireOnce == currentDayOfWeek - 1 )) {
				//Firedate modifiy not needed but check time
				//�ð��� ���Ÿ� �˶� �߰� ���ؾ��� + �������� �Ѱܾߵ�
				if (funcDate <= (Calendar.Instance.TimeInMillis / 1000) ) {
					//������ �˶��̱� ������, �������� �Ѱܾߵ�!
					
					if (fireOnce == -1) { //�ݺ� ������ ��� �׳� �������� �ѱ�.
						Log.Debug(UPLog.TAG, "Past alarm!! add 1 day");
						date.Add(CalendarField.Date, 1);
					} else {
						//���� �ݺ��ϱ��� ����� �߰�
						if (fireOnce - (currentDayOfWeek - 1) > 0) {
							fireAfterDay = fireOnce - (currentDayOfWeek - 1);
							Log.Debug(UPLog.TAG, "(past) Firedate is over today: " + fireAfterDay.ToString());
						} else {
							fireAfterDay = (7 - (currentDayOfWeek - 1)) + fireOnce;
							Log.Debug(UPLog.TAG, "(past) Firedate is before today: " + fireAfterDay.ToString());
						}
						date.Add(CalendarField.Date, fireAfterDay);
						Log.Debug(UPLog.TAG, "Firedate " + date.ToString());
					}
				
				} else {
					Log.Debug(UPLog.TAG, "This is not past alarm.");
				} //end if
			
			} else {
				//Firedate modify.
				if (fireOnce - (currentDayOfWeek - 1) > 0) {
					fireAfterDay = fireOnce - (currentDayOfWeek - 1);
					Log.Debug(UPLog.TAG, "Firedate is over today: " + fireAfterDay.ToString());
				} else {
					fireAfterDay = (7 - (currentDayOfWeek - 1)) + fireOnce;
					Log.Debug(UPLog.TAG, "Firedate is before today: " + fireAfterDay.ToString());
				}
				//Add to date
				date.Add(CalendarField.Date, fireAfterDay);
				Log.Debug(UPLog.TAG, "Firedate " + date.ToString());
			}
		
			int alarmUUID = alarmID == -1 ? (int) Calendar.Instance.TimeInMillis : alarmID;
		
			//// ~~ ���� �� �κп� Notifi ��� �κ��� �־��µ�,
			//// �ȵ���̵�� ���񽺷� ���� �˶��� ����� �ϸ� ���� ��� �� �����ϸ� ��Ƽ��Ƽ�� ����
			//// �۾��� �� �ű� ������ �˶� �����͸� ����Ǹ� ��. (�� ��Ƽ�� �ʿ� X)

			//Remove seconds
			date.Set(CalendarField.Second, 0);
			
			//Add alarm to system (array) and save to nsdef
			AlarmElement tmpAlarmEle = new AlarmElement(
				alarmTitle, gameID, repeatArr, soundFile, (int) date.TimeInMillis /* �и������� �״�� ������ */,
				isToggled, alarmUUID
				);
			
			if (insertAt == -1) {
				//add to arr and save
				alarmsArray.Add( tmpAlarmEle );
			} else {
				alarmsArray.Insert(insertAt, tmpAlarmEle);
			}
		
			//// TODO!!! 20160421
			//// �ȵ���̵� ������ ���� �ڵ带 �־����
			UPDataManager.SaveDataJObject(UPDataManager.KEY_ALARMS, JsonConvert.SerializeObject(alarmsArray, Formatting.Indented));
		}

	}
}