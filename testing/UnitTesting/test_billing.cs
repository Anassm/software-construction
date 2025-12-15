using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Xunit;
using v2.Infrastructure.Data;
using v2.Core.Models;
using v2.infrastructure.Services;
using v2.Core.DTOs;
using System.Reflection;

public class BillingServiceTests
{
    private readonly BillingService _service;
    private readonly ApplicationDbContext _context;
    private readonly User _user;
    private readonly User _admin;
    private readonly Invoice _invoice1;
    private readonly Invoice _invoice2;

    private readonly Session _session1;  
    private readonly Session _session2;  

    public BillingServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var identityUser = new IdentityUser
        {
            UserName = "testuser",
            Id = Guid.NewGuid().ToString()
        };

        var identityAdmin = new IdentityUser
        {
            UserName = "adminuser",
            Id = Guid.NewGuid().ToString()
        };

        _user = new User
        {
            ID = Guid.NewGuid(),
            IdentityUserId = identityUser.Id,
            IdentityUser = identityUser,
            Username = "testuser",
            Name = "Test User",
            Email = "test@example.com",
            PhoneNumber = "1234567890",
            Role = "user",
            BirthYear = 1990,
            IsActive = true,
            Vehicles = new List<Vehicle>(),
            Sessions = new List<Session>(),
            Reservations = new List<Reservation>()
        };

        _admin = new User
        {
            ID = Guid.NewGuid(),
            IdentityUserId = identityAdmin.Id,
            IdentityUser = identityAdmin,
            Username = "adminuser",
            Name = "Admin User",
            Email = "admin@example.com",
            PhoneNumber = "1234567890",
            Role = "admin",
            BirthYear = 1990,
            IsActive = true,
            Vehicles = new List<Vehicle>(),
            Sessions = new List<Session>(),
            Reservations = new List<Reservation>()
        };

        _context.Users.Add(_admin);
        _context.Users.Add(_user);
        _context.SaveChanges();

        _invoice1 = new Invoice
        {
            ID = Guid.NewGuid(),
            InvoiceNumber = "INV-001",
            TotalAmount = 50.00f,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            DueDate = DateTime.UtcNow.AddDays(4),
            Status = InvoiceStatus.Paid,
            UserID = _user.ID,
            User = _user
        };

        _invoice2 = new Invoice
        {
            ID = Guid.NewGuid(),
            InvoiceNumber = "INV-002",
            TotalAmount = 75.00f,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            DueDate = DateTime.UtcNow.AddDays(9),
            Status = InvoiceStatus.Open,
            UserID = _user.ID,
            User = _user
        };

        var parkingLot = new ParkingLot
        {
            ID = Guid.NewGuid(),
            Name = "Test Parking",
            Location = "Amsterdam",
            Address = "Test Street 123",
            Capacity = 100,
            Reserved = 0,
            Tariff = 2.50f,
            DayTariff = 20.00f,
            latitude = 52.3676f,
            longitude = 4.9041f,
            Reservations = new List<Reservation>(),
            Sessions = new List<Session>()
        };
        _context.ParkingLots.Add(parkingLot);
        _context.SaveChanges();

   
        var session1 = new Session
        {
            ID = Guid.NewGuid(),
            LicensePlate = "ABC123",
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow,
            duration = 2,
            Price = 5.00f,
            PaymentStatus = PaymentStatus.Pending,
            UserID = _user.ID,
            ParkingLotID = parkingLot.ID,
            User = _user,
            ParkingLot = parkingLot
        };

        var session2 = new Session
        {
            ID = Guid.NewGuid(),
            LicensePlate = "XYZ789",
            StartTime = DateTime.UtcNow.AddHours(-3),
            EndTime = DateTime.UtcNow,
            duration = 3,
            Price = 7.50f,
            PaymentStatus = PaymentStatus.Pending,
            UserID = _user.ID,
            ParkingLotID = parkingLot.ID,
            User = _user,
            ParkingLot = parkingLot
        };

        _context.Sessions.Add(session1);
        _context.Sessions.Add(session2);
        _context.SaveChanges();


        _session1 = session1;
        _session2 = session2;

        _context.Invoices.Add(_invoice1);
        _context.Invoices.Add(_invoice2);
        _context.SaveChanges();

