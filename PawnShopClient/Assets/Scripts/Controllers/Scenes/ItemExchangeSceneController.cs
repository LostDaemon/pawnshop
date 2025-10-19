using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ItemExchangeSceneController : MonoBehaviour
{
    [SerializeField] private Button _closeButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeButtons();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitializeButtons()
    {
        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    private void OnCloseButtonClicked()
    {
        UnloadItemExchangeScene();
    }

    private void UnloadItemExchangeScene()
    {
        SceneManager.UnloadSceneAsync("ItemExchangePartialScene");
    }

    private void OnDestroy()
    {
        // Unsubscribe from button events
        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }
    }
}
