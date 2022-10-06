using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace booking_back_end.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    roomNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    adultCapacity = table.Column<int>(type: "INTEGER", nullable: false),
                    childCapacity = table.Column<int>(type: "INTEGER", nullable: false),
                    price = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Amenities",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    text = table.Column<string>(type: "TEXT", nullable: true),
                    Roomid = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenities", x => x.id);
                    table.ForeignKey(
                        name: "FK_Amenities_Rooms_Roomid",
                        column: x => x.Roomid,
                        principalTable: "Rooms",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    guestFirstName = table.Column<string>(type: "TEXT", nullable: true),
                    guestLastName = table.Column<string>(type: "TEXT", nullable: true),
                    numberOfAdults = table.Column<int>(type: "INTEGER", nullable: false),
                    numberOfChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    checkInDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    checkOutDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    status = table.Column<string>(type: "TEXT", nullable: true),
                    roomId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.id);
                    table.ForeignKey(
                        name: "FK_Bookings_Rooms_roomId",
                        column: x => x.roomId,
                        principalTable: "Rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Amenities_Roomid",
                table: "Amenities",
                column: "Roomid");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_roomId",
                table: "Bookings",
                column: "roomId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Amenities");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Rooms");
        }
    }
}
