using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBase.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agents",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false, defaultValue: " "),
                    is_zak = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "currencies",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    currency_name = table.Column<string>(type: "TEXT", nullable: false, defaultValue: " "),
                    currency_sign = table.Column<string>(type: "TEXT", nullable: false, defaultValue: " "),
                    to_usd = table.Column<double>(type: "REAL", nullable: false),
                    can_delete = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currencies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "main_name",
                columns: table => new
                {
                    uni_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    count = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_main_name", x => x.uni_id);
                });

            migrationBuilder.CreateTable(
                name: "producer",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    producer_name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agent_transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    agent_id = table.Column<int>(type: "INTEGER", nullable: false),
                    transaction_status = table.Column<int>(type: "INTEGER", nullable: false),
                    transaction_sum = table.Column<double>(type: "REAL", nullable: false),
                    balance = table.Column<double>(type: "REAL", nullable: false),
                    transaction_datatime = table.Column<string>(type: "TEXT", nullable: false),
                    currency = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_agent_transactions_agents_agent_id",
                        column: x => x.agent_id,
                        principalTable: "agents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_agent_transactions_currencies_currency",
                        column: x => x.currency,
                        principalTable: "currencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "main_cat",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    uni_id = table.Column<int>(type: "INTEGER", nullable: false),
                    uni_value = table.Column<string>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "  "),
                    producer_id = table.Column<int>(type: "INTEGER", nullable: false),
                    count = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_main_cat", x => x.id);
                    table.ForeignKey(
                        name: "FK_main_cat_main_name_uni_id",
                        column: x => x.uni_id,
                        principalTable: "main_name",
                        principalColumn: "uni_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_main_cat_producer_producer_id",
                        column: x => x.producer_id,
                        principalTable: "producer",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "prod_main_group",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    datetime = table.Column<string>(type: "TEXT", nullable: false),
                    total_sum = table.Column<double>(type: "REAL", nullable: false),
                    agent_id = table.Column<int>(type: "INTEGER", nullable: false),
                    currency_id = table.Column<int>(type: "INTEGER", nullable: false),
                    transaction_id = table.Column<int>(type: "INTEGER", nullable: false),
                    comment = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prod_main_group", x => x.id);
                    table.ForeignKey(
                        name: "FK_prod_main_group_agent_transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "agent_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prod_main_group_agents_agent_id",
                        column: x => x.agent_id,
                        principalTable: "agents",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_prod_main_group_currencies_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "zak_main_group",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    datetime = table.Column<string>(type: "TEXT", nullable: false),
                    total_sum = table.Column<double>(type: "REAL", nullable: false),
                    agent_id = table.Column<int>(type: "INTEGER", nullable: false),
                    currency_id = table.Column<int>(type: "INTEGER", nullable: false),
                    transaction_id = table.Column<int>(type: "INTEGER", nullable: false),
                    comment = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zak_main_group", x => x.id);
                    table.ForeignKey(
                        name: "FK_zak_main_group_agent_transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "agent_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_zak_main_group_agents_agent_id",
                        column: x => x.agent_id,
                        principalTable: "agents",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_zak_main_group_currencies_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "main_cat_prices",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    main_cat_id = table.Column<int>(type: "INTEGER", nullable: false),
                    currency_id = table.Column<int>(type: "INTEGER", nullable: false),
                    price = table.Column<double>(type: "REAL", nullable: false),
                    count = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_main_cat_prices", x => x.id);
                    table.ForeignKey(
                        name: "FK_main_cat_prices_currencies_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_main_cat_prices_main_cat_main_cat_id",
                        column: x => x.main_cat_id,
                        principalTable: "main_cat",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prodaja",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    prodaja_id = table.Column<int>(type: "INTEGER", nullable: false),
                    price = table.Column<double>(type: "REAL", nullable: false),
                    count = table.Column<int>(type: "INTEGER", nullable: false),
                    main_cat_id = table.Column<int>(type: "INTEGER", nullable: true),
                    uniValue = table.Column<string>(type: "TEXT", nullable: true),
                    main_name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prodaja", x => x.id);
                    table.ForeignKey(
                        name: "FK_prodaja_main_cat_main_cat_id",
                        column: x => x.main_cat_id,
                        principalTable: "main_cat",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_prodaja_prod_main_group_prodaja_id",
                        column: x => x.prodaja_id,
                        principalTable: "prod_main_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "zakupka",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    count = table.Column<int>(type: "INTEGER", nullable: false),
                    price = table.Column<double>(type: "REAL", nullable: false),
                    zak_id = table.Column<int>(type: "INTEGER", nullable: false),
                    main_cat_id = table.Column<int>(type: "INTEGER", nullable: true),
                    uni_value = table.Column<string>(type: "TEXT", nullable: true),
                    main_name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zakupka", x => x.id);
                    table.ForeignKey(
                        name: "FK_zakupka_main_cat_main_cat_id",
                        column: x => x.main_cat_id,
                        principalTable: "main_cat",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_zakupka_zak_main_group_zak_id",
                        column: x => x.zak_id,
                        principalTable: "zak_main_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agent_transactions_agent_id",
                table: "agent_transactions",
                column: "agent_id");

            migrationBuilder.CreateIndex(
                name: "IX_agent_transactions_currency",
                table: "agent_transactions",
                column: "currency");

            migrationBuilder.CreateIndex(
                name: "IX_agent_transactions_id",
                table: "agent_transactions",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agents_id",
                table: "agents",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_currencies_id",
                table: "currencies",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_main_cat_id",
                table: "main_cat",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_main_cat_producer_id",
                table: "main_cat",
                column: "producer_id");

            migrationBuilder.CreateIndex(
                name: "IX_main_cat_uni_id",
                table: "main_cat",
                column: "uni_id");

            migrationBuilder.CreateIndex(
                name: "IX_main_cat_prices_currency_id",
                table: "main_cat_prices",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_main_cat_prices_id",
                table: "main_cat_prices",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_main_cat_prices_main_cat_id",
                table: "main_cat_prices",
                column: "main_cat_id");

            migrationBuilder.CreateIndex(
                name: "IX_main_name_uni_id",
                table: "main_name",
                column: "uni_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prod_main_group_agent_id",
                table: "prod_main_group",
                column: "agent_id");

            migrationBuilder.CreateIndex(
                name: "IX_prod_main_group_currency_id",
                table: "prod_main_group",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_prod_main_group_id",
                table: "prod_main_group",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prod_main_group_transaction_id",
                table: "prod_main_group",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_prodaja_id",
                table: "prodaja",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prodaja_main_cat_id",
                table: "prodaja",
                column: "main_cat_id");

            migrationBuilder.CreateIndex(
                name: "IX_prodaja_prodaja_id",
                table: "prodaja",
                column: "prodaja_id");

            migrationBuilder.CreateIndex(
                name: "IX_producer_id",
                table: "producer",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_zak_main_group_agent_id",
                table: "zak_main_group",
                column: "agent_id");

            migrationBuilder.CreateIndex(
                name: "IX_zak_main_group_currency_id",
                table: "zak_main_group",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_zak_main_group_id",
                table: "zak_main_group",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_zak_main_group_transaction_id",
                table: "zak_main_group",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_zakupka_id",
                table: "zakupka",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_zakupka_main_cat_id",
                table: "zakupka",
                column: "main_cat_id");

            migrationBuilder.CreateIndex(
                name: "IX_zakupka_zak_id",
                table: "zakupka",
                column: "zak_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "main_cat_prices");

            migrationBuilder.DropTable(
                name: "prodaja");

            migrationBuilder.DropTable(
                name: "zakupka");

            migrationBuilder.DropTable(
                name: "prod_main_group");

            migrationBuilder.DropTable(
                name: "main_cat");

            migrationBuilder.DropTable(
                name: "zak_main_group");

            migrationBuilder.DropTable(
                name: "main_name");

            migrationBuilder.DropTable(
                name: "producer");

            migrationBuilder.DropTable(
                name: "agent_transactions");

            migrationBuilder.DropTable(
                name: "agents");

            migrationBuilder.DropTable(
                name: "currencies");
        }
    }
}
