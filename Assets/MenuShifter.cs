using System.Collections;
using TMPro;
using UnityEngine;

public class MenuShifter : MonoBehaviour
{
	[SerializeField] private Transform gamePanel;
	[SerializeField] private Transform accountPanel;
	[SerializeField] private Transform loginPanel;
	[SerializeField] private Transform registerPanel;
	[SerializeField] private Transform leaderboardPanel;
	[SerializeField] private GameObject currentPanel;

	private void Start()
	{
		gamePanel.gameObject.SetActive(false);
		loginPanel.gameObject.SetActive(false);
		registerPanel.gameObject.SetActive(false);
		leaderboardPanel.gameObject.SetActive(false);
		accountPanel.gameObject.SetActive(true);
		currentPanel = accountPanel.gameObject;
	}

	public void OpenPanel(GameObject panel)
	{
		currentPanel.SetActive(false);
		currentPanel = panel;
		currentPanel.SetActive(true);
	}

	public void ClearTextBox(TMP_Text textBox)
	{
		textBox.text = "";
	}

	public void GoGame()
	{
		OpenPanel(gamePanel.gameObject);
	}
}
