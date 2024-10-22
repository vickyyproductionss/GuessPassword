using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase;
using Firebase.Auth;
using TMPro;
using Firebase.Extensions;
using Newtonsoft.Json;
using VickyCustoms;
using static PlayerDataManager;
using System;

namespace VickyCustoms
{
	public enum LockerDigits
	{
		Three, Four, Five, Six
	}
}

public class PFManager : MonoBehaviour
{
	#region References
	FirebaseAuth auth;
	public FirebaseFirestore firestore;
	bool IsFirebaseInitialised;

	[Header("Different Windows References")]
	[SerializeField] GameObject PassWindow;
	[SerializeField] GameObject NameWindow;
	[SerializeField] GameObject MessageWindow;
	[SerializeField] GameObject AskForAdsWindow;
	[SerializeField] GameObject AskForLockerTypeWindow;

	[Header("Locker Window Text References")]
	[SerializeField] TMP_Text FreeChancesText;
	[SerializeField] TMP_Text FailedAttempText;
	[SerializeField] TMP_Text TotalAmountInLockerText;
	[SerializeField] TMP_Text Locker_IDText;
	[SerializeField] TMP_Text WinningChancesText;
	[SerializeField] TMP_Text LockerInfoText;

	[Header("Instantiate Prefab Prerequisite")]
	[SerializeField] GameObject IncorrectPassPrefab;
	[SerializeField] GameObject IncorrectPassParent;

	[Header("Financial Section")]
	[SerializeField] TMP_InputField BankAccountNumber;
	[SerializeField] TMP_InputField BankHolderName;
	[SerializeField] TMP_InputField BankIFSCCode;
	[SerializeField] TMP_InputField UPIIdInput;
	[HideInInspector] public string ThreeDigitLockerID;
	[HideInInspector] public string FourDigitLockerID;
	[HideInInspector] public string FiveDigitLockerID;
	[HideInInspector] public string SixDigitLockerID;

	private string PlayerName;
	private string PlayerPassword;
	private string PlayerID;
	
	[HideInInspector] public ChestType ActiveChest;

	[Header("Different chest Scriptables")]
	public ChestType Wooden;
	public ChestType Golden;
	public ChestType Silver;
	public ChestType Diamond;
	public ChestType Youtube;

	[HideInInspector] public LockerDigits ActiveLockerDigits;
	private int TotalFailedAttemptsForCurrentLocker;
	public static PFManager instance;
	#endregion

	#region Unity Defaults
	private void Awake()
	{
		instance = this;
		Screen.orientation = ScreenOrientation.LandscapeRight;
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;
	}
	void Start()
    {
		InitialiseFirebase();
        Login();
		LoadingManager.instance.ShowLoading();
		ActiveLockerDigits = LockerDigits.Six;
    }
	#endregion

	#region Login
	private void Login()
	{
		PlayerID = PlayerPrefs.GetString("FirebaseUserId");
		var request = new LoginWithCustomIDRequest
		{
			CustomId = PlayerID,
			CreateAccount = true // Creates an account if it doesn't exist
		};

		PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
	}
	private void OnLoginSuccess(LoginResult result)
	{
		PlayerPrefs.SetString("PF_ID", result.PlayFabId);
		// You can now access player data or perform other actions
		// Save players unique device identifier to firebase firestore
		if (PlayerPrefs.GetInt("Registered") == 0)
		{
			PassWindow.SetActive(true);
		}
		GetCurrentLockerID();
		AskForDefaults();
	}
    private void OnEnable()
    {
        if(PlayFabClientAPI.IsClientLoggedIn())
		{
			Debug.Log("calling backas logged in");
            GetCurrentLockerID();
            AskForDefaults();
        }
    }

    private void OnLoginFailure(PlayFabError error)
	{
		//Show a connection error message and ask user to connect to internet...
		LoadingManager.instance.ShowLoading();
	}

	#endregion

