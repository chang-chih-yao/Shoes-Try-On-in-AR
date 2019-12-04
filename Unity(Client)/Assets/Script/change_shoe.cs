using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class change_shoe : MonoBehaviour
{
    public static int index = 0;

    public void click_change_shoe()
    {
        index = (index + 1) % 5;
    }
}
