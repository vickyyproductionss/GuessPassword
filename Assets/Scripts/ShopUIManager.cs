using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopUIManager : MonoBehaviour
{
    [SerializeField] TMP_Text CashInWalletParent;
    [SerializeField] TMP_Text CashInWalletChild;
    [SerializeField] TMP_Text WinningParent;
    [SerializeField] TMP_Text WinningChild;

	private void Update()
	{
		if(CashInWalletChild.text != CashInWalletParent.text)
		{
			CashInWalletChild.text = CashInWalletParent.text;
		}
	}
}
