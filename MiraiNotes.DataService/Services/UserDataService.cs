using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiraiNotes.Data;
using MiraiNotes.Data.Models;
using MiraiNotes.DataService.Interfaces;

namespace MiraiNotes.DataService.Services
{
    public class UserDataService : Repository<GoogleUser>, IUserDataService
    {
        public MiraiNotesContext MiraiNotesContext
        {
            get => _context as MiraiNotesContext;
        }

        public UserDataService(DbContext context)
            : base(context)
        {
        }

        public async Task<GoogleUser> GetCurrentActiveUserAsync()
        {
            var currentUser = await base.GetAsync(u => u.IsActive, null, string.Empty);
            if (currentUser.Count() > 1)
            {
                throw new ApplicationException($"We cant have more than 1 active user. Current active users = {currentUser.Count()}");
            }
            var user = currentUser.FirstOrDefault();
            return user;
        }

        public async Task ChangeCurrentUserStatus(bool isActive)
        {
            var currentUser = await GetCurrentActiveUserAsync();
            if (currentUser == null)
                throw new NullReferenceException("The current active user couldnt be found on db");
            currentUser.IsActive = isActive;
            base.Update(currentUser);
        }
    }
}
