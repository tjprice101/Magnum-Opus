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
            
            // === EPIC CAST SOUND STACK ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.7f }, targetPos);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(-0.1f, 0.3f), Volume = 0.5f }, targetPos);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Pitch = 0.3f, Volume = 0.35f }, targetPos);
            
            // === BLACK TO ORANGE FLAMING SPIRAL SPAWN EFFECTS ===
            Color campanellaBlack = ThemedParticles.CampanellaBlack;
            Color campanellaOrange = ThemedParticles.CampanellaOrange;
            
            // === UnifiedVFX LA CAMPANELLA IMPACT ===
            UnifiedVFX.LaCampanella.Impact(targetPos, 1.3f);
            
            // === UNIQUE: FANG BITE MARK PATTERN ===
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
            
            // Black to orange gradient spiral burst
            for (int i = 0; i < 12; i++)
            {
                float progress = (float)i / 12f;
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 flarePos = targetPos + angle.ToRotationVector2() * Main.rand.NextFloat(25f, 45f);
                Color flareColor = Color.Lerp(UnifiedVFX.LaCampanella.Black, UnifiedVFX.LaCampanella.Orange, progress);
                CustomParticles.GenericFlare(flarePos, flareColor, 0.55f, 19);
            }
            
            // Heavy smoke burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                var smoke = new HeavySmokeParticle(targetPos + Main.rand.NextVector2Circular(20f, 20f),
                    vel, campanellaBlack, Main.rand.Next(30, 50), Main.rand.NextFloat(0.3f, 0.5f), 0.5f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Halo rings - black to orange gradient
            for (int ring = 0; ring < 4; ring++)
            {
                float progress = (float)ring / 4f;
                Color ringColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                CustomParticles.HaloRing(targetPos, ringColor, 0.4f + ring * 0.12f, 15 + ring * 3);
            }
            
            // Screen shake
            // REMOVED: Screen shake disabled for La Campanella weapons
            // player.GetModPlayer<ScreenShakePlayer>()?.AddShake(4f, 8);
            
            Lighting.AddLight(targetPos, 1.2f, 0.6f, 0.2f);
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            // === UNIQUE: SERPENT FANG AURA ===
            // The "Fang" emerges - serpentine fire that coils around the player
            
            // === COILING SERPENT FLAMES ===
            // Two "fangs" of fire orbit in opposite spirals
            float time = Main.GameUpdateCount * 0.04f;
            for (int fang = 0; fang < 2; fang++)
            {
                float baseAngle = time + fang * MathHelper.Pi; // Opposite sides
                float spiralRadius = 25f + (float)Math.Sin(time * 2f + fang * 1.5f) * 8f;
                
                // Create serpentine wave pattern
                for (int segment = 0; segment < 5; segment++)
                {
                    float segmentAngle = baseAngle - segment * 0.3f;
                    float segmentRadius = spiralRadius - segment * 3f;
                    Vector2 segmentPos = player.Center + segmentAngle.ToRotationVector2() * segmentRadius;
                    
                    // Gradient from tip (bright orange) to tail (black)
                    float segmentProgress = segment / 4f;
                    Color fangColor = Color.Lerp(UnifiedVFX.LaCampanella.Orange, UnifiedVFX.LaCampanella.Black, segmentProgress);
                    float segmentScale = 0.25f - segment * 0.04f;
                    
                    if (Main.rand.NextBool(3))
                    {
                        var segment_ = new GenericGlowParticle(segmentPos, Vector2.Zero, fangColor, segmentScale, 8, true);
                        MagnumParticleHandler.SpawnParticle(segment_);
                    }
                }
            }
            
            // === INFINITY SYMBOL GLOW (for "Infinite" theme) ===
            // When empowered, show ∁Esymbol
            if (player.HasBuff(ModContent.BuffType<InfiniteBellEmpoweredBuff>()))
            {
                // Draw infinity symbol with particles
                for (int i = 0; i < 12; i++)
                {
                    float t = i / 12f * MathHelper.TwoPi;
                    // Parametric infinity symbol: x = cos(t), y = sin(t)*cos(t)
                    float x = (float)Math.Cos(t + time * 0.5f) * 20f;
                    float y = (float)Math.Sin(t + time * 0.5f) * (float)Math.Cos(t + time * 0.5f) * 15f;
                    Vector2 infinityPos = player.Center + new Vector2(x, y - 40f); // Above player
                    
                    Color infinityColor = Color.Lerp(UnifiedVFX.LaCampanella.Orange, Color.White, 0.5f);
                    CustomParticles.GenericFlare(infinityPos, infinityColor * 0.7f, 0.18f, 5);
                }
            }
            
            // === VENOM DRIP EFFECT ===
            // Occasional "venom" drips from the fang tips
            if (Main.rand.NextBool(15))
            {
                Vector2 dripPos = player.Center + Main.rand.NextVector2CircularEdge(28f, 28f);
                Vector2 dripVel = new Vector2(0, Main.rand.NextFloat(1.5f, 3f)); // Falls down
                Color venomColor = Color.Lerp(UnifiedVFX.LaCampanella.Orange, new Color(255, 200, 50), Main.rand.NextFloat(0.5f));
                var drip = new GenericGlowParticle(dripPos, dripVel, venomColor, 0.12f, 20, true);
                MagnumParticleHandler.SpawnParticle(drip);
            }
            
            // Smoke wisps (original)
            if (Main.rand.NextBool(12))
            {
                var smoke = new HeavySmokeParticle(player.Center + Main.rand.NextVector2Circular(20f, 20f),
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.8f), UnifiedVFX.LaCampanella.Black,
                    Main.rand.Next(25, 40), Main.rand.NextFloat(0.14f, 0.22f), 0.32f, 0.016f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // === MUSIC NOTES ===
            if (Main.rand.NextBool(20))
            {
                ThemedParticles.LaCampanellaMusicNotes(player.Center, 1, 25f);
            }
            
            // Gradient light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Vector3 lightColor = Color.Lerp(UnifiedVFX.LaCampanella.Black, UnifiedVFX.LaCampanella.Orange, 0.5f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * pulse * 0.5f);
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
            
            // === MASSIVE EMPOWERMENT ACTIVATION VFX ===
            Color campanellaBlack = ThemedParticles.CampanellaBlack;
            Color campanellaOrange = ThemedParticles.CampanellaOrange;
            Color campanellaRed = ThemedParticles.CampanellaRed;
            
            // Epic sounds
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 1f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.4f, Volume = 0.7f }, Player.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = 0.2f, Volume = 0.6f }, Player.Center);
            
            // Grand impact effects
            ThemedParticles.LaCampanellaImpact(Player.Center, 2f);
            ThemedParticles.LaCampanellaShockwave(Player.Center, 1.5f);
            ThemedParticles.LaCampanellaBellChime(Player.Center, 1.5f);
            
            // Massive black to orange gradient burst
            for (int i = 0; i < 16; i++)
            {
                float progress = (float)i / 16f;
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 flarePos = Player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(40f, 70f);
                Color flareColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                CustomParticles.GenericFlare(flarePos, flareColor, 0.7f, 25);
            }
            
            // Inner red/orange burst
            for (int i = 0; i < 10; i++)
            {
                float progress = (float)i / 10f;
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 flarePos = Player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(20f, 40f);
                Color flareColor = Color.Lerp(campanellaOrange, campanellaRed, progress);
                CustomParticles.GenericFlare(flarePos, flareColor, 0.55f, 20);
            }
            
            // Heavy smoke explosion
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                float progress = (float)i / 12f;
                Color smokeColor = Color.Lerp(campanellaBlack, new Color(40, 25, 15), progress);
                var smoke = new HeavySmokeParticle(Player.Center + Main.rand.NextVector2Circular(30f, 30f),
                    vel, smokeColor, Main.rand.Next(40, 60), Main.rand.NextFloat(0.4f, 0.6f), 0.55f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Multiple halo rings - black to orange
            for (int ring = 0; ring < 6; ring++)
            {
                float progress = (float)ring / 6f;
                Color ringColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                CustomParticles.HaloRing(Player.Center, ringColor, 0.5f + ring * 0.2f, 20 + ring * 5);
            }
            
            // Screen shake
            // REMOVED: Screen shake disabled for La Campanella weapons
            // Player.GetModPlayer<ScreenShakePlayer>()?.AddShake(12f, 25);
            
            Lighting.AddLight(Player.Center, 2f, 1f, 0.3f);
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
            Color campanellaRed = ThemedParticles.CampanellaRed;
            
            // === EPIC EXPLOSION SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.6f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = 0.1f, Volume = 0.5f }, Projectile.Center);
            
            // Impact effects
            ThemedParticles.LaCampanellaImpact(Projectile.Center, 1.3f);
            ThemedParticles.LaCampanellaShockwave(Projectile.Center, 1f);
            
            // Black to orange explosion burst
            for (int i = 0; i < 14; i++)
            {
                float progress = (float)i / 14f;
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 flarePos = Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(30f, 55f);
                Color flareColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                CustomParticles.GenericFlare(flarePos, flareColor, 0.55f, 20);
            }
            
            // Heavy smoke burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f) + new Vector2(0, -1.5f);
                var smoke = new HeavySmokeParticle(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    vel, campanellaBlack, Main.rand.Next(35, 55), Main.rand.NextFloat(0.35f, 0.55f), 0.5f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Spark burst
            CustomParticles.ExplosionBurst(Projectile.Center, campanellaOrange, 16, 8f);
            
            // Halo rings
            for (int ring = 0; ring < 4; ring++)
            {
                float progress = (float)ring / 4f;
                Color ringColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.4f + ring * 0.12f, 15 + ring * 4);
            }
            
            // REMOVED: Screen shake disabled for La Campanella weapons
            // Player owner = Main.player[Projectile.owner];
            // owner.GetModPlayer<ScreenShakePlayer>()?.AddShake(5f, 10);
            
            Lighting.AddLight(Projectile.Center, 1.5f, 0.75f, 0.25f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Resonant Toll
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // Register hit for empowerment
            Player owner = Main.player[Projectile.owner];
            owner.GetModPlayer<InfiniteBellPlayer>().RegisterHit(target.Center);
            
            // Hit VFX
            Color campanellaBlack = ThemedParticles.CampanellaBlack;
            Color campanellaOrange = ThemedParticles.CampanellaOrange;
            
            Vector2 hitDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            ThemedParticles.LaCampanellaSparks(target.Center, hitDir, 5, 6f);
            
            // === SIGNATURE FRACTAL FLARE BURST - BLACK ↁEORANGE ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = angle.ToRotationVector2() * 30f;
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, progress);
                CustomParticles.GenericFlare(target.Center + flareOffset, fractalColor, 0.45f, 18);
            }
            
            // === GLYPH IMPACT - SPIRAL PROJECTILE ===
            if (CustomParticleSystem.TexturesLoaded)
            {
                for (int i = 0; i < 4; i++)
                {
                    float glyphAngle = MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.05f;
                    Vector2 glyphPos = target.Center + glyphAngle.ToRotationVector2() * 28f;
                    Color glyphColor = Color.Lerp(campanellaBlack, campanellaOrange, (float)i / 4f) * 0.65f;
                    CustomParticles.Glyph(glyphPos, glyphColor, 0.25f, -1);
                }
            }
            
            // Music notes on hit
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 3, 25f);
            
            // Smoke puff
            for (int i = 0; i < 3; i++)
            {
                var smoke = new HeavySmokeParticle(target.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(2f, 2f), campanellaBlack,
                    Main.rand.Next(20, 35), Main.rand.NextFloat(0.2f, 0.35f), 0.4f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.2f, 0.6f), Volume = 0.4f }, target.Center);
            
            Lighting.AddLight(target.Center, 0.8f, 0.4f, 0.12f);
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
            
            // === BLACK TO ORANGE FLAME TRAIL ===
            if (Main.rand.NextBool(2))
            {
                float progress = Main.rand.NextFloat();
                Color trailColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                var glow = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    -Projectile.velocity * 0.1f, trailColor, Main.rand.NextFloat(0.25f, 0.4f), Main.rand.Next(12, 20), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Heavy smoke trail
            if (Main.rand.NextBool(3))
            {
                var smoke = new HeavySmokeParticle(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -Projectile.velocity * 0.05f, campanellaBlack,
                    Main.rand.Next(20, 35), Main.rand.NextFloat(0.2f, 0.35f), 0.4f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Occasional flares
            if (Main.rand.NextBool(5))
            {
                CustomParticles.GenericFlare(Projectile.Center, campanellaOrange, 0.35f, 10);
            }
            
            Lighting.AddLight(Projectile.Center, 0.7f, 0.35f, 0.1f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Resonant Toll
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 2);
            
            // Register hit
            Player owner = Main.player[Projectile.owner];
            owner.GetModPlayer<InfiniteBellPlayer>().RegisterHit(target.Center);
            
            // === SIGNATURE FRACTAL FLARE BURST - BLACK ↁEORANGE ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = angle.ToRotationVector2() * 30f;
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, progress);
                CustomParticles.GenericFlare(target.Center + flareOffset, fractalColor, 0.45f, 18);
            }
            
            // === GLYPH IMPACT - ARCANE EXPLOSIVE ===
            if (CustomParticleSystem.TexturesLoaded)
            {
                for (int i = 0; i < 4; i++)
                {
                    float glyphAngle = MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.04f;
                    Vector2 glyphPos = target.Center + glyphAngle.ToRotationVector2() * 30f;
                    Color glyphColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, (float)i / 4f) * 0.6f;
                    CustomParticles.Glyph(glyphPos, glyphColor, 0.26f, -1);
                }
            }
            
            // Music notes on hit
            ThemedParticles.LaCampanellaMusicNotes(target.Center, 4, 30f);
            
            // === MASSIVE EXPLOSION WITH LIGHTNING ===
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
            Color campanellaRed = ThemedParticles.CampanellaRed;
            
            // === EPIC EXPLOSION SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.1f, Volume = 0.8f }, position);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = -0.1f, Volume = 0.6f }, position);
            SoundEngine.PlaySound(SoundID.Item94 with { Pitch = 0.3f, Volume = 0.5f }, position); // Thunder/lightning sound
            
            // Impact effects
            ThemedParticles.LaCampanellaImpact(position, 1.8f);
            ThemedParticles.LaCampanellaShockwave(position, 1.3f);
            ThemedParticles.LaCampanellaBellChime(position, 1.2f);
            
            // Black to orange explosion burst
            for (int i = 0; i < 16; i++)
            {
                float progress = (float)i / 16f;
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 flarePos = position + angle.ToRotationVector2() * Main.rand.NextFloat(35f, 60f);
                Color flareColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                CustomParticles.GenericFlare(flarePos, flareColor, 0.6f, 22);
            }
            
            // Heavy smoke explosion
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f) + new Vector2(0, -2f);
                var smoke = new HeavySmokeParticle(position + Main.rand.NextVector2Circular(25f, 25f),
                    vel, campanellaBlack, Main.rand.Next(40, 60), Main.rand.NextFloat(0.4f, 0.6f), 0.55f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Spark burst
            CustomParticles.ExplosionBurst(position, campanellaOrange, 20, 10f);
            
            // Halo rings
            for (int ring = 0; ring < 5; ring++)
            {
                float progress = (float)ring / 5f;
                Color ringColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                CustomParticles.HaloRing(position, ringColor, 0.45f + ring * 0.15f, 18 + ring * 4);
            }
            
            // === MASSIVE LIGHTNING STRIKES ===
            // Draw lightning from sky to explosion point
            Vector2 skyPoint = position + new Vector2(Main.rand.NextFloat(-100f, 100f), -600f);
            MagnumVFX.DrawLaCampanellaLightning(skyPoint, position, 20, 80f, 8, 0.6f);
            
            // Secondary lightning branches
            for (int i = 0; i < 3; i++)
            {
                float branchAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 branchEnd = position + branchAngle.ToRotationVector2() * Main.rand.NextFloat(80f, 150f);
                MagnumVFX.DrawLaCampanellaLightning(position, branchEnd, 8, 35f, 3, 0.4f);
                
                // Small explosion at branch end
                for (int j = 0; j < 6; j++)
                {
                    float progress = (float)j / 6f;
                    float angle = MathHelper.TwoPi * j / 6f;
                    Vector2 flarePos = branchEnd + angle.ToRotationVector2() * Main.rand.NextFloat(10f, 20f);
                    Color flareColor = Color.Lerp(campanellaBlack, campanellaOrange, progress);
                    CustomParticles.GenericFlare(flarePos, flareColor, 0.35f, 15);
                }
            }
            
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
            
            // Draw trail with black to orange gradient
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = 1f - (i / (float)Projectile.oldPos.Length);
                float trailScale = Projectile.scale * (0.5f + progress * 0.5f);
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailRotation = Projectile.oldRot[i];
                
                Color trailColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, progress) * progress * 0.6f;
                
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, trailRotation, origin, trailScale * 0.6f, SpriteEffects.None, 0);
            }
            
            // Additive glow
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, 
                ThemedParticles.CampanellaOrange * 0.6f, Projectile.rotation, origin, Projectile.scale * 0.8f, SpriteEffects.None, 0);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Main projectile
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, 
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
