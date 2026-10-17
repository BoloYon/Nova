using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nova.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameProFormaRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProFormas_Companies_CompanyId",
                table: "ProFormas");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "ProFormas",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_ProFormas_CompanyId",
                table: "ProFormas",
                newName: "IX_ProFormas_CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProFormas_Companies_CustomerId",
                table: "ProFormas",
                column: "CustomerId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProFormas_Companies_CustomerId",
                table: "ProFormas");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "ProFormas",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_ProFormas_CustomerId",
                table: "ProFormas",
                newName: "IX_ProFormas_CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProFormas_Companies_CompanyId",
                table: "ProFormas",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
