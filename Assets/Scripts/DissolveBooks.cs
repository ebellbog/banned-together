using DG.Tweening;
using Replicator;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct ColorByMaterial {
    public Material material;
    public Color color;
}


[RequireComponent(typeof(ReplicatorController))]
public class DissolveBooks : MonoBehaviour
{
    public float dissolveDuration = 10f;
    public float initialDelay = 0;
    public float delayPerBook = .75f;
    public float accelerationFactor = 1f;
    public float minDelayPerBook = .1f;

    public Shader dissolveShader;
    public List<ColorByMaterial> colorsByMaterial;

    private Dictionary<string, Color> colorDict = new Dictionary<string, Color>();
    private List<GameObject> _associatedBooks;
    private List<Material> _bookMaterials;

    // Start is called before the first frame update
    void Start()
    {
        foreach (ColorByMaterial colorByMaterial in colorsByMaterial)
        {
            colorDict.Add($"{colorByMaterial.material.name} (Instance)", colorByMaterial.color);
        }

        _associatedBooks = GetComponent<ReplicatorController>().replicatedObjectInstances;
        _bookMaterials = _associatedBooks.Select(book => book.GetComponentInChildren<Renderer>().material).ToList();


        Sequence sequence = DOTween.Sequence();
        float delay = 0, delayIncrement = delayPerBook;
        foreach (Material bookMaterial in _bookMaterials)
        {
            bookMaterial.shader = dissolveShader;
            bookMaterial.SetColor("_EdgeColor", colorDict[bookMaterial.name]);
            bookMaterial.SetFloat("_EdgeColorIntensity", .5f);
            bookMaterial.SetFloat("_Dissolve", 0);

            Vector2 offset = new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f));
            bookMaterial.SetVector("_NoiseUVSpeed", offset);

            if (delay == 0)
            {
                sequence.Append(bookMaterial.DOFloat(1f, "_Dissolve", dissolveDuration).SetDelay(initialDelay));
            }
            else
            {
                sequence.Insert(initialDelay + delay, bookMaterial.DOFloat(1f, "_Dissolve", 10));
            }

            delay += delayIncrement;
            delayIncrement = Mathf.Max(delayIncrement * accelerationFactor, minDelayPerBook);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
