using UnityEngine;
using TMPro;
using System.Collections;
public class DialogueScript : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed;
    public GameObject canvas;

    private DialogueScript dialogue;

    [SerializeField] private GameObject DialogueTrigger;
    [SerializeField] private int index;
    [SerializeField] private bool hasPlayed;
    

    void Awake()
    {
        canvas.SetActive(false);
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.hasPlayed = false; 
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(textComponent.text == lines[index])
            {
                NextLine();

                Debug.Log(textComponent.text + " : " + lines[index]);
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index];

                Debug.Log("Stopping Coroutines");
            }
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.tag == "Player" && !hasPlayed)
        {
            dialogue = this;

            if(dialogue != null)
            {
                canvas.SetActive(true);
                textComponent.text = string.Empty;
                this.hasPlayed = true;

                StartDialogue();

                Debug.Log("Player Entered Dialogue");
            }

            else
            {
                Debug.Log("Returning");
                return;
            }
        }
        
        
    }

    void StartDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());

        Debug.Log("Dialoge Started");
    }

    IEnumerator TypeLine()
    {
        foreach(char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if(index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());

            Debug.Log("Next Line");
        }
        else
        {

            canvas.SetActive(false);
            Debug.Log("Hiding Dialogue Box");

            dialogue = null;
        }
    }
}
