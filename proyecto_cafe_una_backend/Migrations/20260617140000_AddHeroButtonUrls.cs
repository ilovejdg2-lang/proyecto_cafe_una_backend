using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyecto_cafe_una_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroButtonUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE hero_principal
                    ADD COLUMN IF NOT EXISTS "PrimaryButtonUrl" character varying(500) NOT NULL DEFAULT '';

                ALTER TABLE hero_principal
                    ADD COLUMN IF NOT EXISTS "ButtonUrl" character varying(500) NOT NULL DEFAULT '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE hero_principal DROP COLUMN IF EXISTS "PrimaryButtonUrl";
                ALTER TABLE hero_principal DROP COLUMN IF EXISTS "ButtonUrl";
                """);
        }
    }
}
