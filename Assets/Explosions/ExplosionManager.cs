using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : MonoBehaviour
{
    // Start is called before the first frame

    public static ExplosionManager Instance { get; private set; }
    private void Awake()
    {
        //singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        Instance = this;
    }

    public GameObject heartExplosion;
    public GameObject SwordExplosion;
    public Transform heartExplosionTarget; 
    public GameObject armorExplosion;
    public Transform armorExplosionTarget;
    public GameObject manaExplosion;
    public Transform manaExplosionTarget;
    public GameObject EraseAnimation;
    public GameObject exhaustAnimation;
    public GameObject StunAnimation;
    public Transform StunAnimationTarget;
    public void playCard(Card card){
        if(card.block!=0){
            PlayArmorAnimation(card.block);
        }
        if(card.magic!=0){
            PlayManaAnimation(card.magic);
        }
    }
    
    public void PlayEraseAnimation(GameObject Erased)
    {
        var i = Instantiate(EraseAnimation);
        i.transform.position = StunAnimationTarget.position;
        StartCoroutine(AudioManager.Instance.PlayDelayedSound(AudioManager.AudioEffects.Erase,1.5f));
        Erased.transform.position= StunAnimationTarget.position+new Vector3(3,-3,0);
        Destroy(Erased,3.5f);
    }
    public void PlayExhaustAnimation(Vector3 pos)
    {
        var i = Instantiate(exhaustAnimation);
        i.transform.position = pos;
    }
        public void PlayStunAnimation()
    {
            var i = Instantiate(StunAnimation);
            i.transform.position = StunAnimationTarget.position;
        AudioManager.Instance.PlayOneShot(AudioManager.AudioEffects.stun);
        
    }
    public void PlayStunAnimation(Vector3 pos)
    {
        var i = Instantiate(StunAnimation);
        i.transform.position = StunAnimationTarget.position;
        AudioManager.Instance.PlayOneShot(AudioManager.AudioEffects.stun);

    }
    public void PlayHealthAnimation(int damage){
        if(Mathf.Abs(damage)>0){
        var i=Instantiate(heartExplosion);
        var t=i.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        t.text=("+") +Mathf.Abs(damage);
        i.transform.position=heartExplosionTarget.position;
    }
    }
    GameObject playerDamageText;
    public void PlaySwordAnimation(int damage){
        if(Mathf.Abs(damage)>0){

                var i=Instantiate(SwordExplosion);
        var t=i.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (playerDamageText == null)
            {


                t.text = ("-") + Mathf.Abs(damage);

            }
            else
            {
                t.text = ("-") + (-int.Parse(playerDamageText.GetComponentInChildren<TMPro.TextMeshProUGUI>().text) + Mathf.Abs(damage));
                Destroy(playerDamageText);
            }
            playerDamageText = i;
            i.transform.position=heartExplosionTarget.position;
            AudioManager.Instance.PlayOneShot(AudioManager.AudioEffects.dealDamage);
    }}
    GameObject damagetext;
    
    public void PlaySwordAnimation(int damage,Vector3 pos){

        if (Mathf.Abs(damage) > 0)
        {
            if (damagetext == null)
            {
                var i = Instantiate(SwordExplosion);
                var t = i.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                t.text = ("-") + Mathf.Abs(damage);
                i.transform.position = pos;
                damagetext = i;

                AudioManager.Instance.PlayOneShot(AudioManager.AudioEffects.dealDamage);
            }else
            {
                var i = Instantiate(SwordExplosion);
                var t = i.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                t.text = ("-") + (-int.Parse( damagetext.GetComponentInChildren<TMPro.TextMeshProUGUI>().text) +Mathf.Abs(damage));
                Destroy(damagetext);
                damagetext = null;
                damagetext = i;
                i.transform.position = pos;
                AudioManager.Instance.PlayOneShot(AudioManager.AudioEffects.dealDamage);
            }
        }
    }
        public void PlayHealthAnimation(int damage,Vector3 pos){
        if(Mathf.Abs(damage)>0){
        var i=Instantiate(heartExplosion);
        var t=i.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        t.text=("+") +Mathf.Abs(damage);
        i.transform.position=pos;
    }}
        public void PlayArmorAnimation(int block){
        if(Mathf.Abs(block)>0){
        var i=Instantiate(armorExplosion);
        i.transform.position=armorExplosionTarget.position;
        var t=i.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        t.text=(block>0?"+":"-") +Mathf.Abs(block);
        i.transform.position=armorExplosionTarget.position;
            AudioManager.Instance.PlayOneShot(AudioManager.AudioEffects.shield);
        }
    }
    public void PlayArmorAnimation(int block,Vector3 pos){
        if(Mathf.Abs(block)>0){
        var i=Instantiate(armorExplosion);
        var t=i.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        t.text=(block>0?"+":"-") +Mathf.Abs(block);
        i.transform.position=pos;
            AudioManager.Instance.PlayOneShot(AudioManager.AudioEffects.shield);
        }
    }
        public void PlayManaAnimation(int mana){
        if(Mathf.Abs(mana)>0){
        var i=Instantiate(manaExplosion);
        i.transform.position=manaExplosionTarget.position;
        var t=i.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        t.text=(mana>0?"+":"-") +Mathf.Abs(mana);
        i.transform.position=manaExplosionTarget.position;
    }}

}