	#region Initialise Game And Utility Functions
	void AskForDefaults()
	{
		if(!PlayerPrefs.HasKey("DefaultsSet"))
		{
			AskForLockerTypeWindow.SetActive(true);
		}
		else
		{
			string ChestType = PlayerPrefs.GetString("DefaultChest");
			LockerDigits digits = LockerDigits.Six;
			ActiveChest = Diamond;
			ActiveLockerDigits = LockerDigits.Six;
			if(ChestType == "Three")
			{
				ActiveLockerDigits = LockerDigits.Three;
				digits = LockerDigits.Three;
				ActiveChest = Wooden;
			}
			else if(ChestType == "Four")
			{
				ActiveLockerDigits = LockerDigits.Four;
				ActiveChest = Silver;
				digits = LockerDigits.Four;
			}
			else if(ChestType == "Five")
			{ 
				ActiveLockerDigits = LockerDigits.Five;
				ActiveChest = Golden;
				digits = LockerDigits.Five;
			}
			IPManager.instance.OnLockerTypeChanged(digits);
			GetTotalChancesForToday();
			GetTotalFailedAttempts();
		}
	}
	public void GetAllIncorrectPassesForCurrentLocker()
	{
		string ID = "";
		if (ActiveLockerDigits == LockerDigits.Three)
		{
			ID = ThreeDigitLockerID;
		}
		else if (ActiveLockerDigits == LockerDigits.Four)
		{
			ID = FourDigitLockerID;
		}
		else if (ActiveLockerDigits == LockerDigits.Five)
		{

			ID = FiveDigitLockerID;
		}
		else if (ActiveLockerDigits == LockerDigits.Six)
		{
			ID = SixDigitLockerID;
		}
		GetAllIncorrectPasswords(ID);
	}
	public string GetLockerIDFromDigitsCount(LockerDigits lockerDigits)
	{
		string LockerID = "Three";
		//Add code to find the correct lockerID
		if(lockerDigits == LockerDigits.Three)
		{
			LockerID = ThreeDigitLockerID;
		}
		else if(lockerDigits == LockerDigits.Four)
		{
			LockerID = FourDigitLockerID;
		}
		else if (lockerDigits == LockerDigits.Five)
		{
			LockerID = FiveDigitLockerID;
		}
		else
		{
			LockerID = SixDigitLockerID;
		}
		return LockerID;
	}
	void GetAllIncorrectPasswords(string _lockerID)
	{
		for(int i = IncorrectPassParent.transform.childCount-1; i >= 0 ; i--)
		{
			Destroy(IncorrectPassParent.transform.GetChild(i).gameObject);
		}
		CollectionReference WrongAttemptsRef = firestore.Collection("Lockers").Document(_lockerID).Collection("Attempts");
		WrongAttemptsRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
		{
			if(task.IsCompleted)
			{
				QuerySnapshot snapshot = task.Result;
				int index = 1;
				foreach(DocumentSnapshot documentSnapshot in snapshot.Documents)
				{
					GameObject WrongPass = Instantiate(IncorrectPassPrefab);
					WrongPass.transform.SetParent(IncorrectPassParent.transform, false);
					WrongPass.transform.GetChild(0).GetComponent<TMP_Text>().text = index.ToString()+".";
					string PName = documentSnapshot.GetValue<string>("Player");
					if(PName != "")
					{
						WrongPass.transform.GetChild(1).GetComponent<TMP_Text>().text = PName;
					}
					else
					{
						WrongPass.transform.GetChild(1).GetComponent<TMP_Text>().text = "Unknown player";
					}
					WrongPass.transform.GetChild(2).GetComponent<TMP_Text>().text = documentSnapshot.GetValue<string>("PasswordUsed");
					index++;
					if(index > 100)
					{
						return;
					}
				}
			}
		});
	}
	public void GetTotalChancesForToday()
	{
		var request = new ExecuteCloudScriptRequest
		{
			FunctionName = "getPlayerData",
			FunctionParameter = new { PlayerID = PlayerPrefs.GetString("PF_ID") },
			GeneratePlayStreamEvent = true
		};
		PlayFabClientAPI.ExecuteCloudScript(request, result =>
		{
			GetTotalFailedAttempts();
			PlayerData _playerData = JsonConvert.DeserializeObject<PlayerData>(result.FunctionResult.ToString());

		}, error =>
		{
			// Handle error response

		});
	}
	public string GetActiveLockerID()
	{
		LockerDigits lockerDigits = ActiveLockerDigits;
		string LockerID = "Three";
		//Add code to find the correct lockerID
		if (lockerDigits == LockerDigits.Three)
		{
			LockerID = ThreeDigitLockerID;
		}
		else if (lockerDigits == LockerDigits.Four)
		{
			LockerID = FourDigitLockerID;
		}
		else if (lockerDigits == LockerDigits.Five)
		{
			LockerID = FiveDigitLockerID;
		}
		else
		{
			LockerID = SixDigitLockerID;
		}
		return LockerID;
	}
	void GetCurrentLockerID()
	{
		// Prepare the Cloud Script request
		var request = new ExecuteCloudScriptRequest
		{
			FunctionName = "getThreeDigitLockerID",
			FunctionParameter = new { key = LockerDigits.Three.ToString() },
			GeneratePlayStreamEvent = true
		};

		// Call the Cloud Script function
		PlayFabClientAPI.ExecuteCloudScript(request, result =>
		{
			LockerData lockerData = JsonConvert.DeserializeObject<LockerData>(result.FunctionResult.ToString());
			ThreeDigitLockerID = lockerData.LockerID;
			
		}, error =>
		{
			// Handle error response
		});
		var request2 = new ExecuteCloudScriptRequest
		{
			FunctionName = "getFourDigitLockerID",
			FunctionParameter = new {key = LockerDigits.Four.ToString()},
			GeneratePlayStreamEvent = true
		};

		// Call the Cloud Script function
		PlayFabClientAPI.ExecuteCloudScript(request2, result =>
		{
			LockerData lockerData = JsonConvert.DeserializeObject<LockerData>(result.FunctionResult.ToString());
			FourDigitLockerID = lockerData.LockerID;
		}, error =>
		{
			// Handle error response
		});
		var request3 = new ExecuteCloudScriptRequest
		{
			FunctionName = "getFiveDigitLockerID",
			FunctionParameter = new { key = LockerDigits.Five.ToString() },
			GeneratePlayStreamEvent = true
		};

		// Call the Cloud Script function
		PlayFabClientAPI.ExecuteCloudScript(request3, result =>
		{
			LockerData lockerData = JsonConvert.DeserializeObject<LockerData>(result.FunctionResult.ToString());
			FiveDigitLockerID = lockerData.LockerID;
		}, error =>
		{
			// Handle error response
		});
		var request4 = new ExecuteCloudScriptRequest
		{
			FunctionName = "getSixDigitLockerID",
			FunctionParameter = new { key = LockerDigits.Six.ToString() },
			GeneratePlayStreamEvent = true
		};

		// Call the Cloud Script function
		PlayFabClientAPI.ExecuteCloudScript(request4, result =>
		{
			LockerData lockerData = JsonConvert.DeserializeObject<LockerData>(result.FunctionResult.ToString());
			SixDigitLockerID = lockerData.LockerID;
			GetTotalChancesForToday();
			DigitRevealManager.Instance.CheckPurchasedDigits();
			StartListeningForWrongPasswordsOnCurrentLocker();
		}, error =>
		{
			// Handle error response
			PFManager.instance.ShowMessage("Error", "Something went wrong!", "Error");
		});
	}

	public void UpdateWinningChancesForThisLocker()
	{
		int RevealedDigits = DigitRevealManager.Instance.RevealedDigits;
		if(RevealedDigits != 0)
		{
			RevealedDigits = ActiveChest.TotalPossiblePasswords - 1000;
		}
		float totalFailedAttempts = TotalFailedAttemptsForCurrentLocker;
		totalFailedAttempts += RevealedDigits;
		float totalChancesForLocker = ActiveChest.TotalPossiblePasswords;
		float winningPercentage = (totalFailedAttempts / totalChancesForLocker) * 100;
		string WinChanceInfo = $"Winning chances {winningPercentage.ToString("F3")} %";
		WinningChancesText.text = WinChanceInfo;
		UpdateLockerInfoText();
	}
	void UpdateLockerInfoText()
	{
		if (ActiveChest._lockerDigits == LockerDigits.Three)
		{
			LockerInfoText.text = "PASSWORD RANGE (100-999)";
		}
		else if (ActiveChest._lockerDigits == LockerDigits.Four)
		{
			LockerInfoText.text = "PASSWORD RANGE (1000-9999)";
		}
		else if (ActiveChest._lockerDigits == LockerDigits.Five)
		{
			LockerInfoText.text = "PASSWORD RANGE (10000-99999)";
		}
		else if (ActiveChest._lockerDigits == LockerDigits.Six)
		{
			LockerInfoText.text = "PASSWORD RANGE (100000-999999)";
		}
	}

	public void GetTotalFailedAttempts()
	{
		string ID = "";
		if (ActiveLockerDigits == LockerDigits.Three)
		{
			ID = ThreeDigitLockerID;
		}
		else if (ActiveLockerDigits == LockerDigits.Four)
		{
			ID = FourDigitLockerID;
		}
		else if (ActiveLockerDigits == LockerDigits.Five)
		{

			ID = FiveDigitLockerID;
		}
		else if (ActiveLockerDigits == LockerDigits.Six)
		{
			ID = SixDigitLockerID;
		}
		if (ID == "")
		{
			return;
		}
		CollectionReference lockerRef = FirebaseFirestore.DefaultInstance.Collection("Lockers").Document(ID).Collection("Attempts");
		lockerRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
		{
			if (task.IsCompleted)
			{
				QuerySnapshot snapshot = task.Result;
				int totalAttempts = snapshot.Count;
				TotalFailedAttemptsForCurrentLocker = totalAttempts;
				UpdateWinningChancesForThisLocker();
				WrongPasswordEntered(totalAttempts);
				UpdatePrizeMoney(totalAttempts);
			}
		});
		Locker_IDText.text = GetActiveLockerID();
		LoadingManager.instance.HideLoading();
	}
	private void InitialiseFirebase()
	{
		// Initialize Firebase components
		FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
		{
			var dependencyStatus = task.Result;
			if (dependencyStatus == DependencyStatus.Available)
			{
				OnFirebaseInitialised();
			}
			else
			{
				PFManager.instance.ShowMessage("Error", "Something went wrong!", "Error");
			}
		});
	}
	public void ShowMessage(string Title, string Description, string MessageType)
	{
		MessageWindow.SetActive(true);
		MessageWindow.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = Title;
		if(MessageType == "Success")
		{
			MessageWindow.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().color = Color.green;
		}
		else if(MessageType == "Warning")
		{
			MessageWindow.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().color = Color.yellow;
		}
		else if(MessageType == "Error")
		{
			MessageWindow.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().color = Color.red;
		}
		MessageWindow.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = Description;
	}
	private void OnFirebaseInitialised()
	{
		Debug.Log("Init Success");
		IsFirebaseInitialised = true;
		auth = FirebaseAuth.DefaultInstance;
		firestore = FirebaseFirestore.DefaultInstance;
	}
	#endregion

	#region Core Mechanics
	public void CheckMyPassword()
	{
        int Chances = PaymentHandler.chancedPurchased;
        if (Chances <= 0)
        {
            FreeChancesText.text = "No Free Chances Left";
            AskForAdsWindow.SetActive(true);
            return;
        }
        else if (Chances > 0)
        {
            FreeChancesText.text = Chances.ToString() + " Free Chances";
            MatchMyPassword(ActiveLockerDigits.ToString(), IPManager.instance.Password);
        }
    }
	private void MatchMyPassword(string key, string value)
	{
		if(!string.IsNullOrEmpty(value))
		{
            CollectionReference chancesRef = firestore.Collection("Users").Document(PlayerPrefs.GetString("FirebaseUserId")).Collection("PasswordTries");

            // Create a new document in "Purchases" collection with the fields price and chances
            Dictionary<string, object> PasswordData = new Dictionary<string, object>
            {
				{ "Type", key},
				{ "password", value },
			};

            // Add a new purchase document
            chancesRef
				.AddAsync(PasswordData)
				.ContinueWithOnMainThread(task =>
				{
					if (task.IsCompleted && !task.IsFaulted)
					{
						//Succeeded
						PaymentHandler.instance.DeductAChance();
					}
					else
					{
						//Failed
						return;
					}
            });
            float prizeMoney = (ActiveChest.StartingPrize + TotalFailedAttemptsForCurrentLocker * 0.013f);
			if (prizeMoney > ActiveChest.MaximumPrize)
			{
				prizeMoney = ActiveChest.MaximumPrize;
			}
			var request = new ExecuteCloudScriptRequest
			{
				FunctionName = "checkPasswordFromServer",
				GeneratePlayStreamEvent = true,
				FunctionParameter = new
				{
					key = key,
					value = value,
					attemptPrizeMoney = prizeMoney
				}
			};
			PlayFabClientAPI.ExecuteCloudScript(request, OnExecuteCloudScriptSuccess, OnExecuteCloudScriptFailure);
		}
		else
		{
			ShowMessage("Warning", "Please enter a password", "Warning");
		}
	}
	private void OnExecuteCloudScriptSuccess(ExecuteCloudScriptResult result)
	{
		IPManager.instance.AgainSelect();
		IPManager.instance.EmptyTheIP();
		LockerData lockerData = JsonConvert.DeserializeObject<LockerData>(result.FunctionResult.ToString());
		if(lockerData != null )
		{
			if(lockerData.Status == "Locked")
			{
				IPManager.instance.ShowFailure();
				SaveLockerData(lockerData.LockerID,lockerData.PasswordUsed,lockerData.Status);
			}
			else if(lockerData.Status == "Unlocked")
			{
				IPManager.instance.ShowSuccess();
				SaveUnlockedLockerData(lockerData.LockerID, lockerData.PasswordUsed, lockerData.Status);
			}
		}
		GetCurrentLockerID();
		DigitRevealManager.Instance.CheckPurchasedDigits();
	}

	void SaveUnlockedLockerData(string lockerID, string PasswordUsed, string status)
	{
		Dictionary<string, object> attempt = new Dictionary<string, object>
		{
			{ "LockerID", lockerID },
			{ "PasswordUsed", PasswordUsed },
			{ "Status", status },
			{ "DeviceId",  SystemInfo.deviceUniqueIdentifier},
			{ "Player", PlayerPrefs.GetString("PName")}
		};
		Dictionary<string, object> winner = new Dictionary<string, object>
		{
			{ "LockerID", lockerID },
			{ "PasswordUsed", PasswordUsed },
			{ "Status", status },
			{ "DeviceId",  SystemInfo.deviceUniqueIdentifier},
			{ "Winner", PlayerPrefs.GetString("PName") }
		};
		Dictionary<string, object> UnlockedLocker = new Dictionary<string, object>
		{
			{ "LockerID", lockerID },
			{ "PasswordUsed", PasswordUsed.ToString() },
			{ "UnlockedAt", DateTime.Now.ToString()},
			{ "DeviceId",  SystemInfo.deviceUniqueIdentifier},
			{ "Winner", PlayerPrefs.GetString("PName") },
			{ "AttemptNumber", (TotalFailedAttemptsForCurrentLocker + 1).ToString()}
		};
		DocumentReference lockerRef = firestore.Collection("Lockers").Document(lockerID).Collection("Attempts").Document(PasswordUsed);
		DocumentReference unlockedLockerRef = firestore.Collection("Lockers").Document(lockerID);
		DocumentReference playerRef = firestore.Collection("users").Document(SystemInfo.deviceUniqueIdentifier).Collection("Lockers").Document(lockerID).Collection("Attempts").Document(PasswordUsed);

		DocumentReference UnlockedLockerDataRef = firestore.Collection("Unlocked").Document(lockerID);

		lockerRef.SetAsync(attempt).ContinueWithOnMainThread(task =>
		{
			if (task.IsCompleted)
			{
				playerRef.SetAsync(attempt).ContinueWithOnMainThread(task =>
				{
					if (task.IsCompleted)
					{
						unlockedLockerRef.SetAsync(winner).ContinueWithOnMainThread(task =>
						{
							if (task.IsCompleted)
							{

							}
							else if (task.IsFaulted)
							{

							}
						});
					}
					else if (task.IsFaulted)
					{
						unlockedLockerRef.SetAsync(winner).ContinueWithOnMainThread(task =>
						{
							if (task.IsCompleted)
							{

							}
							else if (task.IsFaulted)
							{

							}
						});
					}
				});
			}
			else if (task.IsFaulted)
			{
				playerRef.SetAsync(attempt).ContinueWithOnMainThread(task =>
				{
					if (task.IsCompleted)
					{
						unlockedLockerRef.SetAsync(winner).ContinueWithOnMainThread(task =>
						{
							if (task.IsCompleted)
							{

							}
							else if (task.IsFaulted)
							{

							}
						});
					}
					else if (task.IsFaulted)
					{
						unlockedLockerRef.SetAsync(winner).ContinueWithOnMainThread(task =>
						{
							if (task.IsCompleted)
							{

							}
							else if (task.IsFaulted)
							{

							}
						});
					}
				});
			}
		});

		UnlockedLockerDataRef.SetAsync(UnlockedLocker).ContinueWithOnMainThread(task =>
		{
			if(task.IsCompleted)
			{

			}
		});
	}

	void SaveLockerData(string lockerID, string PasswordUsed, string status)
	{
		string PName = PlayerPrefs.GetString("PName").ToString();
		Dictionary<string, object> attempt = new Dictionary<string, object>
		{
			{ "LockerID", lockerID },
			{ "PasswordUsed", PasswordUsed },
			{ "Status", status },
			{ "DeviceId",  SystemInfo.deviceUniqueIdentifier},
			{ "Player", PName}

		};
		DocumentReference lockerRef = firestore.Collection("Lockers").Document(lockerID).Collection("Attempts").Document(PasswordUsed);
		DocumentReference playerRef = firestore.Collection("users").Document(SystemInfo.deviceUniqueIdentifier).Collection("Lockers").Document(lockerID).Collection("Attempts").Document(PasswordUsed);
		
		lockerRef.SetAsync(attempt).ContinueWithOnMainThread(task =>
		{
			if (task.IsCompleted)
			{
				playerRef.SetAsync(attempt).ContinueWithOnMainThread(task =>
				{
					if (task.IsCompleted)
					{

					}
					else if (task.IsFaulted)
					{

					}
				});
			}
			else if (task.IsFaulted)
			{
				playerRef.SetAsync(attempt).ContinueWithOnMainThread(task =>
				{
					if (task.IsCompleted)
					{

					}
					else if (task.IsFaulted)
					{

					}
				});
			}
		});
	}

	private void OnExecuteCloudScriptFailure(PlayFabError error)
	{
		PFManager.instance.ShowMessage("Error", "Something went wrong!", "Error");
	}
	public void OnWatchingAdFinished()
	{
		var request = new ExecuteCloudScriptRequest
		{
			FunctionName = "GiveAdReward",
			GeneratePlayStreamEvent = true
		};

		// Call the Cloud Script function
		PlayFabClientAPI.ExecuteCloudScript(request, result =>
		{
			if (result.FunctionResult != null)
			{
				if (result.FunctionResult.ToString() == "1")
				{
					GetTotalChancesForToday();
				}
			}
			//Handle purchase success here
		}, error =>
		{
			// Handle error response
		});
		AskForAdsWindow.SetActive(false);
	}
	#endregion

	#region Custom Classes

	[System.Serializable]
	public class LockerData
	{
		public string LockerID;
		public string PasswordUsed;
		public string Status;
	}
	[System.Serializable]
	public class TodayDate
	{
		public string LockerID;
		public string PasswordUsed;
		public string Status;
		public string CurrentDate;
	}

	[System.Serializable]
	public class BankDetails
	{
		public string AccountNumber;
		public string IFSCCode;
		public string BankingName;
	}

	#endregion

	#region Financial Data Handling
	public void UpdateBankDetails()
	{
		if(!string.IsNullOrEmpty(BankAccountNumber.text) && !string.IsNullOrEmpty(BankIFSCCode.text))
		{
			PlayerPrefs.SetString("BankAccountNumber",BankAccountNumber.text);
			PlayerPrefs.SetString("AccountHolderName",BankHolderName.text);
			PlayerPrefs.SetString("IFSC",BankIFSCCode.text);

			DocumentReference lockerRef = FirebaseFirestore.DefaultInstance.Collection("users").Document(SystemInfo.deviceUniqueIdentifier).Collection("BankDetails").Document(BankAccountNumber.text);
			Dictionary<string, object> BankDetails = new Dictionary<string, object>
			{
				{ "AccountNumber", BankAccountNumber.text },
				{ "AccountHolderName", BankHolderName.text },
				{ "IFSC", BankIFSCCode.text }
			};
			lockerRef.SetAsync(BankDetails).ContinueWithOnMainThread(task =>
			{
				if(task.IsCompleted)
				{
					ShowMessage("Success", "Bank Details Updated Successfully", "Success");
					PlayerPrefs.SetInt("UPIDetails", 1);
				}
				else
				{
					ShowMessage("Error !!!", "Bank Details Update Failed", "Error");
				}
			});
		}
		else
		{
			ShowMessage("Incorrect !!!", "Please Fill All Details", "Error");
		}
	}
	public void UpdateUPIId()
	{
		if (!string.IsNullOrEmpty(UPIIdInput.text))
		{
			PlayerPrefs.SetString("UPIId", UPIIdInput.text);
			DocumentReference lockerRef = FirebaseFirestore.DefaultInstance.Collection("users").Document(SystemInfo.deviceUniqueIdentifier).Collection("BankDetails").Document("UPI");
			Dictionary<string, object> BankDetails = new Dictionary<string, object>
			{
				{ "UPIId", UPIIdInput.text }
			};
			lockerRef.SetAsync(BankDetails).ContinueWithOnMainThread(task =>
			{
				if (task.IsCompleted)
				{
					ShowMessage("Success", "UPI Updated Successfully", "Success");
					PlayerPrefs.SetInt("UPIDetails", 1);
				}
				else
				{
					ShowMessage("Error !!!", "UPI Update Failed", "Error");
				}
			});
		}
		else
		{
			ShowMessage("Incorrect !!!", "Please Enter UPI ID.", "Error");
		}
	}

	public void CreateWithdrawalRequest(TMP_InputField Amount)
	{
		if (Amount.text == null || Amount.text == "0")
		{
			return;
		}
		int AmountRequested = int.Parse(Amount.text);
		if(AmountRequested < 25)
		{
			ShowMessage("Less Amount !!!", "Minimum withdrawal limit is INR 25", "Warning");
			return;
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
			PlayerData _playerData = JsonConvert.DeserializeObject<PlayerData>(result.FunctionResult.ToString());
			if(AmountRequested > _playerData.Amount)
			{
				ShowMessage("Not Enough Balance", "You don't have that much money in wallet.", "Error");
			}
			else
			{
				DocumentReference lockerRef = FirebaseFirestore.DefaultInstance.Collection("Withdrawals").Document(SystemInfo.deviceUniqueIdentifier);
				Dictionary<string, object> BankDetails = new Dictionary<string, object>
				{
					{ "AmountRequested", AmountRequested}
				};
				lockerRef.SetAsync(BankDetails).ContinueWithOnMainThread(task =>
				{
					if (task.IsCompleted)
					{
						ShowMessage("Success", "Withdrawal Request Placed Successfully", "Success");
					}
					else
					{
						ShowMessage("Error !!!", "Withdrawal Request Failed", "Error");
					}
				});
				}
		}, error =>
		{
			// Handle error response
			PFManager.instance.ShowMessage("Error", "Something went wrong!", "Error");
		});
	}

	#endregion

	#region Register Player
	public void OnClick_Register(TMP_InputField PlayerNameField)
	{
		if(!string.IsNullOrEmpty(PlayerNameField.text))
		{
			PlayerName = PlayerNameField.text;
			SavePlayerData(PlayerID, PlayerName, PlayerPassword);
		}
		else
		{
			ShowMessage("Warning", "Enter name please", "Warning");
		}
	}
	public void SavePlayerData(string deviceId, string playerName, string password)
	{
		UpdateDisplayName(playerName);
		PlayerPrefs.SetString("PName", playerName);
		if(!IsFirebaseInitialised)
		{
			NameWindow.SetActive(false);
            Debug.Log("Failed2");
            ShowMessage("OOPS !!!", "Something went wrong \n Check your connection", "Error");
			return;
		}
		// Create a new user document in Firestore
		Dictionary<string, object> user = new Dictionary<string, object>
		{
			{ "DeviceID", deviceId },
			{ "Name", playerName},
			{ "ProfileImg", "" },
			{ "Email", "" },
			{ "Password", password }
		};
		DocumentReference playerRef = firestore.Collection("users").Document(PlayerID);
		// Add the user data to Firestore
		playerRef.SetAsync(user).ContinueWithOnMainThread(task =>
		{
			if (task.IsCompleted)
			{
				RegisterSuccess();
			}
			else if (task.IsFaulted)
			{
				RegisterFailed();
			}
		});
	}
	private void UpdateDisplayName(string newDisplayName)
	{
		var request = new UpdateUserTitleDisplayNameRequest
		{
			DisplayName = newDisplayName
		};
		PlayerPrefs.SetString("Name", newDisplayName);

		PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdated, OnError);
	}
	private void OnDisplayNameUpdated(UpdateUserTitleDisplayNameResult result)
	{
		NameWindow.SetActive(false);
        PFManager.instance.ShowMessage("Message", "Name Changed Successfully", "Success");
    }
	private void OnError(PlayFabError error)
	{
		Debug.Log(error);
		PFManager.instance.ShowMessage("Error", "Something went wrong!\n Report at vickychaudhary8955@gmail.com", "Error");
	}
	void RegisterSuccess()
	{
		NameWindow.SetActive(false);
		PlayerPrefs.SetInt("Registered", 1);
		PlayerPrefs.SetInt("Name", 1);
		ShowMessage("Congratulations !!!", "You're registered successfully", "Success");
	}
	void RegisterFailed()
	{
		NameWindow.SetActive(false);
		Debug.Log("Failed");
		ShowMessage("OOPS !!!", "Something went wrong \n Check your connection", "Error");
	}
	public void OnClick_ContinueInPassWindow(TMP_InputField PlayerPassField)
	{
		if(PlayerPassField.text.Length < 6)
		{
			ShowMessage("Warning", "Password is weak \nPassword length is less than 6 characters\n Enter again", "Warning");
			return;
		}
		PlayerPassword = PlayerPassField.text;
		if(!PlayerPrefs.HasKey("Name"))
		{
			PassWindow.SetActive(false);
			NameWindow.SetActive(true);
		}
		else
		{
			PlayerPrefs.DeleteKey("Name");
            PassWindow.SetActive(false);
            NameWindow.SetActive(true);
        }
	}

	#endregion

	#region Listen For Firestore Changes
	bool ListeningLiveForChanges;
	public void StartListeningForWrongPasswordsOnCurrentLocker()
	{
		StartCoroutine(InitialiseLiveListeningForChanges());
		//If Other player entered a wrong password
		CollectionReference lockerRef = FirebaseFirestore.DefaultInstance.Collection("Lockers").Document(GetActiveLockerID()).Collection("Attempts");
		if(string.IsNullOrEmpty(GetActiveLockerID()))
		{
			ListeningLiveForChanges = false;
			return;
		}
		ListeningLiveForChanges = true;
		ListenerRegistration listener = lockerRef.Listen(snapshot => {
			WrongPasswordEntered(snapshot.Count);
		});
	}
	IEnumerator InitialiseLiveListeningForChanges()
	{
		yield return new WaitForSeconds(1);
		if(!ListeningLiveForChanges)
		{
			StartListeningForWrongPasswordsOnCurrentLocker();
		}
	}
	void WrongPasswordEntered(int passwordsCount)
	{
		TotalFailedAttemptsForCurrentLocker = passwordsCount;
		UpdateWinningChancesForThisLocker();
		UpdatePrizeMoney(passwordsCount);
		FailedAttempText.text = passwordsCount.ToString() + " Incorrect Passwords";
	}
	void UpdatePrizeMoney(int totalAttempts)
	{
		float prizeMoney = (ActiveChest.StartingPrize + totalAttempts * 0.013f);
		if (prizeMoney > ActiveChest.MaximumPrize)
		{
			prizeMoney = ActiveChest.MaximumPrize;
		}
		TotalAmountInLockerText.text = "Unlock and get INR " + prizeMoney.ToString();
	}
	#endregion
}
//
//1000Lines completed by a comment