using UnityEngine;

public interface IConstructable
{
    public PlaceableSO PlaceableToConstruct { get; }
    public Transform ModelTransform { get; }
    public Material TransparentMaterial { get; }
    public void OnConstructionFinished();
}
