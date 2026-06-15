using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyecto_cafe_una_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioPerfilFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE usuarios
                    ADD COLUMN IF NOT EXISTS "FotoPerfilUrl" character varying(1000);

                ALTER TABLE usuarios
                    ADD COLUMN IF NOT EXISTS "FotoBannerUrl" character varying(1000);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE usuarios DROP COLUMN IF EXISTS "FotoPerfilUrl";
                ALTER TABLE usuarios DROP COLUMN IF EXISTS "FotoBannerUrl";
                """);
        }
    }
}
