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
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons.SymphonysEnd
{
    /// <summary>
    /// Symphony's End — Where all melodies find their conclusion.
    /// 
    /// Fate-theme endgame magic wand that unleashes spiraling spectral blades
    /// that corkscrew toward the cursor. Rapid fire creates overlapping helix
    /// patterns. Blades shatter into 4 fragments on contact.
    /// 
    /// Self-contained weapon system with zero shared VFX system references.
    /// </summary>
    public class SymphonysEndItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/SymphonysEnd";

        // ─── Stats (preserved exactly) ────────────────────────────

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage        = 500;
            Item.DamageType    = DamageClass.Magic;
            Item.width         = 32;
            Item.height        = 32;
            Item.useTime       = 8;
            Item.useAnimation  = 8;
            Item.useStyle      = ItemUseStyleID.Shoot;
            Item.knockBack     = 4f;
            Item.value         = Item.sellPrice(gold: 55);
            Item.rare          = ModContent.RarityType<FateRarity>();
            Item.UseSound      = SoundID.Item8;
            Item.autoReuse     = true;
            Item.noMelee       = true;
            Item.mana          = 8;
            Item.shoot         = ModContent.ProjectileType<SymphonySpiralBlade>();
            Item.shootSpeed    = 10f;
        }

        // ─── Tooltips ─────────────────────────────────────────────

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Rapid-fire spiraling spectral blades that corkscrew toward the cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Blades shatter into 4 gravity-affected blade fragments on impact"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Crescendo Mode: 3 seconds of continuous fire increases rate by 50% and blade size"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "Diminuendo: stopping after Crescendo grants +20% damage for 2 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect5",
                "Final Note: 10 seconds of continuous fire launches a massive 5x blade with full pierce"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Every symphony must end. This one ends the world.'")
            {
                OverrideColor = new Color(180, 40, 80) // Cosmic Crimson — Fate lore color
            });
        }

        // ─── Hold VFX: Wand Tip Crackle ──────────────────────────

        public override void HoldItem(Player player)
        {
            var sym = player.Symphony();
            sym.IsHoldingWand = true;

            if (!Main.dedServ)
            {
                Vector2 tipPos = player.MountedCenter + new Vector2(player.direction * 20f, -8f);

                // Crackle intensity scales with recent fire rate
                SymphonyParticleFactory.SpawnCrackleAura(tipPos, 0.3f + sym.FireIntensity * 0.7f);

                // Color-shifting aura light
                float time  = (float)Main.timeForVisualEffects;
                float shift = MathF.Sin(time * 0.04f) * 0.5f + 0.5f;
                Color lightCol = Color.Lerp(SymphonyUtils.SymphonyViolet, SymphonyUtils.SymphonyPink, shift);
                Lighting.AddLight(tipPos, lightCol.ToVector3() * (0.25f + sym.FireIntensity * 0.25f));
            }
        }

        // ─── Shoot: Spawn Spiraling Blade ─────────────────────────

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var sym = player.Symphony();
            sym.OnFire();

            // Final Note: after 10s continuous fire, the last blade is a giant 5x blade
            bool isFinalNote = sym.FinalNoteReady && !sym.FinalNoteFired;
            if (isFinalNote)
            {
                sym.FinalNoteFired = true;
            }

            // Random 60 px offset from player center
            Vector2 spawnOffset = Main.rand.NextVector2CircularEdge(60f, 60f);
            Vector2 spawnPos    = player.Center + spawnOffset;

            // Velocity toward cursor with ±45° spiral component
            Vector2 toCursor   = (Main.MouseWorld - spawnPos).SafeNormalize(Vector2.UnitX);
            float spiralAngle  = Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);
            Vector2 spiralVel  = toCursor.RotatedBy(spiralAngle) * velocity.Length();

            // Final Note: 5x damage, no shatter, full pierce
            int finalDamage = isFinalNote ? damage * 5 : damage;

            // ai[0] = MouseWorld.X, ai[1] = MouseWorld.Y
            int projIdx = Projectile.NewProjectile(source, spawnPos, spiralVel, type, finalDamage, knockback,
                player.whoAmI, Main.MouseWorld.X, Main.MouseWorld.Y);

            // Apply Crescendo Mode blade scale multiplier
            if (projIdx >= 0 && projIdx < Main.maxProjectiles)
            {
                var proj = Main.projectile[projIdx];
                proj.scale *= sym.BladeScaleMultiplier;
                if (isFinalNote)
                {
                    proj.scale *= 5f; // Giant Final Note blade
                    proj.penetrate = -1; // Pass through all enemies
                    proj.timeLeft = 300; // Extended range before detonation
                    proj.localAI[1] = 1f; // Flag for Final Note detonation on kill
                }
            }

            // ─── Spawn VFX ────────────────────────────────────────
            if (!Main.dedServ)
            {
                Vector2 dir = spiralVel.SafeNormalize(Vector2.UnitX);

                // Flash ring
                SymphonyParticleHandler.Spawn(SymphonyParticleFactory.Ring(
                    spawnPos, SymphonyUtils.FinalWhite * 0.8f, 0.12f, 14));

                // Directional sparks
                for (int i = 0; i < 4; i++)
                {
                    float angle    = MathHelper.TwoPi * i / 4f;
                    Vector2 spkVel = dir.RotatedBy(angle * 0.3f) * Main.rand.NextFloat(3f, 5f);
                    SymphonyParticleHandler.Spawn(SymphonyParticleFactory.Spark(
                        spawnPos, spkVel, SymphonyUtils.RandomPaletteColor(), 0.06f, 12));
                }

                // Occasional music note
                if (Main.rand.NextBool(3))
                {
                    SymphonyParticleHandler.Spawn(SymphonyParticleFactory.Note(
                        spawnPos, new Vector2(0, -1.5f), SymphonyUtils.HarmonyBlue, 0.18f, 28));
                }

                Lighting.AddLight(spawnPos, SymphonyUtils.SymphonyPink.ToVector3() * 0.5f);
            }

            return false;
        }

        // ─── World Sprite Glow ────────────────────────────────────

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor,
            Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 pos    = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            float pulse    = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.04f) * 0.05f;

            // Additive bloom layers
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, pos, null,
                SymphonyUtils.Additive(SymphonyUtils.SymphonyViolet, 0.3f),
                rotation, origin, scale * 1.12f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, pos, null,
                SymphonyUtils.Additive(SymphonyUtils.SymphonyPink, 0.25f),
                rotation, origin, scale * 1.06f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, pos, null,
                SymphonyUtils.Additive(SymphonyUtils.FinalWhite, 0.18f),
                rotation, origin, scale * 1.01f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            // Main sprite
            spriteBatch.Draw(texture, pos, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);

            Lighting.AddLight(Item.Center, SymphonyUtils.SymphonyPink.ToVector3() * 0.5f);

            return false;
        }

        // ─── Inventory Pulse ──────────────────────────────────────

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position,
            Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            float pulse    = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.08f + 1f;
            Texture2D tex  = TextureAssets.Item[Item.type].Value;

            // Subtle glow behind
            spriteBatch.Draw(tex, position, frame,
                SymphonyUtils.SymphonyViolet * 0.2f,
                0f, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);

            // Main draw
            spriteBatch.Draw(tex, position, frame,
                drawColor, 0f, origin, scale * pulse, SpriteEffects.None, 0f);

            return false;
        }
    }
}
