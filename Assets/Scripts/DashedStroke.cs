using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashedStroke : MonoBehaviour
{
    public static DashedStroke instance;
    private InputSlot parentSlot;
    private GameControl GCtrl;
    private GameObject slot;
    private int row, pos;

    public int Pos { get { return pos; }}

    void Awake()
    {
        if(instance == null)
            instance = this;
        else Destroy(this.gameObject);
    }

    void Start()
    {
        GCtrl = GameControl.instance;
    }

    private void BorrowNumber()
    {
        int prev = pos - 1;
        int value = GCtrl.Slots[row][pos].SlotText.ToInt(); 
        GCtrl.TermsResult[prev] += 10;
        GCtrl.Slots[0][prev].SlotText = GCtrl.Slots[0][prev].SlotText.Insert(0, "1");
        int newValueAfterBorrow = (row == 0)? value - 1 : value + 1; // Na primeira row = --1
        if(newValueAfterBorrow <= 10)                                // Na segunda row = ++1
            GCtrl.Slots[row][pos].SlotText = newValueAfterBorrow.ToString();
        GCtrl.TermsResult[pos]--;
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        parentSlot = transform.parent.GetComponent<InputSlot>();
        row = parentSlot.row;
        pos = parentSlot.pos;
    }

    public void OnStrokeDrag()
    {
        BorrowNumber();
    }
}