        _service = new BillingService(_context);
    }

    [Fact]
    public async Task GetMyInvoiceHistory_UserNotFound()
    {
        var result = await _service.GetMyInvoiceHistoryAsync(Guid.NewGuid().ToString());
        Assert.Equal(404, result.statusCode);
    }

    [Fact]
    public async Task GetMyInvoiceHistory_Success()
    {
        var result = await _service.GetMyInvoiceHistoryAsync(_user.IdentityUserId);
        Assert.Equal(200, result.statusCode);

        
        var dataType = result.data.GetType();
        var invoicesProperty = dataType.GetProperty("invoices");
        var invoices = invoicesProperty.GetValue(result.data) as List<InvoiceSummaryDto>;

        Assert.NotNull(invoices);
        Assert.Equal(2, invoices.Count);
        Assert.Contains(invoices, i => i.InvoiceNumber == "INV-001");
        Assert.Contains(invoices, i => i.InvoiceNumber == "INV-002");
    }

    [Fact]
    public async Task GetMyInvoiceHistory_EmptyList()
    {
        var identityUser = new IdentityUser 
        { 
            UserName = "noinvoices",
            Id = Guid.NewGuid().ToString()
        };
        
        var newUser = new User
        {
            ID = Guid.NewGuid(),
            IdentityUserId = identityUser.Id,
            IdentityUser = identityUser,
            Username = "noinvoices",
            Name = "No Invoices User",
            Email = "noinvoices@example.com",
            PhoneNumber = "1234567890",
            Role = "user",
            BirthYear = 1990,
            IsActive = true,
            Vehicles = new List<Vehicle>(),
            Sessions = new List<Session>(),
            Reservations = new List<Reservation>()
        };
        _context.Users.Add(newUser);
        _context.SaveChanges();

        var result = await _service.GetMyInvoiceHistoryAsync(newUser.IdentityUserId);
        Assert.Equal(200, result.statusCode);

        var dataType = result.data.GetType();
        var invoicesProperty = dataType.GetProperty("invoices");
        var invoices = invoicesProperty.GetValue(result.data) as List<InvoiceSummaryDto>;

        Assert.NotNull(invoices);
        Assert.Empty(invoices);
    }

    [Fact]
    public async Task GetMyInvoiceHistory_OrderedByCreatedDate()
    {
        var result = await _service.GetMyInvoiceHistoryAsync(_user.IdentityUserId);
        Assert.Equal(200, result.statusCode);

        var dataType = result.data.GetType();
        var invoicesProperty = dataType.GetProperty("invoices");
        var invoices = invoicesProperty.GetValue(result.data) as List<InvoiceSummaryDto>;

        Assert.NotNull(invoices);
        Assert.Equal(2, invoices.Count);
        
        
        Assert.Equal("INV-002", invoices[0].InvoiceNumber);
        Assert.Equal("INV-001", invoices[1].InvoiceNumber);
    }

    [Fact]
    public async Task GetMyInvoiceHistory_CorrectInvoiceProperties()
    {
        var result = await _service.GetMyInvoiceHistoryAsync(_user.IdentityUserId);
        Assert.Equal(200, result.statusCode);

        var dataType = result.data.GetType();
        var invoicesProperty = dataType.GetProperty("invoices");
        var invoices = invoicesProperty.GetValue(result.data) as List<InvoiceSummaryDto>;
        var firstInvoice = invoices.FirstOrDefault(i => i.InvoiceNumber == "INV-001");

        Assert.NotNull(firstInvoice);
        Assert.Equal(_invoice1.ID, firstInvoice.Id);
        Assert.Equal("INV-001", firstInvoice.InvoiceNumber);
        Assert.Equal(50.00f, firstInvoice.TotalAmount);
        Assert.Equal("Paid", firstInvoice.Status);
    }

    [Fact]
