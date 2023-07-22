using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OpponentScrollManager : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject ScrollNameTemplate;
    public void createEntry(string name) {
        var entry=Instantiate(ScrollNameTemplate);
        entry.transform.parent = this.transform;
        entry.transform.localScale = new Vector3(1, 1, 1);
        entry.GetComponent<TMPro.TextMeshProUGUI>().text = name;
        entry.transform.GetChild(0).gameObject.SetActive(false);
    }
    public void updateEntrys()
    {
        int index = 0;
        foreach (Transform i in transform)
        {

            if (Deck.Instance.bossesDefeated > index)
            {
                i.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                i.GetChild(0).gameObject.SetActive(false);
            }
            index++;
        }
    }
    public void createBossScroll()
    {
        //search spawn function target object
        foreach (var i in Deck.Instance.BossBattles)
        {
            foreach (var j in i.options)
            {
                foreach (var effect in j.effects)
                {
                    int eventCount = effect.targetEvent.GetPersistentEventCount();
                    for (int e = 0; e < eventCount; e++)
                    {
                        Debug.Log(effect.targetEvent.GetPersistentMethodName(e));
                        if (effect.targetEvent.GetPersistentMethodName(e).Contains("Spawn"))
                        {
                            var enemyName = ((CreatureDataContainer)effect.targetEvent.GetPersistentTarget(e)).name;
                            createEntry(enemyName);
                        }


                    }
                }
            }
        }
    }
    void Start()
    {
        Invoke("createBossScroll",0.2f);
        updateEntrys();
        
    }

    // Update is called once per frame
    void Update()
    {
        updateEntrys();
    }
}
