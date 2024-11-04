using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Lobby : MonoBehaviourPunCallbacks
{
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _startButton;
    [SerializeField] private GameObject _leaveButton;
    [SerializeField] private GameObject _loadingScreen;
    [SerializeField] private GameObject _lobbyScreen;
    [SerializeField] private BoardCreator _boardCreator;

    private readonly RaiseEventOptions _eventOptions = new RaiseEventOptions()
    {
        Receivers = ReceiverGroup.All
    };
    private const byte MaxPlayer = 6;

    public bool IsMultiplayer { get; private set; } = false;

    public void Reset()
    {
        IsMultiplayer = false;
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void ConnectToServer()
    {
        PhotonNetwork.NickName = PlayerData.instance.previousName;
        PhotonNetwork.SendRate = 20;
        PhotonNetwork.SerializationRate = 60;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void Leave()
    {
        PhotonNetwork.Disconnect();
    }

    public void Play()
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        photonView.RPC(nameof(PlayNetwork), RpcTarget.AllBuffered, seed);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        _camera.transform.position = new Vector3(_camera.transform.position.x, 4f, _camera.transform.position.z);
        _camera.transform.rotation = Quaternion.Euler(14.947f, 0, 0);
        _loadingScreen.SetActive(true);
        _lobbyScreen.SetActive(false);
        _boardCreator.Clear();

        Screen.orientation = ScreenOrientation.Portrait;
    }

    public override void OnConnectedToMaster()
    {
        if (PhotonNetwork.InLobby == false)
            PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = MaxPlayer;

        PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: options);
    }

    public override void OnJoinedRoom()
    {
        _loadingScreen.SetActive(false);
        _boardCreator.Create(PhotonNetwork.LocalPlayer);
        _startButton.SetActive(PhotonNetwork.IsMasterClient);
        _leaveButton.SetActive(true);

        Hashtable hashtable = new Hashtable()
        {
            ["CharacterIndex"] = GetCharacterIndex()
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        _boardCreator.Create(newPlayer);
        photonView.RPC(nameof(CreatePlayersInBoard), newPlayer, _boardCreator.ActorNumbers.ToArray());
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        _boardCreator.TryDestroy(otherPlayer);
    }

    [PunRPC]
    public void PlayNetwork(int seed)
    {
        _camera.transform.position = new Vector3(_camera.transform.position.x, 8f, _camera.transform.position.z);
        _camera.transform.rotation = Quaternion.Euler(34.84f, 0, 0);
        Screen.orientation = ScreenOrientation.LandscapeLeft;
                                                                                                                                                                                                                                                        
        IsMultiplayer = true;
        TrackManager.SetSeed(seed);
        GameManager.instance.SwitchState("Multiplayer");
    }

    [PunRPC]
    public void CreatePlayersInBoard(int[] actorNumbers)
    {
        foreach (var actorNumber in actorNumbers)
        {
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            _boardCreator.Create(player);
        }
    }
    
    private int GetCharacterIndex()
    {
        if (PlayerPrefs.HasKey("CharacterIndex"))
            return PlayerPrefs.GetInt("CharacterIndex");

        return 0;
    }
}
