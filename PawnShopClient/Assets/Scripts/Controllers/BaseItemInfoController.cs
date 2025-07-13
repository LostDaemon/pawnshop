using UnityEngine;

public abstract class BaseItemInfoController : MonoBehaviour
{
    protected ItemModel Item { get; private set; }

    public void SetItem(ItemModel item)
    {
        Item = item;
        OnItemChanged();
    }

    protected abstract void OnItemChanged();

    private void Awake()
    {
        OnAwake();
    }

    protected virtual void OnAwake()
    {
    }
}