using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace proyecto_cafe_una_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "enlaces_sitio",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Etiqueta = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Ruta = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Seccion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    AbrirEnNuevaPestana = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enlaces_sitio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "galeria_institucional",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Image = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_galeria_institucional", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hero_principal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Subtitle = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ButtonText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BackgroundImage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hero_principal", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "informacion_footer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LogoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    LogoClaroUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FraseMarca = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Correo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FacebookUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    InstagramUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MapsUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TextoCopyright = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_informacion_footer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "informacion_navbar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LogoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    LogoClaroUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_informacion_navbar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_entries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Correo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExpiraEnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Usado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "solicitudes_voluntariado",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FechaSolicitud = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Pendiente"),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TipoVoluntariado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Identificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Institucion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Pais = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Modalidad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CantidadParticipantes = table.Column<int>(type: "integer", nullable: true),
                    Residencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Horario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Dias = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Area = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Descripcion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Motivacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitudes_voluntariado", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "textos_institucionales",
                columns: table => new
                {
                    Clave = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_textos_institucionales", x => x.Clave);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Correo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "activo"),
                    Roles = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_entries_Token",
                table: "password_reset_entries",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Correo",
                table: "usuarios",
                column: "Correo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enlaces_sitio");

            migrationBuilder.DropTable(
                name: "galeria_institucional");

            migrationBuilder.DropTable(
                name: "hero_principal");

            migrationBuilder.DropTable(
                name: "informacion_footer");

            migrationBuilder.DropTable(
                name: "informacion_navbar");

            migrationBuilder.DropTable(
                name: "password_reset_entries");

            migrationBuilder.DropTable(
                name: "solicitudes_voluntariado");

            migrationBuilder.DropTable(
                name: "textos_institucionales");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
