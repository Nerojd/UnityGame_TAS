using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterColorSelectionUI : MonoBehaviour
{

    [SerializeField] private GameObject[] selectedGameObject;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < selectedGameObject.Length; i++)
        {
            if (i > GameManager.Instance.GetPlayerColorCount()) break;

            CharacterColorSelectSingleUI colorSingleUI = selectedGameObject[i].GetComponent<CharacterColorSelectSingleUI>();
            if (colorSingleUI != null)
            {
                colorSingleUI.SetId(i);
                colorSingleUI.SetColor(GameManager.Instance.GetPlayerColor(i));
                colorSingleUI.SetItsSelection(false);
                colorSingleUI.UpdateIsSelected();
            }
        }
    }
}
