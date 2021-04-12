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
    public void Pressed()
    {
        LeanTween.scale(gameObject, buttonScale, buttonAnimationTime).setEase(buttonAnimationEase).setOnComplete(() =>
        {
            LeanTween.scale(gameObject, new Vector3(1, 1, 1), buttonAnimationTime).setEase(buttonAnimationEase)
            .setOnComplete(() =>
            {
                Invoke("function",0);
            });
        });
    }
}
