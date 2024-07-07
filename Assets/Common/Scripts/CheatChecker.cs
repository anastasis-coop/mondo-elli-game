using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatChecker : MonoBehaviour {
	// Cheat string to go to room menu
	public string cheat;

	private int cheatIndex = 0;

	private void CheckCheat() {
		if ((cheat != "") && Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), cheat.ToUpper().Substring(cheatIndex, 1)))) {
			cheatIndex++;
			if (cheatIndex == cheat.Length) {
				ActivateCheat();
				cheatIndex = 0;
			}
		}
		else if (Input.anyKeyDown) {
			cheatIndex = 0;
		}
	}

	public void ActivateCheat() {
		// MOdalità test
		GameState.Instance.testMode = true;
		// Disabilita il salvataggio 
		GameState.Instance.levelBackend.isActive = false;
		SceneManager.LoadScene("RoomStart");
	}

	// Update is called once per frame
	void Update() {
		CheckCheat();
	}
}
