using System;
using System.Threading.Tasks;
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
 * https://github.com/winject/ForceActionMode | https://github.com/logicspawn/GTARPG
 * https://github.com/oldnapalm/BicycleCity   | https://github.com/udoprog/ChaosMod
 * 
 ***************************************************************************************|
 * 
 * [Notes]
 * - Each script class must inherit from `Script`, which contains the `Tick`, 
 *    `KeyUp`, and `KeyDown` events.
 *    
 * - SHV.NET Namespaces: GTA, GTA.Math, GTA.Native, GTA.NaturalMotion
 * 
 * - Useful classes: Game, World, Entity, Ped, Vehicle, Weapon, Task, Native.PedHash
 * 
 * [Keys Enum]
 * - "[" = OemOpenBrackets = Oem4,  "]" = OemCloseBrackets = Oem6, "\" = OemPipe/Oem5
 * - ";" = Oem1 = OemSemicolon,  "'" = Oem7 = OemQuotes
 * - "," = Oemcomma,  "." = OemPeriod,  "/" = OemQuestion
 *  
\***************************************************************************************/

namespace TestScripts
{
    /// <summary>
    /// Entry point for all test scripts. Scripts are instantiated and toggled from here.
    /// </summary>
    public class TestScripts : Script
    {
        // [HACK] Make a better way to do this kind of thing.
        public static float TEMPFORCERADIUS = 20;


        private Dictionary<TestScriptTemplate, Keys> scripts;
        private const int notificationTimeout = 200;


        public TestScripts()
        {
            // Instantiate each script class and map its keybind in a dict (list all script class names here)
            // [IDEA] Use numpad for all keybinds, combined with Shift and Ctrl for 30 total keys.
            scripts = new Dictionary<TestScriptTemplate, Keys>();
            scripts.Add(new TS_NearbyPedsStartFighting(), Keys.NumPad3);
            scripts.Add(new TS_MatrixMode(), Keys.NumPad2);
            scripts.Add(new TS_RadiusView(), Keys.D0);
            scripts.Add(new TS_CarDance(), Keys.None);
            scripts.Add(new TS_HelicopterBuddies(), Keys.NumPad1);
            scripts.Add(new TS_FollowMode(), Keys.OemPipe);
            scripts.Add(new TS_PreciseTeleport(), Keys.None);

            // Append script functions for handling key presses and actions on ticks
            KeyUp += MainKeyUp;
            KeyDown += MainKeyDown;
            Tick += MainTick;
        }

