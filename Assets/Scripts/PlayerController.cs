using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    //for   AI
    public bool AI; //  AI ON/OFF

    // public bool Turret_Turn; //Turn ON/OFF
    public Transform[] targetPointsPos; //(enemy AI) Points for positions 
    private byte sel_ltargetPointPos; //(enemy AI) selected targetPointPos in array

    public float m_Speed = 8.0f; // How fast the tank moves forward and back.
    public float m_TurnSpeed = 180f; // How fast the tank turns in degrees per second.
    public int currentHealth = 50; //The tank's current health point total

    public float m_PitchRange = 0.2f; // The amount by which the pitch of the engine noises can vary.


    private float TrackLeft_y;
    private float TrackRight_y;

    public AudioClip tank_idle;
    public AudioClip tankDead;

    private string m_MovementAxisName; // The name of the input axis for moving forward and back.
    private string m_TurnAxisName; // The name of the input axis for turning.
    private Rigidbody m_Rigidbody; // Reference used to move the tank.
    private float m_MovementInputValue; // The current value of the movement input.
    private float m_TurnInputValue; // The current value of the turn input.
    private float m_OriginalPitch; // The pitch of the audio source at the start of the scene.
    private AudioSource m_MovementAudio; // Reference to the audio source used to play engine sounds.
    private Animator animator;


    private WaitForSeconds shotDuration = new WaitForSeconds(15f); // WaitForSeconds hide object 
    private ParticleSystem m_ExplosionParticles; //  the particles that will play on explosion.
    string animDo; //the animation used now


    public Transform Turret;
    public Transform Barrel;
    public Transform GunEnd;

    // private Transform TargetForTurn;
    // private Vector3 TargetForTurnOld;
    // private float TargetForTurnTimer;
    public float EnemyRangeFire = 50;
    public Rigidbody shell;

    public float FireRate = 0.5f; // Number in seconds which controls how often the player can fire
    public float speedShell = 80f;


    private float nextFire; // Float to store the time the player will be allowed to fire again, after firing
    private Vector3 pos_barrel;


    private bool m_dead;
    private bool EnemyFire;

    private Quaternion target;
    public ParticleSystem m_smokeBarrel;
    public ParticleSystem m_smokeDead;
    public ParticleSystem m_tankExplosion;
    public AudioSource m_AudioSource;
    public AudioClip soundFire;


    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        pos_barrel = Barrel.transform.localPosition;

        m_MovementAudio = GetComponent<AudioSource>();

        m_MovementAudio.clip = tank_idle;
    }


    private void OnEnable()
    {
        // Also reset the input values.
        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;
    }


    private void OnDisable()
    {
    }


    private void Start()
    {
        // The axes names are based on player number.
        m_MovementAxisName = "Vertical";
        m_TurnAxisName = "Horizontal";

        m_OriginalPitch = m_MovementAudio.pitch;
    }


    private void Update()
    {
        if (!IsOwner) return;
        if (m_dead)
            return;

        if (!AI && Input.GetButtonDown("Fire1")) EnemyFire = true;

        if (EnemyFire && nextFire <= 0)
        {
            EnemyFire = false;
            nextFire = FireRate;
            Fire();
            m_smokeBarrel.Play();
            m_AudioSource.PlayOneShot(soundFire);
        }

        if (nextFire > 0)
        {
            nextFire -= 0.01f;
            Barrel.transform.localPosition = new Vector3(Barrel.transform.localPosition.x, pos_barrel.y + nextFire / 2,
                Barrel.transform.localPosition.z);
        }


        // Store the value of both input axes.
        m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
        m_TurnInputValue = Input.GetAxis(m_TurnAxisName);

        EngineAudio();

        if (animDo != "Idle" && m_MovementInputValue == 0 && m_TurnInputValue == 0)
        {
            animDo = "Idle";
            animator.SetBool("Idle" + (int)Random.Range(1, 4), true);
        }

        if (Input.GetButtonDown("Fire2"))
        {
            GameObject sphereGameObject = Instantiate(spherePrefab, transform.position + Vector3.up * 10, Quaternion.identity);
            NetworkObject networkObject = sphereGameObject.GetComponent<NetworkObject>();
            Debug.Log(networkObject);
            networkObject.Spawn(true);
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (m_dead)
        {
            Vector3 pos = transform.position;
            pos.y -= 0.002f;
            transform.position = pos;
            return;
        }

        // Adjust the rigidbodies position and orientation in FixedUpdate.
        Move();
        Turn();
    }

    private void EngineAudio()
    {
        // If there is no input (the tank is stationary)...
        if (Mathf.Abs(m_MovementInputValue) == 0 && Mathf.Abs(m_TurnInputValue) < 0.1f)
        {
            // ... change the clip to idling and play it.
            m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
        }
        else
            m_MovementAudio.pitch = 1 + Mathf.Abs(m_MovementInputValue);
    }

    private void Move()
    {
        // Create a vector in the direction the tank is facing with a magnitude based on the input, speed and the time between frames.
        Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;

        // Apply this movement to the rigidbody's position.
        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
        if (m_TurnInputValue == 0)
            if (m_MovementInputValue > 0)
            {
                //Movement of the texture of the tank caterpillar
                if (animDo != "Move")
                {
                    animDo = "Move";
                    animator.SetBool("MoveForwStart", true);
                }
            }
            else if (m_MovementInputValue < 0)
            {
                //Movement of the texture of the tank caterpillar
                if (animDo != "Move")
                {
                    animDo = "Move";
                    animator.SetBool("MoveBackStart", true);
                }
            }
    }


    private void Turn()
    {
        // Determine the number of degrees to be turned based on the input, speed and time between frames.
        float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;
        if (turn != 0)
        {
            // Movement of the texture of the tank caterpillar
        }

        // Make this into a rotation in the y axis.
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

        // Apply this rotation to the rigidbody's rotation.
        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);

        if (m_TurnInputValue > 0)
        {
            //Movement of the texture of the tank caterpillar
            if (animDo != "TurnRight")
            {
                animDo = "TurnRight";
                if (m_MovementInputValue >= 0) animator.SetBool("TurnRight", true);
                else animator.SetBool("TurnLeft", true);
            }
        }
        else if (m_TurnInputValue < 0)
        {
            //Movement of the texture of the tank caterpillar
            if (animDo != "TurnLeft")
            {
                animDo = "TurnLeft";
                if (m_MovementInputValue >= 0) animator.SetBool("TurnLeft", true);
                else animator.SetBool("TurnRight", true);
            }
        }
    }

    //Collider col = Physics.OverlapBox(enemyCheck.position, 0.6f, LayerEnemy);
    void OnTriggerEnter(Collider col)
    {
        // if (m_dead) return;
        // if (col.gameObject.tag == "Shell")
        // {
        //     SCT_Shell shell = col.GetComponent<SCT_Shell>();
        //     Damage(shell.shellDamage);
        //
        //     if (currentHealth > 0)
        //     {
        //         var a = (int)Random.Range(1, 5);
        //         if (a == 1) animator.SetBool("HitLeft", true);
        //         if (a == 2) animator.SetBool("HitRight", true);
        //         if (a == 3) animator.SetBool("HitForw", true);
        //         if (a == 4) animator.SetBool("HitBack", true);
        //         if (a == 5) animator.SetBool("HitStrong", true);
        //         animDo = "Hit";
        //     }
        // }
    }


    public void Damage(int damageAmount)
    {
        //subtract damage amount when Damage function is called
        currentHealth -= damageAmount;


        //Check if health has fallen below zero
        if (currentHealth <= 0)
        {
            //DEAD
            m_dead = true;
            animator.SetBool("Dead" + (int)Random.Range(1, 5), true);
            animDo = "Dead";
            m_MovementAudio.loop = false;
            m_MovementAudio.pitch = 1;
            m_MovementAudio.clip = tankDead;
            m_MovementAudio.Play();
            GetComponent<BoxCollider>().enabled = false;
            transform.gameObject.tag = "Respawn";
            Destroy(GetComponent<Rigidbody>());

            m_smokeDead.Play();
            m_tankExplosion.Play();


            StartCoroutine(HideTank());
        }
    }

    void LateUpdate()
    {
        if (m_dead) return;
        Turret.transform.rotation = Quaternion.Euler(transform.eulerAngles.x - 90, transform.eulerAngles.y,
            transform.eulerAngles.z);
    }

    [SerializeField] private GameObject spherePrefab;

    void Fire() //shot
    {
        Rigidbody shellInstance = Instantiate(shell, GunEnd.position, Turret.rotation) as Rigidbody;
        shellInstance.velocity = speedShell * -Turret.transform.up;
        shellInstance.GetComponent<NetworkObject>().Spawn(true);
    }

    private IEnumerator HideTank()
    {
        //Wait for 15 seconds
        yield return shotDuration;
        //hide tank
        gameObject.SetActive(false);
    }
}
