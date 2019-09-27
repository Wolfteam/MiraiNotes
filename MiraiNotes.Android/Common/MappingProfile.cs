using AutoMapper;
using MiraiNotes.Android.ViewModels;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Models;
using MiraiNotes.Core.Models.GoogleApi;

namespace MiraiNotes.Android.Common
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region Api mappings
            CreateMap<GoogleTaskListModel, ItemModel>()
                .ForMember(d => d.ItemId, opt => opt.MapFrom(s => s.TaskListID))
                .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Title));

            CreateMap<GoogleTaskModel, ItemModel>()
                .ForMember(d => d.ItemId, opt => opt.MapFrom(s => s.TaskID))
                .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Title));

            CreateMap<TaskItemViewModel, ItemModel>()
                .ForMember(d => d.ItemId, opt => opt.MapFrom(s => s.TaskID))
                .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Title));

            CreateMap<TaskItemViewModel, TaskItemViewModel>()
                .ConstructUsingServiceLocator();
            #endregion

            #region Database mappings
            CreateMap<GoogleTaskList, TaskListItemViewModel>()
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.GoogleTaskListID))
                .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(s => s.UpdatedAt))
                .ForMember(d => d.NumberOfTasks, opt => opt.Ignore());

            CreateMap<GoogleTaskList, ItemModel>()
                .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Title))
                .ForMember(d => d.ItemId, opt => opt.MapFrom(s => s.GoogleTaskListID));

            CreateMap<GoogleTask, TaskItemViewModel>()
                .ForMember(d => d.CompletedOn, opt => opt.MapFrom(s => s.CompletedOn))
                .ForMember(d => d.TaskID, opt => opt.MapFrom(s => s.GoogleTaskID))
                .ForMember(d => d.IsDeleted, opt => opt.MapFrom(s => s.IsDeleted))
                .ForMember(d => d.IsHidden, opt => opt.MapFrom(s => s.IsHidden))
                .ForMember(d => d.Notes, opt => opt.MapFrom(s => s.Notes))
                .ForMember(d => d.ParentTask, opt => opt.MapFrom(s => s.ParentTask))
                .ForMember(d => d.Position, opt => opt.MapFrom(s => s.Position))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status))
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
                .ForMember(d => d.ToBeCompletedOn, opt => opt.MapFrom(s => s.ToBeCompletedOn))
                .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(s => s.UpdatedAt))
                .ConstructUsingServiceLocator();

            CreateMap<GoogleTask, ItemModel>()
                .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Title))
                .ForMember(d => d.ItemId, opt => opt.MapFrom(s => s.GoogleTaskID));

            CreateMap<GoogleUser, GoogleUserViewModel>()
                .ForMember(d => d.Id, opts => opts.MapFrom(s => s.ID))
                .ConstructUsingServiceLocator();
            #endregion

        }
    }
}