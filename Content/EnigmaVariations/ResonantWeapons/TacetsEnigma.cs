using Microsoft.Xna.Framework.Graphics;
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
    /// TACET'S ENIGMA - Enigma Ranged Repeater Gun
    /// ============================================
    /// UNIQUE MECHANICS:
    /// - Fast-firing ranged weapon that consumes bullets
    /// - Fires enigma-infused bullets with heavy particle trails
    /// - Every 5th shot fires a PARADOX BOLT that pierces and chains
    /// - Builds Paradox stacks on enemies - at 5 stacks they EXPLODE
    /// - Eyes watch from bullet trails and impact points
    /// - Weapon visually points toward cursor when firing
    /// </summary>
    public class TacetsEnigma : ModItem
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private int shotCounter = 0;
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetDefaults()
        {
            Item.damage = 380;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 60;
            Item.useTime = 11; // 50% faster than before
            Item.useAnimation = 11;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 18);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<TacetEnigmaShot>();
            Item.shootSpeed = 14f;
            Item.noMelee = true;
            Item.noUseGraphic = false; // Show weapon
            Item.useAmmo = AmmoID.Bullet;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect1", "Uses bullets as ammunition"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "Every 5th shot fires a piercing paradox bolt"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "Enemies at 5 Paradox stacks trigger an explosion"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'Each shot poses a silent question'") 
            { 
                OverrideColor = EnigmaPurple 
            });
        }
        
        public override void HoldItem(Player player)
        {
            // Point weapon toward cursor
            if (player.itemAnimation > 0)
            {
                Vector2 toMouse = Main.MouseWorld - player.Center;
                player.direction = toMouse.X > 0 ? 1 : -1;
            }
            
            // === AMBIENT ENIGMA AURA ===
            if (Main.GameUpdateCount % 5 == 0)
            {
                // Get weapon position - toward cursor
                Vector2 weaponPos = player.Center + (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * 30f;
                
                // Enigma particles around weapon
                Vector2 particleVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1f, -0.5f));
                Color particleColor = Color.Lerp(EnigmaGreen, EnigmaPurple, Main.rand.NextFloat());
                var glow = new GenericGlowParticle(weaponPos + Main.rand.NextVector2Circular(6f, 6f), 
                    particleVel, particleColor * 0.5f, Main.rand.NextFloat(0.15f, 0.25f), Main.rand.Next(10, 16), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Ambient enigma glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.15f + 0.85f;
            Lighting.AddLight(player.Center + new Vector2(player.direction * 25f, 0f), EnigmaGreen.ToVector3() * 0.3f * pulse);
        }
        
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            // Point the weapon toward the cursor when firing
            Vector2 toMouse = Main.MouseWorld - player.Center;
            float rotation = toMouse.ToRotation();
            
            // Set player arm rotation to point at cursor
            player.itemRotation = rotation;
            if (player.direction == -1)
                player.itemRotation += MathHelper.Pi;
                
            // Position the item at an offset from player center
            player.itemLocation = player.Center + toMouse.SafeNormalize(Vector2.UnitX) * 10f;
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            
            bool isParadoxShot = shotCounter >= 5;
            if (isParadoxShot)
                shotCounter = 0;
            
            // Spawn projectile
            int projType = isParadoxShot ? 
                ModContent.ProjectileType<TacetParadoxBolt>() : 
                ModContent.ProjectileType<TacetEnigmaShot>();
            
            Projectile.NewProjectile(source, position, velocity, projType, damage, knockback, player.whoAmI);
            
            // Muzzle flash VFX
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 20f;
            
            if (isParadoxShot)
            {
                // Enhanced muzzle flash for paradox shot
                CustomParticles.GenericFlare(muzzlePos, EnigmaGreen, 0.7f, 16);
                CustomParticles.GenericFlare(muzzlePos, EnigmaGreen, 0.55f, 14);
                CustomParticles.GlyphBurst(muzzlePos, EnigmaPurple, count: 4, speed: 3f);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 0.5f }, muzzlePos);
            }
            else
            {
                // Normal muzzle flash
                for (int i = 0; i < 4; i++)
                {
                    float angle = velocity.ToRotation() + MathHelper.PiOver4 * ((float)i / 4f - 0.5f);
                    Vector2 sparkVel = angle.ToRotationVector2() * 4f;
                    float progress = (float)i / 4f;
                    Color sparkColor = GetEnigmaGradient(progress);
                    var glow = new GenericGlowParticle(muzzlePos, sparkVel, sparkColor, 0.28f, 12, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                CustomParticles.GenericFlare(muzzlePos, EnigmaPurple, 0.45f, 12);
                CustomParticles.HaloRing(muzzlePos, EnigmaGreen * 0.5f, 0.22f, 10);
                
                // Music notes at muzzle
                if (Main.rand.NextBool(3))
                    ThemedParticles.EnigmaMusicNotes(muzzlePos, 2, 20f);
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// Normal bullet projectile for Tacet's Enigma
    /// </summary>
    public class TacetEnigmaShot : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = flareTex.Size() / 2f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.7f;
                float trailScale = (1f - trailProgress * 0.5f) * 0.35f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = GetEnigmaGradient(trailProgress);
                spriteBatch.Draw(flareTex, trailPos, null, trailColor * trailAlpha, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Draw main projectile
            float pulse = 0.9f + MathF.Sin(Main.GameUpdateCount * 0.2f) * 0.1f;
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreen * 0.9f, Projectile.rotation, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaPurple * 0.6f, Projectile.rotation, origin, 0.2f * pulse, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Trail particles
            if (Projectile.timeLeft % 3 == 0)
            {
                Color trailColor = GetEnigmaGradient(Main.rand.NextFloat());
                CustomParticles.GenericFlare(Projectile.Center, trailColor * 0.5f, 0.25f, 10);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.35f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 240);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 1);
            
            int stacks = brandNPC.paradoxStacks;
            
            // At 5 stacks: EXPLOSION
            if (stacks >= 5)
            {
                TriggerParadoxExplosion(target, brandNPC);
            }
            
            // Impact VFX
            CustomParticles.GenericFlare(target.Center, EnigmaGreen, 0.5f, 14);
            CustomParticles.HaloRing(target.Center, EnigmaPurple * 0.6f, 0.3f, 12);
            
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 offset = angle.ToRotationVector2() * 20f;
                CustomParticles.GenericFlare(target.Center + offset, GetEnigmaGradient((float)i / 4f), 0.35f, 12);
            }
            
            // Glyph stack display
            if (stacks > 0 && stacks < 5)
            {
                CustomParticles.GlyphStack(target.Center + new Vector2(0, -25f), EnigmaGreen, stacks, 0.25f);
            }
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.6f);
        }
        
        private void TriggerParadoxExplosion(NPC target, ParadoxBrandNPC brandNPC)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.8f }, target.Center);
            
            brandNPC.paradoxStacks = 0;
            
            float explosionRadius = 150f;
            
            // Central flash
            CustomParticles.GenericFlare(target.Center, EnigmaGreen, 1.0f, 22);
            CustomParticles.GenericFlare(target.Center, EnigmaGreen, 0.8f, 20);
            
            // Fractal burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 35f;
                CustomParticles.GenericFlare(target.Center + offset, GetEnigmaGradient((float)i / 8f), 0.5f, 18);
            }
            
            // Halos
            for (int ring = 0; ring < 3; ring++)
            {
                Color ringColor = Color.Lerp(EnigmaPurple, EnigmaGreen, ring / 3f) * 0.7f;
                CustomParticles.HaloRing(target.Center, ringColor, 0.4f + ring * 0.15f, 16 + ring * 4);
            }
            
            // Glyphs
            CustomParticles.GlyphBurst(target.Center, EnigmaPurple, count: 6, speed: 4f);
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 6, 4f);
            
            // Damage nearby enemies
            Player owner = Main.player[Projectile.owner];
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.whoAmI == target.whoAmI) continue;
                
                float dist = Vector2.Distance(npc.Center, target.Center);
                if (dist <= explosionRadius)
                {
                    float falloff = 1f - (dist / explosionRadius) * 0.4f;
                    int explosionDamage = (int)(Projectile.damage * 1.5f * falloff);
                    npc.SimpleStrikeNPC(explosionDamage, owner.direction, true, 8f);
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 180);
                    npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 1);
                    
                    MagnumVFX.DrawFractalLightning(target.Center, npc.Center, EnigmaGreen, 8, 20f, 3, 0.3f);
                }
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // Death burst
            CustomParticles.GenericFlare(Projectile.Center, EnigmaGreen, 0.45f, 14);
            
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                var glow = new GenericGlowParticle(Projectile.Center, vel, GetEnigmaGradient((float)i / 4f), 0.3f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }
    }
    
    /// <summary>
    /// Paradox bolt - every 5th shot, pierces and chains
    /// </summary>
    public class TacetParadoxBolt : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private List<int> hitEnemies = new List<int>();
        
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1; // Infinite pierce
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = flareTex.Size() / 2f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw trail with glyphs
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.8f;
                float trailScale = (1f - trailProgress * 0.5f) * 0.4f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = GetEnigmaGradient(trailProgress);
                
                spriteBatch.Draw(flareTex, trailPos, null, trailColor * trailAlpha, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
                
                // Glyph accents
                if (i % 4 == 0)
                {
                    spriteBatch.Draw(glyphTex, trailPos, null, EnigmaPurple * trailAlpha * 0.5f,
                        Main.GameUpdateCount * 0.05f + i, glyphTex.Size() / 2f, trailScale * 0.5f, SpriteEffects.None, 0f);
                }
            }
            
            // Draw main projectile - bigger and more intense
            float pulse = 0.9f + MathF.Sin(Main.GameUpdateCount * 0.25f) * 0.15f;
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaPurple * 0.9f, Projectile.rotation, origin, 0.55f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreen * 0.7f, Projectile.rotation, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreen * 0.6f, Projectile.rotation, origin, 0.2f * pulse, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Heavy trail particles
            if (Projectile.timeLeft % 2 == 0)
            {
                Color trailColor = GetEnigmaGradient(Main.rand.NextFloat());
                CustomParticles.GenericFlare(Projectile.Center, trailColor * 0.6f, 0.35f, 12);
                
                var glow = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    -Projectile.velocity * 0.1f, trailColor * 0.5f, 0.25f, 10, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Glyph trail
            if (Projectile.timeLeft % 6 == 0)
            {
                CustomParticles.GlyphTrail(Projectile.Center, Projectile.velocity, EnigmaPurple * 0.6f, 0.25f);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.5f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 2); // Heavy stacking
            
            hitEnemies.Add(target.whoAmI);
            
            // Chain to all previously hit enemies
            foreach (int npcIndex in hitEnemies)
            {
                if (npcIndex == target.whoAmI) continue;
                if (npcIndex < 0 || npcIndex >= Main.maxNPCs) continue;
                
                NPC other = Main.npc[npcIndex];
                if (!other.active || other.friendly) continue;
                
                float dist = Vector2.Distance(target.Center, other.Center);
                if (dist > 300f) continue;
                
                MagnumVFX.DrawFractalLightning(target.Center, other.Center, EnigmaGreen, 10, 25f, 4, 0.35f);
            }
            
            // Impact VFX
            CustomParticles.GenericFlare(target.Center, EnigmaGreen, 0.7f, 18);
            CustomParticles.GenericFlare(target.Center, EnigmaGreen, 0.6f, 16);
            
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 30f;
                CustomParticles.GenericFlare(target.Center + offset, GetEnigmaGradient((float)i / 6f), 0.45f, 14);
            }
            
            CustomParticles.HaloRing(target.Center, EnigmaPurple, 0.45f, 16);
            CustomParticles.GlyphBurst(target.Center, EnigmaGreen, count: 4, speed: 3f);
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 4, 4f);
            
            int stacks = brandNPC.paradoxStacks;
            if (stacks >= 5)
            {
                // Trigger explosion
                brandNPC.paradoxStacks = 0;
                
                CustomParticles.GenericFlare(target.Center, EnigmaGreen, 1.2f, 25);
                CustomParticles.GlyphBurst(target.Center, EnigmaGreen, count: 8, speed: 5f);
                
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 burstVel = angle.ToRotationVector2() * 5f;
                    var sparkle = new GenericGlowParticle(target.Center, burstVel, GetEnigmaGradient((float)i / 8f), 0.45f, 18, true);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // Damage nearby
                Player owner = Main.player[Projectile.owner];
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.friendly || npc.whoAmI == target.whoAmI) continue;
                    float dist = Vector2.Distance(npc.Center, target.Center);
                    if (dist <= 150f)
                    {
                        npc.SimpleStrikeNPC(Projectile.damage, owner.direction, true, 8f);
                        npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 2);
                    }
                }
            }
            else if (stacks > 0)
            {
                CustomParticles.GlyphStack(target.Center + new Vector2(0, -28f), EnigmaGreen, stacks, 0.28f);
            }
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // End burst
            CustomParticles.GenericFlare(Projectile.Center, EnigmaGreen, 0.55f, 16);
            
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 4f;
                var glow = new GenericGlowParticle(Projectile.Center, vel, GetEnigmaGradient((float)i / 6f), 0.35f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaPurple, count: 4, speed: 3f);
        }
    }
}
