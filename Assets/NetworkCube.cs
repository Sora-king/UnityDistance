using Photon.Pun;
using UnityEngine;

public class NetworkCube : MonoBehaviour
{
    [SerializeField] private PhotonView photonView;

    private void Update()
    {
        var add = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            add = GetAdditionalMovement(-0.1f, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            add = GetAdditionalMovement(0.1f, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            add = GetAdditionalMovement(0, 0, 0.1f);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            add =GetAdditionalMovement(0, 0, -0.1f);
        }
        transform.position += add;
    }

    private Vector3 GetAdditionalMovement(float x, float y, float z)
    {
        if (!photonView.IsMine)
        {
            Debug.Log("同期されるオブジェクトなので操作できません。");
            return Vector3.zero;
        }
        return new Vector3(x, 0, z);
    }
}

