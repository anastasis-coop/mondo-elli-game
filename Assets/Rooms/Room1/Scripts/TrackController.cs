using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Room1
{
	public class TrackController : MonoBehaviour
	{
		public ConveyorRoom conveyorRoom;
		public float speed = 0.1f;
		public float radius = 0.37f;
		public float newSpeed;
		public GameObject gear1;
		public GameObject gear2;
		public GameObject module;
		public List<Material> material;

		private GameObject pieces;
		private List<GameObject> items;
		private List<GameObject> gears;

		public bool stop = false;
		public float startSpeed;

		private float step;
		private int horizSteps;
		private float halfCircle;
		private float distance; // distance between gears 
		private float checkPoint1;
		private float checkPoint2;
		private float checkPoint3;
		private float startOffset; // current offset
		private float totalLength; // total length of the conveyor

		private void Start() 
		{
			speed *= conveyorRoom.trackSpeedMultiplier;
			halfCircle = radius * Mathf.PI;
			step = halfCircle / 12;
			horizSteps = Mathf.RoundToInt((gear2.transform.localPosition.x - gear1.transform.localPosition.x) / step);
			if (horizSteps % 2 != 0)
			{
				horizSteps++;
			}
			distance = horizSteps * step;
			checkPoint1 = halfCircle;
			checkPoint2 = checkPoint1 + distance;
			checkPoint3 = checkPoint2 + halfCircle;
			startOffset = 0;
			totalLength = 2 * (distance + halfCircle);
			startSpeed = speed;
			CreateGears();
			AddCollider();
			CreatePiecesParent();
			CreateAllPieces();
			UpdatePieces();
		}

		private void CreateGears() {
			Vector3 gearPos = gear1.transform.localPosition;
			gears = new List<GameObject>();
			gears.Add(gear1);
			int extraGears = Mathf.RoundToInt(distance / 2);
			for (int i = 0; i < extraGears; i++) {
				GameObject newGear = Instantiate(gear1);
				newGear.transform.parent = transform;
				newGear.name = "extragear" + (i + 1);
				Vector3 pos = gearPos;
				pos.x += (i + 1) * distance / (extraGears + 1);
				newGear.transform.localPosition = pos;
				newGear.transform.localEulerAngles = Vector3.zero;
				gears.Add(newGear);
			}
			gearPos.x += distance;
			gear2.transform.localPosition = gearPos;
			gears.Add(gear2);
		}

		public void StartFlickering(Color color)
		{
			foreach (GameObject gear in this.gears)
			{
				gear.GetComponent<FlickeringLight>().startFlickering(color);
			}
		}

		public void StopFlickering()
		{
			foreach (GameObject gear in this.gears)
			{
				gear.GetComponent<FlickeringLight>().stopFlickering();
			}
		}

		private void CreateAllPieces() {
			items = new List<GameObject>();
			int numPieces = (12 + horizSteps) * 2;
			for (int i = 0; i < numPieces; i++) {
				GameObject piece = Instantiate(module);
				piece.transform.parent = pieces.transform;
				piece.GetComponent<MeshRenderer>().material = material[items.Count % 4];
				items.Add(piece);
				piece.name = "piece" + items.Count;
			}
		}

		private void CreatePiecesParent() {
			pieces = new GameObject();
			pieces.name = "pieces";
			pieces.transform.parent = transform;
			pieces.transform.position = transform.position;
			pieces.transform.Translate(new Vector3(0, 0, -2));
			pieces.transform.eulerAngles = transform.eulerAngles;
		}
		private void AddCollider() {
			BoxCollider collider = gameObject.AddComponent<BoxCollider>();
			collider.isTrigger = true;
			collider.center = new Vector3(0, 0, -2f);
			collider.size = new Vector3(distance + radius * 2 + 2, 2f, 8f);
		}

		private float advanceOffset(float offset, float delta) {
			offset += delta;
			if (offset < 0) {
				offset += totalLength;
			} else if (offset >= totalLength) {
				offset -= totalLength;
			}
			return offset;
		}
		private void UpdatePieces() {
			float current = startOffset;
			UpdatePiece(items[0], current);
			foreach (GameObject item in items) {
				UpdatePiece(item, current);
				current = advanceOffset(current, step);
			}
		}
		private void UpdatePiece(GameObject piece, float pieceOffset) {
			Vector3 pos = Vector3.zero;
			Vector3 euler = Vector3.zero;
			if (pieceOffset < checkPoint1) {
				float angle = pieceOffset / radius;
				pos.x = gear1.transform.localPosition.x - Mathf.Sin(angle) * radius;
				pos.y = gear1.transform.localPosition.y + Mathf.Cos(angle) * radius;
				euler.z = angle * Mathf.Rad2Deg;
			} else if (pieceOffset < checkPoint2) {
				pos.x = gear1.transform.localPosition.x + pieceOffset - checkPoint1;
				pos.y = gear1.transform.localPosition.y - radius;
				euler.z = 180;
			} else if (pieceOffset < checkPoint3) {
				float angle = (pieceOffset - checkPoint2) / radius + Mathf.PI;
				pos.x = gear2.transform.localPosition.x - Mathf.Sin(angle) * radius;
				pos.y = gear2.transform.localPosition.y + Mathf.Cos(angle) * radius;
				euler.z = angle * Mathf.Rad2Deg;
			} else {
				pos.x = gear2.transform.localPosition.x - (pieceOffset - checkPoint3);
				pos.y = gear2.transform.localPosition.y + radius;
			}
			piece.transform.localPosition = pos;
			piece.transform.localEulerAngles = euler;
		}
		private void Update()
		{
			float delta = stop ? 0 : -speed * Time.deltaTime;
			startOffset = advanceOffset(startOffset, delta);
			delta = delta * Mathf.Rad2Deg / radius;
			foreach (GameObject gear in gears)
			{
				gear.transform.Rotate(new Vector3(0, 0, delta));
			}
			UpdatePieces();
		}
		private void OnTriggerStay(Collider other) {
			other.transform.Translate(other.transform.InverseTransformDirection(gameObject.transform.right) * (stop ? 0 : speed) * Time.deltaTime);
		}

	}
}
