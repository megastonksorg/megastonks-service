using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Megastonks.Migrations
{
    /// <inheritdoc />
    public partial class MessageViewers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MessageViewerId",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MessageViewers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageViewers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageViewers_Message_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Message",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_MessageViewerId",
                table: "Accounts",
                column: "MessageViewerId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageViewers_MessageId",
                table: "MessageViewers",
                column: "MessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_MessageViewers_MessageViewerId",
                table: "Accounts",
                column: "MessageViewerId",
                principalTable: "MessageViewers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_MessageViewers_MessageViewerId",
                table: "Accounts");

            migrationBuilder.DropTable(
                name: "MessageViewers");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_MessageViewerId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "MessageViewerId",
                table: "Accounts");
        }
    }
}
