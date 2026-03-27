using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Winter.Materials;

namespace MagnumOpus.Content.Winter.Enemies
{
    public class Frostwalker : ModNPC
    {
        private static readonly Color WinterIce = new Color(140, 200, 240);
        private static readonly Color WinterCrystal = new Color(240, 248, 255);

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
            NPC.damage = 60;
            NPC.defense = 25;
            NPC.lifeMax = 750;
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath7;
            NPC.knockBackResist = 0.25f;
            NPC.value = Item.buyPrice(gold: 1, silver: 20);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            AIType = NPCID.IceElemental;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Snow,
                new FlavorTextBestiaryInfoElement(
                    "A crystalline wind elemental of translucent ice and pale silver steel. " +
                    "Snowflakes swirl around its angular lattice frame as it glides through frozen wastes.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneSnow &&
                (spawnInfo.Player.ZoneOverworldHeight || spawnInfo.Player.ZoneRockLayerHeight) &&
                Main.hardMode)
                return 0.07f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FrostEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2.5f);
            Lighting.AddLight(NPC.Center, WinterIce.ToVector3() * 0.35f * pulse);

            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.IceTorch, Main.rand.NextFloat(-1f, 1f), -1f, 100, default, 0.7f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Snow, Main.rand.NextFloat(-0.5f, 0.5f), 0.5f, 80, default, 0.8f);
                dust.noGravity = false;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 15; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.IceTorch, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 80, default, 1.2f);
                }
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.Snow, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-2f, 2f), 60, default, 1f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(drawColor, WinterCrystal, 0.2f);
        }
    }
}
