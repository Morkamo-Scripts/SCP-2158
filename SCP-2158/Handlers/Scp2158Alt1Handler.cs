using System.Collections;
using System.ComponentModel;
using System.Linq;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Item;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Firearms.Attachments;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using RueI.API;
using RueI.API.Elements;
using SCP_2158.Components;
using SCP_2158.Features;
using UnityEngine;
using events = Exiled.Events.Handlers;
using Light = Exiled.API.Features.Toys.Light;
using Pickup = Exiled.API.Features.Pickups.Pickup;
using Player = Exiled.API.Features.Player;
using Round = Exiled.API.Features.Round;

namespace SCP_2158.Handlers;

public class Scp2158Alt1Handler : Scp2158Component
{
    public override uint Id { get; set; } = 91;
    public override string Name { get; set; } = "SCP-2158-ALT-1";
    public override string Description { get; set; } = "<b><color=#00ffbf>Анигилирующий автомат.\nПри стрельбе можно не целиться...</color>";
    
    public override ItemType Type { get; set; } = ItemType.GunE11SR;
    
    public override bool EnableHighlight { get; set; } = true;
    public override string HighlightColor { get; set; } = "#00ffbf";
    public string HighlightSecondColor { get; set; } = Color.white.ToHex();
    public override float HighlightRange { get; set; } = 0.9f;
    public override float HighlightIntensity { get; set; } = 5f;
    
    public override bool EnableParticles { get; set; } = true;
    public override Vector3 SpawnRange { get; set; } = new(1.2f, 1.2f, 1.2f);
    public override float ParticleSize { get; set; } = 0.2f;
    public override ushort Intensity { get; set; } = 5;
    
    [Description("Это сообщение появляется когда игрок подбирает предмет с земли.")]
    public override string PickupMessage { get; set; } = "<size=35><color=#d18f00><b>Ты подобрал SCP-2158-ALT-1.</b>\n" +
                                                         "<i><color=#00ffbf>Анигилирующий автомат.\n" +
                                                         "При стрельбе можно не целиться...</i></color></size>";
    public override ushort CustomItemPickupMessageDuration { get; set; } = 5;
    public override ushort CustomItemSelectMessageDuration { get; set; } = 3;
    public override ushort PickupMessageVerticalPosition { get; set; } = 400;

    protected override void SubscribeEvents()
    {
        events.Item.ChangingAttachments += OnChangingAttachments;
        events.Player.ChangedItem += OnChangedItem;
        events.Player.DroppedItem += OnDroppedItem;
        events.Player.ItemAdded += OnGetItem;
        LabApi.Events.Handlers.ServerEvents.PickupCreated += OnPickupCreated;
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        events.Item.ChangingAttachments -= OnChangingAttachments;
        events.Player.ChangedItem -= OnChangedItem;
        events.Player.DroppedItem -= OnDroppedItem;
        events.Player.ItemAdded -= OnGetItem;
        LabApi.Events.Handlers.ServerEvents.PickupCreated -= OnPickupCreated;
        base.UnsubscribeEvents();
    }
    
    private void OnGetItem(ItemAddedEventArgs ev)
    {
        if (!Check(ev.Pickup))
            return;
            
        RueDisplay.Get(ev.Player).Show(
            new Tag(),
            new BasicElement(PickupMessageVerticalPosition, PickupMessage),
            CustomItemPickupMessageDuration);
    }

