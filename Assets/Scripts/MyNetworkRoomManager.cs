using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyNetworkRoomManager : NetworkRoomManager
{
    public bool isFirstTime = true;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (isFirstTime)
        {
            
            base.OnServerAddPlayer(conn);
        }
        else
        {
            Transform startPos = GetStartPosition();
            GameObject player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            // instantiating a "Player" prefab gives it the name "Player(clone)"
            // => appending the connectionId is WAY more useful for debugging!
            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, player);
        }
    }

    public override Transform GetStartPosition()
    {
        isFirstTime = false;
        // first remove any dead transforms
        startPositions.RemoveAll(t => t == null);

        if (startPositions.Count == 0)
            return null;

        if (playerSpawnMethod == PlayerSpawnMethod.Random)
        {
            Transform curStart = startPositions[UnityEngine.Random.Range(0, startPositions.Count)];
            UnRegisterStartPosition(curStart);
            return curStart;
        }
        else
        {
            Transform startPosition = startPositions[startPositionIndex];
            startPositionIndex = (startPositionIndex + 1) % startPositions.Count;
            return startPosition;
        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        //if (newSceneName == RoomScene)
        //{
        //    foreach (NetworkRoomPlayer roomPlayer in roomSlots)
        //    {
        //        if (roomPlayer == null)
        //            continue;

        //        // find the game-player object for this connection, and destroy it
        //        NetworkIdentity identity = roomPlayer.GetComponent<NetworkIdentity>();

        //        if (NetworkServer.active)
        //        {
        //            // re-add the room object
        //            roomPlayer.GetComponent<NetworkRoomPlayer>().readyToBegin = !isFirstTime;
        //            NetworkServer.ReplacePlayerForConnection(identity.connectionToClient, roomPlayer.gameObject);
        //        }
        //    }

        //    allPlayersReady = !isFirstTime;
        //    isFirstTime = false;
        //}
        if (string.IsNullOrWhiteSpace(newSceneName))
        {
            Debug.LogError("ServerChangeScene empty scene name");
            return;
        }

        if (NetworkServer.isLoadingScene && newSceneName == networkSceneName)
        {
            Debug.LogError($"Scene change is already in progress for {newSceneName}");
            return;
        }

        // Debug.Log($"ServerChangeScene {newSceneName}");
        NetworkServer.SetAllClientsNotReady();
        networkSceneName = newSceneName;

        // Let server prepare for scene change
        OnServerChangeScene(newSceneName);

        // set server flag to stop processing messages while changing scenes
        // it will be re-enabled in FinishLoadScene.
        NetworkServer.isLoadingScene = true;

        loadingSceneAsync = SceneManager.LoadSceneAsync(newSceneName);

        // ServerChangeScene can be called when stopping the server
        // when this happens the server is not active so does not need to tell clients about the change
        if (NetworkServer.active)
        {
            // notify all clients about the new scene
            NetworkServer.SendToAll(new SceneMessage
            {
                sceneName = newSceneName
            });
        }

        startPositionIndex = 0;
        startPositions.Clear();
    }

}
