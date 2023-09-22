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
	[SerializeField] TMP_Text PricingForRevealText;
	[SerializeField] GameObject AddFundsWindow;
	public delegate void PaymentCallback(int amount);
	public static DigitRevealManager Instance;
	private void Awake()
	{
		Instance = this;
	}
	public void CheckPurchasedDigits()
	{
		FirebaseFirestore.DefaultInstance.Collection("users").Document(SystemInfo.deviceUniqueIdentifier).Collection("RevealedDigits").GetSnapshotAsync().ContinueWithOnMainThread(task =>
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
		PricingForRevealText.text = $"Reveal first {digitCountToreveal} digits for INR {priceToReveal.ToString()}";
		PricingForRevealText.GetComponent<Button>().interactable = true;
		PricingForRevealText.fontStyle = FontStyles.Normal;
		PricingForRevealText.fontStyle = FontStyles.Underline;
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
		FirebaseFirestore.DefaultInstance.Collection("users").Document(SystemInfo.deviceUniqueIdentifier).Collection("RevealedDigits").Document(lockerID).SetAsync(data).ContinueWithOnMainThread(task =>
		{
			if(task.IsCompleted)
			{
				PricingForRevealText.text = "Password starts with: " + digits;
				PricingForRevealText.GetComponent<Button>().interactable = false;
				PricingForRevealText.fontStyle = FontStyles.UpperCase;
			}
		});
	}
	public void GetAmountInWallet(PaymentCallback callback)
	{
		CollectionReference PaymentsRef = FirebaseFirestore.DefaultInstance.Collection("PaymentRequests");
		PaymentsRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
		{
			if (task.IsCompleted)
			{
				QuerySnapshot snapshot = task.Result;
				if (snapshot != null)
				{
					int amountInWallet = 0;
					foreach (var doc in snapshot.Documents)
					{
						if (doc.GetValue<string>("PlayerID") == PlayerPrefs.GetString("PF_ID"))
						{
							int amt = int.Parse(doc.GetValue<string>("Amount"));
							amountInWallet += amt;
						}
					}

					var request = new ExecuteCloudScriptRequest
					{
						FunctionName = "getPlayerData",
						FunctionParameter = new { PlayerID = PlayerPrefs.GetString("PF_ID") },
						GeneratePlayStreamEvent = true
					};

					// Call the Cloud Script function
					PlayFabClientAPI.ExecuteCloudScript(request, result =>
					{
						PlayerDataManager.PlayerData _playerData = JsonConvert.DeserializeObject<PlayerDataManager.PlayerData>(result.FunctionResult.ToString());
						amountInWallet += _playerData.Amount;
						amountInWallet += _playerData.SpentAmount;
						callback(amountInWallet);
					}, error =>
					{
						// Handle error response
						PFManager.instance.ShowMessage("Error", "Something went wrong!", "Error");
					});
				}
			}
		});
	}
}
