using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Bosses
{
    /// <summary>
    /// Third movement minion of Eroica's Retribution.
    /// Orbits the main boss and shoots pink flaming bolts at the player.
    /// </summary>
    public class MovementIII : ModNPC
    {
        // Use the Archangel of Eroica sprite
        public override string Texture => "MagnumOpus/Content/Eroica/Bosses/ArchangelOfEroica";

        private int parentBossIndex = -1;
        private float orbitAngle = 0f;
        private const float BaseOrbitRadius = 150f;
        private const float BaseOrbitSpeed = 0.02f;
        
        // Fluid movement variables
        private float waveOffset = 0f;
        private float radiusWobble = 0f;
        private float speedVariation = 0f;
        private float divePhase = 0f;
        private bool isDiving = false;

        private float AttackTimer
        {
            get => NPC.ai[0];
            set => NPC.ai[0] = value;
        }

        private float BurstCount
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.TrailCacheLength[Type] = 6;
            NPCID.Sets.TrailingMode[Type] = 1;
            
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 50;
            NPC.height = 50;
            NPC.damage = 55;
            NPC.defense = 65; // Increased armor
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

            // Orange-pink glow (fire theme)
            Lighting.AddLight(NPC.Center, 1f, 0.5f, 0.6f);

            // Update fluid movement variables
            waveOffset += 0.055f;
            radiusWobble = (float)Math.Sin(waveOffset * 0.9f) * 40f;
            speedVariation = (float)Math.Sin(waveOffset * 1.1f) * 0.015f;
            
            // Fluid orbit with fire-like flickering motion
            float currentOrbitRadius = BaseOrbitRadius + radiusWobble;
            float currentOrbitSpeed = BaseOrbitSpeed + speedVariation;
            orbitAngle += currentOrbitSpeed;
            
            float offsetAngle = MathHelper.TwoPi * 2f / 3f; // 240 degrees
            
            // Fire-like flickering weave
            float flickerX = (float)Math.Sin(waveOffset * 4f) * 15f + (float)Math.Sin(waveOffset * 7f) * 8f;
            float flickerY = (float)Math.Sin(waveOffset * 3f) * 12f + (float)Math.Cos(waveOffset * 5f) * 6f;
            
            Vector2 orbitPosition = parentBoss.Center + new Vector2(
                (float)Math.Cos(orbitAngle + offsetAngle) * currentOrbitRadius + flickerX,
                (float)Math.Sin(orbitAngle + offsetAngle) * currentOrbitRadius + flickerY
            );
            
            // Occasional dive-bomb toward player while shooting
            if (!isDiving && Main.rand.NextBool(400) && target.active && !target.dead)
            {
                isDiving = true;
                divePhase = 0f;
            }
            
            if (isDiving)
            {
                divePhase++;
                if (divePhase < 25)
                {
                    // Dive toward player
                    Vector2 diveDir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                    NPC.velocity = Vector2.Lerp(NPC.velocity, diveDir * 20f, 0.15f);
                }
                else if (divePhase < 50)
                {
                    // Pull away with arc
                    Vector2 awayDir = (parentBoss.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                    awayDir += new Vector2((float)Math.Sin(divePhase * 0.2f) * 0.5f, -0.3f);
                    NPC.velocity = Vector2.Lerp(NPC.velocity, awayDir.SafeNormalize(Vector2.UnitY) * 16f, 0.12f);
                }
                else
                {
                    isDiving = false;
                }
            }
            else
            {
                // Normal fluid orbital movement
                Vector2 direction = orbitPosition - NPC.Center;
                if (direction.Length() > 5f)
                {
                    direction.Normalize();
                    float speed = 12f + (float)Math.Sin(waveOffset * 2.5f) * 4f;
                    NPC.velocity = Vector2.Lerp(NPC.velocity, direction * speed, 0.11f);
                }
                else
                {
                    NPC.velocity *= 0.8f;
                }
            }

            // Fire particles
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkTorch, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            // Flame particles
            if (Main.rand.NextBool(5))
            {
                Dust flame = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, new Color(255, 100, 150), 1.2f);
                flame.noGravity = true;
                flame.velocity = new Vector2(0, -1f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
            }

            // Attack logic - shoot pink flaming bolts
            AttackTimer++;
            if (AttackTimer >= 150 && target.active && !target.dead && Main.netMode != NetmodeID.MultiplayerClient) // Every 2.5 seconds
            {
                // Fire 3 bolts in quick succession
                if (BurstCount < 3)
                {
                    if (AttackTimer % 15 == 0) // One bolt every 0.25 seconds
                    {
                        // Visual telegraph
                        for (int i = 0; i < 8; i++)
                        {
                            Dust telegraph = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.Torch, 0f, 0f, 100, new Color(255, 100, 150), 2f);
                            telegraph.noGravity = true;
                            telegraph.velocity = Main.rand.NextVector2Circular(4f, 4f);
                        }

                        // Predict player position slightly for more interesting dodging
                        Vector2 predictedPos = target.Center + target.velocity * 10f;
                        Vector2 velocity = (predictedPos - NPC.Center).SafeNormalize(Vector2.UnitY) * 14f;
                        
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, 
                            ModContent.ProjectileType<PinkFlamingBolt>(), 70, 2f, Main.myPlayer);

                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
                        EroicaScreenShake.SmallShake(NPC.Center);
                        BurstCount++;
                    }
                }
                else
                {
                    // Reset for next burst
                    AttackTimer = 0;
                    BurstCount = 0;
                }
            }

            // Keep sprite stationary (no rotation)
            NPC.rotation = 0f;
        }

        private void FindParentBoss()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<EroicasRetribution>())
                {
                    parentBossIndex = i;
                    orbitAngle = MathHelper.TwoPi * 2f / 3f; // Start at 240 degrees
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
            // Fire burst
            for (int i = 0; i < 30; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkTorch, 0f, 0f, 100, default, 2f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }

            for (int i = 0; i < 15; i++)
            {
                Dust flame = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, new Color(255, 100, 150), 1.8f);
                flame.noGravity = true;
                flame.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Main.NewText("Movement III has concluded...", 255, 150, 200);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            
            for (int k = 0; k < NPC.oldPos.Length; k++)
            {
                Vector2 drawPos = NPC.oldPos[k] - screenPos + new Vector2(NPC.width / 2, NPC.height / 2);
                Color trailColor = new Color(255, 100, 150, 100) * ((float)(NPC.oldPos.Length - k) / NPC.oldPos.Length);
                spriteBatch.Draw(texture, drawPos, null, trailColor, NPC.oldRot[k], drawOrigin, NPC.scale * 0.8f, SpriteEffects.None, 0f);
            }

            return true;
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return new Color(255, 140, 180, 200);
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.2f;
            return null;
        }
    }
}
