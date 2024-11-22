using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

using Unity.Services.Authentication;
using Unity.Services.Core;
using Utp;
using System;
using Mirror.BouncyCastle.Bcpg.OpenPgp;

// Custom NetworkManager that simply assigns the correct racket positions when
// spawning players. The built in RoundRobin spawn method wouldn't work after
// someone reconnects (both players would be on the same side).
[AddComponentMenu("")]
public class RelayPongNetworkManager : RelayNetworkManager
{
    public Transform leftRacketSpawn;
    public Transform rightRacketSpawn;

    public bool isLoggedIn = false;
    GameObject ball;
    public async void UnityLogin()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Logged into Unity, player ID: " + AuthenticationService.Instance.PlayerId);
            isLoggedIn = true;
        }
        catch (Exception e)
        {
            isLoggedIn = false;
            Debug.Log(e);
        }
    }

    public void StartRelayHost()
    {
        int maxPlayers = 8;
        base.StartRelayHost(maxPlayers);
    }

    /*public void StartStandartHost()
    {
        base.StartStandardHost();
    }*/
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // add player at correct spawn position
        Transform start = numPlayers == 0 ? leftRacketSpawn : rightRacketSpawn;
        GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
        NetworkServer.AddPlayerForConnection(conn, player);

        // spawn ball if two players
        if (numPlayers == 2)
        {
            ball = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Ball"));
            NetworkServer.Spawn(ball);
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // destroy ball
        if (ball != null)
            NetworkServer.Destroy(ball);

        // call base functionality (actually destroys the player)
        base.OnServerDisconnect(conn);
    }
}
