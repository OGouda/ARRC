using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Vuforia;

/**
 * This class handles the management of multiple instances of the same VuMark with different ID's.
 * The basic process works like this:
 *	1. Every time a VuMarkerBehaviour is recognized, we check if we already registered a VuMarkAssigned
 *	event handler. If not we register it.
 *	
 *	2. The VuMarkAssigned event handler is called when a VuMark is assigned to a VuMarkBehaviour. 
 *	This distinction is being made because although the VuMarkBehaviour wraps a single VuMark image in 
 *	the scene, you can print multiple copies of this VuMark all with either the same or distinctive ID's.
 *	So the VuMarkBehaviourDetectedCallback is triggered when Vuforia detects a VuMark of type Morton Tuxedo
 *	for example, and the VuMarkTargetAssignedCallback is triggered when Vuforia has read the encoding 
 *	in the VuMark and knows which ID matches that instance, eg 01, 02 or whatever you assigned as ID's.
 *	
 *	The moment the ID becomes known, we disable all children of the marker except the child whose ID
 *	matches. This also works when you have two of the same VuMarks with equal or different ID's.
 *	The moment you hold up two images, Vuforia copies the original marker, and starts the event
 *	triggering process. For some weird reason during that process, some events that below to the clone
 *	are triggered on the original, but doing it the way we do it below circumvents those problems.
 *	
 *	@author J.C.Wichman, InnerDriveStudios.com
 */
public class VumarkManager : MonoBehaviour
{
	//if you specified an ID of length 4 for example in the TargetManager, but use ID's that are
	//actually shorter, you should adapt this field. So basically:
	//make sure you ID length in the targetmanager matches the ID length of the ID's you generate,
	//and the number set below
	public int IDLength = 5;

	//simple list to check whether a found VuMarkBehaviour was already registered or not
	//if you have a loooot of VuMarks you might want to change this to a Dictionary
	List<VuMarkBehaviour> registeredBehaviours = new List<VuMarkBehaviour>();

	private void Awake()
	{
		TrackerManager.Instance.
			GetStateManager().
			GetVuMarkManager().
			RegisterVuMarkBehaviourDetectedCallback(onVuMarkBehaviourFound);
	}

	private void onVuMarkBehaviourFound(VuMarkBehaviour pVuMarkBehaviour)
	{
		//check if we have already registered for the target assigned callbacks
		if (registeredBehaviours.Contains(pVuMarkBehaviour))
		{
			log("Previously tracked VumarkBehaviour found (" + pVuMarkBehaviour.name+")");
		} else
		{
			log("Newly tracked VumarkBehaviour found (" + pVuMarkBehaviour.name + ")");
			log("Registering for VuMarkTargetAssignedCallbacks from " + pVuMarkBehaviour.name);

			//if we hadn't registered yet, we do so now
			registeredBehaviours.Add(pVuMarkBehaviour);

			pVuMarkBehaviour.RegisterVuMarkTargetAssignedCallback(
				() => vumarkTargetAssigned(pVuMarkBehaviour)
			);
		}
	}

	/**
	 * Every time a vumarkTarget is assigned to a specific vuMarkBehaviour
	 * we can process it's children to make sure only the right ones are visible.
	 */
	private void vumarkTargetAssigned(VuMarkBehaviour pVuMarkBehaviour)
	{
		log("VuMarkTarget assigned to " + pVuMarkBehaviour.name + " with ID:"+pVuMarkBehaviour.VuMarkTarget.InstanceId.ToString());
	
		string myID = GetID(pVuMarkBehaviour.VuMarkTarget.InstanceId);

		log("Enabling object with ID:" + myID + " ....");

		foreach (Transform child in pVuMarkBehaviour.transform)
		{
			log("Matching gameObject " + child.name + " with ID " + myID + " SetActive (" + (myID == child.name) + ")");
			child.gameObject.SetActive(myID == child.name);
		}
	}

	/**
	 * Helper method to sanitize the returned ID's
	 */
	private string GetID (InstanceId pInstanceId)
	{
		int inputLength = pInstanceId.StringValue.Length;
		int outputLength = Mathf.Min(IDLength, inputLength);
		string subString = pInstanceId.StringValue.Substring(0, outputLength);
		return subString;
	}

	[Conditional("VUMARKHANDLER_DEBUG")]
	private void log (string pInfo)
	{
		UnityEngine.Debug.Log("VUMARKHANDLER:"+pInfo);
	}
}