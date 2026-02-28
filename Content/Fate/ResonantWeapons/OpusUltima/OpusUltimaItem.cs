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
            Item.channel = false;
            Item.UseSound = null; // Swing projectile handles sounds
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Each swing writes a measure of cosmic music"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Fires cosmic energy balls that explode into 5 homing seekers"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every third swing performs the Recapitulation — a massive energy blast"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The ultimate composition, the magnum opus of destruction'")
            {
                OverrideColor = new Color(180, 40, 80) // Cosmic Crimson
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var op = player.Opus();

            // Fire the held swing projectile
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI);

            // Register swing with player tracker (returns movement index)
            int movement = op.OnSwing();

            // Movement-specific sound accents
            if (movement == 2)
            {
                // Recapitulation: dramatic crescendo sound
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.1f, Volume = 0.7f }, player.Center);
            }

            return false;
        }

        public override void HoldItem(Player player)
        {
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
