using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddWithoutsigns : MonoBehaviour
{

    public string AddFunct(int i, bool substract)
    {
        bool negative = false;
        string number = i.ToString();
        char lastNumber;
        int numberOfExtraDigits = 0;
        if (number.Length == 1)
        {
            lastNumber = number[numberOfExtraDigits];
        }
        else
        {
            numberOfExtraDigits = Convert.ToInt32(AddFunct(number.Length, true));
            lastNumber = (number[numberOfExtraDigits]);
            if ((int)Convert.ToChar(number[0]) == 45)
            {
                negative = true;
            }
        }
        number = number.Remove(numberOfExtraDigits);
        switch (lastNumber)
        {
            case '0':
                if (!negative)
                {
                    if (substract)
                    {
                        if (numberOfExtraDigits > 0)
                        {
                            lastNumber = '9';
                            number = AddFunct(Convert.ToInt32(number), true);
                        }
                        else
                        {
                            number = "m1";
                            return number.Replace('m', Convert.ToChar(45));
                        }
                    }
                    else
                    {
                        lastNumber = '1';
                    }
                }
                else
                {
                    if (substract)
                    {

                        lastNumber = '1';

                    }
                    else
                    {
                        lastNumber = '9';
                        number = AddFunct(Convert.ToInt32(number), false);
                    }
                }
                break;
            case '1':
                if (negative)
                {
                    if (substract)
                    {
                        lastNumber = '2';
                    }
                    else
                    {
                        return "0";
                    }

                }
                else
                {
                    lastNumber = substract ? '0' : '2';
                }
                break;
            case '2':
                lastNumber = negative ? substract ? '3' : '1' : substract ? '1' : '3';
                break;
            case '3':
                lastNumber = negative ? substract ? '4' : '2' : substract ? '2' : '4';
                break;
            case '4':
                lastNumber = negative ? substract ? '5' : '3' : substract ? '3' : '5';
                break;
            case '5':
                lastNumber = negative ? substract ? '6' : '4' : substract ? '4' : '6';
                break;
            case '6':
                lastNumber = negative ? substract ? '7' : '5' : substract ? '5' : '7';
                break;
            case '7':
                lastNumber = negative ? substract ? '8' : '6' : substract ? '6' : '8';
                break;
            case '8':
                lastNumber = negative ? substract ? '9' : '7' : substract ? '7' : '9';
                break;
            case '9':
                if (!negative)
                {
                    if (substract)
                    {
                        if (numberOfExtraDigits > 0)
                        {
                            lastNumber = '8';
                        }
                    }
                    else
                    {
                        lastNumber = '0';
                        number = AddFunct(Convert.ToInt32(number), false);
                    }
                }
                else
                {
                    if (substract)
                    {
                        lastNumber = '0';
                        number = AddFunct(Convert.ToInt32(number), true);
                    }
                    else
                    {
                        lastNumber = '8';
                    }
                }
                break;

        }
        return string.Concat(number, lastNumber.ToString());
    }
}
