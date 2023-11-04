using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    private const int SPERM_WARMUP          = 2000;
    private const string BUNDLE_NAME        = "preggoassets";
    private const string UTERUS_PREFAB_NAME = "UterusWindow";
    private const string SPERM_PREFAB_NAME  = "Sperm";
    private const string OVUM_PREFAB_NAME   = "Ovum";

    internal static PrefabManager Instance;

    private List<Sperm> ActiveSperm     = new List<Sperm>();
    private List<Sperm> InactiveSperm   = new List<Sperm>();

    public AssetBundle Assets;

    public Uterus UterusPrefab;
    public Sperm SpermPrefab;
    public Ovum OvumPrefab;

    public Sprite[] SpritesArray;
    public Dictionary<string, Sprite> Sprites;

    private void Start()
    {
        Instance = this;
        PortalsOfPreggoPlugin.Instance.Log.LogInfo("Loading assets");

        Assets          = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(PortalsOfPreggoPlugin.Instance.Info.Location), BUNDLE_NAME));
        UterusPrefab    = Assets.LoadAsset<GameObject>(UTERUS_PREFAB_NAME).GetComponent<Uterus>() ?? throw new System.Exception("Uterus asset was null");
        SpermPrefab     = Assets.LoadAsset<GameObject>(SPERM_PREFAB_NAME).GetComponent<Sperm>() ?? throw new System.Exception("Uterus asset was null");
        OvumPrefab      = Assets.LoadAsset<GameObject>(OVUM_PREFAB_NAME).GetComponent<Ovum>() ?? throw new System.Exception("Uterus asset was null");
        SpritesArray    = Assets.LoadAllAssets<Sprite>();

        Sprites = new Dictionary<string, Sprite>();
        foreach (var sprite in SpritesArray)
        {
            Sprites[sprite.name] = sprite;
        }

        Warmup();
    }

    public Sperm GetSperm()
    {
        if (!InactiveSperm.Any())
            CreateNewSperm();

        var sperm = InactiveSperm.First();
        sperm.gameObject.SetActive(true);
        InactiveSperm.Remove(sperm);
        ActiveSperm.Add(sperm);
        return sperm;
    }

    // Aren't frequent enough to warrant pooling
    public Ovum GetOvum()       => Instantiate<Ovum>(OvumPrefab);
    public Uterus GetUterus()   => Instantiate<Uterus>(UterusPrefab);

    private void Warmup()
    {
        for (int i = 0; i < SPERM_WARMUP; i++)
            CreateNewSperm();
    }

    public void ReturnSperm(Sperm sperm)
    {
        DeactivateSperm(sperm);
        InactiveSperm.Add(sperm);
        ActiveSperm.Remove(sperm);
    }

    private void CreateNewSperm()
    {
        var sperm = Instantiate<Sperm>(PrefabManager.Instance.SpermPrefab);
        DeactivateSperm(sperm);
        InactiveSperm.Add(sperm);
    }

    private void DeactivateSperm(Sperm sperm)
    {
        sperm.transform.SetParent(this.transform);
        sperm.gameObject.SetActive(false);
    }
}
