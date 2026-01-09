using System;
using Exiled.API.Features;
using Exiled.CustomItems.API;
using HarmonyLib;

namespace SCP_2158
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "SCP-2158";
        public override string Prefix => Name;
        public override string Author => "Morkamo";
        public override Version Version => new Version(2, 0, 0);
        public override Version RequiredExiledVersion => new Version(9, 12, 1);

        public static Plugin Instance { get; private set; }
        private static Harmony _harmony;

        public override void OnEnabled()
        {
            Instance = this;
            _harmony = new Harmony("ru.morkamo.scp2158.patches");
            _harmony.PatchAll();
            
            Config.Scp2158.Register();
            Config.Scp2158Alt1.Register();
            
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Config.Scp2158.Unregister();
            Config.Scp2158Alt1.Unregister();
            
            _harmony.UnpatchAll();
            _harmony = null;
            Instance = null;
            base.OnDisabled();
        }
    }
}