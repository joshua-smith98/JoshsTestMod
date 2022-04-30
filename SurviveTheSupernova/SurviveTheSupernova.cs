using System;
using System.Collections.Generic;
using OWML.Common;
using OWML.ModHelper;
using UnityEngine;

namespace ModTemplate
{
    public class SurviveTheSupernova : ModBehaviour
    {
        bool disableSupernovaRumble;
        bool triggerSupernovaOnWake;
        
        bool sunShouldExplode = false;
        float wakeUpTime;
        
        SupernovaDestructionVolume SDV;
        SupernovaEffectController SEC;
        DeathManager deathManager;
        OWScene currentScene;

        #region OWML Methods

        public override void Configure(IModConfig config)
        {
            disableSupernovaRumble = config.GetSettingsValue<bool>("Disable Supernova Rumble");
            triggerSupernovaOnWake = config.GetSettingsValue<bool>("Trigger Supernova on Wake");
        }

        #endregion

        #region Unity Methods

        private void Start()
        {
            // Send load message
            ModHelper.Console.WriteLine($"{nameof(SurviveTheSupernova)} is loaded!", MessageType.Success);

            // On scene load
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                currentScene = loadScene;
                if (currentScene is not OWScene.SolarSystem)
                {
                    SDV = null;
                    SEC = null;
                    return;
                }

                SDV = FindObjectOfType<SupernovaDestructionVolume>();
                SEC = FindObjectOfType<SupernovaEffectController>();
                deathManager = FindObjectOfType<DeathManager>();

                // Disable timeloop and supernova deaths
                ModHelper.HarmonyHelper.AddPrefix<DeathManager>(nameof(DeathManager.KillPlayer), typeof(SurviveTheSupernova), nameof(SurviveTheSupernova.DeathManagerPrefix_KillPlayer));

                // We need to do this anyway, so that the ship isn't destroyed
                SDV.SetActivation(false);

                GlobalMessenger.AddListener("WakeUp", OnWakeUp);

                ModHelper.Console.WriteLine($"{nameof(SurviveTheSupernova)} initialised.");
            };
        }

        private void FixedUpdate()
        {
            if (currentScene is not OWScene.SolarSystem)
                return;

            // Explode sun 0.1 seconds after you wake up - should avoid the loading time interfering on later loops
            if (sunShouldExplode && Time.time - wakeUpTime >= 0.1f)
            {
                sunShouldExplode = false;

                if (triggerSupernovaOnWake)
                    GlobalMessenger.FireEvent("TriggerSupernova");
            }

            //Mute Supernova Rumble
            if (disableSupernovaRumble & SEC._audioSource.isPlaying)
                SEC._audioSource.Stop();
        }

        private void Destroy()
        {
            if (currentScene is not OWScene.SolarSystem)
                return;

            //Get rid of listener on destroy so we don't double up
            GlobalMessenger.RemoveListener("WakeUp", OnWakeUp);
        }

        #endregion

        #region Callback & Patch Methods

        private void OnWakeUp()
        {
            wakeUpTime = Time.time;
            sunShouldExplode = true;
        }
        
        private static bool DeathManagerPrefix_KillPlayer(DeathType deathType)
        {
            return deathType is not (DeathType.TimeLoop or DeathType.Supernova); //Skip original method and don't kill player on timeloop or supernova
        }

        #endregion
    }
}
