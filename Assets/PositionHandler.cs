using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionHandler : MonoBehaviour
{
	void Start()
	{
		// Start the iteration from the root of the hierarchy (usually the scene's root).
		Transform root = transform.root;

		// Call the recursive function to iterate through all GameObjects.
		IterateThroughHierarchy(root);
	}

	void IterateThroughHierarchy(Transform parent)
	{
		// Loop through all child GameObjects of the current parent.
		foreach (Transform child in parent)
		{
			RectTransform rectTransform = child.GetComponent<RectTransform>();

			if (rectTransform != null)
			{
				// Reset the RectTransform properties to their default values.
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = Vector2.one;
				rectTransform.sizeDelta = Vector2.zero;
				rectTransform.anchoredPosition = Vector2.zero;
				rectTransform.pivot = new Vector2(0.5f, 0.5f);
			}

			// Recursively call the function to iterate through the child's children.
			IterateThroughHierarchy(child);
		}
	}
}
