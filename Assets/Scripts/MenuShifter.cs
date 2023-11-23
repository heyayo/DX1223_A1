using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuShifter : MonoBehaviour
{
	[SerializeField] private GameObject currentPanel;

	public void OpenPanel(GameObject panel)
	{
		currentPanel.SetActive(false);
		currentPanel = panel;
		currentPanel.SetActive(true);
	}

	public void ClearTextBox(TMP_Text textBox)
	{ textBox.text = ""; }

	public void GoGame()
	{
		SceneManager.LoadScene("GameMenus");
	}
}
