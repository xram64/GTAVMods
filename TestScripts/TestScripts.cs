using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

/***************************************************************************************\
 * [Docs]
 * https://nitanmarcel.github.io/shvdn-docs.github.io/namespace_g_t_a.html  (v3)
 * https://nitanmarcel.github.io/scripthookvdotnet/scripting_v3/index.html  (v2)
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.keys   (Keys)
 * 
 * [Refs]
 * https://github.com/crosire/scripthookvdotnet/wiki
 * https://gtaforums.com/topic/789907-community-script-hook-v-net
 * 
 * [Examples]
 * https://github.com/winject/ForceActionMode
 * https://github.com/logicspawn/GTARPG
 * https://github.com/oldnapalm/BicycleCity
 * https://github.com/udoprog/ChaosMod
 * 
 ***************************************************************************************|
 * 
 * [Notes]
 * - Each script class must inherit from `Script`, which contains the `Tick`, 
 *    `KeyUp`, and `KeyDown` events.
 *    
 * - SHV.NET Namespaces: GTA, GTA.Math, GTA.Native, GTA.NaturalMotion
 * 
 * - Useful classes:
 *   - Game, World, Entity, Ped, Vehicle, Weapon, Task, Native.PedHash
 *   
 * - Interesting methods/properties:
 *   - Ped.StaysInVehicleWhenJacked, Ped.Euphoria
 * 
 * [Keys Enum]
 * - "[" = OemOpenBrackets = Oem4,  "]" = OemCloseBrackets = Oem6, "\" = OemPipe/Oem5
 * - ";" = Oem1 = OemSemicolon,  "'" = Oem7 = OemQuotes
 * - "," = Oemcomma,  "." = OemPeriod,  "/" = OemQuestion
 * 
 * 
 * [Ideas]
 * - Single-elimination battle royale: First contestent ped is chosen and picks a fight 
 *    with a random nearby ped. Winner of fight takes on another nearby ped, and so on.
 *    (Keep track of current champion on screen?)
 *    (Declare winner after a certain number rounds/wins?)
 *    
 *  - Initiate targeted car chases between two or more peds.
 *  
 *  - Add "wanted level" to ped, plus health/weapon mods.
 *  
 *  
\***************************************************************************************/

namespace TestScripts
{
    public class TestScripts : Script
    {
        /// <summary>
        /// Starting point for all test scripts. Scripts can be loaded and unloaded.
        /// </summary>

        // [TODO]
        //  • Move "active = !active" and Notification lines from individual KeyUp methods to MainKeyUp?

        // [HACK] //
        public static float TEMPFORCERADIUS = 20;

        Dictionary<TestScriptTemplate, Keys> scripts;


        public TestScripts()
        {
            // Instantiate each script class and map its keybind in a dict (list all script class names here)
            scripts = new Dictionary<TestScriptTemplate, Keys>();
            scripts.Add(new TS_NearbyPedsStartFighting(), Keys.NumPad3);
            scripts.Add(new TS_MatrixMode(), Keys.NumPad2);
            scripts.Add(new TS_RadiusView(), Keys.D0);
            scripts.Add(new TS_CarDance(), Keys.None);
            scripts.Add(new TS_HelicopterBuddies(), Keys.NumPad1);
            scripts.Add(new TS_FollowMode(), Keys.OemPipe);


            // Append script functions for handling key presses and actions on ticks
            KeyUp += MainKeyUp;
            KeyDown += MainKeyDown;
            Tick += MainTick;
        }


        public void MainKeyUp(object sender, KeyEventArgs e)
        {
            // Run the ScriptKeyUp() method of each script, if its designated hotkey was pressed.
            foreach (KeyValuePair<TestScriptTemplate, Keys> scriptToHotkey in scripts)
            {
                if (e.KeyCode == scriptToHotkey.Value)
                {
                    TestScriptTemplate script = scriptToHotkey.Key;
                    scriptToHotkey.Key.ScriptKeyUp();

                    active = !active;

                    if (active)
                        GTA.UI.Notification.Show("Helicopter Buddies: ~g~Active~s~");
                    else
                        GTA.UI.Notification.Show("Helicopter Buddies: ~r~Disabled~s~");
                }
            }

            // [HACK] //
            //  [  : Decrease radius
            if (e.KeyCode == Keys.OemOpenBrackets)
                ModifyValues.ChangeRadius(ref TestScripts.TEMPFORCERADIUS, "Down");

            //  ]  : Increase radius
            else if (e.KeyCode == Keys.OemCloseBrackets)
                ModifyValues.ChangeRadius(ref TestScripts.TEMPFORCERADIUS, "Up");
        }

