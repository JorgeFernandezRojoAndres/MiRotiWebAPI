using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiRoti.Migrations
{
    /// <inheritdoc />
    public partial class CadeteContacto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cadete_Direccion",
                table: "Usuario",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Cadete_Telefono",
                table: "Usuario",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cadete_Direccion",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "Cadete_Telefono",
                table: "Usuario");
        }
    }
}
