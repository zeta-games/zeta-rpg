﻿using System;
using System.Collections.Generic;
using UnityEngine;

/***********************************************
 * TODO:
 * -Implement a priority system for transitions in case there are multiple transitions
 *      that come back as true and possible. (Is this needed since I can prioritize on instantiation order?)
************************************************/

namespace ZetaGames.RPG {
    public class HumanBaseBrain : AIBrain {

        private void Start() {
            // FORCED TEST PARAMETERS
            numMaterialsRequiredList.Add(ResourceCategory.Wood, 6);
            specificMaterialTypeList.Add(ResourceCategory.Wood, ResourceType.None);
            numMaterialsRequiredList.Add(ResourceCategory.Stone, 6);
            specificMaterialTypeList.Add(ResourceCategory.Stone, ResourceType.Rock);

            npcMemory.AddMemory("home", transform.position);

            // Create AI Personality
            personality = new Personality(PersonalityType.Default);

            // Create all NPC states
            var searchForResource = new SearchForResource(this);
            var harvestResource = new HarvestResource(this);
            var storeResource = new StoreResource(this);
            //var getUnstuck = new GetUnstuck(this);
            //var wander = new Wander(this, personality.wanderRadius, personality.wanderCycle);

            /***************************************************************
             * SPECIFIC STATE TRANSITIONS
            ***************************************************************/
            // FROM 'search for resource' to ...
            AT(searchForResource, harvestResource, new List<Func<bool>> { HasResourceTarget(), IsFalse(IsInventoryFull()), IsFalse(IsCarryingResource()), IsResourceTargetHarvestable() });
           
            // FROM 'harvest resource' to ...
            AT(harvestResource, searchForResource, new List<Func<bool>> { IsFalse(HasResourceTarget()), IsFalse(IsInventoryFull()), IsFalse(IsCarryingResource()) });

            /***************************************************************
             * FROM ANY STATE TRANSITIONS
            ***************************************************************/
            // TO 'store resource' from *any* if carrying a resource
            stateMachine.AddFromAnyTransition(storeResource, new List<Func<bool>> { IsCarryingResource() });

            // TO 'return home' from *any state* when stuck
            //stateMachine.AddFromAnyTransition(getUnstuck, new List<Func<bool>> { StuckOnMove() });

            // TO 'wander' from *any*
            //stateMachine.AddFromAnyTransition(wander, new List<Func<bool>> { () => wanderCooldown > personality.wanderMaxCooldown });

            /***************************************************************
             * TO ANY STATE TRANSITIONS ((use caution as this opens a lot of unintended transitions))
            ***************************************************************/
            // FROM 'store resource' to *any state*
            stateMachine.AddToAnyTransition(storeResource);
            
            // FROM 'wander' to *any state*
            //stateMachine.AddToAnyTransition(wander);

            // FROM 'get unstuck' to *any state*
            //stateMachine.AddToAnyTransition(getUnstuck);

            /**********************************************************************************************************************************************************************************
             * END TRANSITIONS
            ***********************************************************************************************************************************************************************************/

            // Set initial NPC state
            stateMachine.SetState(searchForResource);

            // AT(Add Transition) -- Internal function to provide a shorter name to declutter the transition list above (not technically needed)
            void AT(State from, State to, List<Func<bool>> conditions) => stateMachine.AddTransition(from, to, conditions);

            // Conditionals for transitions
            //Func<bool> AtDestinationStopped() => () => pathAgent.remainingDistance < 1f && pathAgent.isStopped;
            Func<bool> HasResourceTarget() => () => resourceTileTarget != null;
            Func<bool> IsResourceTargetHarvestable() => () => resourceTileTarget.occupiedStatus.Contains(ZetaUtilities.OCCUPIED_NODE_FULL);
            Func<bool> IsInventoryFull() => () => npcInventory.IsInventoryFull();
            Func<bool> IsCarryingResource() => () => npcInventory.IsCarryingSomething();

            // Inverse a condition
            Func<bool> IsFalse(Func<bool> conditionToInverse) => () => {
                if (conditionToInverse()) {
                    return false;
                } else {
                    return true;
                }
            };

            /*
            // Stuck character timer
            Func<bool> StuckOnMove() => () => {
                if (timeStuck > 30f) {
                    Debug.Log("I'm stuck!");
                    return true;
                } else {
                    return false;
                }
            };
            */
        }
    }
}

