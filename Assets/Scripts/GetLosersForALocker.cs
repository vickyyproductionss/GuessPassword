using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using TMPro;

public class GetLosersForALocker : MonoBehaviour
{
	[SerializeField] GameObject IncorrectPassParent;
	[SerializeField] GameObject IncorrectPassPrefab;
	[SerializeField] GameObject WindowToShow;
	FirebaseFirestore firestore;
	public static GetLosersForALocker instance;
	private void Awake()
	{
		instance = this;
		firestore = FirebaseFirestore.DefaultInstance;
	}
	public void GetAllIncorrectPasswords(string _lockerID, string _correctPass)
	{
		string CorrectPass = _correctPass;
		WindowToShow.SetActive(true);
		for (int i = IncorrectPassParent.transform.childCount - 1; i >= 0; i--)
		{
			Destroy(IncorrectPassParent.transform.GetChild(i).gameObject);
		}
		CollectionReference WrongAttemptsRef = firestore.Collection("Lockers").Document(_lockerID).Collection("Attempts");
		WrongAttemptsRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
		{
			if (task.IsCompleted)
			{
				QuerySnapshot snapshot = task.Result;
				int index = 1;
				foreach (DocumentSnapshot documentSnapshot in snapshot.Documents)
				{
					GameObject WrongPass = Instantiate(IncorrectPassPrefab);
					WrongPass.transform.SetParent(IncorrectPassParent.transform, false);
					WrongPass.transform.GetChild(0).GetComponent<TMP_Text>().text = index.ToString() + ".";
					string PName = documentSnapshot.GetValue<string>("Player");
					
					if (PName != "")
					{
						WrongPass.transform.GetChild(1).GetComponent<TMP_Text>().text = PName;
					}
					else
					{
						WrongPass.transform.GetChild(1).GetComponent<TMP_Text>().text = "Unknown player";
					}
					
					string PassUsed = documentSnapshot.GetValue<string>("PasswordUsed");
					int AccuracyReturned = ReturnAccuracy(CorrectPass, PassUsed);
					Color textColor = Color.white;
					if(AccuracyReturned < 25)
					{
						textColor = Color.grey;
					}
					if(AccuracyReturned >= 25)
					{
						textColor = Color.white;
					}
					if (AccuracyReturned >= 50)
					{
						textColor = Color.yellow;
					}
					if (AccuracyReturned >= 85)
					{
						textColor = Color.green;
					}	

					string Accuracy = "("+ AccuracyReturned.ToString()+"% Accurate"+")";
					string CorrectPassAccuracy = PassUsed + $"<size=14>{Accuracy}</size>";
					WrongPass.transform.GetChild(2).GetComponent<TMP_Text>().text = CorrectPassAccuracy;
					WrongPass.transform.GetChild(2).GetComponent<TMP_Text>().color = textColor;
					
					index++;
					if (index > 100)
					{
						return;
					}
				}
				
			}
		});
	}
	int ReturnAccuracy(string correctPass, string UsedPass)
	{
		int numOfDigits = correctPass.Length;
		int correctPassword = int.Parse(correctPass);
		int usedPassword = int.Parse(UsedPass);
		int totalPossibilites = 0;
		if(numOfDigits == 3)
		{
			totalPossibilites = 900;
		}
		else if(numOfDigits == 4)
		{
			totalPossibilites = 9000;
		}
		else if (numOfDigits == 5)
		{
			totalPossibilites = 90000;
		}
		else if (numOfDigits == 6)
		{
			totalPossibilites = 900000;
		}
		int difference = (correctPassword - usedPassword);
		if(difference < 0)
		{
			difference = difference * (-1);
		}
		int accuracy = totalPossibilites - difference;
		int percentage = (accuracy * 100 / totalPossibilites);
		if(percentage == 100 &&(correctPass != UsedPass))
		{
			percentage = 90;
		}
		return percentage;
	}
}
