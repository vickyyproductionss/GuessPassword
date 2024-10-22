using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VickyCustoms;

public class IPManager : MonoBehaviour
{
	[SerializeField] TMP_Text ShowMessagehere;
	[SerializeField] Button CheckPasswordButton;
	[SerializeField] TMP_InputField IP;
	[SerializeField] TMP_Text[] Digits;
	public string Password;
	public static IPManager instance;
	private void Awake()
	{
		instance = this;
	}
	public void EmptyTheIP()
	{
		IP.text = "";
	}
	public void OnLockerTypeChanged(LockerDigits ActiveLockerDigit)
	{
		for (int i = 0; i < 6; i++)
		{
			Digits[i].transform.parent.gameObject.SetActive(false);
		}
		if (ActiveLockerDigit == LockerDigits.Three)
		{
			IP.characterLimit = 3;
			for (int i =0; i < 3; i++)
			{
				Digits[i].transform.parent.gameObject.SetActive(true);
			}
		}
		else if(ActiveLockerDigit == LockerDigits.Four)
		{
			IP.characterLimit = 4;
			for (int i = 0; i < 4; i++)
			{
				Digits[i].transform.parent.gameObject.SetActive(true);
			}
		}
		else if(ActiveLockerDigit == LockerDigits.Five)
		{
			IP.characterLimit = 5;
			for (int i = 0; i < 5; i++)
			{
				Digits[i].transform.parent.gameObject.SetActive(true);
			}
		}
		else if(ActiveLockerDigit == LockerDigits.Six)
		{
			IP.characterLimit = 6;
			for (int i = 0; i < 6; i++)
			{
				Digits[i].transform.parent.gameObject.SetActive(true);
			}
		}
	}

	public void OnValueUpdated()
	{
		CheckPasswordButton.interactable = false;
		AgainSelect();
		char[] chars = IP.text.ToCharArray();
		for(int i = 0; i < chars.Length; i++)
		{
			Digits[i].text = chars[i].ToString();
		}
		Password = "";
		for(int i = 0; i < IP.characterLimit; i++)
		{
			Password += Digits[i].text;
		}
		
	}
	public void CheckExistingPass()
	{
		if (IP.text.Length == IP.characterLimit)
		{
			CheckIfPassAlreadyEntered(Password);
		}
	}
	void CheckIfPassAlreadyEntered(string password)
	{
		char[] chars = password.ToCharArray();
		if (chars[0] != '0')
		{
			string ID = "";
			if (PFManager.instance.ActiveLockerDigits == LockerDigits.Three)
			{
				ID = PFManager.instance.ThreeDigitLockerID;
			}
			else if (PFManager.instance.ActiveLockerDigits == LockerDigits.Four)
			{
				ID = PFManager.instance.FourDigitLockerID;
			}
			else if (PFManager.instance.ActiveLockerDigits == LockerDigits.Five)
			{

				ID = PFManager.instance.FiveDigitLockerID;
			}
			else if (PFManager.instance.ActiveLockerDigits == LockerDigits.Six)
			{
				ID = PFManager.instance.SixDigitLockerID;
			}
			DocumentReference docRef = PFManager.instance.firestore.Collection("Lockers").Document(ID).Collection("Attempts").Document(password);

			docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
			{
				if (task.IsCompleted)
				{
					DocumentSnapshot snapshot = task.Result;
					if (snapshot.Exists)
					{
						AgainSelect();
						string playerName = snapshot.GetValue<string>("Player");
						CheckPasswordButton.interactable = false;
						if (playerName != "")
						{
							StartCoroutine(ShowThisMessage($"Player: {playerName} already tried this wrong password"));
						}
						else
						{
							StartCoroutine(ShowThisMessage($"A player already tried this wrong password"));
						}
					}
					else
					{
						CheckPasswordButton.interactable = true;
					}
				}
			});
		}
		else
		{
			StartCoroutine(ShowThisMessage("Password cannot start with a '0'", true));
		}
		
	}
	public void ShowFailure()
	{
		AgainSelect();
		StartCoroutine(ShowThisMessage("Wrong Password !!! Try Again", true));
	}
	public void ShowSuccess()
	{
		AgainSelect();
		StartCoroutine(ShowThisMessage("Congratulations Locker Unlocked !!! \n Prize Money Added To Wallet", false));
	}
	IEnumerator ShowThisMessage(string msg, bool IsError = true)
	{
		ShowMessagehere.text = msg;
		if(IsError)
		{
			ShowMessagehere.color = Color.red;
		}
		else
		{
			ShowMessagehere.color = Color.green;
		}
		yield return new WaitForSeconds(3);
		ShowMessagehere.text = PFManager.instance.GetActiveLockerID();
		ShowMessagehere.color = Color.white;
	}
	public void AgainSelect()
	{
		Password = "";
		for (int i = 0; i < 6; i++)
		{
			Digits[i].text = "";
		}
	}
}