        private void MainKeyDown(object sender, KeyEventArgs e)
        {
            // Run the ScriptKeyDown() method of each script, if its designated hotkey was pressed.
            foreach (KeyValuePair<TestScriptTemplate, Keys> scriptToHotkey in scripts)
            {
                if (e.KeyCode == scriptToHotkey.Value)
                {
                    scriptToHotkey.Key.ScriptKeyDown();
                }
            }
        }

        private void MainTick(object sender, EventArgs e)
        {
            // Run the ScriptTick() method of each script (no checking here for active/inactive scripts).
            foreach (TestScriptTemplate script in scripts.Keys)
            {
                script.ScriptTick();
            }
        }


    }

    // Abstract type to define the interface for each script to follow.
    public abstract class TestScriptTemplate
    {
        public abstract void ScriptKeyDown();
        public abstract void ScriptKeyUp();
        /// <summary>
        /// Code for this script to run on each tick.
        /// </summary>
        /// <remarks>
        /// All code in this method should be wrapped by an 'if (active)' to prevent running when script should be off.
        /// </remarks>
        public abstract void ScriptTick();
    }


    public class TS_NearbyPedsStartFighting : TestScriptTemplate
    {
        private bool active;

        public TS_NearbyPedsStartFighting() { }


        public override void ScriptKeyUp()
        {
            active = !active;

            if (active)
                GTA.UI.Notification.Show("Fight!: ~g~Active~s~");
            else
                GTA.UI.Notification.Show("Fight!: ~r~Disabled~s~");
        }

        public override void ScriptTick()
        {
            if (active)
            {
                Ped[] nearbyPeds = World.GetNearbyPeds(Game.Player.Character.Position, 20);

                //Ped[] filteredPeds = nearbyPeds.;

                foreach (Ped ped in nearbyPeds)
                {
                    ped.Task.FightAgainstHatedTargets(300);  // TEMP
                }
            }

        }

        public override void ScriptKeyDown() { }
    }


    public class TS_MatrixMode : TestScriptTemplate
    {
        private bool active;

        private List<Projectile> activeProjs = new List<Projectile>();
        private List<Projectile> deletedProjs = new List<Projectile>();

        public TS_MatrixMode() { }

        public override void ScriptKeyUp()
        {
            active = !active;

            if (active)
                GTA.UI.Notification.Show("Matrix Mode: ~g~Active~s~");
            else
                GTA.UI.Notification.Show("Matrix Mode: ~r~Disabled~s~");
        }

        public override void ScriptTick()
        {
            if (active)
            {
                // Check for nearby projectiles and add any new ones to the list
                Projectile[] nearbyProjs = World.GetNearbyProjectiles(Game.Player.Character.Position, 200);
                foreach (Projectile proj in nearbyProjs)
                {
                    // For any new projectiles, slow them down before handling in the next step
                    if (!activeProjs.Contains(proj))
                    {
                        activeProjs.Add(proj);
                        proj.Velocity = 0.5f * proj.Velocity;
                    }
                }

                foreach (Projectile proj in activeProjs)
                {
                    // If this projectile has been deleted, mark it for removal from the list
                    if (!proj.Exists())
                    {
                        deletedProjs.Add(proj);
                    }
                    else
                    {
                        // If the projectile has an owner, send it back, otherwise stop it
                        if (proj.Owner != null)
                        {
                            //Vector3 error = 5 * Vector3.RandomXYZ();

                            Vector3 toSender = proj.Position - proj.Owner.Position;
                            proj.ApplyForce(-2f * toSender.Around(5));
                        }
                        else
                        {
                            proj.Velocity = new Vector3(0, 0, 0);
                        }
                    }
                }

                // Remove deleted projectiles from list
                foreach (Projectile proj in deletedProjs)
                {
                    if (activeProjs.Contains(proj))
                        activeProjs.Remove(proj);
                }
                // Clean up deleted projectile list
                deletedProjs = new List<Projectile>();
            }
        }

        public override void ScriptKeyDown() { }
    }


