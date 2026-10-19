using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NivBot.Migrations
{
    /// <inheritdoc />
    public partial class RolesPointsCompetitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityRanks");

            migrationBuilder.DropTable(
                name: "SkillRanks");

            migrationBuilder.DropIndex(
                name: "IX_DiscordRoles_ActivityId",
                table: "DiscordRoles");

            migrationBuilder.AddColumn<int>(
                name: "OverallLevel",
                table: "RunescapeAccounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "OverallXp",
                table: "RunescapeAccounts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSkippedAt",
                table: "GoodplaceSkillTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSkippedAt",
                table: "GoodplaceActivityTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Threshold",
                table: "DiscordRoles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "ActivityId",
                table: "DiscordRoles",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "DiscordRoleId",
                table: "DiscordRoles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "CloseTrigger",
                table: "DiscordRoles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Emoji",
                table: "DiscordRoles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GuildId",
                table: "DiscordRoles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "DiscordRoles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Metric",
                table: "DiscordRoles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "DiscordRoles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Skill",
                table: "DiscordRoles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TierGroup",
                table: "DiscordRoles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ChannelId",
                table: "CompetitionProvidersDetails",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedAt",
                table: "CompetitionProvidersDetails",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GuildId",
                table: "CompetitionProvidersDetails",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "CompetitionProvidersDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Bosses",
                columns: table => new
                {
                    OsrsName = table.Column<string>(type: "text", nullable: false),
                    KillsPerHour = table.Column<double>(type: "double precision", nullable: false),
                    MinimumKc = table.Column<int>(type: "integer", nullable: false),
                    IsRaid = table.Column<bool>(type: "boolean", nullable: false),
                    WomMetric = table.Column<string>(type: "text", nullable: true),
                    TempleMetric = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bosses", x => x.OsrsName);
                    table.CheckConstraint("CK_Boss_KillsPerHour", "\"KillsPerHour\" >= 0");
                    table.CheckConstraint("CK_Boss_MinimumKc", "\"MinimumKc\" >= 0");
                    table.CheckConstraint("CK_Boss_OsrsName", "length(\"OsrsName\") <= 100");
                });

            migrationBuilder.CreateTable(
                name: "RoleProgressNotices",
                columns: table => new
                {
                    GoodplaceUserId = table.Column<int>(type: "integer", nullable: false),
                    DiscordRoleId = table.Column<long>(type: "bigint", nullable: false),
                    NotifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleProgressNotices", x => new { x.GoodplaceUserId, x.DiscordRoleId });
                    table.ForeignKey(
                        name: "FK_RoleProgressNotices_DiscordRoles_DiscordRoleId",
                        column: x => x.DiscordRoleId,
                        principalTable: "DiscordRoles",
                        principalColumn: "DiscordRoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleProgressNotices_GoodplaceUsers_GoodplaceUserId",
                        column: x => x.GoodplaceUserId,
                        principalTable: "GoodplaceUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkillRates",
                columns: table => new
                {
                    Skill = table.Column<int>(type: "integer", nullable: false),
                    StartXp = table.Column<int>(type: "integer", nullable: false),
                    XpPerHour = table.Column<int>(type: "integer", nullable: false),
                    Method = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillRates", x => new { x.Skill, x.StartXp });
                    table.CheckConstraint("CK_SkillRate_StartXp", "\"StartXp\" >= 0 AND \"StartXp\" <= 200000000");
                    table.CheckConstraint("CK_SkillRate_XpPerHour", "\"XpPerHour\" >= 0");
                    table.ForeignKey(
                        name: "FK_SkillRates_Skills_Skill",
                        column: x => x.Skill,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Bosses",
                columns: new[] { "OsrsName", "IsRaid", "KillsPerHour", "MinimumKc", "TempleMetric", "WomMetric" },
                values: new object[,]
                {
                    { "Abyssal Sire", false, 50.0, 5, "sire", "abyssal_sire" },
                    { "Alchemical Hydra", false, 30.0, 5, "hydra", "alchemical_hydra" },
                    { "Amoxliatl", false, 85.0, 5, "amoxliatl", "amoxliatl" },
                    { "Araxxor", false, 40.0, 5, "araxxor", "araxxor" },
                    { "Artio", false, 60.0, 5, "artio", "artio" },
                    { "Barrows Chests", false, 22.0, 5, "barrows", "barrows_chests" },
                    { "Brutus", false, 250.0, 5, "brutus", "brutus" },
                    { "Bryophyta", false, 9.0, 5, "bryophyta", "bryophyta" },
                    { "Callisto", false, 142.0, 5, "callisto", "callisto" },
                    { "Calvar'ion", false, 55.0, 5, "calvarion", "calvarion" },
                    { "Cerberus", false, 65.0, 5, "cerberus", "cerberus" },
                    { "Chambers of Xeric", true, 3.5, 5, "cox", "chambers_of_xeric" },
                    { "Chambers of Xeric: Challenge Mode", true, 3.0, 5, "coxcm", "chambers_of_xeric_challenge_mode" },
                    { "Chaos Elemental", false, 120.0, 5, "chaoselemental", "chaos_elemental" },
                    { "Chaos Fanatic", false, 100.0, 5, "chaosfanatic", "chaos_fanatic" },
                    { "Commander Zilyana", false, 60.0, 5, "commanderzilyana", "commander_zilyana" },
                    { "Corporeal Beast", false, 60.0, 5, "corporealbeast", "corporeal_beast" },
                    { "Crazy Archaeologist", false, 75.0, 5, "crazyarchaeologist", "crazy_archaeologist" },
                    { "Dagannoth Prime", false, 105.0, 5, "prime", "dagannoth_prime" },
                    { "Dagannoth Rex", false, 105.0, 5, "rexbro", "dagannoth_rex" },
                    { "Dagannoth Supreme", false, 105.0, 5, "supreme", "dagannoth_supreme" },
                    { "Deranged Archaeologist", false, 80.0, 5, "derangedarchaeologist", "deranged_archaeologist" },
                    { "Doom of Mokhaiotl", false, 20.0, 5, "mokhaiotl", "doom_of_mokhaiotl" },
                    { "Duke Sucellus", false, 39.0, 5, "duke", "duke_sucellus" },
                    { "General Graardor", false, 58.0, 5, "generalgraardor", "general_graardor" },
                    { "Giant Mole", false, 125.0, 5, "giantmole", "giant_mole" },
                    { "Grotesque Guardians", false, 37.0, 5, "grotesqueguardians", "grotesque_guardians" },
                    { "Hespori", false, 60.0, 5, "hespori", "hespori" },
                    { "K'ril Tsutsaroth", false, 65.0, 5, "kriltsutsaroth", "kril_tsutsaroth" },
                    { "Kalphite Queen", false, 55.0, 5, "kq", "kalphite_queen" },
                    { "King Black Dragon", false, 130.0, 5, "kbd", "king_black_dragon" },
                    { "Kraken", false, 100.0, 5, "kraken", "kraken" },
                    { "Kree'Arra", false, 40.0, 5, "kreearra", "kreearra" },
                    { "Lunar Chests", false, 18.0, 5, "lunarchests", "lunar_chests" },
                    { "Mad Angel", false, 60.0, 5, "angel", "mad_angel" },
                    { "Maggot King", false, 30.0, 5, "maggotking", "maggot_king" },
                    { "Mimic", false, 60.0, 1, "mimic", "mimic" },
                    { "Nex", false, 23.5, 5, "nex", "nex" },
                    { "Nightmare", false, 14.0, 5, "nightmare", "nightmare" },
                    { "Obor", false, 12.0, 5, "obor", "obor" },
                    { "Phantom Muspah", false, 30.0, 5, "muspah", "phantom_muspah" },
                    { "Phosani's Nightmare", false, 9.5999999999999996, 5, "phosanis", "phosanis_nightmare" },
                    { "Sarachnis", false, 110.0, 5, "sarachnis", "sarachnis" },
                    { "Scorpia", false, 130.0, 5, "scorpia", "scorpia" },
                    { "Scurrius", false, 70.0, 5, "scurrius", "scurrius" },
                    { "Shellbane Gryphon", false, 95.0, 5, "shellbane", "shellbane_gryphon" },
                    { "Skotizo", false, 45.0, 5, "skotizo", "skotizo" },
                    { "Sol Heredit", false, 2.7000000000000002, 5, "solheredit", "sol_heredit" },
                    { "Spindel", false, 55.0, 5, "spindel", "spindel" },
                    { "Tempoross", false, 0.0, 5, "tempoross", "tempoross" },
                    { "The Corrupted Gauntlet", false, 7.2000000000000002, 5, "gauntlethard", "the_corrupted_gauntlet" },
                    { "The Gauntlet", false, 10.0, 5, "gauntlet", "the_gauntlet" },
                    { "The Hueycoatl", false, 20.0, 5, "hueycoatl", "the_hueycoatl" },
                    { "The Leviathan", false, 31.0, 5, "leviathan", "the_leviathan" },
                    { "The Royal Titans", false, 55.0, 5, "titans", "the_royal_titans" },
                    { "The Whisperer", false, 22.0, 5, "whisperer", "the_whisperer" },
                    { "Theatre of Blood", true, 3.2000000000000002, 5, "tob", "theatre_of_blood" },
                    { "Theatre of Blood: Hard Mode", true, 3.0, 5, "tobcm", "theatre_of_blood_hard_mode" },
                    { "Thermonuclear Smoke Devil", false, 150.0, 5, "smokedevil", "thermonuclear_smoke_devil" },
                    { "Tombs of Amascut", true, 3.7000000000000002, 5, "toa", "tombs_of_amascut" },
                    { "Tombs of Amascut: Expert Mode", true, 3.0, 5, "toae", "tombs_of_amascut_expert" },
                    { "TzKal-Zuk", false, 1.0, 1, "zuk", "tzkal_zuk" },
                    { "TzTok-Jad", false, 2.5, 5, "jad", "tztok_jad" },
                    { "Vardorvis", false, 39.0, 5, "vardorvis", "vardorvis" },
                    { "Venenatis", false, 80.0, 5, "venenatis", "venenatis" },
                    { "Vet'ion", false, 50.0, 5, "vetion", "vetion" },
                    { "Vorkath", false, 34.0, 5, "vorkath", "vorkath" },
                    { "Wintertodt", false, 0.0, 5, "wintertodt", "wintertodt" },
                    { "Yama", false, 18.0, 5, "yama", "yama" },
                    { "Zalcano", false, 0.0, 5, "zalcano", "zalcano" },
                    { "Zulrah", false, 46.0, 5, "zulrah", "zulrah" }
                });

            migrationBuilder.InsertData(
                table: "SkillRates",
                columns: new[] { "Skill", "StartXp", "Method", "XpPerHour" },
                values: new object[,]
                {
                    { 1, 0, "Bonus XP from Slayer", 15000 },
                    { 1, 37224, "Bonus XP from Slayer", 38000 },
                    { 1, 100000, "Bonus XP from Slayer", 55000 },
                    { 1, 1000000, "Bonus XP from Slayer", 65000 },
                    { 1, 1210421, "Bonus XP from Slayer", 315000 },
                    { 1, 13034431, "Bonus XP from Slayer", 505000 },
                    { 2, 0, "Chinning in Kruk's Dungeon", 420000 },
                    { 3, 0, "Quests", 15000 },
                    { 3, 37224, "Bonus XP from Slayer", 38000 },
                    { 3, 61512, "Bonus XP from Slayer", 145000 },
                    { 3, 449428, "Bonus XP from Slayer", 245000 },
                    { 3, 1986068, "Bonus XP from Slayer", 300000 },
                    { 3, 6517253, "Bonus XP from Slayer", 335000 },
                    { 3, 13034431, "Bonus XP from Slayer", 415000 },
                    { 5, 0, "Bonus XP from Slayer", 250000 },
                    { 5, 6517253, "Bonus XP from Slayer", 330000 },
                    { 5, 13034431, "Chinning maniacal monkeys", 1190000 },
                    { 6, 0, "Dagannoth bones at the chaos altar", 1670000 },
                    { 6, 737627, "Superior dragon bones at the chaos altar", 2000000 },
                    { 8, 0, "1t poison karambwan", 172800 },
                    { 8, 13363, "1t karambwan", 519100 },
                    { 8, 37224, "1t karambwan", 591600 },
                    { 8, 101333, "1t karambwan", 663600 },
                    { 8, 273742, "1t karambwan", 735700 },
                    { 8, 737627, "1t karambwan", 808000 },
                    { 8, 1986068, "1t karambwan", 880400 },
                    { 8, 5346332, "1t karambwan", 948100 },
                    { 8, 13034431, "1t karambwan", 980000 },
                    { 9, 0, "Quests and trees", 29000 },
                    { 9, 2411, "2t oaks (average rate)", 56000 },
                    { 9, 22406, "1.5t teaks", 93174 },
                    { 9, 41171, "1.5t teaks", 114728 },
                    { 9, 111945, "1.5t teaks", 127339 },
                    { 9, 302288, "1.5t teaks", 172507 },
                    { 9, 814445, "1.5t teaks", 194022 },
                    { 9, 1986068, "1.5t teaks", 207636 },
                    { 9, 5346332, "1.5t teaks", 221977 },
                    { 9, 13034431, "1.5t teaks", 235000 },
                    { 11, 0, "Quests", 29200 },
                    { 11, 14612, "3t fly fishing", 46592 },
                    { 11, 75127, "Drift net fishing (67.7k hunter & 58k fishing xp/h)", 58000 },
                    { 11, 106046, "Drift net fishing (84.7k hunter & 67k fishing xp/h)", 67000 },
                    { 11, 229685, "Drift net fishing (98.7k hunter & 74.9k fishing xp/h)", 74900 },
                    { 11, 302288, "Drift net fishing (112.8k hunter & 83.3k fishing xp/h)", 83300 },
                    { 11, 593234, "Drift net fishing (123k hunter & 90.5k fishing xp/h)", 90500 },
                    { 11, 737627, "Drift net fishing (123k hunter & 93.3k fishing xp/h) + 2t swordfish & tuna", 93300 },
                    { 12, 0, "Colored logs", 73700 },
                    { 12, 22406, "Teak logs", 138900 },
                    { 12, 45529, "Arctic pine logs", 184250 },
                    { 12, 61512, "Maple logs", 198990 },
                    { 12, 101333, "Artefacts with firemaking (160.5k thieving & 135.9k firemaking xp/h)", 135900 },
                    { 12, 273742, "Artefacts with firemaking (191k thieving & 174.7k firemaking xp/h)", 174700 },
                    { 12, 1210421, "Artefacts with firemaking (220.6k thieving & 262.1k firemaking xp/h)", 262100 },
                    { 12, 5346332, "Artefacts with firemaking (242.2k thieving & 302k firemaking xp/h)", 302000 },
                    { 12, 13034431, "Artefacts with firemaking (255k thieving & 302k firemaking xp/h)", 302000 },
                    { 13, 0, "Leather items", 37000 },
                    { 13, 4470, "Sapphires", 139000 },
                    { 13, 9730, "Emeralds", 187650 },
                    { 13, 20224, "Rubies", 236300 },
                    { 13, 50339, "Diamonds", 298850 },
                    { 13, 368599, "Green d'hide bodies", 335230 },
                    { 13, 814445, "Blue d'hide bodies", 378490 },
                    { 13, 1475581, "Red d'hide bodies", 421740 },
                    { 13, 2951373, "Black d'hide bodies", 465000 },
                    { 14, 0, "Quests", 46500 },
                    { 14, 37224, "Dolo Blast Furnace gold", 475000 },
                    { 14, 13034431, "Dolo Blast Furnace gold", 505000 },
                    { 15, 0, "Quests", 20000 },
                    { 15, 35025, "Motherlode Mine for the prospector kit and Shooting Stars for the celestial ring", 50600 },
                    { 15, 393485, "3t granite", 106540 },
                    { 15, 1210421, "3t granite", 112166 },
                    { 15, 3258594, "3t granite", 116760 },
                    { 15, 8771558, "3t granite", 119438 },
                    { 15, 13034431, "3t granite", 126000 },
                    { 16, 0, "Quests", 11100 },
                    { 16, 8025, "Serum 207's", 218750 },
                    { 16, 123660, "Super energies", 293750 },
                    { 16, 166636, "Super strengths", 312500 },
                    { 16, 368599, "Super restores", 356250 },
                    { 16, 496254, "Super defences", 375000 },
                    { 16, 668051, "Antifire potions", 393750 },
                    { 16, 899257, "Ranging potions", 406250 },
                    { 16, 1336443, "Magic potions", 431250 },
                    { 16, 1475581, "1t stamina potions", 535500 },
                    { 16, 2951373, "1t extended antifires", 577500 },
                    { 16, 3972294, "1t anti-venoms", 630000 },
                    { 16, 11805606, "1t extended super antifires", 840000 },
                    { 17, 0, "Quests", 15100 },
                    { 17, 75127, "Wilderness Agility Course", 35000 },
                    { 17, 123660, "Hallowed Sepulchre", 45000 },
                    { 17, 333804, "Hallowed Sepulchre", 56300 },
                    { 17, 899257, "Hallowed Sepulchre", 68900 },
                    { 17, 2421087, "Hallowed Sepulchre", 79700 },
                    { 17, 6517253, "Hallowed Sepulchre with ancient & forgotten brews", 102000 },
                    { 18, 0, "Quests, fruit stalls", 15000 },
                    { 18, 61512, "Blackjacking", 80000 },
                    { 18, 91721, "Artefacts with firemaking (160.5k thieving & 135.9k firemaking xp/h)", 160500 },
                    { 18, 295315, "Artefacts with firemaking (191k thieving & 174.7k firemaking xp/h)", 191000 },
                    { 18, 1319012, "Artefacts with firemaking (220.6k thieving & 262.1k firemaking xp/h)", 220600 },
                    { 18, 4799743, "Artefacts with firemaking (242.2k thieving & 302k firemaking xp/h)", 242200 },
                    { 18, 10966391, "Artefacts with ancient brews (255.6k thieving & 250k herblore xp/h)", 255600 },
                    { 18, 13034431, "Artefacts with ancient brews after 200m firemaking (260k thieving & 250k herblore xp/h)", 260000 },
                    { 19, 0, "Efficient slayer", 5000 },
                    { 19, 37224, "Efficient Slayer", 12000 },
                    { 19, 101333, "Efficient Slayer", 40000 },
                    { 19, 449428, "Efficient Slayer", 74250 },
                    { 19, 1986068, "Efficient Slayer", 79000 },
                    { 19, 3258594, "Efficient Slayer", 86500 },
                    { 19, 5346332, "Efficient Slayer", 87000 },
                    { 19, 7195629, "Efficient Slayer", 93000 },
                    { 19, 13034431, "Efficient Slayer", 105800 },
                    { 20, 0, "Quests", 16000 },
                    { 20, 32500, "Tree runs", 304000 },
                    { 20, 61512, "Tree runs", 506000 },
                    { 20, 166636, "Tree runs", 719000 },
                    { 20, 273742, "Tree runs", 1083000 },
                    { 20, 605032, "Tree runs", 1362000 },
                    { 20, 1210421, "Tree runs", 1896000 },
                    { 20, 2192818, "Tree runs", 2314000 },
                    { 20, 3258594, "Tree runs", 2475000 },
                    { 20, 5346332, "Tree runs", 2500000 },
                    { 21, 0, "Quests", 13600 },
                    { 21, 33210, "GotR rewards", 45000 },
                    { 21, 1210421, "Solo mud runes", 75400 },
                    { 21, 3258594, "Solo mud runes", 106100 },
                    { 21, 13034431, "Duo lava runes", 162000 },
                    { 22, 0, "Varrock museum and birdhouses", 30000 },
                    { 22, 2107, "Oak birdhouses", 83000 },
                    { 22, 7028, "Willow birdhouses", 110000 },
                    { 22, 20224, "Teak birdhouses", 138000 },
                    { 22, 55649, "Drift net fishing (67.7k hunter & 58.0k fishing xp/h)", 67700 },
                    { 22, 91721, "Drift net fishing (84.7k hunter & 67.0k fishing xp/h)", 84700 },
                    { 22, 343551, "Drift net fishing (112.8k hunter & 83.3k fishing xp/h)", 112800 },
                    { 22, 737627, "Drift net fishing (123.0k hunter & 90.5k fishing xp/h)", 123000 },
                    { 22, 933979, "Drift net fishing (123k hunter & 93.3k fishing xp/h), scales to black chinchompas", 123000 },
                    { 23, 0, "Low-level furniture", 54700 },
                    { 23, 18247, "Oak larders", 450000 },
                    { 23, 123660, "Mahogany tables", 950000 },
                    { 23, 1475581, "Mahogany benches", 1070000 },
                    { 24, 0, "Port tasks, Charting & Quests", 10000 },
                    { 24, 13363, "Barracuda trials (The Tempor Tantrum)", 25000 },
                    { 24, 101333, "Large shipwrecks with boost", 35000 },
                    { 24, 166636, "Barracuda Trials (The Jubbly Jive)", 85000 },
                    { 24, 899257, "Barracuda Trials (The Gwenith Glide) - Camphor hull", 205000 },
                    { 24, 5346332, "Barracuda Trials (The Gwenith Glide) - Rosewood hull", 240000 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordRoles_ActivityId",
                table: "DiscordRoles",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordRoles_GuildId",
                table: "DiscordRoles",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordRoles_ItemId",
                table: "DiscordRoles",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordRoles_Skill",
                table: "DiscordRoles",
                column: "Skill");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DiscordRole_Emoji",
                table: "DiscordRoles",
                sql: "length(\"Emoji\") <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DiscordRole_Name",
                table: "DiscordRoles",
                sql: "length(\"Name\") <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DiscordRole_Threshold",
                table: "DiscordRoles",
                sql: "\"Threshold\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DiscordRole_TierGroup",
                table: "DiscordRoles",
                sql: "length(\"TierGroup\") <= 50");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CompetitionProviderDetails_Title",
                table: "CompetitionProvidersDetails",
                sql: "length(\"Title\") <= 100");

            migrationBuilder.CreateIndex(
                name: "IX_RoleProgressNotices_DiscordRoleId",
                table: "RoleProgressNotices",
                column: "DiscordRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordRoles_Items_ItemId",
                table: "DiscordRoles",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordRoles_Skills_Skill",
                table: "DiscordRoles",
                column: "Skill",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscordRoles_Items_ItemId",
                table: "DiscordRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscordRoles_Skills_Skill",
                table: "DiscordRoles");

            migrationBuilder.DropTable(
                name: "Bosses");

            migrationBuilder.DropTable(
                name: "RoleProgressNotices");

            migrationBuilder.DropTable(
                name: "SkillRates");

            migrationBuilder.DropIndex(
                name: "IX_DiscordRoles_ActivityId",
                table: "DiscordRoles");

            migrationBuilder.DropIndex(
                name: "IX_DiscordRoles_GuildId",
                table: "DiscordRoles");

            migrationBuilder.DropIndex(
                name: "IX_DiscordRoles_ItemId",
                table: "DiscordRoles");

            migrationBuilder.DropIndex(
                name: "IX_DiscordRoles_Skill",
                table: "DiscordRoles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DiscordRole_Emoji",
                table: "DiscordRoles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DiscordRole_Name",
                table: "DiscordRoles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DiscordRole_Threshold",
                table: "DiscordRoles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DiscordRole_TierGroup",
                table: "DiscordRoles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CompetitionProviderDetails_Title",
                table: "CompetitionProvidersDetails");

            migrationBuilder.DropColumn(
                name: "OverallLevel",
                table: "RunescapeAccounts");

            migrationBuilder.DropColumn(
                name: "OverallXp",
                table: "RunescapeAccounts");

            migrationBuilder.DropColumn(
                name: "LastSkippedAt",
                table: "GoodplaceSkillTasks");

            migrationBuilder.DropColumn(
                name: "LastSkippedAt",
                table: "GoodplaceActivityTasks");

            migrationBuilder.DropColumn(
                name: "CloseTrigger",
                table: "DiscordRoles");

            migrationBuilder.DropColumn(
                name: "Emoji",
                table: "DiscordRoles");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "DiscordRoles");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "DiscordRoles");

            migrationBuilder.DropColumn(
                name: "Metric",
                table: "DiscordRoles");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "DiscordRoles");

            migrationBuilder.DropColumn(
                name: "Skill",
                table: "DiscordRoles");

            migrationBuilder.DropColumn(
                name: "TierGroup",
                table: "DiscordRoles");

            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "CompetitionProvidersDetails");

            migrationBuilder.DropColumn(
                name: "FinishedAt",
                table: "CompetitionProvidersDetails");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "CompetitionProvidersDetails");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "CompetitionProvidersDetails");

            migrationBuilder.AlterColumn<int>(
                name: "Threshold",
                table: "DiscordRoles",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "ActivityId",
                table: "DiscordRoles",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "DiscordRoleId",
                table: "DiscordRoles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateTable(
                name: "ActivityRanks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActivityId = table.Column<int>(type: "integer", nullable: false),
                    DiscordRoleId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityRanks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityRanks_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityRanks_DiscordRoles_DiscordRoleId",
                        column: x => x.DiscordRoleId,
                        principalTable: "DiscordRoles",
                        principalColumn: "DiscordRoleId");
                });

            migrationBuilder.CreateTable(
                name: "SkillRanks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DiscordRoleId = table.Column<long>(type: "bigint", nullable: false),
                    Skill = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillRanks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillRanks_DiscordRoles_DiscordRoleId",
                        column: x => x.DiscordRoleId,
                        principalTable: "DiscordRoles",
                        principalColumn: "DiscordRoleId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordRoles_ActivityId",
                table: "DiscordRoles",
                column: "ActivityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityRanks_ActivityId",
                table: "ActivityRanks",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityRanks_DiscordRoleId",
                table: "ActivityRanks",
                column: "DiscordRoleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkillRanks_DiscordRoleId",
                table: "SkillRanks",
                column: "DiscordRoleId",
                unique: true);
        }
    }
}
