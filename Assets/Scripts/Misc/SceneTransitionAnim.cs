using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class SceneTransitionAnim : MonoBehaviour
{
    public static SceneTransitionAnim Instance { get; private set; }
    [SerializeField] private UIDocument uIDocument;
    [SerializeField] private PlayableDirector sceneAnim;
    [SerializeField] private TimelineAsset exitAnim;
    [SerializeField] private TimelineAsset enterAnim;
    private VisualElement _transition;
    private TextElement _sceneName;
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        if (UnityUtils.ContainsElement(uIDocument.rootVisualElement, "Transition-Container", out VisualElement output))
        {
            _transition = output;
        }
        if (UnityUtils.ContainsElement(uIDocument.rootVisualElement, "Scene-Display", out TextElement sceneName))
        {
            _sceneName = sceneName;
        }
        sceneAnim.stopped += OnAnimationFinished;
    }

    void OnDisable()
    {
        sceneAnim.stopped -= OnAnimationFinished;
    }
    public void ExitAnim()
    {
        if (sceneAnim.state != PlayState.Playing)
        {
            sceneAnim.playableAsset = exitAnim;
            _transition.visible = true;
            sceneAnim.Play();
        }
    }
    public void EnterAnim()
    {
        string activeScene = SceneManager.GetActiveScene().name;
        activeScene = activeScene.Replace("Gameplay-", "");
        if (activeScene == "Network-Lobby") return;
        sceneAnim.playableAsset = enterAnim;
        _sceneName.visible = true;
        _sceneName.text = activeScene;
        _transition.visible = true;
        sceneAnim.Play();
    }
    private void OnAnimationFinished(PlayableDirector director)
    {
        _transition.visible = false;
        if (sceneAnim.playableAsset == enterAnim)
        {
            _sceneName.visible = false;
        }
    }
}
