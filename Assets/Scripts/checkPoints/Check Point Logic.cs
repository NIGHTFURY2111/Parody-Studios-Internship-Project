using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointLogic : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager manager = FindObjectOfType<GameManager>();
            manager?.RegisterCheckpoint();

            Destroy(gameObject);
        }
    }
}
