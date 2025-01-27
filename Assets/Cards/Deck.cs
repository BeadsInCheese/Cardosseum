using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class Deck : MonoBehaviour
{
    public static Deck Instance { get; private set; }
    public List<Card> BattleDeck;
    public List<Card> BattleDiscardPile;
    public List<Card> ExaustPile;
    public List<EventCardData> EventDeck;

    public GameObject CardBasePrefab;
    public List<BattleCardDataContainer> cardPrefabs;
    public List<int> deckList;
    public List<int> Bit1deckList;
    public List<string> flags;
    public int money = 0;
    public int bossesDefeated = 0;
    public int mana = 0;
    public float dotDamageMultiplier=1;
    public float playerDotDamageMultiplier=1;
    internal int bossCounter=0;
    public int MaxHp = 20;
    public int Hp = 0;
    public static int difficulty = 1;
    public bool Lucky=false;
    public int block = 0;
    public int MaxactionPoints = 3;
    public int actionPoints = 3;
    public bool inBattle=false;
    public bool inReward = false;
    public int gainAPOnExhaust=0;
    public bool eventVisible = false;
    public List<EventCardData> finalbossPool;
    public EnemyCard enemy;
    public GameObject Hand;
    public GameObject eventBase;
    public List<EventCardData> BossBattles=new List<EventCardData>();
    public List<EventCardData> BossBattlePool=new List<EventCardData>();
    public ObservableCollection<StatusEffectInstance> statuses = new ObservableCollection<StatusEffectInstance>();
    public bool stunned=false;
    public int CardsDrawnAtStartOfTurn=5;
    public float PlayerDamageModifier = 1;
    public bool reversed=false;
    public List<BattleCardDataContainer> cardsToRemoveFromEnemies=new List<BattleCardDataContainer>();
    public DayManager day;
    [HideInInspector]
    public List<CreatureDataContainer> allEnemies;
    public Dictionary<CreatureDataContainer, int> enemyAffectedByCombatRewards;
   
    public Dictionary<string,List<BattleCardDataContainer>> CardsToAdd=new Dictionary<string,List<BattleCardDataContainer>>();
    public Dictionary<string,List<StatusEffect>> ApplyStatusToEnemy=new Dictionary<string,List<StatusEffect>>();
    
    private void FindAllCreatureContainers()
    {
        allEnemies = new List<CreatureDataContainer>();
        enemyAffectedByCombatRewards = new Dictionary<CreatureDataContainer, int>();

        #if UNITY_EDITOR
            string path = "Assets/Cards/Enemies";
            string[] files = System.IO.Directory.GetFiles(path, "*.asset", System.IO.SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var creature = (CreatureDataContainer)UnityEditor.AssetDatabase.LoadAssetAtPath(file, typeof(CreatureDataContainer));
                allEnemies.Add(creature);
            }
        # else
            // TODO load enemies (only when old combat reward system needed)
        #endif
    }

    public void multiplyStatusLength(float amount){
        foreach(var status in statuses){
            status.multiplyStatusLength(amount);
        }
    }
    // wrap list<Card>.Add() function
    public void BattleDeckAdd(Card card)
    {
        BattleDeck.Add(card);
        card.status = Card.BelongTo.Deck;
    }

    // wrap list<Card>.Remove() function
    public void BattleDeckRemove(Card card)
    {
        BattleDeck.Remove(card);
        card.status = Card.BelongTo.Default;
    }


    public void RemoveAndDestroyCard(Card card)
    {
        BattleDeck.Remove(card);
        BattleDiscardPile.Remove(card);
        ExaustPile.Remove(card);

        Destroy(card.gameObject);
    }
    public void multiplyHandCardCost(float amount)
    {
        foreach (Transform card in Hand.transform)
        {
            Card cardComponent=card.gameObject.GetComponent<Card>();
            cardComponent.multiplyHandCardCost(amount);
        }

    }
        public void multiplyCardCost(float amount)
    {
        foreach (Transform card in Hand.transform)
        {
            var cardComponent=card.gameObject.GetComponent<Card>();
            cardComponent.multiplyHandCardCost(amount);
        }
        foreach (Card card in BattleDeck)
        {
           
            card.multiplyHandCardCost(amount);
        }
        foreach (Card card in BattleDiscardPile)
        {
           
            card.multiplyHandCardCost(amount);
        }

    }
    public void addExtraAttack(int id, int amount)
    {
        foreach (Transform card in Hand.transform)
        {
            var cardComponent = card.gameObject.GetComponent<Card>();
            if (cardComponent.id == id) {
                cardComponent.CardExtraDamage += amount;
            }
        }
        foreach (Card card in BattleDeck)
        {

            if (card.id == id)
            {
                card.CardExtraDamage += amount;
            }
        }
        foreach (Card card in BattleDiscardPile)
        {
            if (card.id == id)
            {
                card.CardExtraDamage += amount;
            }
        }

    }
    public void resetCards(){
        foreach(Card i in BattleDeck){
            i.setCardCostmulti(1);
            i.CardExtraDamage = 0;
            i.cardExtraDamageMod = 1;
        }
        foreach(Card i in ExaustPile){
            i.setCardCostmulti(1);
            i.CardExtraDamage = 0;
            i.cardExtraDamageMod = 1;
        }
        foreach(Card i in BattleDiscardPile){
            i.setCardCostmulti(1);
            i.CardExtraDamage = 0;
            i.cardExtraDamageMod = 1;
        }

    }
    private void Awake()
    {
        //singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        Instance = this;
    }
    public GameObject ShowbossList;
    public GameObject bossImage;
    public List<int> RandomCardPool;
    public List<int> GetRandomCards(int amount)
    {
        List<int> c = new List<int>();
        for (int i = 0; i < amount; i++)
        {
            c.Add(RandomCardPool[(int)Random.Range(0, RandomCardPool.Count-1)]);
        }
        return c;
    }

    void Start()
    {
        int index = 0;
        //inits all cards specified by their index
        List<int> dl;
        if (isbit1Final) {
            dl = Bit1deckList;
        }
        else
        {
            dl = deckList;
            dl.AddRange(GetRandomCards(10));
        }
        foreach (var i in dl)
        {
            var j = Instantiate(CardBasePrefab).GetComponent<Card>();
            j.name += index; index++;
            j.createCard(cardPrefabs[i]);
            j.transform.position = new Vector2(1000000, 100000);
            BattleDeckAdd(j);

        }
        for(int i=0; i<5;i++){
            var temp=BossBattlePool[Random.Range(0, BossBattlePool.Count )];
            BossBattles.Add(temp);
            BossBattlePool.Remove(temp);
            /**
            var imagetemp = Instantiate(bossImage);
            Vector3 s=imagetemp.transform.localScale;
            imagetemp.GetComponent<Image>().sprite=temp.BossImage;
            imagetemp.transform.parent=ShowbossList.transform;
            imagetemp.transform.localScale=s;
        **/
           }

        FindAllCreatureContainers();
        AudioManager.Instance.PlayDrawEventCardBGM();
    }
    private void ResetBosses()
    {
        BossBattles = new List<EventCardData>();
        for (int i = 0; i < 5; i++)
        {
            var temp = BossBattlePool[Random.Range(0, BossBattlePool.Count )];
            BossBattles.Add(temp);
            BossBattlePool.Remove(temp);
        }
    }
    public void exaustCard(int CardIndex){
        if(Hand.transform.childCount>0){
            ExaustPile.Add(Hand.transform.GetChild(CardIndex).GetComponent<Card>());
            Hand.transform.GetChild(CardIndex).position=new Vector2(10000000,100000000);
            Hand.transform.GetChild(CardIndex).GetComponent<Card>().status=Card.BelongTo.DiscardPile;
            Hand.transform.GetChild(CardIndex).transform.parent=null;
        }    
}
    public void exaustRandom(){
        
        exaustCard(Random.Range(0,Hand.transform.childCount));
    }
    internal int bosses=0;
    int dayindex = 0;
    bool finalBattle = false;


    public List<EventCardData> Bit1Final;
    public bool isbit1Final = true; 
    public void DrawEventCardBit1FinalOverride()
    {
        if (Bit1Final.Count > 0)
        {
            Instantiate(eventBase).GetComponent<EventCard>().CreateEventCard(Bit1Final[0]);
            Bit1Final.RemoveAt(0);
        }
        else
        {
            SceneLoader.LoadGameVictory();
        }
    }
        public void DrawEventCard()
    {
        if (!isbit1Final)
        {
            Debug.Log(15 + ">=" + (bossCounter + bosses));
            if (bossCounter + bosses <= 15)
            {
                if (bossCounter <= 10 && (bosses >= 5 | Random.value < 0.50))
                {
                    Instantiate(eventBase).GetComponent<EventCard>().CreateEventCard(EventDeck[Random.Range(0, EventDeck.Count - 1)]);
                    bossCounter += 1;
                }
                else
                {
                    Instantiate(eventBase).GetComponent<EventCard>().CreateEventCard(BossBattles[bosses]);
                    //Destroy(ShowbossList.transform.GetChild(0).gameObject);
                    bosses += 1;
                }
            }
            else
            {

                if (!finalBattle)
                {
                    Instantiate(eventBase).GetComponent<EventCard>().CreateEventCard(finalbossPool[Random.Range(0, finalbossPool.Count)]);
                    finalBattle = true;
                }
                else
                {
                    if (!day.SwitchDay(dayindex))
                    {
                        return;
                    }
                    bosses = 0;
                    dayindex++;
                    bossCounter = 0;
                    ResetBosses();
                }
            }
        }
        else {
            DrawEventCardBit1FinalOverride();
        }
        eventVisible = true;
    }
    public bool PlayerConfused=false;
    public void takeDamage(int amount)
    {
        if(!reversed){
            if(PlayerDamageModifier*amount>=0){
                Hp -= Mathf.Max(0,(int)(PlayerDamageModifier*amount)-block);
                block=Mathf.Max(0,(int)(block-PlayerDamageModifier*amount));
                ExplosionManager.Instance.PlaySwordAnimation(Mathf.Max(0,(int)(PlayerDamageModifier*amount)-block));
            }else{
                Hp-=(int)(PlayerDamageModifier*amount);
                ExplosionManager.Instance.PlayHealthAnimation((int)(PlayerDamageModifier*amount));
            }
        }else{
            if(PlayerDamageModifier*amount>=0){
                Hp += Mathf.Max(0,(int)(PlayerDamageModifier*amount));
                ExplosionManager.Instance.PlayHealthAnimation((int)(PlayerDamageModifier*amount));
            }else{
                Hp+=(int)(PlayerDamageModifier*amount);
                ExplosionManager.Instance.PlaySwordAnimation(Mathf.Max(0,(int)(PlayerDamageModifier*amount)-block));
                block=Mathf.Max(0,(int)(block-PlayerDamageModifier*amount));
            }
        }
        if(Hp<=0){
            SceneLoader.LoadGameOver();
        }
        if (Hp > MaxHp)
        {
            Hp = MaxHp;
        }
    }
    public void EndTurnCoroutineStart()
    {
        StartCoroutine(inBattleEndTurn());
    }
    public IEnumerator inBattleEndTurn()
    {
        enemyTurn=false;
        enemy.block=0;
        PlayerDamageModifier = 1;
        enemy.Lucky=false;
        yield return ApplyStatusEffects(StatusActivation.DOTMOD);
        yield return ApplyStatusEffects(StatusActivation.ENEMYTURNSTART);
        statusCleanup();
        PutHandInDiscardPile();
        //run at the end of the turn
        if (enemy != null)
        {
            if(enemy.cards.Count!=0){
                if (enemy.HP > 0)
                {
                    if (!enemy.stunned )
                    {
                        enemy.StartCoroutine("attackAnimation");
                    }
                    else
                    {
                        ExplosionManager.Instance.PlayStunAnimation();
                    }
                }
                if (enemy.HP > 0)
                {
                    yield return enemy.Playcard();
                }

            }else{
                if(!enemy.confused){
                    takeDamage(enemy.damage);
                }else{
                    enemy.HP-=enemy.damage;
                }
            enemy.damage = Random.Range(enemy.MinDamageRange, enemy.MaxDamageRange);
            
            }
            yield return 1;
            StartCoroutine(inBattleStartTurn());
            //inBattleStartTurn();
            Debug.Log("HP: " + Hp);
            
        }
        block = 0;
        PlayerConfused=false;
        enemy.confused=false;
    }

    public void PutHandInDiscardPile(){
        while(Hand.transform.childCount>0){
            discard(0);
        }
    }

    public void discard(int CardIndex){
        if(Hand.transform.childCount>0){
            BattleDiscardPile.Add(Hand.transform.GetChild(CardIndex).GetComponent<Card>());
            Hand.transform.GetChild(CardIndex).position=new Vector2(10000000,100000000);
            Hand.transform.GetChild(CardIndex).transform.parent=null;
        }
    }

    public void discardRandom(){
        
        discard(Random.Range(0,Hand.transform.childCount));
    }

    #region Replace cards
    public void ReplaceCard(Card card, Card toCard)
    {
        if (card == null || toCard == null)
            return;

        card.createCard(toCard.BattleCardData);
    }
    public void ReplaceCard(Card card, BattleCardDataContainer toCard)
    {
        if (card == null || toCard == null)
            return;

        card.createCard(toCard);
    }

    public void ReplaceCard(List<Card> list, int i, Card ToCard)
    {
        if (list.Count < i)
            return;

        ReplaceCard(list[i], ToCard);
    }

    public void ReplaceCards(Card[] cards, Card toCard)
    {
        foreach (Card card in cards)
        {
            ReplaceCard(card, toCard);
        }
    }

    public void ReplaceCards(List<Card> list, int[] indexs, Card toCard)
    {
        foreach (int i in indexs)
        {
            ReplaceCard(list, i, toCard);
        }
    }

    public void ReplaceAllCards(List<Card> list, Card toCard)
    {
        for (int i = 0; i < list.Count; i++)
        {
            ReplaceCard(list, i, toCard);
        }
    }
    public void ReplaceAllCardsOfType(List<Card> list, BattleCardDataContainer fromCard,BattleCardDataContainer toCard)
    {
        foreach (Card c in list)
        {
            if(c.BattleCardData.name.Equals(fromCard.name)){
                ReplaceCard(c, toCard);
            }

        }
    }
    public void ReplaceCard(List<Card> list, Card card, Card ToCard)
    {
        int index = list.IndexOf(card);

        if(index != -1)
            ReplaceCard(list[index], ToCard);
    }

    public void ReplacedCards(List<Card> list, Card[] cards, Card toCard)
    {
        foreach (Card card in cards)
        {
            ReplaceCard(list, card, toCard);
        }
    }
    #region hand cards

    public void ReplaceHandCard(int childIndex, Card toCard)
    {
        Card card = Hand.transform.GetChild(childIndex).GetComponent<Card>();
        card.createCard(toCard.BattleCardData);
    }

    public void ReplaceHandCards(int[] childIndexs, Card toCard)
    {
        foreach(int i in childIndexs)
        {
            Card card = Hand.transform.GetChild(i).GetComponent<Card>();
            card.createCard(toCard.BattleCardData);
        }
    }

    public void ReplaceAllHandCards(Card toCard)
    {
        foreach(Card card in Hand.GetComponentsInChildren<Card>())
        {
            card.createCard(toCard.BattleCardData);
        }
    }
    #endregion

    #endregion

    public void Shuffle<T>(List<T> list)
    {
        for(int i=0; i < list.Count; i++)
        {
            swap(i, Random.Range(i, list.Count), list);

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
                BattleDeckAdd(i);
            }
            Shuffle(BattleDeck);
            Debug.Log("Deck shuffled");
            BattleDiscardPile = new List<Card>();
    }
    public void putExaustPileBackInDeck(){
            foreach (var i in ExaustPile)
            {
                BattleDeckAdd(i);
            }
            Shuffle(BattleDeck);
            Debug.Log("Deck shuffled");
            ExaustPile = new List<Card>();
    }

    public Card DrawCard()
    {
        //pops card from deck
        if (BattleDeck.Count == 0)
        {
            shuffleDiscardPileBackInDeck();
        }
            var temp = BattleDeck[0];
            BattleDeckRemove(temp);
        
        return temp;
    }

    public void DrawCardInHand(int amount)
    {
        AudioManager.Instance.PlayOneShot(AudioManager.AudioEffects.shuffleDeck);
        var cardDistanceScalar = Hand.GetComponent<HandOrganizer>().cardDistanceScalar;
        var leftoverCardCount = Hand.transform.childCount;

        //draws specified amount of cards to hand
        for(int i=0; i<amount; i++)
        {
            if (BattleDeck.Count > 0||BattleDiscardPile.Count>0)
            {
                var card = DrawCard();
                card.status = Card.BelongTo.PlayerHand;
                var deckPos = Deck.Instance.getPosition();
                card.transform.position = deckPos;
                BattleCardMenuItem.Activate(card.BattleCardData.DrawEventConditionalEffects);
                card.gameObject.transform.parent = Hand.transform;
                
                var startPos = new Vector3(deckPos.x, deckPos.y, transform.position.z);
                var endPos = Hand.GetComponent<HandOrganizer>().CalculateCardPos(leftoverCardCount+amount, leftoverCardCount + i, card.transform.localScale.x);
                card.triggerMove(startPos, endPos); 
            }
        }
    }

    public void MoveCardToHand(GameObject cardGO)
    {
        /*var cardDistanceScalar = Hand.GetComponent<HandOrganizer>().cardDistanceScalar;
        var leftoverCardCount = Hand.transform.childCount;

        if (BattleDeck.Count > 0 || BattleDiscardPile.Count > 0)
        {
            var card = DrawCard();
            card.status = Card.BelongTo.PlayerHand;

            var deckPos = Deck.Instance.getPosition();
            card.transform.position = deckPos;
            card.gameObject.transform.parent = Hand.transform;

            var startPos = new Vector3(deckPos.x, deckPos.y, transform.position.z);
            var endPos = Hand.GetComponent<HandOrganizer>().CalculateCardPos(leftoverCardCount + amount, leftoverCardCount + i, card.transform.localScale.x);
            card.triggerMove(startPos, endPos);
        }*/
    }

internal bool enemyTurn=true;

public enum StatusActivation{PLAYERTURNSTART,ENEMYTURNSTART,DOTMOD}
public IEnumerator ApplyStatusEffects(StatusActivation StA){
            
            for( int i=0; i<statuses.Count; i++){
            var status = statuses[i];
            if(StA==StatusActivation.DOTMOD){
            
            if(status.ISDOTMOD){
                    

                    status.triggered = true;
                    yield return new WaitForSeconds(0.5f);

                    status.triggered = false;
                    status.trigger();
                    //yield return new WaitForSeconds(0.3f);


                }
            }
            if(StA==StatusActivation.ENEMYTURNSTART){
            
            if(status.targetsEnemy&&!status.ISDOTMOD){
                    

                        status.triggered = true;
                    yield return new WaitForSeconds(0.5f);

                    status.triggered = false;

                    status.trigger();
                    yield return new WaitForSeconds(0.3f);
                }


            }
            if(StA==StatusActivation.PLAYERTURNSTART&&!status.ISDOTMOD){
            if (!status.targetsEnemy)
            {
                    

                        status.triggered = true;
                    yield return new WaitForSeconds(0.5f);
                    status.triggered = false;
                    status.trigger();
                    //yield return new WaitForSeconds(0.3f);
                }


        }}
}
public void statusCleanup(){
            for( int i=0; i<statuses.Count; i++){
            var status = statuses[i];
            if(!(status.duration > 0))
            {
                statuses.Remove(status);
                i--;
            }}
}

    public IEnumerator inBattleStartTurn()
    {

        

        reversed=false;
        gainAPOnExhaust=0;
        dotDamageMultiplier=1;



        if (enemy != null)
        {

            enemy.EnemyDamageModifier = 1;
        }else{yield break;}

        Lucky=false;
        //run at the start of the turn

        actionPoints = MaxactionPoints;
        yield return StartCoroutine(ApplyStatusEffects(StatusActivation.PLAYERTURNSTART));
        enemyTurn = true;
        if (Deck.Instance.enemy.HP <= 0) { yield break; }
        if (!stunned)
        {
            DrawCardInHand(CardsDrawnAtStartOfTurn);
        }
        playerDotDamageMultiplier = 1;
        if (stunned)
        {
            ExplosionManager.Instance.PlayStunAnimation();
            yield return new WaitForSeconds(1);
            
            stunned = false;
            StartCoroutine(inBattleEndTurn());
        }

    }

    public void ResetDeck()
    {
        while (Hand.transform.childCount > 0)
        {
            foreach (Transform i in Hand.transform)
            {
                i.parent = null;
                BattleDeckAdd(i.gameObject.GetComponent<Card>());
                i.position = new Vector2(1000000, 100000);
                continue;
            }
        }
        resetCards();
        removeAllStatuses();
        putExaustPileBackInDeck();
        Lucky=false;
        dotDamageMultiplier=1;
        playerDotDamageMultiplier=1;
        PlayerDamageModifier=1;
        PlayerConfused=false;
        reversed=false;
        stunned=false;
        gainAPOnExhaust=0;

        Debug.Log("Reset Deck");
        //moves all cards back to deck 
        shuffleDiscardPileBackInDeck();

        actionPoints = MaxactionPoints;
    }
    public void removeAllStatuses()
    {
        while (statuses.Count > 0)
        {
            statuses.RemoveAt(0);
        }
    }

    public void UpdateEveryCardDescription()
    {
        //Seaches for all game objects tagged with "Card"
        GameObject[] cardsInGame = GameObject.FindGameObjectsWithTag("Card");
        foreach(GameObject cardGO in cardsInGame)
        {
            Card card;

            //Checks if game object has a component called Card and if so, sets card value to it and returns true
            if(cardGO.TryGetComponent<Card>(out card))
            {
                if(enemy!=null){
                    card.UpdateDescriptionText(enemy.EnemyDamageModifier);
                }            }
        }
    }

    public void battleStart()
    {
       //run when battle starts
       Shuffle(BattleDeck);
       AudioManager.Instance.playBattleBGM();
       DrawCardInHand(CardsDrawnAtStartOfTurn);
    }
    

    public void BattleDeckAddCard(int index) {
        BattleDeckAddCardFromCardData(cardPrefabs[index]);
    }

    public void BattleDeckAddCardFromCardData(BattleCardDataContainer cardData) {
        var j = Instantiate(CardBasePrefab).GetComponent<Card>();
        j.createCard(cardData);
        j.transform.position = new Vector2(1000000, 100000);
        BattleDeckAdd(j);
    }
    public void EnemyDeckAddCardFromCardData(BattleCardDataContainer cardData)
    {
        var card = Instantiate(enemy.cardPrefab).GetComponent<Intent>();
        card.createCard(cardData);
        var scale = CardHandler.Instance.cardScaleInEnermyDeck;
        card.transform.localScale = new Vector3(scale, scale, scale);
        enemy.cards.Add(card);
        card.transform.parent = enemy.enemyDeck.transform;
        card.status = Card.BelongTo.Enermy;
        enemy.PostCardAddCleanup();

    }

    // Return the deck position under world coordination
    public Vector3 getPosition()
    {
        var camPos = Camera.main.ScreenToWorldPoint(this.transform.position);
        return new Vector3(camPos.x, camPos.y, 0);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GainMaxHpByMama()
    {
        Hp = MaxHp;
        mana = 0;
    }

    public bool HasCardInHand(BattleCardDataContainer battleCardDataContainer)
    {
        Card[] cardsInHand = Hand.GetComponentsInChildren<Card>();

        foreach(Card card in cardsInHand)
        {
            if (card.BattleCardData == battleCardDataContainer)
                return true;
        }
        return false;
    }

    public bool HasCardsInHand(List<BattleCardDataContainer> list)
    {
        foreach (BattleCardDataContainer b in list)
        {
            if (!HasCardInHand(b))
                return false;
        }
        return true;
    }

    public bool HasCardInDeck(BattleCardDataContainer battleCardDataContainer)
    {
        GameObject[] cardsGO = GameObject.FindGameObjectsWithTag("Card");
        
        foreach(GameObject go in cardsGO)
        {
            Card card;

            if(go.TryGetComponent<Card>(out card))
            {
                if (card.BattleCardData == battleCardDataContainer)
                    return true;
            }
        }
        return false;
    }


    public bool HasCardsInDeck(List<BattleCardDataContainer> list)
    {
        foreach (BattleCardDataContainer b in list)
        {
            if (!HasCardInDeck(b))
                return false;
        }
        return true;
    }
}
