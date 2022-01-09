using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Weapons
{
    public class Scanner : BaseWeapon
    {
        [SerializeField]
        private List<BaseWeapon> modes;

        private ParticleSystem overheatParticleSystem;
        
        public float condition = 100f;
        public float maxCondition = 100f;
        public bool overHeated = false;
        
        public float regenerationRate = 10f;
        public float cooldownTime = 5;
        private float timeSinceoverheat = 0f;
        
        private float deltaTime;
        [FormerlySerializedAs("shooted")] public bool wasShot = false;

        public Transform modesTransform;
        
        [SerializeField]
        private Transform modeIndicator;

        [SerializeField]
        private int currentMode = 0;
        public override float Damage => modes[currentMode].Damage;
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
            if (overHeated)
            {
                wasShot = false;
                if(timeSinceoverheat > cooldownTime){
                    overHeated = false;
                    timeSinceoverheat = 0;
                }

                timeSinceoverheat += Time.deltaTime;
                return;
            }
            if (wasShot && !overHeated)
            {
                condition -= Time.deltaTime*modes[currentMode].UseCost;
                if (condition < 0)
                {
                    condition = 0;
                    overHeated = true;
                    PlayOverheatSound();
                }
            }
            else if(!wasShot && condition<maxCondition && !overHeated)
            {
                condition += Time.deltaTime*regenerationRate;
                if (condition > maxCondition)
                {
                    condition = maxCondition;
                }
            }
       
               
        }

        public void Reset()
        {
            overHeated = false;
            timeSinceoverheat = 0;
            UpdateOverheatDrawParticleSystem();
            condition = maxCondition;

        }

        private void UpdateOverheatDrawParticleSystem()
        {
            if (overheatParticleSystem == null)
            {
                return;
            }
            
            if (overHeated && overheatParticleSystem.isStopped)
            {
                overheatParticleSystem.Play();
            }
            else if(!overHeated &&  overheatParticleSystem.isPlaying)
            {
                overheatParticleSystem.Stop();
            }
        }

        private void PlayOverheatSound()
        {
            if (!audioSource.isPlaying)
            {
                audioSource.volume = 0.5f;
                audioSource.PlayOneShot(audioSource.clip);
            }
        }

        private void Awake()
        {
            overheatParticleSystem = GetComponentInChildren<ParticleSystem>();
            overheatParticleSystem.Stop();
            Type = WeaponType.SCANNER;
            deltaTime = atackSpeed+1;
            audioSource = GetComponent<AudioSource>();
        }

        public override void Use()
        {
            wasShot = true;
        }
        
        public void LateUpdate()
        {
            deltaTime += Time.deltaTime;
            handleHeatingAndCooling();
            if (wasShot)
            {
                wasShot = false;
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

        public override bool CanAttack() { return true; }

        public void AddSkill(BaseWeapon weapon)
        {
            var alreadyExists= modes.Any(item => item.Type == weapon.Type);
            if (alreadyExists) { return;}
            weapon.InitWeapon(Owner,Camera,Firepoint);
            weapon.transform.parent = modesTransform;
            weapon.enabled = true;
            modes.Add(weapon);
        }
    }
}