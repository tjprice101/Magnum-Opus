using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons
{
    /// <summary>
    /// COSMIC CONDUIT STAFF - Magic Weapon #2
    /// 
    /// UNIQUE ABILITY: "EVENT HORIZON BEAM"
    /// Channels a massive continuous beam that pulls enemies toward its path
    /// while dealing damage. The beam creates visible gravitational lensing
    /// and heat wave distortions around it.
    /// 
    /// PASSIVE: While channeling, creates a black hole visual at the staff tip
    /// with swirling kaleidoscopic energy and massive lens flares.
    /// Enemies in the beam path slowly get dragged toward the beam's center.
    /// </summary>
    public class Fate9 : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.ApprenticeStaffT3;
        
        public override void SetDefaults()
        {
            Item.damage = 180;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 8;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 0f;
            Item.value = Item.sellPrice(gold: 27);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<EventHorizonBeam>();
            Item.shootSpeed = 0f;
            Item.noMelee = true;
            Item.channel = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Hold to channel the Event Horizon Beam"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect2", "The beam pulls enemies toward its gravitational path"));
            tooltips.Add(new TooltipLine(Mod, "FateEffect3", "Creates reality-warping visual distortions"));
            tooltips.Add(new TooltipLine(Mod, "FateLore", "'Where light itself cannot escape, neither can they'") { OverrideColor = FateLensFlare.FatePurple });
        }
        
        public override void HoldItem(Player player)
        {
            Vector2 staffTip = player.Center + new Vector2(player.direction * 35f, -15f);
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.2f + 0.8f;
            
            // Ambient singularity effect
            if (!player.channel && Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 particlePos = staffTip + angle.ToRotationVector2() * Main.rand.NextFloat(15f, 30f);
                Vector2 toCenter = (staffTip - particlePos).SafeNormalize(Vector2.Zero) * 1.5f;
                Color particleColor = FateLensFlare.GetFateGradient(Main.rand.NextFloat()) * 0.4f;
                
                var vortex = new GenericGlowParticle(particlePos, toCenter, particleColor, 0.12f, 15, true);
                MagnumParticleHandler.SpawnParticle(vortex);
            }
            
            // Lens flare ambient
            if (Main.GameUpdateCount % 35 == 0)
                FateLensFlareDrawLayer.AddFlare(staffTip, 0.2f * pulse, 0.25f, 10);
            
            Lighting.AddLight(staffTip, FateLensFlare.FatePurple.ToVector3() * 0.25f * pulse);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Only spawn beam if not already active
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<EventHorizonBeam>())
                    return false;
            }
            
            Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<EventHorizonBeam>(),
                damage, knockback, player.whoAmI);
            
            SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.5f, Volume = 0.5f }, position);
            
            return false;
        }
    }
    
    public class EventHorizonBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private const float BeamLength = 1200f;
        private const float BeamWidth = 60f;
        private const float GravityRadius = 150f;
        private const float PullStrength = 4f;
        
        private float beamAngle;
        private float intensity;
        
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Check if should stay alive
            if (!owner.active || owner.dead || !owner.channel || owner.noItems || owner.CCed)
            {
                Projectile.Kill();
                return;
            }
            
            // Drain mana
            if (owner.CheckMana(8, true, false))
            {
                owner.manaRegenDelay = 30;
            }
            else
            {
                Projectile.Kill();
                return;
            }
            
            // Update position and angle
            Vector2 staffTip = owner.Center + new Vector2(owner.direction * 35f, -15f);
            Projectile.Center = staffTip;
            
            beamAngle = (Main.MouseWorld - staffTip).ToRotation();
            owner.ChangeDir(Math.Sign(Main.MouseWorld.X - owner.Center.X));
            
            // Ramp up intensity
            intensity = Math.Min(intensity + 0.03f, 1f);
            
            // === SINGULARITY AT STAFF TIP ===
            DrawSingularityEffect(staffTip);
            
            // === THE BEAM ITSELF ===
            DrawBeamEffect(staffTip);
            
            // === GRAVITATIONAL PULL ===
            ApplyGravitationalPull(staffTip);
            
            // === PERSISTENT LENS FLARE ===
            if (Main.GameUpdateCount % 3 == 0)
                FateLensFlareDrawLayer.AddFlare(staffTip, 0.6f * intensity, 0.5f, 8);
            
            // Keep projectile alive
            Projectile.timeLeft = 2;
            
            // Sound loop
            if (Main.GameUpdateCount % 30 == 0)
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.3f, Volume = 0.3f }, staffTip);
        }
        
        private void DrawSingularityEffect(Vector2 center)
        {
            // Swirling particles being pulled in
            for (int i = 0; i < (int)(3 * intensity); i++)
            {
                float angle = Main.GameUpdateCount * 0.15f + Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = Main.rand.NextFloat(30f, 60f) * intensity;
                Vector2 particlePos = center + angle.ToRotationVector2() * radius;
                Vector2 velocity = (center - particlePos).SafeNormalize(Vector2.Zero) * 4f + angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 2f;
                Color vortexColor = FateLensFlare.GetFateGradient((angle / MathHelper.TwoPi + Main.GameUpdateCount * 0.01f) % 1f);
                
                var vortex = new GenericGlowParticle(particlePos, velocity, vortexColor * 0.5f, 0.15f * intensity, 12, true);
                MagnumParticleHandler.SpawnParticle(vortex);
            }
            
            // Central dark core
            CustomParticles.GenericFlare(center, FateLensFlare.FateBlack, 0.4f * intensity, 6);
            
            // Event horizon ring
            int ringParticles = (int)(12 * intensity);
            for (int i = 0; i < ringParticles; i++)
            {
                float ringAngle = MathHelper.TwoPi * i / ringParticles + Main.GameUpdateCount * 0.1f;
                Vector2 ringPos = center + ringAngle.ToRotationVector2() * 25f * intensity;
                Color ringColor = FateLensFlare.GetFateGradient((float)i / ringParticles);
                CustomParticles.GenericFlare(ringPos, ringColor * 0.4f, 0.1f, 5);
            }
            
            // Heat distortion around singularity
            FateLensFlare.DrawHeatWaveDistortion(center, 50f * intensity, 0.5f * intensity);
            
            Lighting.AddLight(center, FateLensFlare.FatePurple.ToVector3() * intensity * 0.6f);
        }
        
        private void DrawBeamEffect(Vector2 start)
        {
            Vector2 beamDir = beamAngle.ToRotationVector2();
            
            // Calculate actual beam end (check for tiles)
            Vector2 beamEnd = start + beamDir * BeamLength;
            for (float dist = 0; dist < BeamLength; dist += 16f)
            {
                Vector2 checkPos = start + beamDir * dist;
                Point tilePos = checkPos.ToTileCoordinates();
                if (WorldGen.SolidTile(tilePos.X, tilePos.Y))
                {
                    beamEnd = checkPos;
                    break;
                }
            }
            
            float actualLength = Vector2.Distance(start, beamEnd);
            
            // Beam particles along length
            int beamParticles = (int)(actualLength / 30f * intensity);
            for (int i = 0; i < beamParticles; i++)
            {
                float progress = (float)i / beamParticles;
                Vector2 beamPos = Vector2.Lerp(start, beamEnd, progress);
                
                // Add perpendicular offset for beam width
                Vector2 perpendicular = beamDir.RotatedBy(MathHelper.PiOver2);
                float offset = (float)Math.Sin(Main.GameUpdateCount * 0.1f + progress * 10f) * BeamWidth * 0.3f * intensity;
                beamPos += perpendicular * offset;
                
                Color beamColor = FateLensFlare.GetFateGradient((progress + Main.GameUpdateCount * 0.02f) % 1f);
                CustomParticles.GenericFlare(beamPos, beamColor * 0.5f * intensity, 0.2f, 8);
                
                // Chromatic aberration along beam
                if (Main.rand.NextBool(3))
                {
                    CustomParticles.GenericFlare(beamPos + perpendicular * 10f, Color.Red * 0.2f, 0.08f, 5);
                    CustomParticles.GenericFlare(beamPos - perpendicular * 10f, FateLensFlare.FateCyan * 0.2f, 0.08f, 5);
                }
            }
            
            // Heat wave distortion along beam
            if (Main.GameUpdateCount % 5 == 0)
            {
                Vector2 distortionPos = Vector2.Lerp(start, beamEnd, Main.rand.NextFloat());
                FateLensFlare.DrawHeatWaveDistortion(distortionPos, 40f * intensity, 0.3f * intensity);
            }
            
            // Kaleidoscopic burst at beam end
            FateLensFlare.KaleidoscopeBurst(beamEnd, 0.3f * intensity, 4);
            FateLensFlareDrawLayer.AddFlare(beamEnd, 0.3f * intensity, 0.3f, 6);
            
            // Lighting along beam
            for (float dist = 0; dist < actualLength; dist += 50f)
            {
                Vector2 lightPos = start + beamDir * dist;
                Lighting.AddLight(lightPos, FateLensFlare.FateBrightRed.ToVector3() * intensity * 0.3f);
            }
        }
        
        private void ApplyGravitationalPull(Vector2 start)
        {
            Vector2 beamDir = beamAngle.ToRotationVector2();
            
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.CountsAsACritter) continue;
                
                // Check if NPC is near the beam
                Vector2 toNPC = npc.Center - start;
                float distAlongBeam = Vector2.Dot(toNPC, beamDir);
                
                if (distAlongBeam < 0 || distAlongBeam > BeamLength) continue;
                
                Vector2 closestPointOnBeam = start + beamDir * distAlongBeam;
                float perpDist = Vector2.Distance(npc.Center, closestPointOnBeam);
                
                if (perpDist < GravityRadius)
                {
                    // Pull toward beam center
                    Vector2 pullDir = (closestPointOnBeam - npc.Center).SafeNormalize(Vector2.Zero);
                    float pullMagnitude = PullStrength * intensity * (1f - perpDist / GravityRadius);
                    
                    npc.velocity += pullDir * pullMagnitude;
                    
                    // Visual: distortion lines from NPC to beam
                    if (Main.rand.NextBool(4))
                    {
                        Color lineColor = FateLensFlare.GetFateGradient(Main.rand.NextFloat()) * 0.3f;
                        CustomParticles.GenericFlare(Vector2.Lerp(npc.Center, closestPointOnBeam, Main.rand.NextFloat()), lineColor, 0.1f, 6);
                    }
                }
            }
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 beamDir = beamAngle.ToRotationVector2();
            Vector2 start = Projectile.Center;
            Vector2 end = start + beamDir * BeamLength;
            
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, BeamWidth * intensity, ref point);
        }
        
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SourceDamage *= intensity;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 120);
            
            // Small impact VFX
            Color impactColor = FateLensFlare.GetFateGradient(Main.rand.NextFloat());
            CustomParticles.GenericFlare(target.Center, impactColor, 0.25f * intensity, 8);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Beam is drawn via particles, no sprite needed
            return false;
        }
        
        public override void OnKill(int timeLeft)
        {
            // Shutdown VFX
            FateLensFlareDrawLayer.AddFlare(Projectile.Center, 0.8f, 0.6f, 20);
            FateLensFlare.KaleidoscopeBurst(Projectile.Center, 0.6f, 6);
            
            CustomParticles.ExplosionBurst(Projectile.Center, FateLensFlare.FatePurple, 15, 6f);
            CustomParticles.HaloRing(Projectile.Center, FateLensFlare.FateDarkPink, 0.6f, 20);
        }
    }
}
