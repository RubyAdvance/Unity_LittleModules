using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragButtonManager
{
    private static DragButtonManager _instance = new DragButtonManager();
    public static DragButtonManager instance
    {
        get
        {
            return _instance;
        }
    }



    public int curPointerId {get;set;} = -1;

    public int clickCount {get;set;} = 0;

    public int dragButtonId {get;set;} = 1;


    public void ResetData()
    {
        curPointerId = -1;
        clickCount = 0;
        dragButtonId = 1;
    }




}
