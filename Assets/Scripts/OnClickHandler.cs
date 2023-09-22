using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Windows;

public class OnClickHandler : MonoBehaviour
{
	[SerializeField] TMP_Text PasswordText;
	public void OnClickSeeAllLosers(TMP_Text LockerID)
	{
		string prefix = "Password: ";
		string CorrectPass = "";
		if (PasswordText.text.StartsWith(prefix))
		{
			CorrectPass = PasswordText.text.Substring(prefix.Length);
		}

		GetLosersForALocker.instance.GetAllIncorrectPasswords(LockerID.text,CorrectPass);
	}
}
