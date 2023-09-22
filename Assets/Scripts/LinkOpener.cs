using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkOpener : MonoBehaviour
{
	public YoutubeManager.YoutubeVideo video = new YoutubeManager.YoutubeVideo();
	public string Link = "";
	public void UpdateVideo(YoutubeManager.YoutubeVideo _video)
	{
		video = _video;
		Link = _video.Link;
	}
	public void OnClickOpen()
	{
		Application.OpenURL(video.Link);
	}
}