    protected override void OnShot(ShotEventArgs ev)
    {
        Vector3 from = ev.Player.CameraTransform.position;
        Vector3 to = ev.RaycastHit.point;
        float radius = 6;
        bool isDryShot = true;

        /*BulletTrace(from + ev.Player.CameraTransform.forward * 3f, to, radius);*/
        
        if (CheckCapsuleHit(from + ev.Player.CameraTransform.forward, to, radius, out var hitbox))
        {
            isDryShot = false;
            
            var target = Player.Get(hitbox.TargetHub);
            
            if (target == ev.Player)
                return;

            if (target.Role.Team.GetFaction() == ev.Player.Role.Team.GetFaction())
                return;

            if (target.IsGodModeEnabled)
                return;
            
            if (target.IsSpawnProtected || ev.Player.GetEffect<SpawnProtected>().Intensity > 0)
                return;

            var damage = ev.Firearm.Damage * 2;

            var headHitbox = target.GameObject
                .GetComponentsInChildren<HitboxIdentity>()
                .FirstOrDefault(h => h.HitboxType == HitboxType.Headshot);
            
            if (headHitbox != null)
                CurvedBulletTrace(
                    from + ev.Player.CameraTransform.forward * 0.2f,
                    headHitbox.transform.position,
                    ev.Player.CameraTransform.forward,
                    segments: 18,
                    curveStrength: 0.35f,
                    color: Color.magenta
                );
            else
                CurvedBulletTrace(
                    from + ev.Player.CameraTransform.forward * 0.2f,
                    hitbox.transform.position,
                    ev.Player.CameraTransform.forward,
                    segments: 14,
                    curveStrength: 0.25f,
                    color: Color.red
                );
            
            target.Hurt(ev.Player, damage, DamageType.E11Sr);
            ev.Player.ShowHitMarker();
        }
        
        if (isDryShot)
            BulletTrace(from + ev.Player.CameraTransform.forward * 0.2f, to, 0.055f, 0.6f, Color.white, 15f);
        
        base.OnShot(ev);
    }
    
    private void OnPickupCreated(PickupCreatedEventArgs ev) => HighlightItemDouble(Pickup.Get(ev.Pickup.GameObject));
    private void OnDroppedItem(DroppedItemEventArgs ev) => HighlightItemDouble(ev.Pickup);
    
    private void HighlightItemDouble(Pickup pickup)
    {
        if (Check(pickup))
        {
            if (ColorUtility.TryParseHtmlString(HighlightColor, out var color))
            {
                var anchor = HighlightManager.MakeLight(pickup.Position, color,
                    LightShadows.None, HighlightRange, HighlightIntensity - 1.5f);

                Light anchor2 = null;
                
                if (ColorUtility.TryParseHtmlString(HighlightSecondColor, out var lightSecondColor))
                {
                    anchor2 = HighlightManager.MakeLight(pickup.Position, lightSecondColor,
                        LightShadows.None, HighlightRange, HighlightIntensity);
                }
                
                
                if (EnableParticles)
                {
                    HighlightManager.ProceduralParticles(anchor.GameObject, color, 0, 0.05f,
                        SpawnRange, ParticleSize, Intensity);
                    
                    if (ColorUtility.TryParseHtmlString(HighlightSecondColor, out var secondColor))
                        HighlightManager.ProceduralParticles(anchor.GameObject, secondColor, 0, 0.05f,
                            SpawnRange, ParticleSize, Intensity);
                }
                
                anchor.Transform.SetParent(pickup.Transform);
                anchor.Spawn();
                
                anchor2?.Transform.SetParent(pickup.Transform);
                anchor2?.Spawn();
            }
            else
            {
                var anchor = HighlightManager.MakeLight(pickup.Position, Color.white,
                    LightShadows.None, HighlightRange, HighlightIntensity);
                
                if (EnableParticles)
                {
                    HighlightManager.ProceduralParticles(anchor.GameObject, Color.white, 0, 0.05f,
                        SpawnRange, ParticleSize, Intensity);
                    
                    if (ColorUtility.TryParseHtmlString(HighlightSecondColor, out var secondColor))
                        HighlightManager.ProceduralParticles(anchor.GameObject, Color.white, 0, 0.05f,
                            SpawnRange, ParticleSize, Intensity);
                }
                
                anchor.Transform.SetParent(pickup.Transform);
                anchor.Spawn();
                    
                Log.Warn("Установлен некорректный цвет подсветки, выбор значения по умолчанию..."); 
            }
        }
    } 

