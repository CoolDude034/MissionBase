using System;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using GTA.Math;

namespace MissionBase
{
    public class Mission : Script
    {
        int missionIndex = -1; // determine the state of the mission, -1 is not in a mission
        bool missionActive = false; // if the mission is active or not.
        bool showncopmessage = false;
        Vector3 Blipposition = new Vector3(414.32f, -978.70f, 29.45f); // blip near the police station
        Vector3 PoliceCaptainPos = new Vector3(439.313f, -992.324f, 30.690f); // Inside some room, idk
        Vector3 archivePos1 = new Vector3(443.828f, -975.937f, 30.690f); // blip position for archive room
        Blip missionBlip;
        Ped policecaptain;

        public Mission()
        {
            Tick += UpdateFunction;
            KeyDown += startMission;
        }

        void UpdateFunction(object sender, EventArgs e)
        {
            MainMission();
        }

        void startMission(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F6 && missionActive != true)
            {
                missionIndex = 0;
                missionActive = true;


                // setup mission entities
                policecaptain = World.CreatePed(PedHash.Cop01SMY, PoliceCaptainPos, 266.188f);
                policecaptain.AlwaysKeepTask = true;
                policecaptain.IsEnemy = true;
                policecaptain.AddBlip();
                policecaptain.CurrentBlip.Scale = 1;
                policecaptain.CurrentBlip.IsFriendly = false;
                policecaptain.CurrentBlip.Sprite = BlipSprite.Enemy;
                policecaptain.CurrentBlip.Color = BlipColor.White;
                Function.Call(Hash.SET_BLIP_SHOW_CONE, policecaptain.CurrentBlip, true);
                policecaptain.Weapons.Give(WeaponHash.Pistol, 9999, true, true);

                // setup waypoint
                UI.ShowSubtitle("Go to the ~y~Police Station.~s~");
                UI.Notify("Alright, our target is inside the Mission Row PD, you can do this silently or go loud your choice");
                UI.ShowHelpMessage("Enemies are marked with white dots with vision cones, dont get spotted.");
                missionBlip = World.CreateBlip(Blipposition);
                missionBlip.Name = "Destination";
                missionBlip.Sprite = (BlipSprite)1;
                missionBlip.Color = BlipColor.Yellow;
                missionBlip.ShowRoute = true;
            }
        }

        void MainMission()
        {
            if (missionActive == true)
            {
                CopsAlerted();
                PlayerDied();
                switch (missionIndex)
                {
                    case 0: // the beginning
                        if (Game.Player.Character.Position.DistanceTo(Blipposition) < 30f)
                        {
                            UI.ShowSubtitle("Take out the ~r~Police Captain~s~.");
                            if (policecaptain.Exists())
                            {
                                if (policecaptain.IsDead)
                                {
                                    missionIndex = 10;
                                }
                                if (policecaptain.IsInCombat == true | Game.Player.WantedLevel > 0) // if the NPC enters combat, we want its blip to change to red
                                {
                                    if (policecaptain.CurrentBlip.Exists())
                                    {
                                        policecaptain.CurrentBlip.Color = BlipColor.Red;
                                        Function.Call(Hash.SET_BLIP_SHOW_CONE, policecaptain.CurrentBlip, false);
                                    }
                                }
                            }
                            if (missionBlip.Exists())
                            {
                                missionBlip.ShowRoute = false;
                                missionBlip.Alpha = 0;
                                missionBlip.Remove();
                            }
                        }
                        break;
                    case 10:
                        UI.ShowSubtitle("Go to the ~y~Archives.~s~");
                        missionBlip = World.CreateBlip(archivePos1);
                        if (missionBlip.Exists())
                        {
                            missionBlip.Name = "Destination";
                            missionBlip.Sprite = (BlipSprite)1;
                            missionBlip.Color = BlipColor.Yellow;
                            missionBlip.ShowRoute = false;
                        }
                        if (policecaptain.Exists())
                        {
                            if (policecaptain.CurrentBlip.Exists())
                            {
                                Function.Call(Hash.SET_BLIP_SHOW_CONE, policecaptain.CurrentBlip, false);
                                policecaptain.CurrentBlip.Alpha = 0;
                                policecaptain.CurrentBlip.Remove();
                            }
                        }
                        missionIndex = 20;
                        break;
                    case 20:
                        if (Game.Player.Character.Position.DistanceTo(archivePos1) < 10f)
                        {
                            if (missionBlip.Exists())
                            {
                                missionBlip.ShowRoute = false;
                                missionBlip.Alpha = 0;
                                missionBlip.Remove();
                            }
                            UI.Notify("~b~Lester:~s~ Shit it's not there, alright Plan B, escape the police station as i will figure out the target by other means.");
                            missionIndex = 30;
                        }
                        break;
                    case 30:
                        if (Game.Player.WantedLevel > 0)
                        {
                            if (Game.Player.Character.IsOnFoot == true) // Player is mostly onfoot, and if played with Assault Teams mod, then this becomes more immersive.
                            {
                                UI.ShowHelpMessage("Police have launched an assault, hold them off if you can.");
                            }
                            UI.ShowSubtitle("Lose The Cops.");
                        }
                        else if (Game.Player.WantedLevel == 0)
                        {
                            missionIndex = 40;
                        }
                        break;
                    case 40:
                        UI.ShowHelpMessage("You have completed the mission!");
                        UI.Notify("~b~Lester:~s~ Alright, that was a hefty mission, here is your cut.");
                        Game.Player.Money = Game.Player.Money + 4000;
                        ResetMission();
                        break;
                }
            }

            if (Game.Player.Character.IsDead)
            {
                ResetMission();
            }
        }
        
        void ResetMission()
        {
            missionIndex = -1; // reset mission
            missionActive = false;
            showncopmessage = false;
            // cleanup...
            if (policecaptain.Exists())
            {
                if (policecaptain.CurrentBlip.Exists())
                {
                    Function.Call(Hash.SET_BLIP_SHOW_CONE, policecaptain.CurrentBlip, false);
                    policecaptain.CurrentBlip.Remove();
                }
                policecaptain.MarkAsNoLongerNeeded();
            }
            if (missionBlip.Exists())
            {
                missionBlip.ShowRoute = false;
                missionBlip.Alpha = 0;
                missionBlip.Remove();
            }
        }

        void CopsAlerted()
        {
            if (Game.Player.WantedLevel > 1 && showncopmessage == false)
            {
                showncopmessage = true;
                UI.ShowHelpMessage("You have been ~r~compromised~s~");
            }
        }

        void PlayerDied()
        {
            if (Game.Player.Character.IsDead)
            {
                ResetMission();
            }
        }
    }
}
