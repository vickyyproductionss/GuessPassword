using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingManager : MonoBehaviour
{
	[SerializeField] GameObject LoaderWindow;
    public static LoadingManager instance;
	private void Awake()
	{
		instance = this;
	}
	public void ShowLoading()
	{
		LoaderWindow.SetActive(true);
		Invoke("HideLoading", 5);
	}
	public void HideLoading()
	{
		LoaderWindow?.SetActive(false);
	}
}
