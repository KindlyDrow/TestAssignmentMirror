using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnPlayerScoreChanged;
    public event EventHandler OnGameEnded;

    public enum State
    {
        GamePreparing = default,
        GameStarted,
        GameEnd,
    }

    public State _state { get; private set; }

    public readonly SyncDictionary<string, int> playersScoreDictionary = new SyncDictionary<string, int>();

    [SyncVar]
    private float _countDownTimer;

    [SerializeField] private int _winingScore = 3;
    [SerializeField] private float _restartGameEndTimer = 5;

    [SyncVar]
    private string _gameWiner;

    private void Awake()
    {
        Instance = this;
        playersScoreDictionary.Clear();

    }

    private void Update()
    {

        if (!isServer) return;
        switch (_state)
        {
            case State.GamePreparing:

                break;
            case State.GameStarted:
                break;
            case State.GameEnd:
                _countDownTimer -= Time.deltaTime;
                if (_countDownTimer < 0)
                {
                    CmdRestartScene();
                }
                break;
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdRestartScene()
    {
        MyNetworkRoomManager.singleton.Reset();
        MyNetworkRoomManager.singleton.ServerChangeScene(SceneManager.GetActiveScene().name);
    }

    [Command(requiresAuthority = false)]
    public void CmdChangePlayersScore(string name, int score)
    {
        playersScoreDictionary[name] = score;

        RpcChangePlayersScore();

        if (score >= _winingScore)
        {
            _gameWiner = name;
            CmdChangeState(State.GameEnd);
        }

    }

    [ClientRpc]
    private void RpcChangePlayersScore()
    {
        OnPlayerScoreChanged?.Invoke(this, EventArgs.Empty);
    }

    [Command(requiresAuthority = false)]
    private void CmdChangeState(State state)
    {
        RpcChangeState(state);
    }

    [ClientRpc]
    private void RpcChangeState(State state)
    {
        EndState(_state);
        switch (state)
        {
            case State.GamePreparing:
                break;
            case State.GameStarted:
                break;
            case State.GameEnd:
                OnGameEnded?.Invoke(this, EventArgs.Empty);
                _countDownTimer = _restartGameEndTimer;
                break;
        }
        _state = state;
    }

    private void EndState(State state)
    {
        switch (state)
        {
            case State.GamePreparing:
                break;
            case State.GameStarted:
                break;
            case State.GameEnd:
                break;
        }
    }

    public Dictionary<string, int> GetPlayersScoreDictionary()
    {
        Dictionary<string, int> dictionary = new Dictionary<string, int>();

        foreach (var player in playersScoreDictionary)
        {
            dictionary.Add(player.Key, player.Value);
        }

        return dictionary;
    }

    public string GetWinerName()
    {
        return _gameWiner;
    }

    private void OnDestroy()
    {
        playersScoreDictionary.Reset();
    }
}
