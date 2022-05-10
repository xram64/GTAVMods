using System;
using System.Windows.Forms;
using GTA;
using GTA.Math;


/* __________________
 * || Discrete Mod ||
 * Main hub for toggling mod options and launching scripts.
 * 
 * [Keys Used]
 *  -  \  : 
 *  
 * [TODO]
 * 
 */


public class DiscreteMod : Script
{
    // Global constants
    public const int MAX_RADIUS = 100;     // Max distance from player to select nearby peds/vehicles

    // Global script parameters
    public int moveDistance;
    public Ped player;


    // Initialize mod and set default param values
    public DiscreteMod()
    {
        //KeyDown += OnKeyDown;
        //KeyUp += OnKeyUp;
        //Tick += OnTick;


        // Set default values
        moveDistance = 20;
        player = Game.Player.Character;
    }


    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        //  \  : Send peds
        if (e.KeyCode == Keys.OemPipe)
        {
            Vector3 playerPosition = Game.Player.Character.Position;
            Vector3 playerFwdVector = Game.Player.Character.ForwardVector;

            Ped[] nearbyPeds = World.GetNearbyPeds(playerPosition, MAX_RADIUS);
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
            Helpers.ChangeDistance(ref moveDistance, "Down");

        //  ]  : Increase distance
        else if (e.KeyCode == Keys.OemCloseBrackets)
            Helpers.ChangeDistance(ref moveDistance, "Up");
    }
}



public static class Helpers
{
    public static void ChangeDistance(ref int dist, string direction, bool showDebug = false)
    {
        // Amount of change in each interval (capped to [1, 1000]):
        //   [1, 10)      : 1
        //   [10, 100)    : 5
        //   [100, 250)   : 10
        //   [250, 1000]  : 50

        if (direction == "Up")
        {
            if (dist >= 1000)
                dist = 1000;
            else if (dist >= 250)
                dist += 50;
            else if (dist >= 100)
                dist += 10;
            else if (dist >= 10)
                dist += 5;
            else if (dist >= 1)
                dist += 1;
            else
                dist = 1;
        }

        else if (direction == "Down")
        {
            if (dist > 250)
                dist -= 50;
            else if (dist > 100)
                dist -= 10;
            else if (dist > 10)
                dist -= 5;
            else if (dist > 1)
                dist -= 1;
            else
                dist = 1;
        }

        if (showDebug)
            GTA.UI.Screen.ShowSubtitle("Radius: " + dist.ToString(), 2000);

    }

}