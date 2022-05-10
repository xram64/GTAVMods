using System;
using System.Windows.Forms;
using GTA;
using GTA.Math;

/* ___________________
 * || PedController ||
 * Tools for controlling pedestrian movement, tasks, and more.
 * 
 * [Keys Used]
 *  -  \  : Send peds
 *  -  [  : Decrease distance
 *  -  ]  : Increase distance
 *  
 * [TODO]
 *  - Make a system to toggle parts of this mod on and off (create a menu?)
 */

namespace Tools
{
    public class RunThere : Script
    {
        const int PED_RADIUS = 100;  // peds within this radius will be included in the command

        int moveDistance = 20;      // set default distance

        public RunThere()
        {
            //KeyUp += OnKeyUp;    // ***Enable/disable script functionality here***
        }

        private void ChangeDistance(string direction)
        {
            // Amount of change in each interval (capped to [1, 1000]):
            //   [1, 10)      : 1
            //   [10, 100)    : 5
            //   [100, 250)   : 10
            //   [250, 1000]  : 50

            if (direction == "Up")
            {
                if (moveDistance >= 1000)
                    moveDistance = 1000;
                else if (moveDistance >= 250)
                    moveDistance += 50;
                else if (moveDistance >= 100)
                    moveDistance += 10;
                else if (moveDistance >= 10)
                    moveDistance += 5;
                else if (moveDistance >= 1)
                    moveDistance += 1;
                else
                    moveDistance = 1;
            }
            else if (direction == "Down")
            {
                if (moveDistance > 250)
                    moveDistance -= 50;
                else if (moveDistance > 100)
                    moveDistance -= 10;
                else if (moveDistance > 10)
                    moveDistance -= 5;
                else if (moveDistance > 1)
                    moveDistance -= 1;
                else
                    moveDistance = 1;
            }

            GTA.UI.Screen.ShowSubtitle("Radius: " + moveDistance.ToString(), 2000);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            //  \  : Send peds
            if (e.KeyCode == Keys.OemPipe)
            {
                Vector3 playerPosition = Game.Player.Character.Position;
                Vector3 playerFwdVector = Game.Player.Character.ForwardVector;

                Ped[] nearbyPeds = World.GetNearbyPeds(playerPosition, PED_RADIUS);
                foreach (Ped ped in nearbyPeds)
                {
                    if (!ped.IsPlayer)  // only apply to NPCs
                    {
                        ped.Task.ClearAllImmediately();
                        ped.Task.RunTo(playerPosition + playerFwdVector * moveDistance, true);
                    }
                }
            }

            //  [  : Decrease distance
            else if (e.KeyCode == Keys.OemOpenBrackets)
                ChangeDistance("Down");

            //  ]  : Increase distance
            else if (e.KeyCode == Keys.OemCloseBrackets)
                ChangeDistance("Up");
        }
    }


    public class NightmareMode : Script
    {
        bool active;
        int pedRadius;
        Vector3 forceRotation;
        double forceAngle;

        public NightmareMode()
        {
            //KeyUp += OnKeyUp;    // ***Enable/disable script functionality here***
            Tick += OnTick;


            // Init variables
            active = false;     // flag to activate radius display
            pedRadius = 60;     // peds in this radius will be affected

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
                if (pedRadius >= 1000)
                    pedRadius = 1000;
                else if (pedRadius >= 250)
                    pedRadius += 50;
                else if (pedRadius >= 100)
                    pedRadius += 10;
                else if (pedRadius >= 10)
                    pedRadius += 5;
                else if (pedRadius >= 1)
                    pedRadius += 1;
                else
                    pedRadius = 1;
            }
            else if (direction == "Down")
            {
                if (pedRadius > 250)
                    pedRadius -= 50;
                else if (pedRadius > 100)
                    pedRadius -= 10;
                else if (pedRadius > 10)
                    pedRadius -= 5;
                else if (pedRadius > 1)
                    pedRadius -= 1;
                else
                    pedRadius = 1;
            }

            GTA.UI.Screen.ShowSubtitle("Radius: " + pedRadius.ToString(), 2000);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            //  0  : Activate
            if (e.KeyCode == Keys.D0)
            {
                active = !active;  // toggle flag
                if (active)
                    GTA.UI.Notification.Show("Nightmare mode: Active");
                else
                    GTA.UI.Notification.Show("Nightmare mode: Disabled");
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

                // Update rotation vector
                forceAngle += 0.1;  // +0.1 radians each frame

                if (forceAngle >= 2 * Math.PI)
                    forceAngle = 0;

                // rotation unneccesary?
                forceRotation = new Vector3((float)Math.Cos(forceAngle), (float)Math.Sin(forceAngle), 0);


                Ped[] nearbyPeds = World.GetNearbyPeds(playerPosition, pedRadius);
                foreach (Ped ped in nearbyPeds)
                {
                    Vector3 forceDir;

                    if (!ped.IsPlayer)  // only apply to NPCs
                    {
                        //ped.HasGravity = false;

                        if (ped.HeightAboveGround < 10)
                        {
                            forceDir = new Vector3(0, 0, 0.75f);
                        }
                        else if (ped.HeightAboveGround > 25)
                        {
                            forceDir = new Vector3(0, 0, -0.75f);
                        }
                        else
                        {
                            forceDir = new Vector3(0, 0, 0.0f);
                        }

                        ped.ApplyForce(forceDir, forceRotation);
                    }
                }
            }
        }
    }

}