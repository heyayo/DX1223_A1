using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StatisticsPager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PlayerData data;
    [SerializeField] private Button prestigeButton;
    [SerializeField] private TMP_Text prestigeButtonText;
    
    [Header("Statistics")]
    [SerializeField] private TMP_Text name;
    [SerializeField] private TMP_Text currency;
    [SerializeField] private TMP_Text matches;
    [SerializeField] private TMP_Text minutes;
    [SerializeField] private TMP_Text dateCreated;
    [SerializeField] private TMP_Text xp;
    [SerializeField] private TMP_Text level;
    [SerializeField] private TMP_Text prestige;

    [Header("Acheivement Icons")]
    [SerializeField] private Image afk;
    [SerializeField] private Image wave30;
    [SerializeField] private Image allShips;
    [SerializeField] private Image maxLevel;
    [SerializeField] private Image prestiged;
    [SerializeField] private Image bsry;
    [SerializeField] private TMP_Text bsryText;

    private void Start()
    { gameObject.SetActive(false); }

    public void PopulateTexts()
    {
        name.text = "Name: " + data.displayName;
        currency.text = "XRCredits: " + data.xr_credits;
        matches.text = "Matches Played: " + data.playStatistics.stat_matchesPlayed;
        minutes.text = "Time Played: " + data.playStatistics.stat_minutesPlayed.ToString("0.##") + " Minutes";
        dateCreated.text = "Created: " + data.timeCreated.ToString("dd/MM/yyyy");
        xp.text = "XP: " + data.playStatistics.stat_xp;
        level.text = "Level: " + data.playStatistics.stat_level;
        prestige.text = "Prestige: " + data.playStatistics.stat_prestige;

        // Achievements Checking
        data.achievements.achievement_breenseerayyang = true;
        if (data.achievements.achievement_afk)
            afk.color = Color.green;
        else
        {
            data.achievements.achievement_breenseerayyang = false;
            afk.color = Color.black;
        }
        if (data.achievements.achievement_wave30)
            wave30.color = Color.green;
        else
        {
            wave30.color = Color.black;
            data.achievements.achievement_breenseerayyang = false;
        }
        if (data.achievements.achievement_allShips)
            allShips.color = Color.green;
        else
        {
            allShips.color = Color.black;
            data.achievements.achievement_breenseerayyang = false;
        }
        if (data.achievements.achievement_maxLevel)
            maxLevel.color = Color.green;
        else
        {
            maxLevel.color = Color.black;
            data.achievements.achievement_breenseerayyang = false;
        }

        if (data.achievements.achievement_prestiged)
            prestiged.color = Color.green;
        else
        {
            prestiged.color = Color.black;
            data.achievements.achievement_breenseerayyang = false;
        }

        if (data.achievements.achievement_breenseerayyang)
        {
            bsry.color = Color.green;
            bsryText.text = "You got all the Achievements!";
        }
        else bsry.color = Color.black;
    }

    public void PrestigeCharacter()
    {
        if (data.playStatistics.stat_level < 50)
        {
            StartCoroutine(Deny());
            return;
        }

        data.playStatistics.stat_prestige += 1;
        data.playStatistics.stat_level = 0;
        data.playStatistics.stat_xp = 0;

        StartCoroutine(Accept());
        data.achievements.achievement_prestiged = true;
        data.UploadStats();
        PopulateTexts();
    }

    private IEnumerator Deny()
    {
        prestigeButton.enabled = false;
        prestigeButtonText.text = "TOO LOW";
        prestigeButtonText.fontSize = 40;

        yield return new WaitForSeconds(2);

        prestigeButton.enabled = true;
        prestigeButtonText.text = "Prestige";
        prestigeButtonText.fontSize = 50;
    }

    private IEnumerator Accept()
    {
        prestigeButton.enabled = false;
        prestigeButtonText.text = "PRESTIGED!";
        prestigeButtonText.fontSize = 34;

        yield return new WaitForSeconds(2);
        
        prestigeButton.enabled = true;
        prestigeButtonText.text = "Prestige";
        prestigeButtonText.fontSize = 50;
    }
}
