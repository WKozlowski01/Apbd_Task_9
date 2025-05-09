using Tutorial9.DTOs;

namespace Tutorial9.Services;

public interface IDbService
{
    Task<(int id, string Error)> CreaateRecordAsync(InsertDataDto data);
    Task<DbResult> ProcedureAsync(InsertDataDto data);

}