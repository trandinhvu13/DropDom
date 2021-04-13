using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Gui;
using UnityEngine;
using UnityEngine.Events;
using LeanSelectable = Lean.Touch.LeanSelectable;

[RequireComponent(typeof(LeanSelectable))]
public class Button : MonoBehaviour
{
    [SerializeField] private string name;
    public UnityEvent function;
    [SerializeField] private float buttonAnimationTime;
    [SerializeField] private LeanTweenType buttonAnimationEase;
    [SerializeField] private Vector3 buttonScale;
    [SerializeField] private bool canPress = true;

    private void OnEnable()
    {
        GameEvents.Instance.OnChangeCanPress += ChangeCanPress;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnChangeCanPress -= ChangeCanPress;
    }

    private void ChangeCanPress(string name, bool canPress)
    {
        if (name == this.name)
        {
            this.canPress = canPress;
        }
    }
    public void Pressed()
    {
        if(!canPress) return;
        if (LeanTween.isTweening(gameObject)) return;
        LeanTween.scale(gameObject, buttonScale, buttonAnimationTime).setEase(buttonAnimationEase).setOnComplete(() =>
        {
            LeanTween.scale(gameObject, new Vector3(1, 1, 1), buttonAnimationTime).setEase(buttonAnimationEase)
                .setOnComplete(() => { function.Invoke(); });
        });
    }
}