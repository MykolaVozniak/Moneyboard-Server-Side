using AutoMapper;
using Moneyboard.Core.DTO.BankCardDTO;
using Moneyboard.Core.DTO.ProjectDTO;
using Moneyboard.Core.DTO.RoleDTO;
using Moneyboard.Core.DTO.UserDTO;
using Moneyboard.Core.Entities.BankCardEntity;
using Moneyboard.Core.Entities.ProjectEntity;
using Moneyboard.Core.Entities.RoleEntity;
using Moneyboard.Core.Entities.UserEntity;
using Moneyboard.Core.Entities.UserProjectEntity;

namespace Moneyboard.Core.Helpers
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {

            CreateMap<User, UserChangeInfoDTO>().ReverseMap();
            CreateMap<UserRegistrationDTO, User>().ReverseMap();
            CreateMap<User, UserChangeInfoDTO>();

            CreateMap<ProjectCreateDTO, BankCard>().ReverseMap();
            CreateMap<ProjectCreateDTO, Project>().ReverseMap();
            CreateMap<Project, ProjectInfoDTO>()
                .ForMember(dest => dest.PayDay, ops => ops.MapFrom(src => src.SalaryDate.Month));
            CreateMap<Project, ProjectEditDTO>();


            CreateMap<ProjectForUserDTO, Project>().ReverseMap();
            CreateMap<Project, ProjectDetailsDTO>()
                 .ForMember(dest => dest.ProjectPointPercent, opt => opt.MapFrom(src => src.ProjectPoinPercent));


            CreateMap<User, ProjectMemberDTO>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.Firstname} {src.Lastname}"))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));

            CreateMap<Role, ProjectMemberDTO>();
            CreateMap<UserProject, ProjectMemberDTO>();
            CreateMap<Role, RoleInfoDTO>();
            CreateMap<RoleCreateDTO, Role>().ReverseMap();

            CreateMap<BankCardEditDTO, BankCard>().ReverseMap();
            CreateMap<BankCard, BankCardInfoDTO>();

            CreateMap<Role, UserCalculatorPaymentDTO>();



            /*CreateMap<InviteUser, UserInviteInfoDTO>()
                .ForMember(x => x.Id, act => act.MapFrom(srs => srs.Id))
                .ForMember(x => x.Date, act => act.MapFrom(srs => srs.Date))
                .ForMember(x => x.IsConfirm, act => act.MapFrom(srs => srs.IsConfirm))
                .ForMember(x => x.WorkspaceName, act => act.MapFrom(srs => srs.Workspace.Name))
                .ForMember(x => x.FromUserName, act => act.MapFrom(srs => srs.FromUser.Name))
                .ForMember(x => x.ToUserId, act => act.MapFrom(srs => srs.ToUserId));*/


            /* CreateMap<BlobDownloadInfo, DownloadFile>()
                 .ForMember(x => x.ContentType, act => act.MapFrom(srs => srs.Details.ContentType))
                 .ForMember(x => x.Content, act => act.MapFrom(srs => srs.Content));*/

        }
    }
}
