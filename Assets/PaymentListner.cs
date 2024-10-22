using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using TMPro;
using UnityEngine;

public class PaymentListner : MonoBehaviour
{
    public FirebaseFirestore firestore;
    public SampleWebView webview;
    public static PaymentListner Instance;
    public GameObject PaymentStatusPanel;
    public GameObject SuccessIcon;
    public TMP_Text PaymentStatusText;

    private void Awake()
    {
        Instance = this;
        firestore = FirebaseFirestore.DefaultInstance;
        ListenForPayments();
    }
    public void ListenForPayments()
    {
        string userId = PlayerPrefs.GetString("PF_ID");
        CollectionReference paymentsRef = firestore.Collection("Users").Document(userId).Collection("Payments");

        // Set up the listener to listen for real-time updates
        paymentsRef.Listen(snapshot =>
        {
            if (snapshot != null && snapshot.Documents != null)
            {
                foreach (var change in snapshot.GetChanges())
                {
                    // We're only interested in newly added documents
                    if (change.ChangeType == DocumentChange.Type.Added)
                    {
                        var documentSnapshot = change.Document;

                        if (documentSnapshot.Exists)
                        {
                            Dictionary<string, string> docValues = GetKeyValuesOfDocument(documentSnapshot);
                            string amount = docValues.GetValueOrDefault("payload.payment.entity.amount");
                            string status = docValues.GetValueOrDefault("payload.payment.entity.status");

                            if (status == "captured")
                            {
                                Debug.Log("New payment captured with amount: " + amount);
                                // Update UI or show message for captured payment
                                ShowPaymentSuccessMessage(amount);

                                // You can handle the latest added document here
                                return; // Exit after processing the latest added document
                            }
                        }
                    }
                }

            }
            else
            {
                Debug.LogWarning("No documents found in the Payments collection");
            }
        });
    }


    private void ShowPaymentSuccessMessage(string amount)
    {
        // This is just an example. You can show a message or update the UI to inform the user about the payment.
        Debug.Log($"Payment of {int.Parse(amount)/100} INR was successfully captured!");
        PaymentStatusText.text = $"We Recieved Payment of {int.Parse(amount)/100} INR successfully!";
        SuccessIcon.SetActive(true);
        Destroy(webview.webViewObject);
        webview.gameObject.SetActive(false);
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
                ExtractKeyValues(nestedDict, result, currentKey);
            }
            else if (item.Value is IList<object> nestedList)
            {
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
                result.Add(currentKey, item.Value?.ToString());
            }
        }
    }
}
