using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Room2
{

	[Serializable]
	public class PrefabsFolder
	{
		public string name;
		public List<GameObject> prefabs;
	}
	[Serializable]
	public class AudioClipsFolder
	{
		public string name;
		public List<AudioClip> audioClips;
	}
	[Serializable]
	public class LocalizedAudioClipsFolder
	{
		public string name;
		// Using array because in Localization 1.4.4 List<> fires nullrefs in the editor drawer
		public LocalizedAudioClip[] audioClips;
	}

	public class ResourcesManager : MonoBehaviour
	{
		public List<PrefabsFolder> prefabFolders;

		public List<AudioClipsFolder> audioClipFolders;

		public List<LocalizedAudioClipsFolder> localizedAudioClipsFolders;

        public List<GameObject> GetPrefabsByFolderName(string name) {
			PrefabsFolder folder = prefabFolders.Find(f => f.name.Equals(name));
			if (folder == null)
				return null;
			return folder.prefabs;
		}

		public IEnumerator GetAudioClipsByFolderName(string name, List<AudioClip> result)
        {
			result.Clear();

            var folder = audioClipFolders.Find(f => f.name.Equals(name));

			if (folder != null)
			{
				result.AddRange(folder.audioClips);
				yield break;
			}

			var localizedFolder = localizedAudioClipsFolders.Find(f => f.name.Equals(name));

			if (localizedFolder != null)
			{
				foreach (LocalizedAudioClip clip in localizedFolder.audioClips)
				{
					var op = clip.LoadAssetAsync();

					yield return op;

					if (op.Status == AsyncOperationStatus.Succeeded)
                        result.Add(op.Result);
                }
			}
		}

	}

}
