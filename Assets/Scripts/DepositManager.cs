using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DepositManager : MonoBehaviour
{
    public string baseURL = "https://my-ap-is-theta.vercel.app/api/payments/verification";
    public TMP_InputField AmountInput;
    public Button PayButton;
    public TMP_Text PayButtonText;
    public TMP_Text MessageText;

    public SampleWebView sampleWebView;

    private void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
    }
    public void GoToHome()
    {
        Screen.orientation = ScreenOrientation.LandscapeRight;
        SceneManager.LoadScene(1);
    }

    public void OnAmountEditEnd()
    {
        MessageText.text = "Minimum Amount is INR 10.";
        if (string.IsNullOrEmpty(AmountInput.text) || AmountInput.text == "0")
        {
            PayButton.interactable = false;
            return;
        }
        int amount = int.Parse(AmountInput.text);
        if (amount < 10)
        {
            PayButton.interactable = false;
            return;
        }
        MessageText.text = "";
        PayButton.interactable = true;
        PayButtonText.text = $"Add {amount}";
    }
    public void CreatePaymentLink()
    {
        PayButton.interactable = false;
        int amount = int.Parse(AmountInput.text);
        PlayerPrefs.SetInt("LastAmount", amount);
        string customerName = PlayerPrefs.GetString("PName", "GuessThePassword_Player");
        string customerEmail = PlayerPrefs.GetString("Email", "");
        string customerPhone = PlayerPrefs.GetString("Phone", "");
        string description = "GTP Wallet";
        string userID = PlayerPrefs.GetString("FirebaseUserId");
        string URLWithData = baseURL + $"?amount={amount}&customerName={customerName}&customerEmail={customerEmail}&customerContact={customerPhone}&description={description}&userid={userID}";
        StartCoroutine(GetPaymentLink(URLWithData));
    }
    private IEnumerator GetPaymentLink(string url)
    {
        Debug.Log("URL:" + url);
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Send the request and wait for the response
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Response: " + request.downloadHandler.text);

                // Parse JSON response
                string jsonResponse = request.downloadHandler.text;
                JObject parsedResponse = JObject.Parse(jsonResponse);

                // Extract the payment_link from the JSON
                string paymentLink = parsedResponse["payment_link"].ToString();

                Debug.Log("Payment Link: " + paymentLink);

                // Call the function where you'll use the payment link
                HandlePaymentLink(paymentLink);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }

    // Function where you can use the payment link
    private void HandlePaymentLink(string paymentLink)
    {
        sampleWebView.gameObject.SetActive(true);
        sampleWebView.enabled = true;
        sampleWebView.Url = paymentLink;
    }

    public void CloseWebView()
    {
        GameObject WebView = GameObject.Find("WebViewObject");
        Destroy(WebView);
    }
}
