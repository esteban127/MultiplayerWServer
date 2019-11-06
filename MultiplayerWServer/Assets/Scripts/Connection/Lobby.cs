using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Lobby : MonoBehaviour
{
    [SerializeField] GameObject playerLabelPrefav;
    [SerializeField] Transform parent;
    [SerializeField] GameObject playButton;
    bool hosting;
    public void Join(bool isHost)
    {
        gameObject.SetActive(true);
        hosting = isHost;
        if (hosting)
            playButton.SetActive(true);
    }
    public GameObject AddPlayer(string playerName)
    {
        GameObject newPlayer = Instantiate(playerLabelPrefav, parent);
        newPlayer.GetComponentInChildren<Text>().text = playerName;
        return newPlayer;
    }
    public void Leave()
    {
        gameObject.SetActive(false);
        foreach (Transform child in parent)
        {
            GameObject.Destroy(child.gameObject);
        }
        if (hosting)
            playButton.SetActive(false);
    }
}
