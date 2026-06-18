using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyecto_cafe_una_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeContentLinkFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE textos_institucionales
                    ADD COLUMN IF NOT EXISTS "LinkText" character varying(200);

                ALTER TABLE tarjetas_inicio
                    ADD COLUMN IF NOT EXISTS "TextoBoton" character varying(200) NOT NULL DEFAULT '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE textos_institucionales DROP COLUMN IF EXISTS "LinkText";
                ALTER TABLE tarjetas_inicio DROP COLUMN IF EXISTS "TextoBoton";
                """);
        }
    }
}
