using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float jumpForce = 10f;
    public float gravityMultiplier = 1f;
    public Rigidbody rb;
    public Animator playerAnim;
    public AudioClip jumpSfx;
    public AudioClip crashSfx;
    public AudioSource playerAudio;
    public ParticleSystem explosionFx;
    public ParticleSystem dirtFx;

    public TextMeshProUGUI immuneCountdownText;
    public GameObject immuneTextWorld;

    public float boostedSpeed = 20f;
    public float powerUpDuration = 5f;

    private InputAction jumpAction;
    private bool isOnGround = true;
    public bool IsGameOver = false;

    private bool isImmune = false;
    private float immuneTimer;
    private bool showImmuneText = false;
    private float originalSceneSpeed;
    private MoveLeft[] allMoveScripts;
    private MoveLeft[] onlySceneScripts;

    public int maxHP = 3;
    private int currentHP;

    public GameObject[] heartIcons;

    // === เพิ่มระบบระยะทาง ===
    public TextMeshProUGUI distanceText;
    private float distanceTraveled = 0f;
    private float sceneSpeed = 5f; // ควรตรงกับ speed ของ MoveLeft.cs

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    void Start()
    {
        Physics.gravity = new Vector3(0, -9.81f * gravityMultiplier, 0);
        playerAnim.SetFloat("Speed_f", 1.0f);
        currentHP = maxHP;
        UpdateHearts();

        // หาความเร็วฉากจาก MoveLeft
        var sceneObj = GameObject.FindGameObjectWithTag("Scene");
        if (sceneObj != null)
        {
            var moveLeft = sceneObj.GetComponent<MoveLeft>();
            if (moveLeft != null) sceneSpeed = moveLeft.speed;
        }
    }

    void Update()
    {
        if (jumpAction.triggered && isOnGround && !IsGameOver)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isOnGround = false;
            playerAnim.SetTrigger("Jump_trig");
            playerAudio.PlayOneShot(jumpSfx);
            dirtFx.Stop();
        }

        if (showImmuneText)
        {
            immuneTimer -= Time.deltaTime;
            immuneCountdownText.text = "Power Up : " + Mathf.CeilToInt(immuneTimer);

            if (immuneTimer <= 0)
            {
                immuneCountdownText.text = "";
                showImmuneText = false;
            }
        }

        // === อัปเดตระยะทาง ===
        if (!IsGameOver)
        {
            distanceTraveled += sceneSpeed * Time.deltaTime;
            distanceText.text = Mathf.FloorToInt(distanceTraveled) + " m";
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;
            dirtFx.Play();
        }
        else if (collision.gameObject.CompareTag("Obstacle") && !isImmune)
        {
            TakeDamage(1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PowerUp") && !isImmune)
        {
            Destroy(other.gameObject);
            ActivatePowerUp();
        }
        else if (other.CompareTag("Health"))
        {
            Destroy(other.gameObject);
            AddHealth(1);
        }
    }

    public void ActivatePowerUp()
    {
        isImmune = true;
        immuneTimer = powerUpDuration;
        showImmuneText = true;

        allMoveScripts = FindObjectsOfType<MoveLeft>();
        onlySceneScripts = Array.FindAll(allMoveScripts, obj => obj.CompareTag("Scene"));

        foreach (var move in onlySceneScripts)
        {
            originalSceneSpeed = move.speed;
            move.speed = boostedSpeed;
        }

        sceneSpeed = boostedSpeed;

        if (immuneTextWorld != null)
            immuneTextWorld.SetActive(true);

        Invoke(nameof(DeactivatePowerUp), powerUpDuration);
    }

    void DeactivatePowerUp()
    {
        isImmune = false;

        foreach (var move in onlySceneScripts)
        {
            move.speed = originalSceneSpeed;
        }

        sceneSpeed = originalSceneSpeed;

        if (immuneTextWorld != null)
            immuneTextWorld.SetActive(false);
    }

    void TakeDamage(int amount)
    {
        if (IsGameOver || isImmune) return;

        currentHP -= amount;
        UpdateHearts();

        if (currentHP <= 0)
        {
            IsGameOver = true;
            playerAnim.SetBool("Death_b", true);
            playerAudio.PlayOneShot(crashSfx);
            explosionFx.Play();
            dirtFx.Stop();

            if (immuneTextWorld != null)
                immuneTextWorld.SetActive(false);
        }
        else
        {
            playerAudio.PlayOneShot(crashSfx);
        }
    }

    public void AddHealth(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        UpdateHearts();
    }

    void UpdateHearts()
    {
        for (int i = 0; i < heartIcons.Length; i++)
        {
            heartIcons[i].SetActive(i < currentHP);
        }
    }
}
