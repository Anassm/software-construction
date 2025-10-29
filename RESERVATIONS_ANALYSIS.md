# Reservations Component - Analyse en Verbeterpunten

## Overzicht
Dit document bevat een grondige analyse van de reservatiecomponent in het software-construction project met concrete verbeterpunten en aanbevelingen.

## 📊 Geanalyseerde Bestanden
- `v2/Controller/ReservationController.cs` - API endpoint voor reservaties
- `v2/infrastructure/services/ReservationService.cs` - Business logic
- `v2/Core/Models/Reservation.cs` - Data model
- `v2/Core/DTOs/ReservationDTO.cs` - Data transfer objects
- `v2/Core/Interfaces/IReservation.cs` - Service interface
- `v2/infrastructure/data/ApplicationDbContext.cs` - Database configuratie

---

## 🔴 Kritieke Problemen

### 1. **Ontbrekende Autorisatie en Authenticatie**
**Probleem:** De ReservationController heeft geen autorisatie checks.
```csharp
[ApiController]
[Route("reservations")]
public class ReservationController : ControllerBase
{
    // Geen [Authorize] attributen!
```

**Impact:** Elke gebruiker (zelfs niet-geauthenticeerde) kan reservaties maken.

**Oplossing:**
```csharp
[ApiController]
[Route("reservations")]
[Authorize]  // Voeg toe
public class ReservationController : ControllerBase
```

---

### 2. **TotalPrice Wordt Nooit Berekend**
**Probleem:** De TotalPrice wordt altijd op 0 gezet:
```csharp
var reservation = new Reservation
{
    // ...
    TotalPrice = 0f,  // Altijd 0!
```

**Impact:** Geen prijsberekening, klanten betalen niets.

**Oplossing:** Implementeer prijsberekening op basis van:
- Aantal dagen/uren tussen StartDate en EndDate
- Tarief van de parking lot (lot.Tariff en lot.DayTariff)

**Voorbeeld implementatie:**
```csharp
var duration = (request.EndDate - request.StartDate).TotalDays;
var totalPrice = (float)(duration * lot.DayTariff);

var reservation = new Reservation
{
    // ...
    TotalPrice = totalPrice,
```

---

### 3. **Geen Capaciteitscontrole**
**Probleem:** Er wordt niet gecontroleerd of de parking lot nog capaciteit heeft.

**Impact:** Overboeking mogelijk - meer reservaties dan plekken beschikbaar.

**Oplossing:** Check beschikbare capaciteit:
```csharp
// Check if parking lot has available capacity
var existingReservations = await _db.Reservation
    .Where(r => r.ParkingLotID == request.ParkingLotId 
        && r.Status != "Cancelled"
        && r.StartDate < request.EndDate 
        && r.EndDate > request.StartDate)
    .CountAsync();

if (existingReservations >= lot.Capacity)
    throw new ArgumentException("Parking lot is fully booked for the selected dates.");
```

---

### 4. **Geen Overlappende Reservaties Check**
**Probleem:** Er wordt niet gecontroleerd of dezelfde vehicle al een overlappende reservatie heeft.

**Impact:** Een voertuig kan op meerdere locaties tegelijk gereserveerd zijn.

**Oplossing:**
```csharp
var overlappingReservations = await _db.Reservation
    .Where(r => r.VehicleID == vehicle.ID
        && r.Status != "Cancelled"
        && r.StartDate < request.EndDate 
        && r.EndDate > request.StartDate)
    .AnyAsync();

if (overlappingReservations)
    throw new ArgumentException("Vehicle already has a reservation for the selected dates.");
```

---

## ⚠️ Beveiligingsproblemen

### 5. **Geen Rate Limiting**
**Probleem:** Geen bescherming tegen spam/DOS aanvallen.

**Oplossing:** Implementeer rate limiting middleware.

---

### 6. **Ontbrekende Input Validatie**
**Probleem:** Minimale validatie van data:
- StartDate kan in het verleden liggen
- EndDate kan jaren in de toekomst liggen
- Geen maximum reservatieduur

