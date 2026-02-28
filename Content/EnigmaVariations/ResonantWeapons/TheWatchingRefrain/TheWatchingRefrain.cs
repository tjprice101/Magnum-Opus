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
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.EnigmaVariations.Debuffs;
using Terraria.GameContent;
using ReLogic.Content;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheWatchingRefrain.Particles;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheWatchingRefrain.Dusts;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheWatchingRefrain.Utilities;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheWatchingRefrain
{
    /// <summary>
    /// THE WATCHING REFRAIN - Summoner weapon that summons Unsolved Phantom minions
    /// Phantoms attack enemies with bolts, create rifts, and mystery zones
    /// </summary>
    public class TheWatchingRefrain : ModItem
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/TheWatchingRefrain/TheWatchingRefrain";
        
        public override void SetDefaults()
        {
            Item.damage = 220;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 14;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = SoundID.Item44;
            Item.shoot = ModContent.ProjectileType<UnsolvedPhantomMinion>();
            Item.buffType = ModContent.BuffType<UnsolvedPhantomBuff>();
            Item.noMelee = true;
        }
        
        public override void HoldItem(Player player)
        {
            int phantomCount = 0;
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<UnsolvedPhantomMinion>())
                    phantomCount++;
            }
            
            Lighting.AddLight(player.Center, EnigmaPurple.ToVector3() * (0.1f + phantomCount * 0.03f));
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect", "Summons an Unsolved Phantom to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "Phantoms fire homing bolts and create mystery zones"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "Mystery zones slow and pull enemies toward their center"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'The refrain repeats, watching, waiting — always watching.'")
            {
                OverrideColor = EnigmaPurple
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.5f }, position);
            return false;
        }
    }
    
    public class UnsolvedPhantomBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<UnsolvedPhantomMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
    
    public class UnsolvedPhantomMinion : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private int phaseTimer = 0;
        private float visibility = 0f;
        private int attackCooldown = 0;
        private int mysteryZoneCooldown = 0;
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D bloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 bloomOrigin = bloom.Size() / 2f;
            
            // === Shader overlay: Procedural watching eyes on phantom ===
            EnigmaShaderHelper.DrawShaderOverlay(sb, ShaderLoader.WatchingPhantomAura,
                bloom, drawPos, bloomOrigin, 1.8f,
                WatchingUtils.GazeGreen.ToVector3(), WatchingUtils.RefrainPurple.ToVector3(),
                opacity: 0.5f * Projectile.Opacity, intensity: 1.0f,
                noiseTexture: ShaderLoader.GetNoiseTexture("PerlinNoise"),
                techniqueName: "WatchingPhantomGhost");
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer pulsing glow — RefrainPurple, ghostly halo
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.RefrainPurple * 0.3f, 0f, bloomOrigin, 0.6f * pulse, SpriteEffects.None, 0f);
            
            // Inner phantom body — GazeGreen, brighter core
            float innerPulse = 0.9f + 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f + 1f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.GazeGreen * 0.5f, 0f, bloomOrigin, 0.3f * innerPulse, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.SpectralMint * 0.25f, 0f, bloomOrigin, 0.15f, SpriteEffects.None, 0f);
            
            // Draw the minion's actual sprite with ghostly transparency
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 texOrigin = tex.Size() / 2f;
            sb.Draw(tex, drawPos, null, WatchingUtils.RefrainPurple * (0.6f * Projectile.Opacity), Projectile.rotation, texOrigin, Projectile.scale, SpriteEffects.None, 0f);
            
            WatchingUtils.ExitShaderRegion(sb);
            return false;
        }
        
        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1f;
            Projectile.minion = true;
        }
        
        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Check if minion should still exist
            if (!CheckActive(owner)) return;
            
            phaseTimer++;
            
            // Fade in/out
            visibility = MathHelper.Lerp(visibility, 1f, 0.05f);
            Projectile.Opacity = visibility;
            
            // Decrease cooldowns
            if (attackCooldown > 0) attackCooldown--;
            if (mysteryZoneCooldown > 0) mysteryZoneCooldown--;
            
            // Find target
            NPC target = FindTarget(owner);
            
            if (target != null)
            {
                // Move toward enemy
                Vector2 toTarget = (target.Center - Projectile.Center);
                float dist = toTarget.Length();
                toTarget = toTarget.SafeNormalize(Vector2.UnitX);
                
                float desiredDist = 150f; // Hover at range
                if (dist > desiredDist + 50f)
                {
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 10f, 0.05f);
                }
                else if (dist < desiredDist - 50f)
                {
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, -toTarget * 5f, 0.05f);
                }
                else
                {
                    // Orbit sideways
                    Vector2 sideways = new Vector2(-toTarget.Y, toTarget.X);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, sideways * 4f, 0.05f);
                }
                
                // Attack
                if (attackCooldown <= 0 && dist < 400f)
                {
                    AttackTarget(target);
                    attackCooldown = 40;
                }
                
                // Create mystery zone
                if (mysteryZoneCooldown <= 0 && dist < 300f)
                {
                    CreateMysteryZone(target);
                    mysteryZoneCooldown = 300;
                }
            }
            else
            {
                // Idle: follow owner
                Vector2 toOwner = (owner.Center - Projectile.Center);
                float distToOwner = toOwner.Length();
                
                if (distToOwner > 300f)
                {
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toOwner.SafeNormalize(Vector2.UnitX) * 12f, 0.08f);
                }
                else if (distToOwner > 80f)
                {
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toOwner.SafeNormalize(Vector2.UnitX) * 4f, 0.03f);
                }
                else
                {
                    Projectile.velocity *= 0.95f;
                }
            }
            
            // Velocity cap
            if (Projectile.velocity.Length() > 14f)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 14f;
            
            Projectile.rotation += 0.02f;
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.25f);
            
            // Ambient particles
            if (Main.GameUpdateCount % 3 == 0)
            {
                WatchingParticleHandler.Spawn(new PhantomWispParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    8f,
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    WatchingUtils.RefrainPurple,
                    Main.rand.NextFloat(0.2f, 0.3f),
                    40));
            }
            if (Main.GameUpdateCount % 10 == 0)
            {
                Vector2 eyeOffset = new Vector2(0, Main.rand.NextBool() ? -16f : 16f);
                WatchingParticleHandler.Spawn(new WatchingEyeParticle(
                    Projectile.Center + eyeOffset,
                    Vector2.Zero,
                    WatchingUtils.GazeGreen,
                    0.15f,
                    20));
            }
            if (Main.GameUpdateCount % 5 == 0)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<WatchingPhantomDust>(), 0f, -0.5f);
            }
        }
        
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<UnsolvedPhantomBuff>());
                Projectile.Kill();
                return false;
            }
            if (owner.HasBuff(ModContent.BuffType<UnsolvedPhantomBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            return true;
        }
        
        private NPC FindTarget(Player owner)
        {
            NPC bestTarget = null;
            float bestDist = 800f;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestTarget = npc;
                }
            }
            
            return bestTarget;
        }
        
        private void AttackTarget(NPC target)
        {
            SoundEngine.PlaySound(SoundID.Item125 with { Pitch = 0.3f, Volume = 0.4f }, Projectile.Center);
            
            Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 12f;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, toTarget,
                ModContent.ProjectileType<PhantomBolt>(),
                Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
        
        private void CreateMysteryZone(NPC target)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 0.5f }, target.Center);
            
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                ModContent.ProjectileType<MysteryZone>(),
                (int)(Projectile.damage * 0.4f), 0f, Projectile.owner);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.4f);
        }
    }
    
    public class PhantomBolt : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private List<int> hitEnemies = new List<int>();
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/MusicNote";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Texture2D bloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 bloomOrigin = bloom.Size() / 2f;
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Velocity-stretched trail — MagicPixel bar
            float rot = Projectile.velocity.ToRotation();
            Vector2 trailScale = new Vector2(18f / pixel.Width, 6f / pixel.Height);
            sb.Draw(pixel, drawPos, null, WatchingUtils.RefrainPurple * 0.7f, rot, pixel.Size() / 2f, trailScale, SpriteEffects.None, 0f);
            
            // Bloom core — GazeGreen
            sb.Draw(bloom, drawPos, null, WatchingUtils.GazeGreen * 0.6f, 0f, bloomOrigin, 0.25f, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.SpectralMint * 0.3f, 0f, bloomOrigin, 0.1f, SpriteEffects.None, 0f);
            
            // Draw glyph on top with additive glow
            Texture2D glyphTex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 glyphOrigin = glyphTex.Size() / 2f;
            sb.Draw(glyphTex, drawPos, null, WatchingUtils.PhantomWhite * 0.8f, Projectile.rotation, glyphOrigin, Projectile.scale * 0.8f, SpriteEffects.None, 0f);
            
            WatchingUtils.ExitShaderRegion(sb);
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.2f);
            
            // Trail particles — every frame
            WatchingParticleHandler.Spawn(new PhantomBoltTrailMote(
                Projectile.Center + Main.rand.NextVector2Circular(2f, 2f),
                -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                Color.Lerp(WatchingUtils.GazeGreen, WatchingUtils.SpectralMint, Main.rand.NextFloat()),
                Main.rand.NextFloat(0.1f, 0.15f),
                15));
            
            if (Main.GameUpdateCount % 3 == 0)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<WatchingPhantomDust>(), Projectile.velocity.X * -0.15f, Projectile.velocity.Y * -0.15f);
            }
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            // Impact VFX
            WatchingParticleHandler.Spawn(new MysteryZoneRipple(
                target.Center,
                WatchingUtils.RefrainPurple,
                40f,
                25));
            int burstCount = Main.rand.Next(3, 6);
            for (int i = 0; i < burstCount; i++)
            {
                WatchingParticleHandler.Spawn(new PhantomBoltTrailMote(
                    target.Center,
                    Main.rand.NextVector2CircularEdge(3f, 3f),
                    WatchingUtils.GazeGreen,
                    Main.rand.NextFloat(0.1f, 0.2f),
                    18));
            }
            WatchingParticleHandler.Spawn(new WatchingEyeParticle(
                target.Center + new Vector2(0, -24f),
                new Vector2(0, -0.3f),
                WatchingUtils.GazeGreen,
                0.2f,
                25));
            
            // Chain damage to one nearby enemy
            if (!hitEnemies.Contains(target.whoAmI))
            {
                hitEnemies.Add(target.whoAmI);
                
                float chainRange = 200f;
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.friendly || npc.whoAmI == target.whoAmI) continue;
                    if (Vector2.Distance(npc.Center, target.Center) > chainRange) continue;
                    
                    npc.SimpleStrikeNPC((int)(Projectile.damage * 0.3f), 0, false, 0f, null, false, 0f, true);
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 180);
                    break;
                }
            }
            
            // Spawn a rift at the impact point
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                ModContent.ProjectileType<PhantomRift>(),
                (int)(Projectile.damage * 0.4f), 0f, Projectile.owner);
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.5f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Death burst VFX
            int moteCount = Main.rand.Next(4, 7);
            for (int i = 0; i < moteCount; i++)
            {
                WatchingParticleHandler.Spawn(new PhantomBoltTrailMote(
                    Projectile.Center,
                    Main.rand.NextVector2CircularEdge(2.5f, 2.5f),
                    WatchingUtils.SpectralMint,
                    Main.rand.NextFloat(0.1f, 0.18f),
                    20));
            }
            WatchingParticleHandler.Spawn(new PhantomWispParticle(
                Projectile.Center,
                6f,
                Main.rand.NextFloat(MathHelper.TwoPi),
                WatchingUtils.RefrainPurple,
                0.4f,
                45));
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<WatchingPhantomDust>(), Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
            }
        }
    }
    
    public class PhantomRift : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D bloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 bloomOrigin = bloom.Size() / 2f;
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 0.8f + 0.2f * (float)Math.Sin(time * 4f);
            float antiPulse = 0.8f + 0.2f * (float)Math.Sin(time * 4f + MathHelper.Pi);
            
            // Outer swirling glow — RefrainPurple, pulsing
            sb.Draw(bloom, drawPos, null, WatchingUtils.RefrainPurple * (0.35f * pulse * Projectile.Opacity), time * 0.5f, bloomOrigin, 0.8f * pulse, SpriteEffects.None, 0f);
            
            // Inner rift core — GazeGreen, opposite phase
            sb.Draw(bloom, drawPos, null, WatchingUtils.GazeGreen * (0.5f * antiPulse * Projectile.Opacity), -time * 0.3f, bloomOrigin, 0.4f * antiPulse, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.SpectralMint * (0.2f * Projectile.Opacity), 0f, bloomOrigin, 0.2f, SpriteEffects.None, 0f);
            
            // Cross-beams — rotating "tear in space" effect
            float beamRot = time * 0.8f;
            Vector2 beamScale1 = new Vector2(40f / pixel.Width, 2f / pixel.Height);
            Vector2 beamScale2 = new Vector2(40f / pixel.Width, 2f / pixel.Height);
            sb.Draw(pixel, drawPos, null, WatchingUtils.PhantomWhite * (0.4f * Projectile.Opacity), beamRot, pixel.Size() / 2f, beamScale1, SpriteEffects.None, 0f);
            sb.Draw(pixel, drawPos, null, WatchingUtils.PhantomWhite * (0.4f * Projectile.Opacity), beamRot + MathHelper.PiOver2, pixel.Size() / 2f, beamScale2, SpriteEffects.None, 0f);
            
            // Draw rift texture with additive blend
            Texture2D riftTex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 riftOrigin = riftTex.Size() / 2f;
            sb.Draw(riftTex, drawPos, null, WatchingUtils.RefrainPurple * (0.5f * Projectile.Opacity), time * 0.4f, riftOrigin, Projectile.scale * 0.6f, SpriteEffects.None, 0f);
            
            WatchingUtils.ExitShaderRegion(sb);
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 60f);
            float opacity = (float)Math.Sin(lifeProgress * MathHelper.Pi);
            Projectile.Opacity = opacity;
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * opacity * 0.3f);
            
            // Ambient rift particles
            if (Main.GameUpdateCount % 2 == 0)
            {
                WatchingParticleHandler.Spawn(new PhantomWispParticle(
                    Projectile.Center,
                    20f,
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    WatchingUtils.RefrainPurple,
                    Main.rand.NextFloat(0.15f, 0.25f),
                    30));
            }
            if (Main.GameUpdateCount % 6 == 0)
            {
                WatchingParticleHandler.Spawn(new WatchingEyeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(25f, 25f),
                    Vector2.Zero,
                    WatchingUtils.GazeGreen,
                    0.15f,
                    20));
            }
            if (Main.GameUpdateCount % 4 == 0)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<WatchingPhantomDust>(), 0f, -0.3f);
            }
            if (Main.GameUpdateCount % 10 == 0)
            {
                WatchingParticleHandler.Spawn(new MysteryZoneRipple(
                    Projectile.Center,
                    WatchingUtils.RefrainPurple * 0.6f,
                    40f,
                    30));
            }
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 240);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.3f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Rift collapse VFX
            WatchingParticleHandler.Spawn(new MysteryZoneRipple(
                Projectile.Center,
                WatchingUtils.PhantomWhite,
                60f,
                35));
            int wispCount = Main.rand.Next(6, 9);
            for (int i = 0; i < wispCount; i++)
            {
                WatchingParticleHandler.Spawn(new PhantomWispParticle(
                    Projectile.Center,
                    15f,
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    WatchingUtils.RefrainPurple,
                    Main.rand.NextFloat(0.2f, 0.35f),
                    35));
            }
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<WatchingPhantomDust>(), Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f));
            }
        }
    }
    
    public class MysteryZone : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private int damageTimer = 0;
        
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D bloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 bloomOrigin = bloom.Size() / 2f;
            
            // === Shader overlay: Panopticon surveillance grid ===
            {
                float lifeFadeShader = MathHelper.Clamp(Projectile.timeLeft / 60f, 0f, 1f);
                float zoneScaleShader = Projectile.width / 64f;
                EnigmaShaderHelper.DrawShaderOverlay(sb, ShaderLoader.WatchingMysteryZone,
                    bloom, drawPos, bloomOrigin, zoneScaleShader * 2.5f,
                    WatchingUtils.GazeGreen.ToVector3(), WatchingUtils.RefrainPurple.ToVector3(),
                    opacity: 0.4f * lifeFadeShader, intensity: 1.0f,
                    noiseTexture: ShaderLoader.GetNoiseTexture("VoronoiNoise"),
                    techniqueName: "WatchingMysteryField");
            }
            
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            float time = Main.GlobalTimeWrappedHourly;
            float zoneScale = Projectile.width / 64f; // scale bloom to match zone hitbox
            float lifeFade = MathHelper.Clamp(Projectile.timeLeft / 60f, 0f, 1f); // fade out in last second
            
            // Large soft mist circle — RefrainPurple, low opacity
            float slowRot = time * 0.5f;
            sb.Draw(bloom, drawPos, null, WatchingUtils.RefrainPurple * (0.18f * lifeFade), slowRot, bloomOrigin, zoneScale * 1.8f, SpriteEffects.None, 0f);
            
            // Concentric inner ring — GazeGreen, slightly brighter
            float innerPulse = 0.9f + 0.1f * (float)Math.Sin(time * 3f);
            sb.Draw(bloom, drawPos, null, WatchingUtils.GazeGreen * (0.25f * lifeFade * innerPulse), -slowRot * 0.7f, bloomOrigin, zoneScale * 1.2f, SpriteEffects.None, 0f);
            
            // Subtle core highlight
            sb.Draw(bloom, drawPos, null, WatchingUtils.SpectralMint * (0.12f * lifeFade), 0f, bloomOrigin, zoneScale * 0.5f, SpriteEffects.None, 0f);
            
            WatchingUtils.ExitShaderRegion(sb);
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override bool? CanDamage() => false;
        
        public override void AI()
        {
            damageTimer++;
            if (damageTimer >= 20)
            {
                damageTimer = 0;
                DealZoneDamage();
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.2f);
            
            float zoneRadius = Projectile.width / 2f;
            
            // Ambient zone particles
            if (Main.GameUpdateCount % 3 == 0)
            {
                Vector2 randomPos = Projectile.Center + Main.rand.NextVector2Circular(zoneRadius * 0.8f, zoneRadius * 0.8f);
                WatchingParticleHandler.Spawn(new MysteryZoneRipple(
                    randomPos,
                    WatchingUtils.RefrainPurple * 0.5f,
                    Main.rand.NextFloat(30f, 50f),
                    25));
            }
            if (Main.GameUpdateCount % 6 == 0)
            {
                Vector2 driftPos = Projectile.Center + Main.rand.NextVector2Circular(zoneRadius * 0.6f, zoneRadius * 0.6f);
                WatchingParticleHandler.Spawn(new PhantomWispParticle(
                    driftPos,
                    10f,
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    WatchingUtils.GazeGreen,
                    Main.rand.NextFloat(0.15f, 0.25f),
                    30));
            }
            if (Main.GameUpdateCount % 8 == 0)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(zoneRadius * 0.7f, zoneRadius * 0.7f);
                Dust.NewDust(dustPos - new Vector2(4f), 8, 8,
                    ModContent.DustType<WatchingPhantomDust>(), 0f, -0.2f);
            }
        }
        
        private void DealZoneDamage()
        {
            float zoneRadius = Projectile.width / 2f;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist > zoneRadius) continue;
                
                // Damage
                npc.SimpleStrikeNPC(Projectile.damage, 0, false, 0f, null, false, 0f, true);
                npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 180);
                npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 1);
                
                // Slow effect (reduce velocity)
                npc.velocity *= 0.85f;
                
                // Pull toward center
                Vector2 pullDir = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                npc.velocity += pullDir * 1.5f;
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // Zone dissipation VFX
            WatchingParticleHandler.Spawn(new MysteryZoneRipple(
                Projectile.Center,
                WatchingUtils.PhantomWhite,
                80f,
                40));
            int wispCount = Main.rand.Next(5, 8);
            for (int i = 0; i < wispCount; i++)
            {
                WatchingParticleHandler.Spawn(new PhantomWispParticle(
                    Projectile.Center,
                    25f,
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    WatchingUtils.RefrainPurple,
                    Main.rand.NextFloat(0.2f, 0.35f),
                    40));
            }
            for (int i = 0; i < 4; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<WatchingPhantomDust>(), Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, 0f));
            }
        }
    }
}
