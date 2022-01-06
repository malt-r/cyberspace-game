using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Weapons
{
    public abstract class BaseWeapon: MonoBehaviour,Weapon
    {
        public WeaponType Type { protected set; get; }

        public Transform Owner { get; protected set; }
        public Transform Firepoint { get;protected set; }
        public Transform Camera { get; protected set;}

        [SerializeField]
        private float useCost;
        public float UseCost { get=>useCost; set=>useCost=value; }

        [SerializeField]
        public Material Material;

        [SerializeField]
        protected float damage =10f;
        public virtual float Damage => damage;
        
        [SerializeField]
        protected float atackSpeed =10f;
        public float AttackSpeed => atackSpeed;
        
        public abstract void Use();

        protected AudioSource audioSource;

        protected virtual void PlayUseSound()
        {
            audioSource.volume=0.5f;
            audioSource.PlayOneShot(audioSource.clip);
        }
        public virtual void InitWeapon(Transform owner, Transform camera, Transform firepoint)
        {
            Owner = owner;
            Camera = camera;
            Firepoint = firepoint;
            audioSource = GetComponent<AudioSource>();
        }

        public abstract bool CanAttack();
        
        
    }
}