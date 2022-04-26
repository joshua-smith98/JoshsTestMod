using System;
using System.Collections.Generic;
using OWML.Common;
using OWML.ModHelper;
using UnityEngine;

namespace ModTemplate
{
    public class SurviveTheSupernova : ModBehaviour
    {
        bool isInitialised = false;
        bool sunShouldExplode = false;
        float wakeUpTime;
        
        SupernovaDestructionVolume SDV;
        OWScene currentScene;
        
        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(SurviveTheSupernova)} is loaded!", MessageType.Success);

            // Example of accessing game code.
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                currentScene = loadScene;
                if (currentScene is not OWScene.SolarSystem)
                {
                    SDV = null;
                    return;
                }

                SDV = FindObjectOfType<SupernovaDestructionVolume>();

                //Got to do this because otherwise it seems to add an additional listener per restart
                if (!isInitialised)
                {
                    GlobalMessenger.AddListener("WakeUp", OnWakeUp);
                }

                isInitialised = true;
            };
        }

        private void FixedUpdate()
        {
            if (currentScene is not OWScene.SolarSystem)
                return;

            SDV._checkForPlayerDestruction = false; //Fingers crossed this works... nope
            SDV.SetActivation(false); //Hopefully this isn't too laggy or something

            // Explode sun 0.1 seconds after you wake up - should avoid the loading time interfering on later loops
            if (sunShouldExplode && Time.time - wakeUpTime >= 0.1f)
            {
                sunShouldExplode = false;
                OnEarlyExplode();
            }
        }

        private void OnWakeUp()
        {
            wakeUpTime = Time.time;
            sunWillExplode = true;
        }

        private void OnEarlyExplode()
        {
            //TimeLoop.SetTimeLoopEnabled(false); //Right...apparently this just gives you the You Are Dead screen after the ATP pulls you back. K.
            GlobalMessenger.FireEvent("TriggerSupernova");

            var deathController = FindObjectOfType<DeathManager>();
            Destroy(deathController); //Trying to force the player to not be killed by the timeloop.
            //Yeah that's right, I just destroyed death. This can't end badly at all.
            //NOOOFUUUU

            var timeloopCoreController = FindObjectOfType<TimeLoopCoreController>();
            Destroy(timeloopCoreController); //Yeah screw the ATP!!!
            //Oh god it actually worked.
        }
    }
}
