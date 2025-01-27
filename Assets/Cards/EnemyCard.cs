using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  UnityEngine.UI;
public class EnemyCard : MonoBehaviour
{
    // Start is called before the first frame update
    public int HP = 10;
    public int MaxDamageRange = 6;
    public int block=0;
    public string id="";
    public int MinDamageRange = 1;
    public bool Lucky=false;
    public int damage = 0;
    public GameObject DamageText;
    public  GameObject VictoryText;
    public List<Card> cards;
    public TMPro.TextMeshProUGUI text; 

    public TMPro.TextMeshProUGUI DeckSize; 

    public GameObject cardPrefab;
    public GameObject enemyDeck;
    public GameObject sprite;
    public EventCardData postBattleEvent;

    public GameObject combatReward;

    [HideInInspector]
    public CreatureDataContainer creatureDataContainer;

    public bool confused=false;
    public bool stunned = false;
    public float EnemyDamageModifier = 1;
    public Image img;
    int decks;

    [Header("Card UI")]
    [SerializeField] float firstCardScale = 0.4f;
    [SerializeField] float otherCardScale = 0.30f; // 0.25
    [SerializeField] float cardsYoffSet = 0.5f; //-2.25

    [Header("Others")]
    [SerializeField] bool autoUpdate = false;
    Vector3 startpos ;
    void Start()
    {
        RepositionSprite();
        RescaleSprite();
        startpos = sprite.transform.position;

        img =sprite.GetComponent<Image>();
        Debug.Log(img);
        Deck.Instance.battleStart();

    }

    private void RepositionSprite()
    {
        sprite.transform.position = creatureDataContainer.pictureDefaultPosition;
    }

    private void RescaleSprite()
    {
        sprite.transform.localScale = creatureDataContainer.pictureScale;
    }
    //bool dir;
    List<float> t=new List<float>();
    List<bool> dirs = new List<bool>();
    public void reorganize(){

    for(int j=0; j<enemyDeck.transform.childCount; j++){
            t[j] += Time.deltaTime*0.7f;
            float temp = t[j];
            bool dir = dirs[j];
            float y = NonLinInterpolationUtil.QuadraticBounce(ref temp, 1, ref dir);
            t[j] = temp;
            dirs[j] = dir;
            enemyDeck.transform.GetChild(j).GetComponent<Card>().status=Card.BelongTo.Enermy;
        if(j==0){
            enemyDeck.transform.GetChild(j).transform.position = new Vector3(enemyDeck.transform.position.x + cardsYoffSet, y+enemyDeck.transform.position.y);
            enemyDeck.transform.GetChild(j).transform.localScale = new Vector3(firstCardScale, firstCardScale, 1f);
        } else {
            enemyDeck.transform.GetChild(j).transform.position = new Vector3(enemyDeck.transform.position.x + j * 5 + cardsYoffSet*j, y*0.5f+enemyDeck.transform.position.y);
            enemyDeck.transform.GetChild(j).transform.localScale = new Vector3(otherCardScale, otherCardScale, 1f);
            }
    }
    DeckSize.text=""+(decks-enemyDeck.transform.childCount);
}


IEnumerator shake(){
    Vector3 startPos=sprite.transform.position;
    Vector3 startScale=sprite.transform.localScale;
    float speed=20;
    for(int i=0;i<speed;i++){
        //sprite.transform.position=startPos-new Vector3(0,Mathf.Lerp(0,5,(float)((i*1.0)/speed)),0);
        sprite.transform.localScale=startScale-new Vector3(-Mathf.Lerp(0, 0.2f, (float)((i * 1.0) / speed)), Mathf.Lerp(0,0.2f,(float)((i*1.0)/speed)),0);
        yield return null;
    }
    for(int i=0;i<speed;i++){
        //sprite.transform.position=startPos-new Vector3(0,Mathf.Lerp(0,5,1.0f-((float)(i*1.0/speed))),0);
        sprite.transform.localScale=startScale-new Vector3(Mathf.Lerp(0,0.2f,1.0f-((float)(i*1.0/speed))), -Mathf.Lerp(0, 0.2f, 1.0f - ((float)(i * 1.0 / speed))), 0);
        yield return null;
    }
}
    public void takeDamage(int amount) {
        if(amount>0){
            StartCoroutine("shake");
            CameraEffectManager._instance.shake();
            //Instantiate(DamageText).GetComponent<DamageText>().changeTextString(""+(amount*EnemyDamageModifier-block));
            ExplosionManager.Instance.PlaySwordAnimation((int)Mathf.Max(amount*EnemyDamageModifier-block,0),transform.position-new Vector3(5f,0,0));
        }else{
            ExplosionManager.Instance.PlayHealthAnimation((int)(-amount*EnemyDamageModifier),new Vector3(0,0,0));
        }
        if(!Deck.Instance.reversed){
        if(!(confused&&!Deck.Instance.enemyTurn)){
        HP = Mathf.Min((int) (HP +block- amount * EnemyDamageModifier),amount>0?HP:HP-amount);
        }else{
            HP = Mathf.Min((int) (HP +block- amount * Deck.Instance.PlayerDamageModifier),amount>0?HP:HP-amount);
        }

                ExplosionManager.Instance.PlayArmorAnimation((int)-(block - Mathf.Max(0, block - amount * EnemyDamageModifier)), new Vector3(0, 0, 0));
            
            if(amount>0){
            block=Mathf.Max(0,block-amount);}
        }else{
            HP = (int) (HP + amount * EnemyDamageModifier);
        }
        if(HP>maxHP){
            HP=maxHP;
        }
    }
    int maxHP;
    bool lastBoss;

