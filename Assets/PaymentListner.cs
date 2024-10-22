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
    ListenerRegistration listenerRegistration;
    public void ListenForPayments()
    {
        string userId = PlayerPrefs.GetString("FirebaseUserId");
        CollectionReference paymentsRef = firestore.Collection("Users").Document(userId).Collection("Payments");

        // Set up the listener to listen for real-time updates
        listenerRegistration = paymentsRef.Listen(snapshot =>
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

                            if (status == "captured" && int.Parse(amount)/100 == PlayerPrefs.GetInt("LastAmount"))
                            {
                                ShowPaymentSuccessMessage(amount);
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
        PlayerPrefs.DeleteKey("LastAmount");
        // This is just an example. You can show a message or update the UI to inform the user about the payment.
        PaymentStatusText.text = $"We Recieved Payment of {int.Parse(amount)/100} INR successfully!";
        SuccessIcon.SetActive(true);
        Destroy(webview.webViewObject);
        webview.gameObject.SetActive(false);
        listenerRegistration.Stop();
    }
    private void Update()
    {
        if(PaymentStatusText.text.Contains("We Recieved Payment of") && webview.webViewObject != null && webview.gameObject.activeInHierarchy)
        {
            Destroy(webview.webViewObject);
            webview.gameObject.SetActive(false);
        }
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
