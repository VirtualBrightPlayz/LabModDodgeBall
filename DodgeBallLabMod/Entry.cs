using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LabMod;
using LabMod.Events;
using RemoteAdmin;
using UnityEngine;

namespace DodgeBallLabMod
{
    public class Entry : ILabModGameConsoleQuery, ILabModRoundStart, ILabModPlayerHurt, ILabModPreRoundStart, ILabModPostRoundStart, ILabModUpdate, ILabModRoundEnd
    {
        public static bool dodgeball = false;

        public bool hasGun = false;
        public float timer = 3f;
        public float timer2 = 0f;

        public static void Main()
        {
            LabMod.LabMod.RegisterEvent(new Entry());
        }

        bool ILabModGameConsoleQuery.Event(QueryProcessor processor, string query, bool encrypted)
        {
            ServerConsole.AddLog(query);
            if (PermissionsHandler.IsPermitted(processor.GetComponent<ServerRoles>().Permissions, PlayerPermissions.RoundEvents))
            {
                if (query.Equals("dodgeball"))
                {
                    dodgeball = !dodgeball;
                    processor.GCT.SendToClient(processor.connectionToClient, "dodgeball=" + dodgeball.ToString(), "green");
                    return false;
                }
            }
            return true;
        }

        bool ILabModPlayerHurt.Event(PlayerStats stats, PlayerStats.HitInfo info, GameObject go)
        {
            if (!dodgeball)
                return true;
            /*if (go.GetComponent<CharacterClassManager>().NetworkCurClass == RoleType.Spectator)
            {
                stats.StartCoroutine("SpawnLate", new PlayerStats.SpawnLateHelper() { ccm = go.GetComponent<CharacterClassManager>(), rt = info.GetPlayerObject().GetComponent<CharacterClassManager>().NetworkCurClass });
            }*/
            return true;
        }

        bool ILabModRoundStart.Event(CharacterClassManager ccm)
        {
            if (!dodgeball)
                return true;

            return true;
        }

        void ILabModPreRoundStart.Event(CharacterClassManager ccm)
        {
            if (!dodgeball)
                return;
            RoundSummary.RoundLock = true;
            hasGun = false;
            timer = 3f;
            timer2 = 0f;
        }

        void ILabModPostRoundStart.Event(CharacterClassManager ccm)
        {
            if (!dodgeball)
                return;
            ccm.LaterJoinEnabled = false;
            foreach (var player in PlayerManager.players)
            {
                var cc = player.GetComponent<CharacterClassManager>();
                cc.StartCoroutine(nameof(CharacterClassManager.SpawnLate), new CharacterClassManager.SpawnLateHelper() { ccm = cc, rt = RoleType.ClassD, rtspawn = RoleType.Scp173 });
                cc.GetComponent<Inventory>().Clear();
                cc.GetComponent<Inventory>().AddNewItem(ItemType.SCP018);
                cc.GetComponent<Inventory>().AddNewItem(ItemType.SCP018);
                cc.GetComponent<Inventory>().AddNewItem(ItemType.SCP018);
                cc.GetComponent<Inventory>().AddNewItem(ItemType.Painkillers);
                cc.GetComponent<Inventory>().AddNewItem(ItemType.Painkillers);
                cc.GetComponent<Inventory>().AddNewItem(ItemType.Painkillers);
            }
            //ccm.StartCoroutine(nameof(CharacterClassManager.EndRoundSoon), 60f);
        }

        void ILabModRoundEnd.Event(RoundSummary sum, RoundSummary.SumInfo_ClassList list_start, RoundSummary.SumInfo_ClassList list_finish, RoundSummary.LeadingTeam leadingTeam, int e_ds, int e_sc, int scp_kills, int round_cd)
        {
        }

        void ILabModUpdate.Event()
        {
            if (!dodgeball)
                return;
            PlayerManager.localPlayer.GetComponent<CharacterClassManager>().LaterJoinEnabled = false;
            if (!RoundSummary.RoundLock)
                return;
            timer -= Time.deltaTime;
            timer2 += Time.deltaTime;
            if (timer > 0f)
                return;
            if (timer2 >= 60f)
            {
                foreach (var plr in PlayerManager.players)
                {
                    NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(".g7 .g7", 0.5f, 0.5f);
                    plr.GetComponent<Broadcast>().RpcAddElement("You all failed! Pathetic...", 100, false);
                }
                RoundSummary.RoundLock = false;
            }

            if (timer2 >= 45f && !hasGun)
            {
                foreach (var plr in PlayerManager.players)
                {
                    plr.GetComponent<Broadcast>().RpcAddElement("GUNS!!!", 3, false);
                    plr.GetComponent<Inventory>().AddNewItem(ItemType.GunUSP);
                    plr.GetComponent<Inventory>().AddNewItem(ItemType.Ammo9mm);
                    plr.GetComponent<Inventory>().AddNewItem(ItemType.Ammo9mm);
                }
                hasGun = true;
            }
            timer = 0.5f;
            int dc_c = 0;
            CharacterClassManager win = null;
            foreach (var plr in PlayerManager.players)
            {
                if (plr.GetComponent<CharacterClassManager>().NetworkCurClass == RoleType.ClassD)
                {
                    dc_c++;
                    win = plr.GetComponent<CharacterClassManager>();
                }
            }
            if (dc_c == 0)
            {
                foreach (var plr in PlayerManager.players)
                {
                    NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(".g7 .g7", 0.5f, 0.5f);
                    plr.GetComponent<Broadcast>().RpcAddElement("You all died! Pathetic...", 100, false);
                }
                RoundSummary.RoundLock = false;
            }
            if (dc_c == 1 && win != null)
            {
                foreach (var plr in PlayerManager.players)
                {
                    plr.GetComponent<Broadcast>().RpcAddElement(win.GetComponent<NicknameSync>().Network_myNickSync + " won!", 100, false);
                }
                RoundSummary.RoundLock = false;
            }
        }
    }
}
