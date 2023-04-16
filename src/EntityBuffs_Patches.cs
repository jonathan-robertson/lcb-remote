using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LcbRemote
{
    internal class EntityBuffs_Patches
    {

        [HarmonyPatch(typeof(EntityBuffs), "AddBuffNetwork")]
        internal class EntityBuffs_AddBuffNetwork_Patch
        {
            private static readonly ModLog<EntityBuffs_AddBuffNetwork_Patch> log = new ModLog<EntityBuffs_AddBuffNetwork_Patch>();
            internal static bool Prefix(EntityAlive ___parent, string _name, int _instigatorId = -1)
            {
                try
                {
                    // TODO: make sure this works for local as well...

                    if (_name.Equals(""))
                    {

                    }

                    var shouldAllow = ShouldAllowBuff(___parent, _name, _instigatorId);
                    if (!shouldAllow)
                    {
                        ___parent.Buffs.RemoveBuff(_name, false); // echo removal back to the rest of the system
                    }
                    return shouldAllow;
                }
                catch (Exception e)
                {
                    log.Error("Error in Prefix.", e);
                    return true;
                }
            }
        }
    }
}
