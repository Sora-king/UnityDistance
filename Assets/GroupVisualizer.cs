using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GroupVisualizer : MonoBehaviour
{
    public GameObject groupCirclePrefab; // 円を描画するためのプレハブ（LineRenderer付き）
    public GameObject groupNameUIPrefab; // グループ名表示用のUIプレハブ
    public Transform canvasTransform; // UI を配置するキャンバスの Transform

    /// <summary>
    /// グループの可視化を行う
    /// </summary>
    /// <param name="center">グループの中心位置</param>
    /// <param name="radius">グループの円の半径</param>
    /// <param name="groupName">グループ名</param>
    /// <param name="groupColor">グループの色</param>
    public void VisualizeGroup(Vector3 center, float radius, string groupName, Color groupColor)
    {
        // グループ円の生成
        GameObject circle = Instantiate(groupCirclePrefab, center, Quaternion.identity);
        LineRenderer lineRenderer = circle.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.startColor = groupColor;
            lineRenderer.endColor = groupColor;
            DrawCircle(lineRenderer, radius);
        }

        // グループ名の生成
        GameObject nameTag = Instantiate(groupNameUIPrefab, canvasTransform);
        nameTag.GetComponentInChildren<TMP_Text>().text = groupName;

        // グループ名UIの色を変更
        Image background = nameTag.GetComponentInChildren<Image>();
        if (background != null)
        {
            background.color = groupColor;
        }

        // UI の位置を調整
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(center);
        nameTag.transform.position = new Vector3(screenPosition.x, screenPosition.y + 50, 0);
    }

    /// <summary>
    /// LineRenderer を使って円を描画する
    /// </summary>
    /// <param name="lineRenderer">LineRenderer コンポーネント</param>
    /// <param name="radius">円の半径</param>
    private void DrawCircle(LineRenderer lineRenderer, float radius)
    {
        int segments = 100; // 円のセグメント数
        lineRenderer.positionCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * 2 * Mathf.PI / segments;
            Vector3 position = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            lineRenderer.SetPosition(i, position);
        }

        lineRenderer.loop = true; // 円を閉じる
    }
}
