using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class LeaderboardHandler : MonoBehaviour
{
	[SerializeField] GameObject PlayersParent;
	[SerializeField] GameObject PlayerPrefab;
	[SerializeField] TMP_Text ResetInfo;

	private void OnEnable()
	{
		GetLeaderboardForTopPlayers();
	}
	void GetLeaderboardForTopPlayers()
	{
		var request = new GetLeaderboardRequest
		{
			StatisticName = "PasswordsTried",
			MaxResultsCount = 100 // You can change this to any desired number of top players
		};

		PlayFabClientAPI.GetLeaderboard(request, OnGetLeaderboardSuccess, OnGetLeaderboardFailure);
	}

	private void OnGetLeaderboardFailure(PlayFabError error)
	{
		PFManager.instance.ShowMessage("Error", "Something went wrong!", "Error");
	}

	private void OnGetLeaderboardSuccess(GetLeaderboardResult result)
	{
		ResetInfo.text = "Rank 1 player will be given 1000 INR as prize at " + result.NextReset.Value.ToString();
		GameObject label = Instantiate(PlayerPrefab);
		label.transform.SetParent(PlayersParent.transform, false);
		label.transform.GetChild(0).GetComponent<TMP_Text>().text = "Rank";
		label.transform.GetChild(1).GetComponent<TMP_Text>().text = "Name";
		label.transform.GetChild(2).GetComponent<TMP_Text>().text = "Passwords Tried";
		label.GetComponent<Image>().color = Color.clear;
		foreach (var player in result.Leaderboard)
		{
			GameObject go = Instantiate(PlayerPrefab);
			go.transform.SetParent(PlayersParent.transform, false);
			go.transform.GetChild(0).GetComponent<TMP_Text>().text = (player.Position+1).ToString();
			if(player.DisplayName != null)
			{
				go.transform.GetChild(1).GetComponent<TMP_Text>().text = (player.DisplayName).ToString();
			}
			else
			{
				go.transform.GetChild(1).GetComponent<TMP_Text>().text = "GTP Player";
			}
			go.transform.GetChild(2).GetComponent<TMP_Text>().text = (player.StatValue).ToString();
			if(player.PlayFabId == PlayerPrefs.GetString("PF_ID"))
			{
				go.GetComponent<Image>().color = Color.cyan;
				go.GetComponent<Image>().color = new Color(go.GetComponent<Image>().color.r, go.GetComponent<Image>().color.g, go.GetComponent<Image>().color.b, 0.05f);
			}
		}
	}
	private void OnDisable()
	{
		for(int i = PlayersParent.transform.childCount - 1;i >= 0;i--)
		{
			Destroy(PlayersParent.transform.GetChild(i).gameObject);
		}
	}
}
