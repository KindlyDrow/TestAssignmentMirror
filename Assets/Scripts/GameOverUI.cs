using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winerNameText;

    private void Start()
    {
        
        StartCoroutine(WaitngForGameManager());
        

    }

    private IEnumerator WaitngForGameManager()
    {
        while (GameManager.Instance == null)
        {
            yield return null;
        }
        GameManager.Instance.OnGameEnded += GameManager_OnGameEnded;
        Hide();
    }

    private void GameManager_OnGameEnded(object sender, System.EventArgs e)
    {
        Show();
        winerNameText.text = (GameManager.Instance.GetWinerName() + " WIN");
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameEnded -= GameManager_OnGameEnded;
    }
}