public async Task GetMyInvoiceHistory_MultipleStatuses()
{
    var overdueInvoice = new Invoice
    {
        ID = Guid.NewGuid(),
        InvoiceNumber = "INV-003",
        TotalAmount = 100.00f,
        CreatedAt = DateTime.UtcNow.AddDays(-30),
        DueDate = DateTime.UtcNow.AddDays(-5),
        Status = InvoiceStatus.Overdue,
        UserID = _user.ID,
        User = _user
    };

    var voidInvoice = new Invoice
    {
        ID = Guid.NewGuid(),
        InvoiceNumber = "INV-004",
        TotalAmount = 25.00f,
        CreatedAt = DateTime.UtcNow.AddDays(-15),
        DueDate = DateTime.UtcNow.AddDays(10),
        Status = InvoiceStatus.Void,
        UserID = _user.ID,
        User = _user
    };

    _context.Invoices.Add(overdueInvoice);
    _context.Invoices.Add(voidInvoice);
    _context.SaveChanges();

    var result = await _service.GetMyInvoiceHistoryAsync(_user.IdentityUserId);
    Assert.Equal(200, result.statusCode);

    var dataType = result.data.GetType();
    var invoicesProperty = dataType.GetProperty("invoices");
    var invoices = invoicesProperty.GetValue(result.data) as List<InvoiceSummaryDto>;

    Assert.NotNull(invoices);
    Assert.Equal(4, invoices.Count);
    Assert.Contains(invoices, i => i.Status == "Paid");
    Assert.Contains(invoices, i => i.Status == "Open");
    Assert.Contains(invoices, i => i.Status == "Overdue");
    Assert.Contains(invoices, i => i.Status == "Void");
}

[Fact]
public async Task GetMyInvoiceHistory_OnlyUserInvoices()
{
    var identityOther = new IdentityUser 
    { 
        UserName = "otheruser",
        Id = Guid.NewGuid().ToString()
    };
    
    var otherUser = new User
    {
        ID = Guid.NewGuid(),
        IdentityUserId = identityOther.Id,
        IdentityUser = identityOther,
        Username = "otheruser",
        Name = "Other User",
        Email = "other@example.com",
        PhoneNumber = "1234567890",
        Role = "user",
        BirthYear = 1990,
        IsActive = true,
        Vehicles = new List<Vehicle>(),
        Sessions = new List<Session>(),
        Reservations = new List<Reservation>()
    };
    _context.Users.Add(otherUser);

    var otherInvoice = new Invoice
    {
        ID = Guid.NewGuid(),
        InvoiceNumber = "INV-OTHER",
        TotalAmount = 25.00f,
        CreatedAt = DateTime.UtcNow,
        DueDate = DateTime.UtcNow.AddDays(14),
        Status = InvoiceStatus.Open,
        UserID = otherUser.ID,
        User = otherUser
    };
    _context.Invoices.Add(otherInvoice);
    _context.SaveChanges();

    var result = await _service.GetMyInvoiceHistoryAsync(_user.IdentityUserId);
    Assert.Equal(200, result.statusCode);

    var dataType = result.data.GetType();
    var invoicesProperty = dataType.GetProperty("invoices");
    var invoices = invoicesProperty.GetValue(result.data) as List<InvoiceSummaryDto>;

    Assert.NotNull(invoices);
    Assert.Equal(2, invoices.Count);
    Assert.DoesNotContain(invoices, i => i.InvoiceNumber == "INV-OTHER");
}


[Fact]
public async Task GetInvoiceDetails_AdminSuccess()
{
    var result = await _service.GetInvoiceDetailsAsync(_invoice1.ID, _admin.IdentityUserId);
    Assert.Equal(200, result.statusCode);

    var dataType = result.data.GetType();
    var invoiceProperty = dataType.GetProperty("invoice");
    var invoice = invoiceProperty.GetValue(result.data);

    Assert.NotNull(invoice);
}

[Fact]
public async Task GetInvoiceDetails_NonAdminForbidden()
{
    var result = await _service.GetInvoiceDetailsAsync(_invoice1.ID,_user.IdentityUserId);
    Assert.Equal(403, result.statusCode);

}
[Fact]
public async Task GetInvoiceDetails_UserNotFound()
{
    var result = await _service.GetInvoiceDetailsAsync(_invoice1.ID, Guid.NewGuid().ToString());
    Assert.Equal(404, result.statusCode);
}

[Fact]
public async Task GetInvoiceDetails_InvoiceNotFound()
{
    var result = await _service.GetInvoiceDetailsAsync(Guid.NewGuid(), _admin.IdentityUserId);
    Assert.Equal(404, result.statusCode);
}

