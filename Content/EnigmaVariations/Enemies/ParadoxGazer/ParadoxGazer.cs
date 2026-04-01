using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.EnigmaVariations.Enemies
{
    public class ParadoxGazer : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/Enemies/ParadoxGazer/ParadoxGazer";

        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(60, 200, 100);

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
            NPC.damage = 125;
            NPC.defense = 56;
            NPC.lifeMax = 3200;
            NPC.HitSound = SoundID.NPCHit36;
            NPC.DeathSound = SoundID.NPCDeath39;
            NPC.knockBackResist = 0.08f;
            NPC.value = Item.buyPrice(gold: 8);
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
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Jungle,
                new FlavorTextBestiaryInfoElement(
                    "A floating eye of paradox, wreathed in shifting void-purple and eerie green flame. " +
                    "It watches from impossible angles, its gaze unraveling certainty itself.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (NPC.downedMoonlord &&
                spawnInfo.Player.ZoneJungle)
                return 0.05f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MysteryEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            NPC.rotation = NPC.velocity.X * 0.02f;
            float shift = MathF.Sin(Main.GlobalTimeWrappedHourly * 2f);
            float pulse = 0.6f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);

            Color lightColor = Color.Lerp(EnigmaPurple, EnigmaGreen, shift * 0.5f + 0.5f);
            Lighting.AddLight(NPC.Center, lightColor.ToVector3() * 0.4f * pulse);

            if (Main.rand.NextBool(7))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.GreenTorch;
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    dustType, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), 100, default, 0.7f);
                dust.noGravity = true;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 20; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.GreenTorch;
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        dustType, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 60, default, 1.3f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            float flicker = 0.8f + 0.2f * MathF.Sin(Main.GlobalTimeWrappedHourly * 5f);
            return Color.Lerp(drawColor, EnigmaPurple, 0.2f) * flicker;
        }
    }
}
