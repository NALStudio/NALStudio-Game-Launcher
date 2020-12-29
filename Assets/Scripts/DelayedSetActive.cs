using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedSetActive : MonoBehaviour
{
	public GameObject toSet;
	public bool value;
	public float delay;

	public void SetActive()
	{
		if (gameObject.activeInHierarchy)
			StartCoroutine(Delay());
	}

	IEnumerator Delay()
	{
		yield return new WaitForSeconds(delay);
		toSet.SetActive(value);
	}

#if UNITY_EDITOR
	void Reset()
	{
		toSet = gameObject;
		value = false;
		delay = 1f;
	}
#endif
}
