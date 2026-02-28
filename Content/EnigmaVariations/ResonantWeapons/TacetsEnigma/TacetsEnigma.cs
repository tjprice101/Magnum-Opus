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
using MagnumOpus.Content.EnigmaVariations.Debuffs;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TacetsEnigma.Particles;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TacetsEnigma.Dusts;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TacetsEnigma.Utilities;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TacetsEnigma
{
    /// <summary>
    /// TACET'S ENIGMA - Ranged weapon that fires enigmatic shots
    /// Every 4th shot is a "paradox shot" — larger, stronger, and triggers paradox explosions
    /// </summary>
    public class TacetsEnigma : ModItem
    {
        private int shotCounter = 0;
        
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/TacetsEnigma/TacetsEnigma";
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override void SetDefaults()
        {
            Item.damage = 265;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 28;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = SoundID.Item12;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<TacetEnigmaShot>();
            Item.shootSpeed = 18f;
            Item.noMelee = true;
            Item.useAmmo = AmmoID.Bullet;
        }
        
        public override void HoldItem(Player player)
        {
            // Point weapon toward cursor
            if (Main.myPlayer == player.whoAmI)
            {
                Vector2 toCursor = Main.MouseWorld - player.Center;
                player.ChangeDir(toCursor.X > 0 ? 1 : -1);
            }
            
            Lighting.AddLight(player.Center, EnigmaPurple.ToVector3() * 0.15f);
        }
        
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            // Keep the weapon pointed at the cursor
            if (Main.myPlayer == player.whoAmI)
            {
                float aimRotation = (Main.MouseWorld - player.Center).ToRotation();
                player.itemRotation = aimRotation;
                if (player.direction == -1)
                    player.itemRotation += MathHelper.Pi;
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect", "Fires enigmatic shots that phase through reality"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "Every 4th shot is a Paradox Shot with enhanced power"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "Paradox shots trigger chain explosions on impact"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'In silence, the greatest riddles find their voice.'")
            {
                OverrideColor = EnigmaPurple
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            bool isParadoxShot = (shotCounter % 4 == 0);
            
            if (isParadoxShot)
            {
                // Paradox shot - stronger with different projectile
                int paradoxDamage = (int)(damage * 2.5f);
                Projectile.NewProjectile(source, position, velocity, 
                    ModContent.ProjectileType<TacetParadoxBolt>(),
                    paradoxDamage, knockback * 2f, player.whoAmI);
                    
                SoundEngine.PlaySound(SoundID.Item125 with { Pitch = -0.2f, Volume = 0.7f }, position);
                
                // Paradox muzzle flash — brighter, green-white tinted
                Vector2 fireDir = velocity.SafeNormalize(Vector2.UnitX);
                for (int i = 0; i < 5; i++)
                {
                    float spread = MathHelper.ToRadians(Main.rand.NextFloat(-15f, 15f));
                    Vector2 burstVel = fireDir.RotatedBy(spread) * Main.rand.NextFloat(10f, 18f);
                    Color burstColor = Color.Lerp(TacetUtils.TacetPurple, TacetUtils.FlashWhite, Main.rand.NextFloat(0.3f, 0.7f));
                    TacetParticleHandler.Spawn(new SilenceBurstParticle(position, burstVel, burstColor, Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(8, 14)));
                }
                for (int i = 0; i < 3; i++)
                {
                    Vector2 dustVel = fireDir.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(2f, 5f);
                    Dust.NewDustPerfect(position, ModContent.DustType<TacetSilenceDust>(), dustVel, 0, default, Main.rand.NextFloat(0.5f, 1.0f));
                }
            }
            else
            {
                // Normal shot
                Projectile.NewProjectile(source, position, velocity, 
                    ModContent.ProjectileType<TacetEnigmaShot>(),
                    damage, knockback, player.whoAmI);
                
                // Normal muzzle flash — pure purple
                Vector2 fireDir = velocity.SafeNormalize(Vector2.UnitX);
                for (int i = 0; i < 3; i++)
                {
                    float spread = MathHelper.ToRadians(Main.rand.NextFloat(-15f, 15f));
                    Vector2 burstVel = fireDir.RotatedBy(spread) * Main.rand.NextFloat(8f, 14f);
                    TacetParticleHandler.Spawn(new SilenceBurstParticle(position, burstVel, TacetUtils.TacetPurple, Main.rand.NextFloat(0.2f, 0.45f), Main.rand.Next(6, 10)));
                }
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustVel = fireDir.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(1f, 3f);
                    Dust.NewDustPerfect(position, ModContent.DustType<TacetSilenceDust>(), dustVel, 0, default, Main.rand.NextFloat(0.4f, 0.8f));
                }
            }
            
            return false;
        }
    }
    
    public class TacetEnigmaShot : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/MusicNote";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Texture2D bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Texture2D glyphTex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float rotation = Projectile.velocity.ToRotation();
            
            // === Shader overlay: Crystalline fracture shard trail ===
            EnigmaShaderHelper.DrawShaderOverlay(sb, ShaderLoader.TacetBulletTrail,
                bloomTex, drawPos, bloomTex.Size() / 2f, 0.8f,
                TacetUtils.TacetPurple.ToVector3(), TacetUtils.ParadoxGreen.ToVector3(),
                opacity: 0.5f, intensity: 1.0f, rotation: rotation,
                noiseTexture: ShaderLoader.GetNoiseTexture("VoronoiNoise"),
                techniqueName: "TacetBulletFlow");
            
            TacetUtils.EnterAdditiveShaderRegion(sb);
            
            // Glow trail — narrow MagicPixel bar behind the projectile
            sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), TacetUtils.TacetPurple * 0.6f,
                rotation, new Vector2(0.5f, 0.5f), new Vector2(20f, 6f), SpriteEffects.None, 0f);
            
            // Bloom core at center
            Color coreColor = Color.Lerp(TacetUtils.TacetPurple, TacetUtils.ParadoxGreen, 0.25f);
            sb.Draw(bloomTex, drawPos, null, coreColor * 0.7f, 0f,
                bloomTex.Size() / 2f, 0.2f, SpriteEffects.None, 0f);
            
            // Glyph sprite faintly on top
            sb.Draw(glyphTex, drawPos, null, TacetUtils.TacetPurple * 0.35f, rotation,
                glyphTex.Size() / 2f, 0.5f, SpriteEffects.None, 0f);
            
            TacetUtils.ExitShaderRegion(sb);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.3f);
            
            int timer = (int)Projectile.ai[0]++;
            
            // Trail motes every 2 frames
            if (timer % 2 == 0)
            {
                Vector2 trailVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f);
                TacetParticleHandler.Spawn(new ChainLightningMoteParticle(
                    Projectile.Center, trailVel, TacetUtils.TacetPurple, Main.rand.NextFloat(0.15f, 0.25f), 12));
            }
            
            // Silence dust every 3 frames
            if (timer % 3 == 0)
            {
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<TacetSilenceDust>(),
                    Projectile.velocity * -0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f), 0, default, Main.rand.NextFloat(0.3f, 0.6f));
            }
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            // Check for paradox explosion on stacked targets
            if (target.GetGlobalNPC<ParadoxBrandNPC>().GetParadoxStacks() >= 10)
            {
                TriggerParadoxExplosion(target);
            }
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.4f);
        }
        
        private void TriggerParadoxExplosion(NPC target)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f, Volume = 0.6f }, target.Center);
            
            // Reset stacks
            target.GetGlobalNPC<ParadoxBrandNPC>().ResetParadoxStacks(target);
            
            // Damage nearby enemies
            float explosionRadius = 200f;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                if (Vector2.Distance(npc.Center, target.Center) > explosionRadius) continue;
                
                npc.SimpleStrikeNPC((int)(Projectile.damage * 0.5f), 0, false, 0f, null, false, 0f, true);
                npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 240);
                npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 2);
            }
            
            // Paradox explosion VFX
            for (int i = 0; i < Main.rand.Next(20, 31); i++)
            {
                Vector2 burstVel = Main.rand.NextVector2CircularEdge(8f, 8f) * Main.rand.NextFloat(0.5f, 1.5f);
                Color burstColor = Color.Lerp(TacetUtils.ParadoxGreen, TacetUtils.TacetPurple, Main.rand.NextFloat());
                TacetParticleHandler.Spawn(new SilenceBurstParticle(
                    target.Center, burstVel, burstColor, Main.rand.NextFloat(0.3f, 0.7f), Main.rand.Next(10, 18)));
            }
            for (int i = 0; i < Main.rand.Next(3, 6); i++)
            {
                Vector2 expandVel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                TacetParticleHandler.Spawn(new ParadoxBoltGlowParticle(
                    target.Center, expandVel, TacetUtils.ParadoxGreen, Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(20, 35)));
            }
            for (int i = 0; i < Main.rand.Next(8, 13); i++)
            {
                Dust.NewDustPerfect(target.Center, ModContent.DustType<TacetSilenceDust>(),
                    Main.rand.NextVector2Circular(5f, 5f), 0, default, Main.rand.NextFloat(0.5f, 1.0f));
            }
            // Central paradox burst symbol
            TacetParticleHandler.Spawn(new ParadoxStackParticle(
                target.Center, 0f, 0f, TacetUtils.UnstableLime, 0.8f, 30));
        }
        
        public override void OnKill(int timeLeft)
        {
            // Death burst — silence shards scatter
            for (int i = 0; i < Main.rand.Next(4, 7); i++)
            {
                Vector2 burstVel = Main.rand.NextVector2CircularEdge(4f, 4f) * Main.rand.NextFloat(0.6f, 1.2f);
                TacetParticleHandler.Spawn(new SilenceBurstParticle(
                    Projectile.Center, burstVel, TacetUtils.TacetPurple, Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(8, 14)));
            }
            for (int i = 0; i < Main.rand.Next(2, 4); i++)
            {
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<TacetSilenceDust>(),
                    Main.rand.NextVector2Circular(2f, 2f), 0, default, Main.rand.NextFloat(0.4f, 0.7f));
            }
        }
    }
    
    public class TacetParadoxBolt : ModProjectile
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private List<int> hitEnemies = new List<int>();
        
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Texture2D bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Texture2D glyphTex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float rotation = Projectile.velocity.ToRotation();
            float spinRotation = Projectile.rotation; // spinning glyph
            
            // === Shader overlay: Multi-ring moiré paradox cascade ===
            EnigmaShaderHelper.DrawShaderOverlay(sb, ShaderLoader.TacetParadoxExplosion,
                bloomTex, drawPos, bloomTex.Size() / 2f, 1.5f,
                TacetUtils.TacetPurple.ToVector3(), TacetUtils.UnstableLime.ToVector3(),
                opacity: 0.55f, intensity: 1.2f,
                noiseTexture: ShaderLoader.GetNoiseTexture("TileableFBMNoise"),
                techniqueName: "TacetParadoxBlast");
            
            TacetUtils.EnterAdditiveShaderRegion(sb);
            
            // Wide glow trail — MagicPixel bar, green-purple gradient
            Color trailColor = Color.Lerp(TacetUtils.ParadoxGreen, TacetUtils.TacetPurple, 0.4f);
            sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), trailColor * 0.7f,
                rotation, new Vector2(0.5f, 0.5f), new Vector2(30f, 12f), SpriteEffects.None, 0f);
            
            // Secondary outer glow — large, low opacity, purple
            sb.Draw(bloomTex, drawPos, null, TacetUtils.TacetPurple * 0.2f, 0f,
                bloomTex.Size() / 2f, 0.7f, SpriteEffects.None, 0f);
            
            // Large bloom core — bright unstable lime/green
            Color coreColor = Color.Lerp(TacetUtils.UnstableLime, TacetUtils.ParadoxGreen, 0.5f);
            sb.Draw(bloomTex, drawPos, null, coreColor * 0.8f, 0f,
                bloomTex.Size() / 2f, 0.4f, SpriteEffects.None, 0f);
            
            // Spinning glyph on top
            sb.Draw(glyphTex, drawPos, null, TacetUtils.ParadoxGreen * 0.5f, spinRotation,
                glyphTex.Size() / 2f, 0.7f, SpriteEffects.None, 0f);
            
            TacetUtils.ExitShaderRegion(sb);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation += 0.15f;
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.5f);
            
            int timer = (int)Projectile.ai[0]++;
            
            // Afterimage glow every frame
            TacetParticleHandler.Spawn(new ParadoxBoltGlowParticle(
                Projectile.Center, Vector2.Zero, TacetUtils.ParadoxGreen, 0.18f, 8));
            
            // Trail motes every 2 frames
            if (timer % 2 == 0)
            {
                Vector2 trailVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.8f, 2f);
                TacetParticleHandler.Spawn(new ChainLightningMoteParticle(
                    Projectile.Center, trailVel, TacetUtils.ParadoxGreen, Main.rand.NextFloat(0.15f, 0.3f), 12));
            }
            
            // Silence dust every 3 frames
            if (timer % 3 == 0)
            {
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<TacetSilenceDust>(),
                    Projectile.velocity * -0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f), 0, default, Main.rand.NextFloat(0.4f, 0.7f));
            }
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 360);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 3);
            
            // Chain damage to nearby enemy not yet hit
            if (!hitEnemies.Contains(target.whoAmI))
            {
                hitEnemies.Add(target.whoAmI);
                
                float chainRange = 250f;
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.friendly || npc.whoAmI == target.whoAmI) continue;
                    if (hitEnemies.Contains(npc.whoAmI)) continue;
                    if (Vector2.Distance(npc.Center, target.Center) > chainRange) continue;
                    
                    npc.SimpleStrikeNPC((int)(Projectile.damage * 0.3f), 0, false, 0f, null, false, 0f, true);
                    
                    // Chain lightning VFX — motes along the line between target and chained enemy
                    int moteCount = Main.rand.Next(5, 9);
                    for (int m = 0; m < moteCount; m++)
                    {
                        float lerp = (float)m / moteCount;
                        Vector2 motePos = Vector2.Lerp(target.Center, npc.Center, lerp) + Main.rand.NextVector2Circular(6f, 6f);
                        Vector2 moteVel = Main.rand.NextVector2Circular(1f, 1f);
                        Color moteColor = Color.Lerp(TacetUtils.FlashWhite, TacetUtils.ParadoxGreen, Main.rand.NextFloat(0.3f, 0.7f));
                        TacetParticleHandler.Spawn(new ChainLightningMoteParticle(
                            motePos, moteVel, moteColor, Main.rand.NextFloat(0.2f, 0.4f), 12));
                    }
                    
                    break;
                }
            }
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.6f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Death burst — paradox collapse
            for (int i = 0; i < Main.rand.Next(8, 13); i++)
            {
                Vector2 burstVel = Main.rand.NextVector2CircularEdge(6f, 6f) * Main.rand.NextFloat(0.5f, 1.3f);
                Color burstColor = Color.Lerp(TacetUtils.ParadoxGreen, TacetUtils.TacetPurple, Main.rand.NextFloat());
                TacetParticleHandler.Spawn(new SilenceBurstParticle(
                    Projectile.Center, burstVel, burstColor, Main.rand.NextFloat(0.25f, 0.5f), Main.rand.Next(10, 16)));
            }
            // Fading paradox glow
            TacetParticleHandler.Spawn(new ParadoxBoltGlowParticle(
                Projectile.Center, Vector2.Zero, TacetUtils.ParadoxGreen, 0.5f, 25));
            for (int i = 0; i < Main.rand.Next(4, 6); i++)
            {
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<TacetSilenceDust>(),
                    Main.rand.NextVector2Circular(3f, 3f), 0, default, Main.rand.NextFloat(0.5f, 0.9f));
            }
        }
    }
}
