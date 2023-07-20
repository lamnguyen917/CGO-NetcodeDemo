using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed = 50;
    public float lifeTime = 2.0f;
    public int shellDamage = 10;
    public ParticleSystem m_ExplosionParticles; // Reference to the particles that will play on explosion.
    public AudioSource m_ExplosionAudio;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.05f);
        GetComponent<BoxCollider>().enabled = true;
        GetComponent<SphereCollider>().enabled = true;
        yield return new WaitForSeconds(lifeTime);
        if (gameObject != null) Destroy(gameObject);
        DeSpawnServerRpc();
    }

    [ServerRpc]
    void DeSpawnServerRpc()
    {
        var networkObject = GetComponent<NetworkObject>();
        networkObject.Despawn();
    }

    private void Update()
    {
        var direction = -transform.up * speed * Time.deltaTime;
        transform.position += direction;
    }

    private void FixedUpdate()
    {
        // rb.velocity = -transform.up * speed;
        // UIManager.Instance.SetStatus($"{rb.velocity} - {speed} - {transform.up}");
    }

    void OnTriggerEnter(Collider col)
    {
        Debug.Log(col.tag + col.gameObject.name);
        // // Play the particle system.
        // m_ExplosionParticles.Play();
        // // Play the explosion sound effect.
        // m_ExplosionAudio.Play();
        // GetComponent<Rigidbody>().velocity = Vector3.zero;
        // GetComponent<Collider>().enabled = false;
        // GetComponent<Renderer>().enabled = false;
        // Destroy(gameObject, 2);
    }
}
