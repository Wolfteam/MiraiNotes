using AutoMapper;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.Models.API;
using MiraiNotes.UWP.ViewModels;

namespace MiraiNotes.UWP.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<GoogleTaskModel, TaskItemViewModel>();
            CreateMap<TaskItemViewModel, GoogleTaskModel>();

            CreateMap<GoogleTaskListModel, TaskListItemViewModel>();
            CreateMap<TaskListItemViewModel, GoogleTaskListModel>();

            CreateMap<GoogleTaskListModel, ItemModel>()
                .ForMember(d => d.ItemID, opt => opt.MapFrom(s => s.TaskListID))
                .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Title));

            CreateMap<GoogleTaskModel, ItemModel>()
                .ForMember(d => d.ItemID, opt => opt.MapFrom(s => s.TaskID))
                .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Title));

            CreateMap<TaskListItemViewModel, ItemModel>()
                .ForMember(d => d.ItemID, opt => opt.MapFrom(s => s.TaskListID))
                .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Title));

            CreateMap<TaskItemViewModel, ItemModel>()
                .ForMember(d => d.ItemID, opt => opt.MapFrom(s => s.TaskID))
                .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Title));
        }
    }
}
