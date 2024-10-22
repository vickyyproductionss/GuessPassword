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
	public static int WalletBalance;
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
}
