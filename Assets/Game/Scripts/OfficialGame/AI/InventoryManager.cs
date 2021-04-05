using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class InventoryManager : MonoBehaviour {

        private ResourceManager resourceManager;
        public Dictionary<ResourceType, int> inventory { get; set; }

        //debug vars
        //private Dictionary<ResourceType, int> resourceList;
        //private bool updated = false;
        //public int coinsToAdd = 0;
        //public int coinsToRemove = 0;

        private void Awake() {
            // INVENTORY SETUP
            resourceManager = FindObjectOfType<ResourceManager>();
            resourceManager.addNPC(gameObject.GetInstanceID());
            inventory = resourceManager.getNpcInventory(gameObject.GetInstanceID());
        }

        void Update() {
            /*
            ////////////////////////debug lines
            if(!updated) {
                printInventory();
            }

            if(coinsToAdd > 0) {
                resourceManager.addResource(gameObject.GetInstanceID(), ResourceType.Coin, coinsToAdd);
                coinsToAdd = 0;
                updated = false;
            }

            if (coinsToRemove > 0) {
                resourceManager.subtractResource(gameObject.GetInstanceID(), ResourceType.Coin, coinsToRemove);
                coinsToRemove = 0;
                updated = false;
            }
            ///////////////////////////////////////////
            */
        }

        /*
        //debug method
        private void printInventory() {
            resourceList = resourceManager.getNpcResources(gameObject.GetInstanceID());

            Debug.Log(gameObject.name + "'s Resources:");
            Debug.Log("==========================");
            foreach (KeyValuePair<ResourceType, int> resourceInfo in resourceList) {
                Debug.Log(resourceInfo.Key + ": " + resourceInfo.Value);
            }
            Debug.Log("==========================");
            updated = true;
        }
        */
    }
}