using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Bosses
{
    /// <summary>
    /// Second movement minion of Eroica's Retribution.
    /// Orbits the main boss and occasionally charges at the player.
    /// </summary>
    public class MovementII : ModNPC
    {
        // Use the Archangel of Eroica sprite
        public override string Texture => "MagnumOpus/Content/Eroica/Bosses/ArchangelOfEroica";

        private int parentBossIndex = -1;
        private float orbitAngle = 0f;
        private const float BaseOrbitRadius = 150f;
        private const float BaseOrbitSpeed = 0.02f;
        private bool isCharging = false;
        
        // Fluid movement variables
        private float waveOffset = 0f;
        private float radiusWobble = 0f;
        private float speedVariation = 0f;
        private Vector2 chargeTarget = Vector2.Zero;

        private float AttackTimer
        {
            get => NPC.ai[0];
            set => NPC.ai[0] = value;
        }

        private float ChargeTimer
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.TrailCacheLength[Type] = 8;
            NPCID.Sets.TrailingMode[Type] = 1;
            
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 50;
            NPC.height = 50;
            NPC.damage = 75; // Higher damage for charging
            NPC.defense = 55; // Increased armor
            NPC.lifeMax = 240254; // Endgame challenge (reduced 15% from original)
            NPC.HitSound = SoundID.NPCHit54;
            NPC.DeathSound = SoundID.NPCDeath52;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = 0;
            NPC.aiStyle = -1;
        }

        public override void AI()
        {
            // Find parent boss
            if (parentBossIndex == -1 || !Main.npc[parentBossIndex].active || 
                Main.npc[parentBossIndex].type != ModContent.NPCType<EroicasRetribution>())
            {
                FindParentBoss();
            }

            if (parentBossIndex == -1)
            {
                NPC.active = false;
                return;
            }

            NPC parentBoss = Main.npc[parentBossIndex];
            Player target = Main.player[parentBoss.target];

            // Pink glow (more intense when charging)
            float glowIntensity = isCharging ? 1.2f : 0.8f;
            Lighting.AddLight(NPC.Center, 0.9f * glowIntensity, 0.4f * glowIntensity, 0.6f * glowIntensity);

            if (isCharging)
            {
                ChargeAttack(target);
            }
            else
            {
                OrbitalBehavior(parentBoss, target);
            }

            // Particle effects
            if (Main.rand.NextBool(isCharging ? 1 : 4))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkTorch, 0f, 0f, 100, default, isCharging ? 2f : 1.2f);
                dust.noGravity = true;
                dust.velocity = isCharging ? -NPC.velocity * 0.2f : Vector2.Zero;
            }

            // Keep sprite stationary (no rotation)
            NPC.rotation = 0f;
        }

        private void OrbitalBehavior(NPC parentBoss, Player target)
        {
            // Update fluid movement variables
            waveOffset += 0.06f;
            radiusWobble = (float)Math.Sin(waveOffset * 0.8f) * 35f;
            speedVariation = (float)Math.Sin(waveOffset * 1.3f) * 0.012f;
            
            // Fluid orbit with varying radius and speed
            float currentOrbitRadius = BaseOrbitRadius + radiusWobble;
            float currentOrbitSpeed = BaseOrbitSpeed + speedVariation;
            orbitAngle += currentOrbitSpeed;
            
            float offsetAngle = MathHelper.TwoPi / 3f; // 120 degrees
            
            // Add weaving motion
            float weaveX = (float)Math.Sin(waveOffset * 2.5f) * 25f;
            float weaveY = (float)Math.Sin(waveOffset * 1.8f) * 18f;
            
            Vector2 orbitPosition = parentBoss.Center + new Vector2(
                (float)Math.Cos(orbitAngle + offsetAngle) * currentOrbitRadius + weaveX,
                (float)Math.Sin(orbitAngle + offsetAngle) * currentOrbitRadius + weaveY
            );

            Vector2 direction = orbitPosition - NPC.Center;
            if (direction.Length() > 5f)
            {
                direction.Normalize();
                float speed = 12f + (float)Math.Sin(waveOffset * 2f) * 4f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * speed, 0.12f);
            }
            else
            {
                NPC.velocity *= 0.8f;
            }

            // Attack timer - charge attack
            AttackTimer++;
            if (AttackTimer >= 240 && target.active && !target.dead)
            {
                AttackTimer = 0;
                isCharging = true;
                ChargeTimer = 0;
                
                // Telegraph - predict where player might go
                chargeTarget = target.Center + target.velocity * 20f;
                
                for (int i = 0; i < 15; i++)
                {
                    Dust telegraph = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.PinkFairy, 0f, 0f, 0, default, 2.5f);
                    telegraph.noGravity = true;
                    telegraph.velocity = Main.rand.NextVector2Circular(6f, 6f);
                }
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
            }
        }

        private void ChargeAttack(Player target)
        {
            ChargeTimer++;
            waveOffset += 0.08f;

            if (ChargeTimer < 30)
            {
                // Wind up with weaving anticipation
                NPC.velocity *= 0.85f;
                NPC.velocity += new Vector2((float)Math.Sin(waveOffset * 5f) * 0.5f, (float)Math.Cos(waveOffset * 4f) * 0.3f);
                
                // Aim at predicted player position with slight adjustment
                chargeTarget = target.Center + target.velocity * 15f;
                Vector2 toTarget = chargeTarget - NPC.Center;
                NPC.rotation = toTarget.ToRotation();
            }
            else if (ChargeTimer == 30)
            {
                // Launch charge with curved approach
                Vector2 direction = (chargeTarget - NPC.Center).SafeNormalize(Vector2.UnitY);
                NPC.velocity = direction * 25f;
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
                EroicaScreenShake.MediumShake(NPC.Center);
                
                for (int i = 0; i < 20; i++)
                {
                    Dust dust = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.PinkTorch, 0f, 0f, 100, default, 2f);
                    dust.noGravity = true;
                    dust.velocity = Main.rand.NextVector2Circular(10f, 10f);
                }
            }
            else if (ChargeTimer > 30 && ChargeTimer < 60)
            {
                // Charging with slight curve toward player (makes it feel more organic)
                Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                NPC.velocity += toPlayer * 0.3f; // Slight homing
                NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * 22f; // Maintain speed
            }
            else if (ChargeTimer >= 60)
            {
                // Graceful slowdown and return
                NPC.velocity *= 0.92f;
                
                if (NPC.velocity.Length() < 2f)
                {
                    isCharging = false;
                    ChargeTimer = 0;
                }
            }
        }

        private void FindParentBoss()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<EroicasRetribution>())
                {
                    parentBossIndex = i;
                    orbitAngle = MathHelper.TwoPi / 3f; // Start at 120 degrees
                    return;
                }
            }
            parentBossIndex = -1;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RemnantOfEroicasTriumph>(), 1, 10, 20));
        }

        public override void OnKill()
        {
            for (int i = 0; i < 30; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkTorch, 0f, 0f, 100, default, 2f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Main.NewText("Movement II has concluded...", 255, 150, 200);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            
            for (int k = 0; k < NPC.oldPos.Length; k++)
            {
                Vector2 drawPos = NPC.oldPos[k] - screenPos + new Vector2(NPC.width / 2, NPC.height / 2);
                Color trailColor = new Color(255, 120, 180, 100) * ((float)(NPC.oldPos.Length - k) / NPC.oldPos.Length);
                float scale = isCharging ? NPC.scale : NPC.scale * 0.8f;
                spriteBatch.Draw(texture, drawPos, null, trailColor, NPC.oldRot[k], drawOrigin, scale * (1f - k * 0.1f), SpriteEffects.None, 0f);
            }

            return true;
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return new Color(255, 160, 200, 200);
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.2f;
            return null;
        }
    }
}
