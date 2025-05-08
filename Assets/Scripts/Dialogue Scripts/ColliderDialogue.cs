using UnityEngine;

public class ColliderDialogue : MonoBehaviour
{
    public GameObject DialogueBox;
    public DialogueManager dialogueManager;
    [SerializeField] DialogueTrigger dialogueTrigger;
    public bool playingDialogue;

    bool dialogueTriggered = false;

    void Awake()
    {
         dialogueManager = GameObject.Find("DIALOGUE MANAGER").GetComponent<DialogueManager>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DialogueBox.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dialogueManager.DisplayNextSentence();
        }
        if(dialogueManager.playingDialogue == true)
        {
            playingDialogue = true;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.tag == "Player" && !dialogueTriggered)
        {
            DialogueBox.SetActive(true);
            dialogueTrigger.TriggerDialogue();
            dialogueTriggered = true;
        }
    }
}
