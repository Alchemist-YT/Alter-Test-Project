using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseScreen : MonoBehaviour
{
    [SerializeField] protected GameObject screen;
    public void Show()
    {
        screen.SetActive(true);
    }
    public void Hide()
    {
        screen.SetActive(false);
    }
}
