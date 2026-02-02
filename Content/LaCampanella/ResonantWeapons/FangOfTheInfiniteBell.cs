using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons
{
    /// <summary>
    /// Fang of the Infinite Bell - Magic weapon with flaming spiral and empowerment mechanics.
    /// On Cast: Creates a flaming spiral explosion at cursor location.
    /// After 3 hits: Gain damage buff (5s) + infinite mana (10s), with 20s cooldown.
    /// During buff: Shoots explosive versions that cause lightning strikes on impact.
    /// </summary>
    public class FangOfTheInfiniteBell : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 280;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.UseSound = SoundID.Item20;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<InfiniteBellSpiralProjectile>();
            Item.shootSpeed = 12f;
            Item.noMelee = true;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Creates a flaming spiral explosion at cursor location"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "After 3 hits, gain a damage buff and infinite mana for a short time"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "During empowerment, shoots explosive versions that cause lightning strikes"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The serpent's fang strikes with the infinite resonance of the bell'") { OverrideColor = new Color(255, 140, 40) });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 targetPos = Main.MouseWorld;
            
            // Check if player has the empowered buff - shoot explosive version instead
            var bellPlayer = player.GetModPlayer<InfiniteBellPlayer>();
            if (player.HasBuff(ModContent.BuffType<InfiniteBellEmpoweredBuff>()))
            {
                // Shoot explosive lightning version
                Projectile.NewProjectile(source, player.Center, (targetPos - player.Center).SafeNormalize(Vector2.UnitX) * Item.shootSpeed,
                    ModContent.ProjectileType<InfiniteBellExplosiveProjectile>(), (int)(damage * 1.5f), knockback, player.whoAmI);
            }
            
            // Always spawn the flaming spiral at cursor
            Projectile.NewProjectile(source, targetPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            // === CAST SOUNDS (2 max) ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.7f }, targetPos);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(-0.1f, 0.3f), Volume = 0.5f }, targetPos);
            
            // === BLACK TO ORANGE FLAMING SPIRAL SPAWN EFFECTS ===
            Color campanellaBlack = ThemedParticles.CampanellaBlack;
            
            // === UnifiedVFX LA CAMPANELLA IMPACT (handles core VFX) ===
            UnifiedVFX.LaCampanella.Impact(targetPos, 1.3f);
            
            // === UNIQUE: FANG BITE MARK PATTERN (keep this - it's unique!) ===
            // Two crescent "fangs" bite down at the target location
            for (int fang = 0; fang < 2; fang++)
            {
                float fangSide = fang == 0 ? -1f : 1f;
                float crescentStart = MathHelper.PiOver4 * fangSide;
                
                // Each fang is a crescent arc of particles
                for (int tooth = 0; tooth < 8; tooth++)
                {
                    float toothAngle = crescentStart + (tooth / 7f - 0.5f) * MathHelper.PiOver2 * fangSide;
                    float toothRadius = 35f - Math.Abs(tooth - 3.5f) * 4f; // Curved inward at tips
                    Vector2 toothPos = targetPos + toothAngle.ToRotationVector2() * toothRadius;
                    
                    // Fang gradient: tip is white-hot, base is orange
                    float toothProgress = Math.Abs(tooth - 3.5f) / 3.5f;
                    Color toothColor = Color.Lerp(Color.White, UnifiedVFX.LaCampanella.Orange, toothProgress);
                    
                    CustomParticles.GenericFlare(toothPos, toothColor, 0.4f - toothProgress * 0.15f, 15);
                }
            }
            
            // Light smoke burst (reduced to 3)
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                var smoke = new HeavySmokeParticle(targetPos + Main.rand.NextVector2Circular(15f, 15f),
                    vel, campanellaBlack, Main.rand.Next(25, 40), Main.rand.NextFloat(0.25f, 0.4f), 0.4f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Single halo ring
            CustomParticles.HaloRing(targetPos, ThemedParticles.CampanellaOrange, 0.45f, 16);
            
            // Screen shake
            // REMOVED: Screen shake disabled for La Campanella weapons
            // player.GetModPlayer<ScreenShakePlayer>()?.AddShake(4f, 8);
            
            Lighting.AddLight(targetPos, 1.2f, 0.6f, 0.2f);
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            // === SUBTLE SERPENT FANG AURA ===
            // Two "fangs" of fire orbit in opposite spirals (reduced intensity)
            float time = Main.GameUpdateCount * 0.04f;
            for (int fang = 0; fang < 2; fang++)
            {
                float baseAngle = time + fang * MathHelper.Pi;
                float spiralRadius = 22f + (float)Math.Sin(time * 2f + fang * 1.5f) * 6f;
                
                // Create serpentine wave pattern (reduced segments)
                for (int segment = 0; segment < 3; segment++)
                {
                    float segmentAngle = baseAngle - segment * 0.3f;
                    float segmentRadius = spiralRadius - segment * 3f;
                    Vector2 segmentPos = player.Center + segmentAngle.ToRotationVector2() * segmentRadius;
                    
                    float segmentProgress = segment / 2f;
                    Color fangColor = Color.Lerp(UnifiedVFX.LaCampanella.Orange, UnifiedVFX.LaCampanella.Black, segmentProgress);
                    float segmentScale = 0.18f - segment * 0.04f;
                    
                    if (Main.rand.NextBool(5))
                    {
                        var segment_ = new GenericGlowParticle(segmentPos, Vector2.Zero, fangColor, segmentScale, 6, true);
                        MagnumParticleHandler.SpawnParticle(segment_);
                    }
                }
            }
            
            // === INFINITY SYMBOL GLOW (empowered only, reduced) ===
            if (player.HasBuff(ModContent.BuffType<InfiniteBellEmpoweredBuff>()))
            {
                for (int i = 0; i < 8; i++)
                {
                    float t = i / 8f * MathHelper.TwoPi;
                    float x = (float)Math.Cos(t + time * 0.5f) * 18f;
                    float y = (float)Math.Sin(t + time * 0.5f) * (float)Math.Cos(t + time * 0.5f) * 12f;
                    Vector2 infinityPos = player.Center + new Vector2(x, y - 35f);
                    
                    Color infinityColor = Color.Lerp(UnifiedVFX.LaCampanella.Orange, Color.White, 0.5f);
                    CustomParticles.GenericFlare(infinityPos, infinityColor * 0.5f, 0.15f, 4);
                }
            }
            
            // Rare venom drip effect
            if (Main.rand.NextBool(20))
            {
                Vector2 dripPos = player.Center + Main.rand.NextVector2CircularEdge(25f, 25f);
                Vector2 dripVel = new Vector2(0, Main.rand.NextFloat(1.5f, 2.5f));
                Color venomColor = UnifiedVFX.LaCampanella.Orange;
                var drip = new GenericGlowParticle(dripPos, dripVel, venomColor, 0.1f, 15, true);
                MagnumParticleHandler.SpawnParticle(drip);
            }
            
            // Subtle light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Lighting.AddLight(player.Center, 0.4f * pulse, 0.2f * pulse, 0.06f * pulse);
        }

        public override void ModifyManaCost(Player player, ref float reduce, ref float mult)
        {
            // Infinite mana during empowered state
            if (player.HasBuff(ModContent.BuffType<InfiniteBellEmpoweredBuff>()))
            {
                mult = 0f;
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.15f + 1f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // === ORBITING GLYPHS AROUND WEAPON ===
            if (CustomParticleSystem.TexturesLoaded)
            {
                Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
                for (int i = 0; i < 5; i++)
                {
                    float glyphAngle = Main.GameUpdateCount * 0.05f + MathHelper.TwoPi * i / 5f;
                    Vector2 glyphPos = position + glyphAngle.ToRotationVector2() * 20f;
                    Color glyphColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, (float)i / 5f) * 0.6f;
                    spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, glyphAngle * 2f, glyphTex.Size() / 2f, 0.18f * pulse, SpriteEffects.None, 0f);
                }
            }
            
            // Black to orange gradient glow
            spriteBatch.Draw(texture, position, null, ThemedParticles.CampanellaOrange * 0.5f, rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, 0.4f) * 0.35f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, ThemedParticles.CampanellaBlack * 0.3f, rotation, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Lighting.AddLight(Item.Center, 0.6f, 0.35f, 0.1f);
            
            return true;
        }
    }

    /// <summary>
    /// ModPlayer to track hit counter and cooldowns for Fang of the Infinite Bell
    /// </summary>
    public class InfiniteBellPlayer : ModPlayer
    {
        public int HitCounter = 0;
        public const int HitsForBuff = 3;
        public int BuffCooldown = 0;
        public const int BuffCooldownMax = 20 * 60; // 20 seconds
        
        public override void PostUpdate()
        {
            if (BuffCooldown > 0)
                BuffCooldown--;
        }
        
        public void RegisterHit(Vector2 hitPosition)
        {
            HitCounter++;
            
            if (HitCounter >= HitsForBuff && BuffCooldown <= 0)
            {
                HitCounter = 0;
                ActivateEmpowerment(hitPosition);
            }
        }
        
        private void ActivateEmpowerment(Vector2 position)
        {
            BuffCooldown = BuffCooldownMax;
            
            // Grant buffs
            Player.AddBuff(ModContent.BuffType<InfiniteBellDamageBuff>(), 5 * 60); // 5 seconds damage
            Player.AddBuff(ModContent.BuffType<InfiniteBellEmpoweredBuff>(), 10 * 60); // 10 seconds empowered (infinite mana + explosive shots)
            
            // === EMPOWERMENT ACTIVATION VFX (cleaned) ===
            Color campanellaBlack = ThemedParticles.CampanellaBlack;
            Color campanellaOrange = ThemedParticles.CampanellaOrange;
            
            // 2 sounds max
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 0.9f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.4f, Volume = 0.6f }, Player.Center);
            
            // UnifiedVFX handles core impact
            UnifiedVFX.LaCampanella.Impact(Player.Center, 1.6f);
            
            // Heavy smoke (reduced to 3)
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                var smoke = new HeavySmokeParticle(Player.Center + Main.rand.NextVector2Circular(25f, 25f),
                    vel, campanellaBlack, Main.rand.Next(35, 55), Main.rand.NextFloat(0.35f, 0.55f), 0.5f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Single halo ring
            CustomParticles.HaloRing(Player.Center, campanellaOrange, 0.6f, 20);
            
            Lighting.AddLight(Player.Center, 1.8f, 0.9f, 0.25f);
        }
    }

    /// <summary>
    /// Flaming spiral projectile - explodes at cursor location with spiral pattern.
    /// </summary>
    public class InfiniteBellSpiralProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/FangOfTheInfiniteBell";
        
        private float spiralRotation = 0f;
        private int explosionTimer = 0;
        private const int ExplosionDelay = 8;
        
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        
        public override void AI()
        {
            spiralRotation += 0.3f;
            explosionTimer++;
            
            Color campanellaBlack = ThemedParticles.CampanellaBlack;
            Color campanellaOrange = ThemedParticles.CampanellaOrange;
            
            // === FLAMING SPIRAL VISUAL ===
            int spiralArms = 6;
            for (int arm = 0; arm < spiralArms; arm++)
            {
                float baseAngle = spiralRotation + MathHelper.TwoPi * arm / spiralArms;
                for (int point = 0; point < 5; point++)
                {
                    float spiralAngle = baseAngle + point * 0.4f;
                    float spiralRadius = 15f + point * 12f;
                    Vector2 particlePos = Projectile.Center + spiralAngle.ToRotationVector2() * spiralRadius;
                    
                    float progress = (float)point / 5f;
                    Color particleColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                    
                    var glow = new GenericGlowParticle(particlePos, Vector2.Zero, particleColor,
                        Main.rand.NextFloat(0.2f, 0.35f), Main.rand.Next(8, 15), true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }
            
            // Smoke swirl
            if (Main.rand.NextBool(2))
            {
                float angle = spiralRotation + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 smokePos = Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(20f, 50f);
                var smoke = new HeavySmokeParticle(smokePos, angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 2f,
                    campanellaBlack, Main.rand.Next(20, 35), Main.rand.NextFloat(0.2f, 0.35f), 0.4f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Periodic flares
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 flarePos = Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(15f, 45f);
                float progress = Main.rand.NextFloat();
                Color flareColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                CustomParticles.GenericFlare(flarePos, flareColor, 0.35f, 12);
            }
            
            // Explosion after delay
            if (explosionTimer >= ExplosionDelay)
            {
                SpawnSpiralExplosion();
                Projectile.Kill();
            }
            
            Lighting.AddLight(Projectile.Center, 0.8f, 0.4f, 0.12f);
        }
        
        private void SpawnSpiralExplosion()
        {
            Color campanellaBlack = ThemedParticles.CampanellaBlack;
            Color campanellaOrange = ThemedParticles.CampanellaOrange;
            Color campanellaGold = ThemedParticles.CampanellaGold;
            
            // 2 sounds max
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.6f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = 0.1f, Volume = 0.4f }, Projectile.Center);
            
            // === IRIDESCENT WINGSPAN STYLE IMPACT ===
            UnifiedVFX.LaCampanella.Impact(Projectile.Center, 1.2f);
            
            // === MUSIC NOTES BURST! ===
            ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 8, 40f);
            
            // === HEAVY SMOKE BURST ===
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f) + new Vector2(0, -1.5f);
                var smoke = new HeavySmokeParticle(Projectile.Center + Main.rand.NextVector2Circular(18f, 18f),
                    vel, campanellaBlack, Main.rand.Next(35, 50), Main.rand.NextFloat(0.35f, 0.5f), 0.5f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // === GRADIENT HALO RINGS (3 stacked) ===
            for (int ring = 0; ring < 3; ring++)
            {
                float progress = (float)ring / 3f;
                Color ringColor = Color.Lerp(campanellaOrange, campanellaGold, progress);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.4f + ring * 0.12f, 14 + ring * 3);
            }
            
            // === FIRE SHIMMER FLARE BURST ===
            for (int i = 0; i < 10; i++)
            {
                float fireHue = 0.02f + (i / 10f) * 0.08f;
                Color flareColor = Main.hslToRgb(fireHue, 1f, 0.7f);
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(16f, 16f), flareColor, 0.55f, 20);
            }
            
            // === RADIAL DUST EXPLOSION ===
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                float progress = (float)i / 16f;
                Color dustColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                int dustType = i % 2 == 0 ? DustID.OrangeTorch : DustID.Smoke;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(5f, 10f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, vel, i % 2 == 1 ? 80 : 0, dustColor, 1.7f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }
            
            // === GOLD SPARKLES ===
            for (int i = 0; i < 6; i++)
            {
                Dust gold = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), DustID.Enchanted_Gold,
                    Main.rand.NextVector2Circular(4f, 4f), 0, campanellaGold, 1.4f);
                gold.noGravity = true;
            }
            
            Lighting.AddLight(Projectile.Center, 1.4f, 0.7f, 0.25f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Resonant Toll
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // === SEEKING CRYSTALS - 33% chance on hit ===
            if (Main.rand.NextBool(3))
            {
                SeekingCrystalHelper.SpawnLaCampanellaCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.2f),
                    Projectile.knockBack,
                    Projectile.owner,
                    4
                );
            }
            
            // Register hit for empowerment
            Player owner = Main.player[Projectile.owner];
            owner.GetModPlayer<InfiniteBellPlayer>().RegisterHit(target.Center);
            
            // Hit VFX (cleaned)
            Color campanellaOrange = ThemedParticles.CampanellaOrange;
            
            Vector2 hitDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            ThemedParticles.LaCampanellaSparks(target.Center, hitDir, 4, 5f);
            
            // Music notes (reduced to 2)
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 2, 20f);
            
            // Smoke puff (reduced to 2)
            for (int i = 0; i < 2; i++)
            {
                var smoke = new HeavySmokeParticle(target.Center + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextVector2Circular(1.5f, 1.5f), ThemedParticles.CampanellaBlack,
                    Main.rand.Next(18, 30), Main.rand.NextFloat(0.18f, 0.3f), 0.35f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.2f, 0.6f), Volume = 0.35f }, target.Center);
            
            Lighting.AddLight(target.Center, 0.6f, 0.3f, 0.1f);
        }
        
        public override bool PreDraw(ref Color lightColor) => false; // Pure particle visual
    }

    /// <summary>
    /// Explosive projectile shot during empowered state - causes lightning on impact.
    /// </summary>
    public class InfiniteBellExplosiveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/FangOfTheInfiniteBell";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            
            Color campanellaBlack = ThemedParticles.CampanellaBlack;
            Color campanellaOrange = ThemedParticles.CampanellaOrange;
            Color campanellaGold = ThemedParticles.CampanellaGold;
            
            // === IRIDESCENT WINGSPAN STYLE - HEAVY DUST TRAILS (2+ per frame) ===
            for (int i = 0; i < 2; i++)
            {
                // Main flame trail - Black to Orange gradient
                float progress = Main.rand.NextFloat();
                Color dustColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                int dustType = progress < 0.5f ? DustID.Torch : DustID.OrangeTorch;
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), dustType,
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f),
                    progress < 0.3f ? 80 : 0, dustColor, 1.8f);
                d.noGravity = true;
                d.fadeIn = 1.4f;
            }
            
            // === CONTRASTING SPARKLES - Gold highlights against black smoke ===
            if (Main.rand.NextBool(2))
            {
                Dust gold = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), DustID.Enchanted_Gold,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    0, campanellaGold, 1.4f);
                gold.noGravity = true;
            }
            
            // === FIRE SHIMMER TRAIL - Like rainbow but with flame tones ===
            if (Main.rand.NextBool(3))
            {
                // Cycle through fire colors: red -> orange -> yellow -> gold
                float fireHue = 0.02f + Main.rand.NextFloat() * 0.08f; // 0.02-0.10 hue range (red-orange-yellow)
                Color fireShimmer = Main.hslToRgb(fireHue, 1f, 0.65f);
                Dust r = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f), 0, fireShimmer, 1.5f);
                r.noGravity = true;
            }
            
            // === EMBER SHIMMER - Like pearlescent but with warm tones ===
            if (Main.rand.NextBool(4))
            {
                Color ember = Main.rand.NextBool() ? new Color(255, 180, 100) : new Color(255, 140, 60);
                CustomParticles.GenericFlare(Projectile.Center, ember, 0.35f, 15);
            }
            
            // === FREQUENT FLARES (1 in 2) ===
            if (Main.rand.NextBool(2))
            {
                float flareProgress = Main.rand.NextFloat();
                Color flareColor = Color.Lerp(campanellaOrange, campanellaGold, flareProgress);
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), flareColor, 0.5f, 18);
            }
            
            // === HEAVY SMOKE TRAIL ===
            if (Main.rand.NextBool(2))
            {
                var smoke = new HeavySmokeParticle(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -Projectile.velocity * 0.08f + new Vector2(0, -0.5f), campanellaBlack,
                    Main.rand.Next(20, 35), Main.rand.NextFloat(0.25f, 0.4f), 0.45f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // === MUSIC NOTES - The bell's melody ===
            if (Main.rand.NextBool(6))
            {
                Color noteColor = Color.Lerp(campanellaOrange, campanellaGold, Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.35f, 35);
            }
            
            // === PULSING LIGHT ===
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f + 1f;
            Lighting.AddLight(Projectile.Center, 0.9f * pulse, 0.45f * pulse, 0.12f * pulse);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Resonant Toll
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 2);
            
            // Register hit
            Player owner = Main.player[Projectile.owner];
            owner.GetModPlayer<InfiniteBellPlayer>().RegisterHit(target.Center);
            
            // Music notes (reduced to 2)
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 2, 20f);
            
            // === EXPLOSION WITH LIGHTNING ===
            SpawnExplosionWithLightning(target.Center);
        }
        
        public override void OnKill(int timeLeft)
        {
            SpawnExplosionWithLightning(Projectile.Center);
        }
        
        private void SpawnExplosionWithLightning(Vector2 position)
        {
            Color campanellaBlack = ThemedParticles.CampanellaBlack;
            Color campanellaOrange = ThemedParticles.CampanellaOrange;
            Color campanellaGold = ThemedParticles.CampanellaGold;
            Color campanellaRed = ThemedParticles.CampanellaRed;
            
            // 2 sounds max
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.1f, Volume = 0.7f }, position);
            SoundEngine.PlaySound(SoundID.Item94 with { Pitch = 0.3f, Volume = 0.4f }, position); // Thunder sound
            
            // === IRIDESCENT WINGSPAN STYLE - LAYERED IMPACT ===
            // UnifiedVFX handles core impact
            UnifiedVFX.LaCampanella.Impact(position, 1.5f);
            
            // === MUSIC NOTES BURST - The bell's chord! ===
            ThemedParticles.LaCampanellaMusicNotes(position, 10, 44f);
            
            // === HEAVY SMOKE BURST (enhanced) ===
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f) + new Vector2(0, -2f);
                var smoke = new HeavySmokeParticle(position + Main.rand.NextVector2Circular(25f, 25f),
                    vel, campanellaBlack, Main.rand.Next(40, 60), Main.rand.NextFloat(0.4f, 0.6f), 0.55f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // === GRADIENT HALO RINGS (Iridescent style - 4 stacked rings!) ===
            for (int ring = 0; ring < 4; ring++)
            {
                float progress = (float)ring / 4f;
                // Fire gradient: Black -> Orange -> Gold -> White-hot
                Color ringColor;
                if (progress < 0.33f)
                    ringColor = Color.Lerp(campanellaBlack, campanellaOrange, progress * 3f);
                else if (progress < 0.66f)
                    ringColor = Color.Lerp(campanellaOrange, campanellaGold, (progress - 0.33f) * 3f);
                else
                    ringColor = Color.Lerp(campanellaGold, Color.White, (progress - 0.66f) * 3f);
                    
                CustomParticles.HaloRing(position, ringColor, 0.35f + ring * 0.15f, 14 + ring * 4);
            }
            
            // === FIRE SHIMMER FLARE BURST (like rainbow burst but fire tones) ===
            for (int i = 0; i < 12; i++)
            {
                float fireHue = 0.02f + (i / 12f) * 0.1f; // Red to yellow gradient
                Color flareColor = Main.hslToRgb(fireHue, 1f, 0.7f);
                CustomParticles.GenericFlare(position + Main.rand.NextVector2Circular(20f, 20f), flareColor, 0.6f, 24);
            }
            
            // === BLACK AND ORANGE EXPLOSION DUST (like B/W but fire themed) ===
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Color dustColor = i % 2 == 0 ? campanellaOrange : campanellaBlack;
                int dustType = i % 2 == 0 ? DustID.OrangeTorch : DustID.Smoke;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(6f, 12f);
                Dust d = Dust.NewDustPerfect(position, dustType, vel, i % 2 == 1 ? 100 : 0, dustColor, 1.9f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
            
            // === EMBER SPARK EXPLOSION (like rainbow sparks) ===
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                float fireHue = 0.02f + (i / 24f) * 0.08f;
                Color sparkColor = Main.hslToRgb(fireHue, 1f, 0.65f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(7f, 14f);
                Dust spark = Dust.NewDustPerfect(position, DustID.Torch, vel, 0, sparkColor, 1.8f);
                spark.noGravity = true;
                spark.fadeIn = 1.2f;
            }
            
            // === GOLD CONTRASTING SPARKLES ===
            for (int i = 0; i < 8; i++)
            {
                Dust gold = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(15f, 15f), DustID.Enchanted_Gold,
                    Main.rand.NextVector2Circular(5f, 5f), 0, campanellaGold, 1.6f);
                gold.noGravity = true;
            }
            
            // === LIGHTNING STRIKE (keep - it's unique!) ===
            Vector2 skyPoint = position + new Vector2(Main.rand.NextFloat(-80f, 80f), -500f);
            MagnumVFX.DrawLaCampanellaLightning(skyPoint, position, 16, 70f, 5, 0.5f);
            
            // Single secondary branch
            float branchAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 branchEnd = position + branchAngle.ToRotationVector2() * Main.rand.NextFloat(60f, 120f);
            MagnumVFX.DrawLaCampanellaLightning(position, branchEnd, 6, 30f, 2, 0.35f);
            
            // Damage nearby enemies with lightning
            float lightningRadius = 200f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                float distance = Vector2.Distance(position, npc.Center);
                if (distance <= lightningRadius && distance > 30f)
                {
                    // Draw lightning to enemy
                    MagnumVFX.DrawLaCampanellaLightning(position, npc.Center, 10, 40f, 4, 0.5f);
                    
                    // Deal bonus damage
                    int lightningDamage = Projectile.damage / 3;
                    npc.SimpleStrikeNPC(lightningDamage, 0, false, 0f, null, false, 0f, true);
                    
                    // Apply debuff
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 1);
                    
                    // Lightning hit VFX
                    for (int i = 0; i < 4; i++)
                    {
                        float progress = (float)i / 4f;
                        float angle = MathHelper.TwoPi * i / 4f;
                        Vector2 flarePos = npc.Center + angle.ToRotationVector2() * 15f;
                        Color flareColor = Color.Lerp(campanellaOrange, campanellaRed, progress);
                        CustomParticles.GenericFlare(flarePos, flareColor, 0.4f, 12);
                    }
                }
            }
            
            // Screen shake
            // REMOVED: Screen shake disabled for La Campanella weapons
            // Player owner = Main.player[Projectile.owner];
            // owner.GetModPlayer<ScreenShakePlayer>()?.AddShake(10f, 18);
            
            // Sky flash
            if (!Main.dedServ && SkyManager.Instance["MagnumOpus:LaCampanellaSky"] is LaCampanellaSkyEffect sky)
            {
                sky.TriggerFlash(0.6f);
            }
            
            Lighting.AddLight(position, 2.5f, 1.2f, 0.4f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            
            Color campanellaOrange = ThemedParticles.CampanellaOrange;
            Color campanellaGold = ThemedParticles.CampanellaGold;
            Color campanellaBlack = ThemedParticles.CampanellaBlack;
            
            // === IRIDESCENT WINGSPAN STYLE - GRADIENT TRAIL ===
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = 1f - (i / (float)Projectile.oldPos.Length);
                float trailScale = Projectile.scale * (0.4f + progress * 0.6f);
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailRotation = Projectile.oldRot[i];
                
                // Fire gradient trail: Black -> Orange -> Gold
                Color trailColor;
                if (progress < 0.5f)
                    trailColor = Color.Lerp(campanellaBlack, campanellaOrange, progress * 2f);
                else
                    trailColor = Color.Lerp(campanellaOrange, campanellaGold, (progress - 0.5f) * 2f);
                trailColor *= progress * 0.7f;
                
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, trailRotation, origin, trailScale * 0.6f, SpriteEffects.None, 0);
            }
            
            // === MULTI-LAYER ADDITIVE GLOW (Iridescent Wingspan style) ===
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.15f + 1f;
            Vector2 centerPos = Projectile.Center - Main.screenPosition;
            
            // Outer orange glow
            Main.EntitySpriteDraw(texture, centerPos, null, 
                campanellaOrange * 0.5f, Projectile.rotation, origin, Projectile.scale * pulse * 1.4f, SpriteEffects.None, 0);
            // Middle gold glow
            Main.EntitySpriteDraw(texture, centerPos, null, 
                campanellaGold * 0.4f, Projectile.rotation, origin, Projectile.scale * pulse * 1.2f, SpriteEffects.None, 0);
            // Inner black shadow
            Main.EntitySpriteDraw(texture, centerPos, null, 
                campanellaBlack * 0.3f, Projectile.rotation, origin, Projectile.scale * pulse * 1.05f, SpriteEffects.None, 0);
            // White-hot ember shimmer
            Color emberColor = Color.Lerp(new Color(255, 200, 150), new Color(255, 160, 80), (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.5f + 0.5f);
            Main.EntitySpriteDraw(texture, centerPos, null, 
                emberColor * 0.25f, Projectile.rotation, origin, Projectile.scale * pulse * 1.1f, SpriteEffects.None, 0);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Main projectile
            Main.EntitySpriteDraw(texture, centerPos, null, 
                lightColor, Projectile.rotation, origin, Projectile.scale * 0.6f, SpriteEffects.None, 0);
            
            return false;
        }
    }

    /// <summary>
    /// Damage buff from 3-hit activation (5 seconds)
    /// </summary>
    public class InfiniteBellDamageBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            // Damage increase
            player.GetDamage(DamageClass.Magic) += 0.25f; // 25% magic damage
            player.GetDamage(DamageClass.Generic) += 0.15f; // 15% all damage
            
            // Buff visual - orange/black aura
            if (Main.rand.NextBool(4))
            {
                float progress = Main.rand.NextFloat();
                Color color = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, progress);
                var glow = new GenericGlowParticle(player.Center + Main.rand.NextVector2Circular(20f, 20f),
                    new Vector2(0, -1f), color, Main.rand.NextFloat(0.2f, 0.35f), Main.rand.Next(15, 25), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            Lighting.AddLight(player.Center, 0.6f, 0.3f, 0.1f);
        }
    }

    /// <summary>
    /// Empowered state buff (10 seconds) - infinite mana + explosive shots
    /// </summary>
    public class InfiniteBellEmpoweredBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            // Visual indicator - more intense aura
            if (Main.rand.NextBool(3))
            {
                float progress = Main.rand.NextFloat();
                Color color = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, progress);
                var glow = new GenericGlowParticle(player.Center + Main.rand.NextVector2Circular(25f, 25f),
                    Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, -1.5f), color,
                    Main.rand.NextFloat(0.25f, 0.4f), Main.rand.Next(18, 28), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Heavy smoke wisps during empowered state
            if (Main.rand.NextBool(5))
            {
                var smoke = new HeavySmokeParticle(player.Center + Main.rand.NextVector2Circular(20f, 20f),
                    new Vector2(Main.rand.NextFloat(-1f, 1f), -1.2f), ThemedParticles.CampanellaBlack,
                    Main.rand.Next(25, 40), Main.rand.NextFloat(0.2f, 0.35f), 0.4f, 0.015f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Occasional flares
            if (Main.rand.NextBool(8))
            {
                CustomParticles.GenericFlare(player.Center + Main.rand.NextVector2Circular(30f, 30f), 
                    ThemedParticles.CampanellaOrange, 0.3f, 15);
            }
            
            Lighting.AddLight(player.Center, 0.8f, 0.4f, 0.12f);
        }
    }
}
