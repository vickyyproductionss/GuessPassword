using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionHandler : MonoBehaviour
{
	private bool isConnectionAlive = true;
	[SerializeField] GameObject NoInternetPopup;

	private void Start()
	{
		StartCoroutine(CheckConnection());
	}

	private IEnumerator CheckConnection()
	{
		while (true)
		{
			//// Check the network connection
			//Ping ping = new Ping("8.8.8.8"); // Google's DNS server
			//yield return new WaitForSeconds(2f); // Adjust the interval as needed

			//if (ping.isDone && ping.time >= 0)
			//{
			//	// Connection is alive
			//	if (!isConnectionAlive)
			//	{
			//		Debug.Log("Internet connection is now alive.");
			//		NoInternetPopup.SetActive(false);
			//		// Add your debug or gameplay logic here when the connection is restored
			//	}
			//	isConnectionAlive = true;
			//}
			//else
			//{
			//	// Connection is lost
			//	if (isConnectionAlive)
			//	{
			//		NoInternetPopup.SetActive(true);
			//		Debug.LogWarning("Internet connection is lost.");
			//		// Add your debug or gameplay logic here when the connection is lost
			//	}
			//	isConnectionAlive = false;
			//}
			yield return new WaitForSeconds(2f); // Adjust the interval as needed

			if (Application.internetReachability != NetworkReachability.NotReachable)
			{
				if (!isConnectionAlive)
				{
					NoInternetPopup.SetActive(false);
				}
				isConnectionAlive = true;
			}
			else
			{
				if (isConnectionAlive)
				{
					NoInternetPopup.SetActive(true);
				}
				isConnectionAlive = false;
			}
		}
	}
}
