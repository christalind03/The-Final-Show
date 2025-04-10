using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class UITesting : MonoBehaviour
{
    [SerializeField] private UIDocument uIDocument;
    [SerializeField] private PlayableDirector sceneAnim;
    [SerializeField] private TimelineAsset testAnim;
    private VisualElement _menuView;
    private VisualElement _joinView;

    void Start()
    {
        VisualElement rootVisualElement = uIDocument.rootVisualElement;
        if (UnityUtils.ContainsElement(rootVisualElement, "LobbyMenu", out VisualElement mainmenu))
        {
            _menuView = mainmenu;
        }
        if (UnityUtils.ContainsElement(rootVisualElement, "LobbyJoin", out VisualElement joinmenu))
        {
            _joinView = joinmenu;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchUI(_menuView, _joinView);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
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