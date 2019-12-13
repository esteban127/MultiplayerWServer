using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionBarDisplay : MonoBehaviour
{
    [SerializeField] Sprite emptySprite;
    [SerializeField] Color selectedColor;
    [SerializeField] Color notSelectedColor;
    [SerializeField] Button ability0;
    [SerializeField] Button ability1;
    [SerializeField] Button ability2;
    [SerializeField] Button ability3;
    [SerializeField] Button ultimate;
    [SerializeField] Button catalizer0;
    [SerializeField] Button catalizer1;
    [SerializeField] Button catalizer2;
    [SerializeField] Button endTurnButton;

    Button GetButtonByPos(int pos)
    {
        switch (pos)
        {
            case 0:
                return ability0;
            case 1:
                return ability1;
            case 2:
                return ability2;
            case 3:
                return ability3;
            case 4:
                return ultimate;
            case 5:
                return catalizer0;
            case 6:
                return catalizer1;
            case 7:
                return catalizer2;  
        }
        return null;
    }

    public void SetSprite(int pos, Sprite sprite)
    {
        GetButtonByPos(pos).GetComponent<Image>().sprite = sprite;
    }
    public void SetCooldown(int pos, int cooldown)
    {
        if (cooldown > 0)
        {
            GetButtonByPos(pos).interactable = false;
            GetButtonByPos(pos).GetComponentInChildren<Text>().text = cooldown.ToString();
        }
        else
        {
            if(cooldown== 0)
            {
                GetButtonByPos(pos).interactable = true;
            }
            GetButtonByPos(pos).GetComponentInChildren<Text>().text = "";
            if (cooldown < 0)
            {
                SetSprite(pos, emptySprite);
            }
        }
    }
    public void Select(int pos)
    {
        GetButtonByPos(pos).GetComponent<Image>().color = selectedColor;
    }
    public void Unselect(int pos)
    {
        GetButtonByPos(pos).GetComponent<Image>().color = notSelectedColor;
    }
    public void ConfirmEndTurn(int pos)
    {
        endTurnButton.GetComponent<Image>().color = selectedColor;
    }
    public void CancelEndTurn(int pos)
    {
        endTurnButton.GetComponent<Image>().color = notSelectedColor;
    }
    public void DisableAll()
    {
        endTurnButton.interactable = false;
        for (int i = 0; i < 8; i++)
        {
            GetButtonByPos(i).interactable = false;            
        }
    }
    public void EneableAll()
    {
        endTurnButton.interactable = true;
        for (int i = 0; i < 8; i++)
        {
            GetButtonByPos(i).interactable = true;
            Unselect(i);
        }
    }
    public void DisableOnPos(int pos)
    {
        GetButtonByPos(pos).interactable = false;
    }

}