    public void PostCardAddCleanup()
    {
        decks = enemyDeck.transform.childCount;
        for (int i = 0; i < decks; i++)
        {
            t.Add(1 - i * (1f / decks));
            dirs.Add(false);
        }
        reorganize();
    }
    public void Create(CreatureDataContainer data)
    {
        lastBoss = data.lastBoss;
        creatureDataContainer = data;
        // check if the battle card is banned by a previous battle reward
        int affectedBattleCardIndex = (Deck.Instance.enemyAffectedByCombatRewards.ContainsKey(data))?
            Deck.Instance.enemyAffectedByCombatRewards[data] : -1;
        name=data.name;
        HP = data.HP+(int)(Deck.difficulty * data.HP*0.1f);
        maxHP = data.HP + (int)(Deck.difficulty * data.HP * 0.1f);
        MaxDamageRange = data.MaxDamageRange;
        MinDamageRange = data.MinDamageRange;
        postBattleEvent = data.postBattleEvent;
        int j=0;
        if(data.Picture!=null){
            img.sprite=data.Picture;
        }
        List<StatusEffect>  status;
        Debug.Log("getting value from key :"+this.name);
       if(!Deck.Instance.ApplyStatusToEnemy.TryGetValue(this.name,out status)){
            status=new List<StatusEffect>(); 
       }
        
        
           foreach(var stat in status){
                Debug.Log(status);
                stat.add();
                }
        List<BattleCardDataContainer>  CardsToadd;
        Debug.Log("getting value from key :"+this.name);
       if(!Deck.Instance.CardsToAdd.TryGetValue(this.name,out CardsToadd)){
            CardsToadd=new List<BattleCardDataContainer>(); 
       }
        
        
           foreach(var additioncard in CardsToadd){
                Debug.Log(CardsToadd);
                      
            var card=Instantiate(cardPrefab).GetComponent<Intent>();
            card.createCard(additioncard);
            var scale = CardHandler.Instance.cardScaleInEnermyDeck;
            card.transform.localScale=new Vector3(scale, scale, scale);
            cards.Add(card);
            //card.transform.position=new Vector2(enemyDeck.transform.position.x-transform.childCount * 10/2  + j * 10,enemyDeck.transform.position.y);
            card.transform.parent=enemyDeck.transform;
            card.status = Card.BelongTo.Enermy;
            }
        foreach(BattleCardDataContainer i in data.deck){
            if(!( Deck.Instance.cardsToRemoveFromEnemies.Contains(i)) ){
            
            // Skip instantiating if the card is banned
            if(affectedBattleCardIndex == j)
            {
                Deck.Instance.enemyAffectedByCombatRewards.Remove(data);
                j++;
                continue;
            }

            var card=Instantiate(cardPrefab).GetComponent<Intent>();
            card.createCard(i);
            var scale = CardHandler.Instance.cardScaleInEnermyDeck;
            card.transform.localScale=new Vector3(scale, scale, scale);
            cards.Add(card);
            //card.transform.position=new Vector2(enemyDeck.transform.position.x-transform.childCount * 10/2  + j * 10,enemyDeck.transform.position.y);
            card.transform.parent=enemyDeck.transform;
            card.status = Card.BelongTo.Enermy;
            
            j++;
        }
        }
        
        decks=enemyDeck.transform.childCount;
        for (int i = 0; i < decks; i++)
        {
            t.Add(1-i*(1f/decks));
            dirs.Add(false);
        }
        reorganize();
        
        Deck.Instance.Shuffle(cards);
        
    }
    public IEnumerator swapImage(Sprite n)
    {
        var temp = img.sprite;
        img.sprite = n;
        yield return new WaitForSeconds(1);
        img.sprite = temp;
    }
    public IEnumerator Playcard(){
        if (stunned) {
            stunned = false;
            yield break;
        }
        
        if(enemyDeck.transform.childCount==0){
            Deck.Instance.Shuffle(cards);
            int j=0;
            foreach(Card card in cards){
                
                //card.transform.position=new Vector2(enemyDeck.transform.position.x-transform.childCount * 10/2  + j * 10,enemyDeck.transform.position.y);
                card.transform.parent=enemyDeck.transform;
                
                j++;
            }
            reorganize();
        }else{
            var temp=enemyDeck.transform.GetChild(0).GetComponent<Intent>();
            if (temp.SpecialEffect != null)
            {
                StartCoroutine(swapImage(temp.SpecialEffect));
            }
            if (temp.AudioOnPlay != null)
            {
                AudioManager.Instance.playCustomSound(temp.AudioOnPlay);
            }
            enemyDeck.transform.GetChild(0).transform.position=new Vector2(10000,100000);
            enemyDeck.transform.GetChild(0).transform.parent=this.transform;
            yield return temp.EnemyPlayCard(this);
            if(temp.exaust){
                cards.Remove(temp);
                Destroy(temp.gameObject);
            }
            reorganize();

            
        }
    }
    // Update is called once per frame
    float idletime=0;
    float idleMaxtime=0.5f;
    bool idleDir=true;
    public void idleAnim(){
        idletime+=Time.deltaTime;
        float anim=NonLinInterpolationUtil.QuadraticBounce(ref idletime,idleMaxtime,ref idleDir);
        sprite.transform.localScale= new Vector3(creatureDataContainer.pictureScale.x, creatureDataContainer.pictureScale.y - anim*0.05f,creatureDataContainer.pictureScale.z); //creatureDataContainer.pictureScale.y
    }
    public bool VictoryButtonPressed=false;
    public void WinBattle() { Deck.Instance.bossesDefeated += 1; VictoryButtonPressed = true; }
    public void WinGame()
    {
        SceneLoader.LoadGameVictory();
    }
    public IEnumerator fade()
    {

        for (int i = 0; i < 100; i++)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, Mathf.Lerp(0, 1, (100f - i) / 100));
            yield return 1;
        }
    }
    public IEnumerator Death()
    {
        var victory=Instantiate(VictoryText);
        victory.name = "Victory";
        AudioManager.Instance.stopMusic();
        AudioManager.Instance.PlayOneShot(AudioManager.AudioEffects.VictoryBeeb);

        if (!lastBoss)
        {
            GameObject.Find("Victory/Canvas/VictoryButton").GetComponent<Button>().onClick.AddListener(WinBattle);
        }
        else
        {
            GameObject.Find("Victory/Canvas/VictoryButton").GetComponent<Button>().onClick.AddListener(WinGame);
        }
        StartCoroutine(fade());
        while (!VictoryButtonPressed)
        {
            yield return 0;
        }
        Destroy(victory);

        Deck.Instance.enemy = null;
        Deck.Instance.inBattle = false;
        Deck.Instance.ResetDeck();

        if (combatReward != null)
        {
            var combatRewardObj = Instantiate(combatReward);
            combatRewardObj.GetComponent<CombatReward>().OnCreate(creatureDataContainer);

            Deck.Instance.inReward = true;
            Deck.Instance.eventVisible = true;
        }
        else if (postBattleEvent != null)
        {
            Instantiate(Deck.Instance.eventBase).GetComponent<EventCard>().CreateEventCard(postBattleEvent);
            Deck.Instance.eventVisible = true;
        }
        Destroy(gameObject);
        AudioManager.Instance.PlayDrawEventCardBGM();

    }
    bool dead = false;
    void Update()
    {
        reorganize();  //Remove after testing;

        idleAnim();

        //Cange gamestate back to drawing event cards if player wins battle
        //REMOVE
        //text.text = damage + "\n" + HP;
        if (HP <= 0&&Deck.Instance.enemyTurn&&!dead)
        {
            dead = true;
            StartCoroutine(Death());

        }

        if (autoUpdate)
        {
            RepositionSprite();
            RescaleSprite();
        }
    }
    float attackAnimationDuration=1;
    public void moveToPos(Vector3 startpos,Vector3 targetPos,float time,float duration){
        var r=NonLinInterpolationUtil.Interpolate(0,1,time/(duration),NonLinInterpolationUtil.EaseType.Quadratic,NonLinInterpolationUtil.EaseType.Quadratic);
        sprite.transform.position=startpos+r*targetPos;
    }
    IEnumerator attackAnimation(){
        //Anticipation
        
        float time=0;
        while(time<attackAnimationDuration/3){
        time+=Time.deltaTime;
        moveToPos(startpos,new Vector3(-2.5f,0,0),time,attackAnimationDuration/3);
        yield return 0;
        }
        //attack and overshoot
        time=0;
        Vector3 posStage2=sprite.transform.position;
        while(time<attackAnimationDuration/6){
        time+=Time.deltaTime;
        moveToPos(posStage2,new Vector3(20,0,0),time,attackAnimationDuration/6);
        yield return 0;
        }
        //overshoot
        time=0;
        posStage2=sprite.transform.position;
        while(time<attackAnimationDuration/(24)){
        time+=Time.deltaTime;
        moveToPos(posStage2,new Vector3(-3,0,0),time,attackAnimationDuration/(24));
        yield return 0;
        }
        //return
        time=0;
         posStage2=sprite.transform.position;
        while(time<attackAnimationDuration/3){
        time+=Time.deltaTime;
        moveToPos(posStage2,startpos-posStage2,time,attackAnimationDuration/3);
        yield return 0;
        }
        sprite.transform.position=startpos;
    }
    IEnumerator stunnedAnimation()
    {
        //Anticipation
        yield return 0;
    }
}
