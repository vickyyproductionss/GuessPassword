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
    [SerializeField] TMP_Text AmountRecievedTest;
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
		checkPaymentRecieved();

	}
	void checkPaymentRecieved()
	{
		// Check if this is running on an Android device
		if (Application.platform == RuntimePlatform.Android)
		{
			//Access the Unity activity's intent
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>("getIntent");

			// Retrieve the data using the key specified in MainActivity
			string receivedData = intent.Call<string>("getStringExtra", "amount");
			UpdateAmountInWallet(int.Parse(receivedData));
			intent.Call("removeExtra", "amount");
		}
	}
	void UpdateAmountInWallet(int amountInWallet)
	{
		CollectionReference PaymentsRef = firestore.Collection("PaymentRequests");
		Dictionary<string, object> request = new Dictionary<string, object>()
		{
			{"Amount",amountInWallet.ToString()},
			{"PlayerID",PlayerPrefs.GetString("PF_ID")}
		};
		PaymentsRef.Document(PlayerPrefs.GetString("PF_ID")).SetAsync(request).ContinueWithOnMainThread(task =>
		{
			PFManager.instance.ShowMessage("Success", "Money added to wallet", "Success");
			FetchLatestAmount();
		});
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

	#region Handle Native Side Payment
	// Android package name and class name of the activity you want to launch
	private string packageName = "com.VGames.GuessThePassword"; // Replace with your Android app's package name
	private string className = "com.VGames.GuessThePassword.MainActivity"; // Replace with the full path to your Android activity

	// Function to launch the Android activity
	public void LaunchAndroidActivity()
	{
		try
		{
			// Create an AndroidJavaObject for the Intent class
			AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");

			// Set the action for the intent (launching an activity)
			intentObject.Call<AndroidJavaObject>("setAction", "android.intent.action.MAIN");

			// Set the package and class name of the target activity
			AndroidJavaObject component = new AndroidJavaClass("android.content.ComponentName");
			component = new AndroidJavaObject("android.content.ComponentName", packageName, className);
			intentObject.Call<AndroidJavaObject>("setComponent", component);

			// Get the current Unity activity
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

			// Start the Android activity
			currentActivity.Call("startActivity", intentObject);
		}
		catch (System.Exception e)
		{
			Debug.LogError("Error launching Android activity: " + e.Message);
		}
	}
	public void AddMoneyToWallet()
    {
		LaunchAndroidActivity();
		//Application.OpenURL("https://guessthepassword.web.app/?info=" + PlayerPrefs.GetString("PF_ID"));
	}
	#endregion
}
