using UnityEngine;
using UnityEngine.UI;

public class MaintainDistanceFromImageTop : MonoBehaviour
{
	public float distanceFromTop = 10f; // Desired distance from the top of the image

	private RectTransform myRectTransform;

	private void Start()
	{
		// Cache the RectTransform of this game object
		myRectTransform = GetComponent<RectTransform>();
	}

	private void Update()
	{
		if (myRectTransform != null)
		{
			// Get the top position of the target image in world space
			Vector3 targetTopPosition = myRectTransform.transform.position + new Vector3(0, myRectTransform.sizeDelta.y / 2, 0);

			// Calculate the desired position with the specified distance from the top of the image
			Vector3 desiredPosition = targetTopPosition - new Vector3(0, distanceFromTop, 0);

			// Move the game object to the desired position
			myRectTransform.position = desiredPosition;
		}
	}
}
