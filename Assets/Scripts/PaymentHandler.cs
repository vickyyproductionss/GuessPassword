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
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;

public class PaymentHandler : MonoBehaviour
{
    [SerializeField] TMP_Text AmountInWalletText;
    [SerializeField] TMP_Text FreeChancesText;
	[SerializeField] GameObject AddFundsWindow;
    public static int amountInWallet = 0;
    public static int chancedPurchased = 0;
	public FirebaseFirestore firestore;
	public static PaymentHandler instance;

	public List<Item> items = new List<Item>();

	[Serializable]
	public class Item
	{
		public int price;
		public int chancesToAdd;
	}
    public void PurchaseForPrice(int price)
    {
        foreach(var item in items)
        {
            if(item.price == price)
            {
                if(amountInWallet > price)
                {
                    PurchaseThisitem(price, item.chancesToAdd);
                }
                else
                {
                    Debug.Log("Not enough balance");
                }
            }
        }
    }
    public void PurchaseForDigits(int price)
    {
        PurchaseThisService(price, "DigitsRevealed");
    }
    void PurchaseThisService(int price, string service)
    {
        CollectionReference purchasesRef = firestore.Collection("Users").Document(PlayerPrefs.GetString("FirebaseUserId")).Collection("Purchases");

        // Create a new document in "Purchases" collection with the fields price and chances
        Dictionary<string, object> purchaseData = new Dictionary<string, object>
        {
            { "price", price },
            { "service", service }
        };

        // Add a new purchase document
        purchasesRef.AddAsync(purchaseData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("Service successfully added!");
                StartCoroutine(CheckBalance());
            }
            else
            {
                Debug.LogError("Error adding purchase: " + task.Exception);
            }
        });
    }

    void PurchaseThisitem(int price, int chances)
    {
        CollectionReference purchasesRef = firestore.Collection("Users").Document(PlayerPrefs.GetString("FirebaseUserId")).Collection("Purchases");

        // Create a new document in "Purchases" collection with the fields price and chances
        Dictionary<string, object> purchaseData = new Dictionary<string, object>
        {
            { "price", price },
            { "chances", chances }
        };

        // Add a new purchase document
        purchasesRef.AddAsync(purchaseData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("Purchase successfully added!");
                StartCoroutine(CheckBalance());
            }
            else
            {
                Debug.LogError("Error adding purchase: " + task.Exception);
            }
        });
    }

    public Dictionary<string, string> GetKeyValuesOfDocument(DocumentSnapshot docs)
    {
        Dictionary<string, string> keyValues = new Dictionary<string, string>();

        if (docs.Exists)
        {
            var docData = docs.ToDictionary();
            ExtractKeyValues(docData, keyValues, "");
        }

        return keyValues;
    }

    private void ExtractKeyValues(IDictionary<string, object> data, Dictionary<string, string> result, string parentKey)
    {
        foreach (var item in data)
        {
            string currentKey = string.IsNullOrEmpty(parentKey) ? item.Key : parentKey + "." + item.Key;

            if (item.Value is IDictionary<string, object> nestedDict)
            {
                // Recursively extract key-values for nested dictionaries
                ExtractKeyValues(nestedDict, result, currentKey);
            }
            else if (item.Value is IList<object> nestedList)
            {
                // Handle lists if needed
                for (int i = 0; i < nestedList.Count; i++)
                {
                    if (nestedList[i] is IDictionary<string, object> listItemDict)
                    {
                        ExtractKeyValues(listItemDict, result, currentKey + "[" + i + "]");
                    }
                    else
                    {
                        result.Add(currentKey + "[" + i + "]", nestedList[i]?.ToString());
                    }
                }
            }
            else
            {
                // Add the current key-value pair if it's not nested
                result.Add(currentKey, item.Value?.ToString());
            }
        }
    }
    public void DeductAChance()
    {
        chancedPurchased -= 1;
        FreeChancesText.text = chancedPurchased.ToString() + " Free Chances";
    }
    bool RunningAyncFunction = false;
    IEnumerator CheckBalance()
    {
        yield return new WaitForSeconds(1);
        if(RunningAyncFunction)
        {
            Debug.Log("Going back");
            yield break;
        }
        string userId = PlayerPrefs.GetString("FirebaseUserId");
        Debug.Log(userId + " this is userid");
        // Reference to the user's Payments sub-collection in Firestore
        CollectionReference paymentsRef = firestore.Collection("Users").Document(userId).Collection("Payments");

        QuerySnapshot snapshot = null;
        double totalCapturedAmount = 0;

        // Fetch all payments for the user
        RunningAyncFunction = true;
        paymentsRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                snapshot = task.Result;
                foreach(var child in snapshot.Documents)
                {
                    Dictionary<string, string> docvaluesv = GetKeyValuesOfDocument(child);
                    string amount = docvaluesv.GetValueOrDefault("payload.payment.entity.amount");
                    string status = docvaluesv.GetValueOrDefault("payload.payment.entity.status");
                    if(status == "captured")
                    {
                        double amt = double.Parse(amount);
                        totalCapturedAmount += amt;
                    }
                }
            }
            else
            {
                Debug.LogError("Error fetching payments: " + task.Exception);
            }
        });

        CollectionReference purchasesRef = firestore.Collection("Users").Document(PlayerPrefs.GetString("FirebaseUserId")).Collection("Purchases");
        QuerySnapshot purchase_snapshot = null;
        double _chances = 0;
        // Fetch all payments for the user
        purchasesRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                purchase_snapshot = task.Result;
                foreach (var child in purchase_snapshot.Documents)
                {
                    Dictionary<string, string> docvaluesv = GetKeyValuesOfDocument(child);
                    string amount = docvaluesv.GetValueOrDefault("price");
                    double amt = double.Parse(amount);
                    string chances = docvaluesv.GetValueOrDefault("chances");
                    if(!string.IsNullOrEmpty(chances))
                    {
                        double chnces = double.Parse(chances);
                        _chances += chnces;
                    }
                    amt *= 100;
                    totalCapturedAmount -= amt;
                }
            }
            else
            {
                Debug.LogError("Error fetching payments: " + task.Exception);
            }
        });

        CollectionReference chancesRef = firestore.Collection("Users").Document(PlayerPrefs.GetString("FirebaseUserId")).Collection("PasswordTries");
        QuerySnapshot chances_snapshot = null;
        // Fetch all payments for the user
        chancesRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                chances_snapshot = task.Result;
                _chances -= chances_snapshot.Count;
            }
            else
            {
                Debug.LogError("Error fetching payments: " + task.Exception);
            }
        });
        // Wait for the task to complete
        yield return new WaitUntil(() => snapshot != null);

        // Variable to hold the total captured amount
        Debug.Log("Total Captured Amount (INR): " + totalCapturedAmount / 100);
        amountInWallet = (int) totalCapturedAmount / 100;
        chancedPurchased = (int)_chances;
        AmountInWalletText.text = amountInWallet.ToString();
        FreeChancesText.text = chancedPurchased.ToString() + " Free Chances";
        RunningAyncFunction = false;
    }

    private void Awake()
	{
        firestore = FirebaseFirestore.DefaultInstance;
		instance = this;
        StartCoroutine(CheckBalance());
	}
    public void AddMoney()
	{
		Screen.orientation = ScreenOrientation.Portrait;
		SceneManager.LoadScene(2);
	}
}
