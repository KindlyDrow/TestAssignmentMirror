using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameSingleUI;
    [SerializeField] private TextMeshProUGUI playerScoreSingleUI;

    public void SetPlayerInfo(string playerName, int score, int position)
    {
        playerNameSingleUI.text = (position + ". " + playerName);
        playerScoreSingleUI.text = score.ToString();
    }
}
