using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon; // Photon の Hashtable を使用
using System.Collections.Generic;

public class GroupVisualizer2 : MonoBehaviourPunCallbacks
{
    public GameObject cylinderPrefab; // グループを囲む円柱のプレハブ
    private const string GROUP_KEY = "Groups";
    private List<GameObject> groupCylinders = new List<GameObject>(); // 生成された円柱の参照リスト
    private bool isReady = false; // 部屋入室待機状態

    private void Start()
    {
        // 部屋入室待機コルーチンを開始
        StartCoroutine(WaitForRoomAndInitialize());
    }

    private System.Collections.IEnumerator WaitForRoomAndInitialize()
    {
        // プレイヤーが部屋に入室するまで待機
        while (!PhotonNetwork.InRoom)
        {
            Debug.Log("部屋に入室待機中...");
            yield return null; // フレームごとにチェック
        }

        Debug.Log("部屋に入室しました。初期化を開始します。");
        isReady = true;

        // カスタムプロパティを初期化
        UpdateGroupVisualization(PhotonNetwork.CurrentRoom.CustomProperties);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        // 部屋入室が完了している場合のみ、更新を適用
        if (isReady && propertiesThatChanged.ContainsKey(GROUP_KEY))
        {
            UpdateGroupVisualization(propertiesThatChanged);
        }
    }

    private void UpdateGroupVisualization(ExitGames.Client.Photon.Hashtable properties)
    {
        // 古い円柱を削除
        ClearPreviousCylinders();

        // グループ情報が存在しない場合は終了
        if (!properties.ContainsKey(GROUP_KEY)) return;

        ExitGames.Client.Photon.Hashtable groups = properties[GROUP_KEY] as ExitGames.Client.Photon.Hashtable;
        if (groups == null) return;

        // 各グループに対して円柱を生成
        foreach (var entry in groups)
        {
            ExitGames.Client.Photon.Hashtable groupInfo = entry.Value as ExitGames.Client.Photon.Hashtable;
            if (groupInfo == null) continue;

            // グループ情報から必要なデータを取得
            if (!groupInfo.ContainsKey("Center") || !groupInfo.ContainsKey("Radius") || !groupInfo.ContainsKey("Color")) continue;

            float[] centerArray = groupInfo["Center"] as float[];
            float radius = (float)groupInfo["Radius"];
            float[] colorArray = groupInfo["Color"] as float[];

            if (centerArray == null || centerArray.Length < 3 || colorArray == null || colorArray.Length < 3) continue;

            Vector3 center = new Vector3(centerArray[0], centerArray[1], centerArray[2]);
            Color color = new Color(colorArray[0], colorArray[1], colorArray[2]);

            // 円柱を生成
            CreateCylinder(center, radius, color);
        }
    }

    private void CreateCylinder(Vector3 center, float radius, Color color)
    {
        GameObject cylinder = Instantiate(cylinderPrefab, center, Quaternion.identity);
        cylinder.transform.localScale = new Vector3(radius * 2, 0.1f, radius * 2); // X,Zに半径を適用、Yを薄く設定
        Renderer renderer = cylinder.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
        groupCylinders.Add(cylinder);
    }

    private void ClearPreviousCylinders()
    {
        foreach (GameObject cylinder in groupCylinders)
        {
            Destroy(cylinder);
        }
        groupCylinders.Clear();
    }
}
