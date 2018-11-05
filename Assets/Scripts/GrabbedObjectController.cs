using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;

public class GrabbedObjectController : MonoBehaviour
{
    private bool isGrabbed;
    private Outline[] outlines;

    void Start()
    {
        outlines = GetComponentsInChildren<Outline>();
    }

    void Update()
    {
        if (transform.parent != null)
        {
            //Debug.Log("I have a parent");
            SetIsGrabbed(true);
            SetOutline(false);
        }
        else
        {
            //Debug.Log("I do not have a parent");
            SetIsGrabbed(false);
        }
    }

    public void SetIsGrabbed(bool state)
    {
        isGrabbed = state;
    }

    public void SetOutline(bool state)
    {
        foreach (Outline outline in outlines)
        {
            outline.enabled = state;
        }
    }

    public bool GetIsGrabbed()
    {
        return isGrabbed;
    }
}
