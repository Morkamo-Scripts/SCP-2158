using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using UnityEngine;

namespace SCP_2158.Components
{
    public abstract class Scp2158Component : CustomWeapon
    {
        public override SpawnProperties SpawnProperties { get; set; } = null;
        public override float Weight { get; set; } = 2;
        
        public abstract bool EnableHighlight { get; set; }
        public abstract string HighlightColor { get; set; }
        public abstract float HighlightRange { get; set; }
        public abstract float HighlightIntensity { get; set; }
        
        public abstract bool EnableParticles { get; set; }
        public abstract Vector3 SpawnRange { get; set; }
        public abstract float ParticleSize { get; set; }
        public abstract ushort Intensity { get; set; }
        
        public abstract string PickupMessage { get; set; }
        public abstract ushort CustomItemPickupMessageDuration { get; set; }
        public abstract ushort CustomItemSelectMessageDuration { get; set; }
        public abstract ushort PickupMessageVerticalPosition { get; set; }
    }
}