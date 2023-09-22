using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PendingWithdrawalFetcher : MonoBehaviour
{
	[SerializeField] TMP_Text InfoText;
	private void OnEnable()
	{
		DocumentReference WithdrawalRef = FirebaseFirestore.DefaultInstance.Collection("Withdrawals").Document(SystemInfo.deviceUniqueIdentifier);

		WithdrawalRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
		{
			if(task.IsCompleted)
			{
				DocumentSnapshot snapshot = task.Result;
				if(!snapshot.Exists)
				{
					InfoText.text = "No Requests";
				}
				else
				{
					InfoText.text = $"Amount: {snapshot.GetValue<int>("AmountRequested")}";
				}
			}
		});

	}
}
