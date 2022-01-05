using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;

public class RNG
{
    private static readonly RNGCryptoServiceProvider _generator = new RNGCryptoServiceProvider();

    public static long GetRand()
    {
        byte[] buffer = new byte[8];

        _generator.GetNonZeroBytes(buffer);

        var rand = BitConverter.ToInt64(buffer, 0);
        
        //double asciiValueOfRandomCharacter = Convert.ToDouble(rand);

        // We are using Math.Max, and substracting 0.00000000001, 
        // to ensure "multiplier" will always be between 0.0 and .99999999999
        // Otherwise, it's possible for it to be "1", which causes problems in our rounding.
        //double multiplier = Math.Max(0, ((asciiValueOfRandomCharacter / 255d) - 0.00000000001d));

        //// We need to add one to the range, to allow for the rounding done with Math.Floor
        //long range = maximumValue - minimumValue + 1;

        //double randomValueInRange = Math.Floor(multiplier * range);

        return rand; //(long)(minimumValue + randomValueInRange);
    }
}
