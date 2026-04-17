using ERP.Repository;

namespace ERP.Service.Common;

/// <summary>
/// This is the ICrudService interface which defines basic CRUD operations for entities.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICrudService<T>
{
    public Task<T> Add(T entity);
    public Task<T> Update(T entity);
    public Task Update(IEnumerable<T> entities);
    public Task<IEnumerable<T>> Get();
    public Task<bool> Delete(T entity);
    public Task<bool> DeleteRange(IEnumerable<T> entities);
}

/// <summary>
/// This is the <see cref="CrudService{T}"/> class which implements ICrudService interface.
/// </summary>
/// <typeparam name="T"></typeparam>
public class CrudService<T> : ICrudService<T>
{
    private readonly IUnitOfWork _unitOfWork;
    private IRepository<T> _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CrudService{T}"/> class.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="unitOfWork"></param>
    public CrudService(IRepository<T> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Method to add an entity to the repository and commit the changes to the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual async Task<T> Add(T entity)
    {
        await _repository.Add(entity);
        await _unitOfWork.CommitAsync();
        return entity;
    }

    /// <summary>
    /// Method to delete an entity from the repository and commit the changes to the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual async Task<bool> Delete(T entity)
    {
        await _repository.Delete(entity);
        await _unitOfWork.CommitAsync();
        return true;
    }

    /// <summary>
    /// Method to get all entities from the repository.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<T>> Get()
    {
        return await _repository.Get();
    }

    /// <summary>
    /// Method to update an entity in the repository and commit the changes to the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual async Task<T> Update(T entity)
    {
        await _repository.Update(entity);
        await _unitOfWork.CommitAsync();
        return entity;
    }

    /// <summary>
    /// Method to update multiple entities in the repository and commit the changes to the database.
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public async Task Update(IEnumerable<T> entities)
    {
        await _repository.Update(entities);
        await _unitOfWork.CommitAsync();
    }

    /// <summary>
    /// Method to delete a range of entities from the repository and commit the changes to the database.
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public virtual async Task<bool> DeleteRange(IEnumerable<T> entities)
    {
        await _repository.DeleteRange(entities);  // Assuming DeleteRange method exists in the repository
        await _unitOfWork.CommitAsync();
        return true;
    }
}