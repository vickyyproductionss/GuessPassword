using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;

public class PlayerDataManager : MonoBehaviour
{
	public PlayerData playerData = null;
	[SerializeField] TMP_Text AmountText,PlayerNameText;
	public static int WinningAmount;
	public static PlayerDataManager Instance;
	private void Awake()
	{
		Instance = this;
	}
	[System.Serializable]
	public class PlayerData
	{
		public string PlayerName;
		public int UnlockedLockers;
		public int PasswordsTried;
		public int Amount;
		public int PurchasedChances;
		public int SpentAmount;
		public int IsPremiumUser;
		public string AccountStatus;
	}

	public void FetchLatestData()
	{
		var request = new ExecuteCloudScriptRequest
		{
			FunctionName = "getPlayerData",
			FunctionParameter = new { PlayerID = PlayerPrefs.GetString("PF_ID")},
			GeneratePlayStreamEvent = true
		};

		// Call the Cloud Script function
		PlayFabClientAPI.ExecuteCloudScript(request, result =>
		{
			Debug.Log(result.FunctionResult);
			PlayerData _playerData = JsonConvert.DeserializeObject<PlayerData>(result.FunctionResult.ToString());
			playerData = _playerData;
			OnDataRecieved(playerData);
			PaymentHandler.instance.FetchLatestAmount();
		}, error =>
		{
			// Handle error response
			Debug.LogError("Cloud Script Error: " + error.ErrorMessage);
		});
	}
	public void FetchLatestChances()
	{
		var request = new ExecuteCloudScriptRequest
		{
			FunctionName = "getPlayerData",
			FunctionParameter = new { PlayerID = PlayerPrefs.GetString("PF_ID") },
			GeneratePlayStreamEvent = true
		};

		// Call the Cloud Script function
		PlayFabClientAPI.ExecuteCloudScript(request, result =>
		{
			Debug.Log(result.FunctionResult);
			PlayerData _playerData = JsonConvert.DeserializeObject<PlayerData>(result.FunctionResult.ToString());
			playerData.PurchasedChances = _playerData.PurchasedChances;
		}, error =>
		{
			// Handle error response
			Debug.LogError("Cloud Script Error: " + error.ErrorMessage);
		});
	}
	void OnDataRecieved(PlayerData data)
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
					if(amountInWallet + data.SpentAmount > 0)
					{
						AmountText.text = FormatNumber(data.Amount);
						WinningAmount = data.Amount;
					}
					else if(amountInWallet + data.Amount + data.SpentAmount > 0)
					{
						WinningAmount = amountInWallet + data.Amount + data.SpentAmount;
						AmountText.text = FormatNumber(WinningAmount);
					}
					else if(amountInWallet + data.Amount + data.SpentAmount <= 0)
					{
						WinningAmount = 0;
						AmountText.text = FormatNumber(0);
					}
				}
			}
		});
		PlayerNameText.text = "Logged in as " + data.PlayerName;
	}
	public static string FormatNumber(float number)
	{
		string[] suffixes = { "", "k", "M", "B" }; // Add more suffixes for larger numbers if needed
		int suffixIndex = 0;

		while (number >= 1000 && suffixIndex < suffixes.Length - 1)
		{
			number /= 1000;
			suffixIndex++;
		}

		return number.ToString("F1") + suffixes[suffixIndex];
	}

}
