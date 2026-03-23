using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Projectiles;
using MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Projectiles;
using MagnumOpus.Content.SandboxExoblade.Utilities;

namespace MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars
{
    /// <summary>
    /// Fractal of the Stars — A blade forged from shattered constellations.
    ///
    /// SELF-CONTAINED WEAPON SYSTEM (no shared VFX libraries):
    ///   - Own particle system (FractalParticleHandler)
    ///   - Own GPU trail renderer (FractalTrailRenderer)
    ///   - Own shader pipeline (FractalShaderLoader → 4 .fx files)
    ///   - Own ModPlayer state (FractalPlayer via player.Fractal())
    ///   - Own projectiles (FractalSwingProjectile, FractalOrbitBlade)
    ///
    /// ATTACK PATTERN:
    ///   TRUE MELEE — fires FractalSwingProjectile as a held swing.
    ///   3-phase combo: Horizontal Sweep → Rising Uppercut → Gravity Slam
    ///   On hit: spawns orbiting spectral star blades (max 6)
    ///   Every 3rd hit (Gravity Slam): Star Fracture — geometric fractal explosion
    ///   Orbit blades periodically fire prismatic beams
    /// </summary>
    public class FractalOfTheStarsItem : ModItem, IOverdriveItem
    {
        public IResonantOverdrive GetOverdrivePlayer(Player player) => player.GetModPlayer<FractalPlayer>();

        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/FractalOfTheStars";

        private static Asset<Texture2D> _glowTex;

        /// <summary>Tracks the 3-phase combo: Sweep → Uppercut → Gravity Slam.
        /// Gravity Slam spawns extra orbit blades.</summary>
        private int comboPhase = 0;

        /// <summary>Maximum FractalOrbitBlade count per player.</summary>
        private const int MaxOrbitBlades = 6;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            // === PRESERVED STATS ===
            Item.damage = 850;
            Item.DamageType = DamageClass.Melee;
            Item.width = 58;
            Item.height = 58;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 58);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.autoReuse = true;

