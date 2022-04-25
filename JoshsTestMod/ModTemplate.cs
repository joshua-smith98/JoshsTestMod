using System;
using System.Collections.Generic;
using OWML.Common;
using OWML.ModHelper;
using UnityEngine;

namespace ModTemplate
{
    public class ModTemplate : ModBehaviour
    {
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
            ModHelper.Console.WriteLine($"My mod {nameof(ModTemplate)} is loaded!", MessageType.Success);

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
            };
        }

        private void FixedUpdate()
        {
            if (currentScene is not OWScene.SolarSystem || SDV is null)
                return;

            SDV._checkForPlayerDestruction = false; //Fingers crossed this works... nope
        }

        private void OnWakeUp()
        {
            GlobalMessenger.FireEvent("TriggerSupernova");
        }
    }
}
