using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Spring.Materials;

namespace MagnumOpus.Content.Spring.Enemies
{
    public class Pollengust : ModNPC
    {
        private static readonly Color SpringCream = new Color(255, 245, 200);
        private static readonly Color SpringRose = new Color(180, 120, 150);

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
            NPC.height = 80;
            NPC.damage = 40;
            NPC.defense = 14;
            NPC.lifeMax = 350;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.35f;
            NPC.value = Item.buyPrice(silver: 55);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            AIType = NPCID.Pixie;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement(
                    "A creature of crystallized pollen and pale jade, awakened by the first notes of spring. " +
                    "Seed pods and butterfly wings swirl in its wake as it drifts through flowering meadows.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneOverworldHeight &&
                !spawnInfo.Player.ZoneDesert && !spawnInfo.Player.ZoneSnow &&
                (spawnInfo.Player.ZoneForest || spawnInfo.Player.ZoneHallow) &&
                Main.hardMode)
                return 0.07f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BlossomEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            Lighting.AddLight(NPC.Center, SpringCream.ToVector3() * 0.25f);

            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.GreenFairy, NPC.velocity.X * 0.3f, -0.8f, 100, default, 0.7f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(18))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.PinkFairy, Main.rand.NextFloat(-1f, 1f), -0.3f, 120, default, 0.5f);
                dust.noGravity = true;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 12; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.GreenFairy, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 80, default, 1f);
                }
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.PinkFairy, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-4f, 0f), 100, default, 0.9f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(drawColor, Color.White, 0.1f);
        }
    }
}