    private void OnChangingAttachments(ChangingAttachmentsEventArgs ev)
    {
        /*if (Check(ev.Item))
        {
            RueDisplay.Get(ev.Player).Show(
                new Tag(),
                new BasicElement(900, "<size=40><b><color=#C70000>Изменение модификаций на SCP-2158 недоступно!</color></b></size>"), 3);

            Timing.CallDelayed(3.1f, () => RueDisplay.Get(ev.Player).Update());
            ev.IsAllowed = false;
        }*/
    }

    private void OnChangedItem(ChangedItemEventArgs ev)
    {
        if (Check(ev.Item))
        {
            CoroutineRunner.Run(HintsHandler(ev.Player));
        }
    }

    public IEnumerator HintsHandler(Player player)
    {
        while (!Round.IsEnded && Round.IsStarted && player.IsAlive && Check(player.CurrentItem))
        {
            RueDisplay.Get(player).Show(
                new Tag(),
                new BasicElement(130, "<size=40><i><color=#00ffbf>Вы используете SCP-2158-ALT-1.\nПри стрельбе можно не целиться...</color></b></size>"), 1.1f);

            foreach (var spec in player.CurrentSpectatingPlayers)
            {
                RueDisplay.Get(spec).Show(
                    new Tag(),
                    new BasicElement(130, "<size=40><i><color=#00ffbf>Игрок использует SCP-2158-ALT-1!\n<i>~Анигилирующий автомат.</i></color></b></size>"), 1.1f);
                    
                Timing.CallDelayed(1.2f, () => RueDisplay.Get(spec).Update());
            }
            Timing.CallDelayed(1.2f, () => RueDisplay.Get(player).Update());

            yield return new WaitForSeconds(1f);
        }
    }
    
    private bool CheckCapsuleHit(
        Vector3 from,
        Vector3 to,
        float radius,
        out HitboxIdentity hitbox)
    {
        hitbox = null;

        var direction = (to - from).normalized;
        var distance = Vector3.Distance(from, to);

        float halfHeight = Mathf.Max(0.01f, distance * 0.5f - radius);

        var center = from + direction * (distance * 0.5f);

        var point1 = center + direction * halfHeight;
        var point2 = center - direction * halfHeight;

        // ReSharper disable once Unity.PreferNonAllocApi
        var hits = Physics.OverlapCapsule(
            point1,
            point2,
            radius,
            LayerMask.GetMask("Hitbox")
        );

        foreach (var col in hits)
        {
            hitbox = col.GetComponent<HitboxIdentity>();
            if (hitbox != null)
                return true;
        }

        return false;
    }
    
    private Primitive BulletTrace(
        Vector3 from,
        Vector3 to,
        float diameter,
        float lifetime = 1f,
        Color? colorOverride = null,
        float lightIntensity = 1f,
        bool collidable = false,
        bool isStatic = false)
    {
        var trace = Primitive.Create(PrimitiveType.Cylinder);

        var color = colorOverride ?? new Color(1f, 0, 0f, 0.5f);
        trace.Color = (color * lightIntensity) with { a = 0.5f };

        var direction = to - from;
        var distance = direction.magnitude;

        trace.Scale = new Vector3(
            diameter,
            distance * 0.5f,
            diameter
        );

        trace.Position = from + direction * 0.5f;
        trace.Rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);

        trace.Collidable = collidable;
        trace.IsStatic = isStatic;

        trace.Spawn();
        Timing.CallDelayed(lifetime, trace.Destroy);

        return trace;
    }
    
    private void CurvedBulletTrace(
        Vector3 start,
        Vector3 target,
        Vector3 forward,
        int segments = 16,
        float curveStrength = 0.8f,
        float radius = 0.055f,
        float duration = 0.6f,
        Color color = default,
        float size = 15f)
    {
        Vector3 lastPoint = start;

        float distance = Vector3.Distance(start, target);

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            
            Vector3 linearPoint = Vector3.Lerp(start, target, t);
            float curveOffset = Mathf.Sin(t * Mathf.PI) * curveStrength * distance;
            Vector3 curvedPoint =
                linearPoint + forward.normalized * curveOffset;

            BulletTrace(
                lastPoint,
                curvedPoint,
                radius,
                duration,
                color,
                size
            );

            lastPoint = curvedPoint;
        }
    }
}