[Fact]
public async Task GetInvoiceDetails_EmployeeSuccess()
{
    
    var identityEmployee = new IdentityUser
    {
        UserName = "employee",
        Id = Guid.NewGuid().ToString()
    };

    var employee = new User
    {
        ID = Guid.NewGuid(),
        IdentityUserId = identityEmployee.Id,
        IdentityUser = identityEmployee,
        Username = "employee",
        Name = "Employee User",
        Email = "employee@example.com",
        PhoneNumber = "1234567890",
        Role = "employee",
        BirthYear = 1990,
        IsActive = true,
        Vehicles = new List<Vehicle>(),
        Sessions = new List<Session>(),
        Reservations = new List<Reservation>()
    };
    _context.Users.Add(employee);
    _context.SaveChanges();

    var result = await _service.GetInvoiceDetailsAsync(_invoice1.ID, employee.IdentityUserId);
    Assert.Equal(200, result.statusCode);
}

[Fact]
public async Task GetInvoiceDetails_CorrectInvoiceStructure()
{
    var result = await _service.GetInvoiceDetailsAsync(_invoice1.ID, _admin.IdentityUserId);
    Assert.Equal(200, result.statusCode);

    var dataType = result.data.GetType();
    var invoiceProperty = dataType.GetProperty("invoice");
    var invoice = invoiceProperty.GetValue(result.data);

    var invoiceType = invoice.GetType();
    Assert.NotNull(invoiceType.GetProperty("id").GetValue(invoice));
    Assert.NotNull(invoiceType.GetProperty("invoiceNumber").GetValue(invoice));
    Assert.NotNull(invoiceType.GetProperty("totalAmount").GetValue(invoice));
    Assert.NotNull(invoiceType.GetProperty("status").GetValue(invoice));
}

[Fact]
public async Task GetInvoiceDetails_IncludesCustomerDetails()
{
    var result = await _service.GetInvoiceDetailsAsync(_invoice1.ID, _admin.IdentityUserId);
    Assert.Equal(200, result.statusCode);

    var dataType = result.data.GetType();
    var invoiceProperty = dataType.GetProperty("invoice");
    var invoice = invoiceProperty.GetValue(result.data);

    
    var invoiceType = invoice.GetType();
    var customer = invoiceType.GetProperty("customer").GetValue(invoice);
    Assert.NotNull(customer);

    var customerType = customer.GetType();
    Assert.NotNull(customerType.GetProperty("id").GetValue(customer));
    Assert.NotNull(customerType.GetProperty("username").GetValue(customer));
    Assert.NotNull(customerType.GetProperty("name").GetValue(customer));
    Assert.NotNull(customerType.GetProperty("email").GetValue(customer));
}

[Fact]
public async Task CreateBundleInvoice_AdminSuccess()
{
    var dto = new CreateBundleInvoiceDto
    {
        SessionIds = new List<Guid> { _session1.ID, _session2.ID },
        CompanyName = "Test Company"
    };

    var result = await _service.CreateBundleInvoiceAsync(dto, _admin.IdentityUserId);
    Assert.Equal(201, result.statusCode);
}

[Fact]
public async Task CreateBundleInvoice_EmployeeSuccess()
{

    var identityEmployee = new IdentityUser
    {
        UserName = "employee",
        Id = Guid.NewGuid().ToString()
    };

    var employee = new User
    {
        ID = Guid.NewGuid(),
        IdentityUserId = identityEmployee.Id,
        IdentityUser = identityEmployee,
        Username = "employee",
        Name = "Employee User",
        Email = "employee@example.com",
        PhoneNumber = "1234567890",
        Role = "employee",
        BirthYear = 1990,
        IsActive = true,
        Vehicles = new List<Vehicle>(),
        Sessions = new List<Session>(),
        Reservations = new List<Reservation>()
    };
    _context.Users.Add(employee);
    _context.SaveChanges();

    var dto = new CreateBundleInvoiceDto
    {
        SessionIds = new List<Guid> { _session1.ID, _session2.ID }
    };

    var result = await _service.CreateBundleInvoiceAsync(dto, employee.IdentityUserId);
    Assert.Equal(201, result.statusCode);
}

