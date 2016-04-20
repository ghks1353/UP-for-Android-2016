using System.Collections.Generic;
using UP.Elements;
using Java.Util;
using Android.Util;
using Newtonsoft.Json;
using UP.Debug;

namespace UP.Managers {
	public class UPAlarmManager {
		/*
			LocalNotification을 이용한 알람 설정 및 해제, 다음 알람 반복 관련 처리
			알람 설정시 월화수목금토일 반복에 대한 정보를 userInfo에 저장함.
			앱 실행 / 알람 추가 / 수정 시 다음 사항 확인.
				- 다음 반복이 필요한지 체크함
				- 필요한 경우 날짜만 더해서 (다음 알람에 대해서만) 알람을 재등록함
				- 반복이 없으면 리스트에 off상태로 둠. 알람등록은 안함
				- 다음날이 알람일이 아니지만 반복은 있는경우 그 날이 돌아올때까지 일만 추가함
		*/
		public static List<AlarmElement> alarmsArray;
		public static bool isAlarmMergedFirst = false; //첫회 merge 체크용
		public static int alarmMaxRegisterCount = 20; //알람 최대 등록 가능 개수
	
		public static bool alarmRingActivated = false; //알람 액티비티.. 아니 뷰가 뜨고 있을 때 true. (게임 진행 중일 때.)
		public static bool alarmSoundPlaying = false;

		// ~~ 알람 울림과 알람 끄기의 경우는 다른 곳에서 구현하거나 방도를 찾아보기로. ~~

		//알람이 켜져있는 게 있을 경우 다음 알람이 울릴 시각을 가져옴
		public static int GetNextAlarmFireInSeconds() {
			//Merge된 후 실행되야 함
			if (!isAlarmMergedFirst) {
				MergeAlarm();
			} //merge first
		
			int alarmNextFireDate = -1;
			for (int i = 0; i < alarmsArray.Count; ++i) {
				if (alarmsArray[i].alarmToggle == false) {
					continue;
				} //ignores off
				if (alarmNextFireDate == -1 || alarmNextFireDate > alarmsArray[i].alarmFireDate) {
					//적은 시간 우선으로 대입
					alarmNextFireDate = alarmsArray[i].alarmFireDate;
				}
			}
		
			return alarmNextFireDate; //-1을 리턴한 경우, 켜져있는 알람이 없음
		} //end func

		//울리고 있는 알람을 가져옴. 여러개인 경우, 첫번째 알람만 리턴함
		public static AlarmElement GetRingingAlarm() {
			//나중에 게임 클리어 후 알람을 끌 때, 울리고 있는 알람 전체를 끌 수 있게 해야됨
			//(그렇지 않으면 울린 알람만큼 게임을 깨야됨)
		
			//또한 게임 진행중일땐 앱 자체에 게임 진행중이라는 체크가 필요하며 그렇지 않을 경우
			//게임하다가 밖으로 나갔다왔는데 알람 울림화면으로 다시..
			int currentDateInMilliSeconds = (int) Calendar.Instance.TimeInMillis;
			for (int i = 0; i < alarmsArray.Count; ++i) { 
				if (alarmsArray[i].alarmToggle == false) {
					continue;
				} //ignores off
			
				if (alarmsArray[i].alarmFireDate <= currentDateInMilliSeconds
					&& alarmsArray[i].alarmCleared == false) {
					/*  1. fired된 알람일 때.
						2. 클리어를 못했을 때.
						3. toggled된 알람일 때. */
						return alarmsArray[i];
				}
			} //end for
			
			return null; //Element 없음
		}

