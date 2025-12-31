using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionAndScene : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CurrentSceneId",
                table: "Sessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentSessionId",
                table: "Campaigns",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastSessionNumber",
                table: "Campaigns",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Scenes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SceneNumber = table.Column<int>(type: "integer", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: true),
                    SummaryEmbedding1 = table.Column<Vector>(type: "vector(1024)", nullable: true),
                    SummaryEmbedding = table.Column<Vector>(type: "vector", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scenes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scenes_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_CurrentSceneId",
                table: "Sessions",
                column: "CurrentSceneId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_CurrentSessionId",
                table: "Campaigns",
                column: "CurrentSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scenes_SessionId",
                table: "Scenes",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenes_SummaryEmbedding1",
                table: "Scenes",
                column: "SummaryEmbedding1")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Sessions_CurrentSessionId",
                table: "Campaigns",
                column: "CurrentSessionId",
                principalTable: "Sessions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Scenes_CurrentSceneId",
                table: "Sessions",
                column: "CurrentSceneId",
                principalTable: "Scenes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Sessions_CurrentSessionId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Scenes_CurrentSceneId",
                table: "Sessions");

            migrationBuilder.DropTable(
                name: "Scenes");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_CurrentSceneId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_CurrentSessionId",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "CurrentSceneId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "CurrentSessionId",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "LastSessionNumber",
                table: "Campaigns");
        }
    }
}
