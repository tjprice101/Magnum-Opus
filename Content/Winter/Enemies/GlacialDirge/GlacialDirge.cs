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
    public class GlacialDirge : ModNPC
    {
        private static readonly Color WinterFrost = new Color(100, 160, 220);
        private static readonly Color WinterBlue = new Color(180, 220, 255);

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
            NPC.damage = 65;
            NPC.defense = 28;
            NPC.lifeMax = 800;
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.2f;
            NPC.value = Item.buyPrice(gold: 1, silver: 50);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            AIType = NPCID.GraniteGolem;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Snow,
                new FlavorTextBestiaryInfoElement(
                    "An ancient beast of glacial bone and heavy permafrost armor. " +
                    "Frozen horns crackle with rime as it drags itself through subzero caverns.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneSnow &&
                (spawnInfo.Player.ZoneOverworldHeight || spawnInfo.Player.ZoneRockLayerHeight) &&
                Main.hardMode)
                return 0.06f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FrostEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2f);
            Lighting.AddLight(NPC.Center, WinterFrost.ToVector3() * 0.3f * pulse);

            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.IceTorch, Main.rand.NextFloat(-0.5f, 0.5f), -0.8f, 120, default, 0.9f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Snow, Main.rand.NextFloat(-0.3f, 0.3f), 0.3f, 100, default, 0.6f);
                dust.noGravity = false;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 18; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.IceTorch, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 60, default, 1.3f);
                }
                for (int i = 0; i < 8; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.Snow, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 1f), 80, default, 1.1f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(drawColor, WinterBlue, 0.15f);
        }
    }
}
