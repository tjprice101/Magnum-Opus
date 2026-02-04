using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons
{
    #region Piercing Bell's Resonance (Gun)
    
    /// <summary>
    /// Piercing Bell's Resonance - Ultra rapid-fire gun with bell chime tempo.
    /// Fires blazing musical bullets at extreme speeds in the tempo of a bell's chime.
    /// Special: Scorching Staccato - sustained fire accelerates to insane speeds, 
    /// bullets leave trails of music notes that detonate on hit.
    /// Bell-ringing explosions cascade to nearby enemies until trigger released.
    /// Secondary: Every 20th bullet fires a massive RESONANT BELL BLAST that 
    /// spawns homing music note projectiles.
    /// </summary>
    public class PiercingBellsResonance : ModItem
    {
        private int rapidFireCounter = 0;
        private float fireRateBonus = 0f;
        private int resonantBlastCounter = 0;
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 165;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 30;
            Item.useTime = 4; // ULTRA FAST - was 12
            Item.useAnimation = 4;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2.5f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 20f; // Faster bullets
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Ultra rapid-fire gun that accelerates with sustained fire"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 20th shot fires a resonant bell blast with homing music notes"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Bullets leave trails of detonating music notes"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The bell's echo shatters through a storm of blazing staccato'") { OverrideColor = new Color(255, 140, 40) });
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Convert to fire bullet
            type = ModContent.ProjectileType<BellFireBullet>();
            
            // Increase fire rate with sustained fire (Scorching Staccato) - MORE AGGRESSIVE
            rapidFireCounter++;
            fireRateBonus = Math.Min(rapidFireCounter * 0.02f, 0.6f); // Max 60% faster (was 40%)
        }

        public override float UseSpeedMultiplier(Player player)
        {
            return 1f + fireRateBonus;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire main bullet
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // Track for Resonant Bell Blast
            resonantBlastCounter++;
            
            // === RESONANT BELL BLAST - Every 20th shot! ===
            if (resonantBlastCounter >= 20)
            {
                resonantBlastCounter = 0;
                TriggerResonantBellBlast(player, source, position, velocity, damage, knockback);
            }
            
            // Spawn trailing music notes on every 5th bullet
            if (rapidFireCounter % 5 == 0)
            {
                Projectile.NewProjectile(source, position, velocity * 0.7f,
                    ModContent.ProjectileType<TrailingMusicNote>(), damage / 3, knockback * 0.3f, player.whoAmI);
            }
            
            // === BELL SOUND ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.2f + fireRateBonus * 0.5f + Main.rand.NextFloat(-0.1f, 0.1f), Volume = 0.35f }, position);
            
            // === MASSIVE MUZZLE FLASH ===
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 30f;
            ThemedParticles.LaCampanellaSparks(muzzlePos, velocity.SafeNormalize(Vector2.Zero), 4, 5f);
            
            // Occasional bloom burst
            if (Main.rand.NextBool(3))
            {
                ThemedParticles.LaCampanellaBloomBurst(muzzlePos, 0.25f + fireRateBonus * 0.15f);
            }
            
            // Halo ring muzzle flash
            if (Main.rand.NextBool(2))
            {
                CustomParticles.HaloRing(muzzlePos, ThemedParticles.CampanellaOrange, 0.15f + fireRateBonus * 0.1f, 8);
            }
            
            // Fire glow particles
            if (Main.rand.NextBool(2))
            {
                Color color = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                var glow = new GenericGlowParticle(muzzlePos + Main.rand.NextVector2Circular(8f, 8f),
                    velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 4f),
                    color, Main.rand.NextFloat(0.2f, 0.35f), Main.rand.Next(6, 12), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Music note muzzle flash at high fire rate
            if (fireRateBonus > 0.3f && Main.rand.NextBool(4))
            {
                ThemedParticles.LaCampanellaMusicNotes(muzzlePos, 1, 15f);
            }
            
            // Screen shake on rapid fire
            // REMOVED: Screen shake disabled for La Campanella weapons
            // if (fireRateBonus > 0.3f)
            // {
            //     player.GetModPlayer<ScreenShakePlayer>()?.AddShake(0.5f + fireRateBonus * 2f, 2);
            // }
            
            Lighting.AddLight(muzzlePos, 0.6f, 0.3f, 0.08f);
            
            return false;
        }
        
        private void TriggerResonantBellBlast(Player player, IEntitySource source, Vector2 position, Vector2 velocity, int damage, float knockback)
        {
            // === RESONANT BELL BLAST - Massive special shot! ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.7f }, position);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.3f, Volume = 0.4f }, position);
            
            // Fire a massive bullet
            int proj = Projectile.NewProjectile(source, position, velocity * 1.5f,
                ModContent.ProjectileType<ResonantBellBlast>(), damage * 3, knockback * 2f, player.whoAmI);
            
            // Spawn homing music notes
            for (int i = 0; i < 5; i++)
            {
                float angle = (i - 2) * 0.3f;
                Vector2 noteVel = velocity.RotatedBy(angle) * 0.6f;
                Projectile.NewProjectile(source, position, noteVel,
                    ModContent.ProjectileType<HomingMusicNote>(), damage / 2, knockback * 0.5f, player.whoAmI);
            }
            
            // === SEEKING INFERNAL CRYSTALS - La Campanella Fire Crystals ===
            SeekingCrystalHelper.SpawnLaCampanellaCrystals(
                source, position, velocity * 0.5f, (int)(damage * 0.45f), knockback, player.whoAmI, 6);
            
            // Massive visual effects
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 40f;
            ThemedParticles.LaCampanellaBloomBurst(muzzlePos, 0.8f);
            ThemedParticles.LaCampanellaShockwave(muzzlePos, 0.6f);
            ThemedParticles.LaCampanellaMusicNotes(muzzlePos, 5, 35f);
            
            // Single halo ring
            CustomParticles.HaloRing(muzzlePos, ThemedParticles.CampanellaOrange, 0.5f, 18);
            
            // REMOVED: Screen shake disabled for La Campanella weapons
            // player.GetModPlayer<ScreenShakePlayer>()?.AddShake(5f, 10);
        }

        public override void HoldItem(Player player)
        {
            // Reset rapid fire when not shooting
            if (!player.controlUseItem)
            {
                rapidFireCounter = 0;
                fireRateBonus = 0f;
            }
            
            // === SIGNATURE HOLD AURA - VIBRANT PARTICLES WHILE HELD! ===
            ThemedParticles.LaCampanellaHoldAura(player.Center, 0.7f);
            
            // Ambient glow
            if (Main.rand.NextBool(12))
            {
                Dust ambient = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(20f, 20f),
                    DustID.Torch, Vector2.Zero, 100, ThemedParticles.CampanellaOrange, 0.8f);
                ambient.noGravity = true;
            }
            
            Lighting.AddLight(player.Center, 0.4f, 0.2f, 0.05f);
        }
    }

    /// <summary>
    /// Bell fire bullet that cascades explosions on hit.
    /// Uses the gun sprite scaled down for bullet visual.
    /// </summary>
    public class BellFireBullet : ModProjectile
    {
        // Use the gun weapon as the bullet sprite
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/PiercingBellsResonance";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // === MUSIC NOTE TRAIL - Every bullet leaves musical energy! ===
            if (Main.rand.NextBool(3))
            {
                ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 1, 10f);
            }
            
            // ☁EMUSICAL NOTATION - Fiery music note trail
            if (Main.rand.NextBool(6))
            {
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(255, 200, 50), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.35f, 35);
            }
            
            // === FLAMING DARK SMOKE TRAIL - LIKE SWAN LAKE! ===
            
            // Heavy black smoke trail with golden sparkles
            if (Main.rand.NextBool(3))
            {
                var smoke = new HeavySmokeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f),
                    ThemedParticles.CampanellaBlack,
                    Main.rand.Next(20, 35),
                    Main.rand.NextFloat(0.2f, 0.35f),
                    0.6f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Golden/orange fire glow trail with GRADIENT
            if (Main.rand.NextBool(2))
            {
                float progress = Main.rand.NextFloat();
                Color trailColor = Color.Lerp(Color.Lerp(ThemedParticles.CampanellaOrange, ThemedParticles.CampanellaYellow, progress * 2f),
                    ThemedParticles.CampanellaGold, Math.Max(0, progress * 2f - 1f));
                var glow = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                    trailColor, Main.rand.NextFloat(0.15f, 0.28f), Main.rand.Next(6, 12), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Frequent prismatic sparkles along trail
            if (Main.rand.NextBool(3))
            {
                CustomParticles.PrismaticSparkle(Projectile.Center, ThemedParticles.CampanellaGold, 0.25f);
            }
            
            // === BLAZING FIRE TRAIL ===
            ThemedParticles.LaCampanellaTrail(Projectile.Center, Projectile.velocity);
            
            // Extra sparks
            if (Main.rand.NextBool(3))
            {
                ThemedParticles.LaCampanellaSparks(Projectile.Center, -Projectile.velocity.SafeNormalize(Vector2.UnitX), 2, 3f);
            }
            
            Lighting.AddLight(Projectile.Center, 0.5f, 0.25f, 0.06f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // === SEEKING CRYSTALS - 25% chance on hit ===
            if (Main.rand.NextBool(4))
            {
                SeekingCrystalHelper.SpawnLaCampanellaCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.18f),
                    Projectile.knockBack,
                    Projectile.owner,
                    3
                );
            }
            
            // ☁EMUSICAL IMPACT - Burst of music notes on hit
            ThemedParticles.MusicNoteBurst(target.Center, new Color(255, 140, 40), 4, 3f);
            
            // === SIGNATURE HIT EFFECT ===
            Vector2 hitDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            ThemedParticles.LaCampanellaSignatureHit(target.Center, hitDir, 0.6f);
            
            // Cascade explosion to nearby enemies
            CascadeExplosion(target.Center);
        }

        private void CascadeExplosion(Vector2 position)
        {
            // === BELL EXPLOSION SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.3f, 0.6f), Volume = 0.4f }, position);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = 0.5f, Volume = 0.2f }, position);
            
            // === EXPLOSIVE BLACK SMOKE BURST ===
            for (int i = 0; i < 3; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(4f, 4f) + new Vector2(0, -1.5f);
                var smoke = new HeavySmokeParticle(
                    position + Main.rand.NextVector2Circular(12f, 12f),
                    smokeVel, ThemedParticles.CampanellaBlack,
                    Main.rand.Next(35, 55), Main.rand.NextFloat(0.35f, 0.55f),
                    0.7f, 0.015f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // === EXPLOSION EFFECTS ===
            ThemedParticles.LaCampanellaBloomBurst(position, 0.7f);
            ThemedParticles.LaCampanellaSparks(position, Main.rand.NextVector2Unit(), 6, 6f);
            ThemedParticles.LaCampanellaMusicNotes(position, 3, 20f);
            
            // === SINGLE HALO RING ===
            CustomParticles.HaloRing(position, ThemedParticles.CampanellaOrange, 0.45f, 15);
            
            Lighting.AddLight(position, 1.2f, 0.6f, 0.15f);
            
            // Cascade to nearby enemies (only if player is still firing)
            Player owner = Main.player[Projectile.owner];
            if (owner.controlUseItem)
            {
                float cascadeRadius = 100f;
                foreach (NPC npc in Main.npc)
                {
                    if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                    if (npc.Center == position) continue;
                    
                    if (Vector2.Distance(position, npc.Center) <= cascadeRadius)
                    {
                        // Small cascade damage
                        npc.SimpleStrikeNPC(Projectile.damage / 3, 0, false, 0f, null, false, 0f, true);
                        npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 1);
                        
                        // Cascade hit effect - more dramatic!
                        ThemedParticles.LaCampanellaSparks(npc.Center, (npc.Center - position).SafeNormalize(Vector2.UnitX), 6, 6f);
                        CustomParticles.HaloRing(npc.Center, ThemedParticles.CampanellaOrange, 0.35f, 12);
                        CustomParticles.GenericFlare(npc.Center, ThemedParticles.CampanellaGold, 0.3f, 10);
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // === SPECTRAL TRAIL ===
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = 1f - (i / (float)Projectile.oldPos.Length);
                Color trailColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, progress) * progress * 0.4f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                Main.EntitySpriteDraw(texture, trailPos, null, trailColor, Projectile.rotation, origin,
                    0.15f * (0.4f + progress * 0.6f), SpriteEffects.None, 0);
            }
            
            // === ADDITIVE GLOW ===
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Main.EntitySpriteDraw(texture, drawPos, null, ThemedParticles.CampanellaOrange * 0.5f, Projectile.rotation, origin,
                0.2f, SpriteEffects.None, 0);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Main bullet sprite
            Main.EntitySpriteDraw(texture, drawPos, null, Color.White, Projectile.rotation, origin,
                0.12f, SpriteEffects.None, 0);
            
            return false;
        }
    }
    
    #endregion

    #region Grandiose Chime (Rifle)
    
    /// <summary>
    /// Grandiose Chime - High-power bell-infused beam rifle with EXTREME fire rate.
    /// Fires beams of high-power, bell-infused musical energy at blistering speeds.
    /// Special: Bellfire Barrage - every third shot releases inferno spread of burning music notes.
    /// Secondary: HARMONIC CONVERGENCE - beams leave behind music note mines that 
    /// detonate in chain reactions when enemies approach.
    /// Kills create "Resonant Echoes" - ghostly music notes that seek new targets.
    /// </summary>
    public class GrandioseChime : ModItem
    {
        private int shotCounter = 0;
        private int echoChargeCounter = 0;
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 240;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 70;
            Item.height = 30;
            Item.useTime = 6; // ULTRA FAST - was 25
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.UseSound = SoundID.Item75;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 24f; // Faster projectiles
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires high-power bell-infused energy beams at extreme speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every third shot triggers a bellfire barrage of burning music notes"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Leaves behind music note mines that detonate in chain reactions"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Kills create resonant echoes that seek new targets"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each beam carries the grandeur of a thousand chiming bells'") { OverrideColor = new Color(255, 140, 40) });
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<BellEnergyBeam>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            echoChargeCounter++;
            
            // Main beam
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // Leave behind music note mines every 4 shots (Harmonic Convergence)
            if (shotCounter % 4 == 0)
            {
                Vector2 minePos = position + velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(60f, 120f);
                Projectile.NewProjectile(source, minePos, Vector2.Zero,
                    ModContent.ProjectileType<MusicNoteMine>(), damage / 2, knockback * 0.3f, player.whoAmI);
            }
            
            // === SOUND EFFECTS - Less frequent for fast fire ===
            if (Main.rand.NextBool(3))
            {
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(-0.1f, 0.3f), Volume = 0.4f }, position);
            }
            if (Main.rand.NextBool(4))
            {
                SoundEngine.PlaySound(SoundID.Item75 with { Pitch = 0.3f, Volume = 0.2f }, position);
            }
            
            // === MUZZLE EFFECTS ===
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 40f;
            ThemedParticles.LaCampanellaSparks(muzzlePos, velocity.SafeNormalize(Vector2.Zero), 3, 5f);
            
            if (Main.rand.NextBool(3))
            {
                ThemedParticles.LaCampanellaBloomBurst(muzzlePos, 0.25f);
            }
            
            // Halo ring muzzle flash
            if (Main.rand.NextBool(2))
            {
                CustomParticles.HaloRing(muzzlePos, ThemedParticles.CampanellaYellow, 0.2f, 8);
            }
            
            // Music notes in muzzle flash
            if (Main.rand.NextBool(4))
            {
                ThemedParticles.LaCampanellaMusicNotes(muzzlePos, 1, 20f);
            }
            
            // Screen shake
            // REMOVED: Screen shake disabled for La Campanella weapons
            // if (Main.rand.NextBool(3))
            // {
            //     player.GetModPlayer<ScreenShakePlayer>()?.AddShake(1f, 3);
            // }
            
            // Bellfire Barrage on every 3rd shot (was 5th)
            if (shotCounter >= 3)
            {
                shotCounter = 0;
                TriggerBellfireBarrage(player, source, position, velocity, damage, knockback);
            }
            
            Lighting.AddLight(muzzlePos, 0.7f, 0.4f, 0.12f);
            
            return false;
        }

        private void TriggerBellfireBarrage(Player player, IEntitySource source, Vector2 position, Vector2 velocity, int damage, float knockback)
        {
            // === BELLFIRE BARRAGE SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 0.7f }, position);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.3f, Volume = 0.4f }, position);
            
            // Fire spread of burning musical notes
            int noteCount = 7;
            float spreadAngle = MathHelper.ToRadians(40f);
            
            for (int i = 0; i < noteCount; i++)
            {
                float angle = (i - noteCount / 2f) * spreadAngle / noteCount;
                Vector2 noteVelocity = velocity.RotatedBy(angle) * 0.8f;
                
                Projectile.NewProjectile(source, position, noteVelocity,
                    ModContent.ProjectileType<BurningMusicalNote>(), (int)(damage * 0.7f), knockback * 0.5f, player.whoAmI);
            }
            
            // === BELLFIRE BARRAGE VISUAL ===
            ThemedParticles.LaCampanellaBellChime(position, 1.2f);
            ThemedParticles.LaCampanellaShockwave(position, 0.9f);
            ThemedParticles.LaCampanellaMusicNotes(position, 5, 30f);
            
            // Single halo ring
            CustomParticles.HaloRing(position, ThemedParticles.CampanellaOrange, 0.55f, 18);
            
            // Sparks
            ThemedParticles.LaCampanellaSparks(position, Main.rand.NextVector2Unit(), 8, 6f);
            
            // Black smoke
            for (int i = 0; i < 3; i++)
            {
                var smoke = new HeavySmokeParticle(position + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0, -1f),
                    ThemedParticles.CampanellaBlack, Main.rand.Next(35, 55),
                    Main.rand.NextFloat(0.45f, 0.65f), 0.55f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Screen shake - DRAMATIC
            // REMOVED: Screen shake disabled for La Campanella weapons
            // player.GetModPlayer<ScreenShakePlayer>()?.AddShake(8f, 15);
            
            Lighting.AddLight(position, 1.5f, 0.75f, 0.25f);
        }

        public override void HoldItem(Player player)
        {
            // === SIGNATURE HOLD AURA - VIBRANT PARTICLES WHILE HELD! ===
            ThemedParticles.LaCampanellaHoldAura(player.Center, 0.8f);
            
            if (Main.rand.NextBool(10))
            {
                ThemedParticles.LaCampanellaAura(player.Center, 25f);
            }
            
            Lighting.AddLight(player.Center, 0.5f, 0.25f, 0.08f);
        }
    }

    /// <summary>
    /// Bell energy beam projectile.
    /// Pure particle-based with glowing trail effect.
    /// </summary>
    public class BellEnergyBeam : ModProjectile
    {
        // Use the rifle weapon as base, drawn with custom glow
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/GrandioseChime";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 2;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // === FLAMING DARK SMOKE TRAIL - EXPLOSIVE BEAM! ===
            
            // Heavy black smoke trail
            if (Main.rand.NextBool(2))
            {
                var smoke = new HeavySmokeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1.2f, 1.2f),
                    ThemedParticles.CampanellaBlack,
                    Main.rand.Next(30, 45),
                    Main.rand.NextFloat(0.3f, 0.45f),
                    0.65f, 0.018f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Golden/yellow fire glow trail
            for (int i = 0; i < 2; i++)
            {
                Color trailColor = Main.rand.Next(3) switch
                {
                    0 => ThemedParticles.CampanellaYellow,
                    1 => ThemedParticles.CampanellaGold,
                    _ => ThemedParticles.CampanellaOrange
                };
                var glow = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1f, 1f),
                    trailColor, Main.rand.NextFloat(0.25f, 0.4f), Main.rand.Next(10, 18), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Prismatic sparkles along beam
            if (Main.rand.NextBool(2))
            {
                CustomParticles.PrismaticSparkle(Projectile.Center, ThemedParticles.CampanellaYellow, 0.35f);
            }
            
            // === GLITTERING SPARKLE TRAIL ===
            ThemedParticles.LaCampanellaSparkles(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), 2, 10f);
            ThemedParticles.LaCampanellaPrismaticSparkles(Projectile.Center, 1, 0.45f);
            
            // === BLAZING BEAM TRAIL ===
            ThemedParticles.LaCampanellaTrail(Projectile.Center, Projectile.velocity);
            
            // Extra sparks along beam
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.LaCampanellaSparks(Projectile.Center, -Projectile.velocity.SafeNormalize(Vector2.UnitX), 3, 4f);
            }
            
            // Occasional flare
            if (Main.rand.NextBool(4))
            {
                Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaYellow : ThemedParticles.CampanellaGold;
                CustomParticles.GenericFlare(Projectile.Center, flareColor, 0.25f, 10);
            }
            
            // Occasional music note
            if (Main.rand.NextBool(6))
            {
                ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 1, 12f);
            }
            
            // ☁EMUSICAL NOTATION - Beam leaves melodic trail
            if (Main.rand.NextBool(5))
            {
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(218, 165, 32), Main.rand.NextFloat());
                Vector2 noteVel = -Projectile.velocity * 0.1f + new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.8f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.3f, 30);
            }
            
            Lighting.AddLight(Projectile.Center, 0.7f, 0.4f, 0.12f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // === HIT SOUND ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.3f, 0.7f), Volume = 0.4f }, target.Center);
            
            // === SMOKE BURST ===
            for (int i = 0; i < 3; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(5f, 5f);
                var smoke = new HeavySmokeParticle(
                    target.Center + Main.rand.NextVector2Circular(15f, 15f),
                    smokeVel,
                    ThemedParticles.CampanellaBlack,
                    Main.rand.Next(40, 60),
                    Main.rand.NextFloat(0.5f, 0.8f),
                    0.7f, 0.022f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // === SIGNATURE HIT ===
            Vector2 hitDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            ThemedParticles.LaCampanellaSignatureHit(target.Center, hitDir, 0.8f);
            ThemedParticles.LaCampanellaSparks(target.Center, hitDir, 8, 6f);
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 4, 25f);
            
            // Single halo ring
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaOrange, 0.5f, 15);
            
            Lighting.AddLight(target.Center, 1.2f, 0.7f, 0.2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // === SPECTRAL BEAM TRAIL - BLACK ↁEORANGE ===
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = 1f - (i / (float)Projectile.oldPos.Length);
                Color trailColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, progress) * progress * 0.5f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                Main.EntitySpriteDraw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin,
                    0.2f * (0.5f + progress * 0.5f), SpriteEffects.None, 0);
            }
            
            // === ADDITIVE GLOW WITH GLYPHS ===
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Orbiting glyphs around beam
            if (CustomParticleSystem.TexturesLoaded)
            {
                Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
                for (int i = 0; i < 3; i++)
                {
                    float glyphAngle = Main.GameUpdateCount * 0.1f + MathHelper.TwoPi * i / 3f;
                    Vector2 glyphPos = drawPos + glyphAngle.ToRotationVector2() * 12f;
                    Color glyphColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, (float)i / 3f) * 0.55f;
                    Main.EntitySpriteDraw(glyphTex, glyphPos, null, glyphColor, glyphAngle * 1.5f, glyphTex.Size() / 2f, 0.12f, SpriteEffects.None, 0);
                }
            }
            
            Main.EntitySpriteDraw(texture, drawPos, null, ThemedParticles.CampanellaOrange * 0.5f, Projectile.rotation, origin,
                0.3f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, ThemedParticles.CampanellaYellow * 0.3f, Projectile.rotation, origin,
                0.25f, SpriteEffects.None, 0);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Main beam sprite
            Main.EntitySpriteDraw(texture, drawPos, null, Color.White, Projectile.rotation, origin,
                0.18f, SpriteEffects.None, 0);
            
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // UNIQUE: Infernal Pillar - Fire erupts upward (beam energy dissipates upward)
            DynamicParticleEffects.CampanellaDeathInfernalPillar(Projectile.Center, 0.8f);
        }
    }

    /// <summary>
    /// Burning musical note projectile from Bellfire Barrage.
    /// Glowing music note with fire trail.
    /// </summary>
    public class BurningMusicalNote : ModProjectile
    {
        // Use the rifle weapon as base, rendered as glowing note
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/GrandioseChime";
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            Projectile.rotation += 0.15f * Math.Sign(Projectile.velocity.X);
            Projectile.velocity *= 0.98f;
            
            // === BURNING MUSICAL NOTE CUSTOM EFFECTS ===
            ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 1, 15f);
            
            // ☁EMUSICAL NOTATION - Extra melodic trail for this music-themed projectile
            if (Main.rand.NextBool(4))
            {
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(255, 200, 50), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1.2f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.4f, 40);
            }
            
            // Fire spark trail
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.LaCampanellaSparks(Projectile.Center, -Projectile.velocity.SafeNormalize(Vector2.UnitX), 1, 2f);
            }
            
            // Smoky fire trail
            if (Main.rand.NextBool(3))
            {
                Color color = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                var glow = new GenericGlowParticle(Projectile.Center, 
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f),
                    color, Main.rand.NextFloat(0.15f, 0.25f), Main.rand.Next(8, 15), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            Lighting.AddLight(Projectile.Center, 0.5f, 0.25f, 0.08f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // ☁EMUSICAL IMPACT - Notes burst on burning note hit
            ThemedParticles.MusicNoteBurst(target.Center, new Color(255, 140, 40), 4, 3f);
            
            // === BLACK SMOKE SPARKLE MAGIC - SIGNATURE HIT EFFECT! ===
            Vector2 hitDir = Main.rand.NextVector2Unit();
            ThemedParticles.LaCampanellaSignatureHit(target.Center, hitDir, 0.5f);
            
            // Musical fire burst
            ThemedParticles.LaCampanellaMusicalImpact(target.Center, 0.8f);
            ThemedParticles.LaCampanellaSparks(target.Center, hitDir, 5, 4f);
        }

        public override void OnKill(int timeLeft)
        {
            // UNIQUE: Bell Toll Shatter - Music note shatters into metallic shards
            DynamicParticleEffects.CampanellaDeathBellTollShatter(Projectile.Center, 0.7f);
        }

        public override bool PreDraw(ref Color lightColor) => false; // Pure custom particle visual
    }
    
    #endregion

    #region Symphonic Bellfire Annihilator (Rocket Launcher)
    
    /// <summary>
    /// Symphonic Bellfire Annihilator - Ultimate homing rocket launcher.
    /// Fires 5 HOMING ROCKETS per shot that aggressively seek enemies!
    /// If all 5 rockets hit, spawns 5 MORE HOMING ROCKETS around the target!
    /// If ALL 10 ROCKETS HIT (both volleys), grants BELLFIRE CRESCENDO buff:
    /// +10% movement speed and +10% damage for 30 seconds (cannot stack).
    /// Special: Grand Crescendo - multiple quick detonations trigger screen-wide wave of destruction.
    /// Secondary: INFERNAL SYMPHONY - each rocket has orbiting music notes.
    /// </summary>
    public class SymphonicBellfireAnnihilator : ModItem
    {
        private int recentExplosions = 0;
        private int explosionTimer = 0;
        private const int GrandCrescendoThreshold = 3;
        
        // === VOLLEY TRACKING SYSTEM ===
        private int currentVolleyId = 0; // Unique ID for each volley
        private static Dictionary<int, VolleyTracker> activeVolleys = new Dictionary<int, VolleyTracker>();
        
        public class VolleyTracker
        {
            public int OwnerId;
            public int TotalRockets;
            public int HitCount;
            public bool IsSecondaryVolley;
            public int ParentVolleyId;
            public Vector2 LastHitPosition;
            public int TimeAlive;
            public const int MaxVolleyTime = 300; // 5 seconds max tracking time
            
            public bool AllRocketsHit => HitCount >= TotalRockets;
        }
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 494; // Base damage increased 30% for smaller explosions
            Item.DamageType = DamageClass.Ranged;
            Item.width = 70;
            Item.height = 30;
            Item.useTime = 35; // Slower since we fire 5 rockets at once
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.UseSound = SoundID.Item92;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<HomingBellRocket>();
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Rocket;
            Item.noMelee = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var descLine = new TooltipLine(Mod, "Description", 
                "[c/FF6600:Fires 5 HOMING ROCKETS that aggressively seek enemies!]\n" +
                "[c/FFAA00:If all 5 hit ↁESpawns 5 MORE homing rockets around target!]\n" +
                "[c/FFD700:If ALL 10 hit ↁEBELLFIRE CRESCENDO: +10% damage & speed for 30s!]\n" +
                "[c/FF4400:'The symphony of destruction seeks its audience...']");
            tooltips.Add(descLine);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<HomingBellRocket>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Create new volley tracker
            currentVolleyId++;
            var volley = new VolleyTracker
            {
                OwnerId = player.whoAmI,
                TotalRockets = 5,
                HitCount = 0,
                IsSecondaryVolley = false,
                ParentVolleyId = -1,
                LastHitPosition = Vector2.Zero,
                TimeAlive = 0
            };
            activeVolleys[currentVolleyId] = volley;
            
            // === FIRE 5 HOMING ROCKETS IN A SPREAD! ===
            float spreadAngle = MathHelper.ToRadians(35f); // Total spread
            for (int i = 0; i < 5; i++)
            {
                float angleOffset = spreadAngle * ((i - 2f) / 2f); // -35, -17.5, 0, +17.5, +35 degrees
                Vector2 rocketVel = velocity.RotatedBy(angleOffset);
                
                int proj = Projectile.NewProjectile(source, position, rocketVel, type, damage, knockback, player.whoAmI);
                if (Main.projectile[proj].ModProjectile is HomingBellRocket rocket)
                {
                    rocket.SetOwnerWeapon(this);
                    rocket.SetVolleyId(currentVolleyId);
                }
                
                // Spawn orbiting music notes for each rocket
                for (int j = 0; j < 2; j++)
                {
                    float noteAngle = MathHelper.TwoPi * j / 2f;
                    Vector2 noteVel = rocketVel.RotatedBy(noteAngle) * 0.3f;
                    Projectile.NewProjectile(source, position, noteVel,
                        ModContent.ProjectileType<OrbitingMusicNote>(), damage / 4, knockback * 0.2f, player.whoAmI, proj);
                }
            }
            
            // === VOLLEY LAUNCH SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item92 with { Pitch = 0.1f, Volume = 0.6f }, position);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.4f, Volume = 0.5f }, position);
            
            // === MUZZLE BLAST ===
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.Zero) * 50f;
            ThemedParticles.LaCampanellaSparks(muzzlePos, velocity.SafeNormalize(Vector2.Zero), 8, 8f);
            ThemedParticles.LaCampanellaBloomBurst(muzzlePos, 0.7f);
            ThemedParticles.LaCampanellaMusicNotes(muzzlePos, 5, 35f);
            
            // Single halo ring
            CustomParticles.HaloRing(muzzlePos, ThemedParticles.CampanellaOrange, 0.5f, 16);
            
            // Backblast smoke
            for (int i = 0; i < 3; i++)
            {
                var smoke = new HeavySmokeParticle(muzzlePos - velocity.SafeNormalize(Vector2.Zero) * 35f,
                    -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 7f) + new Vector2(0, -0.5f),
                    ThemedParticles.CampanellaBlack, Main.rand.Next(35, 55),
                    Main.rand.NextFloat(0.5f, 0.75f), 0.6f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Screen shake - heavier for volley
            // REMOVED: Screen shake disabled for La Campanella weapons
            // player.GetModPlayer<ScreenShakePlayer>()?.AddShake(7f, 15);
            
            Lighting.AddLight(muzzlePos, 1.2f, 0.6f, 0.2f);
            
            return false;
        }

        public void OnRocketExplode(Vector2 position)
        {
            recentExplosions++;
            explosionTimer = 60; // 1 second window
            
            if (recentExplosions >= GrandCrescendoThreshold)
            {
                recentExplosions = 0;
                TriggerGrandCrescendo(position);
            }
        }

        private void TriggerGrandCrescendo(Vector2 position)
        {
            Player owner = Main.player[Main.myPlayer];
            
            // === GRAND CRESCENDO SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f, Volume = 0.9f }, position);
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0f, Volume = 0.6f }, position);
            
            // Screen-wide wave effect
            for (int i = 0; i < 36; i++)
            {
                float angle = MathHelper.TwoPi * i / 36f;
                Vector2 velocity = angle.ToRotationVector2() * 15f;
                
                Projectile.NewProjectile(owner.GetSource_ItemUse(owner.HeldItem), position, velocity,
                    ModContent.ProjectileType<GrandCrescendoWave>(), (int)(Item.damage * 0.5f), 5f, owner.whoAmI);
            }
            
            // === GRAND CRESCENDO VISUAL ===
            ThemedParticles.LaCampanellaImpact(position, 2.5f);
            ThemedParticles.LaCampanellaShockwave(position, 2f);
            ThemedParticles.LaCampanellaMusicNotes(position, 10, 60f);
            
            // Halo rings - reduced
            CustomParticles.HaloRing(position, ThemedParticles.CampanellaOrange, 0.8f, 25);
            CustomParticles.HaloRing(position, ThemedParticles.CampanellaYellow, 0.6f, 20);
            
            // Sparks
            ThemedParticles.LaCampanellaSparks(position, Main.rand.NextVector2Unit(), 12, 10f);
            
            // Black smoke
            for (int i = 0; i < 6; i++)
            {
                var smoke = new HeavySmokeParticle(position + Main.rand.NextVector2Circular(30f, 30f),
                    Main.rand.NextVector2Circular(5f, 5f) + new Vector2(0, -2f),
                    ThemedParticles.CampanellaBlack, Main.rand.Next(50, 80),
                    Main.rand.NextFloat(0.7f, 1.0f), 0.65f, 0.018f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Grant movement speed buff
            owner.AddBuff(ModContent.BuffType<GrandCrescendoBuff>(), 300); // 5 seconds
            
            // Destroy nearby enemy projectiles
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.hostile && Vector2.Distance(proj.Center, position) < 600f)
                {
                    proj.Kill();
                }
            }
            
            // Screen shake
            // REMOVED: Screen shake disabled for La Campanella weapons
            // owner.GetModPlayer<ScreenShakePlayer>()?.AddShake(15f, 30);
        }

        public override void HoldItem(Player player)
        {
            // Decay explosion counter
            if (explosionTimer > 0)
            {
                explosionTimer--;
                if (explosionTimer <= 0)
                    recentExplosions = 0;
            }
            
            // === VOLLEY CLEANUP - Remove old volleys ===
            List<int> toRemove = new List<int>();
            foreach (var kvp in activeVolleys)
            {
                kvp.Value.TimeAlive++;
                if (kvp.Value.TimeAlive > VolleyTracker.MaxVolleyTime)
                    toRemove.Add(kvp.Key);
            }
            foreach (int id in toRemove)
                activeVolleys.Remove(id);
            
            // === SIGNATURE HOLD AURA - VIBRANT PARTICLES WHILE HELD! ===
            ThemedParticles.LaCampanellaHoldAura(player.Center, 0.9f);
            
            if (Main.rand.NextBool(10))
            {
                ThemedParticles.LaCampanellaAura(player.Center, 30f);
            }
            
            Lighting.AddLight(player.Center, 0.5f, 0.25f, 0.08f);
        }
        
        /// <summary>
        /// Called when a homing rocket hits an enemy. Tracks volley completion.
        /// </summary>
        public void OnRocketHit(int volleyId, Vector2 hitPosition, int ownerId)
        {
            if (!activeVolleys.TryGetValue(volleyId, out var volley))
                return;
            
            volley.HitCount++;
            volley.LastHitPosition = hitPosition;
            
            // Check if all rockets in this volley have hit
            if (volley.AllRocketsHit)
            {
                if (!volley.IsSecondaryVolley)
                {
                    // === PRIMARY VOLLEY COMPLETE - SPAWN 5 MORE HOMING ROCKETS! ===
                    TriggerSecondaryVolley(hitPosition, ownerId, volleyId);
                }
                else
                {
                    // === SECONDARY VOLLEY COMPLETE - Check if primary also completed! ===
                    // If the parent volley (primary) also completed, grant the buff!
                    if (volley.ParentVolleyId >= 0 && activeVolleys.TryGetValue(volley.ParentVolleyId, out var parentVolley))
                    {
                        if (parentVolley.AllRocketsHit)
                        {
                            // === ALL 10 ROCKETS HIT! GRANT BELLFIRE CRESCENDO! ===
                            TriggerBellfireCrescendo(hitPosition, ownerId);
                        }
                    }
                }
                
                // Don't remove volley yet - we need to track it for the buff check
            }
        }
        
        private void TriggerSecondaryVolley(Vector2 position, int ownerId, int parentVolleyId)
        {
            Player owner = Main.player[ownerId];
            
            // === EPIC SECONDARY VOLLEY SPAWN EFFECT! ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.8f }, position);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.3f, Volume = 0.6f }, position);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Pitch = 0.4f, Volume = 0.5f }, position);
            
            // Visual feedback
            ThemedParticles.LaCampanellaImpact(position, 1f);
            ThemedParticles.LaCampanellaShockwave(position, 0.7f);
            ThemedParticles.LaCampanellaMusicNotes(position, 5, 30f);
            
            CustomParticles.HaloRing(position, ThemedParticles.CampanellaOrange, 0.45f, 15);
            
            // Create secondary volley tracker
            currentVolleyId++;
            var secondaryVolley = new VolleyTracker
            {
                OwnerId = ownerId,
                TotalRockets = 5,
                HitCount = 0,
                IsSecondaryVolley = true,
                ParentVolleyId = parentVolleyId,
                LastHitPosition = position,
                TimeAlive = 0
            };
            activeVolleys[currentVolleyId] = secondaryVolley;
            
            // === SPAWN 5 HOMING ROCKETS AROUND THE TARGET! ===
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 spawnOffset = angle.ToRotationVector2() * 80f; // Spawn in a ring around target
                Vector2 spawnPos = position + spawnOffset;
                
                // Rockets home toward center (where enemies likely are)
                Vector2 rocketVel = -spawnOffset.SafeNormalize(Vector2.Zero) * 8f;
                
                int proj = Projectile.NewProjectile(owner.GetSource_FromThis(), spawnPos, rocketVel,
                    ModContent.ProjectileType<HomingBellRocket>(), (int)(Item.damage * 0.8f), Item.knockBack * 0.8f, ownerId);
                if (Main.projectile[proj].ModProjectile is HomingBellRocket rocket)
                {
                    rocket.SetOwnerWeapon(this);
                    rocket.SetVolleyId(currentVolleyId);
                    rocket.IsSecondaryRocket = true;
                }
                
                // Spawn effect at each rocket position
                ThemedParticles.LaCampanellaSparks(spawnPos, -spawnOffset.SafeNormalize(Vector2.Zero), 5, 6f);
                CustomParticles.GenericFlare(spawnPos, ThemedParticles.CampanellaOrange, 0.4f, 12);
            }
            
            // Screen shake
            // REMOVED: Screen shake disabled for La Campanella weapons
            // owner.GetModPlayer<ScreenShakePlayer>()?.AddShake(8f, 15);
        }
        
        private void TriggerBellfireCrescendo(Vector2 position, int ownerId)
        {
            Player owner = Main.player[ownerId];
            
            // === BELLFIRE CRESCENDO - ALL 10 ROCKETS HIT! ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 1f }, position);
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.2f, Volume = 0.7f }, position);
            
            // === VISUAL CELEBRATION ===
            ThemedParticles.LaCampanellaImpact(position, 2f);
            ThemedParticles.LaCampanellaShockwave(position, 1.8f);
            ThemedParticles.LaCampanellaMusicNotes(position, 12, 70f);
            
            // Halo rings - reduced
            CustomParticles.HaloRing(position, ThemedParticles.CampanellaGold, 0.7f, 22);
            CustomParticles.HaloRing(position, ThemedParticles.CampanellaOrange, 0.5f, 18);
            
            // === GRANT BELLFIRE CRESCENDO BUFF - 30 SECONDS! ===
            // Only if not already active (cannot stack)
            if (!owner.HasBuff(ModContent.BuffType<BellfireCrescendoBuff>()))
            {
                owner.AddBuff(ModContent.BuffType<BellfireCrescendoBuff>(), 1800); // 30 seconds
            }
            
            // Screen shake
            // REMOVED: Screen shake disabled for La Campanella weapons
            // owner.GetModPlayer<ScreenShakePlayer>()?.AddShake(12f, 25);
        }
    }

    /// <summary>
    /// Bell-shaped rocket projectile.
    /// Blazing rocket with bell-flame trail.
    /// </summary>
    public class BellRocket : ModProjectile
    {
        // Use the launcher weapon as base for bell rocket visual
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/SymphonicBellfireAnnihilator";
        
        private SymphonicBellfireAnnihilator ownerWeapon;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public void SetOwnerWeapon(SymphonicBellfireAnnihilator weapon)
        {
            ownerWeapon = weapon;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            Vector2 exhaustPos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 15f;
            
            // === MUSIC NOTE TRAIL - Rockets leave chains of music notes! ===
            if (Main.rand.NextBool(3))
            {
                ThemedParticles.LaCampanellaMusicNotes(exhaustPos, 1, 12f);
            }
            
            // ☁EMUSICAL NOTATION - Extra music notes trailing the rocket
            if (Main.rand.NextBool(5))
            {
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(218, 165, 32), Main.rand.NextFloat());
                Vector2 noteVel = -Projectile.velocity * 0.12f + new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -0.6f);
                ThemedParticles.MusicNote(exhaustPos, noteVel, noteColor, 0.35f, 35);
            }
            
            // === BELL ROCKET CUSTOM TRAIL ===
            ThemedParticles.LaCampanellaTrail(exhaustPos, Projectile.velocity);
            
            // Sparkle burst from exhaust
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.LaCampanellaSparkles(exhaustPos + Main.rand.NextVector2Circular(6f, 6f), 2, 10f);
            }
            
            // Blazing fire sparks from exhaust
            ThemedParticles.LaCampanellaSparks(exhaustPos, -Projectile.velocity.SafeNormalize(Vector2.UnitX), 3, 4f);
            
            // Heavy black smoke with orange glow mix
            if (Main.rand.NextBool(3))
            {
                var smoke = new HeavySmokeParticle(
                    Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 25f,
                    -Projectile.velocity * 0.2f + new Vector2(0, -0.3f),
                    ThemedParticles.CampanellaBlack, Main.rand.Next(22, 38),
                    Main.rand.NextFloat(0.25f, 0.45f), 0.55f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Golden glow particles
            if (Main.rand.NextBool(2))
            {
                Color glowColor = Main.rand.Next(3) switch { 0 => ThemedParticles.CampanellaGold, 1 => ThemedParticles.CampanellaOrange, _ => ThemedParticles.CampanellaYellow };
                var glow = new GenericGlowParticle(exhaustPos + Main.rand.NextVector2Circular(5f, 5f),
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.8f, 0.8f), glowColor,
                    Main.rand.NextFloat(0.2f, 0.38f), Main.rand.Next(10, 18), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            Lighting.AddLight(Projectile.Center, 0.8f, 0.4f, 0.12f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ☁EMUSICAL IMPACT - Rocket impact music burst
            ThemedParticles.MusicNoteBurst(target.Center, new Color(255, 140, 40), 6, 4f);
            
            // === BLACK SMOKE SPARKLE MAGIC - SIGNATURE HIT EFFECT! ===
            Vector2 hitDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            ThemedParticles.LaCampanellaSignatureHit(target.Center, hitDir, 1.2f);
            
            Explode();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode();
            return true;
        }

        private void Explode()
        {
            // === BELL EXPLOSION SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = Main.rand.NextFloat(0.1f, 0.4f), Volume = 0.7f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f, Volume = 0.4f }, Projectile.Center);
            
            // === EXPLOSION EFFECTS ===
            ThemedParticles.LaCampanellaImpact(Projectile.Center, 1.5f);
            ThemedParticles.LaCampanellaShockwave(Projectile.Center, 1.5f);
            ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 6, 40f);
            ThemedParticles.LaCampanellaSparks(Projectile.Center, Main.rand.NextVector2Unit(), 10, 8f);
            
            // Halo rings - reduced
            CustomParticles.HaloRing(Projectile.Center, ThemedParticles.CampanellaOrange, 0.6f, 20);
            CustomParticles.HaloRing(Projectile.Center, ThemedParticles.CampanellaYellow, 0.45f, 16);
            
            // Black smoke
            for (int i = 0; i < 6; i++)
            {
                var smoke = new HeavySmokeParticle(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(5f, 5f) + new Vector2(0, -1.5f),
                    ThemedParticles.CampanellaBlack, Main.rand.Next(40, 65),
                    Main.rand.NextFloat(0.55f, 0.85f), 0.65f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Harmonic pulse projectiles (pierce walls)
            int pulseCount = 8;
            for (int i = 0; i < pulseCount; i++)
            {
                float angle = MathHelper.TwoPi * i / pulseCount;
                Vector2 velocity = angle.ToRotationVector2() * 10f;
                
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity,
                    ModContent.ProjectileType<HarmonicPulse>(), Projectile.damage / 3, 3f, Projectile.owner);
            }
            
            // AOE damage and stun
            float explosionRadius = 105f; // 30% smaller explosions for more focused damage
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                if (Vector2.Distance(Projectile.Center, npc.Center) <= explosionRadius)
                {
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 2);
                    
                    // Brief stun (slow)
                    npc.velocity *= 0.5f;
                }
            }
            
            // Notify weapon for Grand Crescendo
            ownerWeapon?.OnRocketExplode(Projectile.Center);
            
            // Screen shake
            // REMOVED: Screen shake disabled for La Campanella weapons
            // Player owner = Main.player[Projectile.owner];
            // owner.GetModPlayer<ScreenShakePlayer>()?.AddShake(8f, 15);
            
            Lighting.AddLight(Projectile.Center, 1.8f, 0.9f, 0.3f);

            Projectile.Kill();
        }
        
        public override bool PreDraw(ref Color lightColor) => false; // Pure custom particle visual
    }
    
    /// <summary>
    /// HOMING Bell Rocket - Aggressively seeks enemies!
    /// Part of the 5-rocket volley system. Tracks hits for Bellfire Crescendo buff.
    /// </summary>
    public class HomingBellRocket : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/SymphonicBellfireAnnihilator";
        
        private SymphonicBellfireAnnihilator ownerWeapon;
        private int volleyId = -1;
        private float homingStrength = 0f;
        public bool IsSecondaryRocket = false;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1; // Single hit for tracking
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public void SetOwnerWeapon(SymphonicBellfireAnnihilator weapon)
        {
            ownerWeapon = weapon;
        }
        
        public void SetVolleyId(int id)
        {
            volleyId = id;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // === AGGRESSIVE HOMING LOGIC ===
            homingStrength = Math.Min(homingStrength + 0.008f, 0.18f); // Ramps up fast
            
            // Find target - prioritize closest enemy
            NPC target = null;
            float closestDist = 800f; // Long range homing
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    target = npc;
                }
            }
            
            // Home toward target aggressively
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                float targetSpeed = Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), toTarget, homingStrength) * targetSpeed;
                
                // Maintain minimum speed
                if (Projectile.velocity.Length() < 10f)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f;
            }
            
            Vector2 exhaustPos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 12f;
            Color primaryColor = IsSecondaryRocket ? ThemedParticles.CampanellaGold : ThemedParticles.CampanellaOrange;
            
            // === FLAMING FLARE BALL CORE - Main visual element ===
            // Central blazing fireball glow - ALWAYS visible
            for (int i = 0; i < 3; i++)
            {
                Color coreColor = Main.rand.Next(3) switch { 0 => ThemedParticles.CampanellaYellow, 1 => primaryColor, _ => Color.White };
                var coreGlow = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    Main.rand.NextVector2Circular(0.3f, 0.3f),
                    coreColor, Main.rand.NextFloat(0.45f, 0.65f), Main.rand.Next(3, 6), true);
                MagnumParticleHandler.SpawnParticle(coreGlow);
            }
            
            // Outer flame ring
            for (int i = 0; i < 2; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 ringOffset = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                var flameRing = new GenericGlowParticle(
                    Projectile.Center + ringOffset,
                    -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    primaryColor, Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(6, 12), true);
                MagnumParticleHandler.SpawnParticle(flameRing);
            }
            
            // === BLAZING FLARE TRAIL - Heavy flame trail ===
            // Heavy black smoke trail
            if (Main.rand.NextBool(2))
            {
                var smoke = new HeavySmokeParticle(
                    exhaustPos + Main.rand.NextVector2Circular(5f, 5f),
                    -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, -0.5f),
                    ThemedParticles.CampanellaBlack,
                    Main.rand.Next(25, 40),
                    Main.rand.NextFloat(0.3f, 0.45f),
                    0.55f, 0.018f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Fire glow trail - continuous flaming effect
            for (int i = 0; i < 2; i++)
            {
                Color trailColor = Main.rand.Next(3) switch { 0 => ThemedParticles.CampanellaYellow, 1 => primaryColor, _ => ThemedParticles.CampanellaOrange };
                var glow = new GenericGlowParticle(
                    exhaustPos + Main.rand.NextVector2Circular(6f, 6f),
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                    trailColor, Main.rand.NextFloat(0.25f, 0.4f), Main.rand.Next(10, 18), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Fire sparks trailing
            ThemedParticles.LaCampanellaSparks(exhaustPos, -Projectile.velocity.SafeNormalize(Vector2.UnitX), 3, 5f);
            
            // Occasional bright flare
            if (Main.rand.NextBool(4))
            {
                Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaYellow : primaryColor;
                CustomParticles.GenericFlare(Projectile.Center, flareColor, 0.35f, 12);
            }
            
            // === HOMING INDICATOR - Extra glow when locked on ===
            if (target != null && Main.rand.NextBool(3))
            {
                // Pulsing lock-on indicator
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 0.2f + 0.8f;
                var lockGlow = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 0.5f,
                    Color.White, 0.2f * pulse, Main.rand.Next(8, 14), true);
                MagnumParticleHandler.SpawnParticle(lockGlow);
            }
            
            // Occasional music note
            if (Main.rand.NextBool(6))
            {
                ThemedParticles.LaCampanellaMusicNotes(exhaustPos, 1, 15f);
            }
            
            // ☁EMUSICAL NOTATION - Homing rockets trail music
            if (Main.rand.NextBool(5))
            {
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(255, 200, 50), Main.rand.NextFloat());
                Vector2 noteVel = -Projectile.velocity * 0.1f + new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -0.7f);
                ThemedParticles.MusicNote(exhaustPos, noteVel, noteColor, 0.32f, 32);
            }
            
            // Prismatic sparkle
            if (Main.rand.NextBool(5))
            {
                CustomParticles.PrismaticSparkle(Projectile.Center, primaryColor, 0.35f);
            }
            
            Lighting.AddLight(Projectile.Center, IsSecondaryRocket ? 1.2f : 0.9f, 0.6f, 0.18f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 2);
            
            // === NOTIFY WEAPON OF HIT FOR VOLLEY TRACKING ===
            ownerWeapon?.OnRocketHit(volleyId, target.Center, Projectile.owner);
            
            // === HOMING ROCKET HIT EXPLOSION ===
            Vector2 hitDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            ThemedParticles.LaCampanellaSignatureHit(target.Center, hitDir, 1f);
            ThemedParticles.LaCampanellaImpact(target.Center, 1.2f);
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 8, 40f);
            ThemedParticles.LaCampanellaShockwave(target.Center, 0.8f);
            
            // ☁EMUSICAL IMPACT - Homing rocket musical explosion
            ThemedParticles.MusicNoteBurst(target.Center, new Color(255, 140, 40), 4, 3.5f);
            
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaOrange, 0.4f, 14);
            
            // Sound
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f + (IsSecondaryRocket ? 0.3f : 0f), Volume = 0.6f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.1f, Volume = 0.4f }, target.Center);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Explode on tile collision (missed shot - doesn't count toward volley)
            ThemedParticles.LaCampanellaImpact(Projectile.Center, 0.8f);
            ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 5, 25f);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f, Volume = 0.5f }, Projectile.Center);
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            // UNIQUE: Ember Cascade - Rocket explodes in fountain of embers
            DynamicParticleEffects.CampanellaDeathEmberCascade(Projectile.Center, 0.9f);
        }

        public override bool PreDraw(ref Color lightColor) => false; // Pure flaming flare ball - particles only
    }
    
    /// <summary>
    /// Bellfire Crescendo Buff - Granted when ALL 10 HOMING ROCKETS HIT!
    /// +10% damage and +10% movement speed for 30 seconds. Cannot stack.
    /// </summary>
    public class BellfireCrescendoBuff : ModBuff
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/SymphonicBellfireAnnihilator";
        
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // +10% damage
            player.GetDamage(DamageClass.Generic) += 0.1f;
            
            // +10% movement speed
            player.moveSpeed += 0.1f;
            player.maxRunSpeed += 1f;
            
            // === BELLFIRE CRESCENDO VISUAL AURA ===
            // Golden-orange flame aura around player
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                Color auraColor = Main.rand.Next(3) switch { 0 => ThemedParticles.CampanellaGold, 1 => ThemedParticles.CampanellaOrange, _ => ThemedParticles.CampanellaYellow };
                var glow = new GenericGlowParticle(player.Center + offset,
                    new Vector2(0, -Main.rand.NextFloat(0.5f, 1.5f)),
                    auraColor, Main.rand.NextFloat(0.2f, 0.35f), Main.rand.Next(15, 25), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Music notes rising occasionally
            if (Main.rand.NextBool(8))
            {
                ThemedParticles.LaCampanellaMusicNotes(player.Center + new Vector2(Main.rand.NextFloat(-25f, 25f), 10f), 1, 25f);
            }
            
            // Fire sparks from movement
            if (player.velocity.Length() > 3f && Main.rand.NextBool(4))
            {
                ThemedParticles.LaCampanellaSparks(player.Center + Main.rand.NextVector2Circular(15f, 15f),
                    -player.velocity.SafeNormalize(Vector2.Zero), 2, 3f);
            }
            
            Lighting.AddLight(player.Center, 0.6f, 0.35f, 0.1f);
        }
    }

    /// <summary>
    /// Harmonic pulse that pierces walls.
    /// Pure particle visual - no texture drawn.
    /// </summary>
    public class HarmonicPulse : ModProjectile
    {
        // Uses weapon texture for loading, drawn entirely as particles
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/SymphonicBellfireAnnihilator";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 15;
            Projectile.height = 15;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false; // Pierces walls
            Projectile.ignoreWater = true;
            Projectile.alpha = 150;
        }

        public override void AI()
        {
            Projectile.alpha = Math.Max(0, Projectile.alpha - 5);
            
            // ☁EMUSICAL NOTATION - Harmonic pulse with rising notes
            if (Main.rand.NextBool(5))
            {
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(255, 200, 50), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.35f, 35);
            }
            
            // === HARMONIC PULSE TRAIL ===
            ThemedParticles.LaCampanellaTrail(Projectile.Center, Projectile.velocity);
            
            // === SPARKLE COMBUSTION TRAIL ===
            ThemedParticles.LaCampanellaSparkles(Projectile.Center, 2, 8f);
            if (Main.rand.NextBool(2))
            {
                CustomParticles.PrismaticSparkle(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), ThemedParticles.CampanellaYellow, 0.3f);
            }
            ThemedParticles.LaCampanellaPrismaticSparkles(Projectile.Center, 1, 0.4f);
            
            // Golden pulse glow - MORE INTENSE
            for (int i = 0; i < 2; i++)
            {
                Color color = Main.rand.Next(3) switch { 0 => ThemedParticles.CampanellaYellow, 1 => ThemedParticles.CampanellaGold, _ => ThemedParticles.CampanellaOrange };
                var glow = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f), color, Main.rand.NextFloat(0.22f, 0.38f), Main.rand.Next(10, 18), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Occasional flare
            if (Main.rand.NextBool(4))
            {
                Color flareColor = Main.rand.NextBool() ? ThemedParticles.CampanellaYellow : ThemedParticles.CampanellaOrange;
                CustomParticles.GenericFlare(Projectile.Center, flareColor, 0.25f, 10);
            }
            
            Lighting.AddLight(Projectile.Center, 0.5f, 0.3f, 0.1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // ☁EMUSICAL IMPACT - Harmonic burst on hit
            ThemedParticles.MusicNoteBurst(target.Center, new Color(255, 140, 40), 5, 3.5f);
            
            // === BLACK SMOKE SPARKLE MAGIC - SIGNATURE HIT EFFECT! ===
            Vector2 hitDir = Main.rand.NextVector2Unit();
            ThemedParticles.LaCampanellaSignatureHit(target.Center, hitDir, 0.5f);
            ThemedParticles.LaCampanellaSparks(target.Center, hitDir, 5, 4f);
            
            // === SPARKLE COMBUSTION BURST ===
            ThemedParticles.LaCampanellaSparkles(target.Center, 8, 20f);
            ThemedParticles.LaCampanellaPrismaticSparkles(target.Center, 4, 0.5f);
            for (int s = 0; s < 4; s++)
            {
                CustomParticles.PrismaticSparkle(target.Center + Main.rand.NextVector2Circular(15f, 15f), ThemedParticles.CampanellaYellow, 0.35f);
            }
            
            // Halo ring on hit
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaOrange, 0.3f, 10);
        }

        public override void OnKill(int timeLeft)
        {
            // UNIQUE: Ring of Fire - Harmonic pulse expands as circular fire ring
            DynamicParticleEffects.CampanellaDeathRingOfFire(Projectile.Center, 0.9f);
        }

        public override bool PreDraw(ref Color lightColor) => false; // Pure particle visual
    }

    /// <summary>
    /// Grand Crescendo wave projectile.
    /// Pure particle visual - expanding flame wave.
    /// </summary>
    public class GrandCrescendoWave : ModProjectile
    {
        // Uses weapon texture for loading, drawn entirely as particles
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/SymphonicBellfireAnnihilator";
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            // ☁EMUSICAL NOTATION - Grand crescendo wave with scattered notes
            if (Main.rand.NextBool(4))
            {
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(255, 200, 50), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -1.2f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.4f, 40);
            }
            
            // === MASSIVE WAVE EFFECT ===
            // Fire and sound wave with custom particles
            ThemedParticles.LaCampanellaTrail(Projectile.Center, Projectile.velocity);
            
            // Extra fire sparks
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.LaCampanellaSparks(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), 2, 3f);
            }
            
            // Black smoke wisps
            if (Main.rand.NextBool(4))
            {
                var smoke = new HeavySmokeParticle(Projectile.Center, 
                    -Projectile.velocity * 0.1f + new Vector2(0, -0.5f),
                    ThemedParticles.CampanellaBlack, Main.rand.Next(20, 35), 
                    Main.rand.NextFloat(0.2f, 0.35f), 0.4f, 0.015f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            Lighting.AddLight(Projectile.Center, 0.6f, 0.35f, 0.1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // ☁EMUSICAL IMPACT - Grand crescendo impact burst
            ThemedParticles.MusicNoteBurst(target.Center, new Color(255, 140, 40), 6, 4f);
            
            // === BLACK SMOKE SPARKLE MAGIC - SIGNATURE HIT EFFECT! ===
            Vector2 hitDir = Main.rand.NextVector2Unit();
            ThemedParticles.LaCampanellaSignatureHit(target.Center, hitDir, 0.6f);
            ThemedParticles.LaCampanellaSparks(target.Center, hitDir, 4, 4f);
            ThemedParticles.LaCampanellaBloomBurst(target.Center, 0.3f);
        }

        public override void OnKill(int timeLeft)
        {
            // UNIQUE: Virtuosic Finale - Grand crescendo wave ends with dramatic multi-layer explosion
            DynamicParticleEffects.CampanellaDeathVirtuosicFinale(Projectile.Center, 1.0f);
        }

        public override bool PreDraw(ref Color lightColor) => false; // Pure particle visual
    }

    /// <summary>
    /// Grand Crescendo buff - movement speed boost.
    /// </summary>
    public class GrandCrescendoBuff : ModBuff
    {
        // Use launcher weapon as buff icon
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/SymphonicBellfireAnnihilator";
        
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.moveSpeed += 0.5f;
            player.maxRunSpeed += 4f;
            
            // === GRAND CRESCENDO CUSTOM AURA ===
            // Blazing particle trail
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.LaCampanellaSparks(player.Center + Main.rand.NextVector2Circular(15f, 15f),
                    -player.velocity.SafeNormalize(Vector2.UnitX), 1, 2f);
            }
            
            // Music note particles
            if (Main.rand.NextBool(8))
            {
                ThemedParticles.LaCampanellaMusicNotes(player.Center + new Vector2(Main.rand.NextFloat(-20f, 20f), 5f), 1, 20f);
            }
            
            // Orange/Yellow glow particles
            if (Main.rand.NextBool(3))
            {
                Color color = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                var glow = new GenericGlowParticle(player.Center + Main.rand.NextVector2Circular(25f, 25f),
                    -player.velocity * 0.15f + new Vector2(0, -0.5f),
                    color, Main.rand.NextFloat(0.15f, 0.25f), Main.rand.Next(10, 18), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            Lighting.AddLight(player.Center, 0.5f, 0.25f, 0.08f);
        }
    }
    
    #endregion
    
    #region New Special Projectiles
    
    /// <summary>
    /// Trailing music note that follows bullets and detonates on hit.
    /// </summary>
    public class TrailingMusicNote : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/PiercingBellsResonance";
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            Projectile.rotation += 0.2f;
            Projectile.velocity *= 0.98f;
            
            // ☁EMUSICAL NOTATION - Trailing note with gentle rising notes
            if (Main.rand.NextBool(5))
            {
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(255, 200, 50), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.8f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.3f, 30);
            }
            
            // Music note trail
            ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 1, 8f);
            
            // Fire glow trail
            if (Main.rand.NextBool(2))
            {
                Color color = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                var glow = new GenericGlowParticle(Projectile.Center,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    color, Main.rand.NextFloat(0.15f, 0.25f), Main.rand.Next(8, 14), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            Lighting.AddLight(Projectile.Center, 0.4f, 0.2f, 0.05f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // ☁EMUSICAL IMPACT - Trailing note detonation burst
            ThemedParticles.MusicNoteBurst(target.Center, new Color(255, 140, 40), 5, 3.5f);
            
            // Detonation effect
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 5, 25f);
            ThemedParticles.LaCampanellaBloomBurst(target.Center, 0.4f);
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaOrange, 0.3f, 12);
            
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 0.4f }, target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            // UNIQUE: Bell Toll Shatter - Trailing music note shatters into metallic shards
            DynamicParticleEffects.CampanellaDeathBellTollShatter(Projectile.Center, 0.6f);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
    
    /// <summary>
    /// Resonant Bell Blast - Massive bullet from Piercing Bell's Resonance special.
    /// </summary>
    public class ResonantBellBlast : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/PiercingBellsResonance";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // ☁EMUSICAL NOTATION - Resonant bell blast with powerful notes
            if (Main.rand.NextBool(4))
            {
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(218, 165, 32), Main.rand.NextFloat());
                Vector2 noteVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * 0.5f + new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.8f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.45f, 40);
            }
            
            // Massive trail with music notes
            ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 2, 15f);
            ThemedParticles.LaCampanellaSparks(Projectile.Center, -Projectile.velocity.SafeNormalize(Vector2.UnitX), 4, 6f);
            ThemedParticles.LaCampanellaTrail(Projectile.Center, Projectile.velocity);
            
            // Heavy smoke
            if (Main.rand.NextBool(2))
            {
                var smoke = new HeavySmokeParticle(Projectile.Center,
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    ThemedParticles.CampanellaBlack, Main.rand.Next(25, 40),
                    Main.rand.NextFloat(0.3f, 0.5f), 0.6f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Golden glow
            for (int i = 0; i < 2; i++)
            {
                Color color = Main.rand.Next(3) switch { 0 => ThemedParticles.CampanellaOrange, 1 => ThemedParticles.CampanellaYellow, _ => ThemedParticles.CampanellaGold };
                var glow = new GenericGlowParticle(Projectile.Center,
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    color, Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(12, 20), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            Lighting.AddLight(Projectile.Center, 1f, 0.5f, 0.15f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 3);
            
            // ☁EMUSICAL IMPACT - Massive resonant bell burst
            ThemedParticles.MusicNoteBurst(target.Center, new Color(255, 140, 40), 8, 5f);
            
            // Massive explosion
            ThemedParticles.LaCampanellaImpact(target.Center, 1.5f);
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 10, 40f);
            ThemedParticles.LaCampanellaShockwave(target.Center, 0.8f);
            
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaOrange, 0.45f, 15);
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.6f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.2f, Volume = 0.5f }, target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            // Resonant blast death - dramatic virtuosic finale with musical burst
            DynamicParticleEffects.CampanellaDeathVirtuosicFinale(Projectile.Center, 1.1f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw trail
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = 1f - (i / (float)Projectile.oldPos.Length);
                Color trailColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, progress) * progress * 0.6f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.EntitySpriteDraw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, 0.3f * progress, SpriteEffects.None, 0);
            }
            
            // Main glow
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, ThemedParticles.CampanellaOrange * 0.7f, Projectile.rotation, origin, 0.4f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, ThemedParticles.CampanellaYellow * 0.5f, Projectile.rotation, origin, 0.35f, SpriteEffects.None, 0);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }
    
    /// <summary>
    /// Homing music note that seeks enemies.
    /// </summary>
    public class HomingMusicNote : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/PiercingBellsResonance";
        
        private float homingStrength = 0f;
        
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.25f;
            
            // Ramp up homing over time
            homingStrength = Math.Min(homingStrength + 0.02f, 0.15f);
            
            // Find target
            NPC target = null;
            float closestDist = 500f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    target = npc;
                }
            }
            
            // Home toward target
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 12f, homingStrength);
            }
            
            // ☁EMUSICAL NOTATION - Homing note with chasing trail
            if (Main.rand.NextBool(6))
            {
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(255, 200, 50), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -0.6f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.3f, 30);
            }
            
            // Music note trail
            ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 1, 10f);
            
            // Fire glow
            if (Main.rand.NextBool(2))
            {
                Color color = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                var glow = new GenericGlowParticle(Projectile.Center,
                    Main.rand.NextVector2Circular(1f, 1f),
                    color, Main.rand.NextFloat(0.15f, 0.25f), Main.rand.Next(8, 14), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            Lighting.AddLight(Projectile.Center, 0.5f, 0.25f, 0.08f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // ☁EMUSICAL IMPACT - Homing note impact burst
            ThemedParticles.MusicNoteBurst(target.Center, new Color(255, 140, 40), 5, 3.5f);
            
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 4, 20f);
            ThemedParticles.LaCampanellaBloomBurst(target.Center, 0.35f);
            CustomParticles.HaloRing(target.Center, ThemedParticles.CampanellaOrange, 0.25f, 10);
            
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.6f, Volume = 0.35f }, target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            // Homing note death - smoke wisp dissolve for subtle fade
            DynamicParticleEffects.CampanellaDeathSmokeWispDissolve(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitY), 0.6f);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
    
    /// <summary>
    /// Music note mine that detonates when enemies approach.
    /// </summary>
    public class MusicNoteMine : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/GrandioseChime";
        
        private float pulseTimer = 0f;
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300; // 5 seconds
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
        }

        public override void AI()
        {
            pulseTimer += 0.1f;
            
            // Gentle floating
            Projectile.velocity.Y = (float)Math.Sin(pulseTimer) * 0.3f;
            Projectile.rotation += 0.05f;
            
            // ☁EMUSICAL NOTATION - Music mine with pulsing note aura
            if (Main.rand.NextBool(8))
            {
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(255, 200, 50), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -0.5f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.28f, 35);
            }
            
            // Music note pulsing effect
            if (Main.rand.NextBool(4))
            {
                ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 1, 12f);
            }
            
            // Pulsing glow
            if (Main.rand.NextBool(3))
            {
                float pulse = (float)Math.Sin(pulseTimer * 2f) * 0.5f + 0.5f;
                Color color = Color.Lerp(ThemedParticles.CampanellaOrange, ThemedParticles.CampanellaYellow, pulse);
                var glow = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextVector2Circular(0.3f, 0.3f),
                    color, Main.rand.NextFloat(0.1f, 0.2f), Main.rand.Next(10, 18), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Check for nearby enemies - detonate if close
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                if (Vector2.Distance(Projectile.Center, npc.Center) < 80f)
                {
                    Detonate();
                    break;
                }
            }
            
            float lightPulse = (float)Math.Sin(pulseTimer * 2f) * 0.2f + 0.4f;
            Lighting.AddLight(Projectile.Center, lightPulse, lightPulse * 0.5f, lightPulse * 0.15f);
        }
        
        private void Detonate()
        {
            // ☁EMUSICAL FINALE - Explosive music mine detonation
            ThemedParticles.MusicNoteBurst(Projectile.Center, new Color(218, 165, 32), 10, 6f);
            
            // Chain reaction - spawn more music notes
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f;
                Vector2 vel = angle.ToRotationVector2() * 8f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel,
                    ModContent.ProjectileType<HomingMusicNote>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
            
            // Explosion effect
            ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 8, 35f);
            ThemedParticles.LaCampanellaBloomBurst(Projectile.Center, 0.6f);
            ThemedParticles.LaCampanellaShockwave(Projectile.Center, 0.5f);
            
            CustomParticles.HaloRing(Projectile.Center, ThemedParticles.CampanellaOrange, 0.4f, 14);
            
            // AOE damage
            float radius = 100f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                if (Vector2.Distance(Projectile.Center, npc.Center) < radius)
                {
                    npc.SimpleStrikeNPC(Projectile.damage, 0, false, 0f, null, false, 0f, true);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 2);
                }
            }
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 0.5f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.2f, Volume = 0.4f }, Projectile.Center);
            
            Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            if (timeLeft > 0) return; // Already detonated
            
            // Mine fizzle out - ring of fire effect for explosive theme
            DynamicParticleEffects.CampanellaDeathRingOfFire(Projectile.Center, 0.7f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float pulse = (float)Math.Sin(pulseTimer * 2f) * 0.15f + 1f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            
            // Pulsing glow
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, 
                ThemedParticles.CampanellaOrange * 0.6f * pulse, Projectile.rotation, origin, 0.25f * pulse, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, 
                ThemedParticles.CampanellaYellow * 0.4f * pulse, Projectile.rotation, origin, 0.2f * pulse, SpriteEffects.None, 0);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }
    
    /// <summary>
    /// Orbiting music note that follows a rocket and deals contact damage.
    /// Explodes when the parent rocket dies or when hitting an enemy.
    /// </summary>
    public class OrbitingMusicNote : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/SymphonicBellfireAnnihilator";
        
        private float orbitAngle = 0f;
        private int parentRocketIndex = -1;
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // First frame - store parent rocket index
            if (Projectile.localAI[0] == 0)
            {
                parentRocketIndex = (int)Projectile.ai[0];
                orbitAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Projectile.localAI[0] = 1;
            }
            
            // Check if parent rocket still exists
            if (parentRocketIndex >= 0 && parentRocketIndex < Main.maxProjectiles)
            {
                Projectile parent = Main.projectile[parentRocketIndex];
                if (parent.active && parent.type == ModContent.ProjectileType<BellRocket>())
                {
                    // Orbit around parent
                    orbitAngle += 0.15f;
                    float orbitRadius = 30f;
                    Vector2 targetPos = parent.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                    
                    Projectile.velocity = (targetPos - Projectile.Center) * 0.3f;
                    Projectile.rotation = orbitAngle + MathHelper.PiOver2;
                }
                else
                {
                    // Parent died - explode
                    Explode();
                    return;
                }
            }
            else
            {
                // No parent - seek enemies
                NPC target = null;
                float closestDist = 300f;
                foreach (NPC npc in Main.npc)
                {
                    if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        target = npc;
                    }
                }
                
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 10f, 0.1f);
                }
                
                Projectile.rotation += 0.2f;
            }
            
            // ☁EMUSICAL NOTATION - Orbiting note with swirling trail
            if (Main.rand.NextBool(6))
            {
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(255, 200, 50), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.5f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.25f, 28);
            }
            
            // Music note trail
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 1, 8f);
            }
            
            // Fire glow
            if (Main.rand.NextBool(3))
            {
                Color color = Main.rand.NextBool() ? ThemedParticles.CampanellaOrange : ThemedParticles.CampanellaYellow;
                var glow = new GenericGlowParticle(Projectile.Center,
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    color, Main.rand.NextFloat(0.1f, 0.18f), Main.rand.Next(8, 14), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            Lighting.AddLight(Projectile.Center, 0.4f, 0.2f, 0.05f);
        }
        
        private void Explode()
        {
            // ☁EMUSICAL FINALE - Orbiting note explosion burst
            ThemedParticles.MusicNoteBurst(Projectile.Center, new Color(218, 165, 32), 6, 4f);
            
            ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 5, 25f);
            ThemedParticles.LaCampanellaBloomBurst(Projectile.Center, 0.35f);
            CustomParticles.HaloRing(Projectile.Center, ThemedParticles.CampanellaOrange, 0.25f, 10);
            
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 0.35f }, Projectile.Center);
            
            // AOE damage
            float radius = 60f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                if (Vector2.Distance(Projectile.Center, npc.Center) < radius)
                {
                    npc.SimpleStrikeNPC(Projectile.damage, 0, false, 0f, null, false, 0f, true);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 1);
                }
            }
            
            Projectile.Kill();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // ☁EMUSICAL IMPACT - Orbiting note impact burst
            ThemedParticles.MusicNoteBurst(target.Center, new Color(255, 140, 40), 5, 3.5f);
            
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 4, 20f);
            ThemedParticles.LaCampanellaBloomBurst(target.Center, 0.3f);
        }

        public override void OnKill(int timeLeft)
        {
            // Orbiting note death - bell toll shatter for musical death
            DynamicParticleEffects.CampanellaDeathBellTollShatter(Projectile.Center, 0.5f);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
    
    #endregion
}
