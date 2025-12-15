using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiRoti.Migrations
{
    /// <inheritdoc />
    public partial class AddEspecialidadCocinero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Especialidad",
                table: "Usuario",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Especialidad",
                table: "Usuario");
        }
    }
}
