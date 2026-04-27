namespace ERP.Repository.Contact;

public interface IContactRepository : IRepository<ERP.Entity.Contact.Contact>
{
}
public class ContactRepository(
    IUnitOfWork unitOfWork,
    ILogger<ContactRepository> logger
    ) : Repository<ERP.Entity.Contact.Contact>(unitOfWork, logger), IContactRepository
{
}
