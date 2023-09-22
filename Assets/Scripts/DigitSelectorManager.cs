using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VickyCustoms;

public class DigitSelectorManager : MonoBehaviour
{
    public ChestType Wooden;
    public ChestType Silver;
    public ChestType Golden;
    public ChestType Diamond;
    public ChestType Youtube;

    [SerializeField] TMP_Text ChestName;
    [SerializeField] TMP_Text PrizeMoney;
    [SerializeField] TMP_Text DigitsToUnlock;
    [SerializeField] TMP_Text PrizeMoneyDetail;
    [SerializeField] TMP_Text PasswordsLeft;
    [SerializeField] Image ChestImage;
    [SerializeField] GameObject YoutubeWindow;
    LockerDigits LockerDigits;
    int RemainingAttempts = 0;

    ChestType ActiveChest;

    void Start()
    {
        ActiveChest = Diamond;
        Next();
    }

    public void OnUnlockButtonClicked()
    {
        if(ActiveChest != Youtube)
        {
			this.gameObject.SetActive(false);
			IPManager.instance.OnLockerTypeChanged(ActiveChest._lockerDigits);
			PFManager.instance.ActiveLockerDigits = ActiveChest._lockerDigits;
			PFManager.instance.ActiveChest = ActiveChest;
			DigitRevealManager.Instance.CheckPurchasedDigits();
			PFManager.instance.GetTotalChancesForToday();
			PFManager.instance.GetTotalFailedAttempts();
			PlayerPrefs.SetInt("DefaultsSet", 1);
			PlayerPrefs.SetString("DefaultChest", ActiveChest._lockerDigits.ToString());
		}
        else
        {
            YoutubeWindow.SetActive(true);
        }
        
    }

    void UpdateUI()
    {
        if(ActiveChest != Youtube)
        {
			ChestImage.sprite = ActiveChest.ChestImage;
			ChestName.text = ActiveChest.ChestName;
			PrizeMoney.text = ActiveChest.PrizeMoney;
			DigitsToUnlock.text = ActiveChest.DigitsToUnlock;
			PrizeMoneyDetail.text = "Prize: " + ActiveChest.PrizeMoney + " Rs";
			PasswordsLeft.text = RemainingAttempts.ToString() + " Passwords Left";
		}
        else
        {
			ChestImage.sprite = ActiveChest.ChestImage;
			ChestName.text = ActiveChest.ChestName;
			PrizeMoney.text = "INR 100";
			DigitsToUnlock.text = ActiveChest.DigitsToUnlock;
			PrizeMoneyDetail.text = "Prize: " + ActiveChest.PrizeMoney + " Rs";
			PasswordsLeft.text = "Watch video first";
		}
        
    }
    public void Next()
    {
		if (ActiveChest == Diamond)
        {
			ActiveChest = Youtube;
        }
		else if (ActiveChest == Youtube)
		{
			ActiveChest = Wooden;
		}
		else if(ActiveChest == Golden)
        {
            ActiveChest = Diamond;
        }
        else if(ActiveChest == Wooden)
        {
            ActiveChest = Silver;
        }
        else if(ActiveChest == Silver)
        {
            ActiveChest = Golden;
        }
        UpdateUI();
        if(ActiveChest != Youtube)
        {
            GetFailedAttempts();
        }
    }
    public void Previous()
    {
		if (ActiveChest == Wooden)
		{
			ActiveChest = Youtube;
		}
		else if (ActiveChest == Youtube)
		{
			ActiveChest = Diamond;
		}
		else if (ActiveChest == Golden)
		{
			ActiveChest = Silver;
		}
		else if (ActiveChest == Diamond)
		{
			ActiveChest = Golden;
		}
		else if (ActiveChest == Silver)
		{
			ActiveChest = Wooden;
		}
        UpdateUI();
        if(ActiveChest != Youtube)
        {
            GetFailedAttempts();
        }
	}
    void GetFailedAttempts()
    {
        string ID = "";
        if(ActiveChest._lockerDigits == LockerDigits.Three)
        {
            ID = PFManager.instance.ThreeDigitLockerID;
        }
        else if(ActiveChest._lockerDigits == LockerDigits.Four)
        {
            ID = PFManager.instance.FourDigitLockerID;
        }
		else if (ActiveChest._lockerDigits == LockerDigits.Five)
		{
			ID = PFManager.instance.FiveDigitLockerID;
		}
		else if (ActiveChest._lockerDigits == LockerDigits.Six)
		{
			ID = PFManager.instance.SixDigitLockerID;
		}
		CollectionReference lockerRef = FirebaseFirestore.DefaultInstance.Collection("Lockers").Document(ID).Collection("Attempts");
		lockerRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
		{
			if (task.IsCompleted)
			{
				QuerySnapshot snapshot = task.Result;
				int totalAttempts = snapshot.Count;
                RemainingAttempts = ActiveChest.TotalPossiblePasswords - totalAttempts;
                UpdateUI();
			}
		});
	}
}