        public void MainKeyUp(object sender, KeyEventArgs e)
        {
            // Run the ScriptKeyUp() method of each script, if its designated hotkey was pressed.
            foreach (KeyValuePair<TestScriptTemplate, Keys> script_hotkey in scripts)
            {
                // List to hold handles for notifications to "Hide" them after a time delay,
                //  preventing new notifications being lost due to existing ones staying active.
                //List<int> notificationHandles = new List<int>();

                int notifHandle;

                (var script, var hotkey) = (script_hotkey.Key, script_hotkey.Value);

                if (e.KeyCode == hotkey)  // key pressed matches this script's hotkey
                {
                    // Handle toggling of script 'active' flag and display status notifications.
                    script.ToggleActive();

                    if (script.active)
                    {
                        notifHandle = GTA.UI.Notification.Show(script.name + ": ~g~Active~s~");
                        Task.Delay(notificationTimeout).ContinueWith(o => { GTA.UI.Notification.Hide(notifHandle); TestAPI.DebugMsg<string>(notifHandle.ToString() + "active hidden after" + notificationTimeout.ToString() + "ms"); });
                    }
                    else
                    {
                        notifHandle = GTA.UI.Notification.Show(script.name + ": ~r~Disabled~s~");
                        Task.Delay(notificationTimeout).ContinueWith(o => GTA.UI.Notification.Hide(notifHandle));
                    }


                    // Run any additional code provided by script's KeyUp method.
                    script.ScriptKeyUp();
                }
            }

            // [HACK] See TEMPFORCERADIUS defn
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
            foreach (KeyValuePair<TestScriptTemplate, Keys> script_hotkey in scripts)
            {
                (var script, var hotkey) = (script_hotkey.Key, script_hotkey.Value);

                if (e.KeyCode == hotkey)  // key pressed matches this script's hotkey
                {
                    // Run any additional code provided by script's KeyDown method.
                    script.ScriptKeyDown();
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
    // [TODO] Convert to interface?
    /// <summary>
    /// Template for testing scripts in this project to follow.
    /// </summary>
    /// <remarks>
    /// Make sure to update 'this.name' in each derived script, 
    /// and add the script's class name to the dictionary in TestScripts.
    /// </remarks>
    public abstract class TestScriptTemplate
    {
        /// <summary>
        /// Common name for the script.
        /// </summary>
        /// <remarks>
        /// This should be overridden by the script class constructor. Default value is "Unknown".
        /// </remarks>
        internal string name = "Unknown";

        // Bool inherited by each script to track its Active/Disabled state.
        /// <summary>
        /// Current state of script. Script is 'Active' if true, and 'Disabled' if false.
        /// </summary>
        internal bool active = false;

        /// <summary>
        /// Toggles state of script between 'Active' and 'Disabled'.
        /// </summary>
        public void ToggleActive()
        {
            active = !active;
        }

        /// <summary>
        /// Code for this script to run on KeyDown events for its designated keybind.
        /// </summary>
        public abstract void ScriptKeyDown();
        /// <summary>
        /// Code for this script to run on KeyUp events for its designated keybind.
        /// </summary>
        public abstract void ScriptKeyUp();
        /// <summary>
        /// Code for this script to run on each tick.
        /// </summary>
        /// <remarks>
        /// All code in this method should be wrapped by ``if (this.active)``
        /// to prevent running when script should be Disabled.
        /// </remarks>
        public abstract void ScriptTick();
    }


    public class TS_NearbyPedsStartFighting : TestScriptTemplate
    {
        public TS_NearbyPedsStartFighting() { this.name = "Fight!"; }

        public override void ScriptTick()
        {
            if (this.active)
            {
                Ped[] nearbyPeds = World.GetNearbyPeds(Game.Player.Character.Position, 20);

                //Ped[] filteredPeds = nearbyPeds.;

                foreach (Ped ped in nearbyPeds)
                {
                    ped.Task.FightAgainstHatedTargets(300);  // TEMP
                }
            }

        }

        public override void ScriptKeyUp() { }
        public override void ScriptKeyDown() { }
    }


    public class TS_MatrixMode : TestScriptTemplate
    {
        private List<Projectile> activeProjs = new List<Projectile>();
        private List<Projectile> deletedProjs = new List<Projectile>();

        public TS_MatrixMode() { this.name = "Matrix Mode"; }

        public override void ScriptTick()
        {
            if (this.active)
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

        public override void ScriptKeyUp() { }
        public override void ScriptKeyDown() { }
    }


    public class TS_RadiusView : TestScriptTemplate
    {
        private float forceRadius;
        private Vector3 forceCirclePos;
        private Vector3 forceTarget;
        private Vector3 forceVectorToTarget;
        private double forceAngle;
        private Ped player = Game.Player.Character;

        public TS_RadiusView()
        {
            this.name = "Radius Viewer";

            // Init variables

            //forceRadius = 15f;                // default testing radius
            forceCirclePos = Vector3.UnitX;     // initial force rotation vector
            forceAngle = 0;                     // initial force angle
        }

        public override void ScriptTick()
        {
            if (this.active)
            {
                /* [Planning]
                 * • Alternate implementation ideas:
                 *    - Add a force tangent to player position rotating around a circle, 
                 *      and a second force that pushes entities closer or further to reach the target radius.
                 *    - PID feedback loop?
                 * • Use Tasks to force ragdoll and prevent peds from standing in air?
                 * • Change height of orbit (lower)? Or make height oscillate up and down?
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
                forceTarget = playerPosition + TestScripts.TEMPFORCERADIUS * forceCirclePos;   // [HACK] Temp radius (see TestScripts note)


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

        public override void ScriptKeyUp() { }
        public override void ScriptKeyDown() { }
    }


    public class TS_CarDance : TestScriptTemplate
    {
        public TS_CarDance() { this.name = "Dance Time"; }

        public override void ScriptTick()
        {
            if (this.active)
            {
                // 1. Make all doors open and close rapidly
                // 2. ??


                // 1.) Use native functions?
                //   https://gtamods.com/wiki/SET_VEHICLE_DOOR_OPEN
                //   https://gtamods.com/wiki/SET_VEHICLE_DOOR_SHUT
                // Game.Player.Character.CurrentVehicle.??
            }
        }

        public override void ScriptKeyUp() { }
        public override void ScriptKeyDown() { }
    }


    public class TS_HelicopterBuddies : TestScriptTemplate
    {
        private int MAX_RANGE = 400;
        private List<Ped> activePilots = new List<Ped>();
        private Ped player = Game.Player.Character;

        public TS_HelicopterBuddies() { this.name = "Helicopter Buddies"; }

        public override void ScriptTick()
        {
            // [TODO] Fix so that each active heli will only receive a task once (and focus on one target?)
            // [TODO] Activate spotlight or something to indicate pairs of heli's
            // [TODO] Blimps count as heli's. Handle this seperately?

            if (this.active)
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


                // [TEST] Print number of pilots
                Random random = new Random();
                if (random.Next(100) == 0)
                    TestAPI.DebugMsg<string>("# Pilots : " + activePilots.Count.ToString());

            }
        }

        public override void ScriptKeyUp() { }
        public override void ScriptKeyDown() { }
    }


    public class TS_FollowMode : TestScriptTemplate
    {
        // [TODO] Tweak range/speed constants.
        // [TODO] Tweak task controlling. Periodically check for changed tasks (like running from a crime) and reset?

        private const int MAX_RANGE = 500;
        private const float FOLLOW_SPEED = 80f;

        private List<Ped> activePeds = new List<Ped>();

        private Ped player = Game.Player.Character;

        public TS_FollowMode() { this.name = "Follow Mode"; }

        public override void ScriptKeyUp()
        {
            // Cleanup on disable
            if (!this.active)
            {
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
            if (this.active)
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


                        // [TODO] Add menu to toggle each class

                    }
                }

            }
        }

        public override void ScriptKeyDown() { }

    }

    public class TS_PreciseTeleport : TestScriptTemplate
    {
        public override void ScriptTick()
        {
            if (this.active)
            {
                // See START_PLAYER_TELEPORT (https://docs.fivem.net/natives/?_0xAD15F075A4DA0FDE)
            }
        }

        public override void ScriptKeyUp() { }
        public override void ScriptKeyDown() { }
    }


    public class TS_Bones : TestScriptTemplate
    {
        private int MAX_RANGE = 200;
        private int CLOSE_RANGE = 5;
        private Ped player = Game.Player.Character;
        private Random rand = new Random();

        public override void ScriptTick()
        {
            if (this.active)
            {
                // [TODO] Change all GetNearbyPeds calls to use the Ped object like this, not a Vector3.
                //        This leaves the player out of the returned array, and the position doesn't need to be constantly read.
                Ped[] nearbyPeds = World.GetNearbyPeds(player, MAX_RANGE);

                foreach (Ped ped in nearbyPeds)
                {
                    // Check if there are any close-by peds to this ped
                    Ped[] pedsInPersonalSpace = World.GetNearbyPeds(ped, CLOSE_RANGE);
                    if (pedsInPersonalSpace.Length > 1)
                    {
                        if (rand.NextDouble() < 0.1)  // 10% chance
                            // If roll is successful, choose a random close-by ped to attach to.
                            pedsInPersonalSpace[rand.Next(pedsInPersonalSpace.Length)].AttachTo(ped);
                    }
                }

            }
        }

        public override void ScriptKeyUp() { }
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
                else if (radius > 0.21f)  // avoids rounding issues
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
