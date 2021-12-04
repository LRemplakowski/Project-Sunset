using System.Collections;
using System.Collections.Generic;
using SunsetSystems.Management;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button)), RequireComponent(typeof(TextMeshProUGUI))]

public class DialogueChoiceButton : ExposableMonobehaviour
{
    private DialogueManager manager;
    [ReadOnly]
    public int choiceID;

    // Start is called before the first frame update
    void Start()
    {
        manager = ReferenceManager.GetManager<DialogueManager>();
    }

    public void OnClick()
    {
        Debug.Log("Choice selected: " + GetComponent<TextMeshProUGUI>().text + "\nChoice ID: " + choiceID);
        if (GetComponent<Button>().interactable)
            manager.SelectChoice(choiceID);
    }

    private void OnDestroy()
    {
        Debug.LogWarning("Destroying choice: " + GetComponent<TextMeshProUGUI>().text);
    }
}
