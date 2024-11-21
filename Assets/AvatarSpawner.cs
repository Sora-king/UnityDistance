using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class AvatarSpawner : MonoBehaviourPunCallbacks
{
    public GameObject avatarPrefab; // アバターPrefabを設定

    void Start()
    {
        // アバターを生成
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
            PhotonNetwork.Instantiate(avatarPrefab.name, spawnPosition, Quaternion.identity);
        }
    }
}

