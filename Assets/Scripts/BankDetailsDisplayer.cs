using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BankDetailsDisplayer : MonoBehaviour
{
    [SerializeField] TMP_Text BankAccountNumberText;
    [SerializeField] TMP_Text BankAccountIFSCText;
    [SerializeField] TMP_Text UPIIdText;
    void OnEnable()
    {
        string AccNumber = PlayerPrefs.GetString("BankAccountNumber", "N/A");
        string IFSCCode = PlayerPrefs.GetString("IFSC", "N/A");
        string UPIId = PlayerPrefs.GetString("UPIId","N/A");
		BankAccountNumberText.text = $"Account Number: {AccNumber}";
        BankAccountIFSCText.text = $"IFSC: {IFSCCode}";
        UPIIdText.text = $"UPI ID: {UPIId}";
    }
}
