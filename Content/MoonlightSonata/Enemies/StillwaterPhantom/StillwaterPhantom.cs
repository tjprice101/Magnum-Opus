using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.MoonlightSonata.Enemies
{
    public class StillwaterPhantom : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Enemies/StillwaterPhantom/StillwaterPhantom";

        private static readonly Color MoonlightViolet = new Color(120, 80, 180);
        private static readonly Color MoonlightSilver = new Color(180, 190, 220);

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
            NPC.damage = 95;
            NPC.defense = 44;
            NPC.lifeMax = 2000;
            NPC.HitSound = SoundID.NPCHit36;
            NPC.DeathSound = SoundID.NPCDeath39;
            NPC.knockBackResist = 0.12f;
            NPC.value = Item.buyPrice(gold: 5, silver: 50);
            NPC.aiStyle = NPCAIStyleID.HoveringFighter;
            AIType = NPCID.Pixie;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement(
                    "A translucent phantom of still waters and sorrowful moonlight. " +
                    "It drifts silently above the ground, trailing wisps of silver mist.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (NPC.downedMoonlord &&
                !Main.dayTime &&
                spawnInfo.Player.ZoneOverworldHeight)
                return 0.05f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LunarEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            NPC.rotation = NPC.velocity.X * 0.02f;
            float pulse = 0.6f + 0.4f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2f);
            Lighting.AddLight(NPC.Center, MoonlightViolet.ToVector3() * 0.35f * pulse);

            if (Main.rand.NextBool(6))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.PurpleTorch, Main.rand.NextFloat(-0.8f, 0.8f), -0.5f, 120, default, 0.6f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(NPC.Center + Main.rand.NextVector2Circular(NPC.width / 2, NPC.height / 2),
                    4, 4, DustID.DungeonSpirit, 0f, 0f, 150, default, 0.5f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 22; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.PurpleTorch, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 80, default, 1.2f);
                }
                for (int i = 0; i < 12; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.DungeonSpirit, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 0.8f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            float alpha = 0.7f + 0.15f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            return Color.Lerp(drawColor, MoonlightSilver, 0.3f) * alpha;
        }
    }
}
