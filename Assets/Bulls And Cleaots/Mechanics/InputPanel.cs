using UnityEngine;
using System.Collections;
using System;

namespace BullsAndCleots.Mechanics {
	
/// <summary>
/// Allows for the solution of Bulls and Cleots to vary in length.
/// </summary>
public class InputPanel : MonoBehaviour {

	public GameObject solutionSnappable;
	public int solutionLength;

	// Where to start the box layout.
	public Transform startTransform;

	// Assume horizontal spacing and each box will be to the right of the previous
	// by an extra layoutSpacing.
	private float layoutSpacing = 2.0f;

	void Start() {

		Vector3 startPos = startTransform.position;
        
		// Testing C# lambda functions/expressions/w.e
		Func<int,Vector3> LayoutObject = c => new Vector3(
            startPos.x + (c * layoutSpacing), startPos.y, startPos.z);

		for (int count = 0; count < solutionLength; count++) {
			GameObject go = Instantiate(solutionSnappable) as GameObject;
			go.transform.position = LayoutObject(count);
			go.transform.parent = transform;
            
			SnapArea snapArea = go.GetComponent<SnapArea>();
			snapArea.Index = count;

		}

	}


}
}