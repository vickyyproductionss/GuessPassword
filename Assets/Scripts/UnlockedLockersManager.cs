using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnlockedLockersManager : MonoBehaviour
{
	public FirebaseFirestore firestore;

	[SerializeField] GameObject WinnerPrefab;
	[SerializeField] GameObject WinnerParent;
	[SerializeField] TMP_Text Message;

	private void OnEnable()
	{
		firestore = FirebaseFirestore.DefaultInstance;
		Message.gameObject.SetActive(true);
		Message.text = "Feching Please Wait...";
		for(int i = WinnerParent.transform.childCount-1; i >=0 ; i--)
		{
			Destroy(WinnerParent.transform.GetChild(i).gameObject);
		}
		GetAllUnlockedLockers();
	}

	void GetAllUnlockedLockers()
    {
		CollectionReference UnlockedLockerDataRef = firestore.Collection("Unlocked");
		UnlockedLockerDataRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
		{
			if(task.IsCompleted)
			{
				Message.gameObject.SetActive(false);
				Message.text = "";
				QuerySnapshot snapshot = task.Result;
				foreach(var item in snapshot.Documents)
				{
					GameObject Winner = Instantiate(WinnerPrefab);
					Winner.transform.SetParent(WinnerParent.transform, false);
					string password = item.GetValue<string>("PasswordUsed");
					string totalAttempts = item.GetValue<string>("AttemptNumber");
					string winAmount = ReturnWinningAmountFromPasswordAndAttempts(password, totalAttempts);

					Winner.transform.GetChild(0).GetComponent<TMP_Text>().text = item.GetValue<string>("Winner") +" won " + winAmount + " rupees";//locker ID
					Winner.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = "Unlock Time: " + item.GetValue<string>("UnlockedAt");//unlocked date time
					Winner.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = item.GetValue<string>("LockerID");//winner name
					Winner.transform.GetChild(1).GetChild(2).GetComponent<TMP_Text>().text = "Total Attempts: " + totalAttempts;//Attempt count
					Winner.transform.GetChild(1).GetChild(3).GetComponent<TMP_Text>().text = "Password: " + password;//Password used
				}
			}
			else
			{
				Message.gameObject.SetActive(true);
				Message.text = "Something Went Wrong.";
			}
		});
	}
	string ReturnWinningAmountFromPasswordAndAttempts(string password, string attempts)
	{
		int totalAttempts = int.Parse(attempts);
		int passLength = password.Length;
		int minimumAmount = 0;
		if(passLength == 3)
		{
			minimumAmount = 2;
		}
		else if(passLength == 4)
		{
			minimumAmount = 10;
		}
		else if (passLength == 5)
		{
			minimumAmount = 20;
		}
		else if (passLength == 6)
		{
			minimumAmount = 50;
		}

		float adsWinning = totalAttempts * 0.013f;
		int totalWinning = minimumAmount + (int)adsWinning;
		return totalWinning.ToString();
	}
}
