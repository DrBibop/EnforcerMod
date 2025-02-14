﻿using Enforcer.Emotes;
using EntityStates.Nemforcer.Emotes;
using RoR2;
using UnityEngine;
using Modules;
using static RoR2.CameraTargetParams;
using Modules.Characters;

namespace EntityStates.Enforcer {
    public class BaseEmote : BaseState
    {
        private CharacterCameraParamsData emoteCameraParams =  new CharacterCameraParamsData() {
            maxPitch = 70,
            minPitch = -70,
            pivotVerticalOffset = EnforcerSurvivor.instance.bodyInfo.cameraParamsVerticalOffset,
            idealLocalCameraPos = emoteCameraPosition,
            wallCushion = 0.1f,
        };

        public static Vector3 emoteCameraPosition = new Vector3(0, -1.5f, -9f);

        private CameraParamsOverrideHandle camOverrideHandle;

        private Animator animator;
        private ChildLocator childLocator;
        private MemeRigController memeRig;
        private EnforcerWeaponComponent weaponComponent;

        //protected string soundString;
        //protected string animString;
        private float duration;
        //private float animDuration;

        private uint activePlayID;

        public override void OnEnter()
        {
            base.OnEnter();
            //init
            this.animator = base.GetModelAnimator();
            this.childLocator = base.GetModelChildLocator();
            this.memeRig = base.GetModelTransform().GetComponent<MemeRigController>();
            this.weaponComponent = base.GetComponent<EnforcerWeaponComponent>();

            //hide shit
            HideShit();

            CameraParamsOverrideRequest request = new CameraParamsOverrideRequest {
                cameraParamsData = emoteCameraParams,
                priority = 0,
            };

            camOverrideHandle = base.cameraTargetParams.AddParamsOverride(request, 0.5f);
        }


        protected void PlayEmote(string animString, string soundString = "", float animDuration = 0)
        {
            PlayEmote(animString, soundString, GetModelAnimator(), animDuration);
        }
        protected void PlayEmote(string animString, string soundString, Animator animator, float animDuration = 0)
        {
            if (animDuration >= 0 && this.duration != 0)
                animDuration = this.duration;

            if (duration > 0)
            {
                PlayAnimationOnAnimator(animator, "FullBody, Override", animString, "Emote.playbackRate", animDuration);
            }
            else
            {
                animator.SetFloat("Emote.playbackRate", 1f);
                PlayAnimationOnAnimator(animator, "FullBody, Override", animString);
            }

            if (!string.IsNullOrEmpty(soundString))
            {
                if (Modules.Skins.isEnforcerCurrentSkin(base.characterBody, "ENFORCER_DOOM_SKIN_NAME"))
                    soundString = Modules.Sounds.DOOM;

                activePlayID = Util.PlaySound(soundString, gameObject);
            };
        }

        protected void PlayFromMemeRig(string animString, string soundString = "", float animDuration = 0)
        {
            PlayFromMemeRig(animString, false, soundString, animDuration);
        }
        protected void PlayFromMemeRig(string animString, bool scaled, string soundString = "", float animDuration = 0)
        {
            memeRig.playMemeAnim(scaled);
            PlayEmote(animString, soundString, memeRig.MemeAnimator, animDuration);
        }

        public void HideShit(bool show = false)
        {
            if (weaponComponent)
            {

                if (!show)
                {
                    weaponComponent.HideEquips();
                }
                else
                {
                    weaponComponent.UnHideEquips();
                }
            }

            if (base.GetAimAnimator()) 
                base.GetAimAnimator().enabled = show;
            int aim = show ? 1 : 0;
            this.animator.SetLayerWeight(animator.GetLayerIndex("AimPitch"), aim);
            this.animator.SetLayerWeight(animator.GetLayerIndex("AimYaw"), aim);

            base.characterBody.hideCrosshair = !show;
        }

        public override void Update()
        {
            base.Update();
            
            //dance cancels lol
            if (base.isAuthority)
            {
                if (base.characterBody.baseNameToken == "ENFORCER_NAME")
                {
                    if (Input.GetKeyDown(Config.restKey.Value))
                    {
                        this.outer.SetInterruptState(new Rest(), InterruptPriority.Any);
                        return;
                    }
                    else if (Input.GetKeyDown(Config.saluteKey.Value))
                    {
                        this.outer.SetInterruptState(new EnforcerSalute(), InterruptPriority.Any);
                        return;
                    }
                    if (Input.GetKeyDown(Config.danceKey.Value))
                    {
                        this.outer.SetInterruptState(new DefaultDance(), InterruptPriority.Any);
                        return;
                    }
                    else if (Input.GetKeyDown(Config.runKey.Value))
                    {

                        this.outer.SetInterruptState(new FLINTLOCKWOOD(), InterruptPriority.Any);
                        return;
                    }
                    
                }
                else
                {
                    if (Input.GetKeyDown(Config.restKey.Value))
                    {
                        this.outer.SetInterruptState(new Rest(), InterruptPriority.Any);
                        return;
                    }
                    else if (Input.GetKeyDown(Config.saluteKey.Value))
                    {
                        this.outer.SetInterruptState(new Salute(), InterruptPriority.Any);
                        return;
                    }
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            bool endEmote = false;

            if (base.characterMotor)
            {
                if (!base.characterMotor.isGrounded) endEmote = true;
                //if (base.characterMotor.velocity != Vector3.zero) flag = true;
            }

            if (base.inputBank)
            {
                if (base.inputBank.skill1.down) endEmote = true;
                if (base.inputBank.skill2.down) endEmote = true;
                if (base.inputBank.skill3.down) endEmote = true;
                if (base.inputBank.skill4.down) endEmote = true;

                if (base.inputBank.moveVector != Vector3.zero) endEmote = true;
            }

            if (this.duration > 0 && base.fixedAge >= this.duration)
                endEmote = true;

            if (endEmote)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            HideShit(true);

            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            if (this.activePlayID != 0) AkSoundEngine.StopPlayingID(this.activePlayID);

            if (memeRig && memeRig.isPlaying)
                memeRig.stopAnim();

            base.cameraTargetParams.RemoveParamsOverride(camOverrideHandle, 0.5f);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }
}