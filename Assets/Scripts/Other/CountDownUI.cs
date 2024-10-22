using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountDownUI : MonoBehaviour
{
[SerializeField] private TextMeshProUGUI countdownText;

private void Start() {
    if (HellGameManager.Instance != null) {
        HellGameManager.Instance.OnStateChanged += HellGameManager_OnStateChanged;
        Hide();
    } else {
        Debug.LogError("HellGameManager instance is null in CountDownUI");
    }
}


private void HellGameManager_OnStateChanged(object sender, System.EventArgs e) {
    if (HellGameManager.Instance.IsCountdownToStartActive()) {
        Show();
    } else {
        Hide();
            }
}

private void Update() {
    if (HellGameManager.Instance != null && countdownText != null && HellGameManager.Instance.IsCountdownToStartActive()) 
    {

        countdownText.text = Mathf.Ceil(HellGameManager.Instance.GetCountdownToStartTimer()).ToString();
    }
}
   private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

}

