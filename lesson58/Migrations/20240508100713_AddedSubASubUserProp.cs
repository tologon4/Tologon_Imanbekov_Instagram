using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace lesson58.Migrations
{
    /// <inheritdoc />
    public partial class AddedSubASubUserProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserUser");

            migrationBuilder.AddColumn<int>(
                name: "PostCount",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubscribersCount",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubscribtionsCount",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SubAndSubs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubcribtionId = table.Column<int>(type: "integer", nullable: true),
                    SubcriberId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubAndSubs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubAndSubs_AspNetUsers_SubcriberId",
                        column: x => x.SubcriberId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubAndSubs_AspNetUsers_SubcribtionId",
                        column: x => x.SubcribtionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubAndSubs_SubcriberId",
                table: "SubAndSubs",
                column: "SubcriberId");

            migrationBuilder.CreateIndex(
                name: "IX_SubAndSubs_SubcribtionId",
                table: "SubAndSubs",
                column: "SubcribtionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubAndSubs");

            migrationBuilder.DropColumn(
                name: "PostCount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SubscribersCount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SubscribtionsCount",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "UserUser",
                columns: table => new
                {
                    SubscribersId = table.Column<int>(type: "integer", nullable: false),
                    SubscribtionsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserUser", x => new { x.SubscribersId, x.SubscribtionsId });
                    table.ForeignKey(
                        name: "FK_UserUser_AspNetUsers_SubscribersId",
                        column: x => x.SubscribersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserUser_AspNetUsers_SubscribtionsId",
                        column: x => x.SubscribtionsId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserUser_SubscribtionsId",
                table: "UserUser",
                column: "SubscribtionsId");
        }
    }
}
