using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class ReincarnatedValorPlayer : ModPlayer
    {
        public int wingFrame = 0;
        private int frameCounter = 0;
        private bool wasFlying = false;  // Track previous flying state
        
        public bool hasWingsEquipped = false;
        private int dodgeCooldown = 0;
        private const int DodgeCooldownMax = 25;
        private const float DodgeSpeed = 28f;
        private bool isDodging = false;
        private int dodgeTimer = 0;
        private const int DodgeDuration = 8;

        public override void ResetEffects()
        {
            hasWingsEquipped = false;
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

            // Dodge cooldown tick
            if (dodgeCooldown > 0)
                dodgeCooldown--;
            
            // Handle dodge input - right click
            if (Main.mouseRight && Main.mouseRightRelease && dodgeCooldown <= 0 && !isDodging)
            {
                PerformDodge();
            }
            
            // Handle ongoing dodge
            if (isDodging)
            {
                dodgeTimer++;
                Player.immune = true;
                Player.immuneTime = 2;
                Player.immuneNoBlink = true; // Don't blink during dodge
                
                // Enhanced scarlet trail with new particle system
                ThemedParticles.DodgeTrail(Player.Center, Player.velocity, false);
                
                // Additional dust trail
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
                    
                    // End dodge burst
                    ThemedParticles.EroicaImpact(Player.Center, 1f);
                }
            }

            // Wing animation logic - matches WingsOfTheMoon behavior exactly
            // Flying = actively holding jump while in the air with wings active
            bool isFlying = Player.controlJump && Player.velocity.Y != 0 && !Player.mount.Active;
            bool isOnGround = Player.velocity.Y == 0;
            
            if (isFlying || isDodging)
            {
                // Animate through 6x6 sprite sheet
                frameCounter++;
                if (frameCounter >= 2) // Fast animation
                {
                    frameCounter = 0;
                    wingFrame++;
                    if (wingFrame >= 36) // 6x6 = 36 frames
                        wingFrame = 0;
                }
                wasFlying = true;
            }
            else if (isOnGround)
            {
                // On ground - reset to first frame (idle/folded)
                wingFrame = 0;
                frameCounter = 0;
                wasFlying = false;
            }
            else if (wasFlying && !isFlying)
            {
                // Stopped flying but still in air - hold on first frame until landing
                // This deactivates animation but keeps wings visible
                wingFrame = 0;
                frameCounter = 0;
                // Keep wasFlying true until landing
            }
        }

        private void PerformDodge()
        {
            isDodging = true;
            dodgeTimer = 0;
            dodgeCooldown = DodgeCooldownMax;
            
            Vector2 dodgeDirection = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.UnitX);
            Player.velocity = dodgeDirection * DodgeSpeed;
            
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.3f, Volume = 0.8f }, Player.Center);
            
            // Enhanced burst with new themed particle system
            ThemedParticles.EroicaImpact(Player.Center, 1.5f);
            ThemedParticles.TeleportBurst(Player.Center, false);
        }
        
        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "ReincarnatedValor", EquipType.Wings);
            if (Player.wings == wingSlot && wingSlot > 0)
            {
                // Hide vanilla wing drawing - we do custom drawing
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
