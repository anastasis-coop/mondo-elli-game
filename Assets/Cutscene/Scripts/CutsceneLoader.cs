using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cutscene
{
    public class CutsceneLoader : MonoBehaviour
    {
        [SerializeField]
        private AssetReference cutscene;

        public void Load(string id, Action callback)
        {
            // cleanup
            StartCoroutine(LoadRoutine(id, callback));
        }

        private IEnumerator LoadRoutine(string id, Action callback)
        {
            // lock UI in some way (like video player did maybe?)

            var loadOperation = cutscene.LoadSceneAsync(LoadSceneMode.Additive);

            yield return loadOperation;

            Scene scene = loadOperation.Result.Scene;

            CutscenePlayer.Instance.Play(id, () => OnCutsceneEnd(scene, callback));
        }

        private void OnCutsceneEnd(/*maybe skip*/Scene scene, Action callback)
        {
            if (scene.isLoaded)
            {
                cutscene.UnLoadScene();

                // wait for unload finish
            }

            callback?.Invoke();
        }
    }
}