**Oplossing:**
```csharp
if (request.StartDate < DateTime.UtcNow)
    throw new ArgumentException("StartDate cannot be in the past.");

if (request.EndDate > request.StartDate.AddDays(365))
    throw new ArgumentException("Reservation cannot exceed 365 days.");

var minDuration = TimeSpan.FromHours(1);
if (request.EndDate - request.StartDate < minDuration)
    throw new ArgumentException("Minimum reservation duration is 1 hour.");
```

---

## 🟡 Ontbrekende Functionaliteit

### 7. **Geen GET Endpoints**
**Probleem:** Alleen POST endpoint bestaat. Geen manier om:
- Reservaties op te halen (GET by ID)
- Lijst van reservaties op te halen (GET all)
- Reservaties te filteren (per gebruiker, parking lot, status)

**Oplossing:** Implementeer volledige CRUD:
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)

[HttpGet]
public async Task<IActionResult> GetAll([FromQuery] ReservationQuery query)

[HttpPut("{id}")]
public async Task<IActionResult> Update(Guid id, [FromBody] ReservationUpdateRequest request)

[HttpDelete("{id}")]
public async Task<IActionResult> Cancel(Guid id)
```

---

### 8. **Geen Annulering/Update Functionaliteit**
**Probleem:** Geen manier om reservaties te annuleren of wijzigen.

**Oplossing:** Implementeer Update en Cancel endpoints met business rules:
- Alleen Pending reservaties kunnen geannuleerd
- Niet binnen 24 uur voor StartDate annuleren
- Bij annulering, update ParkingLot.Reserved count

---

### 9. **Status Management Ontbreekt**
**Probleem:** Status wordt altijd op "Pending" gezet, geen state transitions.

**Mogelijke statussen:**
- Pending → Confirmed
- Pending → Cancelled
- Confirmed → Active (bij start)
- Active → Completed (bij einde)
- Any → Cancelled

**Oplossing:** Implementeer state machine met validatie.

---

### 10. **Geen Email/SMS Notificaties**
**Probleem:** Geen bevestiging naar gebruiker.

**Oplossing:** Stuur notificaties bij:
- Nieuwe reservatie
- Annulering
- Reminder 24u voor start

---

## 🟠 Code Kwaliteit Problemen

### 11. **Inconsistente Naming**
**Problemen:**
- `Reservation` DbSet vs `Reservations` collections
- `ParkingLotId` vs `ParkingLotID` in verschillende bestanden
- Namespace `v2.core.Interfaces` moet `v2.Core.Interfaces` zijn (hoofdletter C)

**Oplossing:**
```csharp
// In ApplicationDbContext.cs
public DbSet<Reservation> Reservations { get; set; }  // Was: Reservation

// In namespace
namespace v2.Core.Interfaces;  // Was: v2.core.Interfaces
```

---

### 12. **Geen Logging**
**Probleem:** Geen logging van belangrijke events.

**Oplossing:**
```csharp
private readonly ILogger<ReservationService> _logger;

public ReservationService(ApplicationDbContext db, ILogger<ReservationService> logger)
{
    _db = db;
    _logger = logger;
}

// In CreateReservationAsync:
_logger.LogInformation("Creating reservation for vehicle {LicensePlate} at parking lot {ParkingLotId}", 
    request.LicensePlate, request.ParkingLotId);
