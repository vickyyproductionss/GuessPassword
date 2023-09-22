using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnderMaintenanceManager : MonoBehaviour
{
	public FirebaseFirestore firestore;
	[SerializeField] GameObject MaintenanceWindow;
	[SerializeField] TMP_Text TitleText;
	[SerializeField] TMP_Text MessageText;
	[SerializeField] TMP_Text LinkText;
	private void Start()
	{
		firestore = FirebaseFirestore.DefaultInstance;
		checkMaintenance();
	}
	void checkMaintenance()
	{
		DocumentReference MaintenanceRef = firestore.Collection("Maintenance").Document("Updates");
		Dictionary<string,object> dict = new Dictionary<string, object>() 
		{
			{"Message","Under maintenance"},
			{"Link","link is here" },
			{"UnderMaintenance","True" }
		};
		MaintenanceRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
		{
			if(task.IsCompleted)
			{
				DocumentSnapshot snapshot = task.Result;
				string Title = snapshot.GetValue<string>("Title");
				string Message = snapshot.GetValue<string>("Message");
				string Link = snapshot.GetValue<string>("Link");
				string UnderMaintenance = snapshot.GetValue<string>("UnderMaintenance");
				string Version = snapshot.GetValue<string>("Version");


				if(Application.version != Version)
				{
					if (UnderMaintenance == "True")
					{
						MaintenanceWindow.SetActive(true);
						MessageText.text = Message;
						LinkText.text = Link;
						TitleText.text = Title;
					}
					else if (UnderMaintenance == "Message")
					{
						MaintenanceWindow.SetActive(true);
						MessageText.text = Message;
						LinkText.text = Link;
						TitleText.text = Title;
					}
				}
			}
		});
	}
	public void OnClick()
	{
		Application.OpenURL(LinkText.text);
	}
}
