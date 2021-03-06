﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerHealth : MonoBehaviour
{
    [Header("Settings")]
    public float damageAmount;
    public int effectDuration;

    [Header("Starting Variables")]
    public float maxHealth;

    [Header("Runtime Variables")]
    public float currentHealth;
    public bool isAlive;
    
    private GameObject damageText;
    private float damageDelay;
    private float healDelay;
    private GameObject healText;

    private gameManager manager;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("gameManager").GetComponent<gameManager>();
        maxHealth = manager.playerMaxHealth;

        currentHealth = maxHealth;
        isAlive = true;

        damageText = GameObject.FindGameObjectWithTag("damageText");
        healText = GameObject.FindGameObjectWithTag("healText");
        damageText.SetActive(false);
        healText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (damageDelay > 0) {
        	damageDelay-=Time.deltaTime;
        }
        if (damageDelay <= 0) {
        	damageText.SetActive(false);
        }

        if (healDelay > 0) {
        	healDelay-=Time.deltaTime;
        }
        if (healDelay <= 0) {
        	healText.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Y)) {
        	TakeDamage();
        }
    }

    void TakeDamage() {
    	currentHealth -= damageAmount;

    	if (damageDelay > 0) {
    		damageText.SetActive(false);
    	}
    	damageText.SetActive(true);
    	damageText.GetComponent<Animator>().Play("damageEffect");
    	
    	damageDelay = effectDuration;

    	if (currentHealth <= 0) {
    		currentHealth = 0;
    		Dead();
    	}
    }

    void Dead() {
    	isAlive = false;
        manager.GameOver();
    }

    public void AddHealth(int amount) {
    	currentHealth += amount;

    	if (healDelay > 0) {
    		healText.SetActive(false);
    	}
    	healText.SetActive(true);
    	healText.GetComponent<Animator>().Play("healEffect");
    	
    	healDelay = effectDuration;
    }

    void OnTriggerEnter(Collider other) {
    	if (other.gameObject.CompareTag("catAttackCollider")) {
    		TakeDamage();
    	}
    }
}