```

---

### 13. **Geen Error Handling in Controller**
**Probleem:** Alleen ArgumentException wordt afgevangen. Andere exceptions crashen de app.

**Oplossing:**
```csharp
catch (ArgumentException ex)
{
    return BadRequest(new { error = ex.Message });
}
catch (DbUpdateException ex)
{
    _logger.LogError(ex, "Database error creating reservation");
    return StatusCode(500, new { error = "An error occurred while creating the reservation." });
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error creating reservation");
    return StatusCode(500, new { error = "An unexpected error occurred." });
}
```

---

### 14. **Geen Unit Tests**
**Probleem:** De Python tests in `testing/test_reservations.py` zijn verouderd en testen niet-bestaande endpoints.

**Oplossing:** Schrijf C# unit tests:
```csharp
[Fact]
public async Task CreateReservation_ValidRequest_ReturnsCreated()
{
    // Arrange
    var request = new ReservationCreateRequest { ... };
    
    // Act
    var result = await _controller.Create(request);
    
    // Assert
    var createdResult = Assert.IsType<CreatedResult>(result);
    var response = Assert.IsType<ReservationResponse>(createdResult.Value);
    Assert.NotEqual(Guid.Empty, response.Id);
}
```

---

## 🔵 Architectuur Verbeteringen

### 15. **ParkingLot.Reserved Wordt Niet Bijgewerkt**
**Probleem:** Het `Reserved` veld wordt nergens geüpdatet.

**Oplossing:**
```csharp
// Na succesvolle reservatie:
lot.Reserved++;
await _db.SaveChangesAsync();
```

---

### 16. **Geen Transactie Gebruik**
**Probleem:** Bij fout na `_db.Reservation.Add()` blijft database inconsistent.

**Oplossing:**
```csharp
using var transaction = await _db.Database.BeginTransactionAsync();
try
{
    _db.Reservation.Add(reservation);
    lot.Reserved++;
    await _db.SaveChangesAsync();
    await transaction.CommitAsync();
    return reservation;
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

---

### 17. **CompanyName Veld Ongebruikt**
**Probleem:** `CompanyName` in Reservation model wordt nergens gebruikt of gezet.

**Oplossing:** Of gebruiken voor zakelijke klanten, of verwijderen.

---

### 18. **Geen Paginering**
**Probleem:** Toekomstige GET all endpoint zal alle records retourneren.

**Oplossing:** Implementeer paginering:
```csharp
public class ReservationQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Status { get; set; }
    public Guid? UserId { get; set; }
}
```

---

## 📈 Performance Optimalisaties

### 19. **Inefficiënte Database Queries**
**Probleem:** `Include(v => v.User)` laadt onnodige data.

**Oplossing:** Alleen noodzakelijke velden laden:
```csharp
var vehicle = await _db.Vehicles
    .Where(v => v.LicensePlate == request.LicensePlate)
    .Select(v => new { v.ID, v.UserID })
    .FirstOrDefaultAsync();
```

---

### 20. **Geen Indexes**
**Probleem:** Geen database indexes voor vaak gebruikte queries.

**Oplossing:**
```csharp
// In OnModelCreating:
modelBuilder.Entity<Reservation>()
    .HasIndex(r => r.Status);
modelBuilder.Entity<Reservation>()
    .HasIndex(r => new { r.StartDate, r.EndDate });
modelBuilder.Entity<Vehicle>()
    .HasIndex(v => v.LicensePlate)
    .IsUnique();
```

---

## 🎯 Prioriteiten

### Must-Have (Kritiek - binnen 1 sprint):
1. ✅ Autorisatie toevoegen
2. ✅ TotalPrice berekening implementeren
3. ✅ Capaciteitscontrole toevoegen
4. ✅ Input validatie verbeteren
5. ✅ Logging toevoegen

### Should-Have (Belangrijk - binnen 2 sprints):
6. GET/UPDATE/DELETE endpoints
7. Status management
8. Overlappende reservaties check
9. Error handling verbeteren
10. Unit tests schrijven

### Nice-to-Have (Toekomst):
11. Email notificaties
12. Rate limiting
13. Performance optimalisaties
14. Paginering

---

## 📝 Conclusie

De huidige reservatiecomponent bevat een basisimplementatie maar mist kritieke functionaliteit voor productiegebruik. De belangrijkste problemen zijn:

1. **Beveiliging**: Geen autorisatie of authenticatie
2. **Business Logic**: Geen prijsberekening of capaciteitscontrole
3. **Functionaliteit**: Alleen CREATE, geen READ/UPDATE/DELETE
4. **Code Kwaliteit**: Geen logging, tests of error handling

**Aanbeveling**: Implementeer minimaal de "Must-Have" items voordat deze component naar productie gaat.
