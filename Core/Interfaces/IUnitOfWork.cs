using System;
using System.Threading.Tasks;
using Core.Entities;

namespace Core.Interfaces
{
    //Unit of work. Course item 217
    public interface IUnitOfWork : IDisposable
    {
         IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;

         //Return number of changes to the database
         Task<int> Complete();
    }
}