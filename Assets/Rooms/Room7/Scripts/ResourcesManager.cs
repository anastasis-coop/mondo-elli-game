using System;
using System.Collections.Generic;
using UnityEngine;

namespace Room7
{

	[Serializable]
	public class SpritesFolder
	{
		public string name;
		public List<Sprite> sprites;
	}

	public class ResourcesManager : MonoBehaviour
	{

		public List<SpritesFolder> spriteFolders;

		public List<Sprite> GetSpritesByFolderName(string name) {
			SpritesFolder folder = spriteFolders.Find(f => f.name.Equals(name));
			if (folder == null)
				return null;
			return folder.sprites;
		}

		public Sprite GetSpriteByName(string name) {
			int i = name.LastIndexOf("/");
			string folder = name.Substring(0, i);
			string spriteName = name.Substring(i + 1);
			List<Sprite> sprites = GetSpritesByFolderName(folder);
			if (sprites == null)
				return null;
			return sprites.Find(sprite => sprite.name.Equals(spriteName));
		}

	}

}
