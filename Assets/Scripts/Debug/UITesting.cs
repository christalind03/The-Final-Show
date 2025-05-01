using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class UITesting : MonoBehaviour
{
    [SerializeField] private UIDocument uIDocument;
    [SerializeField] private PlayableDirector sceneAnim;
    [SerializeField] private TimelineAsset testAnim;
    private VisualElement _rootVisualElement;
    private VisualElement _joinView;

    void Start()
    {
        _rootVisualElement = uIDocument.rootVisualElement;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (UnityUtils.ContainsElement(_rootVisualElement, "Transition-Container", out VisualElement transition))
            {
                transition.visible = true;
            }
            if (UnityUtils.ContainsElement(_rootVisualElement, "Scene-Display", out VisualElement scene))
            {
                scene.visible = true;
            }
            ExitAnim();
        }
    }
    public void ExitAnim()
    {
        sceneAnim.playableAsset = testAnim;
        sceneAnim.Play();
    }
    private void SwitchUI(VisualElement curScreen, VisualElement newScreen)
    {
        if (curScreen != null)
        {
            curScreen.RemoveFromClassList("show");
            curScreen.AddToClassList("hide");
            curScreen.SetEnabled(false);
        }

        if (newScreen != null)
        {
            newScreen.RemoveFromClassList("hide");
            newScreen.AddToClassList("show");
            newScreen.SetEnabled(true);
        }
    }
}