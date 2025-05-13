using UnityEngine;
using Zenject;

public class GameLoopState : IGameState
{
    private IItemRepositoryService _itemRepository;

    [Inject]
    public void Construct(IItemRepositoryService itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public void Enter()
    {
        var item = _itemRepository.GetRandomItem();

        var prefab = Resources.Load<ItemController>("UI/ItemPrefab");
        Debug.Log(prefab);
        var canvas = GameObject.FindAnyObjectByType<Canvas>();
        var view = Object.Instantiate(prefab, canvas.transform);
        view.Show(item);
    }

    public void Exit() { }
}