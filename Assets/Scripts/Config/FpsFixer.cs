using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsFixer
{
    public static void FixFrameRate()
    {
        Application.targetFrameRate = 30;
    }
}
