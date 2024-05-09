using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lesson58.Migrations
{
    /// <inheritdoc />
    public partial class ChangedSubToFollow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_OwnerUserId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_SubAndSubs_AspNetUsers_SubcriberId",
                table: "SubAndSubs");

            migrationBuilder.DropForeignKey(
                name: "FK_SubAndSubs_AspNetUsers_SubcribtionId",
                table: "SubAndSubs");

            migrationBuilder.RenameColumn(
                name: "SubcribtionId",
                table: "SubAndSubs",
                newName: "FollowToId");

            migrationBuilder.RenameColumn(
                name: "SubcriberId",
                table: "SubAndSubs",
                newName: "FollowFromId");

            migrationBuilder.RenameIndex(
                name: "IX_SubAndSubs_SubcribtionId",
                table: "SubAndSubs",
                newName: "IX_SubAndSubs_FollowToId");

            migrationBuilder.RenameIndex(
                name: "IX_SubAndSubs_SubcriberId",
                table: "SubAndSubs",
                newName: "IX_SubAndSubs_FollowFromId");

            migrationBuilder.RenameColumn(
                name: "SubscribtionsCount",
                table: "AspNetUsers",
                newName: "FollowingsCount");

            migrationBuilder.RenameColumn(
                name: "SubscribersCount",
                table: "AspNetUsers",
                newName: "FollowersCount");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerUserId",
                table: "Posts",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "Posts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Posts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_OwnerUserId",
                table: "Posts",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubAndSubs_AspNetUsers_FollowFromId",
                table: "SubAndSubs",
                column: "FollowFromId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubAndSubs_AspNetUsers_FollowToId",
                table: "SubAndSubs",
                column: "FollowToId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_OwnerUserId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_SubAndSubs_AspNetUsers_FollowFromId",
                table: "SubAndSubs");

            migrationBuilder.DropForeignKey(
                name: "FK_SubAndSubs_AspNetUsers_FollowToId",
                table: "SubAndSubs");

            migrationBuilder.RenameColumn(
                name: "FollowToId",
                table: "SubAndSubs",
                newName: "SubcribtionId");

            migrationBuilder.RenameColumn(
                name: "FollowFromId",
                table: "SubAndSubs",
                newName: "SubcriberId");

            migrationBuilder.RenameIndex(
                name: "IX_SubAndSubs_FollowToId",
                table: "SubAndSubs",
                newName: "IX_SubAndSubs_SubcribtionId");

            migrationBuilder.RenameIndex(
                name: "IX_SubAndSubs_FollowFromId",
                table: "SubAndSubs",
                newName: "IX_SubAndSubs_SubcriberId");

            migrationBuilder.RenameColumn(
                name: "FollowingsCount",
                table: "AspNetUsers",
                newName: "SubscribtionsCount");

            migrationBuilder.RenameColumn(
                name: "FollowersCount",
                table: "AspNetUsers",
                newName: "SubscribersCount");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerUserId",
                table: "Posts",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "Posts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Posts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_OwnerUserId",
                table: "Posts",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubAndSubs_AspNetUsers_SubcriberId",
                table: "SubAndSubs",
                column: "SubcriberId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubAndSubs_AspNetUsers_SubcribtionId",
                table: "SubAndSubs",
                column: "SubcribtionId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
