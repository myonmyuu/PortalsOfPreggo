using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using JetBrains.Annotations;
using Microsoft.SqlServer.Server;
using UnityEngine;
using UnityEngine.UI;

public enum OvumState
{
	None,
	Alive,
	Fertilized,
	Implanted,
	ReadyToBirth,
	Dead
}

public class OvumVisData
{
	public OvumState State;
	public float GrowthPercent;
}

public class Uterus : MonoBehaviour
{
	public const float UTERUS_WINDOW_SIZE 	= 55f;
	public const float OVUM_MOVE_SPEED 		= 1f;
	public const float SPERM_MOVE_SPEED 	= 13f;


	public const float SPERM_FORWARD_SPEED 	= SPERM_MOVE_SPEED * .2f;
	public const float SPERM_BACKWARD_SPEED	= SPERM_MOVE_SPEED * .1f;

	public struct UterusData
	{
		public UterusData(bool broadcast, float spermVol, bool seenOvu, float seenCum, float scale, params OvumVisData[] ovums)
		{
			Broadcast = broadcast;
			Ovums = ovums ?? Enumerable.Empty<OvumVisData>();
			SpermVolume = spermVol;
			SeenSinceOvulation = seenOvu;
			KnownUterusCum = seenCum;
			Scale = scale;
		}

		public IEnumerable<OvumVisData> Ovums;
		public bool Broadcast;
		public float SpermVolume;

		public bool SeenSinceOvulation;
		public float KnownUterusCum;

		public float Scale;
	}

    public RectTransform Transform;
	public GameObject WindowRoot;
	public RectTransform MoveRoot;
	public RectTransform InfoPanel;
	public Toggle Toggle;
	public RectTransform TextRect;
	public Button MainButton;

	private List<Sperm> Sperms { get; set; } = new List<Sperm>();
	private List<Ovum> Ovums { get; set; } = new List<Ovum>();
	private List<Coroutine> MovementRoutines { get; set; } = new List<Coroutine>();
	private Coroutine PanelMoveRoutine { get; set; }

	public delegate void BroadcastChangedDelegate(bool broadCast);
	public event BroadcastChangedDelegate OnBroadcastChanged;

	private bool IsInitialized { get; set; }
	private bool Extended { get; set; }

	public void SetPosition(Vector2 pos)
	{
		var width 	= Screen.width / PortalsOfPreggoPlugin.SCREEN_WIDTH;
		var height 	= Screen.height / PortalsOfPreggoPlugin.SCREEN_HEIGHT;

		MoveRoot.anchoredPosition = new Vector2(pos.x * width, pos.y * height);
	}

	public int GetSpermAmount(float volume)
	{
		return Mathf.FloorToInt(Mathf.Min(volume * 10, 150)) + (int)Mathf.Sqrt(volume) * 4;
	}

	private void Start()
	{
		Toggle.onValueChanged.AddListener(e => {
			OnBroadcastChanged?.Invoke(e);
		});
		MainButton.onClick.AddListener(delegate {
			TogglePanel();
		});
	}

