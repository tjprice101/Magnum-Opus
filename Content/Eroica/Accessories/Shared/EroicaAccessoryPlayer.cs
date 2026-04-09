using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using ReLogic.Content;
using static Terraria.ModLoader.PlayerDrawLayer;
using MagnumOpus.Content.Eroica.Accessories.SakurasBurningWill;

namespace MagnumOpus.Content.Eroica.Accessories.Shared
{
    /// <summary>
    /// ModPlayer class that handles Sakura's Burning Will summoner accessory effects.
    /// Pyre, Symphony, and Funeral March mechanics have been migrated to MelodicAttunementPlayer.
    /// </summary>
    public class EroicaAccessoryPlayer : ModPlayer
    {
        // ========== SAKURA'S BURNING WILL (Summoner) ==========
        public bool hasSakurasBurningWill = false;
        public int heroicSpiritTimer = 0;
        private const int HeroicSpiritInterval = 720; // 12 seconds

        // ========== FLOATING VISUAL ==========
        public float floatAngle = 0f;

        public override void ResetEffects()
        {
            hasSakurasBurningWill = false;
        }

        public override void PostUpdate()
        {
            if (hasSakurasBurningWill)
            {
                floatAngle += 0.03f;

                heroicSpiritTimer++;
                if (heroicSpiritTimer >= HeroicSpiritInterval)
                {
                    heroicSpiritTimer = 0;
                    SummonHeroicSpirit();
                }

                UpdateMinionProximityBonus();
            }
            else
            {
                heroicSpiritTimer = 0;
            }
        }

        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            if (hasSakurasBurningWill && item.DamageType == DamageClass.Summon)
            {
                damage *= 1.20f;
            }
        }

        private void SummonHeroicSpirit()
        {
            if (Main.myPlayer == Player.whoAmI)
            {
                Vector2 spawnPos = Player.Center + new Vector2(Main.rand.NextFloat(-50f, 50f), -60f);
                int damage = (int)(Player.GetTotalDamage(DamageClass.Summon).ApplyTo(150));

                NPC target = null;
                float maxDist = 600f;
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.CanBeChasedBy())
                    {
                        float dist = Vector2.Distance(Player.Center, npc.Center);
                        if (dist < maxDist)
                        {
                            maxDist = dist;
                            target = npc;
                        }
                    }
                }

                Projectile.NewProjectile(
                    Player.GetSource_Accessory(new Item()),
                    spawnPos,
                    Vector2.Zero,
                    ModContent.ProjectileType<HeroicSpiritMinion>(),
                    damage,
                    6f,
                    Player.whoAmI,
                    target?.whoAmI ?? -1
                );
            }

            SoundEngine.PlaySound(SoundID.Item78 with { Pitch = 0.2f }, Player.Center);
            SakurasBurningWillVFX.HeroicSpiritSummonVFX(Player.Center + new Vector2(0, -60f));
        }

        private void UpdateMinionProximityBonus()
        {
            bool nearMinion = false;

            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.owner == Player.whoAmI && proj.minion)
                {
                    float dist = Vector2.Distance(Player.Center, proj.Center);
                    if (dist < 240f)
                    {
                        nearMinion = true;
                        break;
                    }
                }
            }

            if (nearMinion)
            {
                Player.statDefense += 8;
            }
        }
    }

    /// <summary>
    /// Draw layer for floating Sakura's Burning Will accessory visual.
    /// </summary>
    public class EroicaFloatDrawLayer : PlayerDrawLayer
    {
        private static Asset<Texture2D> sakuraFloatTexture;

        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;
        private const int FrameTime = 4;

        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.BackAcc);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            var modPlayer = drawInfo.drawPlayer.GetModPlayer<EroicaAccessoryPlayer>();
            return modPlayer.hasSakurasBurningWill;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var modPlayer = drawInfo.drawPlayer.GetModPlayer<EroicaAccessoryPlayer>();
            Player player = drawInfo.drawPlayer;

            if (sakuraFloatTexture == null)
                sakuraFloatTexture = ModContent.Request<Texture2D>("MagnumOpus/Content/Eroica/Accessories/SakurasBurningWill/SakurasBurningWill_Float");

            if (sakuraFloatTexture == null || !sakuraFloatTexture.IsLoaded)
                return;

            float baseAngle = modPlayer.floatAngle;
            int currentFrame = (int)(Main.GameUpdateCount / FrameTime) % TotalFrames;

            Texture2D texture = sakuraFloatTexture.Value;

            int sakuraFrame = (currentFrame + TotalFrames / 2) % TotalFrames;
            int sakuraFrameX = sakuraFrame % FrameColumns;
            int sakuraFrameY = sakuraFrame / FrameColumns;

            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            Rectangle sourceRect = new Rectangle(sakuraFrameX * frameWidth, sakuraFrameY * frameHeight, frameWidth, frameHeight);
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);

            Vector2 offset = new Vector2((float)Math.Cos(baseAngle + MathHelper.Pi) * 25f + 35f, (float)Math.Sin(baseAngle * 1.5f + 1f) * 12f - 25f);
            Vector2 drawPos = player.Center + offset - Main.screenPosition;

            Color lightColor = Lighting.GetColor((int)(player.Center.X / 16), (int)(player.Center.Y / 16));

            // Pink/scarlet glow effect
            Color glowColor = new Color(255, 150, 180, 0) * 0.4f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 glowOffset = new Vector2(2f, 0f).RotatedBy(i * MathHelper.PiOver2);
                drawInfo.DrawDataCache.Add(new DrawData(
                    texture,
                    drawPos + glowOffset,
                    sourceRect,
                    glowColor,
                    0f,
                    origin,
                    1f,
                    SpriteEffects.None,
                    0
                ));
            }

            drawInfo.DrawDataCache.Add(new DrawData(
                texture,
                drawPos,
                sourceRect,
                lightColor,
                0f,
                origin,
                1f,
                SpriteEffects.None,
                0
            ));
        }
    }
}
