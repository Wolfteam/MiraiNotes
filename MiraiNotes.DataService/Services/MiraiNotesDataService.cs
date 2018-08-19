using MiraiNotes.Data;
using MiraiNotes.DataService.Interfaces;
using MiraiNotes.Shared.Models;
using System;
using System.Threading.Tasks;

namespace MiraiNotes.DataService.Services
{
    public class MiraiNotesDataService : IMiraiNotesDataService
    {
        private bool disposedValue = false; // To detect redundant calls
        private readonly MiraiNotesContext _miraiNotesContext;

        public IUserDataService UserService { get; }
        public ITaskListDataService TaskListService { get; }
        public ITaskDataService TaskService { get; }


        public MiraiNotesDataService()
        {
            _miraiNotesContext = MiraiNotesContext.Create("Data Source=mirai-notes.db");
            UserService = new UserDataService(_miraiNotesContext);
            TaskListService = new TaskListDataService(_miraiNotesContext);
            TaskService = new TaskDataService(_miraiNotesContext);
        }

        public async Task<Result> SaveChangesAsync()
        {
            var result = new Result
            {
                Succeed = false
            };
            try
            {
                result.Succeed = await _miraiNotesContext.SaveChangesAsync() > 0;
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _miraiNotesContext.Dispose();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
