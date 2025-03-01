using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class SceneTransitionAnim : MonoBehaviour
{
    public static SceneTransitionAnim Instance { get; private set; }
    [SerializeField] private UIDocument uIDocument;
    [SerializeField] private PlayableDirector sceneAnim;
    [SerializeField] private TimelineAsset exitAnim;
    [SerializeField] private TimelineAsset enterAnim;
    VisualElement transition;
    void Start() {
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(gameObject);
        }
    }

    void OnEnable() {
        if(UnityUtils.ContainsElement(uIDocument.rootVisualElement, "Transition-Container", out VisualElement output)){
            transition = output;
        }
        sceneAnim.stopped += OnAnimationFinished;
    }

    void OnDisable() {
        sceneAnim.stopped -= OnAnimationFinished;
    }
    public void ExitAnim() {
        if(sceneAnim.state != PlayState.Playing){
            sceneAnim.playableAsset = exitAnim;
            transition.visible = true;
            sceneAnim.Play();              
        }
    }
    public void EnterAnim() {
        sceneAnim.playableAsset = enterAnim;
        transition.visible = true;
        sceneAnim.Play();   
    }
    private void OnAnimationFinished(PlayableDirector director){
        transition.visible = false;
    }
}