		//알람 게임 클리어 토글
		public static void GameClearToggleAlarm( int alarmID, bool cleared ) {
			AlarmElement modAlarmElement = GetAlarm(alarmID);
			modAlarmElement.alarmCleared = cleared;
		
			//save it
			//TODO 20160421 데이터 저장
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
			//스케줄된 알람들 가져와서 지난것들 merge하고, 발생할 수 있는 오류에 대해서 체크함
			
			int currentTime = (int) Calendar.Instance.TimeInMillis;
			int todayWeekday = Calendar.Instance.Get(CalendarField.DayOfWeek);

			Log.Debug(UPLog.TAG, "TODAY WEEKDAY IS " + todayWeekday.ToString());

			//TODO: 저장된 알람이 하나도 없을 경우, 저장을 할 수 있는 key를 만들고 빈 배열을 넣어준다.
			//있을 경우, 배열 alarmsArray에 받아온다.

			string loadedAlarmsArr = UPDataManager.LoadDataJObject( UPDataManager.KEY_ALARMS );
			alarmsArray = loadedAlarmsArr == "" ? new List<AlarmElement>() : JsonConvert.DeserializeObject<List<AlarmElement>>( loadedAlarmsArr );
			
			//UPDataManager.SaveDataJObject(UPDataManager.KEY_ALARMS, JsonConvert.SerializeObject(alarmsArray, Formatting.Indented));
			
			//Notification의 구현은 안드로이드에서 필요 없으므로 제외함

			Log.Debug(UPLog.TAG, "Scheduled alarm count" + alarmsArray.Count.ToString());
			for (int i = 0; i < alarmsArray.Count; ++i) { 
				//TODO: 사운드가 없는 것에 한해 그 사운드를 기본컴포넌트로 바꿔야함
				//하지만 아직 사운드매니저가 구현되지 않았으므로, 일단은 생략. 20160421. 작업후 todo 삭제바람
				
				//이 다음은, Toggle on된것 대상으로만 검사
				if (alarmsArray[i].alarmToggle == false) {
					Log.Debug(UPLog.TAG, "Scheduled alarm" + alarmsArray[i].alarmID.ToString() + " state off. skipping");
					continue;
				}
				Log.Debug(UPLog.TAG, "alarm id " + alarmsArray[i].alarmID.ToString() + " firedate (millisec)" + alarmsArray[i].alarmFireDate);

				if (alarmsArray[i].alarmFireDate <= currentTime
					&& alarmsArray[i].alarmCleared == true ) { /* 시간이 지났어도, 게임을 클리어 해야됨. 게임 클리어시 true로 설정후 merge 한번더 하면됨 */
					Log.Debug(UPLog.TAG, "Merge start " + alarmsArray[i].alarmID.ToString());
					//Repeat 대상이 있는지 체크

					//1. 오늘의 요일을 얻어옴. 2. 다음 날짜 알람 체크. 3. 날짜만큼 더함.
					//단, 오늘날짜가 아니라 다음날짜로 계산해야함. (왜냐면 오늘은 울렸으니깐.)
					int nextAlarmVaild = -1;
					for (int k = ( todayWeekday == 7 ? 0 : todayWeekday /* 다음날짜부터 */ ); k < alarmsArray[i].alarmRepeat.Length; ++k) {
						//마지막(토요일)에는 다음주 체크
						nextAlarmVaild = alarmsArray[i].alarmRepeat[k] == true ? k : nextAlarmVaild;
						if (alarmsArray[i].alarmRepeat[k] == true) { break; }
					}
					if (todayWeekday != 7 && nextAlarmVaild == -1) { //찾을 수 없는경우 앞에서부터 다시 검색
						//토요일을 배제하는 이유: 토요일은 이미 일요일부터 다시 돌기 때문.
						for (int k = 0; k < alarmsArray[i].alarmRepeat.Length; ++k) {
							nextAlarmVaild = alarmsArray[i].alarmRepeat[k] == true ? k : nextAlarmVaild;
							if (alarmsArray[i].alarmRepeat[k] == true) { break; }
						}
					}
					Log.Debug(UPLog.TAG, "Next alarm day (0=sunday) " + nextAlarmVaild.ToString());
					alarmsArray[i].alarmCleared = false; // 게임클리어 리셋
					
					//2
					//다음 알람 날짜에 알람 추가. (몇일 차이나는지 구해서 day만 더해주면됨. 없으면 추가안하고 토글종료)
					if (nextAlarmVaild == -1) {
						//반복 없는 경우 알람 토글 종료
						alarmsArray[i].alarmToggle = false;
						Log.Debug(UPLog.TAG, "Alarm toggle finished (no-repeat alarm)");
					} else {
						//반복인 경우 다음 반복일 계산
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
					
						//LocalNotifi는 안드로이드에서 필요가 없으니 과감하게 생략

						//add new push for next alarm
						Log.Debug(UPLog.TAG, "Alarm added successfully.");
					
					} //end vaild chk
				
				//alarm merge check if end
				} else {
					//알람이 켜져있지만, 시간이 지나지 않았거나 게임을 클리어하지 않은 경우
					Log.Debug(UPLog.TAG, "Alarm is on but not cleared (or not passed), id" + alarmsArray[i].alarmID.ToString() );
				
				}
			
			} //for end
			Log.Debug(UPLog.TAG, "Merge is done. time to save!");

			//todo 20160421 저장해야함
			UPDataManager.SaveDataJObject(UPDataManager.KEY_ALARMS, JsonConvert.SerializeObject(alarmsArray, Formatting.Indented));
			
			//Badge 표시용
			
			//안드로이드도 뱃지를 지원하긴 하지만, 이게 런처마다 다른데다 특별 플래그를 넣어줘야 하는 부분이 있어 보류
		
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
			//- 알람이 켜져있는 상태에서 끌 경우, LocalNotification도 같이 종료
			//- 알람이 꺼져있는 상태에서 킬 경우, 상황에 따라 (반복체크후) LocalNotification 추가
			if (!isAlarmMergedFirst) {
				MergeAlarm();
			} //merge first
			
			for (int i = 0; i < alarmsArray.Count; ++i) {
				if (alarmsArray[i].alarmID == alarmID) { //target found
					Log.Debug(UPLog.TAG, "Toggling. target:" + alarmID.ToString());

					if (alarmsArray[i].alarmToggle == alarmStatus) {
						Log.Debug(UPLog.TAG, "status already same..!!");
						break; //상태가 같으므로 변경할 필요 없음
					}
					
					//Notifi 구현 필요 없음

					if (alarmStatus == false) { //알람 끄기
						alarmsArray[i].alarmToggle = false; //alarm toggle to off.
					} else {
						//알람 켜기 (addalarm 재탕)
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
					
						//return; //한번더 저장해야됨.
						break; //save
					} //end status
					break; //해당 ID를 처리했으므로 다음부터의 루프는 무의미
				} //end alarmid target search
			} //end for
		
			//save it
			Log.Debug(UPLog.TAG, "Status change saving");

			//TODO: 시스템 저장
			UPDataManager.SaveDataJObject(UPDataManager.KEY_ALARMS, JsonConvert.SerializeObject(alarmsArray, Formatting.Indented));
		} //end func

