using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ovum : MonoBehaviour
{
	public const float OVUM_RADIUS = 19.5f;
	private const float READY_TIME = 1f;

	public RectTransform Transform;
	public Image Image;
	public Image EggA;
	public Image EggB;
	public float EggScale = 1f;
	
	public float Scale => Transform.localScale.x;
	public float OvumRadius => Scale * OVUM_RADIUS;

	public bool Ready { get; set; }

	private IEnumerator StartRoutine(bool seen)
	{
		if (seen)
		{
			Ready = seen;
			yield break;
		}
		var t = 0f;
		
		while (t < READY_TIME)
		{
			t += Time.deltaTime;
			yield return null;
		}

		Ready = true;
	}

	public void Init(OvumVisData data, bool seen)
	{
		Image.enabled = data.State != OvumState.Implanted && data.State != OvumState.ReadyToBirth;
		EggA.enabled = data.State == OvumState.Fertilized;
		EggB.enabled = data.State == OvumState.Implanted;
		if (data.State == OvumState.Implanted)
		{
			EggScale = Mathf.Lerp(1, 6, data.GrowthPercent);
			EggB.transform.localScale = new Vector3(EggScale, EggScale, EggScale);
		}
		StartCoroutine(StartRoutine(seen));
	}

	public void Release()
	{
		StopAllCoroutines();
	}

	void Update()
	{
		
	}
}
