using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [Header("Audio de Passos")]
    [SerializeField] private AudioSource passoAudioSource;
    [SerializeField] private AudioClip[] sonsDePasso;
    [SerializeField] private float intervaloPasso = 0.35f;
    [SerializeField][Range(0f, 1f)] private float volumePasso = 0.7f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private float timerPasso;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (passoAudioSource == null)
            passoAudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        rb.velocity = moveInput * moveSpeed;
        AtualizarSomPassos();
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
            moveInput = Vector2.zero;
            timerPasso = 0f;
            return;
        }

        moveInput = context.ReadValue<Vector2>();
        bool estaMovendo = moveInput.sqrMagnitude > 0.01f;

        animator.SetBool("isWalking", estaMovendo);
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
    }

    private void AtualizarSomPassos()
    {
        if (moveInput.sqrMagnitude <= 0.01f)
        {
            timerPasso = 0f;
            return;
        }

        timerPasso -= Time.deltaTime;
        if (timerPasso <= 0f)
        {
            TocarPasso();
            timerPasso = intervaloPasso;
        }
    }

    private void TocarPasso()
    {
        if (passoAudioSource == null || sonsDePasso == null || sonsDePasso.Length == 0)
            return;

        AudioClip clipEscolhido = sonsDePasso[Random.Range(0, sonsDePasso.Length)];
        if (clipEscolhido == null) return;

        passoAudioSource.pitch = Random.Range(0.92f, 1.08f);
        passoAudioSource.PlayOneShot(clipEscolhido, volumePasso);
        passoAudioSource.pitch = 1f;
    }
}