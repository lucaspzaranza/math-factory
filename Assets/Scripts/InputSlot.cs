using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InputSlot : MonoBehaviour
{
    #region VARS

    public bool isHighlighted;
    public byte row;
    public byte pos;
    
    private GameControl GControl; 
    [SerializeField] private Image slotImg;
    private int clickCount;
    private float timer;
    private Color32 highlightedColor = new Color32(139, 177, 212, 255);
    #endregion 

    #region PROPS

    [SerializeField] private int _carryNumber;
    public int CarryNumber { get { return _carryNumber; } 
                             set { _carryNumber = value; }}
    public bool HasCarryNumber { get { return CarryNumber > 0; }}
    public bool IsEmpty { get { return string.IsNullOrEmpty(SlotText); }}
    private GameUI GUICtrl { get { return GameUI.instance; }}
    private SelectionRect SelectRect { get { return SelectionRect.instance; }}
    public string SlotText
    {
        get { return GetComponentInChildren<TextMeshProUGUI>().text; }
        set { GetComponentInChildren<TextMeshProUGUI>().text = value; }             
    }

    #endregion

    void Start()
    {
        if(slotImg == null)
            slotImg = GetComponent<Image>();
        GControl = GameControl.instance;
        isHighlighted = false;    
    }

    void FixedUpdate()
    {
        if(!isHighlighted && clickCount == 1)
        {
            timer += Time.fixedDeltaTime;
            if(timer >= 0.5f)
            {                
                ClickReset();
                timer = 0f;
            }
        }                 
    }

    public void CallQuantityMenu()
    {
        bool slotsActive = GControl.Slots[0][pos].isActiveAndEnabled;
        slotsActive &= GControl.Slots[1][pos].isActiveAndEnabled;
        if(slotsActive)
            GUICtrl.SetQuantityPanelActivation(true);        
        else Debug.LogWarning("Não precisa do sub-menu, dá pra resolver sem.");
    }

    public void ClickIncrement()
    {
        if(!isHighlighted) clickCount++;

        if(clickCount == 1) 
        {
            SelectSlotRow();
            if(row == 2 && pos < GameControl.maxNumberOfSlots) 
            {
                GControl.Slots.SelectedSlot = this;
                CallQuantityMenu();
                return;
            }
        }

        if(clickCount == 2) SelectCurrentSlot();   
    }

    public void ClickReset()
    {        
        clickCount = 0;
        isHighlighted = false;        
    }

    public void DeselectSlot()
    {             
        StartCoroutine(WaitOneFrameAndDeselect());
    }

    public Color GetColor()
    {
        return slotImg.color;
    }

    public void DeselectSlotImmediate()
    {
        SelectSlotRow();    
        if(slotImg.color == highlightedColor)                            
            slotImg.color = Color.white;
        ClickReset(); 
    }

    public void PrintSlotCoordinates()
    {
        print("Row = " + row + ". Pos = " + pos + ".");
    }

    public void SelectCurrentSlot()
    {        
        GControl.Slots.SelectedSlot = this;
        isHighlighted = true;
        if(slotImg != null) slotImg.color = highlightedColor;
        SelectRect.ChangeRectSize(1);
        SelectRect.ChangeRectPosition(pos);            
    }

    public void SelectSlotRow() // Seleciona o primeiro elemento da row
    {               
        GControl.Slots.SelectedSlot = GControl.Slots[row][0];  
        SelectRect.ChangeRectRow(row);
        SelectRect.ChangeRectSize(GControl.Slots[row].ActivesSlotsCount());
        SelectRect.ChangeRectPosition(0);
    }
    
    public void SetColor(Color newColor)
    {
        if(slotImg != null)
            slotImg.color = newColor;
    }

    public void SetColor(Color newColor, int index)
    {
        print(index);
        slotImg.color = newColor;
    }

    private IEnumerator WaitOneFrameAndDeselect()
    {
        yield return new WaitForEndOfFrame();
        if(!GameUI.instance.QuantityPanelIsActive)
        {
            var currentGObj = EventSystem.current.currentSelectedGameObject;
            bool isInput = false;

            if(currentGObj != null)
                isInput = currentGObj.tag == "Input Buttons";                 
            else SelectSlotRow();  

            if(!isInput && isHighlighted)
            {                              
                if(slotImg.color == highlightedColor) slotImg.color = Color.white;
                ClickReset();              
            }       
        }
    }

    void OnMouseDown()
    {
        ClickIncrement();
    }
} 