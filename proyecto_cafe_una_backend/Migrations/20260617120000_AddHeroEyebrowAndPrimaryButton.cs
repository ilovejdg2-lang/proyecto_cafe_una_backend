using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyecto_cafe_una_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroEyebrowAndPrimaryButton : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE hero_principal
                    ADD COLUMN IF NOT EXISTS "Eyebrow" character varying(200) NOT NULL DEFAULT '';

                ALTER TABLE hero_principal
                    ADD COLUMN IF NOT EXISTS "PrimaryButtonText" character varying(200) NOT NULL DEFAULT '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE hero_principal DROP COLUMN IF EXISTS "Eyebrow";
                ALTER TABLE hero_principal DROP COLUMN IF EXISTS "PrimaryButtonText";
                """);
        }
    }
}