	public void Init(UterusData data)
	{
		if (IsInitialized)
		{
			Release(false);
		}
		IsInitialized = true;
		Toggle.isOn = data.Broadcast;
		var spermAmt = GetSpermAmount(data.SpermVolume);
		var knownAmt = GetSpermAmount(data.KnownUterusCum);
		var hasOvum = data.Ovums.Any();
		Transform.localScale = new Vector3(data.Scale, data.Scale, 1);
		Vector2 ovumOffset = Random.insideUnitCircle;
		var ovumCount = data.Ovums.Count();
		var ovumwithsperm = ovumCount;

		if (ovumCount > 1)
		{
			ovumOffset *= Ovum.OVUM_RADIUS * 1.3f;
		}

		if (hasOvum)
		{
			var spermPerOvum = spermAmt / ovumCount;
			foreach (var ovData in data.Ovums)
			{
				ovumOffset *= -1;

				var ovum = PrefabManager.Instance.GetOvum();
				ovum.transform.SetParent(WindowRoot.transform);
				var pos = ovumOffset;
				ovum.Init(ovData, data.SeenSinceOvulation);

				var scale = Random.Range(0.95f, 1.05f) - ((ovumCount - 1) * 0.4f);
				ovum.Transform.localScale = new Vector3(scale, scale, 1);
				ovum.Transform.anchoredPosition = data.SeenSinceOvulation || ovData.State > OvumState.Fertilized
					? pos
					: new Vector2(-UTERUS_WINDOW_SIZE, 0);
				
				if (ovData.State != OvumState.Implanted)
				{
					if (!data.SeenSinceOvulation)
						MovementRoutines.Add(StartCoroutine(OvumEnterRoutine(ovum, pos)));

					MovementRoutines.Add(StartCoroutine(MoveOvumRoutine(ovum, pos)));
				}

				Ovums.Add(ovum);
				
				
				for (int i = 0; i < spermPerOvum; i++)
				{
					if (ovData.State > OvumState.Fertilized)
					{
						ovumwithsperm--;
						break;
					}
					scale = Random.Range(0.9f, 1.1f) * data.Scale;
					var sperm = PrefabManager.Instance.GetSperm();
					sperm.Transform.localScale = new Vector3(scale, scale, 0);
					sperm.transform.SetParent(WindowRoot.transform);

					var offsetPos = Random.insideUnitCircle.normalized;

					var known = i < knownAmt / ovumCount;
					Vector2 sPos = known
						? (Vector2)ovum.Transform.anchoredPosition + (offsetPos * 12.5f)
						: offsetPos * UTERUS_WINDOW_SIZE * Random.Range(.7f, 1.1f);
					
					sperm.Transform.anchoredPosition = sPos;
					Sperms.Add(sperm);

					MovementRoutines.Add(StartCoroutine(SpermMoveRoutineTarget(sperm, ovum, offsetPos, data.Scale, known)));
				}
			}
		}

		if (ovumwithsperm < ovumCount || !hasOvum)
		{
			for (int i = 0; i < spermAmt; i++)
			{
				var sperm = PrefabManager.Instance.GetSperm();
				var scale = Random.Range(0.9f, 1.1f) * data.Scale;
				sperm.Transform.localScale = new Vector3(scale, scale, 0);
				sperm.transform.SetParent(WindowRoot.transform);
				Sperms.Add(sperm);

				MovementRoutines.Add(StartCoroutine(SpermMoveRoutine(sperm, i < knownAmt)));
			}
		}
	}

	private void TogglePanel()
	{
		if (PanelMoveRoutine != null)
			StopCoroutine(PanelMoveRoutine);
		PanelMoveRoutine = StartCoroutine(TogglePanelRoutine());
	}

	private IEnumerator TogglePanelRoutine()
	{
		Extended = !Extended;
		var target = Extended
			? Vector3.one
			: new Vector3(0, 1, 1);

		float cur = 0;
		for (;;)
		{
			cur += Time.deltaTime;
			InfoPanel.localScale = Vector3.Lerp(InfoPanel.localScale, target, cur);
			yield return null;
		}
	}

	public void Release(bool destroy)
	{
		IsInitialized = false;

		foreach (var r in MovementRoutines)
		{
			StopCoroutine(r);
		}
		MovementRoutines.Clear();

		foreach (var sperm in Sperms)
		{
			sperm.Release();
		}
		Sperms.Clear();

		foreach (var ovum in Ovums)
		{
			ovum.Release();
			Destroy(ovum.gameObject);
		}
		Ovums.Clear();

		if (destroy)
			Destroy(gameObject);
	}

	static double AngleBetweenTwoPoints(double x1, double y1, double x2, double y2)
    {
        double deltaX = x2 - x1;
        double deltaY = y2 - y1;
        double radianAngle = System.Math.Atan2(deltaY, deltaX);
        double degreeAngle = radianAngle * 180 / System.Math.PI;
        return degreeAngle;
    }

