using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Card : MonoBehaviour
{
    // Start is called before the first frame update
    public int Damage = 0;
    public int block = 0;
    public int magic = 0;
    public int money = 0;
    public int actionCost = 0;
    public UnityEvent effect;

    public void Playcard()
    {
        if (Deck.Instance.enemy != null)
        {
            Deck.Instance.enemy.HP -= Damage;
        }
        Deck.Instance.mana -= magic;
        Deck.Instance.money -= money;
        effect.Invoke();

    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