            // === HELD PROJECTILE SWING (TRUE MELEE) ===
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<FractalSwingProjectile>();
            Item.shootSpeed = 1f;
            Item.channel = true;
            Item.UseSound = null; // Swing projectile handles sounds
        }

        public override bool CanShoot(Player player)
        {
            bool isDash = player.altFunctionUse == 2;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != player.whoAmI || p.type != Item.shoot)
                    continue;
                if (isDash) return false;
                if (!(p.ai[0] == 1 && p.ai[1] == 1)) return false;
            }
            return true;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "3-phase geometric combo: Horizontal Sweep, Rising Uppercut, Gravity Slam"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "On hit, spawns orbiting spectral star blades (max 6) that fire prismatic beams every 60 frames"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Gravity Slam triggers Star Fracture — a recursive geometric explosion"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Fractal Recursion: sub-fractures cascade at 1/3 size and 1/3 damage per depth"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "The mathematical beauty of the cosmos expressed as devastation"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The stars do not scatter randomly. They fracture in self-similar patterns, infinitely deep.'")
            {
                OverrideColor = new Color(180, 40, 80) // Cosmic Crimson (Fate theme)
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Right-click: Fractal Slash — slash all on-screen enemies
            if (player.altFunctionUse == 2)
            {
                var fp = player.GetModPlayer<FractalPlayer>();
                if (fp.IsChargeFull)
                {
                    fp.ConsumeCharge();
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f }, player.Center);
                    // Slash all on-screen enemies
                    foreach (NPC npc in Main.ActiveNPCs)
                    {
                        if (!npc.CanBeChasedBy()) continue;
                        if (System.Math.Abs(npc.Center.X - player.Center.X) > Main.screenWidth / 2 + 100) continue;
                        if (System.Math.Abs(npc.Center.Y - player.Center.Y) > Main.screenHeight / 2 + 100) continue;
                        Projectile.NewProjectile(source, npc.Center, Vector2.Zero,
                            ModContent.ProjectileType<Projectiles.FractalSpecialProj>(),
                            damage * 2, knockback, player.whoAmI);
                    }
                }
                else
                    SoundEngine.PlaySound(SoundID.Item16 with { Pitch = 0.5f, Volume = 0.5f }, player.Center);
                return false;
            }

            float state = player.altFunctionUse == 2 ? 1f : 0f;
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, state, 0);

            // --- 3-Phase Fractal Combo ---
            // Sweep (0): Spawn 1 orbit blade (if under cap)
            // Uppercut (1): Spawn 1 orbit blade + 1 at opposite angle
            // Gravity Slam (2): Spawn 2 orbit blades + Star Fracture burst (3 energy balls)
            int phase = comboPhase % 3;
            comboPhase++;
            int orbitType = ModContent.ProjectileType<FractalOrbitBlade>();
            int currentBlades = player.Fractal().OrbitBladeCount;

            switch (phase)
            {
                case 0: // Sweep — single orbit blade
                    if (currentBlades < MaxOrbitBlades)
                    {
                        float startAngle = MathHelper.TwoPi * currentBlades / MaxOrbitBlades;
                        Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero,
                            orbitType, (int)(damage * 0.3f), knockback * 0.3f, player.whoAmI,
                            startAngle, 0f);
                    }
                    break;

                case 1: // Uppercut — 2 orbit blades at opposing angles
                    for (int i = 0; i < 2 && currentBlades + i < MaxOrbitBlades; i++)
                    {
                        float startAngle = MathHelper.TwoPi * (currentBlades + i) / MaxOrbitBlades;
                        Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero,
                            orbitType, (int)(damage * 0.3f), knockback * 0.3f, player.whoAmI,
                            startAngle, 0f);
                    }
                    break;

                case 2: // Gravity Slam — 2 orbit blades + Star Fracture energy ball burst
                    for (int i = 0; i < 2 && currentBlades + i < MaxOrbitBlades; i++)
                    {
                        float startAngle = MathHelper.TwoPi * (currentBlades + i) / MaxOrbitBlades;
                        Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero,
                            orbitType, (int)(damage * 0.3f), knockback * 0.3f, player.whoAmI,
                            startAngle, 0f);
                    }

                    // Star Fracture burst — 3 energy balls toward cursor
                    Vector2 aimDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 ballVel = aimDir.RotatedBy(MathHelper.ToRadians(25 * i)) * 10f;
                        // Use OpusEnergyBallProjectile as seeker (mode=1) — aggressive homing
                        Projectile.NewProjectile(source, player.MountedCenter, ballVel,
                            ModContent.ProjectileType<OpusEnergyBallProjectile>(),
                            (int)(damage * 0.35f), knockback * 0.5f, player.whoAmI,
                            1f, 1f); // mode=Seeker
                    }
                    break;
            }

            return false;
        }

        public override void HoldItem(Player player)
        {
            player.GetModPlayer<FractalPlayer>().IsHoldingFractalOfTheStars = true;
            player.ExoBlade().rightClickListener = true;
            player.ExoBlade().mouseWorldListener = true;

            if (Main.dedServ) return;

            float pulse = 0.6f + MathF.Sin((float)Main.timeForVisualEffects * 0.04f) * 0.15f;
            Lighting.AddLight(player.Center, FractalUtils.FractalPurple.ToVector3() * 0.3f * pulse);

            // Ambient stellar motes
            if (Main.rand.NextBool(10))
            {
                Vector2 motePos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                Color moteCol = FractalUtils.GetStarShimmer((float)Main.timeForVisualEffects * 0.03f + Main.rand.NextFloat());
                FractalParticleHandler.SpawnParticle(new FractalMote(
                    motePos, new Vector2(0, -0.3f), moteCol, 0.1f, 25));
            }

            // Occasional tiny star particle
            if (Main.rand.NextBool(20))
            {
                Vector2 starPos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 starVel = Main.rand.NextVector2Circular(0.5f, 0.5f);
                FractalParticleHandler.SpawnParticle(new FractalStarParticle(
                    starPos, starVel, FractalUtils.ConstellationWhite, 0.08f, 30, 4));
            }
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if (Main.dedServ) return;

            _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            if (_glowTex?.Value == null) return;

            Texture2D itemTex = Terraria.GameContent.TextureAssets.Item[Item.type].Value;
            Vector2 origin = itemTex.Size() / 2f;
            Vector2 drawPos = Item.Center - Main.screenPosition;
            float pulse = 0.85f + MathF.Sin((float)Main.timeForVisualEffects * 0.05f) * 0.15f;

            try
            {
                FractalUtils.BeginAdditive(spriteBatch);
                FractalUtils.DrawItemBloom(spriteBatch, itemTex, drawPos, origin, rotation, scale, pulse);
                FractalUtils.EndAdditive(spriteBatch);
            }
            catch
            {
                try { FractalUtils.EndAdditive(spriteBatch); } catch { }
            }
        }
    }
}
