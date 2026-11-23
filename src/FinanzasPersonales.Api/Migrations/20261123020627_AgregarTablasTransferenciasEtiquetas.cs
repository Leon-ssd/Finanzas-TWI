using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanzasPersonales.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablasTransferenciasEtiquetas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categoria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    limite_mensual = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria", x => x.id);
                    table.CheckConstraint("chk_categoria_limite_mensual", "limite_mensual IS NULL OR limite_mensual >= 0");
                    table.ForeignKey(
                        name: "FK_categoria_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cuenta",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "BOB"),
                    saldo_inicial = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuenta", x => x.id);
                    table.CheckConstraint("chk_cuenta_saldo_inicial", "saldo_inicial >= 0");
                    table.ForeignKey(
                        name: "FK_cuenta_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "etiqueta",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    color_hex = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#000000"),
                    activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_etiqueta", x => x.id);
                    table.ForeignKey(
                        name: "FK_etiqueta_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "preferencia_usuario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    moneda_principal = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "BOB"),
                    dia_inicio_mes = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    alerta_limite_porcentaje = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)80),
                    tema_ui = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "claro")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preferencia_usuario", x => x.id);
                    table.CheckConstraint("chk_preferencia_alerta_limite_porcentaje", "alerta_limite_porcentaje >= 1 AND alerta_limite_porcentaje <= 100");
                    table.CheckConstraint("chk_preferencia_dia_inicio_mes", "dia_inicio_mes >= 1 AND dia_inicio_mes <= 28");
                    table.ForeignKey(
                        name: "FK_preferencia_usuario_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movimiento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cuenta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    monto = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    concepto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fecha_movimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    notas = table.Column<string>(type: "text", nullable: true),
                    fecha_registro = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimiento", x => x.id);
                    table.CheckConstraint("chk_movimiento_monto", "monto > 0");
                    table.ForeignKey(
                        name: "FK_movimiento_categoria_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "categoria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimiento_cuenta_cuenta_id",
                        column: x => x.cuenta_id,
                        principalTable: "cuenta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimiento_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transferencia",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cuenta_origen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cuenta_destino_id = table.Column<Guid>(type: "uuid", nullable: false),
                    monto = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    comision = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    fecha_transferencia = table.Column<DateOnly>(type: "date", nullable: false),
                    concepto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fecha_registro = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transferencia", x => x.id);
                    table.CheckConstraint("chk_transferencia_comision_no_negativa", "comision >= 0");
                    table.CheckConstraint("chk_transferencia_cuentas_distintas", "cuenta_origen_id <> cuenta_destino_id");
                    table.CheckConstraint("chk_transferencia_monto_positivo", "monto > 0");
                    table.ForeignKey(
                        name: "FK_transferencia_cuenta_cuenta_destino_id",
                        column: x => x.cuenta_destino_id,
                        principalTable: "cuenta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transferencia_cuenta_cuenta_origen_id",
                        column: x => x.cuenta_origen_id,
                        principalTable: "cuenta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transferencia_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movimiento_etiqueta",
                columns: table => new
                {
                    movimiento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    etiqueta_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimiento_etiqueta", x => new { x.movimiento_id, x.etiqueta_id });
                    table.ForeignKey(
                        name: "FK_movimiento_etiqueta_etiqueta_etiqueta_id",
                        column: x => x.etiqueta_id,
                        principalTable: "etiqueta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_movimiento_etiqueta_movimiento_movimiento_id",
                        column: x => x.movimiento_id,
                        principalTable: "movimiento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categoria_usuario_id",
                table: "categoria",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_cuenta_usuario_id",
                table: "cuenta",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_etiqueta_usuario_nombre_activa",
                table: "etiqueta",
                columns: new[] { "usuario_id", "nombre" },
                unique: true,
                filter: "activa = true");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_categoria_id",
                table: "movimiento",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_cuenta_id",
                table: "movimiento",
                column: "cuenta_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_usuario_id",
                table: "movimiento",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_etiqueta_etiqueta_id",
                table: "movimiento_etiqueta",
                column: "etiqueta_id");

            migrationBuilder.CreateIndex(
                name: "ix_preferencia_usuario_usuario_id",
                table: "preferencia_usuario",
                column: "usuario_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transferencia_cuenta_destino_id",
                table: "transferencia",
                column: "cuenta_destino_id");

            migrationBuilder.CreateIndex(
                name: "IX_transferencia_cuenta_origen_id",
                table: "transferencia",
                column: "cuenta_origen_id");

            migrationBuilder.CreateIndex(
                name: "IX_transferencia_usuario_id",
                table: "transferencia",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_email",
                table: "usuario",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "movimiento_etiqueta");

            migrationBuilder.DropTable(
                name: "preferencia_usuario");

            migrationBuilder.DropTable(
                name: "transferencia");

            migrationBuilder.DropTable(
                name: "etiqueta");

            migrationBuilder.DropTable(
                name: "movimiento");

            migrationBuilder.DropTable(
                name: "categoria");

            migrationBuilder.DropTable(
                name: "cuenta");

            migrationBuilder.DropTable(
                name: "usuario");
        }
    }
}
