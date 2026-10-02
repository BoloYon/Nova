using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nova.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixPartCompanyPropertyNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartCompanies_Companies_companyId",
                table: "PartCompanies");

            migrationBuilder.DropForeignKey(
                name: "FK_PartCompanies_Parts_partId",
                table: "PartCompanies");

            migrationBuilder.RenameColumn(
                name: "relationshipType",
                table: "PartCompanies",
                newName: "RelationshipType");

            migrationBuilder.RenameColumn(
                name: "partId",
                table: "PartCompanies",
                newName: "PartId");

            migrationBuilder.RenameColumn(
                name: "companyPartNumber",
                table: "PartCompanies",
                newName: "CompanyPartNumber");

            migrationBuilder.RenameColumn(
                name: "companyId",
                table: "PartCompanies",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_PartCompanies_partId",
                table: "PartCompanies",
                newName: "IX_PartCompanies_PartId");

            migrationBuilder.RenameIndex(
                name: "IX_PartCompanies_companyId",
                table: "PartCompanies",
                newName: "IX_PartCompanies_CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartCompanies_Companies_CompanyId",
                table: "PartCompanies",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PartCompanies_Parts_PartId",
                table: "PartCompanies",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartCompanies_Companies_CompanyId",
                table: "PartCompanies");

            migrationBuilder.DropForeignKey(
                name: "FK_PartCompanies_Parts_PartId",
                table: "PartCompanies");

            migrationBuilder.RenameColumn(
                name: "RelationshipType",
                table: "PartCompanies",
                newName: "relationshipType");

            migrationBuilder.RenameColumn(
                name: "PartId",
                table: "PartCompanies",
                newName: "partId");

            migrationBuilder.RenameColumn(
                name: "CompanyPartNumber",
                table: "PartCompanies",
                newName: "companyPartNumber");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "PartCompanies",
                newName: "companyId");

            migrationBuilder.RenameIndex(
                name: "IX_PartCompanies_PartId",
                table: "PartCompanies",
                newName: "IX_PartCompanies_partId");

            migrationBuilder.RenameIndex(
                name: "IX_PartCompanies_CompanyId",
                table: "PartCompanies",
                newName: "IX_PartCompanies_companyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartCompanies_Companies_companyId",
                table: "PartCompanies",
                column: "companyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PartCompanies_Parts_partId",
                table: "PartCompanies",
                column: "partId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
