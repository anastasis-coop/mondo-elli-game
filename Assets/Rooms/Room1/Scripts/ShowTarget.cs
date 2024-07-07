using UnityEngine;

namespace Room1
{
	public class ShowTarget : MonoBehaviour
	{

		public ConveyorRoom conveyorRoom;
		public GameObject solid;
		public bool isFirst;

		private Sprite[] sprites;

		// Called from ConveyorRoom prepare
		public void Prepare()
		{
			Debug.Log("Show Target prepare");
			//sprites = conveyorRoom.getSprites();
		}

		public string getCurrentSprite()
		{
			if (GetComponent<SpriteRenderer>().sprite != null)
			{
				return GetComponent<SpriteRenderer>().sprite.name;
			}
			else
			{
				return null;
			}
		}

		public void ChangeTarget(string targetSpriteName)
		{
			foreach (Sprite elem in sprites)
				if (targetSpriteName == elem.name)
				{
					GetComponent<SpriteRenderer>().sprite = elem;
				}
		}

	}
}
