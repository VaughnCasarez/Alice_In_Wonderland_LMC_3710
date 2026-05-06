using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[System.Serializable]
public class DialogLine
{
    public string text;
    public Sprite portrait;       // drag in a sprite, or leave null for no portrait
    public string speakerName;    // optional
}

public class DialogManager : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public GameObject dialogPanel;
    public TMP_Text dialogText;
    public Image portraitImage;
    public TMP_Text speakerNameText;  // optional, can leave unassigned

    [Header("Dialog Lines")]
    public DialogLine[] lines;

    private int currentIndex = 0;

    void Start()
    {
        ShowCurrentLine();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        currentIndex++;

        if (currentIndex < lines.Length)
            ShowCurrentLine();
        else
            CloseDialog();
    }

    void ShowCurrentLine()
    {
        if (lines.Length == 0) return;

        dialogPanel.SetActive(true);

        DialogLine line = lines[currentIndex];
        dialogText.text = line.text;

        // Update portrait
        if (line.portrait != null)
        {
            portraitImage.sprite = line.portrait;
            portraitImage.gameObject.SetActive(true);
        }
        else
        {
            portraitImage.gameObject.SetActive(false); // hide if no portrait
        }

        // Update speaker name (if you have that text element)
        if (speakerNameText != null)
            speakerNameText.text = line.speakerName;
    }

    void CloseDialog()
    {
        dialogPanel.SetActive(false);
        currentIndex = 0;
    }

    public void StartDialog(DialogLine[] newLines)
    {
        lines = newLines;
        currentIndex = 0;
        ShowCurrentLine();
    }
}