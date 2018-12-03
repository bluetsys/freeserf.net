﻿/*
 * AI.cs - Character AI logic
 *
 * Copyright (C) 2018  Robert Schneckenhaus <robert.schneckenhaus@web.de>
 *
 * This file is part of freeserf.net. freeserf.net is based on freeserf.
 *
 * freeserf.net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * freeserf.net is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with freeserf.net. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freeserf
{
    abstract class AIState
    {
        public bool Killed { get; private set; } = false;
        public AIState NextState { get; protected set; } = null;
        public int Delay { get; set; } = 0;

        public abstract void Update(AI ai, Game game, Player player, PlayerInfo playerInfo, int tick);

        public virtual void Kill(AI ai)
        {
            ai.PopState();
            Killed = true;
        }

        public void GoToState(AI ai, AI.State state, object param = null)
        {
            if (state != AI.State.Idle)
                NextState = ai.CreateState(state, param);

            Kill(ai);
        }
    }

    /// <summary>
    /// Note: The ai will not be the same as in the original game.
    /// But it will reflect some of the ai character descriptions.
    /// The plan is to add some new ai characters with better ai.
    /// 
    /// Maybe the campaign will be extended for this sake as well.
    /// </summary>
    public class AI
    {
        public enum State
        {
            Idle,
            ChooseCastleLocation,
            CastleBuilt,
            BuildBuilding,
            LinkBuilding,
            LinkDisconnectedFlags,
            BuildNeededBuilding,
            CraftWeapons,
            CraftTool
            // TODO ...
        }

        readonly Player player = null;
        readonly PlayerInfo playerInfo = null;
        readonly Stack<AIState> states = new Stack<AIState>();
        int lastTick = 0;
        long lastUpdate = 0;
        internal long GameTime { get; private set; } = 0;
        readonly Random random = new Random(Guid.NewGuid().ToString());

        // Special AI values
        public bool CanAttack { get; protected set; } = true;
        public bool CanExpand { get; protected set; } = true;
        public int MaxMilitaryBuildings { get; protected set; } = -1;


        /// <summary>
        /// How aggressive (2 = very aggressive)
        /// </summary>
        public int Aggressivity { get; private set; } = 0; // 0 - 2
        /// <summary>
        /// How skilled in fights (2 = skilled fighter)
        /// </summary>
        public int MilitarySkill { get; private set; } = 0; // 0 - 2
        /// <summary>
        /// How much focus on military (2 = high focus)
        /// </summary>
        public int MilitaryFocus { get; private set; } = 0; // 0 - 2
        /// <summary>
        /// How much focus on expanding (2 = aggressive expansion)
        /// </summary>
        public int ExpandFocus { get; private set; } = 0; // 0 - 2
        /// <summary>
        /// How much focus at having much buildings
        /// </summary>
        public int BuildingFocus { get; private set; } = 0; // 0 - 2
        /// <summary>
        /// How much focus at having much gold
        /// </summary>
        public int GoldFocus { get; private set; } = 0; // 0 - 2
        /// <summary>
        /// How much focus at having much coal and iron
        /// </summary>
        public int SteelFocus { get; private set; } = 0; // 0 - 2
        /// <summary>
        /// How much focus at having much food
        /// </summary>
        public int FoodFocus { get; private set; } = 0; // 0 - 2
        /// <summary>
        /// How much focus at having much construction materials
        /// </summary>
        public int ConstructionMaterialFocus { get; private set; } = 0; // 0 - 2
        /// <summary>
        /// The ai will try to gather prioritized food if possible
        /// </summary>
        int[] foodSourcePriorities = new int[3]; // 0 - 2 for fish, bread and meat
        /// <summary>
        /// The ai will try to build prioritized military buildings if possible
        /// </summary>
        int[] militaryBuildingPriorities = new int[3]; // 0 - 2 for hut, tower and fortress
        int[] minPlanksForMilitaryBuildings = new int[2]; // minimum free planks for tower and fortress
        int[] minStonesForMilitaryBuildings = new int[2]; // minimum free stones for tower and fortress

        /* Military Focus
         * 
         * - Focuses on getting coal and iron
         * - Focuses on crafting weapons and shields
         * - High knight production
         * - Many military buildings
         */

        /* Military Skill
         * 
         * - Will protect important buildings with some military buildings
         * - Will have enough knights where needed
         * - Will try to attack strategic spots of the enemy
         * - Prefers attacking spots of the enemy that are able to capture
         */

        public AI(Player player, PlayerInfo playerInfo)
        {
            this.player = player;
            this.playerInfo = playerInfo;
        }

        /// <summary>
        /// This is called if an enemy is in range to attack the castle.
        /// </summary>
        /// <param name="lastChance">If only the castle and a few buildings remain. Only set for better ai characters.</param>
        public void PrepareForDefendingCastle(bool lastChance)
        {
            // Order all knights to castle
            // - Set maximum knights of military buildings to minimum
            // - Order knights back to castle

            // If lastChance use drastic methods like burning down military buildings
            if (lastChance)
            {
                // Burn down all military buildings to free knights
            }
        }

        public void PrepareFights(bool quick)
        {
            // Set maximum knights of military buildings to maximum

            if (!quick)
            {
                // If bad knights are in military buildings and there are better
                // ones in inventories, than swap with better ones
            }
        }

        public void HandleEmptyMine()
        {
            // TODO
        }

        internal AIState CreateState(State state, object param = null)
        {
            switch (state)
            {
                case State.Idle:
                    return new AIStates.AIStateIdle();
                case State.ChooseCastleLocation:
                    return new AIStates.AIStateChoosingCastleLocation();
                case State.CastleBuilt:
                    return new AIStates.AIStateCastleBuilt();
                case State.BuildBuilding:
                    return new AIStates.AIStateBuildBuilding((Building.Type)param);
                case State.LinkBuilding:
                    return new AIStates.AIStateLinkBuilding((uint)param);
                case State.LinkDisconnectedFlags:
                    return new AIStates.AIStateLinkDisconnectedFlags();
                case State.BuildNeededBuilding:
                    return new AIStates.AIStateBuildNeededBuilding();
                case State.CraftTool:
                    return new AIStates.AIStateCraftTool((Resource.Type)param);
                case State.CraftWeapons:
                    return new AIStates.AIStateCraftWeapons();
                // TODO ...
            }

            throw new ExceptionFreeserf("Unknown AI state");
        }

        /// <summary>
        /// Create an AI state that is active after the given delay.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="delay">Delay in milliseconds</param>
        /// <param name="param"></param>
        /// <returns></returns>
        internal AIState CreateDelayedState(State state, int delay, object param = null)
        {
            var aiState = CreateState(state, param);

            aiState.Delay = delay * Global.TICKS_PER_SEC / 1000;

            return aiState;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="minDelay">Minimum delay in milliseconds</param>
        /// <param name="maxDelay">Maximum delay in milliseconds</param>
        /// <param name="param"></param>
        /// <returns></returns>
        internal AIState CreateRandomDelayedState(State state, int minDelay, int maxDelay, object param = null)
        {
            int delay = minDelay + random.Next() % (maxDelay + 1 - minDelay);

            return CreateDelayedState(state, delay, param);
        }

        internal void PushState(AIState state)
        {
            states.Push(state);
        }

        /// <summary>
        /// Note the first pushed will be on top.
        /// So the order of the parameters will also
        /// be the execution order of the states.
        /// </summary>
        /// <param name="states"></param>
        internal void PushStates(params AIState[] states)
        {
            for (int i = states.Length - 1; i >= 0; --i)
                PushState(states[i]);
        }

        internal AIState PopState()
        {
            if (states.Count == 0)
                return null;

            return states.Pop();
        }

        internal void ClearStates()
        {
            states.Clear();
        }

        internal bool StupidDecision()
        {
            return random.Next() > 42000 + (int)playerInfo.Intelligence * 500;
        }


        #region Game analysis helper functions

        internal bool HasEssentialBuildings(Game game)
        {
            return
                game.GetPlayerBuildings(player, Building.Type.Lumberjack).Count() > 0 &&
                game.GetPlayerBuildings(player, Building.Type.Stonecutter).Count() > 0 &&
                game.GetPlayerBuildings(player, Building.Type.Sawmill).Count() > 0;
        }

        internal bool HasResourcesForBuilding(Game game, Building.Type type)
        {
            var constructionInfo = Building.ConstructionInfos[(int)type];

            return
                game.GetResourceAmountInInventories(player, Resource.Type.Plank) >= constructionInfo.Planks &&
                game.GetResourceAmountInInventories(player, Resource.Type.Stone) >= constructionInfo.Stones;
        }

        #endregion


        public void Update(Game game)
        {
            if (lastTick == 0)
            {
                lastTick = game.Tick;
                GameTime = game.Tick;
            }
            else
                GameTime += game.Tick - lastTick;

            if (states.Count == 0)
            {
                if (!player.HasCastle())
                {
                    PushState(CreateState(State.ChooseCastleLocation));
                }
                else
                {
                    PushState(CreateState(State.CastleBuilt));
                }
            }

            var currentState = states.Peek();

            if (currentState == null)
            {
                // continue with next state
                states.Pop();
                Update(game);
                return;
            }

            if (currentState.Delay > 0)
                currentState.Delay -= (game.Tick - lastTick);
            else if (GameTime - lastUpdate >= Global.TICKS_PER_SEC) // only update every second
            {
                currentState.Update(this, game, player, playerInfo, (int)(GameTime - lastUpdate));
                lastUpdate = GameTime;
            }

            if (currentState.Killed)
            {
                if (currentState.NextState != null)
                    states.Push(currentState.NextState);
            }

            lastTick = game.Tick;
        }
    }

    public class IntroAI : AI
    {
        public IntroAI(Player player, PlayerInfo playerInfo)
            : base(player, playerInfo)
        {
            CanAttack = false;
            CanExpand = true;
            MaxMilitaryBuildings = 2;
        }
    }
}
