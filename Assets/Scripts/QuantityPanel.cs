using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class QuantityPanel : MonoBehaviour
{ 
    public static QuantityPanel instance; 
    public GameObject[] TermResultBlocks;
    [SerializeField] public TextMeshProUGUI mathSentence;
    private InputSlot currentSlot;
    
    private GameControl GCtrl { get { return GameControl.instance; }}
    private GameUI UICtrl { get { return GameUI.instance; }}

    // Permite a inserção de 2 números apenas pelo usuário
    public bool UserAnswerLimitReached
    { 
        get 
        {
            byte length = (byte)mathSentence.text.Length;
            return char.IsNumber(mathSentence.text[length - 2]);
        }
    }
    private byte Pos { get { return GCtrl.SelectedPos; }}
    public int TermResult 
    {
        get 
        {
            int pos = GCtrl.Slots.SelectedSlot.pos;
            return GCtrl.TermsResult[pos];
        }
    }

    void Awake()
    {        
        if(instance == null)
            instance = this;
        else Destroy(this.gameObject);
    }

    private void InsertAnswerNumber(int newValue)
    {
        if(string.IsNullOrEmpty(GCtrl.Slots[2][Pos].SlotText))
            GameUI.instance.InsertNumber(newValue);
        else
        {
            GCtrl.Slots[2][Pos].SlotText = newValue.ToString();
            GCtrl.AnswerValidation();
        } 
    }

    private string GetUserAnswer(int digitCount)
    {     
        byte length = (byte)mathSentence.text.Length;
        string result = mathSentence.text.Substring(length - digitCount, digitCount);  
        if(string.IsNullOrEmpty(result)) return string.Empty;
        return result;
    }

    public void ResetAlteredSlots()
    {
        if(GCtrl.Slots[2][Pos].SlotText == GCtrl.TermsResult[Pos].ToString())
        {
            if(!string.IsNullOrEmpty(GCtrl.Slots[0][Pos].SlotText))
                ResetAlteredSlotByRow(0);
            if(!string.IsNullOrEmpty(GCtrl.Slots[1][Pos].SlotText))
                ResetAlteredSlotByRow(1);
        }
    }

    public void ResetAlteredSlotsImmediate()
    {
        if(!string.IsNullOrEmpty(GCtrl.Slots[0][Pos].SlotText))
            ResetAlteredSlotByRow(0);
        if(!string.IsNullOrEmpty(GCtrl.Slots[1][Pos].SlotText))
            ResetAlteredSlotByRow(1);
    }

    private void ResetAlteredSlotByRow(int rowID)
    {
        int index = GCtrl.Terms[rowID].ToString().Length - Pos - 1;
        int termDigit = GCtrl.Terms[rowID].ToString()[index].ToInt();
        if(termDigit != GCtrl.Slots[rowID][Pos].SlotText.ToInt())
            GCtrl.Slots[rowID][Pos].SlotText = termDigit.ToString();
    }

    private void SetBlocksActivation(int TermResult, bool value)
    {                
        for (int i = 0; i < TermResult; i++)
        {
            if(TermResultBlocks[i].activeSelf != value)
                TermResultBlocks[i].SetActive(value);
        }
    }

    private void UpdateMathSentence()
    {
        byte pos = GCtrl.SelectedPos;
        currentSlot = GCtrl.Slots[2][pos];
        string first = GCtrl.Slots[0][pos].SlotText;
        string second = GCtrl.Slots[1][pos].SlotText;
        mathSentence.text = string.IsNullOrEmpty(first)? "0" : first;
        mathSentence.text += (GCtrl.IsAddition)? " + ": " - ";
        mathSentence.text += string.IsNullOrEmpty(second)? "0" : second;
        if(GCtrl.Slots[0][pos].HasCarryNumber)        
            mathSentence.text += " + " + GCtrl.Slots[0][pos].CarryNumber.ToString();
        mathSentence.text += " = ";
    }

    public void UpdateSlotAnswer()
    {
        string numberString = GetUserAnswer(1);
        if(!string.IsNullOrWhiteSpace(numberString))
        {
            InsertAnswerNumber(numberString.ToInt());            
            if(!GCtrl.IsSubtraction && TermResult > 9) //Carry number validation
            {
                char firstDigit = GetUserAnswer(2).ToCharArray()[0];
                if(char.IsNumber(firstDigit) && firstDigit != '0')
                {                    
                    int nextIndex = currentSlot.pos;                
                    UICtrl.SetCarryNumberActivation(nextIndex, true);
                    UICtrl.SetCarryNumber(nextIndex, firstDigit.ToInt());
                }
            }
        }
    }

    void OnEnable()
    {
        GCtrl.SelectRect.SetActive(false);
        int res = TermResult;
        if(res < 0)
        {
            Debug.LogWarning("Não pode subtrair por um número maior que o primeiro.");
            gameObject.SetActive(false);
            return;
        }
        SetBlocksActivation(res, true);
        UpdateMathSentence();
    }

    void OnDisable()
    {
        ResetAlteredSlots();
        SetBlocksActivation(TermResult, false);
        GCtrl.SelectRect.SetActive(true);
        if(currentSlot != null) currentSlot.DeselectSlotImmediate();
    }
}