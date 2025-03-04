using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class UITesting : MonoBehaviour
{   
    [SerializeField] private UIDocument uIDocument;
    [SerializeField] private PlayableDirector sceneAnim;
    [SerializeField] private TimelineAsset exitAnim;
    [SerializeField] private TimelineAsset enterAnim;
    VisualElement transition;
    void Start() {
        if(UnityUtils.ContainsElement(uIDocument.rootVisualElement, "Transition-Container", out VisualElement output)){
            transition = output;
        }
        sceneAnim.stopped += OnAnimationFinished;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q)) {
            ExitAnim();
        }
        if(Input.GetKeyDown(KeyCode.E)) {
            EnterAnim();
        }
    }
    public void ExitAnim() {
        sceneAnim.playableAsset = exitAnim;
        transition.visible = true;
        sceneAnim.Play();   
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