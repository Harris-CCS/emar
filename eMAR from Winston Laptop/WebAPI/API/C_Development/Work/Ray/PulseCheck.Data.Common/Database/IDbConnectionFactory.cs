using System.Data;

namespace PulseCheck.Data.Common.Database
{
    public interface IDbConnectionFactory
  {
        IDbConnection Create();

        T Create<T>() where T: IDbConnection;
    }
}