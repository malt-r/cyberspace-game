using UnityEngine;

namespace Assets.Weapons
{
    public abstract class BaseWeapon: MonoBehaviour,Weapon
    {
        public WeaponType Type { get; }
        
        public Transform Owner { get => owner; protected set => owner = value; }
        [SerializeField]
        private Transform owner;

        [SerializeField]
        protected float damage =10f;
        public float Damage => damage;
        
        [SerializeField]
        protected float useCooldown =10f;
        public float UseCooldown => useCooldown;
        
        public abstract void Use();
    }
}