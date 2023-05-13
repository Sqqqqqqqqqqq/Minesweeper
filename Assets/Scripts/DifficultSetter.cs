using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DifficultSetter : MonoBehaviour
{
    public int width;
    public int height;
    public int mines;
    public void StartLevelButton() 
    {
        DataHolder.width = width;
        DataHolder.height = height;
        DataHolder.mines = mines;
    }
}