    public class TS_RadiusView : TestScriptTemplate
    {
        private bool active;
        float forceRadius;
        Vector3 forceCirclePos;
        Vector3 forceTarget;
        Vector3 forceVectorToTarget;
        double forceAngle;
        private Ped player = Game.Player.Character;

        public TS_RadiusView()
        {
            // Init variables
            active = false;     // flag to activate radius display
            //forceRadius = 15f;    // default testing radius

            forceCirclePos = Vector3.UnitX;  // initial force rotation vector
            forceAngle = 0;                 // initial force angle
        }

        public override void ScriptKeyUp()
        {
            // (Display an overlay or something on screen at different radii to determine scale of 'radius' parameters.)

            // Activate "radius display"
            active = !active;

            if (active)
                GTA.UI.Notification.Show("Radius View: ~g~Active~s~");
            else
                GTA.UI.Notification.Show("Radius View: ~r~Disabled~s~");

            //TestAPI.DebugMsg("Force / Rotation: " + forceDir.ToString() + " / " + forceRotation.ToString());

        }

        public override void ScriptTick()
        {
            if (active)
            {
                /* [Planning]
                 * 
                 * • Alternate implementation ideas:
                 *    - Add a force tangent to player position rotating around a circle, 
                 *      and a second force that pushes entities closer or further to reach the target radius.
                 *    - PID feedback loop?
                 * • Use Tasks to force ragdoll and prevent peds from standing in air?
                 * 
                 */


                // Get player properties
                Vector3 playerPosition = player.Position;
                Vector3 playerFwdVector = player.ForwardVector;

                // Update rotation vector
                forceAngle += 0.05;  // +radians each frame

                if (forceAngle >= 2 * Math.PI)
                    forceAngle = 0;


                forceCirclePos = new Vector3((float)Math.Cos(forceAngle), (float)Math.Sin(forceAngle), 0);
                //forceCirclePos = new Vector3((float)(-Math.Sin(forceAngle)), (float)Math.Cos(forceAngle), 0);

                // Point on radius of target circle relative to player
                //forceTarget = playerPosition + forceRadius * forceCirclePos;
                forceTarget = playerPosition + TestScripts.TEMPFORCERADIUS * forceCirclePos;        // TEMP HACK //


                Ped[] nearbyPeds = World.GetNearbyPeds(playerPosition, 80);
                foreach (Ped ped in nearbyPeds)
                {
                    if (!ped.IsPlayer)  // only apply to NPCs
                    {
                        // Point on radius of target circle relative to ped
                        forceVectorToTarget = forceTarget - ped.Position;

                        //ped.ApplyForce(2.5f * forceVectorToTarget);
                        ped.ApplyForce(2f * forceVectorToTarget.Normalized);  // testing
                    }
                }
            }

        }

        public override void ScriptKeyDown() { }
    }


    public class TS_CarDance : TestScriptTemplate
    {
        public TS_CarDance() { }

        public override void ScriptKeyUp() { }

        public override void ScriptTick()
        {
            // 1. Make all doors open and close rapidly
            // 2. ??


            // 1.) Use native functions?
            //   https://gtamods.com/wiki/SET_VEHICLE_DOOR_OPEN
            //   https://gtamods.com/wiki/SET_VEHICLE_DOOR_SHUT
            // Game.Player.Character.CurrentVehicle.??
        }

        public override void ScriptKeyDown() { }
    }


    public class TS_HelicopterBuddies : TestScriptTemplate
    {
        private bool active;
        int MAX_RANGE = 400;
        private List<Ped> activePilots = new List<Ped>();
        Ped player = Game.Player.Character;

        public TS_HelicopterBuddies() { }

        public override void ScriptKeyUp()
        {
            active = !active;

            if (active)
                GTA.UI.Notification.Show("Helicopter Buddies: ~g~Active~s~");
            else
                GTA.UI.Notification.Show("Helicopter Buddies: ~r~Disabled~s~");
        }

