using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

[System.Serializable]
public class DialogLine
{
    public string text;
    public Sprite portrait;
    public string speakerName;
    public AudioClip voiceClip;
}

public class DialogManager : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public GameObject dialogPanel;
    public TMP_Text dialogText;
    public Image portraitImage;
    public TMP_Text speakerNameText;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Dialog Lines")]
    public DialogLine[] lines;

    [Header("Voice Settings")]
    public float micSensitivity = 0.02f;

    private int currentIndex = 0;
    private bool waitingForVoice = false;
    private AudioClip micClip;

    void Start()
    {
        foreach (string device in Microphone.devices)
            Debug.Log("Mic found: " + device);

        ShowCurrentLine();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (waitingForVoice) return;

        AdvanceDialog();
    }

    void AdvanceDialog()
    {
        currentIndex++;

        if (currentIndex >= lines.Length)
        {
            CloseDialog();
            return;
        }

        ShowCurrentLine();

        if (currentIndex == 3 || currentIndex == 6 || currentIndex == 12 || currentIndex == 15 || currentIndex == 19)
        {
            Debug.Log("Reached line 4, starting voice listener");
            StartCoroutine(ListenThenAdvance());
        }
    }

    void ShowCurrentLine()
    {
        DialogLine line = lines[currentIndex];

        dialogText.text = line.text;

        if (line.portrait != null)
        {
            portraitImage.sprite = line.portrait;
            portraitImage.gameObject.SetActive(true);
        }
        else
        {
            portraitImage.gameObject.SetActive(false);
        }

        if (speakerNameText != null)
            speakerNameText.text = line.speakerName;

        dialogPanel.SetActive(true);

        if (line.voiceClip != null)
        {
            audioSource.Stop();
            audioSource.clip = line.voiceClip;
            audioSource.Play();
        }
    }

    IEnumerator ListenThenAdvance()
    {
        waitingForVoice = true;

        // Wait for any voice clip on line 4 to finish before listening
        if (lines[currentIndex].voiceClip != null)
        {
            Debug.Log("Waiting for voice clip to finish...");
            yield return new WaitForSeconds(lines[currentIndex].voiceClip.length);
        }

        micClip = Microphone.Start(null, true, 10, 44100);

        yield return new WaitUntil(() => Microphone.GetPosition(null) > 0);

        Debug.Log("Mic is live, listening for voice...");

        bool heard = false;
        while (!heard)
        {
            yield return new WaitForSeconds(0.1f);
            float volume = GetMicVolume();
            Debug.Log("Mic volume: " + volume);
            if (volume > micSensitivity)
                heard = true;
        }

        Debug.Log("Voice detected, advancing dialog");

        yield return new WaitForSeconds(0.5f);

        Microphone.End(null);
        waitingForVoice = false;

        AdvanceDialog();
    }

    float GetMicVolume()
    {
        int sampleWindow = 128;
        int micPosition = Microphone.GetPosition(null) - sampleWindow;
        if (micPosition < 0) return 0;

        float[] samples = new float[sampleWindow];
        micClip.GetData(samples, micPosition);

        float sum = 0;
        foreach (float s in samples)
            sum += Mathf.Abs(s);

        return sum / sampleWindow;
    }

    void CloseDialog()
    {
        audioSource.Stop();
        dialogPanel.SetActive(false);
        currentIndex = 0;
    }
}