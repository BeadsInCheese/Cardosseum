using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ContentsOfDeck : MonoBehaviour
{

    [SerializeField] bool hideAtStart = true;
    public static ContentsOfDeck Instance { get; private set; }

    /*
     * To use this script just simply call any DisplayCards function.
     *  DisplayCards() // Displays every card, without changing DeckTask and maxSelected.
     *  DisplayCards(DeckTask deckTask, uint maxSelected = 0) // Displays every card, need to set DeckTask.
     *  DisplayCards(List<GameObject> cards, DeckTask deckTask = DeckTask.view, uint maxSelected = 0) // Displays every card that is in card list.
     */

    public enum DeckTask { view, selectStartDeck, removeCardFromDeck, discardPileToHand}

    [SerializeField] DeckTask _currentTask = DeckTask.view;
    [SerializeField] uint _maximumNumberOfSelectedCards = 2;

    [SerializeField] GameObject _playerHand;
    [SerializeField] Deck _playerDeck;

    [Header("Game cards")]
    [SerializeField] bool _updateCardsInGame = true;
    [SerializeField] List<GameObject> _cardsInGame;
    [SerializeField] List<GameObject> _discardPile;
    [SerializeField] List<GameObject> _battleDeck;
    [SerializeField] List<GameObject> _handCards;

    [SerializeField] List<Card> _selectedCards; 

    [Header("Show deck")] // delete later?
    [SerializeField] bool _showDiscardPile = true;
    [SerializeField] bool _showBattleDeck = true;
    [SerializeField] bool _showHandCards = true;

    [Header("Card positions")]
    [SerializeField] Vector2 _gridStartPosition = Vector2.zero;
    [SerializeField, Range(1, 7)] int _numbersOfCardsInRow = 5;
    [SerializeField] float _rowHeight = 10f;
    [SerializeField] float _columnWidth = 10f;

    [Header("Buttons")]
    [SerializeField] GameObject _closeButton;
    [SerializeField] GameObject _actionButton;

    [Header("Text")]
    [SerializeField] TextMeshProUGUI _titleTMP;
    [SerializeField] TextMeshProUGUI _actionButtonTMP;
    [SerializeField] ButtonFlicker _actionButtonFlicker;


    [Header("Others")]
    [SerializeField] GameObject enemyGO;
    [ColorUsageAttribute(true, true)]
    [SerializeField] Color HighlightColor = Color.red;
    [SerializeField] float timer = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }
    Vector3 titleStartPos;
    private void Start()
    {
        gameObject.SetActive(!hideAtStart);
        if (!hideAtStart)
        {
            if (Deck.Instance.isbit1Final)
            {
                DisplayCards(DeckTask.selectStartDeck, 5);
            }
            else
            {
                DisplayCards(DeckTask.selectStartDeck, 7);
            }
            titleStartPos = _titleTMP.transform.position;
        }


    }

    private void Update()
    {
        HandlePlayerInput();
        UpdateUIText();
        HandleActionButton();
        updatePosition();
    }

    private void GetCardsInGame()
    {
        if (!_updateCardsInGame)
            return;

        _cardsInGame = new List<GameObject>(GameObject.FindGameObjectsWithTag("Card"));

        _discardPile = GetListOfCards(_playerDeck.BattleDiscardPile);
        _battleDeck = GetListOfCards(_playerDeck.BattleDeck);
        _handCards = GetHandCards();
    }

    public void DisplayDiscardedDeck()
    {
        _discardPile = GetListOfCards(_playerDeck.BattleDiscardPile);
        Debug.Log($"discard deck pile count: {_playerDeck.BattleDiscardPile.Count} -> {_discardPile.Count}"); //Used this to make sure everything is correct.
        if (_discardPile.Count > 0)
        {
            DisplayCards(_discardPile, DeckTask.view);
            _playerHand.SetActive(false);
        }
    }

    public void DisplayBattleDeck()
    {
        _battleDeck = GetListOfCards(_playerDeck.BattleDeck);
        Debug.Log($"discard deck pile count: {_playerDeck.BattleDeck.Count} -> {_battleDeck.Count}"); //Used this to make sure everything is correct.
        if (_battleDeck.Count > 0)
        {
            DisplayCards(_battleDeck, DeckTask.view);
            _playerHand.SetActive(false);
        }
        /*List<GameObject> finalDeck = new List<GameObject>();
        finalDeck.AddRange(GetListOfCards(_playerDeck.BattleDeck));
        finalDeck.AddRange(GetHandCards());

        if(finalDeck.Count > 0)
        {
            DisplayCards(finalDeck);
        }*/
    }
    [SerializeField]List<GameObject> displayedCards = new List<GameObject>();
    private void DisplayListOfCards(List<GameObject> listOfCards)
    {
        displayedCards = new List<GameObject>();
        float cardZvalue = 0f;

        Vector2 gridStartPosition = _gridStartPosition;

        int totalRows = listOfCards.Count / _numbersOfCardsInRow + 1;

        for (int i = 0; i < totalRows; i++)
        {
            float currentRowPosition = gridStartPosition.y - (i* _rowHeight);

            for (int j = 0; j < _numbersOfCardsInRow; j++)
            {
                int cardIndex = i * _numbersOfCardsInRow + j;

                if (cardIndex >= listOfCards.Count)
                    break;

                GameObject cardGO = listOfCards[cardIndex];
                displayedCards.Add(cardGO);
                Vector3 cardFinalPosition = new Vector3(gridStartPosition.x + (j * _columnWidth), currentRowPosition, cardZvalue);
                cardGO.transform.position = cardFinalPosition;
            }
        }
    }
    Vector2 gridOffset = new Vector2(0,-4);
    public void updatePosition()
    {
        float cardZvalue = 0f;

        Vector2 gridStartPosition = _gridStartPosition+gridOffset;
        int totalRows = displayedCards.Count / _numbersOfCardsInRow + 1;
        _titleTMP.transform.position = (Vector2)titleStartPos + gridOffset;
        for (int i = 0; i < totalRows; i++)
        {
            float currentRowPosition = gridStartPosition.y - (i * _rowHeight);

            for (int j = 0; j < _numbersOfCardsInRow; j++)
            {
                int cardIndex = i * _numbersOfCardsInRow + j;

                if (cardIndex >= displayedCards.Count)
                    break;

                GameObject cardGO = displayedCards[cardIndex];
                Vector3 cardFinalPosition = new Vector3(gridStartPosition.x + (j * _columnWidth), currentRowPosition, cardZvalue);
                cardGO.transform.position = cardFinalPosition;
            }
        }
    } 
    private List<GameObject> GetHandCards()
    {
        List<GameObject> handCards = new List<GameObject>();

        /**
         * Get all gameobject which has Card component and Card.inHand value is True .
         * Another way to get hand cards is to get childrens from _playerHand gameobject.
         * 
         */
        foreach (GameObject cardGO in _cardsInGame)
        {
            if (cardGO == null)
                continue;

            Card card;

            if (cardGO.TryGetComponent<Card>(out card))
            {
                if (card.inHand)
                {
                    handCards.Add(cardGO);
                }
            }
        }

        return handCards;
    }

    private List<GameObject> GetListOfCards(List<Card> compareToList)
    {
        _cardsInGame = new List<GameObject>();
        foreach(Card c in Deck.Instance.BattleDeck) {
            _cardsInGame.Add(c.gameObject);
        }
        foreach (Card c in Deck.Instance.BattleDiscardPile)
        {
            _cardsInGame.Add(c.gameObject);
        }
        foreach (Card c in Deck.Instance.ExaustPile)
        {
            _cardsInGame.Add(c.gameObject);
        }
        List<GameObject> finalList = new List<GameObject>();
        /**
         *  Get all gameobject which has Card component and are in compareToList.
         */
        foreach (GameObject cardGO in _cardsInGame)
        {
            if (cardGO == null)
                continue;

            Card card;
            if (cardGO.TryGetComponent<Card>(out card))
            {
                if (compareToList.Contains(card))
                {
                    finalList.Add(cardGO);
                }
            }
        }
        return finalList;
    }

    private void DisplayEnemyGO(bool disable)
    {
        if(!disable == true) //This is because GameObject.Find does not find disabled gameobjects
            enemyGO = GameObject.Find("CreatureBase(Clone)");

        if (enemyGO != null)
        {
            enemyGO.SetActive(disable);
        }
    }
    private bool mouseHeldDown = false;
    Vector3 prevMousepos;
    private void HandlePlayerInput()
    {
        float totalRowHeight = (displayedCards.Count / _numbersOfCardsInRow + 1) * _rowHeight;
        totalRowHeight = Mathf.Max(0, totalRowHeight - (2.5f * _rowHeight)  );
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            mouseHeldDown = true;
            Vector3 mousePos = Input.mousePosition;
            // Set the z value to 10
            mousePos.z = 10f;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            prevMousepos = worldPos;

        }

        if (mouseHeldDown)
        {
            Vector3 mousePos = Input.mousePosition;
            // Set the z value to 10
            mousePos.z = 10f;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);



            gridOffset.y = Mathf.Clamp(gridOffset.y-(prevMousepos - worldPos).y,-4,totalRowHeight);

            prevMousepos = worldPos;
        }
        gridOffset.y = Mathf.Clamp(gridOffset.y - Input.mouseScrollDelta.y, -4, totalRowHeight);

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            mouseHeldDown = false;
            if (_currentTask != DeckTask.view)
            {
                PointerEventData eventData = new PointerEventData(EventSystem.current);
                eventData.position = Input.mousePosition;

                List<RaycastResult> raysastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, raysastResults);

                if (raysastResults.Count > 0)
                {
                    if (raysastResults[0].gameObject != null)
                    {
                        Card card = raysastResults[0].gameObject.GetComponentInParent<Card>();
                        if (card != null)
                        {
                            AddOrRemoveCardFromSelectedList(card);
                        }
                    }
                }
            }
            else
            {
                FinishDeckTask();
            }
        }

    }

    public void FinishDeckTask()
    {
        gridOffset.y = -4;
        mouseHeldDown = false;
        displayedCards = new List<GameObject>();
        Debug.Log("FinishTask: " + _currentTask);
        switch (_currentTask)
        {
            //Removes every card that is not selected.
            case DeckTask.selectStartDeck:

                if (_selectedCards.Count == _maximumNumberOfSelectedCards)
                {
                    GetCardsInGame();
                    foreach(GameObject cardGO in _cardsInGame)
                    {
                        Card card;

                        if(cardGO.TryGetComponent(out card))
                        {
                            if (!_selectedCards.Contains(card))
                            {
                                _playerDeck.RemoveAndDestroyCard(card);
                            }
                        }
                    }
                    ChangeCurrentTask();
                    gameObject.SetActive(false);
                 }
                break;

            //Remove every card that is selected.
            case DeckTask.removeCardFromDeck:
                if(_selectedCards.Count == _maximumNumberOfSelectedCards)
                {
                    foreach (Card card in _selectedCards)
                    {
                        _playerDeck.RemoveAndDestroyCard(card);
                    }
                    ChangeCurrentTask(_currentTask, _maximumNumberOfSelectedCards);
                    gameObject.SetActive(false);
                }
                break;

            case DeckTask.discardPileToHand:
                if (_selectedCards.Count == _maximumNumberOfSelectedCards)
                {
                    foreach (Card card in _selectedCards)
                    {
                        //Move cards to hand.
                    }
                    ChangeCurrentTask(_currentTask, _maximumNumberOfSelectedCards);
                    gameObject.SetActive(false);
                }

                break;

            default:
                ChangeCurrentTask();
                gameObject.SetActive(false);
                break;
        }
    }
    public void ChangeCurrentTask(DeckTask newTask = DeckTask.view, uint maxSelected = 0)
    {
        _currentTask = newTask;
        RemoveSelectedCardsHighlight();
        _selectedCards.Clear();
        displayedCards = new List<GameObject>();
        _maximumNumberOfSelectedCards = maxSelected;
    }

    private void AddOrRemoveCardFromSelectedList(Card card)
    {
        if (_selectedCards.Contains(card))
        {
            _selectedCards.Remove(card);
            card.SetHighlightAlpha(0f);
        }
        else
        {
            if (_selectedCards.Count < _maximumNumberOfSelectedCards)
            {
                _selectedCards.Add(card);
                card.SetHighlightColor(HighlightColor);
                card.SetHighlightAlpha(1f);
            }
            else
            {
                Debug.Log($"Too many cards selected! ({_selectedCards.Count}/{_maximumNumberOfSelectedCards})");
                //Display error message.
            }
        }
    }

    private void RemoveSelectedCardsHighlight()
    {
        foreach(Card card in _selectedCards)
        {
            card.SetHighlightAlpha(0f);
        }
    }

    public void DisplayCards()
    {
        DisplayCards(_currentTask, _maximumNumberOfSelectedCards);
    }

    public void DisplayCards(DeckTask deckTask, uint maxSelected = 0)
    {
        GetCardsInGame();
        DisplayCards(_cardsInGame, deckTask, maxSelected);
    }

    public void DisplayCards(List<GameObject> cards, DeckTask deckTask = DeckTask.view, uint maxSelected = 0)
    {
        gameObject.SetActive(true);
        _currentTask = deckTask;
        _maximumNumberOfSelectedCards = maxSelected;
        HandleButtonUI();
        DisplayListOfCards(cards);
    }

    private void HandleButtonUI()
    {
        switch (_currentTask)
        {
            case DeckTask.selectStartDeck:
            case DeckTask.removeCardFromDeck:
            case DeckTask.discardPileToHand:
                //_closeButton.SetActive(false);
                _actionButton.SetActive(true);
                break;

            default:
                //_closeButton.SetActive(true);
                _actionButton.SetActive(false);
                break;
        }
    }

    private void HandleActionButton()
    {
        if (_actionButton.activeSelf)
        {
            if(_currentTask != DeckTask.view)
            {
                Button button = _actionButton.GetComponentInChildren<Button>();
                    
                if(button != null)
                {
                    button.interactable = _selectedCards.Count == _maximumNumberOfSelectedCards;
                }
            }
        }
    }

    private void UpdateUIText()
    {
        string buttonText = "";
        switch (_currentTask)
        {
            case DeckTask.selectStartDeck:
                SetTitleText("Create your deck");
                buttonText = _selectedCards.Count != _maximumNumberOfSelectedCards ? $"{_selectedCards.Count} / {_maximumNumberOfSelectedCards}" : "Finish deck";
                _actionButtonFlicker.Flickering = _selectedCards.Count == _maximumNumberOfSelectedCards;
                SetActionButtonText(buttonText);
                break;

            case DeckTask.removeCardFromDeck:
                SetTitleText("Remove cards");
                buttonText = _selectedCards.Count != _maximumNumberOfSelectedCards ? $"{_selectedCards.Count} / {_maximumNumberOfSelectedCards}" : "Remove cards";
                _actionButtonFlicker.Flickering = _selectedCards.Count == _maximumNumberOfSelectedCards;
                SetActionButtonText(buttonText);
                break;

            case DeckTask.discardPileToHand:
                SetTitleText("Select cards");
                buttonText = _selectedCards.Count != _maximumNumberOfSelectedCards ? $"{_selectedCards.Count} / {_maximumNumberOfSelectedCards}" : "Select cards";
                _actionButtonFlicker.Flickering = _selectedCards.Count == _maximumNumberOfSelectedCards;
                SetActionButtonText(buttonText);
                break;

            default:
                SetTitleText();
                _actionButtonFlicker.Flickering = _selectedCards.Count == _maximumNumberOfSelectedCards;
                SetActionButtonText();
                break;
        }
    }

    private void SetTitleText(string newTitle = "")
    {
        if(_titleTMP != null)
            _titleTMP.text = newTitle;
    }
    private void SetActionButtonText(string newText = "")
    {
        if (_actionButtonTMP != null)
            _actionButtonTMP.text = newText;
    }

    private void OnEnable()
    {
        GetCardsInGame();
        _playerHand.GetComponent<HandOrganizer>().enabled = false;
        DisplayEnemyGO(false);
    }

    private void OnDisable()
    {
        if (!_playerHand.activeSelf)
            _playerHand.SetActive(true);

        RemoveSelectedCardsHighlight();
        foreach (GameObject cardGO in _discardPile)
        {
            cardGO.transform.position = new Vector2(1000000, 100000);
        }

        foreach(GameObject cardGO in _battleDeck)
        {
            cardGO.transform.position = new Vector2(1000000, 100000);
        }

        HandOrganizer ho = _playerHand.GetComponent<HandOrganizer>();
        ho.SetWaitTime(timer);
        ho.enabled = true;
        DisplayEnemyGO(true);
    }
}
