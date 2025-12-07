using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuessifyBackend.Migrations
{
    /// <inheritdoc />
    public partial class CorrectVirtualReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "SongId",
                table: "Questions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "QuestionId",
                table: "PlayerAnswers",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "PointsAwarded",
                table: "PlayerAnswers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "PlayerId",
                table: "PlayerAnswers",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "GameCategoryId",
                table: "GameRounds",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_SongId",
                table: "Questions",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAnswers_PlayerId",
                table: "PlayerAnswers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAnswers_QuestionId",
                table: "PlayerAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_GameRounds_GameCategoryId",
                table: "GameRounds",
                column: "GameCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameRounds_GameCategories_GameCategoryId",
                table: "GameRounds",
                column: "GameCategoryId",
                principalTable: "GameCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerAnswers_Players_PlayerId",
                table: "PlayerAnswers",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerAnswers_Questions_QuestionId",
                table: "PlayerAnswers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Songs_SongId",
                table: "Questions",
                column: "SongId",
                principalTable: "Songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameRounds_GameCategories_GameCategoryId",
                table: "GameRounds");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerAnswers_Players_PlayerId",
                table: "PlayerAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerAnswers_Questions_QuestionId",
                table: "PlayerAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Songs_SongId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_SongId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_PlayerAnswers_PlayerId",
                table: "PlayerAnswers");

            migrationBuilder.DropIndex(
                name: "IX_PlayerAnswers_QuestionId",
                table: "PlayerAnswers");

            migrationBuilder.DropIndex(
                name: "IX_GameRounds_GameCategoryId",
                table: "GameRounds");

            migrationBuilder.AlterColumn<string>(
                name: "SongId",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "QuestionId",
                table: "PlayerAnswers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "PointsAwarded",
                table: "PlayerAnswers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PlayerId",
                table: "PlayerAnswers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "GameCategoryId",
                table: "GameRounds",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