	private void ChangeSpermFacing(Sperm sperm, Vector2 lookat, Vector2? start = null)
	{
		var s = start ?? sperm.Transform.anchoredPosition;
		float deltaX = lookat.x - s.x;
        float deltaY = lookat.y - s.y;
        float radianAngle = Mathf.Atan2(deltaY, deltaX);
        float degreeAngle = radianAngle * 180 / (float)System.Math.PI;

		sperm.Transform.rotation = Quaternion.Euler(
			0,
			0,
			degreeAngle + 180
		);
	}

	public IEnumerator SpermMoveRoutine(Sperm sperm, bool seen)
	{
		Vector2 tar = Vector2.zero;
		Vector2 start = Vector2.zero;
		float speed = 1;
		void _setVals()
		{
			tar = new Vector2(Random.value, Random.value).normalized * UTERUS_WINDOW_SIZE;
			start = -new Vector2(Random.value, Random.value).normalized * UTERUS_WINDOW_SIZE;
			ChangeSpermFacing(sperm, tar, start);
			speed = 1f + (Random.value - .5f);
		}
		_setVals();

		var cur = seen
			? Random.value
			: 0;

		for (;;)
		{
			cur += SPERM_MOVE_SPEED / UTERUS_WINDOW_SIZE * Time.deltaTime * speed;
			sperm.Transform.anchoredPosition = Vector2.Lerp(start, tar, cur);

			if (cur > 1)
			{
				_setVals();
				cur = 0;
			}
			yield return null;
		}
		
	}

	public IEnumerator SpermMoveRoutineTarget(Sperm sperm, Ovum ovum, Vector2 offset, float scale, bool seen)
	{
		var cur = seen
			? Random.value
			: 0;

		bool forward = seen
			? Random.value > .5f
			: true;
		
		bool arrived = seen;
		var speed = Random.Range(.8f, 1.2f);

		while (!arrived)
		{
			yield return null;
			var target = ovum.Transform.anchoredPosition + offset;
			var dist = Vector2.Distance(target, sperm.Transform.anchoredPosition);
			
			ChangeSpermFacing(sperm, ovum.Transform.anchoredPosition);

			if (dist < ovum.OvumRadius)
			{
				arrived = ovum.Ready;
				continue;
			}
			else
			{
				var dif = (target - sperm.Transform.anchoredPosition).normalized;
				sperm.Transform.anchoredPosition += Time.deltaTime * SPERM_MOVE_SPEED * speed * dif;
			}
		}

		var sizedOffset = offset * ovum.OvumRadius;
		for (;;)
		{
			
			float v = 0;
			if (forward)
			{
				cur += Time.deltaTime * speed * SPERM_FORWARD_SPEED;
				cur = (Mathf.Clamp01(cur));
				v = EasingFunctions.EaseOutCirc(0, 1, cur);

				forward = cur < 1;
			}
			else
			{
				cur -= Time.deltaTime * speed * SPERM_BACKWARD_SPEED;
				cur = (Mathf.Clamp01(cur));
				v = cur;

				forward = cur <= 0;
			}

			sperm.Transform.anchoredPosition = Vector2.Lerp(ovum.Transform.anchoredPosition + (sizedOffset * 1.2f), ovum.Transform.anchoredPosition + sizedOffset, v);

			ChangeSpermFacing(sperm, ovum.Transform.anchoredPosition);
			yield return null;
		}
	}

	public IEnumerator OvumEnterRoutine(Ovum ovum, Vector2 target)
	{
		for (;;)
		{
			ovum.Transform.anchoredPosition = Vector3.Lerp(ovum.Transform.anchoredPosition, target, Time.deltaTime * OVUM_MOVE_SPEED);
			if (Vector3.Distance(ovum.Transform.anchoredPosition, target) <= 0.01)
				break;
			yield return null;
		}
	}

	public IEnumerator MoveOvumRoutine(Ovum ovum, Vector2 target)
	{
		for (;;)
		{
			yield return null;
		}
	}
}
