using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class change_shoe : MonoBehaviour
{
    public static int index = 0;
    public static bool animation_flag = false;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void click_change_shoe()
    {
        index = (index + 1) % 5;
    }

    public void click_right()
    {
        index = (index + 1) % 5;
        animation_flag = true;
    }

    public void click_left()
    {
        if (index == 0)
            index = 4;
        else
            index = (index - 1) % 5;
        animation_flag = true;
    }
}