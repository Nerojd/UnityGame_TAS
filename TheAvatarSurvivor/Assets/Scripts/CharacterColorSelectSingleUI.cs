using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterColorSelectSingleUI : MonoBehaviour
{
    [SerializeField] private int colorId;
    [SerializeField] private Image image;
    [SerializeField] private GameObject selectedGameObject;

    /*******************************************/
    /*              Unity Methods              */
    /*******************************************/
    private void Start()
    {
        MultiplayerManager.Instance.OnPlayerDataNetworkListChanged += GameManager_OnPlayerDataNetworkListChanged;

        UpdateIsSelected();
    }

    private void OnDestroy()
    {
        MultiplayerManager.Instance.OnPlayerDataNetworkListChanged -= GameManager_OnPlayerDataNetworkListChanged;
    }

    /*******************************************/
    /*             Private Methods             */
    /*******************************************/
    private void GameManager_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdateIsSelected();
    }

    /******************************************/
    /*             Public Methods             */
    /******************************************/
    public void UpdateIsSelected()
    {
        if (MultiplayerManager.Instance.GetPlayerData().colorId == colorId)
        {
            selectedGameObject.SetActive(true);
        }
        else
        {
            selectedGameObject.SetActive(false);
        }
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }
    public void SetId(int id)
    {
        colorId = id;
    }
    public void SetItsSelection(bool isSelected)
    {
        selectedGameObject.SetActive(isSelected);
    }

    public void SetPlayerColor()
    {
        GameManager.Instance.ChangePlayerColorId(colorId);
    }
}