using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NivBot.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OsrsId = table.Column<int>(type: "integer", nullable: false),
                    OsrsName = table.Column<string>(type: "text", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                    table.CheckConstraint("CK_Activity_OsrsName", "length(\"OsrsName\") <= 100");
                    table.CheckConstraint("CK_Activity_Points", "\"Points\" >= 0 AND \"Points\" <= 1000");
                });

            migrationBuilder.CreateTable(
                name: "CompetitionProvidersDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompetitionProvider = table.Column<int>(type: "integer", nullable: false),
                    ExternalId = table.Column<string>(type: "text", nullable: false),
                    CompetitionKey = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionProvidersDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscordMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscordRoles",
                columns: table => new
                {
                    DiscordId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleDescription = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordRoles", x => x.DiscordId);
                });

            migrationBuilder.CreateTable(
                name: "GoodplaceUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DiscordUserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodplaceUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OsrsId = table.Column<int>(type: "integer", nullable: false),
                    OsrsName = table.Column<string>(type: "text", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.CheckConstraint("CK_Item_OsrsName", "length(\"OsrsName\") <= 100");
                    table.CheckConstraint("CK_Item_Points", "\"Points\" >= 0 AND \"Points\" <= 1000");
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    HiscoreIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GlobalActivityBlockLists",
                columns: table => new
                {
                    ActivityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalActivityBlockLists", x => x.ActivityId);
                    table.ForeignKey(
                        name: "FK_GlobalActivityBlockLists_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityCompetitions",
                columns: table => new
                {
                    CompetitionProviderDetailsId = table.Column<int>(type: "integer", nullable: false),
                    ActivityId = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityCompetitions", x => x.CompetitionProviderDetailsId);
                    table.ForeignKey(
                        name: "FK_ActivityCompetitions_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityCompetitions_CompetitionProvidersDetails_Competitio~",
                        column: x => x.CompetitionProviderDetailsId,
                        principalTable: "CompetitionProvidersDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityRanks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActivityId = table.Column<int>(type: "integer", nullable: false),
                    DiscordRoleId = table.Column<long>(type: "bigint", nullable: true),
                    Amount = table.Column<int>(type: "integer", nullable: false)
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
                        principalColumn: "DiscordId");
                });

            migrationBuilder.CreateTable(
                name: "SkillRanks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Skill = table.Column<int>(type: "integer", nullable: false),
                    DiscordRoleId = table.Column<long>(type: "bigint", nullable: true),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillRanks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillRanks_DiscordRoles_DiscordRoleId",
                        column: x => x.DiscordRoleId,
                        principalTable: "DiscordRoles",
                        principalColumn: "DiscordId");
                });

            migrationBuilder.CreateTable(
                name: "ActivityTaskBlockLists",
                columns: table => new
                {
                    GoodplaceUserId = table.Column<int>(type: "integer", nullable: false),
                    ActivityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityTaskBlockLists", x => new { x.GoodplaceUserId, x.ActivityId });
                    table.ForeignKey(
                        name: "FK_ActivityTaskBlockLists_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityTaskBlockLists_GoodplaceUsers_GoodplaceUserId",
                        column: x => x.GoodplaceUserId,
                        principalTable: "GoodplaceUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoodplaceActivityTasks",
                columns: table => new
                {
                    GoodplaceUserId = table.Column<int>(type: "integer", nullable: false),
                    ActivityId = table.Column<int>(type: "integer", nullable: false),
                    CurrentCumulativeAmount = table.Column<int>(type: "integer", nullable: false),
                    GoalAmount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodplaceActivityTasks", x => x.GoodplaceUserId);
                    table.ForeignKey(
                        name: "FK_GoodplaceActivityTasks_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoodplaceActivityTasks_GoodplaceUsers_GoodplaceUserId",
                        column: x => x.GoodplaceUserId,
                        principalTable: "GoodplaceUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoodplaceSkillTasks",
                columns: table => new
                {
                    GoodplaceUserId = table.Column<int>(type: "integer", nullable: false),
                    Skill = table.Column<int>(type: "integer", nullable: false),
                    SummedCurrentXp = table.Column<long>(type: "bigint", nullable: false),
                    GoalXp = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodplaceSkillTasks", x => x.GoodplaceUserId);
                    table.ForeignKey(
                        name: "FK_GoodplaceSkillTasks_GoodplaceUsers_GoodplaceUserId",
                        column: x => x.GoodplaceUserId,
                        principalTable: "GoodplaceUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RunescapeAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RunescapeName = table.Column<string>(type: "text", nullable: false),
                    syncedColLog = table.Column<bool>(type: "boolean", nullable: false),
                    GoodplaceUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RunescapeAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RunescapeAccounts_GoodplaceUsers_GoodplaceUserId",
                        column: x => x.GoodplaceUserId,
                        principalTable: "GoodplaceUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    GoodplaceUserId = table.Column<int>(type: "integer", nullable: false),
                    GoodplacePoints = table.Column<int>(type: "integer", nullable: false),
                    GoodplaceCurrency = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.GoodplaceUserId);
                    table.ForeignKey(
                        name: "FK_Wallets_GoodplaceUsers_GoodplaceUserId",
                        column: x => x.GoodplaceUserId,
                        principalTable: "GoodplaceUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkillCompetitions",
                columns: table => new
                {
                    CompetitionProviderDetailsId = table.Column<int>(type: "integer", nullable: false),
                    Skill = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillCompetitions", x => x.CompetitionProviderDetailsId);
                    table.ForeignKey(
                        name: "FK_SkillCompetitions_CompetitionProvidersDetails_CompetitionPr~",
                        column: x => x.CompetitionProviderDetailsId,
                        principalTable: "CompetitionProvidersDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SkillCompetitions_Skills_Skill",
                        column: x => x.Skill,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SkillTaskBlockLists",
                columns: table => new
                {
                    GoodplaceUserId = table.Column<int>(type: "integer", nullable: false),
                    Skill = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillTaskBlockLists", x => new { x.GoodplaceUserId, x.Skill });
                    table.ForeignKey(
                        name: "FK_SkillTaskBlockLists_GoodplaceUsers_GoodplaceUserId",
                        column: x => x.GoodplaceUserId,
                        principalTable: "GoodplaceUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SkillTaskBlockLists_Skills_Skill",
                        column: x => x.Skill,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    ActivityId = table.Column<int>(type: "integer", nullable: false),
                    RunescapeAccountId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => new { x.RunescapeAccountId, x.ActivityId });
                    table.CheckConstraint("CK_ActivityLog_Amount", "\"Amount\" >= 0");
                    table.ForeignKey(
                        name: "FK_ActivityLogs_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityLogs_RunescapeAccounts_RunescapeAccountId",
                        column: x => x.RunescapeAccountId,
                        principalTable: "RunescapeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionLogs",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    RunescapeAccountId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionLogs", x => new { x.RunescapeAccountId, x.ItemId });
                    table.CheckConstraint("CK_CollectionLog_Amount", "\"Amount\" >= 0");
                    table.ForeignKey(
                        name: "FK_CollectionLogs_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionLogs_RunescapeAccounts_RunescapeAccountId",
                        column: x => x.RunescapeAccountId,
                        principalTable: "RunescapeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RunescapeStats",
                columns: table => new
                {
                    RunescapeAccountId = table.Column<int>(type: "integer", nullable: false),
                    Skill = table.Column<int>(type: "integer", nullable: false),
                    Xp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RunescapeStats", x => new { x.RunescapeAccountId, x.Skill });
                    table.CheckConstraint("CK_RunescapeStat_Xp", "\"Xp\" >= 0 AND \"Xp\" <= 200000000");
                    table.ForeignKey(
                        name: "FK_RunescapeStats_RunescapeAccounts_RunescapeAccountId",
                        column: x => x.RunescapeAccountId,
                        principalTable: "RunescapeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RunescapeStats_Skills_Skill",
                        column: x => x.Skill,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "HiscoreIndex", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Attack" },
                    { 2, 2, "Defence" },
                    { 3, 3, "Strength" },
                    { 4, 4, "Hitpoints" },
                    { 5, 5, "Ranged" },
                    { 6, 6, "Prayer" },
                    { 7, 7, "Magic" },
                    { 8, 8, "Cooking" },
                    { 9, 9, "Woodcutting" },
                    { 10, 10, "Fletching" },
                    { 11, 11, "Fishing" },
                    { 12, 12, "Firemaking" },
                    { 13, 13, "Crafting" },
                    { 14, 14, "Smithing" },
                    { 15, 15, "Mining" },
                    { 16, 16, "Herblore" },
                    { 17, 17, "Agility" },
                    { 18, 18, "Thieving" },
                    { 19, 19, "Slayer" },
                    { 20, 20, "Farming" },
                    { 21, 21, "Runecrafting" },
                    { 22, 22, "Hunter" },
                    { 23, 23, "Construction" },
                    { 24, 24, "Sailing" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_OsrsName",
                table: "Activities",
                column: "OsrsName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCompetitions_ActivityId",
                table: "ActivityCompetitions",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_ActivityId",
                table: "ActivityLogs",
                column: "ActivityId");

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
                name: "IX_ActivityTaskBlockLists_ActivityId",
                table: "ActivityTaskBlockLists",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionLogs_ItemId",
                table: "CollectionLogs",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionProvidersDetails_ExternalId_CompetitionProvider",
                table: "CompetitionProvidersDetails",
                columns: new[] { "ExternalId", "CompetitionProvider" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscordMessages_Type",
                table: "DiscordMessages",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoodplaceActivityTasks_ActivityId",
                table: "GoodplaceActivityTasks",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodplaceUsers_DiscordUserId",
                table: "GoodplaceUsers",
                column: "DiscordUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RunescapeAccounts_GoodplaceUserId",
                table: "RunescapeAccounts",
                column: "GoodplaceUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RunescapeAccounts_RunescapeName",
                table: "RunescapeAccounts",
                column: "RunescapeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RunescapeStats_Skill",
                table: "RunescapeStats",
                column: "Skill");

            migrationBuilder.CreateIndex(
                name: "IX_SkillCompetitions_Skill",
                table: "SkillCompetitions",
                column: "Skill");

            migrationBuilder.CreateIndex(
                name: "IX_SkillRanks_DiscordRoleId",
                table: "SkillRanks",
                column: "DiscordRoleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skills_HiscoreIndex",
                table: "Skills",
                column: "HiscoreIndex",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Name",
                table: "Skills",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkillTaskBlockLists_Skill",
                table: "SkillTaskBlockLists",
                column: "Skill");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityCompetitions");

            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "ActivityRanks");

            migrationBuilder.DropTable(
                name: "ActivityTaskBlockLists");

            migrationBuilder.DropTable(
                name: "CollectionLogs");

            migrationBuilder.DropTable(
                name: "DiscordMessages");

            migrationBuilder.DropTable(
                name: "GlobalActivityBlockLists");

            migrationBuilder.DropTable(
                name: "GoodplaceActivityTasks");

            migrationBuilder.DropTable(
                name: "GoodplaceSkillTasks");

            migrationBuilder.DropTable(
                name: "RunescapeStats");

            migrationBuilder.DropTable(
                name: "SkillCompetitions");

            migrationBuilder.DropTable(
                name: "SkillRanks");

            migrationBuilder.DropTable(
                name: "SkillTaskBlockLists");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "RunescapeAccounts");

            migrationBuilder.DropTable(
                name: "CompetitionProvidersDetails");

            migrationBuilder.DropTable(
                name: "DiscordRoles");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "GoodplaceUsers");
        }
    }
}
