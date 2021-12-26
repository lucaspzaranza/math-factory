using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum Operations
{
    Addition,
    Subtraction
}

public class GameControl : MonoBehaviour
{
    #region VARS
    public GameObject slot;
    public RectTransform opSlot;
    public GameObject dashedStroke;
    public Operations mathOps;
    public int finalResult;
    public static GameControl instance; 
    public const byte maxNumberOfSlots = 6;

    private GameUI UIScript = GameUI.instance; 

    #endregion

    #region PROPS

    private GameUI UICtrl { get { return GameUI.instance;}}

    [SerializeField] private SlotContainer _slots = null;
    public SlotContainer Slots
    {
        get { return _slots; }        
    }

    [SerializeField] private GameObject _selectRect;
    public GameObject SelectRect { get { return _selectRect; }}

    public byte SelectedRow 
    {
        get
        { 
            if(Slots.SelectedSlot == null) return 0;
            return Slots.SelectedSlot.row; 
        }
    }

    public byte SelectedPos
    {
        get 
        { 
            if(Slots.SelectedSlot == null) return 0;
            return Slots.SelectedSlot.pos;         
        }
    }

    [SerializeField] private int[] _terms = new int[3];
    public int[] Terms
    {
        get { return _terms; }
        private set { _terms = value; }
    }

    [SerializeField] private List<int> _termsResult;
    public List<int> TermsResult 
    {
        get { return _termsResult; }
        set { _termsResult = value; }
    }

    public bool IsAddition { get { return mathOps == Operations.Addition;}}
    public bool IsSubtraction { get { return mathOps == Operations.Subtraction;}}
    
    #endregion

    #region MonoBehaviour Lifecycle Functions
    void Awake()
    {
        if(instance == null)
            instance = this;
        else Destroy(this.gameObject);
    }

    void Start()
    {
        mathOps = Operations.Addition;
    }

    void Update()
    {
        KeyboardInput();
    }

    #endregion

    #region Game Functions

    public void AnswerValidation()
    {
        var result = finalResult.ToString().Reverse().ToArray();
        int slotCount = Slots[2].ActivesSlotsCount();
        for (int i = 0; i < slotCount; i++)
        {
            InputSlot slot = Slots[2][i];
            if(i < result.Length)
            {
                if(slot.SlotText.ToInt() == result[i].ToInt())
                    slot.SetColor(Color.yellow);
                else slot.SetColor(Color.red);
            }
            else if(!string.IsNullOrEmpty(slot.SlotText))
            {
                if(slot.SlotText != "0") slot.SetColor(Color.red);
                else LeftZeroValidation(i);
            }
        }
    }
    
    private void LeftZeroValidation(int index)
    {
        string rawTerm = Slots[2].GetRawValue();
        int newIndex = rawTerm.Length - 1 - index;
        string withoutZero = rawTerm.Remove(newIndex, 1);
        //print("Raw: " + rawTerm + ". Without Left Zeros: " + withoutZero.ToInt());
        if(rawTerm.ToInt() != withoutZero.ToInt())
            Slots[2][index].SetColor(Color.red);
        else Slots[2][index].SetColor(Color.white);
    }

    public void CalculateResult()
    {
        if(IsAddition)
            finalResult = Terms[0] + Terms[1];
        else
            finalResult = Terms[0] - Terms[1];

        for(int i = 0; i < GetTermsPairsCount(); i++)
            GetTermsResult(i);
    }

    public void ConcatenateRowNumbers(byte rowID)
    {
        string result = string.Empty;
        
        for(int i = Slots[rowID].Length - 1; i >= 0; i--) 
            result += Slots[rowID][i].SlotText;

        Terms[rowID] = result.ToInt();
    }

    private void DashStrokeActivation()
    {
        bool instantiate;
        
        for (int i = 0; i < GetTermsPairsCount(); i++)
        {
            instantiate = finalResult > 0 && TermsResult[i] < 0;
            instantiate &= string.IsNullOrEmpty(Slots[2][i].SlotText);
            if(instantiate)
            {            
                InstantiateDashedStroke(i + 1);    
                break;
            }
        }
    }

