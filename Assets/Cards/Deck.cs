using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public static Deck Instance { get; private set; }
    public List<Card> BattleDeck;
    public List<Card> BattleDiscardPile;
    public List<EventCardData> EventDeck;
    public GameObject CardBasePrefab;
    public List<BattleCardDataContainer> cardPrefabs;
    public List<int> deckList;
    public int money = 0;
    public int mana = 0;
    public int Hp = 0;
    public int block = 0;
    public int MaxactionPoints = 3;
    public int actionPoints = 3;
    public bool inBattle=false;
    public bool eventVisible = false;
    public EnemyCard enemy;
    public GameObject Hand;
    public GameObject eventBase;
    private void Awake()
    {
        //singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }
    public void DrawEventCard()
    {
        Instantiate(eventBase).GetComponent<EventCard>().CreateEventCard(EventDeck[Random.Range(0, EventDeck.Count)]);
        eventVisible = true;

    }

    public void inBattleEndTurn()
    {
        //run at the end of the turn
        if (enemy != null)
        {
            Hp -= Mathf.Max(0,enemy.damage-block);
            enemy.damage = Random.Range(enemy.MinDamageRange, enemy.MaxDamageRange);
            inBattleStartTurn();
            Debug.Log("HP: " + Hp);
        }
        block = 0;
    }
    public void Shuffle<T>(List<T> list) {
        for(int i=0; i < list.Count; i++)
        {
            swap(i, Random.Range(0, list.Count), list);

        }
    
    }
    public void swap<T>(int a,int b ,List<T> list)
    {
        var temp = list[a];
        list[a] = list[b];
        list[b] = temp;
    }
    public void shuffleDiscardPileBackInDeck()
    {


            foreach (var i in BattleDiscardPile)
            {
                BattleDeck.Add(i);
            }
            Shuffle(BattleDeck);
            Debug.Log("Deck shuffled");
            BattleDiscardPile = new List<Card>();

    }
    public Card DrawCard()
    {
        //pops card from deck
        if (BattleDeck.Count == 0)
        {
            shuffleDiscardPileBackInDeck();
        }
            var temp = BattleDeck[0];
            BattleDeck.Remove(temp);
        
        return temp;
        

    }
    public void DrawCardInHand(int amount)
    {
        //draws specified amount of cards to hand
        for(int i=0; i<amount; i++)
        {
            if (BattleDeck.Count > 0||BattleDiscardPile.Count>0)
            {
                DrawCard().gameObject.transform.parent = Hand.transform;
            }

        }

    }
    public void inBattleStartTurn() {
        //run at the start of the turn
        DrawCardInHand(3);
        actionPoints = MaxactionPoints;

    }
    void Start()
    {
        //inits all cards specified by their index
        foreach(var i in deckList)
        {
            var j = Instantiate(CardBasePrefab).GetComponent<Card>();
            j.createCard(cardPrefabs[i]);
            j.transform.position =new Vector2(1000000,100000);
            BattleDeck.Add(j);

        }
    }
    public void ResetDeck()
    {
        //moves all cards back to deck 
        shuffleDiscardPileBackInDeck();
        while (Hand.transform.childCount > 0)
        {
            foreach (Transform i in Hand.transform)
            {
                i.parent = null;
                BattleDeck.Add(i.gameObject.GetComponent<Card>());
                i.position = new Vector2(1000000, 100000);
                continue;
            }
        }
        actionPoints = MaxactionPoints;
    }
    public void battleStart()
    {
        //run when battle starts
       DrawCardInHand(3);
    }
    public void BattleDeckAddCard(int index) {
        BattleDeckAddCardFromCardData(cardPrefabs[index]);
    }
    public void BattleDeckAddCardFromCardData(BattleCardDataContainer cardData) {
        var j = Instantiate(CardBasePrefab).GetComponent<Card>();
        j.createCard(cardData);
        j.transform.position = new Vector2(1000000, 100000);
        BattleDeck.Add(j);

    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
