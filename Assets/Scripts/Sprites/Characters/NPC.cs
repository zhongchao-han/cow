using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public DialoguePanelController dialoguePanel;
    public Sprite                  npcAvatar;
    public string                  npcName = "村长";

    void OnPlayerInteract()
    {
        dialoguePanel.ShowDialogue(
            npcAvatar,
            npcName,
            "欢迎来到我们的小村庄，快来帮忙打理农场吧！"
        );
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
