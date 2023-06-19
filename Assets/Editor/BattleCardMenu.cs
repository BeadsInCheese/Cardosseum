using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class BattleCardMenu : EditorWindow
{

    BattleCardDataContainer evnt;
    string result = "";
    [MenuItem("Window/BattleCardMenu")]
    public static void Display()
    {
        GetWindow<BattleCardMenu>("BattleCardMenu");
    }
    private string SetValues(string methodName)
    {
        Debug.Log(methodName);
        //todo
        return methodName;
    }

    public string getParameter(SerializedObject so,int index)
    {
        SerializedProperty persistentCalls = so.FindProperty("effect.m_PersistentCalls.m_Calls");
        if (persistentCalls == null)
        {
            return "";
        }
            if (persistentCalls.GetArrayElementAtIndex(index).FindPropertyRelative("m_Arguments.m_IntArgument") != null)
            {
                Debug.Log(persistentCalls.GetArrayElementAtIndex(index).FindPropertyRelative("m_Arguments.m_IntArgument").intValue);
            return ""+persistentCalls.GetArrayElementAtIndex(index).FindPropertyRelative("m_Arguments.m_IntArgument").intValue;
            }
            else
            {
                Debug.Log("no params found");
            return "";    
        }
        return "";

    }
    private string processEventNames(Effect effect)
    {
        string result = "";
        // Check if registered event contains AddBattleCard function
        var eventCount = effect.targetEvent.GetPersistentEventCount();
        for (int i = 0; i < eventCount; i++)
        {
            var funcName = effect.targetEvent.GetPersistentMethodName(i);
            var tarObj = effect.targetEvent.GetPersistentTarget(i);

            string addBattleCardClassName = "BattleCardDataContainer";
            string addBattleCardFuncName = "addCard";
            getParameter(new SerializedObject(tarObj),i);
            result+=SetValues(funcName + tarObj.name)+":" + getParameter(new SerializedObject(tarObj), i);

        }
        return result;

    }
    private string processEventNames(UnityEvent effect, BattleCardDataContainer data)
    {
        result = "";
        // Check if registered event contains AddBattleCard function
        var eventCount = effect.GetPersistentEventCount();
        for (int i = 0; i < eventCount; i++)
        {
            var funcName = effect.GetPersistentMethodName(i);
            var tarObj = effect.GetPersistentTarget(i);


            string addBattleCardClassName = "BattleCardDataContainer";
            string addBattleCardFuncName = "addCard";
            result+=SetValues(funcName)+":" + getParameter(new SerializedObject(data), i);

        }
        return result;
    }
    public string ExamineEvents(BattleCardDataContainer data)
    {
        string result = "";
        foreach (var CondEffect in data.conditionalEffects)
        {
            foreach (var effect in CondEffect.ConditionalEffects)
            {
                result+=processEventNames(effect);

            }
        }
        result += processEventNames(data.effect, data);
        return result;
    }

    private void OnGUI()
    {
        evnt = EditorGUILayout.ObjectField("", evnt, typeof(BattleCardDataContainer), true) as BattleCardDataContainer;
        if (GUILayout.Button("Compile all scriptable cards"))
        {
            Debug.Log("Compile cards");
            result = ExamineEvents(evnt);

        }
        //if (!Application.isPlaying) { return; }


        GUILayout.Label(result, EditorStyles.boldLabel);
    }

}
