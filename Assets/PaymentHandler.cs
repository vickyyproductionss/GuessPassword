using Firebase.Extensions;
using Firebase.Firestore;
using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Newtonsoft.Json;
using System;

public class PaymentHandler : MonoBehaviour
{
    [SerializeField] TMP_Text AmountInWalletText;
	[SerializeField] GameObject AddFundsWindow;
    public static int amountInWallet = 0;
	public FirebaseFirestore firestore;
	public static PaymentHandler instance;

	public List<Item> items = new List<Item>();

	[Serializable]
	public class Item
	{
		public int price;
		public int chancesToAdd;
	}

	private void Awake()
	{
        firestore = FirebaseFirestore.DefaultInstance;
		instance = this;
	}

	public void PurchaseForPrice(int price)
	{
		foreach(var item in items)
		{
			if(item.price == price)
			{
				PurchaseThisItem(item.price, item.chancesToAdd);
			}
		}
	}

	void PurchaseThisItem(int price, int chances)
	{
		CollectionReference PaymentsRef = firestore.Collection("PaymentRequests");
		PaymentsRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
		{
			if (task.IsCompleted)
			{
				QuerySnapshot snapshot = task.Result;
				if (snapshot != null)
				{
					amountInWallet = 0;
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
						if(amountInWallet >= price)
						{
							OnPurchaseComplete(price, chances);
						}
						else
						{
							AddFundsWindow.SetActive(true);
						}
					}, error =>
					{
						// Handle error response
						PFManager.instance.ShowMessage("Error", "Something went wrong!","Error");
					});
				}
			}
		});
	}

	void OnPurchaseComplete(int price, int chances)
	{
		var request = new ExecuteCloudScriptRequest
		{
			FunctionName = "purchaseChances",
			FunctionParameter = new { PlayerID = PlayerPrefs.GetString("PF_ID"), Price = price, Chances = chances },
			GeneratePlayStreamEvent = true
		};

		// Call the Cloud Script function
		PlayFabClientAPI.ExecuteCloudScript(request, result =>
		{
			PlayerDataManager.Instance.FetchLatestData();
			FetchLatestAmount();
			if(result.FunctionResult != null)
			{
				if(result.FunctionResult.ToString() == "1")
				{
					PFManager.instance.ShowMessage("Purchase Success", "Your purchase completed successfully", "Success");
				}
			}
			//Handle purchase success here
		}, error =>
		{
			// Handle error response
			PFManager.instance.ShowMessage("Error", "Something went wrong!", "Error");
		});
	}

    public void FetchLatestAmount()
    {
		AmountInWalletText.text = "";
		CollectionReference PaymentsRef = firestore.Collection("PaymentRequests");
		PaymentsRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
		{
			if (task.IsCompleted)
			{
				QuerySnapshot snapshot = task.Result;
				if (snapshot != null)
				{
					amountInWallet = 0;
					foreach (var doc in snapshot.Documents)
					{
						if (doc.GetValue<string>("PlayerID") == PlayerPrefs.GetString("PF_ID"))
						{
							int amt = int.Parse(doc.GetValue<string>("Amount"));
							amountInWallet += amt;
						}
					}
					if ((amountInWallet + PlayerDataManager.Instance.playerData.SpentAmount) > 0)
					{
						amountInWallet = amountInWallet + PlayerDataManager.Instance.playerData.SpentAmount;
						AmountInWalletText.text = (amountInWallet).ToString();
					}
					else
					{
						amountInWallet = 0;
						AmountInWalletText.text ="0";
					}
					PFManager.instance.GetTotalChancesForToday();
				}
			}
		});
	}
    public void AddMoneyToWallet()
    {
        Application.OpenURL("https://guessthepassword.web.app/?info=" + PlayerPrefs.GetString("PF_ID"));
    }
}
