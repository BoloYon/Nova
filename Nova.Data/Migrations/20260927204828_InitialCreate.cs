using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nova.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Parts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartName = table.Column<string>(type: "TEXT", nullable: false),
                    PrimaryPartNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    UnitType = table.Column<string>(type: "TEXT", nullable: false),
                    ShipWeight = table.Column<decimal>(type: "TEXT", nullable: false),
                    CubicFeet = table.Column<decimal>(type: "TEXT", nullable: false),
                    DrawingOnFile = table.Column<bool>(type: "INTEGER", nullable: false),
                    MoldAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Parts");
        }
    }
}
