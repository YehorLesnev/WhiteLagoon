using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WhiteLagoon.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAmenityTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Amenity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VillaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Amenity_Villas_VillaId",
                        column: x => x.VillaId,
                        principalTable: "Villas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Amenity",
                columns: new[] { "Id", "Description", "Name", "VillaId" },
                values: new object[,]
                {
                    { 1, "Enjoy your own private pool with stunning views.", "Private Pool", 1 },
                    { 2, "Stay connected with complimentary high-speed internet access.", "Free Wi-Fi", 1 },
                    { 3, "Wake up to breathtaking ocean views from your villa.", "Ocean View", 2 },
                    { 4, "Prepare delicious meals in a fully equipped gourmet kitchen.", "Gourmet Kitchen", 2 },
                    { 5, "Relax and unwind in a luxurious spa bath.", "Spa Bath", 3 },
                    { 6, "Stay active with access to a state-of-the-art fitness center.", "Fitness Center", 3 },
                    { 7, "Experience exceptional service with our 24/7 concierge.", "24/7 Concierge", 1 },
                    { 8, "Enjoy exclusive access to a pristine private beach.", "Private Beach Access", 2 },
                    { 9, "Savor gourmet meals in the comfort of your villa with our in-villa dining service.", "In-Villa Dining", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Amenity_VillaId",
                table: "Amenity",
                column: "VillaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Amenity");
        }
    }
}
