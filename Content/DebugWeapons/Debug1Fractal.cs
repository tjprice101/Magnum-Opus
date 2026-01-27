using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using Terraria.DataStructures;
using Terraria.Audio;

namespace MagnumOpus.Content.DebugWeapons
{
    /// <summary>
    /// Debug weapon to test the FeathersCallFlare tracking crystal effect.
    /// Uses tin sword sprite as placeholder.
    /// Left-click fires homing crystal projectiles with fractal explosion on kill.
    /// </summary>
    public class Debug1Fractal : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.TinBroadsword;

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 150;
            Item.knockBack = 4f;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 5;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.shoot = ModContent.ProjectileType<Debug1FractalFlare>();
            Item.shootSpeed = 14f;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(gold: 1);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Find nearest enemy for homing
            int targetIndex = -1;
            float closestDist = 800f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(player.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        targetIndex = i;
                    }
                }
            }

            // Spawn 3 flares in a spread pattern (white, black, rainbow)
            for (int i = 0; i < 3; i++)
            {
                Vector2 spreadVel = velocity.RotatedBy((i - 1) * 0.15f);
                int proj = Projectile.NewProjectile(source, position, spreadVel, type, damage, knockback, player.whoAmI);
                if (proj >= 0 && proj < Main.maxProjectiles)
                {
                    Main.projectile[proj].ai[0] = i; // Flare type (0=white, 1=black, 2=rainbow)
                    Main.projectile[proj].ai[1] = targetIndex; // Target NPC index
                }
            }

            // Spawn VFX at muzzle
            CustomParticles.GenericFlare(position, Color.White, 0.6f, 15);
            CustomParticles.HaloRing(position, Color.White, 0.4f, 12);

            return false;
        }
    }

    /// <summary>
    /// Homing crystal projectile with fractal explosion on kill.
    /// Copied from FeathersCallFlare with modifications.
    /// </summary>
    public class Debug1FractalFlare : ModProjectile
    {
        // Custom texture - no vanilla textures allowed
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

        private int FlareType => (int)Projectile.ai[0];
        private int TargetIndex => (int)Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            // Rotation based on velocity
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Light based on flare type
            Color lightColor = GetFlareColor();
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * 0.8f);

            // Homing behavior
            if (TargetIndex >= 0 && TargetIndex < Main.maxNPCs)
            {
                NPC target = Main.npc[TargetIndex];
                if (target.active && !target.friendly && target.CanBeChasedBy())
                {
                    // Smooth homing
                    Vector2 direction = target.Center - Projectile.Center;
                    float distance = direction.Length();
                    direction.Normalize();

                    float homingStrength = 0.08f;
                    if (distance < 200f)
                        homingStrength = 0.15f; // Stronger homing when close

                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * Projectile.velocity.Length(), homingStrength);
                }
            }
            else
            {
                // Re-acquire target if current is invalid
                float closestDist = 600f;
                int newTarget = -1;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            newTarget = i;
                        }
                    }
                }
                if (newTarget >= 0)
                    Projectile.ai[1] = newTarget;
            }

            // Trail particles
            if (Projectile.timeLeft % 3 == 0)
            {
                Color trailColor = GetFlareColor() * 0.7f;
                var glow = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    -Projectile.velocity * 0.1f,
                    trailColor,
                    0.3f,
                    15,
                    true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        private Color GetFlareColor()
        {
            return FlareType switch
            {
                0 => Color.White,
                1 => new Color(30, 30, 40),
                2 => Main.hslToRgb((Main.GameUpdateCount * 0.02f) % 1f, 1f, 0.7f),
                _ => Color.White
            };
        }

        public override void OnKill(int timeLeft)
        {
            // Sound
            SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.3f }, Projectile.Center);

            Color primaryColor = GetFlareColor();
            Color secondaryColor = FlareType == 2 
                ? Main.hslToRgb((Main.GameUpdateCount * 0.02f + 0.3f) % 1f, 1f, 0.7f) 
                : Color.White;

            // === FRACTAL EXPLOSION EFFECT ===
            
            // Core flash
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 0.9f, 20);
            CustomParticles.GenericFlare(Projectile.Center, primaryColor, 0.7f, 18);

            // Halo rings cascade
            for (int ring = 0; ring < 4; ring++)
            {
                float progress = ring / 4f;
                Color ringColor = Color.Lerp(primaryColor, secondaryColor, progress);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.3f + ring * 0.15f, 15 + ring * 3);
            }

            // FRACTAL FLARE BURST - 6-point geometric pattern
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + Main.GameUpdateCount * 0.03f;
                Vector2 flareOffset = angle.ToRotationVector2() * 30f;
                float hue = (Main.GameUpdateCount * 0.02f + i * 0.16f) % 1f;
                Color fractalColor = FlareType == 2 
                    ? Main.hslToRgb(hue, 1f, 0.85f) 
                    : Color.Lerp(primaryColor, secondaryColor, (float)i / 6f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, fractalColor, 0.5f, 20);
            }

            // Explosion burst - radial particles
            CustomParticles.ExplosionBurst(Projectile.Center, primaryColor, 12, 8f);
            CustomParticles.ExplosionBurst(Projectile.Center, secondaryColor * 0.7f, 8, 6f);

            // Swan feather burst (if available)
            try
            {
                ThemedParticles.SwanFeatherBurst(Projectile.Center, 6, 40f);
            }
            catch { }

            // Prismatic sparkle burst
            CustomParticles.PrismaticSparkleBurst(Projectile.Center, primaryColor, 10);

            // Mini lightning fractals for extra flair
            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(0.3f);
                    Vector2 lightningEnd = Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(40f, 70f);
                    // Draw mini lightning if available
                    try
                    {
                        MagnumVFX.DrawSwanLakeLightning(Projectile.Center, lightningEnd, 4, 8f, 1, 0.3f);
                    }
                    catch { }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glowTex = TextureAssets.Extra[ExtrasID.SharpTears].Value; // Glow orb texture
            Vector2 origin = texture.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;

            Color flareColor = GetFlareColor();

            // Draw trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = 1f - progress;
                float trailScale = (1f - progress * 0.5f) * 0.6f;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = flareColor * trailAlpha * 0.6f;

                // Rainbow cycling for rainbow type
                if (FlareType == 2)
                {
                    float hue = (Main.GameUpdateCount * 0.02f + i * 0.08f) % 1f;
                    trailColor = Main.hslToRgb(hue, 1f, 0.7f) * trailAlpha * 0.6f;
                }

                // Draw glowing orb at trail position
                spriteBatch.Draw(glowTex, trailPos, null, trailColor, 0f, glowOrigin, trailScale, SpriteEffects.None, 0f);
            }

            // Draw main projectile with glow layers
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Outer glow
            spriteBatch.Draw(glowTex, drawPos, null, flareColor * 0.4f, 0f, glowOrigin, 1.2f, SpriteEffects.None, 0f);
            // Middle glow
            spriteBatch.Draw(glowTex, drawPos, null, flareColor * 0.6f, 0f, glowOrigin, 0.8f, SpriteEffects.None, 0f);
            // Core
            spriteBatch.Draw(glowTex, drawPos, null, Color.White * 0.8f, 0f, glowOrigin, 0.4f, SpriteEffects.None, 0f);

            // Draw the actual projectile sprite
            spriteBatch.Draw(texture, drawPos, null, flareColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return GetFlareColor() * ((255 - Projectile.alpha) / 255f);
        }
    }
}

