using System;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

/*******************************************************************************\
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
 *******************************************************************************|
 * 
 * [Notes]
 * Each script class must inherit from `Script`, which contains
 *  the `Tick`, `KeyUp`, and `KeyDown` events.
 * 
 * SHV.NET Namespaces: GTA, GTA.Math, GTA.Native, GTA.NaturalMotion
 * Useful Classes: Game, World, Entity, Ped, Vehicle, Weapon, Task
 * 
 * [Ideas]
 * - Single-elimination battle royale: First contestent ped is chosen and picks
 *    a fight with a random nearby ped. Winner of fight takes on another nearby
 *    ped, and so on. (Keep track of current champion on screen?)
 *    (Declare winner after a certain number rounds/wins?)
 *    
 *  - Initiate targeted car chases between two or more peds.
 *  
 *  - Add "wanted level" to ped, plus health/weapon mods.
 *  
 *  
\*******************************************************************************/

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
            Tick += OnTick;
            KeyUp += OnKeyUp;
            //KeyDown += OnKeyDown;
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
        public TestRadiusView()
        {
            Tick += OnTick;
            KeyUp += OnKeyUp;
            //KeyDown += OnKeyDown;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            // Display an overlay or something on screen at different radii to determine scale of 'radius' parameters.
            if (e.KeyCode == Keys.)
            {

            }
        }

        private void OnTick(object sender, EventArgs e)
        {

        }
        
    }
}