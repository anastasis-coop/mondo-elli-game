using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Room7
{

	public class DropRectangle : MonoBehaviour
	{
		public int numberOfSlots = 0;
		public bool sameSize = false;
		public RectTransform positionRef;

		[SerializeField]
		private List<RectTransform> objectPositions;

		public UnityEvent onAcceptObject = new UnityEvent();

		private List<string> slots = new List<string>();

		void Update() {
			if (numberOfSlots != slots.Count) {
				slots = new List<string>();
				for (int i = 0; i < numberOfSlots; i++) {
					slots.Add("");
				}
			}
		}

		public bool canAccept(Draggable obj) {
			RectTransform rt_box = GetComponent<RectTransform>();
			RectTransform rt_obj = obj.GetComponent<RectTransform>();
			Vector2 center = (positionRef == null) ? rt_box.anchoredPosition : positionRef.anchoredPosition;
			Vector2 delta = rt_obj.anchoredPosition - center;
			Vector2 extent = rt_box.sizeDelta / 2;
			bool isInside = (Mathf.Abs(delta.x) < extent.x && Mathf.Abs(delta.y) < extent.y);
			if (sameSize) {
				return isInside && (rt_box.sizeDelta == rt_obj.sizeDelta);
			} else {
				return isInside;
			}
		}

		private Vector2 getPositionByIndex(int freeIndex) {
			return objectPositions[freeIndex].position;
			//RectTransform rt_box = GetComponent<RectTransform>();
			//Vector2 center = (positionRef == null) ? rt_box.anchoredPosition : positionRef.anchoredPosition;
			//Vector2 topleft = new Vector2(center.x - rt_box.sizeDelta.x / 2, center.y + rt_box.sizeDelta.y / 2);
			//Vector2 num = new Vector2(slots.Count + 1, 2);
			//if (slots.Count > 3) {
			//	num.x = Mathf.Floor(slots.Count / 2) + 1;
			//	num.y++;
			//}
			//Vector2 step = rt_box.sizeDelta / num;
			//step.y = -step.y;
			//Vector2 start = topleft + step;
			//Vector2 pos = start;
			//for (int i = 0; i < slots.Count; i++) {
			//	if (i == freeIndex) {
			//		return pos;
			//	}
			//	pos.x += step.x;
			//	if (pos.x >= topleft.x + rt_box.sizeDelta.x) {
			//		pos.x = start.x;
			//		pos.y += step.y;
			//	}
			//}
			//return rt_box.anchoredPosition;
		}

		public bool addObject(Draggable obj) {
			RectTransform rt_obj = obj.GetComponent<RectTransform>();
			int freeIndex = slots.FindIndex(slot => slot == "");
			if (freeIndex >= 0) {
				slots[freeIndex] = obj.name;
				rt_obj.position = getPositionByIndex(freeIndex);
				onAcceptObject.Invoke();
				return true;
			}
			return false;
		}

		internal void RestoreObject(Draggable obj, int index) {
			slots[index] = obj.name;
		}

		public int removeObject(Draggable obj) {
			int index = slots.FindIndex(slot => slot == obj.name);
			if (index >= 0) {
				slots[index] = "";
			}
			return index;
		}

		public List<string> getObjects() {
			List<string> result = new List<string>();
			slots.ForEach(slot => {
				if (slot != "")
					result.Add(slot);
			});
			return result;
		}

	}

}