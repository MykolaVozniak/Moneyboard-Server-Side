﻿using Moneyboard.Core.DTO.ProjectDTO;
using Moneyboard.Core.DTO.RoleDTO;
using Moneyboard.Core.DTO.UserDTO;
using Moneyboard.Core.Entities.ProjectEntity;
using Moneyboard.Core.Helpers;

namespace Moneyboard.Core.Interfaces.Services
{
    public interface IProjectService
    {
        Task CreateNewProjectAsync(ProjectCreateDTO projectDTO, string userId);
        Task<ProjectInfoDTO> InfoFromProjectAsync(int projectId);
        Task EditProjectDateAsync(ProjectEditDTO projectEditDTO, int projectId);
        Task AddMemberToProjectAsync(string userId, int projectId);
        Task<IEnumerable<ProjectForUserDTO>> GetProjectsOwnedByUserAsync(string userId);
        Task<IEnumerable<ProjectForUserDTO>> GetProjectsUserIsMemberAsync(string userId);
        //Task<(IEnumerable<ProjectForUserDTO> Owners, IEnumerable<ProjectForUserDTO> Members)> GetAllProjectsForUserAsync(string userId);

        //Task CreateNewRoleAsync(int projectId, ProjectCreate2DTO roleCreateDTO);

    }
}
