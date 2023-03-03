using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keepData : MonoBehaviour
{
    public int userid;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }
}
