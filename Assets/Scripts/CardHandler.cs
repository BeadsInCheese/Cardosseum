using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// general handler for cards' animation and display
public class CardHandler : MonoBehaviour
{
    public static CardHandler Instance { get; private set; }
    private static int UILayer;
    public string CardTag = "Card";
    public Card currentFocusedCard; // Hovered by mouse
    private GameObject currentFocusedCardObj;
    public float zoomCardSize = 1.5f;

    void Awake()
    {
        //singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        UILayer = LayerMask.NameToLayer("UI");
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsPointerOverCard())
            return;

        //DisplayCardsWhenMouseHovered(GetCardsUnderMousePointer(GetEventSystemRaycastResults()));
        DisplayCurrentFocusedCard();
    }

    private bool IsPointerOverCard()
    {
        // if the current mouse position is not on top of any gameObjects
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }

        return IsPointerOverCard(GetEventSystemRaycastResults());
    }

    private bool IsPointerOverCard(List<RaycastResult> raycasts)
    {
        if(raycasts.Count <= 0)
            return false;

        //Now, only check the first element hit by ray, modify here when needed
        RaycastResult topRaycastResult = raycasts[0];
        return IsPointerOverCard(topRaycastResult.gameObject);
    }

    // The Raycast does not hit Canvas (why?), thus backtrack parent hierarchy is necessary
    private bool IsPointerOverCard(GameObject obj)
    {
        // Elements inside Card all belong to UILayer
        if (obj.layer != UILayer)
            return false;

        if(obj.CompareTag(CardTag))
            return true;
        if(FindParentWithTag(obj, CardTag))
            return true;
        return false;
    }

    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        
        return raysastResults;
    }

    // return the GameObject with certain tag in obj's parental hierarchy
    GameObject FindParentWithTag(GameObject obj, string tag)
    {
        // return null if current object has no parent
        if(!obj.transform.parent)
            return null;
        
        GameObject parent = obj.transform.parent.gameObject;
        if(parent.CompareTag(tag))
        {
            return parent;
        }
        return FindParentWithTag(parent, tag);
    }

    List<Card> GetCardsUnderMousePointer(List<RaycastResult> raycasts)
    {
        List<Card> cards = new List<Card>();
        for (int index = 0; index < raycasts.Count; index++)
        {
            GameObject cardObj = FindParentWithTag(raycasts[index].gameObject, CardTag);
            if(cardObj)
            {
                cards.Add(cardObj.GetComponent<Card>());
            }
        }
        return cards;
    }

    void DisplayCardsWhenMouseHovered(List<Card> cards)
    {
        for(int index = 0; index < cards.Count; index ++)
        {
            cards[index].DisplayWhenMouseHovered();
        }
    }

    public void SetCurrentFocusedCard(Card card)
    {
        currentFocusedCard = card;

        //Destroy current gameObject
        Destroy(currentFocusedCardObj);
        currentFocusedCardObj = null;

        //Set a new one
        if(card && card.status == Card.BelongTo.Enermy)
        {
            currentFocusedCardObj = Instantiate(card.gameObject);
            currentFocusedCardObj.name = "tipPanel";
            Destroy(currentFocusedCardObj.gameObject.GetComponent<Card>());
            currentFocusedCardObj.transform.localScale = new Vector3(zoomCardSize, zoomCardSize, zoomCardSize);
        }
    }

    void DisplayCurrentFocusedCard()
    {
        
        if(currentFocusedCard && currentFocusedCardObj)
        {
            var campos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var mousepos = new Vector3(campos.x + 8, campos.y - 8, 0);
            currentFocusedCardObj.transform.position = mousepos;
        }
    }
}