		//Remove alarm from system
		public static void RemoveAlarm(int alarmID) {
			if (!isAlarmMergedFirst) {
				MergeAlarm();
			} //merge first
		
			//시스템 노티피 필요없으므로 포팅 생략
			
			for (int i = 0; i < alarmsArray.Count; ++i) {
				if (alarmsArray[i].alarmID == alarmID) {
					alarmsArray.RemoveAt(i);
					break;
				}
			} //remove item from array

			//save it
			Log.Debug(UPLog.TAG, "Alarm removed from system. saving");

			//TODO: 시스템에 저장
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
			
			//Notification 수정 코드가 있었지만 이것 역시 안드로이드에선 무쓸모

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
			//repeatarr에 일,월,화,수,목,금,토 순으로 채움
		
			Calendar date =  Calendar.Instance;
			date.TimeInMillis = funcDate;

			string alarmTitle = funcAlarmTitle;
			int currentDayOfWeek = date.Get(CalendarField.DayOfWeek);

			if(alarmTitle == "") { //알람 타이틀이 없으면 소리만 울리는 상황이 발생하므로 기본 이름 설정
				alarmTitle = "알람";
			}
			
			int fireOnce = -1; /* 반복요일 설정이 없는경우 1회성으로 판단하고 date 변화 없음) */
			bool fireSearched = false;
			for (int i = 0; i < repeatArr.Length; ++i) {
				if (repeatArr[i] == true) {
					fireOnce = i; break;
				}
			}

			if (fireOnce != -1) { //여러번 울려야 하는 경우 오늘을 포함해서 다음 fireDate까지만 더함
				Log.Debug(UPLog.TAG, "TODAY OF WEEK =>" + currentDayOfWeek.ToString());
				//일요일이 0부터 시작하는지 1부터 시작하는지 확인할 필요가 있음. 1부터 시작하면 DayOfWeek 확인을 6으로 해야함
				for (int i = 0; i < (currentDayOfWeek == 7 ? 0 : currentDayOfWeek); ++i ) {
					if (repeatArr[i] == true) {
						fireOnce = i; fireSearched = true; break;
					}
				} //없을경우 다음주로 넘어간것으로 치고 한번더 루프
				
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
				//시간이 과거면 알람 추가 안해야함 + 다음날로 넘겨야됨
				if (funcDate <= (Calendar.Instance.TimeInMillis / 1000) ) {
					//과거의 알람이기 때문에, 다음날로 넘겨야됨!
					
					if (fireOnce == -1) { //반복 꺼짐인 경우 그냥 다음날로 넘김.
						Log.Debug(UPLog.TAG, "Past alarm!! add 1 day");
						date.Add(CalendarField.Date, 1);
					} else {
						//다음 반복일까지 대기후 추가
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
		
			//// ~~ 원래 이 부분에 Notifi 등록 부분이 있었는데,
			//// 안드로이드는 서비스로 직접 알람이 울려야 하면 음악 재생 및 가능하면 액티비티를 띄우는
			//// 작업을 할 거기 때문에 알람 데이터만 저장되면 됨. (즉 노티피 필요 X)

			//Remove seconds
			date.Set(CalendarField.Second, 0);
			
			//Add alarm to system (array) and save to nsdef
			AlarmElement tmpAlarmEle = new AlarmElement(
				alarmTitle, gameID, repeatArr, soundFile, (int) date.TimeInMillis /* 밀리세컨드 그대로 저장함 */,
				isToggled, alarmUUID
				);
			
			if (insertAt == -1) {
				//add to arr and save
				alarmsArray.Add( tmpAlarmEle );
			} else {
				alarmsArray.Insert(insertAt, tmpAlarmEle);
			}
		
			//// TODO!!! 20160421
			//// 안드로이드 데이터 저장 코드를 넣어야함
			UPDataManager.SaveDataJObject(UPDataManager.KEY_ALARMS, JsonConvert.SerializeObject(alarmsArray, Formatting.Indented));
		}

	}
}