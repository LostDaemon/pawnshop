using UnityEngine;

public abstract class BaseLoader<BasePrototype> : MonoBehaviour
{
    [SerializeField] private BasePrototype[] _prototypes;

    void Start()
    {
        foreach (var prototype in _prototypes)
        {
            Load(prototype);
        }
    }

    protected abstract void Load(BasePrototype prototype);
}
