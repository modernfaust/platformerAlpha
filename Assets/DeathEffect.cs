﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }
    void EffectEnd()
    {
        Destroy(gameObject);
    }
}
