using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerListUI : MonoBehaviour
{
    [SerializeField] private GameObject infoContainer;
    [SerializeField] private GameObject infoTemplate;

    private void Start()
    {
        StartCoroutine(WaitngForGameManager());
    }

    private IEnumerator WaitngForGameManager()
    {
        while(GameManager.Instance ==  null)
        {
            yield return null;
        }
        GameManager.Instance.OnPlayerScoreChanged += GameManager_OnPlayerScoreChanged;
    }

    private void GameManager_OnPlayerScoreChanged(object sender, EventArgs e)
    {
        ResetPlayerInfo();
    }

    private void ResetPlayerInfo()
    {
        foreach (Transform child in infoContainer.transform)
        {
            if (child == infoTemplate.transform) continue;
            Destroy(child.gameObject);
        }

        Dictionary<string, int> playersNotSorted = GameManager.Instance.GetPlayersScoreDictionary();

        Dictionary<string, int> playersSorted = playersNotSorted.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

        int i = 0;

        foreach (var player in playersSorted)
        {
            i++;
            GameObject infoGO = Instantiate(infoTemplate, infoContainer.transform);
            infoGO.SetActive(true);
            infoGO.GetComponent<PlayerInfoSingleUI>().SetPlayerInfo(player.Key, player.Value, i);
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPlayerScoreChanged -= GameManager_OnPlayerScoreChanged;
    }
}
