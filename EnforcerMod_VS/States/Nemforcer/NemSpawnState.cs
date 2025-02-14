﻿using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Nemforcer
{
    public class SpawnState : BaseState
    {
        public static float duration = 2.5f;

        private CameraRigController cameraController;
        private bool initCamera;

        public override void OnEnter()
        {
            base.OnEnter();
            base.PlayAnimation("Body", "Spawn");
            Util.PlaySound(NullifierMonster.SpawnState.spawnSoundString, base.gameObject);

            base.GetModelAnimator().SetLayerWeight(base.GetModelAnimator().GetLayerIndex("Minigun"), 0);

            if (NetworkServer.active) base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);

            if (EnforcerPlugin.EnforcerModPlugin.nemesisSpawnEffect)
            {
                EffectManager.SimpleMuzzleFlash(EntityStates.NullifierMonster.SpawnState.spawnEffectPrefab, base.gameObject, "SpawnOrigin", false);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            // i don't know if all this null checking is necessary but i'd rather play it safe than spend time testing
            if (!this.cameraController)
            {
                if (base.characterBody && base.characterBody.master)
                {
                    if (base.characterBody.master.playerCharacterMasterController)
                    {
                        if (base.characterBody.master.playerCharacterMasterController.networkUser)
                        {
                            this.cameraController = base.characterBody.master.playerCharacterMasterController.networkUser.cameraRigController;
                        }
                    }
                }
            }
            else
            {
                if (!this.initCamera)
                {
                    this.initCamera = true;
                    ((RoR2.CameraModes.CameraModePlayerBasic.InstanceData)this.cameraController.cameraMode.camToRawInstanceData[this.cameraController]).SetPitchYawFromLookVector(-base.characterDirection.forward);
                }
            }

            if (base.fixedAge >= SpawnState.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (NetworkServer.active) base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
