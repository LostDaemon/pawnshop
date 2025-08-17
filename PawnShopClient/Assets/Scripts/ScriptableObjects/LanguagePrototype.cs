using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/LanguagePrototype", order = 1)]
public class LanguagePrototype : BasePrototype
{
    [Header("Language Information")]
    public Language Language;
    public string Name;
    public string fileName;
}
