using UnityEngine;
using System.Collections;

public class gun_script : MonoBehaviour
{
    private ParticleSystem muzzleFire;
    public bool isFiring;
    private string weaponType;
    private float fireRate;
    private float sinceFired;
    private bool isReloading = false;
    private AudioSource reloadingSound;
    private AudioSource firingSound;
    private Object bulletProjectile;
    private int clipMax;
    private int clipCur;
    public bool playerWeapon = false;

    // Use this for initialization
    void Start()
    {
        weaponType = gameObject.name;
        isFiring = false;
        muzzleFire = gameObject.transform.FindChild("spawnPoint").GetComponent<ParticleSystem>();
        reloadingSound = gameObject.transform.FindChild("body").GetComponent<AudioSource>();
        firingSound = gameObject.transform.FindChild("spawnPoint").GetComponent<AudioSource>();

        // Adjusts stats based on the type of gun
        if (weaponType == "rifle_prop")
        {
            fireRate = 0.13f;
            bulletProjectile = Resources.Load("guns/rifleProjectile");
            reloadingSound.clip = Resources.Load("guns/rifle_Reload") as AudioClip;
            firingSound.clip = Resources.Load("guns/rifle_Fire") as AudioClip;
            clipMax = 48;
            clipCur = 48;
        }
        if (weaponType == "pistol_prop")
        {
            fireRate = 0.7f;
            bulletProjectile = Resources.Load("guns/pistolProjectile");
            reloadingSound.clip = Resources.Load("guns/pistol_Reload") as AudioClip;
            firingSound.clip = Resources.Load("guns/pistol_Fire") as AudioClip;
            clipMax = 12;
            clipCur = 12;
        }

    }

    void Update ()
    {
        // Increases time since last firing
        // Placed outside of fire control so that a new bullet can be fired immediately if left for a while
        sinceFired = sinceFired + Time.deltaTime;
        if (isFiring && !isReloading)
        {
            // This prevents the gun from firing infinite bullets per frame and also checks the gun has bullets left to fire
            if ((sinceFired > fireRate) && (clipCur > 0))
            {
                // If gun has waited long enough ("new bullet in chamber") then fire next bullet
                FireGun();
            }
        }
    }

    // Function that handles actually spawning a bullet, playing audio, etc
    private void FireGun()
    {
        clipCur--; // Removes one bullet from clip
        // Prevents being called immediately after
        sinceFired = 0;
        // Instantiate a new bullet
        GameObject cloneBullet = Instantiate(bulletProjectile, gameObject.transform.FindChild("spawnPoint").transform.position, gameObject.transform.rotation) as GameObject;
        if (playerWeapon)
        {
            cloneBullet.GetComponent<bullet_script>().isPlayerBullet = true;
        }
        else
        {
            cloneBullet.GetComponent<bullet_script>().isPlayerBullet = false;
        }
        firingSound.Play();
        muzzleFire.Emit(20);
        // If the gun is a pistol, only allows one shot before needing to be reclicked
        if (weaponType == "pistol_prop")
        {
            isFiring = false;
        }
    }

    // Functions for reloading; stops firing, plays sound, gives new ammo
    public IEnumerator ReloadGun(int newClip)
    {
        isReloading = true;
        isFiring = false;
        sinceFired = 0;
        reloadingSound.Play();
        yield return new WaitForSeconds(2);
        clipCur = newClip;
        isReloading = false;
        // If the gun is owned by the player, the update the UI to match
        if (playerWeapon)
        {
            GameObject.Find("playerCollider").GetComponent<player_Script>().updateAmmoUI();
        }
        else // If the gun is owned by NPC, tells them to "stop" reloading, allows firing again
        {
            gameObject.GetComponentInParent<npc_Script>().isReloading = false;
        }
    }


    // Returns the current number of bullets in clip to the player
    public int getClip()
    {
        return clipCur;
    }

    // Starts/stops the automatic firing process for rifles
    // Will not change if the gun is reloading
    public void changeFiring(bool newFiring)
    {
        if (!isReloading)
        isFiring = newFiring;
    }
}
