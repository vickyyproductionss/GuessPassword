using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoCollector : MonoBehaviour
{
	public FirebaseFirestore firestore;
	public static InfoCollector Instance;
	float _totalSeconds;
	private void Awake()
	{
		Instance = this;
	}
	private void Start()
	{
		string activeTimePrefsName = $"TotalSecondsOnline_{System.DateTime.Today.Date.ToString()}";
		_totalSeconds = PlayerPrefs.GetFloat(activeTimePrefsName);
		Debug.Log(activeTimePrefsName + " keyused " + _totalSeconds + " value found");
		firestore = FirebaseFirestore.DefaultInstance;
		StartCoroutine(RecordTime());
	}
	IEnumerator RecordTime()
	{
		yield return new WaitForSeconds(1);
		_totalSeconds++;
		string activeTimePrefsName = $"TotalSecondsOnline_{System.DateTime.Today.Date.ToString()}";
		PlayerPrefs.SetFloat(activeTimePrefsName, _totalSeconds);

		if (_totalSeconds%30 == 0 && _totalSeconds > 0)
		{
			//Send data to server
			AddAppScreenTime(_totalSeconds);
		}
		StartCoroutine(RecordTime());
	}
	void AddAppScreenTime(float secondsToAdd)
	{
		Dictionary<string, object> dict = new Dictionary<string, object>()
		{
			{"SecondsOnline",_totalSeconds.ToString()}
		};
		firestore.Collection("users").Document(SystemInfo.deviceUniqueIdentifier.ToString()).Collection("Logins").Document((System.DateTime.Now.Date - new DateTime(1970, 1, 1)).TotalSeconds.ToString()).SetAsync(dict).ContinueWithOnMainThread(task =>
		{
			if(task.IsCompleted)
			{
				//Handle success here
				Debug.Log("SentToServer");
			}
		});
	}
}
