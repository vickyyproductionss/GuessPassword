using Firebase.Extensions;
using Firebase.Firestore;
using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using static PFManager;
using static PlayerDataManager;

public class YoutubeManager : MonoBehaviour
{
	FirebaseFirestore firestore;
	[SerializeField] GameObject VideoPrefab,VideoParent, AskForAdsWindow;
	[SerializeField] TMP_Text FreeChancesText;

	List<YoutubeVideo> videoList = new List<YoutubeVideo>();
	private void Awake()
	{
		firestore = FirebaseFirestore.DefaultInstance;
		FetchLatestDetails();
	}

	public class YoutubeVideo
	{
		public string Title;
		public string Filename;
		public string Password;
		public string Link;
		public string Eligible;
	}
	void AddUserOnCorrectCode(YoutubeVideo video)
	{
		DocumentReference YoutubeVideoDetails = firestore.Collection("YoutubeWinner").Document(SystemInfo.deviceUniqueIdentifier);
		Dictionary<string, object> winner = new Dictionary<string, object>()
		{
			{"Title", video.Title},
			{"Password", video.Password},
			{"WinnerName", PlayerPrefs.GetString("PName")},
			{"DeviceID", SystemInfo.deviceUniqueIdentifier}
		};
		YoutubeVideoDetails.SetAsync(winner).ContinueWith(t =>
		{
			if(t.IsCompleted)
			{
				CloseThisVideoOnSuccess(video.Password);
				PFManager.instance.ShowMessage("Congratulations!!!", "You've entered the correct code.\nDrop me a mail at\nvickychaudhary8955@gmail.com .\nYour details saved successfully", "Success");
			}
		});
	}
	void CloseThisVideoOnSuccess(string password)
	{
		CollectionReference YoutubeVideoDetails = firestore.Collection("YoutubeVideos");
		YoutubeVideoDetails.GetSnapshotAsync().ContinueWithOnMainThread(task =>
		{
			if (task.IsCompleted)
			{
				QuerySnapshot snapshot = task.Result;
				foreach (var document in snapshot.Documents)
				{
					string Passcode = document.GetValue<string>("Password");
					if(password == Passcode)
					{
						Dictionary<string, object> VideUpdate = new Dictionary<string, object>()
						{
							{"Title",document.GetValue<string>("Title")},
							{"Password",document.GetValue<string>("Password")},
							{"Link",document.GetValue<string>("Link")},
							{"Filename",document.GetValue<string>("Filename")},
							{"Eligible","False"}
						};
						DocumentReference videoDoc = document.Reference;
						videoDoc.SetAsync(VideUpdate).ContinueWithOnMainThread(task2 =>
						{
							if(task2.IsCompleted)
							{
								Debug.Log("Video no longer take responses");
							}
						});
					}

				}
			}
		});
	}

	public void OnClickSubmit(TMP_InputField CodeInput)
	{
		var request = new ExecuteCloudScriptRequest
		{
			FunctionName = "getPlayerData",
			FunctionParameter = new { PlayerID = PlayerPrefs.GetString("PF_ID") },
			GeneratePlayStreamEvent = true
		};

		// Call the Cloud Script function
		PlayFabClientAPI.ExecuteCloudScript(request, result =>
		{
			Debug.Log(result.FunctionResult);
			PlayerData _playerData = JsonConvert.DeserializeObject<PlayerData>(result.FunctionResult.ToString());
			PlayerDataManager.Instance.playerData.PurchasedChances = _playerData.PurchasedChances;
			int Chances = PlayerDataManager.Instance.playerData.PurchasedChances;
			if (Chances <= 0)
			{
				FreeChancesText.text = "No Free Chances Today";
				AskForAdsWindow.SetActive(true);
				return;
			}
			else if (Chances > 0)
			{
				FreeChancesText.text = Chances.ToString() + " Free Chances";
			}
		}, error =>
		{
			// Handle error response
			Debug.LogError("Cloud Script Error: " + error.ErrorMessage);
		});
		

		foreach (YoutubeVideo video in videoList)
		{
			if(video.Password == CodeInput.text && video.Eligible == "True")
			{
				//Reward user from here
				PFManager.instance.ShowMessage("Congratulations!!!", "You've entered the correct code.\nDrop me a mail at\nvickychaudhary8955@gmail.com", "Success");
				AddUserOnCorrectCode(video);
			}
			else if(video.Password == CodeInput.text && video.Eligible == "False")
			{
				//Password is correct but already redeemed
				PFManager.instance.ShowMessage("Correct code", "Code entered is correct but already redeemed!", "Warning");
			}
			else
			{
				//Incorrect passcode
				PFManager.instance.ShowMessage("Incorrect code", "Code entered is not correct!", "Error");
			}
		}
	}

	void FetchLatestDetails()
	{
		CollectionReference YoutubeVideoDetails = firestore.Collection("YoutubeVideos");
		YoutubeVideoDetails.GetSnapshotAsync().ContinueWithOnMainThread(task =>
		{
			if(task.IsCompleted)
			{
				QuerySnapshot snapshot = task.Result;
				foreach(var document in snapshot.Documents)
				{
					GameObject video = Instantiate(VideoPrefab);
					video.transform.SetParent(VideoParent.transform, false);
					YoutubeVideo youtubeVideo = new YoutubeVideo();
					youtubeVideo.Title = document.GetValue<string>("Title");
					youtubeVideo.Filename = document.GetValue<string>("Filename");
					youtubeVideo.Password = document.GetValue<string>("Password");
					youtubeVideo.Link = document.GetValue<string>("Link");
					youtubeVideo.Eligible = document.GetValue<string>("Eligible");
					loadImage(video.transform.GetChild(0).GetComponent<RawImage>(), document.GetValue<string>("Filename"), document.GetValue<string>("Title"), video.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>());
					videoList.Add(youtubeVideo);
					video.GetComponent<LinkOpener>().UpdateVideo(youtubeVideo);
					Debug.Log("Added to list");
				}
			}
		});
	}
	void loadImage(RawImage image, string FileName,string title, TMP_Text TitleText)
	{
		TitleText.text = title;
		Firebase.Storage.StorageReference storageReference =
		Firebase.Storage.FirebaseStorage.DefaultInstance.GetReferenceFromUrl("gs://guessthepassword.appspot.com");

		storageReference.Child(FileName).GetBytesAsync(1024 * 1024).
			ContinueWithOnMainThread((System.Threading.Tasks.Task<byte[]> task) =>
			{
				if (task.IsFaulted || task.IsCanceled)
				{
					Debug.Log(task.Exception.ToString());
				}
				else
				{
					byte[] fileContents = task.Result;
					Texture2D texture = new Texture2D(1, 1);
					texture.LoadImage(fileContents);
					image.texture = texture;
				}
			});
	}
}
