using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    /// THE FINAL QUESTION - Ultimate Enigma melee broadsword
    /// Creates dimensional slashes that persist briefly
    /// Every swing stacks "Inevitability" on ALL enemies on screen
    /// At max stacks: MASSIVE screen-wide paradox collapse
    /// Eyes form formations, glyph circles rotate at multiple radii
    /// </summary>
    public class Enigma12 : ModItem
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        
        private static int inevitabilityStacks = 0;
        private const int MaxInevitabilityStacks = 10;
        
        public static void AddInevitabilityStack()
        {
            inevitabilityStacks = Math.Min(inevitabilityStacks + 1, MaxInevitabilityStacks);
        }
        
        public static void ResetInevitability()
        {
            inevitabilityStacks = 0;
        }
        
        public static int GetInevitabilityStacks() => inevitabilityStacks;
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.Meowmere;
        
        public override void SetDefaults()
        {
            Item.damage = 580;
            Item.DamageType = DamageClass.Melee;
            Item.width = 60;
            Item.height = 60;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<DimensionalSlash>();
            Item.shootSpeed = 12f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect", "[Ultimate Enigma Weapon]") { OverrideColor = EnigmaGreen });
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "Creates dimensional slashes that persist and tear reality"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "Every swing stacks Inevitability on all enemies on screen"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect4", $"Current stacks: {inevitabilityStacks}/{MaxInevitabilityStacks}"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect5", "At max stacks triggers Paradox Collapse with massive devastation"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'What is the answer to everything? There is none. That is the final question.'") 
            { 
                OverrideColor = EnigmaPurple 
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Create dimensional slash
            Vector2 slashDir = velocity.SafeNormalize(Vector2.UnitX);
            float slashAngle = slashDir.ToRotation();
            
            Projectile.NewProjectile(source, player.Center + slashDir * 30f, velocity, type, damage, knockback, player.whoAmI, slashAngle);
            
            // Add inevitability to ALL enemies on screen
            bool anyEnemies = false;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.friendly && Vector2.Distance(npc.Center, player.Center) < 1200f)
                {
                    anyEnemies = true;
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 600);
                    var brandNPC = npc.GetGlobalNPC<ParadoxBrandNPC>();
                    brandNPC.AddParadoxStack(npc, 1);
                    
                    // Visual indicator of inevitability - mystical sparkles
                    if (Main.GameUpdateCount % 2 == 0)
                    {
                        Vector2 sparkPos = npc.Center - new Vector2(0, npc.height / 2f + 15f);
                        Color sparkColor = GetEnigmaGradient(Main.rand.NextFloat()) * 0.6f;
                        CustomParticles.GenericFlare(sparkPos, sparkColor, 0.35f, 12);
                        if (Main.rand.NextBool(3))
                        {
                            var sparkGlow = new GenericGlowParticle(sparkPos, Main.rand.NextVector2Circular(1.5f, 1.5f), sparkColor, 0.25f, 15, true);
                            MagnumParticleHandler.SpawnParticle(sparkGlow);
                        }
                    }
                }
            }
            
            if (anyEnemies)
            {
                AddInevitabilityStack();
            }
            
            // Swing VFX at player
            CustomParticles.GenericFlare(player.Center, EnigmaGreen, 0.7f, 15);
            CustomParticles.HaloRing(player.Center, EnigmaPurple, 0.4f, 12);
            
            // === MUSIC NOTES - The mystery's melody ===
            ThemedParticles.EnigmaMusicNotes(player.Center, 4, 35f);
            
            // Glyph stack showing inevitability
            if (inevitabilityStacks > 0)
            {
                CustomParticles.GlyphStack(player.Center - new Vector2(0, 50f), EnigmaGreen, inevitabilityStacks, 0.25f);
            }
            
            // Check for PARADOX COLLAPSE!
            if (inevitabilityStacks >= MaxInevitabilityStacks)
            {
                TriggerParadoxCollapse(player, source);
            }
            
            return false;
        }
        
        private void TriggerParadoxCollapse(Player player, IEntitySource source)
        {
            ResetInevitability();
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f, Volume = 1.2f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 1f }, player.Center);
            
            // Create the MASSIVE paradox collapse
            Projectile.NewProjectile(source, player.Center, Vector2.Zero,
                ModContent.ProjectileType<ParadoxCollapseUltimate>(), Item.damage * 3, 15f, player.whoAmI);
            
            // Screen-filling visual effects
            
            // MASSIVE sparkle formation around the player
            for (int outerIdx = 0; outerIdx < 12; outerIdx++)
            {
                float outerAngle = MathHelper.TwoPi * outerIdx / 12f + Main.GameUpdateCount * 0.01f;
                Vector2 outerPos = player.Center + outerAngle.ToRotationVector2() * 150f;
                CustomParticles.GenericFlare(outerPos, EnigmaGreen, 0.6f, 22);
                CustomParticles.HaloRing(outerPos, EnigmaPurple * 0.5f, 0.3f, 18);
            }
            for (int midIdx = 0; midIdx < 8; midIdx++)
            {
                float midAngle = MathHelper.TwoPi * midIdx / 8f - Main.GameUpdateCount * 0.015f;
                Vector2 midPos = player.Center + midAngle.ToRotationVector2() * 100f;
                CustomParticles.GenericFlare(midPos, EnigmaPurple, 0.55f, 20);
                CustomParticles.HaloRing(midPos, EnigmaGreen * 0.6f, 0.28f, 16);
            }
            // Orbiting sparkles at the outer edge
            for (int orbitIdx = 0; orbitIdx < 6; orbitIdx++)
            {
                float orbitAngle = Main.GameUpdateCount * 0.08f + MathHelper.TwoPi * orbitIdx / 6f;
                Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * 200f;
                CustomParticles.GenericFlare(orbitPos, EnigmaDeepPurple, 0.5f, 20);
                var orbitSparkle = new GenericGlowParticle(orbitPos, orbitAngle.ToRotationVector2() * 1.5f, GetEnigmaGradient((float)orbitIdx / 6f), 0.35f, 18, true);
                MagnumParticleHandler.SpawnParticle(orbitSparkle);
            }
            
            // Multiple glyph circles at different radii
            CustomParticles.GlyphCircle(player.Center, EnigmaPurple, count: 12, radius: 80f, rotationSpeed: 0.1f);
            CustomParticles.GlyphCircle(player.Center, EnigmaGreen, count: 16, radius: 130f, rotationSpeed: -0.08f);
            CustomParticles.GlyphCircle(player.Center, EnigmaDeepPurple, count: 20, radius: 180f, rotationSpeed: 0.06f);
            
            // Glyph tower at player
            CustomParticles.GlyphTower(player.Center, EnigmaPurple, layers: 6, baseScale: 0.6f);
            
            // Massive glyph explosion
            CustomParticles.GlyphBurst(player.Center, EnigmaGreen, count: 20, speed: 8f);
            
            // Fractal burst pattern
            for (int layer = 0; layer < 6; layer++)
            {
                int points = 8 + layer * 2;
                float radius = 50f + layer * 35f;
                
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points + layer * 0.2f;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    Color burstColor = GetEnigmaGradient((float)(layer * points + i) / (6 * points));
                    CustomParticles.GenericFlare(player.Center + offset, burstColor, 0.7f - layer * 0.08f, 25);
                }
            }
            // === MUSIC NOTES - THE PARADOX SYMPHONY ===
            // This is the ULTIMATE moment - fill the screen with music!
            ThemedParticles.EnigmaMusicNoteBurst(player.Center, 16, 8f);
            ThemedParticles.EnigmaMusicNotes(player.Center, 20, 100f);
            
            // Rising cascade of notes - the climax of the mystery
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 notePos = player.Center + angle.ToRotationVector2() * 60f;
                Color noteColor = ThemedParticles.GetEnigmaGradient((float)i / 12f);
                ThemedParticles.MusicNotes(notePos, noteColor, 2, 20f);
            }
        }
        
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Swing trail particles
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom));
                
                Color trailColor = GetEnigmaGradient(Main.rand.NextFloat());
                CustomParticles.GenericFlare(trailPos, trailColor * 0.7f, 0.35f, 12);
            }
            
            // Glyph along swing path
            if (Main.rand.NextBool(6))
            {
                Vector2 glyphPos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom));
                CustomParticles.Glyph(glyphPos, EnigmaPurple, 0.2f, -1);
            }
            
            // Music notes in swing trail - the blade sings
            if (Main.rand.NextBool(4))
            {
                Vector2 notePos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(15f, 15f);
                Color noteColor = ThemedParticles.GetEnigmaGradient(Main.rand.NextFloat());
                ThemedParticles.EnigmaMusicNotes(notePos, 2, 15f);
            }
        }
        
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 600);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 3);
            
            // === ULTIMATE BLADE REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 4f, 15);
            
            // === NEW UNIFIED VFX EXPLOSION ===
            UnifiedVFX.EnigmaVariations.Explosion(target.Center, 1.5f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // Heavy impact VFX
            CustomParticles.GenericFlare(target.Center, EnigmaGreen, 0.9f, 18);
            CustomParticles.HaloRing(target.Center, EnigmaPurple, 0.5f, 15);
            
            // Prismatic sparkle impact
            for (int spark = 0; spark < 6; spark++)
            {
                float sparkAngle = MathHelper.TwoPi * spark / 6f;
                Vector2 sparkVel = sparkAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color sparkColor = GetEnigmaGradient((float)spark / 6f);
                var sparkGlow = new GenericGlowParticle(target.Center - new Vector2(0, 35f), sparkVel, sparkColor, 0.4f, 20, true);
                MagnumParticleHandler.SpawnParticle(sparkGlow);
            }
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // Glyph impact
            CustomParticles.GlyphImpact(target.Center, EnigmaGreen, EnigmaPurple, 0.55f);
            
            // Glyph stack visualization
            int stacks = brandNPC.paradoxStacks;
            CustomParticles.GlyphStack(target.Center + new Vector2(0, -25f), EnigmaGreen, stacks, 0.25f);
            
            // Fractal burst at impact
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
                Vector2 offset = angle.ToRotationVector2() * 30f;
                Color burstColor = GetEnigmaGradient((float)i / 8f);
                CustomParticles.GenericFlare(target.Center + offset, burstColor, 0.5f, 15);
            }
        }
    }
    
    public class DimensionalSlash : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private float slashAngle => Projectile.ai[0];
        private List<Vector2> slashTrail = new List<Vector2>();
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float lifeProgress = 1f - (Projectile.timeLeft / 50f);
            float intensity = (float)Math.Sin(lifeProgress * MathHelper.Pi);
            float stretchScale = 1.5f;
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
            Texture2D glyphTex = CustomParticleSystem.Glyphs[(int)(Main.GameUpdateCount / 10) % 12].Value;
            Texture2D sparkleTex = CustomParticleSystem.PrismaticSparkles[(int)(Main.GameUpdateCount / 8) % 8].Value;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            
            // Draw stretched slash using EnergyFlare - layered for depth
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaDeepPurple * intensity * 0.85f, slashAngle, flareOrigin, new Vector2(stretchScale * 1.3f, 0.5f) * intensity, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaPurple * intensity * 0.75f, slashAngle, flareOrigin, new Vector2(stretchScale * 1.0f, 0.35f) * intensity, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreenFlame * intensity * 0.65f, slashAngle, flareOrigin, new Vector2(stretchScale * 0.7f, 0.22f) * intensity, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, Color.White * intensity * 0.4f, slashAngle, flareOrigin, new Vector2(stretchScale * 0.4f, 0.12f) * intensity, SpriteEffects.None, 0f);
            
            // Draw glyphs along the slash line
            Vector2 slashDir = slashAngle.ToRotationVector2();
            for (int i = -2; i <= 2; i++)
            {
                if (i == 0) continue; // Skip center
                float offset = i * 25f * intensity;
                Vector2 glyphPos = drawPos + slashDir * offset;
                Color glyphColor = Color.Lerp(EnigmaDeepPurple, EnigmaGreenFlame, (i + 2) / 4f) * intensity * 0.6f;
                float glyphRot = slashAngle + Main.GameUpdateCount * 0.08f * (i % 2 == 0 ? 1 : -1);
                spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, glyphRot, glyphTex.Size() / 2f, 0.18f * intensity, SpriteEffects.None, 0f);
            }
            
            // Draw sparkles perpendicular to slash
            Vector2 perpDir = slashDir.RotatedBy(MathHelper.PiOver2);
            for (int i = 0; i < 4; i++)
            {
                float perpOffset = ((i % 2 == 0) ? 1 : -1) * (12f + i * 5f) * intensity;
                Vector2 sparkPos = drawPos + perpDir * perpOffset;
                Color sparkColor = Color.Lerp(EnigmaPurple, EnigmaGreenFlame, (float)i / 4f) * intensity * 0.5f;
                spriteBatch.Draw(sparkleTex, sparkPos, null, sparkColor, Main.GameUpdateCount * 0.1f, sparkleTex.Size() / 2f, 0.1f * intensity, SpriteEffects.None, 0f);
            }
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 50;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 50f);
            float intensity = (float)Math.Sin(lifeProgress * MathHelper.Pi);
            
            // Record position for persisting slash trail
            if (slashTrail.Count < 15)
            {
                slashTrail.Add(Projectile.Center);
            }
            
            // Move forward
            Projectile.velocity *= 0.95f;
            
            // Draw the dimensional slash - a jagged tear in reality
            if (Main.GameUpdateCount % 2 == 0)
            {
                // Slash line particles
                Vector2 slashDir = slashAngle.ToRotationVector2();
                Vector2 perpendicular = slashDir.RotatedBy(MathHelper.PiOver2);
                
                int segments = 8;
                for (int i = 0; i < segments; i++)
                {
                    float t = ((float)i / segments - 0.5f) * 2f; // -1 to 1
                    float jaggedOffset = (float)Math.Sin(Main.GameUpdateCount * 0.3f + i * 2f) * 10f * intensity;
                    
                    Vector2 slashPos = Projectile.Center + slashDir * t * 50f + perpendicular * jaggedOffset;
                    Color slashColor = GetEnigmaGradient(((float)i / segments + lifeProgress) % 1f) * intensity;
                    
                    CustomParticles.GenericFlare(slashPos, slashColor, 0.4f + intensity * 0.3f, 10);
                    
                    // Void leaking through the slash
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 voidPos = slashPos + Main.rand.NextVector2Circular(15f, 15f);
                        CustomParticles.GenericFlare(voidPos, EnigmaBlack, 0.3f * intensity, 8);
                    }
                }
            }
            
            // Draw persisting trail
            if (Main.GameUpdateCount % 3 == 0)
            {
                for (int i = 0; i < slashTrail.Count; i++)
                {
                    float trailProgress = (float)i / slashTrail.Count;
                    Color trailColor = GetEnigmaGradient(trailProgress) * (0.5f * (1f - trailProgress)) * intensity;
                    CustomParticles.GenericFlare(slashTrail[i], trailColor, 0.25f, 8);
                }
            }
            
            // Arcane sparkle rifts in the dimensional tear
            if (Main.GameUpdateCount % 15 == 0 && intensity > 0.3f)
            {
                Vector2 riftPos = Projectile.Center + Main.rand.NextVector2Circular(30f, 30f);
                CustomParticles.GenericFlare(riftPos, EnigmaPurple * intensity, 0.5f, 15);
                CustomParticles.HaloRing(riftPos, EnigmaGreen * intensity * 0.5f, 0.25f, 12);
                for (int r = 0; r < 3; r++)
                {
                    Vector2 riftVel = slashAngle.ToRotationVector2().RotatedByRandom(0.5f) * Main.rand.NextFloat(1.5f, 3f);
                    var riftGlow = new GenericGlowParticle(riftPos, riftVel, EnigmaPurple * intensity * 0.7f, 0.3f, 18, true);
                    MagnumParticleHandler.SpawnParticle(riftGlow);
                }
            }
            
            // Glyphs along the slash
            if (Main.GameUpdateCount % 8 == 0)
            {
                Vector2 glyphPos = Projectile.Center + slashAngle.ToRotationVector2() * Main.rand.NextFloat(-40f, 40f);
                CustomParticles.Glyph(glyphPos, EnigmaGreen * intensity, 0.25f, -1);
            }
            
            Lighting.AddLight(Projectile.Center, GetEnigmaGradient(0.5f).ToVector3() * intensity * 0.6f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 2);
            
            // === DIMENSIONAL SLICE REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 4.5f, 15);
            FateRealityDistortion.TriggerInversionPulse(6);
            
            // === NEW UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // Dimensional slice impact
            CustomParticles.GenericFlare(target.Center, EnigmaGreen, 0.7f, 15);
            CustomParticles.HaloRing(target.Center, EnigmaPurple, 0.4f, 12);
            
            // Dazzling sparkle impact from dimensional slice
            for (int sparkle = 0; sparkle < 5; sparkle++)
            {
                float sAngle = MathHelper.TwoPi * sparkle / 5f;
                Vector2 sVel = sAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color sColor = GetEnigmaGradient((float)sparkle / 5f);
                var sGlow = new GenericGlowParticle(target.Center - new Vector2(0, 30f), sVel, sColor, 0.35f, 18, true);
                MagnumParticleHandler.SpawnParticle(sGlow);
            }
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // Glyph stack
            int stacks = brandNPC.paradoxStacks;
            CustomParticles.GlyphStack(target.Center + new Vector2(0, -22f), EnigmaGreen, stacks, 0.22f);
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === DIMENSIONAL TEAR CLOSING REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 5f, 18);
            FateRealityDistortion.TriggerInversionPulse(6);
            
            // Eyes watching the dimensional tear close
            CustomParticles.EnigmaEyeExplosion(Projectile.Center, EnigmaPurple, 4, 3f);
            
            // Dimensional tear closes
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                Color burstColor = GetEnigmaGradient((float)i / 10f);
                var glow = new GenericGlowParticle(Projectile.Center, vel, burstColor, 0.4f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            CustomParticles.HaloRing(Projectile.Center, EnigmaPurple, 0.5f, 15);
            // Sparkle cascade as tear closes
            for (int cascade = 0; cascade < 6; cascade++)
            {
                float cAngle = MathHelper.TwoPi * cascade / 6f;
                Vector2 cVel = cAngle.ToRotationVector2() * 3f;
                Color cColor = GetEnigmaGradient((float)cascade / 6f);
                var cGlow = new GenericGlowParticle(Projectile.Center, cVel, cColor, 0.4f, 20, true);
                MagnumParticleHandler.SpawnParticle(cGlow);
            }
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaPurple, count: 5, speed: 3f);
        }
    }
    
    public class ParadoxCollapseUltimate : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        
        private const float MaxExplosionRadius = 600f;
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            float lifeProgress = 1f - (Projectile.timeLeft / 90f);
            float intensity;
            float scale;
            
            // Three phases: buildup, explosion, fade
            if (lifeProgress < 0.2f)
            {
                float buildupProgress = lifeProgress / 0.2f;
                intensity = buildupProgress;
                scale = 2f - buildupProgress * 0.5f;
            }
            else if (lifeProgress < 0.5f)
            {
                float explosionProgress = (lifeProgress - 0.2f) / 0.3f;
                intensity = 1f;
                scale = 1.5f + explosionProgress * 4f;
            }
            else
            {
                float fadeProgress = (lifeProgress - 0.5f) / 0.5f;
                intensity = 1f - fadeProgress;
                scale = 5.5f + fadeProgress * 1.5f;
            }
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f + 1f;
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D eyeTex = CustomParticleSystem.EnigmaEyes[(int)(lifeProgress * 7) % 8].Value;
            Texture2D glyphTex = CustomParticleSystem.Glyphs[(int)(Main.GameUpdateCount / 6) % 12].Value;
            Texture2D sparkleTex = CustomParticleSystem.PrismaticSparkles[(int)(Main.GameUpdateCount / 5) % 8].Value;
            Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
            
            // Draw massive glyph ring - expanding outward
            int glyphCount = 16;
            float ringRadius = scale * 35f * pulse;
            for (int i = 0; i < glyphCount; i++)
            {
                float angle = Main.GameUpdateCount * 0.03f + MathHelper.TwoPi * i / glyphCount;
                Vector2 glyphPos = drawPos + angle.ToRotationVector2() * ringRadius;
                Color glyphColor = Color.Lerp(EnigmaDeepPurple, EnigmaGreenFlame, (float)i / glyphCount) * intensity * 0.7f;
                float glyphScale = 0.25f * (scale / 5f) * pulse;
                spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, angle * 2f + Main.GameUpdateCount * 0.05f, glyphTex.Size() / 2f, glyphScale, SpriteEffects.None, 0f);
            }
            
            // Draw inner sparkle ring
            for (int i = 0; i < 10; i++)
            {
                float angle = -Main.GameUpdateCount * 0.05f + MathHelper.TwoPi * i / 10f;
                float sparkRadius = ringRadius * 0.5f;
                Vector2 sparkPos = drawPos + angle.ToRotationVector2() * sparkRadius;
                Color sparkColor = Color.Lerp(EnigmaPurple, EnigmaGreenFlame, (float)i / 10f) * intensity * 0.6f;
                spriteBatch.Draw(sparkleTex, sparkPos, null, sparkColor, angle * 1.5f, sparkleTex.Size() / 2f, 0.15f * (scale / 5f) * pulse, SpriteEffects.None, 0f);
            }
            
            // Draw watching eyes at cardinal directions during buildup/explosion
            if (lifeProgress < 0.7f)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.PiOver2 * i + Main.GameUpdateCount * 0.02f;
                    Vector2 eyePos = drawPos + angle.ToRotationVector2() * ringRadius * 0.7f;
                    float eyeScale = 0.35f * intensity * (scale / 5f);
                    float eyeRot = (drawPos - eyePos).ToRotation() + MathHelper.PiOver2; // Looking inward
                    Color eyeColor = Color.Lerp(EnigmaPurple, EnigmaGreenFlame, (float)i / 4f) * intensity * 0.8f;
                    spriteBatch.Draw(eyeTex, eyePos, null, eyeColor, eyeRot, eyeTex.Size() / 2f, eyeScale, SpriteEffects.None, 0f);
                }
            }
            
            // Central flare core - massive and pulsing
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaDeepPurple * intensity * 0.85f, Main.GameUpdateCount * 0.04f, flareTex.Size() / 2f, 0.5f * scale * pulse * 0.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaPurple * intensity * 0.7f, -Main.GameUpdateCount * 0.05f, flareTex.Size() / 2f, 0.35f * scale * pulse * 0.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreenFlame * intensity * 0.6f, Main.GameUpdateCount * 0.06f, flareTex.Size() / 2f, 0.2f * scale * pulse * 0.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, Color.White * intensity * 0.5f, 0f, flareTex.Size() / 2f, 0.1f * scale * pulse * 0.3f, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 400;
            Projectile.height = 400;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 90f);
            float intensity;
            float currentRadius;
            
            // Three phases: buildup, explosion, fade
            if (lifeProgress < 0.2f)
            {
                // Buildup - implosion
                float buildupProgress = lifeProgress / 0.2f;
                intensity = buildupProgress;
                currentRadius = MaxExplosionRadius * (1f - buildupProgress * 0.5f);
                
                // Particles pulling inward
                if (Main.GameUpdateCount % 2 == 0)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = Main.GameUpdateCount * 0.1f + MathHelper.TwoPi * i / 12f;
                        Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * currentRadius;
                        Vector2 vel = (Projectile.Center - particlePos).SafeNormalize(Vector2.Zero) * 10f * buildupProgress;
                        
                        Color particleColor = GetEnigmaGradient((float)i / 12f) * intensity;
                        var glow = new GenericGlowParticle(particlePos, vel, particleColor, 0.5f, 15, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                }
                
                // Swirling arcane sparkles during buildup
                if (Main.GameUpdateCount % 8 == 0)
                {
                    float sparkAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * currentRadius * 0.7f;
                    Vector2 sparkVel = -sparkAngle.ToRotationVector2() * 3f;
                    Color sparkColor = GetEnigmaGradient(Main.rand.NextFloat()) * intensity;
                    CustomParticles.GenericFlare(sparkPos, sparkColor, 0.55f, 15);
                    var sparkGlow = new GenericGlowParticle(sparkPos, sparkVel, sparkColor * 0.8f, 0.4f, 18, true);
                    MagnumParticleHandler.SpawnParticle(sparkGlow);
                }
            }
            else if (lifeProgress < 0.5f)
            {
                // EXPLOSION PHASE
                float explosionProgress = (lifeProgress - 0.2f) / 0.3f;
                intensity = 1f;
                currentRadius = MaxExplosionRadius * explosionProgress;
                
                // Massive expanding ring of destruction
                if (Main.GameUpdateCount % 2 == 0)
                {
                    int points = 24;
                    for (int i = 0; i < points; i++)
                    {
                        float angle = MathHelper.TwoPi * i / points;
                        Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * currentRadius;
                        
                        Color particleColor = GetEnigmaGradient((float)i / points);
                        CustomParticles.GenericFlare(particlePos, particleColor, 0.7f, 12);
                        
                        // Inner ring
                        if (i % 2 == 0)
                        {
                            Vector2 innerPos = Projectile.Center + angle.ToRotationVector2() * currentRadius * 0.6f;
                            CustomParticles.GenericFlare(innerPos, EnigmaPurple, 0.5f, 10);
                        }
                    }
                    
                    // Radial beams
                    if (Main.GameUpdateCount % 4 == 0)
                    {
                        for (int beam = 0; beam < 8; beam++)
                        {
                            float beamAngle = MathHelper.TwoPi * beam / 8f + Main.GameUpdateCount * 0.02f;
                            int segments = 10;
                            for (int s = 0; s < segments; s++)
                            {
                                float t = (float)s / segments * explosionProgress;
                                Vector2 beamPos = Projectile.Center + beamAngle.ToRotationVector2() * (MaxExplosionRadius * t);
                                Color beamColor = GetEnigmaGradient(t) * (1f - t * 0.5f);
                                CustomParticles.GenericFlare(beamPos, beamColor, 0.4f, 8);
                            }
                        }
                    }
                }
                
                // Halo rings at various sizes
                if (Main.GameUpdateCount % 6 == 0)
                {
                    Color haloColor = GetEnigmaGradient(explosionProgress);
                    CustomParticles.HaloRing(Projectile.Center, haloColor, 0.8f * explosionProgress, 15);
                }
                
                // Prismatic sparkle explosion burst
                if (Main.GameUpdateCount % 5 == 0)
                {
                    for (int burst = 0; burst < 6; burst++)
                    {
                        float burstAngle = MathHelper.TwoPi * burst / 6f;
                        Vector2 burstVel = burstAngle.ToRotationVector2() * (6f * explosionProgress);
                        Color burstColor = GetEnigmaGradient((float)burst / 6f);
                        var burstGlow = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.5f, 22, true);
                        MagnumParticleHandler.SpawnParticle(burstGlow);
                    }
                }
                
                // Glyph circles rotating
                if (Main.GameUpdateCount % 10 == 0)
                {
                    CustomParticles.GlyphCircle(Projectile.Center, EnigmaPurple, count: 8, radius: currentRadius * 0.5f, rotationSpeed: 0.15f);
                }
            }
            else
            {
                // Fade phase
                float fadeProgress = (lifeProgress - 0.5f) / 0.5f;
                intensity = 1f - fadeProgress;
                currentRadius = MaxExplosionRadius;
                
                // Fading residual particles
                if (Main.GameUpdateCount % 4 == 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                        float radius = Main.rand.NextFloat() * currentRadius;
                        Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * radius;
                        
                        Color particleColor = GetEnigmaGradient(Main.rand.NextFloat()) * intensity * 0.5f;
                        CustomParticles.GenericFlare(particlePos, particleColor, 0.3f * intensity, 10);
                    }
                }
                
                // Glyphs fading
                if (Main.GameUpdateCount % 12 == 0)
                {
                    CustomParticles.GlyphAura(Projectile.Center, EnigmaPurple * intensity, radius: currentRadius * 0.4f, count: 2);
                }
            }
            
            // Central void pulsing throughout
            if (Main.GameUpdateCount % 4 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, EnigmaBlack, 1f * intensity, 15);
            }
            
            // Deal damage to all enemies in range
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.friendly)
                {
                    float dist = Vector2.Distance(npc.Center, Projectile.Center);
                    float damageRadius = lifeProgress < 0.2f ? MaxExplosionRadius : (lifeProgress < 0.5f ? currentRadius : MaxExplosionRadius);
                    
                    if (dist < damageRadius)
                    {
                        // Apply heavy paradox brand
                        npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 900);
                        var brandNPC = npc.GetGlobalNPC<ParadoxBrandNPC>();
                        
                        if (Main.GameUpdateCount % 10 == 0)
                        {
                            brandNPC.AddParadoxStack(npc, 2);
                        }
                    }
                }
            }
            
            Lighting.AddLight(Projectile.Center, GetEnigmaGradient(0.5f).ToVector3() * intensity);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 900);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 5);
            
            // === ULTIMATE PARADOX COLLAPSE REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 6f, 20);
            FateRealityDistortion.TriggerInversionPulse(8);
            
            // === NEW UNIFIED VFX EXPLOSION ===
            UnifiedVFX.EnigmaVariations.Explosion(target.Center, 2f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.6f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 12, 8f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 6, 45f);
            
            // Massive impact VFX
            CustomParticles.GenericFlare(target.Center, EnigmaGreen, 1f, 20);
            CustomParticles.HaloRing(target.Center, EnigmaPurple, 0.6f, 15);
            
            // Radiant sparkle impact
            for (int radiant = 0; radiant < 8; radiant++)
            {
                float radAngle = MathHelper.TwoPi * radiant / 8f;
                Vector2 radVel = radAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color radColor = GetEnigmaGradient((float)radiant / 8f);
                var radGlow = new GenericGlowParticle(target.Center - new Vector2(0, 40f), radVel, radColor, 0.45f, 22, true);
                MagnumParticleHandler.SpawnParticle(radGlow);
            }
            CustomParticles.HaloRing(target.Center - new Vector2(0, 40f), EnigmaGreen, 0.35f, 15);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 8, radius: 55f, rotationSpeed: 0.08f);
            
            // Glyph explosion at target
            CustomParticles.GlyphBurst(target.Center, EnigmaGreen, count: 6, speed: 4f);
            
            // Glyph stack visualization
            int stacks = brandNPC.paradoxStacks;
            CustomParticles.GlyphStack(target.Center + new Vector2(0, -30f), EnigmaPurple, stacks, 0.3f);
            
            // Fractal burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 offset = angle.ToRotationVector2() * 40f;
                Color burstColor = GetEnigmaGradient((float)i / 10f);
                CustomParticles.GenericFlare(target.Center + offset, burstColor, 0.6f, 15);
            }
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 1.2f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === ULTIMATE PARADOX COLLAPSE REALITY WARP - MAXIMUM DISTORTION ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 8f, 25);
            FateRealityDistortion.TriggerInversionPulse(10);
            FateRealityDistortion.TriggerScreenSlice(Projectile.Center - new Vector2(200, 200), Projectile.Center + new Vector2(200, 200), 5f, 20);
            
            // === THE ALL-SEEING FINALE - MAXIMUM EYE SPECTACLE ===
            // Central eye formation watching the collapse
            CustomParticles.EnigmaEyeFormation(Projectile.Center, EnigmaGreen, count: 6, radius: 80f);
            // Eyes exploding outward in grand finale
            CustomParticles.EnigmaEyeExplosion(Projectile.Center, EnigmaPurple, 8, 6f);
            // Secondary ring of eyes
            CustomParticles.EnigmaEyeExplosion(Projectile.Center, EnigmaGreen, 6, 4f);
            
            // Final collapse burst
            for (int layer = 0; layer < 4; layer++)
            {
                int points = 16 + layer * 4;
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points;
                    Vector2 vel = angle.ToRotationVector2() * (5f + layer * 3f);
                    Color burstColor = GetEnigmaGradient((float)(layer * points + i) / (4 * points));
                    var glow = new GenericGlowParticle(Projectile.Center, vel, burstColor, 0.6f - layer * 0.1f, 25, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }
            // Massive dazzling sparkle cascade - the grand finale
            for (int cascade = 0; cascade < 16; cascade++)
            {
                float cascadeAngle = MathHelper.TwoPi * cascade / 16f;
                Vector2 cascadeVel = cascadeAngle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                Color cascadeColor = GetEnigmaGradient((float)cascade / 16f);
                var cascadeGlow = new GenericGlowParticle(Projectile.Center, cascadeVel, cascadeColor, 0.6f, 28, true);
                MagnumParticleHandler.SpawnParticle(cascadeGlow);
            }
            // Secondary sparkle wave
            for (int wave = 0; wave < 12; wave++)
            {
                float waveAngle = MathHelper.TwoPi * wave / 12f + 0.15f;
                Vector2 waveVel = waveAngle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color waveColor = GetEnigmaGradient((float)wave / 12f) * 0.8f;
                var waveGlow = new GenericGlowParticle(Projectile.Center, waveVel, waveColor, 0.5f, 25, true);
                MagnumParticleHandler.SpawnParticle(waveGlow);
            }
            // Central prismatic flare
            CustomParticles.GenericFlare(Projectile.Center, EnigmaGreen, 1.2f, 25);
            CustomParticles.GenericFlare(Projectile.Center, EnigmaPurple, 0.9f, 22);
            
            // Massive glyph explosion
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaPurple, count: 16, speed: 7f);
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaGreen, count: 12, speed: 5f);
            
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.4f, Volume = 1.2f }, Projectile.Center);
        }
    }
}