        public override void ScriptTick()
        {
            // [TODO]
            // • Fix so that each active heli will only receive a task once (and focus on one target?)
            // • Activate spotlight or something to indicate pairs of heli's
            // • Blimps count as heli's. Handle this seperately?

            if (active)
            {
                /////////////////////////////
                // Collect active helicopters

                Vector3 playerPosition = player.Position;
                Ped[] nearbyPeds = World.GetNearbyPeds(playerPosition, MAX_RANGE);

                foreach (Ped ped in nearbyPeds)
                {
                    // Get all helicopter pilots in area
                    if (!ped.IsPlayer && !activePilots.Contains(ped) && ped.IsInHeli && ped.SeatIndex == VehicleSeat.Driver && ped.IsAlive)
                    {
                        // Add any heli pilots to the list
                        activePilots.Add(ped);
                    }
                    // Check for inactive/destroyed helicopters and remove from list
                    else if (activePilots.Contains(ped))
                    {
                        activePilots.Remove(ped);
                    }
                }

                ////////////////////////////
                // Handle active helicopters

                /**
                // Pair off adjacent pilots (starting with 0+1) and make them touch
                for (int idx = 0; idx < Math.Floor((double)activePilots.Count / 2); idx++)
                {
                    // Runs once for every pair of pilots in list.
                    // If an odd number of pilots is found, the last in the list will not be paired.
                    Ped friendA = activePilots[idx];
                    Ped friendB = activePilots[idx + 1];

                    //friendA.Task.ClearAll();
                    //friendA.AlwaysKeepTask = true;
                    //friendA.Task.FightAgainst(friendB);
                    //friendA.Task.LookAt(friendB);
                    friendA.Task.ChaseWithHelicopter(friendB, new Vector3(0, 0, 0));

                    friendB.Task.ChaseWithHelicopter(friendA, new Vector3(0, 0, 0));
                }
                **/

                // Send each pilot in the list after the next in the list, circling back to the start.
                for (int idx = 0; idx < activePilots.Count; idx++)
                {
                    Ped thisPilot = activePilots[idx];
                    Ped nextPilot = activePilots[(idx + 1) % activePilots.Count];

                    //thisPilot.Task.ChaseWithHelicopter(nextPilot, new Vector3(1f, 0, -1f));

                    // Set driving flags
                    VehicleDrivingFlags customDrivingStyle = VehicleDrivingFlags.AllowGoingWrongWay
                        | VehicleDrivingFlags.IgnorePathFinding
                        | VehicleDrivingFlags.DriveBySight
                        | VehicleDrivingFlags.AllowMedianCrossing;

                    // Start follow task
                    //ped.VehicleDrivingFlags = customDrivingStyle;
                    if (thisPilot != null && nextPilot != null)
                        Function.Call(Hash.TASK_VEHICLE_FOLLOW, thisPilot, thisPilot.CurrentVehicle, nextPilot, 100f, (int)customDrivingStyle, 1);

                }


                // TEST Print
                Random random = new Random();
                if (random.Next(100) == 0)
                    TestAPI.DebugMsg<string>("# Pilots : " + activePilots.Count.ToString());

            }
        }

        public override void ScriptKeyDown() { }
    }


    public class TS_FollowMode : TestScriptTemplate
    {
        // [TODO]
        //  • Tweak range/speed constants.
        //  • Tweak task controlling. Periodically check for changed tasks (like running from a crime) and reset?

        private bool active;

        private const int MAX_RANGE = 500;
        private const float FOLLOW_SPEED = 80f;

        private List<Ped> activePeds = new List<Ped>();

        private Ped player = Game.Player.Character;

        public TS_FollowMode() { }

        public override void ScriptKeyUp()
        {
            active = !active;

            if (active)
            {
                GTA.UI.Notification.Show("Follow Mode: ~g~Active~s~");
            }
            else
            {
                GTA.UI.Notification.Show("Follow Mode: ~r~Disabled~s~");

                // Cleanup
                foreach (Ped ped in activePeds)
                {
                    if (ped.IsInVehicle())
                        ped.CurrentVehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Front, false);

                    ped.DrivingStyle = DrivingStyle.Normal;
                    ped.AlwaysKeepTask = false;
                    ped.Task.ClearAll();
                }
                activePeds.Clear();
            }
        }

