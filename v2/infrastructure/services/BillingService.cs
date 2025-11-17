using v2.core.Interfaces;
using v2.Infrastructure.Data;
namespace v2.infrastructure.Services;

public class BillingService: IBilling
{
    private readonly ApplicationDbContext _db; 

    public BillingService(ApplicationDbContext db)
    {
        _db = db; 
    }

    public Task<(int statusCode, object data)> GetPaymentHistoryAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}