using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Booking") ?? "Data Source=Booking.db";
builder.Services.AddSqlite<BookingDb>(connectionString);
// builder.Services.AddDbContext<BookingDb>(opt => opt.UseInMemoryDatabase("BookingDb"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddCors();

var app = builder.Build();

app.UseCors(builder =>
{
    builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader();
});

app.MapGet("/", () => "Hello World");

app.MapGet("/rooms", async (BookingDb db) =>
    await db.Rooms.Include(r => r.amenities).ToListAsync());


app.MapGet("/rooms/{id}", async (int id, BookingDb db) =>
    await db.Rooms.FindAsync(id)
        is Room room
            ? Results.Ok(room)
            : Results.NotFound());

app.MapPost("/rooms", async (Room room, BookingDb db) =>
{
    db.Rooms.Add(room);
    
    await db.SaveChangesAsync();

    return Results.Created($"/rooms/{room.id}", room);
});

app.MapPut("/rooms/{id}", async (int id, Room inputRoom, BookingDb db) =>
{
    var room = await db.Rooms.Include(r=>r.amenities).SingleOrDefaultAsync(r=>r.id == id);

    if (room is null) return Results.NotFound();

    room.roomNumber = inputRoom.roomNumber;
    room.adultCapacity = inputRoom.adultCapacity;
    room.childCapacity = inputRoom.childCapacity;
    room.price = inputRoom.price;
    if (room.amenities != null) db.Amenities.RemoveRange(room.amenities);
    room.amenities = inputRoom.amenities;
    await db.SaveChangesAsync();
    return Results.Ok(new{status=true});
});

app.MapDelete("/rooms/{id}", async (int id, BookingDb db) =>
{
    if (await db.Rooms.FindAsync(id) is Room room)
    {
        db.Rooms.Remove(room);
        await db.SaveChangesAsync();
        return Results.Ok(room);
    }

    return Results.NotFound();
});

app.MapGet("/booking", async (BookingDb db) =>
    await db.Bookings.Include(b => b.room).ToListAsync()
);


app.MapGet("/booking/{id}", async (int id, BookingDb db) =>
    await db.Bookings.Include(b => b.room).SingleOrDefaultAsync(b=>b.id==id)
        is Booking booking
            ? Results.Ok(booking)
            : Results.NotFound());

app.MapGet("/booking/today-check-in", async (BookingDb db) =>
    await db.Bookings.Where(b=>b.checkInDate == DateTime.Today).Include(b => b.room).ToListAsync()
);

app.MapGet("/booking/today-check-out", async (BookingDb db) =>
    await db.Bookings.Where(b=>b.checkOutDate == DateTime.Today).Include(b => b.room).ToListAsync()
);

app.MapPost("/get-rooms", async (Booking booking, BookingDb db) =>
{
    var room = await db.Rooms.Include(r=>r.bookings).Where(r => (
        r.adultCapacity >= booking.numberOfAdults &&
        r.childCapacity >= booking.numberOfChildren &&
        r.bookings.Where(
            b=>
            DateTime.Compare(b.checkInDate,booking.checkInDate) >-1 
            
        ).FirstOrDefault() == null
        )).FirstOrDefaultAsync();
    if (room is null)
    {
        return Results.Ok(new { });
    }
    return Results.Ok(room);
}
);

app.MapPost("/booking", async (Booking booking, BookingDb db) =>
{
    var room = db.Rooms.Find(booking.roomId);
    booking.room = room;
    db.Bookings.Add(booking);
    await db.SaveChangesAsync();

    return Results.Created($"/booking/{booking.id}", booking);
});

app.MapPut("/booking", async (BookingStatusDM inputBooking, BookingDb db) =>
{
    var booking = await db.Bookings.SingleOrDefaultAsync(b => b.id == inputBooking.id);

    if (booking is null) return Results.NotFound();
    if(inputBooking.status == "Check Out"){
        booking.checkOutDate =  DateTime.Today;
    }
    if(inputBooking.status == "Check In"){
        booking.checkInDate =  DateTime.Today;
    }
    booking.status = inputBooking.status;
    await db.SaveChangesAsync();

    return Results.Ok(booking);
});


app.Run();

class Room

{
    public int id { get; set; }
    public int roomNumber { get; set; }
    public int adultCapacity { get; set; }
    public int childCapacity { get; set; }
    public int price { get; set; }
    public ICollection<Amenities>? amenities { get; set; }


    [JsonIgnore]
    public ICollection<Booking>? bookings { get; set; }
}

class Amenities
{
    public int id { get; set; }
    public string? text { get; set; }
}

class Booking
{
    public int id { get; set; }
    public string?  guestFirstName { get; set; }
    public string? guestLastName { get; set; }
    public int numberOfAdults { get; set; }
    public int numberOfChildren { get; set; }
    public DateTime checkInDate { get; set; }
    public DateTime checkOutDate { get; set; }
    public string? status { get; set; }
    public Room? room { get; set; }
    public int roomId { get; set; }
}

class BookingStatusDM
{
    public int id { get; set; }
    public string? status { get; set; }
}

class BookingDb : DbContext
{
    public BookingDb(DbContextOptions<BookingDb> options)
        : base(options) { }

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<Amenities> Amenities => Set<Amenities>();

    public DbSet<Booking> Bookings => Set<Booking>();
}