using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingController : MonoBehaviour
{
    AsyncOperation loadingOperation;
    public Slider progressBar;
    public CanvasGroup canvasGroup;

    // Start is called before the first frame update
    void Start()
    {
        loadingOperation = SceneManager.LoadSceneAsync("Main");
        //StartCoroutine(FadeLoadingScreen(5));
    }

    // Update is called once per frame
    void Update()
    {
        progressBar.value = Mathf.Clamp01(loadingOperation.progress / 0.9f);
    }

    //IEnumerator FadeLoadingScreen(float duration)
    //{
    //    float startValue = canvasGroup.alpha;
    //    float time = 0;

    //    while (time < duration)
    //    {
    //        canvasGroup.alpha = Mathf.Lerp(startValue, 1, time / duration);
    //        time += Time.deltaTime;
    //        yield return null;
    //    }
    //    canvasGroup.alpha = 1;
    //}
}
