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
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Eroica.Bosses
{
    /// <summary>
    /// Flames of Valor - minion of Eroica, God of Valor.
    /// Orbits the main boss and spawns Energy of Eroica projectiles at the player.
    /// </summary>
    public class MovementI : ModNPC
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
        private float swoopPhase = 0f;
        private bool isSwooping = false;

        private float AttackTimer
        {
            get => NPC.ai[0];
            set => NPC.ai[0] = value;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.TrailCacheLength[Type] = 6;
            NPCID.Sets.TrailingMode[Type] = 1;
            
            // Immune to debuffs
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 58;
            NPC.height = 58;
            NPC.damage = 60;
            NPC.defense = 60; // Increased armor
            NPC.lifeMax = 240254; // Endgame challenge (reduced 15% from original)
            NPC.HitSound = SoundID.NPCHit54;
            NPC.DeathSound = SoundID.NPCDeath52;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = 0;
            NPC.aiStyle = -1;
            NPC.dontTakeDamage = false;
            NPC.scale = 0.35f; // 35% of original size
        }

        public override void AI()
        {
            // Find parent boss
            if (parentBossIndex == -1 || !Main.npc[parentBossIndex].active || 
                Main.npc[parentBossIndex].type != ModContent.NPCType<EroicasRetribution>())
            {
                FindParentBoss();
            }

            // If no parent, despawn
            if (parentBossIndex == -1)
            {
                NPC.active = false;
                return;
            }

            NPC parentBoss = Main.npc[parentBossIndex];
            
            // Update fluid movement variables
            waveOffset += 0.05f;
            radiusWobble = (float)Math.Sin(waveOffset * 0.7f) * 30f;
            speedVariation = (float)Math.Sin(waveOffset * 1.2f) * 0.01f;
            
            // Fluid orbit around the boss with varying radius and speed
            float currentOrbitRadius = BaseOrbitRadius + radiusWobble;
            float currentOrbitSpeed = BaseOrbitSpeed + speedVariation;
            orbitAngle += currentOrbitSpeed;
            
            float offsetAngle = 0f; // First position
            
            // Add figure-8 wobble to make movement more organic
            float wobbleX = (float)Math.Sin(waveOffset * 2f) * 20f;
            float wobbleY = (float)Math.Sin(waveOffset * 1.5f) * 15f;
            
            Vector2 orbitPosition = parentBoss.Center + new Vector2(
                (float)Math.Cos(orbitAngle + offsetAngle) * currentOrbitRadius + wobbleX,
                (float)Math.Sin(orbitAngle + offsetAngle) * currentOrbitRadius + wobbleY
            );
            
            // Occasional swoop toward player
            Player target = Main.player[parentBoss.target];
            if (!isSwooping && Main.rand.NextBool(300) && target.active && !target.dead)
            {
                isSwooping = true;
                swoopPhase = 0f;
            }
            
            if (isSwooping)
            {
                swoopPhase++;
                if (swoopPhase < 30)
                {
                    // Swoop toward player
                    Vector2 swoopDir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                    NPC.velocity = Vector2.Lerp(NPC.velocity, swoopDir * 18f, 0.12f);
                }
                else if (swoopPhase < 60)
                {
                    // Return to orbit
                    Vector2 returnDir = (orbitPosition - NPC.Center).SafeNormalize(Vector2.UnitY);
                    NPC.velocity = Vector2.Lerp(NPC.velocity, returnDir * 15f, 0.1f);
                }
                else
                {
                    isSwooping = false;
                }
            }
            else
            {
                // Normal fluid orbital movement
                Vector2 direction = orbitPosition - NPC.Center;
                if (direction.Length() > 5f)
                {
                    direction.Normalize();
                    float speed = 12f + (float)Math.Sin(waveOffset * 3f) * 3f;
                    NPC.velocity = Vector2.Lerp(NPC.velocity, direction * speed, 0.1f);
                }
                else
                {
                    NPC.velocity *= 0.8f;
                }
            }

            // Pink glow
            Lighting.AddLight(NPC.Center, 0.8f, 0.3f, 0.5f);

            // Particle trail
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkTorch, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            // Attack logic - spawn Energy of Eroica
            AttackTimer++;
            if (AttackTimer >= 180 && Main.netMode != NetmodeID.MultiplayerClient) // Every 3 seconds
            {
                AttackTimer = 0;
                
                Player attackTarget = Main.player[parentBoss.target];
                if (attackTarget.active && !attackTarget.dead)
                {
                    // Visual telegraph
                    for (int i = 0; i < 10; i++)
                    {
                        Dust telegraph = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.GoldFlame, 0f, 0f, 0, default, 2f);
                        telegraph.noGravity = true;
                        telegraph.velocity = Main.rand.NextVector2Circular(5f, 5f);
                    }

                    // Spawn Energy of Eroica projectile
                    Vector2 velocity = (attackTarget.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 6f;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, 
                        ModContent.ProjectileType<EnergyOfEroica>(), 80, 2f, Main.myPlayer, attackTarget.whoAmI);

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item117, NPC.Center);
                    EroicaScreenShake.SmallShake(NPC.Center);
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
                    orbitAngle = 0f; // Movement I starts at 0 degrees
                    return;
                }
            }
            parentBossIndex = -1;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Drop 10-20 Remnant of Eroica's Triumph
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RemnantOfEroicasTriumph>(), 1, 10, 20));
        }

        public override void OnKill()
        {
            // Enhanced death burst with multi-layer bloom
            UnifiedVFXBloom.Eroica.ImpactEnhanced(NPC.Center, 1.5f);
            EnhancedThemedParticles.EroicaBloomBurstEnhanced(NPC.Center, 1.2f);
            
            // Radial bloom flares
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 30f;
                Color flareColor = Color.Lerp(new Color(255, 150, 200), new Color(255, 215, 0), (float)i / 6f);
                EnhancedParticles.BloomFlare(NPC.Center + offset, flareColor, 0.4f, 18, 3, 0.8f);
            }

            // Notify boss that this minion is dead
            if (parentBossIndex != -1 && Main.npc[parentBossIndex].active)
            {
                // The boss will check minion count in its AI
            }

            // Chat message
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Main.NewText("Movement I has concluded...", 255, 150, 200);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Draw with pink tint
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            
            // Trail
            for (int k = 0; k < NPC.oldPos.Length; k++)
            {
                Vector2 drawPos = NPC.oldPos[k] - screenPos + new Vector2(NPC.width / 2, NPC.height / 2);
                Color trailColor = new Color(255, 150, 200, 100) * ((float)(NPC.oldPos.Length - k) / NPC.oldPos.Length);
                spriteBatch.Draw(texture, drawPos, null, trailColor, NPC.rotation, drawOrigin, NPC.scale * 0.8f, SpriteEffects.None, 0f);
            }

            return true;
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return new Color(255, 180, 220, 200);
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.2f;
            return null;
        }
    }
}
