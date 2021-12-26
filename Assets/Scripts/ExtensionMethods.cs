using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    // Extension Methods pra que as strings retornem seu valor em inteiro.
    public static int ToInt(this string str)
    {
        int result = 0;
        int.TryParse(str, out result);
        return result;
    }

    public static int ToInt(this char c)
    {
        int result = 0;
        if(char.IsNumber(c))
            int.TryParse(c.ToString(), out result);
        return result;
    }

    // Contador de slots ativos pra cada array de Input Slots.
    public static int ActivesSlotsCount(this InputSlot[] slots)
    {
        int counter = 0;
        for(int i = 0; i < slots.Length; i++)
        {
            if(slots[i].gameObject.activeSelf)
                counter++;
        }
        return counter;
    }

    public static string GetRawValue(this InputSlot[] slotArray)
    {
        string result = string.Empty;
        for (int i = slotArray.ActivesSlotsCount() - 1; i >= 0 ; i--)
        {
            result += slotArray[i].SlotText;
        }
        return result;
    }
}