        public override void ScriptTick()
        {
            if (active)
            {
                // Get player properties
                Vector3 playerPosition = player.Position;
                Vector3 playerFwdVector = player.ForwardVector;

                Ped[] nearbyPeds = World.GetNearbyPeds(playerPosition, MAX_RANGE);

                foreach (Ped ped in nearbyPeds)
                {
                    // Only run for: Non-player NPCs, not already active, currently in vehicles, as the driver
                    if (!ped.IsPlayer && !activePeds.Contains(ped) && ped.IsInVehicle() && ped.SeatIndex == VehicleSeat.Driver)
                    {
                        // Add neon indicator for active vehicles
                        ped.CurrentVehicle.Mods.HasNeonLight(VehicleNeonLight.Front);
                        ped.CurrentVehicle.Mods.NeonLightsColor = Color.DarkRed;
                        ped.CurrentVehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Front, true);

                        // Set tasks
                        ped.Task.ClearAll();
                        ped.AlwaysKeepTask = true;

                        // Set driving flags
                        VehicleDrivingFlags customDrivingStyle = VehicleDrivingFlags.AllowGoingWrongWay
                            | VehicleDrivingFlags.IgnorePathFinding
                            | VehicleDrivingFlags.DriveBySight
                            | VehicleDrivingFlags.AllowMedianCrossing;

                        // Start follow task
                        //ped.VehicleDrivingFlags = customDrivingStyle;
                        Function.Call(Hash.TASK_VEHICLE_FOLLOW, ped, ped.CurrentVehicle, player, FOLLOW_SPEED, (int)customDrivingStyle, 1);


                        // Special handling for followers in air vehicles
                        if (ped.IsInFlyingVehicle)
                        {
                            // Helicopters
                            if (ped.IsInHeli)
                            {

                            }

                            // Planes
                            if (ped.IsInPlane)
                            {

                            }
                        }



                        // Add ped to seen list (cap size of list at 1000)
                        activePeds.Add(ped);
                        if (activePeds.Count > 1000)
                            activePeds.RemoveAt(0);


                        //TestAPI.DebugMsg<string>("Driver added: " + ped.PedGroup.ToString()
                        //    + " / " + ped.Model.ToString()
                        //    + " driving " + ped.CurrentVehicle.DisplayName.ToString()
                        //    + " (" + ped.GetHashCode().ToString() + ")");


                        // TODO ---- Add menu to toggle each class

                    }
                }

            }
        }

        public override void ScriptKeyDown() { }

    }

    public class TS_PreciseTeleport : TestScriptTemplate
    {
        public override void ScriptKeyUp()
        {

        }
        public override void ScriptTick()
        {

        }
        public override void ScriptKeyDown() { }
    }




    ////////////////////
    // Helper Classes //
    ////////////////////

    public static class ModifyValues
    {
        public static void ChangeRadius(ref float radius, string direction)
        {
            // Amount of change in each interval (clamped to [0.2, 1000]):
            //   [0.2, 1)     : 0.2
            //   [1, 10)      : 1
            //   [10, 100)    : 5
            //   [100, 250)   : 10
            //   [250, 1000]  : 50

            if (direction == "Up")
            {
                if (radius >= 1000f)
                    radius = 1000f;
                else if (radius >= 250f)
                    radius += 50f;
                else if (radius >= 100f)
                    radius += 10f;
                else if (radius >= 10f)
                    radius += 5f;
                else if (radius >= 1f)
                    radius += 1f;
                else if (radius >= 0.2f)
                    radius += 0.2f;
                else
                    radius = 0.2f;

                GTA.UI.Screen.ShowSubtitle("~p~Radius: ~h~" + radius.ToString() + "~h~~s~", 2000);
            }

            else if (direction == "Down")
            {
                if (radius > 250f)
                    radius -= 50f;
                else if (radius > 100f)
                    radius -= 10f;
                else if (radius > 10f)
                    radius -= 5f;
                else if (radius > 1f)
                    radius -= 1f;
                else if (radius > 0.21f)  // hack to avoid rounding issues
                    radius -= 0.2f;
                else
                    radius = 0.2f;
            }

            GTA.UI.Screen.ShowSubtitle("~p~Radius: ~h~" + radius.ToString() + "~h~~s~", 2000);
        }
    }



    public static class TestAPI
    {
        // Helper class to discover functionality of API calls by printing debug messages in-game.
        // Accepts an arbitrary object, converts it to a string if needed, and displays the output as a subtitle.
        public static void DebugMsg<T>(T obj)
        {
            GTA.UI.Screen.ShowSubtitle("DEBUG: [ " + obj.ToString() + " ]", 1000);
        }
    }

}
