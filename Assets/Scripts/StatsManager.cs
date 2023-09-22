using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
	[SerializeField] TMP_Text Title;
	[SerializeField] TMP_Text Value;
	[SerializeField] GameObject WithdrawBtnGrp;
	[SerializeField] GameObject WithdrawWindow;
	[SerializeField] GameObject GetUPIDetailsWindow;

	private void Start()
	{
		Title.text = "Win amount";
		Value.text = PlayerDataManager.WalletBalance.ToString();
	}
	public void NextStat()
	{
		if(Title.text == "Win amount")
		{
			WithdrawBtnGrp.SetActive(false);
			Title.text = "Total Passwords Tried";
			Value.text = PlayerDataManager.Instance.playerData.PasswordsTried.ToString();
		}
		else if(Title.text == "Total Passwords Tried")
		{
			WithdrawBtnGrp.SetActive(false);
			Title.text = "Total Lockers Unlocked";
			Value.text = PlayerDataManager.Instance.playerData.UnlockedLockers.ToString();
		}
		else if (Title.text == "Total Lockers Unlocked")
		{
			WithdrawBtnGrp.SetActive(false);
			Title.text = "Name";
			Value.text = PlayerDataManager.Instance.playerData.PlayerName.ToString();
		}
		else if (Title.text == "Name")
		{
			WithdrawBtnGrp.SetActive(true);
			Title.text = "Win amount";
			Value.text = PlayerDataManager.WalletBalance.ToString();
		}

	}
	public void PreviousStat() 
	{
		if (Title.text == "Win amount")
		{
			WithdrawBtnGrp.SetActive(false);
			Title.text = "Name";
			Value.text = PlayerDataManager.Instance.playerData.PlayerName.ToString();
		}
		else if (Title.text == "Total Passwords Tried")
		{
			WithdrawBtnGrp.SetActive(true);
			Title.text = "Win amount";
			Value.text = PlayerDataManager.WalletBalance.ToString();
		}
		else if (Title.text == "Total Lockers Unlocked")
		{
			WithdrawBtnGrp.SetActive(false);
			Title.text = "Total Passwords Tried";
			Value.text = PlayerDataManager.Instance.playerData.PasswordsTried.ToString();
		}
		else if (Title.text == "Name")
		{
			WithdrawBtnGrp.SetActive(false);
			Title.text = "Total Lockers Unlocked";
			Value.text = PlayerDataManager.Instance.playerData.UnlockedLockers.ToString();
		}
	}
	public void OnClickWithdraw()
	{
		if(!PlayerPrefs.HasKey("UPIDetails"))
		{
			GetUPIDetailsWindow.SetActive(true);
		}
		else
		{
			WithdrawWindow.SetActive(true);
		}
	}
}
