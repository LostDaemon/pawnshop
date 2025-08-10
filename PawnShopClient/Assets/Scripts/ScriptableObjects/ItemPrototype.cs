using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ItemPrototype", order = 1)]
public class ItemPrototype : BasePrototype
{
    public string Name;
    public string ImageId;
    public long BasePrice;
    public float Scale;
    public string Description;
}