using Photon.Pun;
using UnityEngine;

public class NetworkCubeMaker : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject networkCube;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            PhotonNetwork.Instantiate(networkCube.name, Vector3.zero, Quaternion.identity, 0);
        }
    }
}