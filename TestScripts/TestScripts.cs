using System;
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
    public class TestFighting : Script
    {
        public void StartNearbyFight()
        {
            Ped[] nearbyPeds = World.GetNearbyPeds(Game.Player.Character.Position, 20);

            //Ped[] filteredPeds = nearbyPeds.;

            foreach (Ped ped in nearbyPeds)
            {
                //ped.; 
            }

        }

    }

    public class TestProjectiles : Script
    {
        public Boolean matrixMode = false;

        public TestProjectiles()
        {
            //KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            Tick += OnTick;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.OemPipe)
            {
                matrixMode = !matrixMode;

                if (matrixMode) { GTA.UI.Notification.Show("Matrix mode toggled ON."); }
                else { GTA.UI.Notification.Show("Matrix mode toggled OFF."); }
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (matrixMode)
            {
                StopProjectiles(50);
            }
        }

        public void StopProjectiles(int radius)
        {
            Projectile[] nearbyProjs = World.GetNearbyProjectiles(Game.Player.Character.Position, radius);
            foreach (Projectile proj in nearbyProjs)
            {
                proj.Velocity = new Vector3(0, 0, 0);
            }
        }

    }


    public class TestRadiusView : Script
    {
        bool active;
        int testRadius;
        Vector3 forceRotation;
        double forceAngle;

        public TestRadiusView()
        {
            //KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            Tick += OnTick;

            // Init variables
            active = false;     // flag to activate radius display
            testRadius = 20;    // default testing radius

            forceRotation = Vector3.UnitX;  // initial force rotation vector
            forceAngle = 0;                 // initial force angle
        }

        private void ChangeRadius(string direction)
        {
            // Amount of change in each interval (capped to [1, 1000]):
            //   [1, 10)      : 1
            //   [10, 100)    : 5
            //   [100, 250)   : 10
            //   [250, 1000]  : 50

            if (direction == "Up")
            {
                if (testRadius >= 1000)
                    testRadius = 1000;
                else if (testRadius >= 250)
                    testRadius += 50;
                else if (testRadius >= 100)
                    testRadius += 10;
                else if (testRadius >= 10)
                    testRadius += 5;
                else if (testRadius >= 1)
                    testRadius += 1;
                else
                    testRadius = 1;
            }
            else if (direction == "Down")
            {
                if (testRadius > 250)
                    testRadius -= 50;
                else if (testRadius > 100)
                    testRadius -= 10;
                else if (testRadius > 10)
                    testRadius -= 5;
                else if (testRadius > 1)
                    testRadius -= 1;
                else
                    testRadius = 1;
            }

            GTA.UI.Screen.ShowSubtitle("Radius: " + testRadius.ToString(), 2000);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            // (Display an overlay or something on screen at different radii to determine scale of 'radius' parameters.)

            //  0  : Activate "radius display"
            if (e.KeyCode == Keys.D0)
            {
                active = !active;  // toggle flag
                if (active)
                    GTA.UI.Notification.Show("Radius View: Active");
                else
                    GTA.UI.Notification.Show("Radius View: Disabled");

                //TestAPI.DebugMsg("Force / Rotation: " + forceDir.ToString() + " / " + forceRotation.ToString());
            }

            //  [  : Decrease radius
            else if (e.KeyCode == Keys.OemOpenBrackets)
                ChangeRadius("Down");

            //  ]  : Increase radius
            else if (e.KeyCode == Keys.OemCloseBrackets)
                ChangeRadius("Up");
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (active)
            {
                // Get player properties
                Vector3 playerPosition = Game.Player.Character.Position;
                Vector3 playerFwdVector = Game.Player.Character.ForwardVector;

                // Update rotation vector
                forceAngle += 0.1;  // +0.1 radians each frame

                if (forceAngle >= 2 * Math.PI)
                    forceAngle = 0;

                forceRotation = new Vector3((float)Math.Cos(forceAngle), (float)Math.Sin(forceAngle), 0);


                Ped[] nearbyPeds = World.GetNearbyPeds(playerPosition, 60);
                foreach (Ped ped in nearbyPeds)
                {
                    if (!ped.IsPlayer)  // only apply to NPCs
                    {
                        //ped.ApplyForceRelative();
                    }
            }
        }

    }


    public static class TestAPI
    {
        // Helper class to discover functionality of API calls by printing debug messages in-game.
        // Accepts an arbitrary object, converts it to a string if needed, and displays the output as a subtitle.
        public static void DebugMsg<T>(T obj)
        {
            GTA.UI.Screen.ShowSubtitle("DEBUG: [ " + obj.ToString() + " ]", 5000);
        }
    }

}