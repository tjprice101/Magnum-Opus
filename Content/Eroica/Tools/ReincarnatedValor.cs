using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using ReLogic.Content;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Enemies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Tools
{
    /// <summary>
    /// Reincarnated Valor - Eroica wings with higher flight time and speed than Moonlight wings.
    /// Rainbow rarity, crafted with Eroica materials.
    /// Uses 6x6 sprite sheet for flying animation.
    /// </summary>
    [AutoloadEquip(EquipType.Wings)]
    public class ReincarnatedValor : ModItem
    {
        public override void SetStaticDefaults()
        {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(
                flyTime: 280, // Higher than Moonlight (220)
                flySpeedOverride: 14f, // Faster than Moonlight (11)
                accelerationMultiplier: 3.2f, // Higher than Moonlight (2.8)
                hasHoldDownHoverFeatures: true
            );
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 2);
            Item.rare = ItemRarityID.Expert; // Rainbow rarity
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Rise upon wings forged from legend'") { OverrideColor = new Color(200, 50, 50) });
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
            ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 1.1f;
            ascentWhenRising = 0.22f;
            maxCanAscendMultiplier = 1.3f;
            maxAscentMultiplier = 3.8f;
            constantAscend = 0.18f;
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ReincarnatedValorPlayer>().hasWingsEquipped = true;
            
            // Enhanced ambient particles when flying using new particle system
            if (!hideVisual && player.velocity.Y != 0)
            {
                ThemedParticles.EroicaAura(player.Center, 35f);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfTriumphsTempo>(), 5)
                .AddIngredient(ItemID.SoulofFlight, 25)
                .AddIngredient(ItemID.SoulofMight, 20)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class ReincarnatedValorPlayer : ModPlayer
    {
        public int wingFrame = 0;
        private int frameCounter = 0;
        private bool wasFlying = false;
        
        // Direction constants for doubleTapCardinalTimer array
        private const int DashDown = 0;
        private const int DashUp = 1;
        private const int DashLeft = 2;
        private const int DashRight = 3;
        
        public bool hasWingsEquipped = false;
        private int dodgeCooldown = 0;
        private const int DodgeCooldownMax = 25;
        private const float DodgeSpeed = 28f;
        private bool isDodging = false;
        private int dodgeTimer = 0;
        private const int DodgeDuration = 8;
        private int dashDir = -1;

        public override void ResetEffects()
        {
            hasWingsEquipped = false;
            
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "ReincarnatedValor", EquipType.Wings);
            bool hasWings = Player.wings == wingSlot && wingSlot > 0;
            
            if (!hasWings && !hasWingsEquipped)
            {
                dashDir = -1;
                return;
            }
            
            if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DashRight] < 15 && Player.doubleTapCardinalTimer[DashLeft] == 0)
            {
                dashDir = DashRight;
            }
            else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DashLeft] < 15 && Player.doubleTapCardinalTimer[DashRight] == 0)
            {
                dashDir = DashLeft;
            }
            else
            {
                dashDir = -1;
            }
        }
        
        public override void PreUpdateMovement()
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "ReincarnatedValor", EquipType.Wings);
            bool hasWings = Player.wings == wingSlot && wingSlot > 0;
            
            if (!hasWings && !hasWingsEquipped)
                return;
            
            if (CanDodge() && dashDir != -1 && dodgeCooldown <= 0)
            {
                int direction = dashDir == DashLeft ? -1 : 1;
                PerformDodge(direction);
            }
            
            if (isDodging)
            {
                dodgeTimer++;
                Player.immune = true;
                Player.immuneTime = 2;
                Player.immuneNoBlink = true;
                
                ThemedParticles.DodgeTrail(Player.Center, Player.velocity, false);
                
                for (int i = 0; i < 2; i++)
                {
                    Dust trail = Dust.NewDustDirect(Player.position, Player.width, Player.height, 
                        DustID.CrimsonTorch, -Player.velocity.X * 0.2f, -Player.velocity.Y * 0.2f, 100, default, 1.4f);
                    trail.noGravity = true;
                }
                
                if (dodgeTimer >= DodgeDuration)
                {
                    isDodging = false;
                    dodgeTimer = 0;
                    ThemedParticles.EroicaImpact(Player.Center, 1f);
                }
            }
            
            if (dodgeCooldown > 0)
                dodgeCooldown--;
        }
        
        private bool CanDodge()
        {
            return (hasWingsEquipped || Player.wings == EquipLoader.GetEquipSlot(Mod, "ReincarnatedValor", EquipType.Wings))
                && Player.dashType == DashID.None
                && !Player.setSolar
                && !Player.mount.Active;
        }

        public override void PostUpdate()
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "ReincarnatedValor", EquipType.Wings);
            bool hasWings = Player.wings == wingSlot && wingSlot > 0;

            if (!hasWings && !hasWingsEquipped)
            {
                wingFrame = 0;
                frameCounter = 0;
                wasFlying = false;
                return;
            }

            bool isFlying = Player.controlJump && Player.velocity.Y != 0 && !Player.mount.Active;
            bool isOnGround = Player.velocity.Y == 0;
            
            if (isFlying || isDodging)
            {
                frameCounter++;
                if (frameCounter >= 2)
                {
                    frameCounter = 0;
                    wingFrame++;
                    if (wingFrame >= 36)
                        wingFrame = 0;
                }
                wasFlying = true;
            }
            else if (isOnGround)
            {
                wingFrame = 0;
                frameCounter = 0;
                wasFlying = false;
            }
            else if (wasFlying && !isFlying)
            {
                wingFrame = 0;
                frameCounter = 0;
            }
        }

        private void PerformDodge(int direction)
        {
            isDodging = true;
            dodgeTimer = 0;
            dodgeCooldown = DodgeCooldownMax;
            
            Vector2 dodgeVelocity = new Vector2(direction, 0f);
            Player.velocity = dodgeVelocity * DodgeSpeed;
            
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.3f, Volume = 0.8f }, Player.Center);
            
            ThemedParticles.EroicaImpact(Player.Center, 1.5f);
            ThemedParticles.TeleportBurst(Player.Center, false);
        }
        
        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "ReincarnatedValor", EquipType.Wings);
            if (Player.wings == wingSlot && wingSlot > 0)
            {
                PlayerDrawLayers.Wings.Hide();
            }
        }
    }
    
    /// <summary>
    /// Custom draw layer for Reincarnated Valor wings.
    /// Uses 6x6 sprite sheet for all animation - frame 0 when idle, animates when flying.
    /// Matches WingsOfTheMoon rendering behavior.
    /// </summary>
    public class ReincarnatedValorLayer : PlayerDrawLayer
    {
        private Asset<Texture2D> _animatedTexture; // 6x6 sprite sheet for all states

        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.BackAcc);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "ReincarnatedValor", EquipType.Wings);
            return drawInfo.drawPlayer.wings == wingSlot && wingSlot > 0 && !drawInfo.drawPlayer.dead && !drawInfo.drawPlayer.invis;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            var modPlayer = player.GetModPlayer<ReincarnatedValorPlayer>();

            // Always use the 6x6 animated sprite sheet
            _animatedTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Content/Eroica/Tools/ReincarnatedValor_Animated");
            if (_animatedTexture.State != AssetState.Loaded)
                return;

            Texture2D tex = _animatedTexture.Value;
            int cols = 6;
            int rows = 6;
            int frameW = tex.Width / cols;
            int frameH = tex.Height / rows;

            // Use wingFrame from modPlayer - it's 0 when idle, animates when flying
            int frame = modPlayer.wingFrame;
            int col = frame % cols;
            int row = frame / cols;

            Rectangle source = new Rectangle(col * frameW, row * frameH, frameW, frameH);

            Vector2 pos = drawInfo.Position - Main.screenPosition + new Vector2(player.width / 2f, player.height / 2f);
            pos.Y += player.gfxOffY;
            pos = pos.Floor();

            SpriteEffects fx = player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (player.gravDir == -1)
                fx |= SpriteEffects.FlipVertically;

            Color color = drawInfo.colorArmorBody;

            Vector2 origin = new Vector2(source.Width / 2f, source.Height / 2f);

            // Scale reduced by 8% more (0.339f)
            DrawData data = new DrawData(tex, pos, source, color, player.bodyRotation, origin, 0.339f, fx, 0);
            drawInfo.DrawDataCache.Add(data);
        }
    }
}
