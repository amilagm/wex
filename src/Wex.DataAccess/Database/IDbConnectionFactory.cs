using System.Data;

namespace Wex.DataAccess.Database;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}
