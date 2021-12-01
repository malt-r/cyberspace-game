using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Weapons
{
    public class Scanner : BaseWeapon
    {
        [SerializeField]
        private List<BaseWeapon> modes;
        
        public WeaponType Type { get; private set; }
        
        private ParticleSystem overheatParticleSystem;
        
        public float condition = 100f;
        public float maxCondition = 100f;
        public bool overHeated = false;
        
        public float regenerationRate = 10f;
        public float cooldownTime = 5;
        
        private float deltaTime;
        public bool shooted = false;

        public Transform modesTransform;
        
        [SerializeField]
        private Transform modeIndicator;

        [SerializeField]
        private int currentMode = 0;

        public void SwitchMode(int index)
        {
            currentMode += index;
            if (currentMode == modes.Count)
            {
                currentMode = 0;
            }

            if (currentMode < 0)
            {
                currentMode = modes.Count - 1;
            }

            modeIndicator.GetComponent<Renderer>().material = modes[currentMode].GetComponent<BaseWeapon>().Material;
        }
        
        private void handleHeatingAndCooling()
        {
            if (shooted && !overHeated)
            {
                condition -= Time.deltaTime*modes[currentMode].UseCost;
                if (condition < 0)
                {
                    condition = 0;
                    overHeated = true;
                    StartCoroutine(StartWeaponCooldown());
                }
            }
            else if(!shooted && condition<maxCondition && !overHeated)
            {
                condition += Time.deltaTime*regenerationRate;
                if (condition > maxCondition)
                {
                    condition = maxCondition;
                }
            }
       
               
        }
    

        private void UpdateOverheatDrawParticleSystem()
        {
            if (overHeated && overheatParticleSystem.isStopped)
            {
                overheatParticleSystem.Play();
            }
            else if(!overHeated &&  overheatParticleSystem.isPlaying)
            {
                overheatParticleSystem.Stop();
            }
        }

        IEnumerator StartWeaponCooldown()
        {
            shooted = false;
            yield return new WaitForSeconds(cooldownTime); 
            overHeated = false;
        }
        
        private void Awake()
        {
            overheatParticleSystem = GetComponentInChildren<ParticleSystem>();
            overheatParticleSystem.Stop();
            Type = WeaponType.SCANNER;
            deltaTime = atackSpeed+1;
        }

        public override void Use()
        {
            shooted = true;
        }
        
        public void Update()
        {
            deltaTime += Time.deltaTime;
            handleHeatingAndCooling();
            if (shooted )
            {
                shooted = false;
                if(!overHeated){
                    deltaTime = 0f;
                    modes[currentMode].Use();
                }
            }

            UpdateOverheatDrawParticleSystem();
        }

        public override void InitWeapon(Transform owner, Transform camera, Transform firepoint)
        {
            base.InitWeapon(owner,camera,firepoint);
            modes[currentMode].InitWeapon(owner,camera,firepoint);
        }

        public void AddSkill(BaseWeapon weapon)
        {
            weapon.InitWeapon(Owner,Camera,Firepoint);
            weapon.transform.parent = modesTransform;
            weapon.enabled = true;
            modes.Add(weapon);
        }
    }
}