[Fact]
public async Task CreateBundleInvoice_BusinessUserSuccess()
{
    
    var identityBusiness = new IdentityUser
    {
        UserName = "business",
        Id = Guid.NewGuid().ToString()
    };

    var businessUser = new User
    {
        ID = Guid.NewGuid(),
        IdentityUserId = identityBusiness.Id,
        IdentityUser = identityBusiness,
        Username = "business",
        Name = "Business User",
        Email = "business@example.com",
        PhoneNumber = "1234567890",
        Role = "business",
        BirthYear = 1990,
        IsActive = true,
        Vehicles = new List<Vehicle>(),
        Sessions = new List<Session>(),
        Reservations = new List<Reservation>()
    };
    _context.Users.Add(businessUser);
    _context.SaveChanges();

    var dto = new CreateBundleInvoiceDto
    {
        SessionIds = new List<Guid> { _session1.ID, _session2.ID }
    };

    var result = await _service.CreateBundleInvoiceAsync(dto, businessUser.IdentityUserId);
    Assert.Equal(201, result.statusCode);
}

[Fact]
public async Task CreateBundleInvoice_SessionNotFound()
{
    var dto = new CreateBundleInvoiceDto
    {
        SessionIds = new List<Guid> { _session1.ID, Guid.NewGuid() }  
    };

    var result = await _service.CreateBundleInvoiceAsync(dto, _admin.IdentityUserId);
    Assert.Equal(404, result.statusCode);
}

[Fact]
public async Task GetUserBillingSummary_AdminCanViewSummary()
{
    var result = await _service.GetUserBillingSummaryAsync(_user.Username, _admin.IdentityUserId);
    Assert.Equal(200, result.statusCode);
}

[Fact]
public async Task GetUserBillingSummary_EmployeeCanViewSummary()
{
  
    var identityEmployee = new IdentityUser
    {
        UserName = "employee",
        Id = Guid.NewGuid().ToString()
    };

    var employee = new User
    {
        ID = Guid.NewGuid(),
        IdentityUserId = identityEmployee.Id,
        IdentityUser = identityEmployee,
        Username = "employee",
        Name = "Employee User",
        Email = "employee@example.com",
        PhoneNumber = "1234567890",
        Role = "employee",
        BirthYear = 1990,
        IsActive = true,
        Vehicles = new List<Vehicle>(),
        Sessions = new List<Session>(),
        Reservations = new List<Reservation>()
    };
    _context.Users.Add(employee);
    _context.SaveChanges();

    var result = await _service.GetUserBillingSummaryAsync(_user.Username, employee.IdentityUserId);
    Assert.Equal(200, result.statusCode);
}

[Fact]
public async Task GetUserBillingSummary_RegularUserForbidden()
{
    var result = await _service.GetUserBillingSummaryAsync(_admin.Username, _user.IdentityUserId);
    Assert.Equal(403, result.statusCode);
}


[Fact]
public async Task GetUserBillingSummary_RequestingUserNotFound()
{
    var result = await _service.GetUserBillingSummaryAsync(_user.Username, Guid.NewGuid().ToString());
    Assert.Equal(404, result.statusCode);
}

[Fact]
public async Task GetUserBillingSummary_TargetUserNotFound()
{
    var result = await _service.GetUserBillingSummaryAsync("nonexistent", _admin.IdentityUserId);
    Assert.Equal(404, result.statusCode);
}

[Fact]
public async Task GetUserBillingSummary_CorrectInvoiceCount()
{
    var result = await _service.GetUserBillingSummaryAsync(_user.Username, _admin.IdentityUserId);
    Assert.Equal(200, result.statusCode);

    var dataType = result.data.GetType();
    var dataProperty = dataType.GetProperty("data");
    var data = dataProperty.GetValue(result.data);

    var dataObjType = data.GetType();
    var summaryProperty = dataObjType.GetProperty("Summary");
    var summary = summaryProperty.GetValue(data);

    var summaryType = summary.GetType();
    var invoiceCount = summaryType.GetProperty("TotalInvoices").GetValue(summary);

    Assert.Equal(2, invoiceCount); 
}
}