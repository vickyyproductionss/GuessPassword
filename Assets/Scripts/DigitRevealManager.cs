using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;
using Newtonsoft.Json;
using UnityEngine.UI;

public class DigitRevealManager : MonoBehaviour
{
	[SerializeField] TMP_Text PricingForRevealText,PricingTextOnPopup,WinningChancesText;
	[SerializeField] GameObject AddFundsWindow,PurchaseButton;
	public delegate void PaymentCallback(int amount);
	public static DigitRevealManager Instance;
	public int RevealedDigits = 0;
	private void Awake()
	{
		Instance = this;
	}
	public void CheckPurchasedDigits()
	{
		RevealedDigits = 0;
		FirebaseFirestore.DefaultInstance.Collection("Users").Document(PlayerPrefs.GetString("FirebaseUserId")).Collection("RevealedDigits").GetSnapshotAsync().ContinueWithOnMainThread(task =>
		{
			if(task.IsCompleted)
			{
				QuerySnapshot snapshot = task.Result;
				ShowPricing();
				foreach(var item in snapshot)
				{
					if(item.GetValue<string>("LockerID") == PFManager.instance.GetActiveLockerID())
					{
						PricingForRevealText.text = "Password starts with: " + item.GetValue<string>("Digits");
						RevealedDigits = item.GetValue<string>("Digits").Length;
						PricingForRevealText.GetComponent<Button>().interactable = false;
						PricingForRevealText.fontStyle = FontStyles.Normal;
						PricingForRevealText.fontStyle = FontStyles.Bold;
					}
				}
			}
		});
	}
	public void ShowPricing()
	{
		int digitCountToreveal = (PFManager.instance.ActiveChest.TotalPossiblePasswords / 3).ToString().Length - 3;
		if(digitCountToreveal <= 0 )
		{
			PricingForRevealText.gameObject.SetActive(false);
			return;
		}
		PricingForRevealText.gameObject.SetActive(true);
		int priceToReveal = digitCountToreveal * 10;
		PricingForRevealText.text = $"Reveal digits & increase winning chances by 99%";
		PricingForRevealText.GetComponent<Button>().interactable = true;
		PricingForRevealText.fontStyle = FontStyles.Normal;
		PricingForRevealText.fontStyle = FontStyles.Underline;
	}
	public void ShowPricingOnPopup()
	{
		if(RevealedDigits == 0)
		{
			PurchaseButton.GetComponent<Button>().interactable = true;
			int digitCountToreveal = (PFManager.instance.ActiveChest.TotalPossiblePasswords / 3).ToString().Length - 3;
			PricingForRevealText.gameObject.SetActive(true);
			int priceToReveal = digitCountToreveal * 10;
			PricingTextOnPopup.text = $"Reveal {digitCountToreveal} Digits for {priceToReveal} INR";
			float percentage = 0;
			if (digitCountToreveal == 1)
			{
				percentage = 88.88f;
			}
			else if (digitCountToreveal == 2)
			{
				percentage = 98.88f;
			}
			else if (digitCountToreveal == 3)
			{
				percentage = 99.88f;
			}
			WinningChancesText.text = $"Winning percent = {percentage} %";
		}
		else
		{
			PurchaseButton.GetComponent<Button>().interactable = false;
			PricingTextOnPopup.text = "";
			WinningChancesText.text = "Already Revealed\n Try for another locker.";
		}
		
	}
	public void CheckAmountBeforeRevealing()
	{
		GetAmountInWallet(OnPaymentDataRecieved);
	}
	void OnPaymentDataRecieved(int amount)
	{
		string lockerid = PFManager.instance.GetActiveLockerID();
		int lockerdigits = (PFManager.instance.ActiveChest.TotalPossiblePasswords / 3).ToString().Length;
		int priceToReveal = (lockerdigits - 3) * 10;
		if (amount >= priceToReveal)
		{
            PaymentHandler.instance.PurchaseForDigits(priceToReveal);
            RevealDigitsForThisLocker(lockerid, lockerdigits);
		}
		else
		{
			AddFundsWindow.SetActive(true);
		}
	}
	public void RevealDigitsForThisLocker(string lockerID,int digitCount)
	{
		int digitCountToreveal = digitCount - 3;
		
		if(digitCountToreveal > 0)
		{
			var request = new ExecuteCloudScriptRequest
			{
				FunctionName = "revealDigits",
				FunctionParameter = new { DigitCount = digitCountToreveal,LockerID = lockerID, key = PFManager.instance.ActiveLockerDigits.ToString()},
				GeneratePlayStreamEvent = true
			};
			PlayFabClientAPI.ExecuteCloudScript(request, result =>
			{
				SaveToFirebase(PFManager.instance.GetActiveLockerID(), result.FunctionResult.ToString());

			}, error =>
			{
				PFManager.instance.ShowMessage("Error", "Something went wrong!", "Error");
			});
		}
	}
	void SaveToFirebase(string lockerID, string digits)
	{
		Dictionary<string, object> data = new Dictionary<string, object>()
		{
			{"LockerID",lockerID},
			{"Digits",digits }
		};
		FirebaseFirestore.DefaultInstance.Collection("Users").Document(PlayerPrefs.GetString("FirebaseUserId")).Collection("RevealedDigits").Document(lockerID).SetAsync(data).ContinueWithOnMainThread(task =>
		{
			if(task.IsCompleted)
			{
				PricingForRevealText.text = "Password starts with: " + digits;
				PricingForRevealText.GetComponent<Button>().interactable = false;
				PFManager.instance.UpdateWinningChancesForThisLocker();
			}
		});
	}
	public void GetAmountInWallet(PaymentCallback callback)
	{
		callback.Invoke(PaymentHandler.amountInWallet);
	}
}
