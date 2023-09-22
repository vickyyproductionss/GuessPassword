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
			// Do something with the child GameObject here.
			// For example, print its name.
			Debug.Log("Name is : " + child.gameObject.name);

			// Recursively call the function to iterate through the child's children.
			IterateThroughHierarchy(child);
		}
	}
}
