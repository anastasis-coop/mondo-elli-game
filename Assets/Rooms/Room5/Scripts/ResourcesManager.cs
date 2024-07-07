using System;
using System.Collections.Generic;
using UnityEngine;

namespace Room5
{

	[Serializable]
	public class PrefabsFolder
	{
		public string name;
		public List<GameObject> prefabs;
	}

	public class ResourcesManager : MonoBehaviour
	{

		public List<PrefabsFolder> prefabFolders;

		public List<GameObject> GetPrefabsByFolderName(string name) {
			PrefabsFolder folder = prefabFolders.Find(f => f.name.Equals(name));
			if (folder == null)
				return null;
			return folder.prefabs;
		}

	}

}
