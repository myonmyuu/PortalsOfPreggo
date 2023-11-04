using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sperm : MonoBehaviour
{
	private const float WRIGGLE_TIME = 0.08f;
	public RectTransform Transform;
	public Sprite[] Sprites;
	public Image Renderer;


	private float Elapsed;
	private int SpriteN;

	private Vector3 ReverseVec = new Vector3(1, -1, 1);

	public void Release()
	{
		PrefabManager.Instance.ReturnSperm(this);
	}

    private void Start()
    {
        Elapsed = Random.value;
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
			return;

		Elapsed += Time.deltaTime;
		if (Elapsed < WRIGGLE_TIME)
			return;

		Elapsed -= WRIGGLE_TIME;

		if (Random.value > .5f)
			Transform.localScale = new Vector3(Transform.localScale.x, -Transform.localScale.y, 1);

		SpriteN = (SpriteN + Random.Range(1, 3)).Wrap(0, Sprites.Length - 1);
		Renderer.sprite = Sprites[SpriteN];
    }
}
