using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalkieClient.Migrations
{
    /// <inheritdoc />
    public partial class AddFilesToMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Messages_MessageID",
                table: "Files");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Messages_MessageID",
                table: "Files",
                column: "MessageID",
                principalTable: "Messages",
                principalColumn: "MessageId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Messages_MessageID",
                table: "Files");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Messages_MessageID",
                table: "Files",
                column: "MessageID",
                principalTable: "Messages",
                principalColumn: "MessageId");
        }
    }
}
