using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;

public class SelfNameDisplay : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text nameDisplayText; // 自分の名前を表示するTextMeshPro
    private const string NAME_KEY_PREFIX = "name_"; // カスタムプロパティキーの接頭辞

    void Start()
    {
        if (nameDisplayText == null)
        {
            Debug.LogError("【エラー】名前表示用のTextMeshProがアサインされていません！");
            return;
        }

        // 初期化して名前を表示
        UpdateNameDisplay();

        // 名前が設定されていない場合、デフォルトの名前を設定
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(NAME_KEY_PREFIX + PhotonNetwork.LocalPlayer.ActorNumber))
        {
            SetDefaultName();
        }
    }

    private void UpdateNameDisplay()
    {
        // CustomPropertiesから名前を取得して表示
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(NAME_KEY_PREFIX + PhotonNetwork.LocalPlayer.ActorNumber, out object playerName))
        {
            nameDisplayText.text = playerName.ToString();
        }
        else
        {
            nameDisplayText.text = $"ID: {PhotonNetwork.LocalPlayer.ActorNumber}";
        }
    }

    private void SetDefaultName()
    {
        string defaultName = "Player_" + PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        {
            { NAME_KEY_PREFIX + PhotonNetwork.LocalPlayer.ActorNumber, defaultName }
        });
        UpdateNameDisplay();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        // 自分の名前が変更された場合に更新
        if (targetPlayer == PhotonNetwork.LocalPlayer && changedProps.ContainsKey(NAME_KEY_PREFIX + targetPlayer.ActorNumber))
        {
            UpdateNameDisplay();
        }
    }
}
