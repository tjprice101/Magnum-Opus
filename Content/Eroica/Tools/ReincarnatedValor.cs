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
            
            // Ambient scarlet particles when flying
            if (!hideVisual && player.velocity.Y != 0 && Main.rand.NextBool(8))
            {
                Dust flame = Dust.NewDustDirect(player.position, player.width, player.height, 
                    DustID.CrimsonTorch, 0f, 2f, 100, default, 1.0f);
                flame.noGravity = true;
                flame.velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), 2f);
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
                
                // Scarlet trail particles
                for (int i = 0; i < 3; i++)
                {
                    Dust trail = Dust.NewDustDirect(Player.position, Player.width, Player.height, 
                        DustID.CrimsonTorch, -Player.velocity.X * 0.2f, -Player.velocity.Y * 0.2f, 100, default, 1.4f);
                    trail.noGravity = true;
                }
                
                if (dodgeTimer >= DodgeDuration)
                {
                    isDodging = false;
                    dodgeTimer = 0;
                }
            }

            // Wing animation - flying when in air with wings active
            bool flying = Player.controlJump && Player.velocity.Y != 0 && !Player.mount.Active;
            
            if (flying || isDodging)
            {
                frameCounter++;
                if (frameCounter >= 2) // Fast animation like moonlight wings
                {
                    frameCounter = 0;
                    wingFrame++;
                    if (wingFrame >= 36) // 6x6 = 36 frames
                        wingFrame = 0;
                }
            }
            else
            {
                wingFrame = 0;
                frameCounter = 0;
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
            
            // Scarlet burst particles
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 4f;
                Dust burst = Dust.NewDustPerfect(Player.Center, DustID.CrimsonTorch, vel, 0, default, 1.5f);
                burst.noGravity = true;
            }
            
            for (int i = 0; i < 10; i++)
            {
                Dust smoke = Dust.NewDustDirect(Player.position, Player.width, Player.height, 
                    DustID.Smoke, 0f, 0f, 150, Color.Black, 1.2f);
                smoke.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
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
    /// Uses 6x6 sprite sheet for flying animation.
    /// </summary>
    public class ReincarnatedValorLayer : PlayerDrawLayer
    {
        private Asset<Texture2D> _wingsTexture;    // _Wings.png - stationary (1x4)
        private Asset<Texture2D> _animatedTexture; // Main folder sprite sheet (6x6)

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

            Texture2D tex;
            Rectangle source;

            bool flying = modPlayer.wingFrame > 0;

            if (flying)
            {
                // FLYING: Use 6x6 sprite sheet
                _animatedTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Content/Eroica/Tools/ReincarnatedValor_Animated");
                if (_animatedTexture.State != AssetState.Loaded)
                    return;

                tex = _animatedTexture.Value;
                int cols = 6;
                int rows = 6;
                int frameW = tex.Width / cols;
                int frameH = tex.Height / rows;

                int frame = modPlayer.wingFrame;
                int col = frame % cols;
                int row = frame / cols;

                source = new Rectangle(col * frameW, row * frameH, frameW, frameH);
            }
            else
            {
                // STATIONARY: Use _Wings.png (1x4)
                _wingsTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Content/Eroica/Tools/ReincarnatedValor_Wings");
                if (_wingsTexture.State != AssetState.Loaded)
                    return;

                tex = _wingsTexture.Value;
                int frameH = tex.Height / 4;
                source = new Rectangle(0, 0, tex.Width, frameH);
            }

            Vector2 pos = drawInfo.Position - Main.screenPosition + new Vector2(player.width / 2f, player.height / 2f);
            pos.Y += player.gfxOffY;
            pos = pos.Floor();

            SpriteEffects fx = player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (player.gravDir == -1)
                fx |= SpriteEffects.FlipVertically;

            Color color = drawInfo.colorArmorBody;

            Vector2 origin = new Vector2(source.Width / 2f, source.Height / 2f);

            DrawData data = new DrawData(tex, pos, source, color, player.bodyRotation, origin, 1f, fx, 0);
            drawInfo.DrawDataCache.Add(data);
        }
    }
}
