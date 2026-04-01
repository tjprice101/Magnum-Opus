using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.LaCampanella.Enemies
{
    public class BellforgeWraith : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/Enemies/BellforgeWraith/BellforgeWraith";

        private static readonly Color BellOrange = new Color(255, 140, 40);
        private static readonly Color BellGold = new Color(255, 200, 80);

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new()
            {
                Velocity = 1f
            };
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = bestiaryData;
        }

        public override void SetDefaults()
        {
            NPC.width = 130;
            NPC.height = 77;
            NPC.damage = 105;
            NPC.defense = 48;
            NPC.lifeMax = 2200;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0.1f;
            NPC.value = Item.buyPrice(gold: 6, silver: 50);
            NPC.aiStyle = NPCAIStyleID.HoveringFighter;
            AIType = NPCID.Pixie;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.lavaImmune = true;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement(
                    "A wraith forged in infernal bell-fire, its form wreathed in black smoke and crackling orange flames. " +
                    "The distant toll of a burning bell echoes in its wake.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (NPC.downedMoonlord &&
                spawnInfo.Player.ZoneDesert)
                return 0.055f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BellEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            NPC.rotation = NPC.velocity.X * 0.015f;
            float flicker = Main.rand.NextFloat(0.9f, 1.1f);
            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(NPC.Center, BellOrange.ToVector3() * 0.5f * pulse * flicker);

            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Torch, Main.rand.NextFloat(-1.5f, 1.5f), -1f, 80, default, 0.8f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Smoke, Main.rand.NextFloat(-0.5f, 0.5f), -0.8f, 150, Color.Black, 1f);
                dust.noGravity = true;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 25; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.Torch, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 60, default, 1.4f);
                }
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.Smoke, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 1f), 180, Color.Black, 1.2f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(drawColor, BellGold, 0.2f);
        }
    }
}
