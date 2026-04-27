using ERP.Repository;
using ERP.Repository.Contact;
using ERP.Service.Common;

namespace ERP.Service.Contact;

public interface IContactService :ICrudService<ERP.Entity.Contact.Contact>
{
}
public class ContactService(
    IContactRepository contactRepository,
    IUnitOfWork unitOfWork
    ) :CrudService <ERP.Entity.Contact.Contact>(contactRepository, unitOfWork), IContactService
{
}
