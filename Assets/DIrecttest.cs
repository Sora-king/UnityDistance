using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
using Photon.Voice.Unity;
using System.Collections.Generic;
using System.Linq;



public class Directiontest : MonoBehaviourPunCallbacks
{
    public Transform myAvatar; // 自分のアバターの Transform
    public float angleThreshold = 30f; // 正面方向の角度閾値（±30度）
    public float defaultVolume = 0.1f; // 正面以外の音量
    public float maxVolume = 1.0f; // 正面方向の音量
    bool flag = false;

    List<int> announce_playerList = new List<int>();

    
    [SerializeField]
    private AudioSource myaudioSource;

    void Start()
    {
        // 自分のアバターを探して設定
        StartCoroutine(WaitForMyAvatar());
    }

    private IEnumerator WaitForMyAvatar()
    {
        while (myAvatar == null)
        {
            foreach (PhotonView view in FindObjectsOfType<PhotonView>())
            {
                if (view.IsMine)
                {
                    myAvatar = view.transform;
                    Debug.Log("【デバッグ】自分のアバターを設定しました。");
                    break;
                }
            }
            yield return null; // 次のフレームまで待機
        }
    }

    void Update()
    {
        if (myAvatar == null)
        {
            Debug.Log("【デバッグ】自分のアバターがまだ設定されていません。");
            return;
        }
        if(flag) return;
        if(photonView.Owner.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            Debug.Log("他人");
            flag = true;
            return;
        }
        // 他のプレイヤーの音量を毎フレーム調整
        AdjustVolumesForAllPlayers();
    }

    private void AdjustVolumesForAllPlayers()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player == PhotonNetwork.LocalPlayer) continue; // 自分自身はスキップ
            if (announce_playerList.Contains(player.ActorNumber)){
                Debug.Log("ああああああ");
                GameObject targetAvatar1 = FindAvatarByPlayer(player);
                Speaker speaker1 = targetAvatar1.transform.GetComponent<Speaker>();
                AudioSource audioSource1 = speaker1.GetComponent<AudioSource>();
                audioSource1.volume = maxVolume;
                continue;
            }

            // プレイヤーに紐付くアバターを取得
            GameObject targetAvatar = FindAvatarByPlayer(player);
            if (targetAvatar != null)
            {
                AdjustVolume(targetAvatar.transform);
            }
        }
    }

    private GameObject FindAvatarByPlayer(Player player)
    {
        foreach (PhotonView view in FindObjectsOfType<PhotonView>())
        {
            if (view.Owner != null && view.Owner == player)
            {
                return view.gameObject; // 該当するアバターを返す
            }
        }
        return null; // アバターが見つからなかった場合
    }

    private void AdjustVolume(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("【デバッグ】ターゲットがnullのため音量調整をスキップしました。");
            return;
        }

        if (myAvatar == null)
        {
            Debug.LogWarning("【デバッグ】自分のアバターがnullのため音量調整をスキップしました。");
            return;
        }

        // 自分の位置とターゲットの位置を取得
        Vector3 directionToTarget = target.position - myAvatar.position;

        // 水平面での方向ベクトルを計算（高さ成分を無視）
        directionToTarget.y = 0; // y成分を無視することで水平方向のみを考慮
        directionToTarget.Normalize(); // 正規化

        // 自分のアバターの正面方向（水平成分のみ）
        Vector3 forwardDirection = myAvatar.forward;
        forwardDirection.y = 0; // y成分を無視
        forwardDirection.Normalize();

        // 水平面での角度を計算
        float angle = Vector3.Angle(forwardDirection, directionToTarget);

        // Speaker に関連付けられた AudioSource を取得
        Speaker speaker = target.GetComponent<Speaker>();
        if (speaker != null)
        {
            AudioSource audioSource = speaker.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.volume = angle <= angleThreshold ? maxVolume : defaultVolume;
                //Debug.Log($"【デバッグ】AudioSourceの音量を変更しました: {audioSource.volume}");
            }
            else
            {
                Debug.LogError("【デバッグ】AudioSourceがSpeakerにアタッチされていません。");
            }
        }
        else
        {
            Debug.LogError("【デバッグ】Speakerコンポーネントがターゲットにアタッチされていません。");
        }
    }

    // RPCでSpatial Blendを切り替える関数
    [PunRPC] // Photon PUNのRPC属性
    public void SetSpatialBlend(bool is3D)
    {
        if (myaudioSource != null)
        {
            // 3Dなら1.0、2Dなら0.0に設定
            myaudioSource.spatialBlend = is3D ? 1.0f : 0.0f;
            Debug.Log($"[Avatar ID: {photonView.Owner.ActorNumber}] Spatial Blend を {(is3D ? "3D" : "2D")} に切り替えました。");
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("announceplayer") && photonView.Owner.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            // カスタムプロパティから取得
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("announceplayer", out object value))
            {
                int[] playerActorNumbers = value as int[]; // カスタムプロパティからIDリストを取得

                if (playerActorNumbers != null)
                {
                    // announce_playerList にIDを追加
                    announce_playerList = playerActorNumbers.ToList();
                    Debug.Log($"Player ID リストをカスタムプロパティから取得しました: {string.Join(", ", announce_playerList)}");
                }
                else
                {
                    Debug.LogWarning("カスタムプロパティ 'announceplayer' に保存されたデータが null です。");
                }
            }
            else
            {
                Debug.LogWarning("カスタムプロパティ 'announceplayer' が見つかりません。");
            }
        }
    }
}