    public void DashStrokeDeactivation()
    {
        bool destroy = IsAddition;

        if(IsSubtraction)
        {
            int pos = dashedStroke.GetComponent<DashedStroke>().Pos;
            destroy = TermsResult[pos - 1] >= 0;
        }                
        
        if(destroy || finalResult < 0) dashedStroke.SetActive(false);            
    }

    // Verifica se precisa "pedir emprestado" do número ao lado esquerdo.
    public void DoBorrowTermsVerification()
    {  
        if(dashedStroke.activeSelf) 
            DashStrokeDeactivation();
        DashStrokeActivation();
    }

    public int GetTermsPairsCount()
    {
        int count = 0;
        int activeSlots = Slots[SelectedRow].ActivesSlotsCount();
        if(SelectedRow == 2 && activeSlots == 7) activeSlots--;
        
        for (int i = 0; i < activeSlots; i++)
        {
            bool isPair = Slots[0][i].isActiveAndEnabled &&
                          Slots[1][i].isActiveAndEnabled;
            if(isPair) count++;
        }
        return count;
    }

    public void GetTermsResult(int index)
    {
        int first = Slots[0][index].SlotText.ToInt();
        int second = Slots[1][index].SlotText.ToInt();
        TermsResult[index] = (IsAddition)? first + second : first - second;       
        if(Slots[0][index].HasCarryNumber)        
            TermsResult[index] += Slots[0][index].CarryNumber;                                  
    }

    private void InstantiateDashedStroke(int pos)
    {   
        int rowIndex = (Slots[0][pos].SlotText.ToInt() > 0)? 0 : 1; //Pede emprestado em cima (0) ou embaixo (1)?
        dashedStroke.transform.SetParent(Slots[rowIndex][pos].transform, false);
        dashedStroke.SetActive(true);
        if(!Slots[rowIndex][pos].gameObject.activeSelf)
            Slots[rowIndex][pos].gameObject.SetActive(true);
    }

    public void ShiftNumbersToLeft(int init)
    {
        for (int i = init; i > 0; i--)
        {
            if(i < Slots[SelectedRow].Length)
                Slots[SelectedRow][i].SlotText = Slots[SelectedRow][i - 1].SlotText;
        }
    }

    public void ShiftValuesToRight(int init, int final)
    {
        for (int i = init; i < final; i++)
        {   
            if(i < Slots[SelectedRow].Length - 1)
                Slots[SelectedRow][i].SlotText = Slots[SelectedRow][i + 1].SlotText;
        }
    }

    public void UpdateOperatorSlotPosition()
    {
        float offset = 115f;
        byte first = (byte)Slots[0].ActivesSlotsCount();
        byte second = (byte)Slots[1].ActivesSlotsCount();
        int biggerRow = (first > second)? 0 : 1;
        int lastIndex = Slots[biggerRow].ActivesSlotsCount() - 1;
        var lastSlot = Slots[biggerRow][lastIndex].GetComponent<Transform>();
        opSlot.localPosition = new Vector3
        (lastSlot.localPosition.x - offset, opSlot.localPosition.y, 0f);
    }

    #endregion

    // Função para fins de testes no Editor. 
    // É mais rápido que usar a UI do game com Mouse :P
    private void KeyboardInput()
    {
        if(Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0))
            GameUI.instance.InsertionSelector(0);
        else if(Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1))
            GameUI.instance.InsertionSelector(1);
        else if(Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2))
            GameUI.instance.InsertionSelector(2);
        else if(Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3))
            GameUI.instance.InsertionSelector(3);
        else if(Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4))
            GameUI.instance.InsertionSelector(4);
        else if(Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5))
            GameUI.instance.InsertionSelector(5);
        else if(Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6))
            GameUI.instance.InsertionSelector(6);
        else if(Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Alpha7))
            GameUI.instance.InsertionSelector(7);
        else if(Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Alpha8))
            GameUI.instance.InsertionSelector(8);
        else if(Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha9))
            GameUI.instance.InsertionSelector(9);
        else if(Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
            GameUI.instance.DeletionNumberSelector();
        else if(Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            GameUI.instance.ToggleMathOperation();
        else if(Input.GetKeyDown(KeyCode.Tab))
        {
            int newRow = (SelectedRow + 1) % 3;
            Slots[newRow][0].SelectSlotRow();
        }
    }
}