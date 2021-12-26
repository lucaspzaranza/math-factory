using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI instance;    
    public Transform[] slotsParents;
    public TextMeshProUGUI mathOperator;
    public GameObject quantityPanel;

    #region Props
    //Props servindo como aliases somente.
    private GameControl GControl { get { return GameControl.instance; }}
    private QuantityPanel _panelScript;
    private QuantityPanel PanelScript 
    { 
        get 
        { 
            if(_panelScript == null) 
                _panelScript = quantityPanel.GetComponent<QuantityPanel>(); 
            return _panelScript;
        }
    }
    private byte Pos { get { return GControl.SelectedPos; }}
    private byte Row { get { return GControl.SelectedRow; }}
    public bool QuantityPanelIsActive { get { return quantityPanel.activeSelf; }}
    private int SlotsCounter { get { return GControl.Slots[Row].ActivesSlotsCount(); }}
    private SelectionRect SelectRect { get { return SelectionRect.instance; }}

    //Props para utilização nos scripts

    [SerializeField] private TextMeshProUGUI[] _carryNumbers;
    public TextMeshProUGUI[] CarryNumbers { get { return _carryNumbers;}}
    
    #endregion

    void Awake()
    {
        if(instance == null)
            instance = this;
        else Destroy(this.gameObject);
    }

    public void Clear()
    {
        SceneManager.LoadScene(0);
    }

    private void DeactivateAllAnswerSlots()
    {
        int prevRow = GControl.SelectedRow;
        GControl.Slots[2][0].SelectSlotRow();
        int activeSlots = GControl.Slots[2].ActivesSlotsCount();
        for (int i = 0; i < activeSlots; i++)
        {
            DeleteNumber();
        }
        GControl.Slots[prevRow][0].SelectSlotRow();
    }

    private void DeactivateCarryNumberInPos(int pos)
    {
        if(CarryNumbers[pos].transform.parent.gameObject.activeSelf)
        {
            CarryNumbers[pos].text = "0";
            CarryNumbers[pos].transform.parent.gameObject.SetActive(false);
        }
    }

    private void DeactivateCarryNumbers()
    {
        foreach (var carry in CarryNumbers)
        {
            if(carry.transform.parent.gameObject.activeSelf)
            {
                carry.text = "0";
                carry.transform.parent.gameObject.SetActive(false);
            }
        }
    }

    public void DeleteNumber()
    {       
        if(SelectedSlotIsHighlighted() && SlotIsEmpty(Pos)) 
        {
            GControl.Slots.SelectedSlot.DeselectSlotImmediate();
            return;
        }

        if(Row < 2) GControl.UpdateOperatorSlotPosition();
        else if (Row == 2) DoSlotResetValidation();

        SlotShifting();
        EmptyLastSlot();
    
        if(SelectedSlotIsHighlighted())
            GControl.Slots.SelectedSlot.DeselectSlotImmediate();

        DoMathCalculation();
        if(Pos > 0) SelectionRect.instance.ChangeRectPosition(0);
    }

    public void DeleteNumberOnPanel()
    {
        int length = PanelScript.mathSentence.text.Length;
        bool prevCharIsNumber = char.IsNumber(PanelScript.mathSentence.text[length - 1]);
        if(prevCharIsNumber)
            PanelScript.mathSentence.text = 
            PanelScript.mathSentence.text.Substring(0, length - 1);
    }

    public void DeletionNumberSelector()
    {
        if(QuantityPanelIsActive) DeleteNumberOnPanel();
        else DeleteNumber(); 
    }

    private void DoMathCalculation()
    {
        GControl.ConcatenateRowNumbers(Row);
        GControl.CalculateResult();
        if(GControl.IsSubtraction)
            GControl.DoBorrowTermsVerification();
    }

    private void DoSlotResetValidation()
    {
        int index = GControl.Slots[2].ActivesSlotsCount();
        if(QuantityPanel.instance != null)
            QuantityPanel.instance.ResetAlteredSlotsImmediate();
        if(GControl.mathOps == Operations.Addition && GControl.Slots[0][index - 1].HasCarryNumber)
            DeactivateCarryNumberInPos(index - 2); // -2 Due array index differences    
        
        if(index < GameControl.maxNumberOfSlots )
        {
            if(index > 1 && GControl.Slots[2][index - 2].GetColor() != Color.white)
                GControl.Slots[2][index - 2].SetColor(Color.white);
        }
        else
        {
            int lastIndex = (!SlotIsEmpty(index - 1))? index - 1 : index - 2;
            GControl.Slots[2][lastIndex].SetColor(Color.white);
        }
    }

    private void EmptyLastSlot()
    {
        int slotsCount = SlotsCounter;
        if(SlotAmountLimitReached())
        {
            if(!SlotIsEmpty(slotsCount - 1))
            {                
                GControl.Slots[Row][slotsCount - 1].SlotText = string.Empty;
                GControl.Slots.SelectedSlot.DeselectSlotImmediate();
                return;
            }
            if(SelectedSlotIsHighlighted())
            {
                if(!SlotIsEmpty(slotsCount - 2))                
                    GControl.Slots[Row][slotsCount - 2].SlotText = string.Empty;                                    
            }
        }
        SlotDeactivationVerification();
        if(slotsCount > 1) SelectRect.ChangeRectSize(slotsCount - 1);
    }

    public void InsertNumber(int number)
    {                     
        if(Pos == 0 && !GControl.Slots[Row][Pos].isHighlighted)
            GControl.ShiftNumbersToLeft(SlotsCounter - 1);

        GControl.Slots.SelectedSlot.SlotText = number.ToString();
        SlotActivationVerification();
        
        if(GControl.Slots.SelectedSlot.isHighlighted)
            EventSystem.current.SetSelectedGameObject(GControl.Slots.SelectedSlot.gameObject);
        
        DoMathCalculation();

        if(Row < 2) GControl.UpdateOperatorSlotPosition();  
        else GControl.AnswerValidation();
        if(Pos == 0 && !GControl.Slots.SelectedSlot.isHighlighted)
            SelectRect.ChangeRectSize(SlotsCounter);
    }

    private void InsertNumberOnPanel(int number)
    {
        if(!PanelScript.UserAnswerLimitReached)
            PanelScript.mathSentence.text += number.ToString();
    }

    public void InsertionSelector(int number)
    {
        if(QuantityPanelIsActive) InsertNumberOnPanel(number);
        else InsertNumber(number);
    }

    public void ResetAllAlteredSlots()
    {
        int slotsCount = GControl.Slots[0].ActivesSlotsCount();
        for(int i = 0; i < slotsCount; i++)
        {
            if(GControl.Slots[0][i].SlotText.Length == 2) // Confere se tem 2 dígitos            
                GControl.Slots[0][i].SlotText = GControl.Slots[0][i].SlotText.Remove(0, 1);
        }
    }

    private bool SelectedSlotIsHighlighted()
    {
        return GControl.Slots.SelectedSlot.isHighlighted;
    }

    public void SetCarryNumberActivation(int index, bool activationValue)
    {
        if(index < GameControl.maxNumberOfSlots - 1)
            CarryNumbers[index].transform.parent.gameObject.SetActive(activationValue);
        else if(!GControl.Slots[2][index + 1].gameObject.activeSelf)
            GControl.Slots[2][index + 1].gameObject.SetActive(true);        
    }

    public void SetCarryNumber(int index, int number)
    {        
        if(index < GameControl.maxNumberOfSlots - 1)
        {
            CarryNumbers[index].text = number.ToString();
            GControl.Slots[0][index + 1].CarryNumber = number;
            GControl.GetTermsResult(index + 1);         
        }
        else
        {
            GControl.Slots[2][index + 1].SlotText = number.ToString();
            GControl.ConcatenateRowNumbers(2);
        }
    }

    public void SetQuantityPanelActivation(bool value)
    {        
        if(!value) QuantityPanel.instance.UpdateSlotAnswer();

        if(quantityPanel.activeSelf != value)
            quantityPanel.SetActive(value);
    }

    private void SlotActivationVerification()
    {
        int slotsCount = SlotsCounter;
        bool logic = slotsCount < GameControl.maxNumberOfSlots;         

        if(logic)
        {
            int next = (GControl.Slots[Row][Pos].isHighlighted)? Pos + 1 : slotsCount;
            bool nextSlotIsNotActive = !GControl.Slots[Row][next].gameObject.activeSelf;
            if(nextSlotIsNotActive)
                GControl.Slots[Row][slotsCount].gameObject.SetActive(true);          
        }
    }

    private bool SlotAmountLimitReached()
    {
        return SlotsCounter == GameControl.maxNumberOfSlots;
    }

    private void SlotDeactivationVerification()
    {
        int slotsCount = SlotsCounter;
        if(slotsCount > 1)
        {
            int last = slotsCount - 1;
            bool nextSlotIsActive = GControl.Slots[Row][last].gameObject.activeSelf;

            if(nextSlotIsActive)
                GControl.Slots[Row][last].gameObject.SetActive(false);          
        }
    }

    private bool SlotIsEmpty(int index)
    {
        return string.IsNullOrEmpty(GControl.Slots[Row][index].SlotText);
    }

    private void SlotShifting()
    {
        int slotsToShift = (SelectedSlotIsHighlighted())?
            (SlotsCounter - 1) - Pos : SlotsCounter - 1;
        GControl.ShiftValuesToRight(Pos, Pos + slotsToShift);
    }

    public void ToggleMathOperation()
    {        
        if(QuantityPanelIsActive) return;
        GControl.Slots.SelectedSlot.DeselectSlotImmediate();
        if(GControl.IsAddition)
        {
            GControl.mathOps = Operations.Subtraction;
            mathOperator.text = "-";
            DeactivateCarryNumbers();
        }
        else if(GControl.IsSubtraction)
        {
            ResetAllAlteredSlots();
            GControl.mathOps = Operations.Addition;
            mathOperator.text = "+";
            GControl.DashStrokeDeactivation();
        }
        if(GControl.Terms[2] > 0)
            DeactivateAllAnswerSlots();
        DoMathCalculation();
        if(Row == 0) GControl.Slots[1][0].SelectSlotRow();
    }
}