using System;
using System.Collections.Generic;
using OWML.Common;
using OWML.ModHelper;
using UnityEngine;

namespace ModTemplate
{
    public class SurviveTheSupernova : ModBehaviour
    {
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
            ModHelper.Console.WriteLine($"{nameof(SurviveTheSupernova)} is loaded!", MessageType.Success);

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

                GlobalMessenger.AddListener("WakeUp", OnWakeUp);

                ModHelper.Console.WriteLine($"{nameof(SurviveTheSupernova)} initialised.");
            };
        }

        private void Destroy()
        {
            if (currentScene is not OWScene.SolarSystem)
                return;

            //Get rid of listener on destroy so we don't double up
            GlobalMessenger.RemoveListener("WakeUp", OnWakeUp);
        }

        private void FixedUpdate()
        {
            if (currentScene is not OWScene.SolarSystem)
                return;

            SDV.SetActivation(false); //Deactivate the SupernovaDestructionVolume

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
            sunShouldExplode = true;
        }

        private void OnEarlyExplode()
        {
            //TimeLoop.SetTimeLoopEnabled(false); //Right...apparently this just gives you the You Are Dead screen after the ATP pulls you back. K.
            ModHelper.Console.WriteLine("Blowing up the sun, and disabling the ATP so the loop never ends.");
            GlobalMessenger.FireEvent("TriggerSupernova");

            var timeloopCoreController = FindObjectOfType<TimeLoopCoreController>();
            Destroy(timeloopCoreController); //Yeah screw the ATP!!!
            //Oh god it actually worked.
        }
    }
}
