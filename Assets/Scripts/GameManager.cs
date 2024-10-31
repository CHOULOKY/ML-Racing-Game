using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get {
            if (instance == null) return null;
            return instance;
        }
    }

    public Checkpoints checkpoints;

    private void Awake()
    {
        instance = this;

        checkpoints = checkpoints != null ? checkpoints : FindAnyObjectByType<Checkpoints>();
    }
}
