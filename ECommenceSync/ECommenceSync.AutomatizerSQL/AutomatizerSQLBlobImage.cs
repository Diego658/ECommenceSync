using ECommenceSync.AutomatizerSQL.Helpers;
using ECommenceSync.Entities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ECommenceSync.AutomatizerSQL
{
    public class AutomatizerSQLBlobImage : Blob<int>
    {
        readonly long _blobId;
        readonly IAutomatizerDataHelper _dataHelper;

        public AutomatizerSQLBlobImage(IAutomatizerDataHelper dataHelper, long blobId)
        {
            _dataHelper = dataHelper;
            _blobId = blobId;
        }

        public override async Task<Stream> GetStream()
        {
            await using var sqlCon = _dataHelper.GetBlobConnection();
            await using var sqlCmd = sqlCon.CreateCommand();
            await sqlCon.OpenAsync();
            sqlCmd.CommandText = "SELECT Data FROM Blobs WHERE Id = @Id";
            sqlCmd.Parameters.AddWithValue("Id", _blobId);
            var reader = await sqlCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                await using var buffer = reader.GetStream(0);
                var memStream = new MemoryStream();
                await buffer.CopyToAsync(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                return memStream;
            }
            else
            {
                throw new InvalidOperationException($"El blob con id {_blobId} no existe!!!");
            }
        }
    }
}
