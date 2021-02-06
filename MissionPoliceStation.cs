using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using GTA.Math;

namespace MissionBase
{
    public class MissionPoliceStation : Script
    {
        int missionIndex = -1; // determine the state of the mission, -1 is not in a mission
        bool missionActive = false;
        Vector3 Blipposition = new Vector3(414.32f, -978.70f, 29.45f);
        Vector3 PoliceCaptainPos = new Vector3(450.33f, -975.29f, 30.69f);
        Blip missionBlip;
        Blip copBlip;
        Ped policecaptain;
        public MissionPoliceStation()
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
            if (e.KeyCode == Keys.H)
            {
                missionIndex = 0;
                missionActive = true;
            }
        }

        void MainMission()
        {
            if (missionActive == true)
            {
                switch (missionIndex)
                {
                    case 0: // initial start
                        UI.ShowSubtitle("Go to the ~y~Police Station.~s~");
                        missionBlip = World.CreateBlip(Blipposition);
                        if (missionBlip.Exists())
                        {
                            missionBlip.Name = "Destination";
                            missionBlip.Sprite = (BlipSprite)1;
                            missionBlip.Color = BlipColor.Yellow;
                            missionBlip.ShowRoute = true;
                        }
                        missionIndex = 10;
                        break;
                    case 10:
                        if (Game.Player.Character.Position.DistanceTo(Blipposition) < 10f)
                        {
                            UI.ShowSubtitle("Take out the ~r~Police Captain~s~.");
                            copBlip = World.CreateBlip(PoliceCaptainPos);
                            copBlip.Sprite = (BlipSprite)1;
                            copBlip.Color = BlipColor.Red;
                            copBlip.Name = "Police Captain";
                            policecaptain = World.CreatePed(PedHash.Cop01SMY, PoliceCaptainPos, 133.71f);
                            policecaptain.AlwaysKeepTask = true;
                            policecaptain.IsEnemy = true;
                            policecaptain.Weapons.Give(WeaponHash.Pistol, 9999, true, true);
                            //Function.Call(Hash.SET_PED_COMBAT_ABILITY, policecaptain, 100); // 100 = attack
                            if (missionBlip.Exists())
                            {
                                missionBlip.ShowRoute = false;
                                missionBlip.Alpha = 0;
                                missionBlip.Remove();
                            }
                        }
                        missionIndex = 20;
                        break;
                    case 20:
                        if (policecaptain.IsDead)
                        {
                            if (copBlip.Exists())
                            {
                                copBlip.Alpha = 0;
                                copBlip.Remove();
                            }
                            UI.ShowSubtitle("Lose The Cops.");
                            policecaptain.MarkAsNoLongerNeeded();
                        }
                        if (Game.Player.WantedLevel == 0)
                        {
                            missionIndex = -1;
                            missionActive = false;
                        }
                        break;
                }
            }

            if (Game.Player.Character.IsDead)
            {
                missionIndex = -1; // reset mission
                missionActive = false;
            }
        }
    }
}
