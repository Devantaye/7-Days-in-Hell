using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using UnityEngine;

public class CountDownUI : MonoBehaviour
{
[SerializeField] private TextMeshProUGUI countdownText;

private void Start() {
    HellGameManager.Instance.OnStateChanged += HellGameManager_OnStateChanged;

    Hide();
}

private void HellGameManager_OnStateChanged(object sender, System.EventArgs e) {
    if (HellGameManager.Instance.IsCountdownToStartActive()) {
        Show();
    } else {
        Hide();
            }
}

private void Update() {
    countdownText.text = HellGameManager.Instance.GetCountdownToStartTimer().ToString();
}
   private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

}

