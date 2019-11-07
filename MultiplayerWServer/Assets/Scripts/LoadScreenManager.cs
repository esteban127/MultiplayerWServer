using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScreenManager : MonoBehaviour
{
    [SerializeField] Slider progressBar;
    private void Start()
    {
        StartCoroutine(LoadAsyncScene());
    }
    IEnumerator LoadAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(Server.Instance.CurrentScene);

        while (!asyncLoad.isDone)
        {
            progressBar.value = asyncLoad.progress;
            yield return null;
        }
    }
}
