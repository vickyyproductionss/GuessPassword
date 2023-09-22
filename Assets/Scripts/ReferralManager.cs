using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ReferralManager : MonoBehaviour
{
	public FirebaseFirestore firestore;

	public void CreateMyReferalCode()
	{
		DocumentReference ReferalRef = firestore.Collection("Referrals").Document(SystemInfo.deviceUniqueIdentifier);
		ReferalRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
		{
			if(task.IsCompleted)
			{
				DocumentSnapshot snapshot = task.Result;
				if(snapshot.Exists)
				{
					//Handle when user already created a referal code.
					string Code = snapshot.GetValue<string>("Code");
					string Name = snapshot.GetValue<string>("RefereeName");
					ShowMyCode(Name,Code);
				}
				else
				{
					//What if user don't have a referal code
					string MyCode = SystemInfo.deviceUniqueIdentifier.Substring(0, 6) + (Random.Range(1000, 9999)).ToString();
					string MyName = PlayerPrefs.GetString("PName");
				    Dictionary<string, object> ReferalCode = new Dictionary<string, object> 
					{
						{"Code",MyCode },
						{"RefereeName",MyName }
					};
					ReferalRef.SetAsync(ReferalCode).ContinueWithOnMainThread(task2 =>
					{
						if(task2.IsCompleted)
						{
							ShowMyCode(MyName, MyCode);
						}
						else
						{
							PFManager.instance.ShowMessage("Error_1", "Referal Code Not Generated\n Check Your Connection.", "Error");
						}
					});

				}
			}
			else
			{
				PFManager.instance.ShowMessage("Error_1", "Referal Code Not Generated\n Check Your Connection.", "Error");
			}
		});
	}

	public void ApplyMyCode(TMP_InputField CodeInput)
	{
		CollectionReference Codes = firestore.Collection("Referrals");
		Codes.GetSnapshotAsync().ContinueWithOnMainThread(task =>
		{
			if(task.IsCompleted)
			{
				QuerySnapshot snapshot = task.Result;
				foreach(var document in snapshot.Documents)
				{
					if(document.GetValue<string>("Code") == CodeInput.text)
					{
						CodeVerifiedGiveReward();
					}
				}
			}
			else
			{
				PFManager.instance.ShowMessage("Error_2", "Something Went Wrong\n Check Your Connection.", "Error");
			}
		});
	}

	void CodeVerifiedGiveReward()
	{
		// Write code to reward the user

		PFManager.instance.ShowMessage("Referral Redeemed", "Code verified successfully\nPrize amount added to wallet", "Success");
	}

	void ShowMyCode(string RefereeName, string code)
	{

	}

}
