using UnityEngine;
using UnityEngine.UI;

public class LineoutSlotSelectorUI : MonoBehaviour
{
    public Button frontButton;
    public Button middleButton;
    public Button backButton;

    public NeedleController needleController;

    LineoutManager lineoutManager;

    private void Awake()
    {
        Hide();
    }
    public void Show(LineoutManager manager)
    {
        lineoutManager = manager;
        gameObject.SetActive(true);
        SetupButtons();
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    private void SetupButtons()
    {
        RegisterButton(frontButton, 0);
        RegisterButton(middleButton, 1);
        RegisterButton(backButton, 2);
    }

    private void RegisterButton(Button button, int index)
    {
        if (button == null) return;
        // When button clicked run OnSlotSelected
        button.onClick.AddListener(() => OnSlotSelected(index));
    }
    private void OnSlotSelected(int slotIndex)
    {
        lineoutManager.NotifySlotSelected(slotIndex);
        Hide();
        needleController.Activate(lineoutManager);
    }
}
