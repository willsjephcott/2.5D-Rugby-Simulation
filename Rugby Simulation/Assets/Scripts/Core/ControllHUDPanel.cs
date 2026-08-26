using UnityEngine;

public class ControlHUDPanel : MonoBehaviour
{

    [SerializeField] private ControlPromptUI leftPrompt;
    [SerializeField] private ControlPromptUI rightPrompt;

    public void ShowAttack(string passLeftKey, string passRightKey)
    {
        leftPrompt.SetContent("Pass Left", passLeftKey);
        rightPrompt.SetContent("Pass Right", passRightKey);
        SetVisible(true);
    }

    public void ShowDefend(string tackleKey, string switchKey)
    {
        leftPrompt.SetContent("Tackle", tackleKey);
        rightPrompt.SetContent("Switch\nPlayer", switchKey);
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}