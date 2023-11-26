using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using PlayFab;

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
	{ SceneManager.LoadScene("GameMenus"); }

	public void BackToAccountScreen()
	{
		PlayFabClientAPI.ForgetAllCredentials();
		SceneManager.LoadScene("Menu");
	}

	public void PlayGame()
	{ SceneManager.LoadScene("GameScene"); }
}
