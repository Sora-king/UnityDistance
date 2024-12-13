using Photon.Pun; 
using Photon.Realtime;
using UnityEngine;
using TMPro;

public class AvatarNameDisplay : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text nameText; // 名前を表示するTextMeshPro
    private Transform avatarTransform; // アバターのTransform
    private const string NAME_KEY_PREFIX = "name_"; // カスタムプロパティキーの接頭辞

    void Start()
    {
        // アバターのTransformを取得
        avatarTransform = transform;

        // 自分のアバターなら名前表示を無効化
        if (photonView.IsMine)
        {
            nameText.gameObject.SetActive(false);
            return;
        }

        // 名前を初期化して表示
        UpdateNameDisplay();
    }

    void LateUpdate()
    {
        if (photonView.IsMine || Camera.main == null)
        {
            return; // 自分のアバターまたはカメラが未設定の場合は処理をスキップ
        }

        // 名前をカメラに向けて表示
        nameText.transform.rotation = Camera.main.transform.rotation;

        // 名前をアバターの頭上に表示（必要に応じてY軸オフセットを調整）
        Vector3 offset = new Vector3(0, 2.5f, 0); // アバターの頭上に名前を表示
        nameText.transform.position = avatarTransform.position + offset;
    }

    private void UpdateNameDisplay()
    {
        // 名前が設定されている場合は表示
        if (photonView.Owner.CustomProperties.TryGetValue(NAME_KEY_PREFIX + photonView.Owner.ActorNumber, out object playerName))
        {
            nameText.text = playerName.ToString();
        }
        else
        {
            // 名前が未設定の場合はIDを表示
            nameText.text = $"ID: {photonView.Owner.ActorNumber}";
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        // プレイヤー名が変更された場合に更新
        if (changedProps.ContainsKey(NAME_KEY_PREFIX + targetPlayer.ActorNumber))
        {
            if (photonView.Owner == targetPlayer)
            {
                UpdateNameDisplay();
            }
        }
    }
}
