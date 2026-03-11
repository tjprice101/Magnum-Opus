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
using MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Projectiles;
using MagnumOpus.Content.SandboxExoblade.Utilities;

namespace MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima
{
    /// <summary>
    /// Opus Ultima — The Magnum Opus. The culmination of all musical training.
    ///
    /// SELF-CONTAINED WEAPON SYSTEM (no shared VFX libraries):
    ///   - Own particle system (OpusParticleHandler)
    ///   - Own GPU trail renderer (OpusTrailRenderer)
    ///   - Own shader pipeline (OpusShaderLoader → 4 .fx files)
    ///   - Own ModPlayer state (OpusPlayer via player.Opus())
    ///   - Own projectiles (OpusSwingProjectile, OpusEnergyBallProjectile)
    ///
    /// ATTACK PATTERN:
    ///   Fires OpusSwingProjectile as a HELD swing (not vanilla useStyle).
    ///   Swing follows 3-movement combo cycle:
    ///     Movement I   (Exposition):     Standard sweep, fires single energy ball
    ///     Movement II  (Development):    Faster cross-slash, fires twin energy balls
    ///     Movement III (Recapitulation): Wide arc, fires massive energy ball (1.5x size/damage)
    ///   Each energy ball explodes into 5 homing seekers on enemy contact.
    ///   On melee hit: DestinyCollapse (4s) + 3-5 seeking crystal shards at 40% damage.
    ///   Weapon GLOWS with increasing intensity through the combo.
    /// </summary>
    public class OpusUltimaItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/OpusUltima";

        // Glow texture for hold VFX
        private static Asset<Texture2D> _glowTex;

        /// <summary>Tracks the 3-movement combo cycle: Exposition → Development → Recapitulation.</summary>
        private int movementCounter = 0;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            // === PRESERVED STATS ===
            Item.damage = 720;
            Item.DamageType = DamageClass.Melee;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 52);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.autoReuse = true;

            // === HELD PROJECTILE SWING ===
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<OpusSwingProjectile>();
            Item.shootSpeed = 12f;
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "3-movement combo: Exposition, Development, Recapitulation"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Swings release energy balls that explode into 5 homing seekers"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Recapitulation fires a massive 1.5x energy blast — on melee hit, spawns 3-5 crystal shards"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Opus Resonance: each completed cycle grants +5% all damage (max 9 stacks, +45%)"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Resonance stacks orbit as visible constellation stars"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The final work. The magnum opus. Written in starfire.'")
            {
                OverrideColor = new Color(180, 40, 80) // Cosmic Crimson
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float state = player.altFunctionUse == 2 ? 1f : 0f;
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, state, 0);

            // --- 3-Movement Opus Combo ---
            // Movement I  (Exposition): Single energy ball forward
            // Movement II (Development): Twin energy balls in narrow spread
            // Movement III (Recapitulation): Massive energy ball (1.5x) + flanking seekers
            Vector2 aimDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            int movement = movementCounter % 3;
            movementCounter++;
            int energyBallType = ModContent.ProjectileType<OpusEnergyBallProjectile>();

            switch (movement)
            {
                case 0: // Exposition — single energy ball
                    Projectile.NewProjectile(source, player.MountedCenter, aimDir * 12f,
                        energyBallType, (int)(damage * 0.4f), knockback * 0.5f, player.whoAmI,
                        0f, 1f); // mode=EnergyBall, size=1.0
                    break;

                case 1: // Development — twin energy balls
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 ballVel = aimDir.RotatedBy(MathHelper.ToRadians(12 * i)) * 13f;
                        Projectile.NewProjectile(source, player.MountedCenter, ballVel,
                            energyBallType, (int)(damage * 0.35f), knockback * 0.5f, player.whoAmI,
                            0f, 1f);
                    }
                    break;

                case 2: // Recapitulation — massive energy ball + 2 crystal shards
                    Projectile.NewProjectile(source, player.MountedCenter, aimDir * 10f,
                        energyBallType, (int)(damage * 0.6f), knockback, player.whoAmI,
                        0f, 1.5f); // size=1.5x massive

                    // Flanking crystal shards
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 shardVel = aimDir.RotatedBy(MathHelper.ToRadians(35 * i)) * 8f;
                        Projectile.NewProjectile(source, player.MountedCenter, shardVel,
                            energyBallType, (int)(damage * 0.25f), knockback * 0.3f, player.whoAmI,
                            2f, 1f); // mode=CrystalShard
                    }
                    break;
            }

            return false;
        }

        public override void HoldItem(Player player)
        {
            player.ExoBlade().rightClickListener = true;
            player.ExoBlade().mouseWorldListener = true;

            if (Main.dedServ) return;

            var op = player.Opus();
            float intensity = op.ComboIntensity;

            // Ambient cosmic motes around weapon (scales with combo)
            if (Main.rand.NextBool(intensity > 0.3f ? 3 : 6))
            {
                Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                Vector2 vel = -offset * 0.02f; // Drift inward
                Color col = OpusUtils.PaletteLerp(Main.rand.NextFloat(0.2f + intensity * 0.3f, 0.8f));
                OpusParticleHandler.SpawnParticle(new OpusMote(
                    player.Center + offset, vel, col, 0.15f + intensity * 0.1f, 25));
            }

            // Gold glow intensifies with combo
            if (intensity > 0.5f && Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Color goldCol = Color.Lerp(OpusUtils.OpusCrimson, OpusUtils.GloryGold, intensity);
                OpusParticleHandler.SpawnParticle(new OpusMote(
                    player.Center + offset, Vector2.Zero, goldCol, 0.1f + intensity * 0.15f, 18));
            }

            // Weapon lighting
            Lighting.AddLight(player.Center, OpusUtils.OpusCrimson.ToVector3() * 0.3f * (0.5f + intensity * 0.5f));
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
            Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");

            if (_glowTex?.Value != null)
            {
                // Inventory glow pulse
                float time = (float)Main.timeForVisualEffects;
                float pulse = 0.85f + MathF.Sin(time * 0.04f) * 0.15f;

                try
                {
                    spriteBatch.Draw(_glowTex.Value, position, null,
                        OpusUtils.Additive(OpusUtils.OpusCrimson, 0.12f * pulse),
                        0f, _glowTex.Value.Size() / 2f, scale * 2.5f, SpriteEffects.None, 0f);
                    spriteBatch.Draw(_glowTex.Value, position, null,
                        OpusUtils.Additive(OpusUtils.GloryGold, 0.08f * pulse),
                        0f, _glowTex.Value.Size() / 2f, scale * 1.5f, SpriteEffects.None, 0f);
                }
                catch { }
            }

            return true;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");

            if (_glowTex?.Value != null)
            {
                float time = (float)Main.timeForVisualEffects;
                float pulse = 0.9f + MathF.Sin(time * 0.05f) * 0.1f;
                Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height / 2f);
                Vector2 origin = _glowTex.Value.Size() / 2f;

                try
                {
                    // Crimson-gold glow behind item in world
                    spriteBatch.Draw(_glowTex.Value, drawPos, null,
                        OpusUtils.Additive(OpusUtils.OpusCrimson, 0.2f * pulse),
                        0f, origin, 1.5f * pulse, SpriteEffects.None, 0f);
                    spriteBatch.Draw(_glowTex.Value, drawPos, null,
                        OpusUtils.Additive(OpusUtils.GloryGold, 0.15f * pulse),
                        0f, origin, 1.0f * pulse, SpriteEffects.None, 0f);
                }
                catch { }
            }

            return true;
        }
    }
}
