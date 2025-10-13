using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class MainSceneController : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string targetSceneName = "CardNegotiationsPartialScene";
    
    private ZenjectSceneLoader _sceneLoader;
    
    [Inject]
    public void Construct(ZenjectSceneLoader sceneLoader)
    {
        _sceneLoader = sceneLoader;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Check for E key press to load target scene
        if (Input.GetKeyDown(KeyCode.E))
        {
            LoadTargetScene();
        }
    }
    
    /// <summary>
    /// Load the target scene additively using Zenject
    /// </summary>
    private void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            _sceneLoader.LoadScene(targetSceneName, LoadSceneMode.Additive);
        }
        else
        {
            Debug.LogWarning("Target scene name is not set!");
        }
    }
}
