using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeParticle : MonoBehaviour
{
    [SerializeField] private int row;
    [SerializeField] private ParticleSystem smoke;

    private void OnEnable()
    {
        GameEvents.Instance.OnPlaySmoke += PlayParticle;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnPlaySmoke -= PlayParticle;
    }

    void PlayParticle(int row)
    {
        if (row != this.row) return;
        smoke.Play(true);
    }
}