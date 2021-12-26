using UnityEngine;

[System.Serializable]
public class SlotContainer
{
    [SerializeField] private InputSlot[] _firstRowSlots = null;
    [SerializeField] private InputSlot[] _secondRowSlots = null;
    [SerializeField] private InputSlot[] _resultRowSlots = null;
    [SerializeField] private InputSlot _selectedSlot;
    public InputSlot SelectedSlot 
    {
        get 
        {
            if(_selectedSlot == null) 
                _selectedSlot = _firstRowSlots[0];
            return _selectedSlot;
        }
        
        set { _selectedSlot = value;}
    }

    public InputSlot[] this[int index]
    {
        get
        {   
            if(index == 0) return _firstRowSlots;
            else if(index == 1) return _secondRowSlots;
            else if(index == 2) return _resultRowSlots;
            else return null;
        }
    }
}