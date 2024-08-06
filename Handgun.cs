using System.Collections;
using UnityEngine;

public class DualWieldHandgun : MonoBehaviour
{
    [Header("Primary Weapon Settings")]
    public Camera cam;
    public float giveDamage = 10f;
    public float shootingRange = 100f;
    public float fireRate = 10f;
    private float nextTimeToShoot = 0f;

    [Header("Primary Ammunition")]
    private int maximumAmmunition = 25;
    public int mag = 10;
    private int presentAmmunition;
    public float reloadingTime = 4.3f;
    private bool isReloading = false;

    [Header("Secondary Weapon Settings")]
    public float secondaryFireRate = 10f;
    private float nextTimeToShootLeftHand = 0f;

    [Header("Secondary Ammunition")]
    private int maximumAmmunitionLeftHand = 25;
    public int magLeftHand = 10;
    private int presentAmmunitionLeftHand;
    public float reloadingTimeLeftHand = 4.3f;
    private bool isReloadingLeftHand = false;

    [Header("Effects")]
    public ParticleSystem muzzleSpark;
    public GameObject metalEffect;

    [Header("Sounds & UI")]
    public AudioSource audioSource;
    public AudioClip shootingSound;
    public AudioClip reloadSound;
    public GameObject AmmoOutUI;

    [Header("Animator")]
    public Animator animator;

    [Header("Player Movement")]
    public CharacterController characterController;
    public float playerSpeed = 1.1f;
    public float playerSprintSpeed = 5f;
    public Transform playerCamera;
    public float gravity = -9.81f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    private Vector3 velocity;
    private bool isGrounded;
    public float jumpHeight = 1f;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        presentAmmunition = maximumAmmunition;
        presentAmmunitionLeftHand = maximumAmmunitionLeftHand;
        AmmoOutUI.SetActive(false);
    }

    void Update()
    {
        if (isReloading || isReloadingLeftHand)
            return;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        HandleMovement();
        HandleRotation();

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetBool("IdleAim", false);
            animator.SetTrigger("Jump");
        }
        else if (!isGrounded)
        {
            animator.SetBool("IdleAim", true);
            animator.ResetTrigger("Jump");
        }

        if (Input.GetButton("Sprint"))
        {
            Sprint();
        }
        else
        {
            animator.SetBool("IdleAim", true);
            animator.SetBool("RunForward", false);
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        if (Input.GetButtonDown("Fire1") && Time.time >= nextTimeToShoot && presentAmmunition > 0)
        {
            nextTimeToShoot = Time.time + 1f / fireRate;
            Shoot();
        }

        if (Input.GetButtonDown("Fire2") && Time.time >= nextTimeToShootLeftHand && presentAmmunitionLeftHand > 0)
        {
            nextTimeToShootLeftHand = Time.time + 1f / secondaryFireRate;
            ShootLeftHand();
        }

        if (presentAmmunition <= 0 && mag > 0 && !isReloading)
        {
            StartCoroutine(Reload());
        }

        if (presentAmmunitionLeftHand <= 0 && magLeftHand > 0 && !isReloadingLeftHand)
        {
            StartCoroutine(ReloadLeftHand());
        }
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            Vector3 moveDirection = playerCamera.forward * vertical + playerCamera.right * horizontal;
            characterController.Move(moveDirection.normalized * (Input.GetButton("Sprint") ? playerSprintSpeed : playerSpeed) * Time.deltaTime);

            animator.SetBool("WalkForward", true);
            animator.SetBool("RunForward", Input.GetButton("Sprint"));
        }
        else
        {
            animator.SetBool("WalkForward", false);
            animator.SetBool("RunForward", false);
        }
    }

    void Sprint()
    {
        animator.SetBool("IdleAim", false);
        animator.SetBool("RunForward", true);
    }

    void HandleRotation()
    {
        float horizontalRotation = Input.GetAxis("Mouse X") * playerSpeed;
        Vector3 playerRotation = transform.eulerAngles;
        playerRotation.y += horizontalRotation;
        transform.eulerAngles = playerRotation;
    }

    void Shoot()
    {
        presentAmmunition--;
        muzzleSpark.Play();
        audioSource.PlayOneShot(shootingSound);

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, shootingRange))
        {
            Debug.Log(hit.transform.name);
            Instantiate(metalEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }

        AmmoOutUI.SetActive(presentAmmunition <= 0);
        animator.SetBool("HasAmmo", presentAmmunition > 0);
    }

    IEnumerator Reload()
    {
        isReloading = true;
        audioSource.PlayOneShot(reloadSound);
        animator.SetTrigger("Reload");
        yield return new WaitForSeconds(reloadingTime);

        int ammoToLoad = Mathf.Min(maximumAmmunition - presentAmmunition, mag);
        presentAmmunition += ammoToLoad;
        mag -= ammoToLoad;
        isReloading = false;
        AmmoOutUI.SetActive(false);
    }

    IEnumerator ReloadLeftHand()
    {
        isReloadingLeftHand = true;
        audioSource.PlayOneShot(reloadSound);
        // Ensure you have a separate animation trigger for left hand reload if needed
        // animator.SetTrigger("ReloadLeftHand");
        yield return new WaitForSeconds(reloadingTimeLeftHand);

        int ammoToLoad = Mathf.Min(maximumAmmunitionLeftHand - presentAmmunitionLeftHand, magLeftHand);
        presentAmmunitionLeftHand += ammoToLoad;
        magLeftHand -= ammoToLoad;
        isReloadingLeftHand = false;
        // Ensure you update any UI or state for the left-hand weapon's ammo here
        // AmmoOutUILeftHand.SetActive(false);
    }

    void ShootLeftHand()
    {
        presentAmmunitionLeftHand--;
        // Assume you have a separate particle system or similar effect for the left-hand gun
        // muzzleSparkLeftHand.Play();
        audioSource.PlayOneShot(shootingSound);

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, shootingRange))
        {
            Debug.Log($"[LeftHand] {hit.transform.name}");
            Instantiate(metalEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }

        // Update UI or logic specific to the left-hand weapon's ammo state
        // AmmoOutUILeftHand.SetActive(presentAmmunitionLeftHand <= 0);
        // animator.SetBool("HasAmmoLeftHand", presentAmmunitionLeftHand > 0);
    }
}
