using Firebase.Extensions;
using Firebase.Firestore;
//using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoogleAdsManager : MonoBehaviour
{
	//private string _adUnitId = "ca-app-pub-6557573345682433/4725889744";
	//private string _disable;
	//public RewardedAd rewardedAd;
	//FirebaseFirestore firestore;
	//public static GoogleAdsManager instance;
	//private void Awake()
	//{
	//	instance = this;
	//}

	//private void Start()
	//{
	//	firestore = FirebaseFirestore.DefaultInstance;
	//	GetDetailsAboutGoogleAds();
	//}
	//void GetDetailsAboutGoogleAds()
	//{
	//	DocumentReference GoogleAdsRef = firestore.Collection("GoogleAds").Document("AdDetails");
	//	GoogleAdsRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
	//	{
	//		if(task.IsCompleted)
	//		{
	//			DocumentSnapshot snapshot = task.Result;
	//			_adUnitId = snapshot.GetValue<string>("RewardedAdID");
	//			_disable = snapshot.GetValue<string>("Disable");
	//			if(_disable != "True")
	//			{
	//				InitGoogleAds();
	//			}
	//		}
	//	});
	//}
	//public void InitGoogleAds()
	//{
	//	MobileAds.Initialize((InitializationStatus initStatus) =>
	//	{
	//		// This callback is called once the MobileAds SDK is initialized.
	//		LoadRewardedAd();
	//	});
	//}

	///// <summary>
	///// Loads the rewarded ad.
	///// </summary>
	//public void LoadRewardedAd()
	//{
	//	if (rewardedAd != null)
	//	{
	//		rewardedAd.Destroy();
	//		rewardedAd = null;
	//	}

	//	Debug.Log("Loading the rewarded ad.");

	//	var adRequest = new AdRequest();
	//	adRequest.Keywords.Add("unity-admob-sample");

	//	RewardedAd.Load(_adUnitId, adRequest,
	//		(RewardedAd ad, LoadAdError error) =>
	//		{
	//			if (error != null || ad == null)
	//			{
	//				Debug.LogError("Rewarded ad failed to load an ad " +
	//							   "with error : " + error);
	//				return;
	//			}

	//			Debug.Log("Rewarded ad loaded with response : "
	//					  + ad.GetResponseInfo());

	//			rewardedAd = ad;
	//		});
	//}

	//public void ShowRewardedAd()
	//{
	//	const string rewardMsg =
	//		"Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

	//	if (rewardedAd != null && rewardedAd.CanShowAd())
	//	{
	//		rewardedAd.Show((Reward reward) =>
	//		{
	//			// TODO: Reward the user.
	//			PFManager.instance.OnWatchingAdFinished();
	//			LoadRewardedAd();
	//			Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
	//		});
	//	}
	//}

	//private void RegisterEventHandlers(RewardedAd ad)
	//{
	//	// Raised when the ad is estimated to have earned money.
	//	ad.OnAdPaid += (AdValue adValue) =>
	//	{
	//		Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
	//			adValue.Value,
	//			adValue.CurrencyCode));
	//	};
	//	// Raised when an impression is recorded for an ad.
	//	ad.OnAdImpressionRecorded += () =>
	//	{
	//		Debug.Log("Rewarded ad recorded an impression.");
	//	};
	//	// Raised when a click is recorded for an ad.
	//	ad.OnAdClicked += () =>
	//	{
	//		Debug.Log("Rewarded ad was clicked.");
	//	};
	//	// Raised when an ad opened full screen content.
	//	ad.OnAdFullScreenContentOpened += () =>
	//	{
	//		Debug.Log("Rewarded ad full screen content opened.");
	//	};
	//	// Raised when the ad closed full screen content.
	//	ad.OnAdFullScreenContentClosed += () =>
	//	{
	//		Debug.Log("Rewarded ad full screen content closed.");
	//	};
	//	// Raised when the ad failed to open full screen content.
	//	ad.OnAdFullScreenContentFailed += (AdError error) =>
	//	{
	//		Debug.LogError("Rewarded ad failed to open full screen content " +
	//					   "with error : " + error);
	//	};
	//}

	//private void RegisterReloadHandler(RewardedAd ad)
	//{
	//	// Raised when the ad closed full screen content.
	//	ad.OnAdFullScreenContentClosed += LoadRewardedAd;

	//{
	//		Debug.Log("Rewarded Ad full screen content closed.");

	//		// Reload the ad so that we can show another as soon as possible.
	//		LoadRewardedAd();
	//	};
	//	// Raised when the ad failed to open full screen content.
	//	ad.OnAdFullScreenContentFailed += (AdError error) =>
	//	{
	//		Debug.LogError("Rewarded ad failed to open full screen content " +
	//					   "with error : " + error);

	//		// Reload the ad so that we can show another as soon as possible.
	//		LoadRewardedAd();
	//	};
	//}

}
