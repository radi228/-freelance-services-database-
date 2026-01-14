using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skillo.Migrations
{
    /// <inheritdoc />
    public partial class AddDualUsernames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old unique constraint on Username
            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            // Drop the Username column
            migrationBuilder.DropColumn(
                name: "Username",
                table: "Users");

            // Add two new username columns
            migrationBuilder.AddColumn<string>(
                name: "UsernameOffering",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UsernameReceiving",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            // Create unique indexes for both new username columns
            migrationBuilder.CreateIndex(
                name: "IX_Users_UsernameOffering",
                table: "Users",
                column: "UsernameOffering",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UsernameReceiving",
                table: "Users",
                column: "UsernameReceiving",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the new unique constraints
            migrationBuilder.DropIndex(
                name: "IX_Users_UsernameOffering",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UsernameReceiving",
                table: "Users");

            // Drop the new columns
            migrationBuilder.DropColumn(
                name: "UsernameOffering",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UsernameReceiving",
                table: "Users");

            // Add back the original Username column
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            // Recreate the original unique index
            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }
    }
}
