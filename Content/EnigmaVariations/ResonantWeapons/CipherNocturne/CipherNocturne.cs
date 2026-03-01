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
using ReLogic.Content;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.EnigmaVariations.Debuffs;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.CipherNocturne.Particles;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.CipherNocturne.Dusts;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.CipherNocturne.Utilities;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.CipherNocturne
{
    /// <summary>
    /// CIPHER NOCTURNE - Magic beam weapon that channels mysterious arcane energy
    /// Creates visual distortion effects and increasing damage over beam duration
    /// When beam ends, all damage areas "snap back" with a burst
    /// </summary>
    public class CipherNocturne : ModItem
    {
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/CipherNocturne/CipherNocturne";

        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override void SetDefaults()
        {
            Item.damage = 290;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 6;
            Item.width = 28;
            Item.height = 28;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 1f;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<RealityUnravelerBeam>();
            Item.shootSpeed = 1f;
            Item.noMelee = true;
            Item.channel = true;
            Item.staff[Item.type] = true;
        }
        
        public override void HoldItem(Player player)
        {
            // Rotate the weapon toward the cursor while holding
            if (Main.myPlayer == player.whoAmI)
            {
                Vector2 toCursor = Main.MouseWorld - player.Center;
                player.itemRotation = toCursor.ToRotation();
                if (player.direction == -1)
                    player.itemRotation += MathHelper.Pi;
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect", "Hold to channel a mysterious beam of arcane energy"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "The beam warps and distorts the space around it"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "Damage increases the longer beam is held on target"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect4", "Releasing the beam causes all affected areas to snap back"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'Pull at a thread of existence, and watch it come undone.'") 
            { 
                OverrideColor = EnigmaPurple 
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int beamCount = player.ownedProjectileCounts[type];
            if (beamCount == 0)
            {
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            }
            return false;
        }
    }
    
    public class RealityUnravelerBeam : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private const float MaxBeamLength = 800f;
        private int channelTime = 0;
        private List<Vector2> unravelPoints = new List<Vector2>();
        
        // Instance fields for rendering access
        private Vector2 currentBeamEnd;
        private float currentBeamLength;
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/MusicNote";
        
        public override bool PreDraw(ref Color lightColor)
        {
            if (channelTime < 2) return false;
            
            Player owner = Main.player[Projectile.owner];
            SpriteBatch sb = Main.spriteBatch;
            
            Vector2 start = owner.Center - Main.screenPosition;
            Vector2 end = currentBeamEnd - Main.screenPosition;
            Vector2 beamDir = (currentBeamEnd - owner.Center).SafeNormalize(Vector2.UnitX);
            float rotation = beamDir.ToRotation();
            float length = currentBeamLength;
            
            // Pulsing factor
            float pulse = 1f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.1f);
            float channelFactor = Math.Min(channelTime / 60f, 1f);
            
            // Width scales with channel time
            float baseWidth = 4f + channelFactor * 16f; // 4 → 20px
            
            // Pixel texture for line drawing
            Texture2D pixel = MagnumTextureRegistry.GetSoftGlow();
            if (pixel == null) return false;
            
            // === Shader overlay: Digital data-stream cipher beam ===
            {
                Texture2D shBloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
                EnigmaShaderHelper.DrawShaderOverlay(sb, ShaderLoader.CipherBeamTrail,
                    shBloom, start + (end - start) * 0.5f, shBloom.Size() / 2f,
                    MathHelper.Clamp(length / 64f, 1f, 12f),
                    CipherUtils.ArcaneViolet.ToVector3(), CipherUtils.UnravelGreen.ToVector3(),
                    opacity: 0.55f * channelFactor, intensity: 1.2f, rotation: rotation,
                    noiseTexture: ShaderLoader.GetNoiseTexture("VoronoiNoise"),
                    techniqueName: "CipherBeamFlow");
            }
            
            CipherUtils.EnterAdditiveShaderRegion(sb);
            
            // Layer 1: Wide soft purple outer glow
            float outerWidth = baseWidth * 3f * pulse;
            sb.Draw(pixel, start + (end - start) * 0.5f, new Rectangle(0, 0, 1, 1), CipherUtils.ArcaneViolet * 0.25f * channelFactor,
                rotation, new Vector2(0.5f, 0.5f), new Vector2(length, outerWidth), SpriteEffects.None, 0f);
            
            // Layer 2: Medium green core glow
            float midWidth = baseWidth * 1.5f * pulse;
            sb.Draw(pixel, start + (end - start) * 0.5f, new Rectangle(0, 0, 1, 1), CipherUtils.UnravelGreen * 0.5f * channelFactor,
                rotation, new Vector2(0.5f, 0.5f), new Vector2(length, midWidth), SpriteEffects.None, 0f);
            
            // Layer 3: Narrow bright white center
            float innerWidth = baseWidth * 0.5f * pulse;
            sb.Draw(pixel, start + (end - start) * 0.5f, new Rectangle(0, 0, 1, 1), CipherUtils.WhiteRevelation * 0.7f * channelFactor,
                rotation, new Vector2(0.5f, 0.5f), new Vector2(length, innerWidth), SpriteEffects.None, 0f);
            
            // Layer 4: Bloom sprite at beam end
            Texture2D bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            float bloomScale = (0.3f + channelFactor * 0.5f) * pulse;
            sb.Draw(bloomTex, end, null, CipherUtils.UnravelGreen * 0.6f * channelFactor, 0f,
                bloomTex.Size() / 2f, bloomScale, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, end, null, CipherUtils.CipherBright * 0.3f * channelFactor, 0f,
                bloomTex.Size() / 2f, bloomScale * 0.5f, SpriteEffects.None, 0f);
            
            // Layer 5: Bloom at beam origin
            sb.Draw(bloomTex, start, null, CipherUtils.ArcaneViolet * 0.4f * channelFactor, 0f,
                bloomTex.Size() / 2f, bloomScale * 0.6f, SpriteEffects.None, 0f);
            
            CipherUtils.ExitShaderRegion(sb);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Check if still channeling
            if (!owner.channel || owner.dead || !owner.active)
            {
                TriggerSnapBack();
                Projectile.Kill();
                return;
            }
            
            // Mana drain
            if (channelTime % 10 == 0 && channelTime > 0)
            {
                if (owner.CheckMana(owner.HeldItem.mana, true))
                {
                    owner.manaRegenDelay = (int)owner.maxRegenDelay;
                }
                else
                {
                    TriggerSnapBack();
                    Projectile.Kill();
                    return;
                }
            }
            
            channelTime++;
            
            // Position at player
            Projectile.Center = owner.Center;
            
            // Aim toward cursor
            Vector2 toMouse = (Main.MouseWorld - owner.Center).SafeNormalize(Vector2.UnitX);
            Projectile.velocity = toMouse;
            Projectile.rotation = toMouse.ToRotation();
            
            // Calculate beam length (raycast for tiles)
            float beamLength = MaxBeamLength;
            for (int i = 0; i < (int)(MaxBeamLength / 16f); i++)
            {
                Vector2 checkPos = owner.Center + toMouse * (i * 16f);
                Point tilePos = checkPos.ToTileCoordinates();
                if (WorldGen.InWorld(tilePos.X, tilePos.Y))
                {
                    Tile tile = Main.tile[tilePos.X, tilePos.Y];
                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        beamLength = i * 16f;
                        break;
                    }
                }
            }
            
            // Store beam end and length for PreDraw rendering
            currentBeamLength = beamLength;
            Vector2 beamEnd = owner.Center + toMouse * beamLength;
            currentBeamEnd = beamEnd;
            
            // Record unravel points for snap-back
            if (channelTime % 15 == 0)
            {
                unravelPoints.Add(beamEnd);
                if (unravelPoints.Count > 20) unravelPoints.RemoveAt(0);
            }
            
            // Deal damage along the beam
            DealBeamDamage(owner.Center, beamEnd, toMouse, beamLength);
            
            // Beam sound
            if (channelTime % 20 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.3f + channelTime * 0.002f, Volume = 0.3f }, owner.Center);
            }
            
            // Keep player facing the right direction
            owner.ChangeDir(toMouse.X > 0 ? 1 : -1);
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            
            Lighting.AddLight(beamEnd, EnigmaGreen.ToVector3() * 0.5f);
            Lighting.AddLight(owner.Center, EnigmaPurple.ToVector3() * 0.3f);
            
            // === VFX: Particle spawning along beam ===
            
            // Update CipherPlayer state
            CipherPlayer cp = owner.GetModPlayer<CipherPlayer>();
            cp.ChannelTime = channelTime;
            cp.UnravelIntensity = Math.Min(channelTime / 120f, 1f);
            
            Vector2 perpDir = new Vector2(-toMouse.Y, toMouse.X);
            
            // Every 3 frames: 1-2 UnravelMoteParticle along beam
            if (channelTime % 3 == 0)
            {
                int moteCount = Main.rand.Next(1, 3);
                for (int i = 0; i < moteCount; i++)
                {
                    float t = Main.rand.NextFloat();
                    Vector2 pos = owner.Center + toMouse * beamLength * t;
                    pos += perpDir * Main.rand.NextFloat(-12f, 12f);
                    Vector2 vel = perpDir * Main.rand.NextFloat(-0.8f, 0.8f);
                    Color col = Color.Lerp(CipherUtils.UnravelGreen, CipherUtils.ArcaneViolet, Main.rand.NextFloat());
                    float scale = Main.rand.NextFloat(0.3f, 0.6f);
                    CipherParticleHandler.Spawn(new UnravelMoteParticle(pos, vel, col, scale, Main.rand.Next(20, 40)));
                }
            }
            
            // Every 5 frames: 1 CipherGlyphParticle near beam end
            if (channelTime % 5 == 0)
            {
                float orbitRadius = Main.rand.NextFloat(15f, 35f);
                float startAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Color glyphCol = Color.Lerp(CipherUtils.DeepEnigma, CipherUtils.ArcaneViolet, Main.rand.NextFloat());
                CipherParticleHandler.Spawn(new CipherGlyphParticle(beamEnd, orbitRadius, startAngle, glyphCol, Main.rand.NextFloat(0.4f, 0.7f), Main.rand.Next(30, 50)));
            }
            
            // Every 10 frames: BeamCorePulseParticle at beam origin
            if (channelTime % 10 == 0)
            {
                CipherParticleHandler.Spawn(new BeamCorePulseParticle(owner.Center, CipherUtils.ArcaneViolet, 0.8f + Math.Min(channelTime / 120f, 1f) * 0.4f, 15));
            }
            
            // Every frame: 1 CipherVoidDust at random beam position
            {
                float dt = Main.rand.NextFloat();
                Vector2 dustPos = owner.Center + toMouse * beamLength * dt;
                dustPos += perpDir * Main.rand.NextFloat(-8f, 8f);
                Dust.NewDust(dustPos, 0, 0, ModContent.DustType<CipherVoidDust>(),
                    perpDir.X * Main.rand.NextFloat(-0.5f, 0.5f), perpDir.Y * Main.rand.NextFloat(-0.5f, 0.5f),
                    0, default, Main.rand.NextFloat(0.5f, 1f));
            }
        }
        
        private void DealBeamDamage(Vector2 start, Vector2 end, Vector2 direction, float beamLength)
        {
            float damageMultiplier = 1f + Math.Min(channelTime / 120f, 2f); // Ramps up over 2 seconds
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                
                float distToLine = DistancePointToLine(npc.Center, start, end);
                if (distToLine > npc.width / 2f + 20f) continue;
                
                float projectionLength = Vector2.Dot(npc.Center - start, direction);
                if (projectionLength < 0 || projectionLength > beamLength) continue;
                
                // Deal periodic damage
                if (channelTime % 6 == 0)
                {
                    int damage = (int)(Projectile.damage * damageMultiplier);
                    npc.SimpleStrikeNPC(damage, 0, false, 0f, null, false, 0f, true);
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
                    npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 1);
                }
            }
        }
        
        private float DistancePointToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            float lineLength = line.Length();
            if (lineLength < 0.001f) return Vector2.Distance(point, lineStart);
            
            Vector2 lineDir = line / lineLength;
            Vector2 toPoint = point - lineStart;
            float projection = Vector2.Dot(toPoint, lineDir);
            projection = MathHelper.Clamp(projection, 0f, lineLength);
            
            Vector2 closestPoint = lineStart + lineDir * projection;
            return Vector2.Distance(point, closestPoint);
        }
        
        private void TriggerSnapBack()
        {
            if (channelTime < 10) return;
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 0.8f }, Projectile.Center);
            
            // Spawn snap-back projectiles at unravel points
            foreach (Vector2 point in unravelPoints)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), point, Vector2.Zero,
                    ModContent.ProjectileType<RealitySnapBack>(),
                    (int)(Projectile.damage * 1.5f), 8f, Projectile.owner);
            }
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => false;
    }
    
    public class RealitySnapBack : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            
            // Progress: 1 at spawn → 0 at death
            float progress = Projectile.timeLeft / 15f;
            
            Texture2D bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = bloomTex.Size() / 2f;
            
            // === Shader overlay: Clock-face sector starburst ===
            EnigmaShaderHelper.DrawShaderOverlay(sb, ShaderLoader.CipherSnapBack,
                bloomTex, drawPos, origin, 1.5f + (1f - progress) * 2f,
                CipherUtils.ArcaneViolet.ToVector3(), CipherUtils.CipherBright.ToVector3(),
                opacity: progress * 0.7f, intensity: 1.5f,
                noiseTexture: ShaderLoader.GetNoiseTexture("SparklyNoiseTexture"),
                techniqueName: "CipherSnapBackMain");
            
            CipherUtils.EnterAdditiveShaderRegion(sb);
            
            // Expanding ring (starts small, grows, fades)
            float ringScale = (1f - progress) * 2.5f + 0.3f;
            float ringAlpha = progress * 0.8f;
            sb.Draw(bloomTex, drawPos, null, CipherUtils.ArcaneViolet * ringAlpha * 0.5f, 0f,
                origin, ringScale, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, drawPos, null, CipherUtils.UnravelGreen * ringAlpha * 0.3f, 0f,
                origin, ringScale * 1.3f, SpriteEffects.None, 0f);
            
            // Shrinking bright core flash
            float coreScale = progress * 1.2f;
            float coreAlpha = progress;
            sb.Draw(bloomTex, drawPos, null, CipherUtils.WhiteRevelation * coreAlpha * 0.9f, 0f,
                origin, coreScale * 0.4f, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, drawPos, null, CipherUtils.CipherBright * coreAlpha * 0.6f, 0f,
                origin, coreScale * 0.7f, SpriteEffects.None, 0f);
            
            CipherUtils.ExitShaderRegion(sb);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 15;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        
        public override void AI()
        {
            float progress = Projectile.timeLeft / 15f;
            float lightIntensity = 0.6f + (1f - progress) * 0.6f;
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * lightIntensity);
            
            // Every frame: 2-3 SnapBackSparkParticle in random outward directions
            int sparkCount = Main.rand.Next(2, 4);
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(4f, 10f);
                Color col = Color.Lerp(CipherUtils.UnravelGreen, CipherUtils.CipherBright, Main.rand.NextFloat());
                CipherParticleHandler.Spawn(new SnapBackSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    vel, col, Main.rand.NextFloat(0.3f, 0.7f), Main.rand.Next(8, 16)));
            }
            
            // Every 5 frames: 1 VoidDistortionRingParticle
            if (Projectile.timeLeft % 5 == 0)
            {
                CipherParticleHandler.Spawn(new VoidDistortionRingParticle(
                    Projectile.Center, CipherUtils.ArcaneViolet * 0.8f,
                    0.5f + (1f - progress) * 0.8f, 20));
            }
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 360);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 3);
            
            SeekingCrystalHelper.SpawnEnigmaCrystals(
                Projectile.GetSource_FromThis(),
                target.Center,
                Projectile.velocity,
                (int)(damageDone * 0.25f),
                5f,
                Projectile.owner,
                3
            );
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Death burst: 10-15 SnapBackSparkParticle in all directions
            int burstCount = Main.rand.Next(10, 16);
            for (int i = 0; i < burstCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 14f);
                Color col = Color.Lerp(CipherUtils.UnravelGreen, CipherUtils.WhiteRevelation, Main.rand.NextFloat());
                CipherParticleHandler.Spawn(new SnapBackSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    vel, col, Main.rand.NextFloat(0.4f, 0.9f), Main.rand.Next(12, 25)));
            }
            
            // 1 large VoidDistortionRingParticle
            CipherParticleHandler.Spawn(new VoidDistortionRingParticle(
                Projectile.Center, CipherUtils.ArcaneViolet,
                1.5f, 30));
            
            // 3-4 UnravelMoteParticle drifting outward
            int moteCount = Main.rand.Next(3, 5);
            for (int i = 0; i < moteCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1f, 3f);
                Color col = Color.Lerp(CipherUtils.DeepEnigma, CipherUtils.UnravelGreen, Main.rand.NextFloat());
                CipherParticleHandler.Spawn(new UnravelMoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    vel, col, Main.rand.NextFloat(0.4f, 0.7f), Main.rand.Next(25, 45)));
            }
            
            // 5 CipherVoidDust
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1f, 4f);
                Dust.NewDust(Projectile.Center, 0, 0, ModContent.DustType<CipherVoidDust>(),
                    vel.X, vel.Y, 0, default, Main.rand.NextFloat(0.6f, 1.2f));
            }
        }
    }
}
