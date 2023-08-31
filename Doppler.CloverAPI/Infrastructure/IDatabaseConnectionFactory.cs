using System.Data;

namespace Doppler.CloverAPI.Infrastructure
{
    public interface IDatabaseConnectionFactory
    {
        IDbConnection GetConnection();
    }
}
