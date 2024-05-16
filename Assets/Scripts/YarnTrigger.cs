using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueType {
    InternalMonologue,
    Tutorial
}

public class YarnTrigger : MonoBehaviour
{
    public DialogueType dialogueType;
    public string[] dialogueNodes;
    private int nodeIdx = -1;

    void OnTriggerEnter(Collider collision) {
        nodeIdx++;
        if (nodeIdx >= dialogueNodes.Length) return;

        if (dialogueType == DialogueType.InternalMonologue)
        {
            if (!YarnDispatcher.StartInternalMonologue(dialogueNodes[nodeIdx])) nodeIdx--;
        } else
        {
            if (!YarnDispatcher.StartTutorial(dialogueNodes[nodeIdx])) nodeIdx--;
        }
    }
}
