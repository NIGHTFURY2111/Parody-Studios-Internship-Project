using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointLogic : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager manager = FindObjectOfType<GameManager>();
            manager?.RegisterCheckpoint();

            Debug.Log("Checkpoint reached");

            Destroy(gameObject);
        }
    }
}
