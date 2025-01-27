using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Start is called before the first frame update
    public int Damage = 0;
    public int CardExtraDamage=0;
    public float cardExtraDamageMod = 1;

    public int block = 0;
    public int magic = 0;
    public int money = 0;
    public int actionCost = 0;
    public int id = 0;

    public float actionCostMultiplier=1;
    public bool exaust=false;
    public TMPro.TextMeshProUGUI description;
    public TMPro.TextMeshProUGUI apCost;
    public TMPro.TextMeshProUGUI ManaCost;
    public TMPro.TextMeshProUGUI CardName;
    public UnityEvent effect;
    public Sprite CardSprite;
    public Image CardImage;
    public Sprite APSprite;
    public Sprite ManaSprite;
    public Image APImage;
    public Image ManaImage;
    public BattleCardDataContainer BattleCardData { get; protected set; }
    BattleCardDataContainer currentBattleCardData;
    protected List<BattleCardMenuItem> conditionalEffects;

    public GameObject CardHighlightLine;
    public Color DefaultHighlightLineColor;
    public Color PlayableHighlightLineColor;
    public Color PlayHighlightLineColor;
    public Color ActiveHighlightLineColor;
    public Image CardHighlightLineImage;
    public Image InnerCardHighlightLineImage;
    public CanvasGroup cg;

    public AudioClip AudioOnPlay;
    enum mode  {DEFAULT,PLAY,ACTIVE,PLAYABLE}
    mode outlineMode= mode.DEFAULT;
    Vector3 Defaultsize = new Vector3(1, 1, 1);
    Vector3 Bigsize = new Vector3(1f, 1f, 1f);

    public int getDamage()
    {
        return (int)((Damage + CardExtraDamage) * cardExtraDamageMod);

    }
    public void outlineColor()
    {
        //CardHighlightLineImage.transform.localScale = Bigsize;
        switch (outlineMode) {
            
            case mode.PLAY:
                CardHighlightLineImage.color = PlayHighlightLineColor;
                cg.alpha = PlayHighlightLineColor.a;
                
                InnerCardHighlightLineImage.color = PlayHighlightLineColor;
                break;
            case mode.ACTIVE:
                CardHighlightLineImage.color = ActiveHighlightLineColor;
                cg.alpha = ActiveHighlightLineColor.a;
                InnerCardHighlightLineImage.color = ActiveHighlightLineColor;
                break;
            case mode.PLAYABLE:
                CardHighlightLineImage.color = PlayableHighlightLineColor;
                cg.alpha = PlayableHighlightLineColor.a;
                InnerCardHighlightLineImage.color = PlayableHighlightLineColor;
                break;
            case mode.DEFAULT:
                CardHighlightLineImage.color = DefaultHighlightLineColor;
                cg.alpha = DefaultHighlightLineColor.a;
                InnerCardHighlightLineImage.color = DefaultHighlightLineColor;
                //CardHighlightLineImage.transform.localScale = Defaultsize;
                break;
        }
    }
    public float tiltFactor = 300f;
    private float tiltSpeed = 0.3f;
    float tiltTime = 0f;
    public float tiltDept = 25;
    Quaternion tiltTarget=Quaternion.Euler(0,0,0);
    Quaternion Startrot = Quaternion.Euler(0, 0, 0);
    public void updateTilt()
    {
        if (status == BelongTo.PlayerHand)
        {
            Vector3 deltaPos = prevPos - transform.position;
            float tiltX = Mathf.Max(-tiltDept,Mathf.Min(tiltDept, -deltaPos.y * tiltFactor));
            float tiltZ = Mathf.Max(-tiltDept*2f, Mathf.Min(tiltDept*2f, deltaPos.x * tiltFactor*2f));
            //float tiltY = Mathf.Max(-tiltDept, Mathf.Min(tiltDept, deltaPos.x * tiltFactor ));

            // create a quaternion from the tilt angles
            Quaternion tilt = Quaternion.Euler(tiltX, tiltZ, 0);
            // assign the quaternion to the card's local rotation
            tiltTime += Time.deltaTime;
            if (tilt != tiltTarget)
            {
                tiltTarget = tilt;
                tiltTime = 0;
            }
            transform.localRotation = Quaternion.Lerp(transform.localRotation, tiltTarget, tiltTime/tiltSpeed);
            if (tiltTime >= tiltSpeed)
            {
                tiltTime = 0;
            }
            
            //transform.localRotation = Quaternion.Euler(deltaPos.y*80, deltaPos.x*10, 0);
        }
        else
        {
            transform.localRotation = Startrot;
        }
        prevPos = transform.position;
        }
    public bool canPlay()
    {
        if (actionCost * actionCostMultiplier <= Deck.Instance.actionPoints && -magic <= Deck.Instance.mana) { return true; } else
        {
            return false;
        }

    }
    public bool conditionsFulfilled() { 
        foreach(BattleCardMenuItem i in conditionalEffects) {
            bool exec = true;
            foreach(Effect j in i.ConditionalEffects) {
                if (j.possibility<0.99&& !Deck.Instance.Lucky)
                {
                    return false;
                }
            }
            foreach (BattleCondition condition in i.conditions)
            {

                
                if (!condition.Evaluate()) {
                    exec = false;
                }
            }
            if (exec)
            {
                return true;
            }
        }
        return false;
    
    }
    public void updateHandHighlightColor()
    {
        if (status==BelongTo.PlayerHand)
        {
            if (canPlay())
            {
                outlineMode = mode.PLAYABLE;
                //if (conditionsFulfilled()) {
                //    outlineMode = mode.ACTIVE;
                //}
                if (transform.position.y > HandOrganizer.playHeight)
                {
                    outlineMode = mode.PLAY;
                    

                }
                return;

            }
        

        }
            outlineMode = mode.DEFAULT; 
    } 
    public void Playcard()
    {
        if(exaust){
            Deck.Instance.actionPoints+=Deck.Instance.gainAPOnExhaust;
            ExplosionManager.Instance.PlayExhaustAnimation(transform.position);
        }
        if (Deck.Instance.enemy != null)
        {
            if(!Deck.Instance.PlayerConfused){
                Deck.Instance.enemy.takeDamage(getDamage());
            }else{
                Deck.Instance.takeDamage(getDamage());
            }
        }

        effect.Invoke();

        foreach(BattleCardMenuItem i in conditionalEffects){
            bool exec=true;
            
            foreach(var j in i.conditions){
                if(!j.Evaluate()){
                    exec=false;
                }
            }
            if(exec){
                BattleCardMenuItem.Activate(i.ConditionalEffects,i.independentRNG);
            }
            
        }
        Deck.Instance.block += block;
        Deck.Instance.mana += magic;
        Deck.Instance.money -= money;
        ExplosionManager.Instance.playCard(this);
        AudioManager.Instance.PlayCardEffectsWhenCardPlayed(this);
    }
    public void multiplyHandCardCost(float amount){
        actionCostMultiplier*=amount;
        apCost.text=actionCost+"";
    }
    public void setCardCostmulti(float amount){
        actionCostMultiplier=amount;
        apCost.text=actionCost+"";
    }
    public IEnumerator EnemyPlayCard(EnemyCard e){
        if (!e.confused)
        {
            Deck.Instance.takeDamage(Damage);
        }
        else
        {
            e.takeDamage(Damage);
        }
        effect.Invoke();
        if (Damage != 0|| effect.GetPersistentEventCount() > 0)
        {
            yield return new WaitForSeconds(1);
        }
        e.block+=block;
        if (block > 0)
        {
            ExplosionManager.Instance.PlayArmorAnimation(e.block, new Vector3(0, 0, 0));
            yield return new WaitForSeconds(1);
        }


        foreach(BattleCardMenuItem i in conditionalEffects){
            bool exec=true;
            
            foreach(var j in i.conditions){
                if(!j.Evaluate()){
                    exec=false;
                }
            }
            if(exec){
                BattleCardMenuItem.Activate(i.ConditionalEffects,i.independentRNG);
                //yield return new WaitForSeconds(1);
            }
            
        }
        // used cards from enermy go to discard pile?
        this.status = BelongTo.DiscardPile;
   

    }
    public virtual void createCard(BattleCardDataContainer data, bool saveContainerData = true) {

        currentBattleCardData = data;
        
        if (saveContainerData)
            BattleCardData = data;
        CardSprite = data.CardImage;
        AudioOnPlay = data.AudioOnPlay;
        CardImage.sprite = CardSprite;
        Damage = data.Damage;
        block = data.block;
        magic = data.magic;
        money = data.money;
        exaust=data.exaust;
        actionCost = data.actionCost;
        effect = data.effect;
        apCost.text=actionCost+"";
        ManaCost.text="";
        CardName.text=data.cardName;
        id = data.cardName.GetHashCode();
        conditionalEffects=data.conditionalEffects;
        var temp=APImage.color;
        
        temp.a=1f;
        APImage.color=temp;
        APImage.sprite=APSprite;

        if(magic<0){
        temp=ManaImage.color;
        temp.a=1f;
        ManaImage.color=temp;
        ManaImage.sprite=ManaSprite;
        ManaCost.text+=-magic;
        }
        description.text =(data.Damage>0? "Deal " +data.Damage+" Damage":"")+(data.block>0? " Block "+data.block+" Damage":"")+(data.magic>0? " Gain "+data.magic+" Mana":"")+data.effectDescriptor;
    }

    private bool movable = false;
    //private Vector3 moveStartPosition; 
    private Vector3 moveTargetPosition;
    private float moveStartTime = 0.0f;
    private float moveDuration = 5.0f; //5.0 seconds
    public bool inHand = false;
    
    // The Card GameObject is a Child of Deck/Hand/Enermy ...
    public enum BelongTo {Default, Deck, PlayerHand, Enermy, DiscardPile} 
    public BelongTo status = BelongTo.Default;

    // Called by HandOrganizer to activate the card movement
    public void triggerMove(Vector3 startPos, Vector3 endPos)
    {
        // TODO: Consider if the card is the child of Deck, should firstly unbound
        // to make sure the transform.position is in the same hierarchy with startPos & endPos        

        //moveStartPosition = startPos;
        moveTargetPosition = endPos;
        moveStartTime = Time.time;
        transform.position = startPos;

        movable = true;
    }

    // Called when the card arrive its destination so that possible post-tasks can be done here
    public void terminateMove()
    {
        movable = false;
        inHand = true;

        // possible TODO: set the card as child of target
    }

    // Called every frame from Update(), so that the cards can slide to hand 
    public void moveToHand()
    {
        if(movable == false){return;}
        if(transform.position == moveTargetPosition){terminateMove(); return;}
        
        // the card is movable, and it has not reached the destination.
        transform.position = Vector3.Lerp(transform.position, moveTargetPosition, (Time.time - moveStartTime)/moveDuration);
    }

    public void moveToTargetPos(Vector3 targetPos)
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, (Time.time - moveStartTime)/moveDuration);
    }

    void Start()
    {
        SetHighlightAlpha(0f);
        SetHighlightColor(DefaultHighlightLineColor);
        Material m = new Material(CardHighlightLineImage.material);
        CardHighlightLineImage.material = m;
        CardHighlightLineImage.material.SetFloat("_ID",Random.Range(0,2000));
    }

    // Update is called once per frame
    private bool reset=false;
    Vector3 prevPos=new Vector3(0,0,0);
    void Update()
    {
        updateState();
        updateTilt();
        prevPos = transform.position;
        if (this.transform.parent != null) {
            moveToHand();
            updateHandHighlightColor();
            outlineColor();
            reset = false;
        } else{
            inHand=false;
            if(reset==false){
            reset=true;
            resetScale();
            }
        }
        apCost.text=""+actionCost*actionCostMultiplier;
        CheckForSpecialEffect();
    }

    private void CheckForSpecialEffect()
    {
        if (BattleCardData.hasSpecialEffect)
        {
            BattleCardDataContainer specialEffectDataContainer = BattleCardData.specialEffectEvent;

            if (BattleCardData.cardsMustBeInHand)
            {
                if (Deck.Instance.HasCardsInHand(BattleCardData.requiredCards))
                {

                    Debug.Log($"1- {BattleCardData.cardName} have special effect! cardsMustBeInHand: {BattleCardData.cardsMustBeInHand}.");
                    if(specialEffectDataContainer != null && currentBattleCardData != specialEffectDataContainer)
                    {
                        createCard(BattleCardData.specialEffectEvent, false);
                    }
                }
                else
                {
                    if(currentBattleCardData != BattleCardData)
                    {
                        createCard(BattleCardData, false);
                    }
                }
            }
            else
            {
                if (Deck.Instance.HasCardsInDeck(BattleCardData.requiredCards))
                {
                    Debug.Log($"2- {BattleCardData.cardName} have special effect! cardsMustBeInHand: {BattleCardData.cardsMustBeInHand}");
                    if (specialEffectDataContainer != null && currentBattleCardData != specialEffectDataContainer)
                    {
                        createCard(BattleCardData.specialEffectEvent, false);
                    }
                }
                else
                {
                    if (currentBattleCardData != BattleCardData)
                    {
                        createCard(BattleCardData, false);
                    }
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        //CardHandler.Instance.SetCurrentFocusedCard(this);
        //DisplayOnPointerEnter(eventData);
        //PlayAudioOnPointerEnter(eventData);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        CardHandler.Instance.RemoveCurrentFocusedCard();
        //DisplayOnPointerExit(eventData);
    }


    private void DisplayOnPointerEnter(PointerEventData eventData)
    {
        if(status == BelongTo.PlayerHand || (ContentsOfDeck.Instance != null && ContentsOfDeck.Instance.gameObject.activeSelf))
        {
            // enlarge the card scale;
            var scale = CardHandler.Instance.cardScaleInPlayerHandWhenHovering;
            this.gameObject.transform.localScale = new Vector3(scale, scale, scale);
            gameObject.GetComponentInChildren<Canvas>().sortingOrder=20;
        }
    }
    private void resetScale(){
        // enlarge the card scale;
        var scale = CardHandler.Instance.cardOriginalScale;
        this.gameObject.transform.localScale = new Vector3(scale, scale, scale);
        gameObject.GetComponentInChildren<Canvas>().sortingOrder=5;
    }
    private void DestroyFocused(){
        // enlarge the card scale;
        var scale = CardHandler.Instance.cardOriginalScale;
        this.gameObject.transform.localScale = new Vector3(scale, scale, scale);            
        gameObject.GetComponentInChildren<Canvas>().sortingOrder=0;
    }
    private void DisplayOnPointerExit(PointerEventData eventData)
    {
        if(status == BelongTo.PlayerHand || (ContentsOfDeck.Instance != null && ContentsOfDeck.Instance.gameObject.activeSelf))
        {
            resetScale();
            // enlarge the card scale;
            //var scale = CardHandler.Instance.cardOriginalScale;
            //this.gameObject.transform.localScale = new Vector3(scale, scale, scale);
            //gameObject.GetComponentInChildren<Canvas>().sortingOrder=0;
        }
    }

    public void SetHighlightColor(Color newColor)
    {
        if(CardHighlightLine != null)
        {
            Image image;

            if(CardHighlightLine.TryGetComponent<Image>(out image))
            {
                image.color = newColor;
                InnerCardHighlightLineImage.color = newColor;
            }
        }
    }

    public void SetHighlightAlpha(float newAlpha)
    {
        if (CardHighlightLine != null)
        {
            CanvasGroup canvasGroup;

            if (CardHighlightLine.TryGetComponent<CanvasGroup>(out canvasGroup))
            {
                canvasGroup.alpha = newAlpha;
                InnerCardHighlightLineImage.color = new Color(InnerCardHighlightLineImage.color.r, InnerCardHighlightLineImage.color.g, InnerCardHighlightLineImage.color.b,newAlpha);
            }
        }
    }

    private void PlayAudioOnPointerEnter(PointerEventData eventData)
    {
        if(status == BelongTo.PlayerHand)
        {
            AudioManager.Instance.PlayOneShot(AudioManager.AudioEffects.exploreCard);
        }
    }

    public void UpdateDescriptionText(float damageModifier = 1f, float blockModifier = 1f, float magicModifier = 1f, float costModifier = 1f)
    {
        description.text = (Damage > 0 ? "Deal " + getDamage() * damageModifier + " Damage" : "")
                                + (block > 0 ? " Block " + block * blockModifier + " Damage" : "")
                                + (magic > 0 ? " Gain " + magic * magicModifier + " Mana" : "")
                                + BattleCardData.effectDescriptor;
        
    }


        bool hovered=false;
        bool ChangeState=true;
        public void updateState() {
            if (hovered || CardHandler.Instance.heldCard == this) {
            if (ChangeState)
            {
                CardHandler.Instance.SetCurrentFocusedCard(this);
                DisplayOnPointerEnter(null);
                PlayAudioOnPointerEnter(null);
                ChangeState = false;
            }   
            }else if (!ChangeState)
        {
            //CardHandler.Instance.RemoveCurrentFocusedCard();
            DisplayOnPointerExit(null);
            ChangeState = true;
        }
        }




}
