using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject masterPanel;
    public GameObject hideButton;
    public GameObject showButton;
    private bool isShown;

    void Start() {
        isShown = masterPanel.activeSelf;
    }


    public void OnHideShowPanelClicked() {
        isShown = !isShown;
        masterPanel.SetActive(isShown);
        hideButton.SetActive(isShown);
        showButton.SetActive(!isShown);
    }
}
