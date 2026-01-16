using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.EnigmaVariations.Debuffs;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons
{
    /// <summary>
    /// Codex of Contradictions - Magic tome that casts conflicting elemental spells simultaneously
    /// Fire and Ice orbit each other, tearing reality where they intersect
    /// </summary>
    public class Enigma7 : ModItem
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.RazorbladeTyphoon;
        
        public override void SetDefaults()
        {
            Item.damage = 470;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 18;
            Item.width = 28;
            Item.height = 30;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 18);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<ContradictionOrb>();
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.staff[Type] = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect", "Casts Fire and Ice orbs that orbit each other in paradox"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "Where contradictions intersect, reality tears open"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'How can opposites exist as one? The Codex knows.'") 
            { 
                OverrideColor = EnigmaPurple 
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Create the paradox pair - fire and ice orbiting each other
            Vector2 perpendicular = velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * 25f;
            
            int fireOrb = Projectile.NewProjectile(source, position + perpendicular, velocity, 
                ModContent.ProjectileType<ContradictionOrb>(), damage, knockback, player.whoAmI, 0, 0);
            int iceOrb = Projectile.NewProjectile(source, position - perpendicular, velocity, 
                ModContent.ProjectileType<ContradictionOrb>(), damage, knockback, player.whoAmI, 1, fireOrb);
            
            // Link the fire orb to its ice partner
            if (fireOrb >= 0 && fireOrb < Main.maxProjectiles)
                Main.projectile[fireOrb].ai[1] = iceOrb;
            
            // Massive cast VFX - glyph magic circle
            CustomParticles.GlyphCircle(position, EnigmaPurple, count: 8, radius: 45f, rotationSpeed: 0.08f);
            CustomParticles.GenericFlare(position, EnigmaGreen, 0.9f, 20);
            CustomParticles.HaloRing(position, EnigmaPurple, 0.6f, 18);
            
            // Dual-element burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 30f;
                Color burstColor = i % 2 == 0 ? new Color(255, 120, 50) : new Color(100, 200, 255);
                CustomParticles.GenericFlare(position + offset, burstColor, 0.5f, 15);
            }
            
            // Dual-element sparkle formation - fire and ice converging
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 sparkPos = position + angle.ToRotationVector2() * 50f;
                Color sparkColor = i % 2 == 0 ? new Color(255, 120, 50) : new Color(100, 200, 255);
                CustomParticles.GenericFlare(sparkPos, sparkColor, 0.48f, 18);
                CustomParticles.GenericFlare(sparkPos, GetEnigmaGradient((float)i / 4f), 0.35f, 15);
            }
            
            // Music notes for the contradicting melodies - fire and ice in harmony
            ThemedParticles.EnigmaMusicNoteBurst(position, 8, 4f);
            
            return false;
        }
    }
    
    public class ContradictionOrb : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        private static readonly Color FireColor = new Color(255, 120, 50);
        private static readonly Color IceColor = new Color(100, 200, 255);
        
        private bool IsFire => Projectile.ai[0] == 0;
        private int PartnerIndex => (int)Projectile.ai[1];
        private float orbitTimer = 0f;
        private int realityTearCooldown = 0;
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Fireball;
        
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }
        
        public override void AI()
        {
            orbitTimer += 0.12f;
            realityTearCooldown--;
            
            Projectile partner = null;
            if (PartnerIndex >= 0 && PartnerIndex < Main.maxProjectiles)
            {
                partner = Main.projectile[PartnerIndex];
                if (!partner.active || partner.type != Type)
                    partner = null;
            }
            
            if (partner != null)
            {
                // Calculate orbit around midpoint between the two orbs
                Vector2 midpoint = (Projectile.Center + partner.Center) / 2f;
                float orbitRadius = 35f + (float)Math.Sin(orbitTimer * 0.5f) * 10f;
                float myAngle = IsFire ? orbitTimer : orbitTimer + MathHelper.Pi;
                
                Vector2 targetOrbitPos = midpoint + myAngle.ToRotationVector2() * orbitRadius;
                Vector2 toOrbit = targetOrbitPos - Projectile.Center;
                
                // Also move forward
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 8f + toOrbit * 0.15f;
                
                // Check for intersection - reality tear effect!
                float distToPartner = Vector2.Distance(Projectile.Center, partner.Center);
                if (distToPartner < 50f && realityTearCooldown <= 0)
                {
                    realityTearCooldown = 15;
                    CreateRealityTear(midpoint);
                }
                
                // Connecting paradox line between orbs
                if (Main.GameUpdateCount % 3 == 0)
                {
                    int linePoints = 5;
                    for (int i = 1; i < linePoints; i++)
                    {
                        float t = (float)i / linePoints;
                        Vector2 linePos = Vector2.Lerp(Projectile.Center, partner.Center, t);
                        Color lineColor = GetEnigmaGradient(t);
                        CustomParticles.GenericFlare(linePos, lineColor * 0.6f, 0.2f, 8);
                    }
                }
            }
            
            // Self-trailing particles
            Color myColor = IsFire ? FireColor : IceColor;
            if (Main.GameUpdateCount % 2 == 0)
            {
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.15f, 
                    myColor * 0.7f, 0.35f, 15, true);
                MagnumParticleHandler.SpawnParticle(glow);
                
                // Add enigma gradient overlay
                CustomParticles.GenericFlare(Projectile.Center, GetEnigmaGradient(Main.GameUpdateCount * 0.01f % 1f) * 0.4f, 0.25f, 10);
            }
            
            // Periodic orbiting sparkle chaos
            if (Main.GameUpdateCount % 40 == 0 && IsFire)
            {
                for (int i = 0; i < 2; i++)
                {
                    float orbitAngle = Main.GameUpdateCount * 0.05f + MathHelper.Pi * i;
                    Vector2 orbitPos = Projectile.Center + orbitAngle.ToRotationVector2() * 30f;
                    CustomParticles.GenericFlare(orbitPos, GetEnigmaGradient((float)i / 2f), 0.4f, 14);
                }
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, (myColor * 0.5f).ToVector3());
        }
        
        private void CreateRealityTear(Vector2 position)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.6f }, position);
            
            // Spawn reality tear projectile
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero,
                ModContent.ProjectileType<RealityTear>(), (int)(Projectile.damage * 1.5f), 0f, Projectile.owner);
            
            // Sparkle manifestation at intersection
            CustomParticles.GenericFlare(position, EnigmaGreen, 0.65f, 18);
            CustomParticles.HaloRing(position, EnigmaPurple * 0.7f, 0.35f, 14);
            
            // Paradox burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 offset = angle.ToRotationVector2() * 25f;
                Color tearColor = GetEnigmaGradient((float)i / 12f);
                CustomParticles.GenericFlare(position + offset, tearColor, 0.5f, 15);
            }
            
            // Glyphs at intersection
            CustomParticles.GlyphBurst(position, EnigmaPurple, count: 4, speed: 3f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 360);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 2);
            
            // Apply elemental debuff based on orb type
            if (IsFire)
                target.AddBuff(BuffID.OnFire, 180);
            else
                target.AddBuff(BuffID.Frostburn, 180);
            
            // === CONTRADICTION REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 3.5f, 12);
            
            // === NEW UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // Impact VFX
            Color hitColor = IsFire ? FireColor : IceColor;
            CustomParticles.GenericFlare(target.Center, hitColor, 0.7f, 15);
            CustomParticles.HaloRing(target.Center, GetEnigmaGradient(0.5f), 0.4f, 12);
            
            // Sparkle crown at struck target
            for (int crown = 0; crown < 4; crown++)
            {
                float crownAngle = MathHelper.Pi + MathHelper.Pi * crown / 4f - MathHelper.PiOver2;
                Vector2 crownPos = target.Center - new Vector2(0, 30f) + crownAngle.ToRotationVector2() * 18f;
                CustomParticles.GenericFlare(crownPos, GetEnigmaGradient((float)crown / 4f), 0.4f, 14);
            }
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // Glyph stack visualization
            int stacks = brandNPC.paradoxStacks;
            if (stacks > 0)
            {
                CustomParticles.GlyphStack(target.Center + new Vector2(0, -20f), EnigmaGreen, stacks, 0.22f);
            }
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === CONTRADICTION COLLAPSE REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 4f, 15);
            
            Color myColor = IsFire ? FireColor : IceColor;
            
            // Elemental explosion
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * (4f + Main.rand.NextFloat() * 3f);
                Color burstColor = Color.Lerp(myColor, GetEnigmaGradient((float)i / 10f), 0.5f);
                var glow = new GenericGlowParticle(Projectile.Center, vel, burstColor, 0.45f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            CustomParticles.HaloRing(Projectile.Center, EnigmaPurple, 0.5f, 15);
            
            // === WATCHING EYE at death - the contradiction resolves ===
            CustomParticles.EnigmaEyeGaze(Projectile.Center, EnigmaGreen * 0.7f, 0.4f, Projectile.velocity.SafeNormalize(Vector2.UnitX));
            
            // Elemental sparkle burst
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 burstVel = angle.ToRotationVector2() * 3f;
                var sparkle = new GenericGlowParticle(Projectile.Center, burstVel, GetEnigmaGradient((float)i / 4f), 0.38f, 15, true);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = IsFire ? 0.2f : -0.2f, Volume = 0.5f }, Projectile.Center);
        }
    }
    
    public class RealityTear : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;
        
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 45;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 45f);
            float intensity = (float)Math.Sin(lifeProgress * MathHelper.Pi);
            
            // Reality tear visual - jagged distortion pattern
            if (Main.GameUpdateCount % 2 == 0)
            {
                int tearPoints = 8;
                for (int i = 0; i < tearPoints; i++)
                {
                    float angle = MathHelper.TwoPi * i / tearPoints + Main.GameUpdateCount * 0.15f;
                    float tearRadius = 30f * intensity + Main.rand.NextFloat(-10f, 10f);
                    Vector2 tearPos = Projectile.Center + angle.ToRotationVector2() * tearRadius;
                    
                    Color tearColor = GetEnigmaGradient((float)i / tearPoints);
                    CustomParticles.GenericFlare(tearPos, tearColor * intensity, 0.4f, 10);
                    
                    // Inner glow
                    if (i % 2 == 0)
                    {
                        Vector2 innerPos = Projectile.Center + angle.ToRotationVector2() * tearRadius * 0.4f;
                        CustomParticles.GenericFlare(innerPos, EnigmaBlack, 0.5f * intensity, 8);
                    }
                }
            }
            
            // Center void pulse
            if (Main.GameUpdateCount % 4 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, EnigmaBlack, 0.7f * intensity, 12);
                CustomParticles.HaloRing(Projectile.Center, EnigmaPurple * intensity, 0.35f, 10);
            }
            
            // Sparkle wisps floating from within the tear
            if (Main.GameUpdateCount % 12 == 0)
            {
                Vector2 wispPos = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 wispVel = Main.rand.NextVector2Unit() * 2f;
                var wisp = new GenericGlowParticle(wispPos, wispVel, GetEnigmaGradient(Main.rand.NextFloat()) * intensity, 0.35f, 16, true);
                MagnumParticleHandler.SpawnParticle(wisp);
            }
            
            // Glyphs orbiting the tear
            if (Main.GameUpdateCount % 8 == 0)
            {
                CustomParticles.GlyphAura(Projectile.Center, EnigmaPurple * intensity, radius: 35f, count: 1);
            }
            
            Lighting.AddLight(Projectile.Center, GetEnigmaGradient(0.5f).ToVector3() * intensity * 0.8f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 3);
            
            // Reality damage - bypass some defense
            target.AddBuff(BuffID.BrokenArmor, 300);
            
            // === REALITY TEAR WARP (STRONGER) ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 5f, 18);
            FateRealityDistortion.TriggerInversionPulse(6);
            
            // === NEW UNIFIED VFX EXPLOSION ===
            UnifiedVFX.EnigmaVariations.Explosion(target.Center, 1.5f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // Heavy impact VFX
            CustomParticles.GenericFlare(target.Center, EnigmaGreen, 0.8f, 18);
            
            // Sparkle burst indicator above target
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.Pi + MathHelper.Pi * i / 5f - MathHelper.PiOver2;
                Vector2 burstPos = target.Center - new Vector2(0, 35f) + angle.ToRotationVector2() * 22f;
                CustomParticles.GenericFlare(burstPos, GetEnigmaGradient((float)i / 5f), 0.48f, 16);
            }
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            int stacks = brandNPC.paradoxStacks;
            CustomParticles.GlyphStack(target.Center + new Vector2(0, -25f), EnigmaGreen, stacks, 0.25f);
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === REALITY TEAR COLLAPSE ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 5f, 18);
            FateRealityDistortion.TriggerInversionPulse(6);
            
            // Reality snap-back
            for (int ring = 0; ring < 4; ring++)
            {
                Color ringColor = GetEnigmaGradient(ring / 4f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.6f - ring * 0.1f, 15 + ring * 3);
            }
            
            // Glyph explosion
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaPurple, count: 8, speed: 4f);
            
            // Sparkle scatter burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 scatterVel = angle.ToRotationVector2() * 4.5f;
                var sparkle = new GenericGlowParticle(Projectile.Center, scatterVel, GetEnigmaGradient((float)i / 6f), 0.42f, 18, true);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            ThemedParticles.EnigmaMusicNotes(Projectile.Center, 4, 35f);
            
            // === WATCHING EYES scatter - reality snaps back ===
            CustomParticles.EnigmaEyeExplosion(Projectile.Center, EnigmaPurple, 5, 3f);
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 0.7f }, Projectile.Center);
        }
    }
}
